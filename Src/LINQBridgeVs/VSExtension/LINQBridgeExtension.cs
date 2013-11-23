using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using EnvDTE;
using LINQBridge.Logging;
using Microsoft.Build.Evaluation;
using Microsoft.Win32;
using Project = EnvDTE.Project;


namespace LINQBridge.VSExtension
{
    [Flags]
    public enum CommandStates
    {
        None = 0,
        Visible = 0x01,
        Enabled = 0x02
    }

    public enum CommandAction
    {
        Enable,
        Disable
    }

    public class LINQBridgeExtension
    {
        #region [ Private Properties ]
        private readonly DTE _application;


        public static bool IsEnvironmentConfigured
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(Resources.ConfigurationRegistryKey))
                {
                    return key != null && Convert.ToBoolean(key.GetValue("IsLINQBridgeConfigured"));
                }
            }
            private set
            {
                using (var key = Registry.CurrentUser.CreateSubKey(Resources.ConfigurationRegistryKey))
                {
                    if (key != null)
                        key.SetValue("IsLINQBridgeConfigured", value);
                }
            }
        }
        #endregion

        public LINQBridgeExtension(DTE app)
        {
            Log.Configure("LINQBridgeVs");
            _application = app;
            Log.Write("Configuring LINQBridgeExtension");

            if (IsEnvironmentConfigured) return;

            Log.Write("Setting the Environment");
            SetEnvironment();
        }


        private static void SetEnvironment()
        {

            //Set in the registry the installer location
            using (var key = Registry.CurrentUser.CreateSubKey(Resources.InstallFolderRegistryKey))
            {
                if (key != null)
                {
                    Log.Write("Setting InstallFolderPath to ", Locations.InstallFolder);
                    key.SetValue("InstallFolderPath", Locations.InstallFolder);
                }
            }

            var linqPadPath = Path.GetDirectoryName(Locations.LinqPadExeFileNamePath);

            if (!Directory.Exists(Locations.LinqPadDestinationFolder))
            {
                Log.Write("Creating LinqPad directory ", Locations.LinqPadDestinationFolder);
                Directory.CreateDirectory(Locations.LinqPadDestinationFolder);
            }


            //Copy the BridgeBuildTask.targets to the default .NET 4.0v framework location
            File.Copy(Locations.LinqBridgeTargetFileNamePath, Path.Combine(Locations.DotNet40FrameworkPath, Locations.LinqBridgeTargetFileName), true);
            File.Copy(Locations.LinqBridgeTargetFileNamePath, Path.Combine(Locations.DotNet40Framework64Path, Locations.LinqBridgeTargetFileName), true);
            Log.Write("LinqBridge Targets copied to ", Locations.DotNet40FrameworkPath, Locations.DotNet40Framework64Path);



            //Install LINQPad in the machine
            if (linqPadPath == null) return;

            Log.Write("Installing LINQPad in the machine");
            foreach (var file in Directory.GetFiles(linqPadPath))
            {
                if (file == null) continue;
                var destinationFileName = Path.Combine(Locations.LinqPadDestinationFolder, Path.GetFileName(file));
                if (File.Exists(destinationFileName))
                    continue;
                File.Move(file, destinationFileName);
            }
            Log.Write("Setting IsEnvironmentConfigured to True");
            IsEnvironmentConfigured = true;


        }

        private static bool IsSupported(string uniqueName)
        {
            return
                uniqueName.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase) || uniqueName.EndsWith(".vbproj", StringComparison.InvariantCultureIgnoreCase);
        }

        private Project SelectedProject
        {
            get
            {
                var items = _application.ActiveSolutionProjects as IEnumerable;
                if (items == null)
                    return null;

                var project = items.OfType<Project>().FirstOrDefault();
                if (project == null || !IsSupported(project.UniqueName))
                    return null;

                return project;
            }
        }

        private string SelectedAssemblyName
        {
            get
            {
                return SelectedProject.Properties.Cast<Property>().First(property => property.Name == "AssemblyName").Value.ToString();
            }
        }

        private static bool IsBridgeEnabled(string assemblyName)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(Resources.EnabledProjectsRegistryKey, true))
            {
                if (key == null) return false;
                var value = key.GetValue(assemblyName);
                return value != null && Convert.ToBoolean(value);
            }
        }

        private static bool IsBridgeDisabled(string assemblyName)
        {
            return !IsBridgeEnabled(assemblyName);
        }

        public void Execute(CommandAction action)
        {

            if (SelectedProject == null)
                return;

            var findProjectReferences = FindAllDependencies(SelectedProject.FullName);
            var projectReferences = findProjectReferences as IList<string> ?? findProjectReferences.ToList();

            switch (action)
            {
                case CommandAction.Enable:
                    Enable(SelectedAssemblyName);
                    MessageBox.Show(string.Format("LINQBridge on {0} has been Enabled...", SelectedAssemblyName), "Success", MessageBoxButtons.OK);

                    if (projectReferences.Where(IsBridgeDisabled).Any())
                    {
                        var result =
                            MessageBox.Show(
                                "Few Project Dependencies have been found. Do you want to LINQBridge them? (Recommended)", "Enable project dependencies...", MessageBoxButtons.OKCancel);

                        if (result == DialogResult.OK)
                            projectReferences.ToList().ForEach(Enable);
                    }
                    break;
                case CommandAction.Disable:
                    Disable(SelectedAssemblyName);
                    MessageBox.Show(string.Format("LINQBridge on {0} has been Disabled...", SelectedAssemblyName), "Success", MessageBoxButtons.OK);

                    if (projectReferences.Where(IsBridgeEnabled).Any())
                    {
                        var result =
                            MessageBox.Show(
                                "Few Project Dependencies have been found. Do you want to Un-LINQBridge them? (Recommended)","Disable project dependencies...", MessageBoxButtons.OKCancel);

                        if (result == DialogResult.OK)
                            projectReferences.ToList().ForEach(Disable);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("action");
            }



        }

        public void UpdateCommand(MenuCommand cmd, CommandAction action)
        {
            var states = GetStatus(action);
            cmd.Visible = (CommandStates.Visible & states) != 0;
            cmd.Enabled = (CommandStates.Enabled & states) != 0;
        }

        private static void Enable(string assemblyName)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(Resources.EnabledProjectsRegistryKey))
            {
                if (key != null)
                    key.SetValue(assemblyName, true);
            }

        }

        private static void Disable(string assemblyName)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(Resources.EnabledProjectsRegistryKey, true))
            {
                if (key != null) key.SetValue(assemblyName, false);
            }

        }

        private CommandStates GetStatus(CommandAction action)
        {
            return GetCommandStatus(GetMultiStatus(), action);
        }

        private int GetMultiStatus()
        {
            var result = 0;

            if (SelectedProject == null)
                return result;

            if (IsBridgeDisabled(SelectedAssemblyName))
                result |= 1;

            if (IsBridgeEnabled(SelectedAssemblyName))
                result |= 2;

            return result;
        }

        private static CommandStates GetCommandStatus(int status, CommandAction action)
        {
            if (status == 0)
                return CommandStates.None;

            var result = ((action == CommandAction.Disable ? status >> 1 : status) & 1) != 0;

            if (result)
                return CommandStates.Enabled | CommandStates.Visible;

            return CommandStates.None;
        }

        /// <summary>
        /// Finds all project dependencies. This methods returns all the dependencies of 
        /// the project LINQbridgeVs is going to be activated on
        /// </summary>
        /// <param name="fullProjectName">Full name of the project.</param>
        /// <returns></returns>
        private static IEnumerable<string> FindAllDependencies(string fullProjectName)
        {
            var loadedProject = ProjectCollection.GlobalProjectCollection.LoadedProjects.FirstOrDefault(p => p.FullPath.Equals(fullProjectName))
                                ??
                                new Microsoft.Build.Evaluation.Project(fullProjectName);

            var references = loadedProject.Items.Where(p => p.ItemType.Equals("ProjectReference"))
                .Where(p => !p.EvaluatedInclude.Contains("Microsoft") && !p.EvaluatedInclude.Contains("System"));

            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("aw", "http://schemas.microsoft.com/developer/msbuild/2003");


            return references.Select(e => XDocument.Load(Path.Combine(loadedProject.DirectoryPath, e.Xml.Include)).XPathSelectElement("/aw:Project/aw:PropertyGroup/aw:AssemblyName", namespaceManager).Value);

        }

    }
}
