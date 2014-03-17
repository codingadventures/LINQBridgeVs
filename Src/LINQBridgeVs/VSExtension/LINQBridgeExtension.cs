#region License
// Copyright (c) 2013 Giovanni Campo
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EnvDTE;
using LINQBridge.Logging;
using LINQBridge.VSExtension.Dependency;
using LINQBridge.VSExtension.Extension;
using LINQBridge.VSExtension.Forms;
using Microsoft.Win32;
using Process = System.Diagnostics.Process;
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
                    Log.Write("Setting InstallFolderPath to {0}", Locations.InstallFolder);
                    key.SetValue("InstallFolderPath", Locations.InstallFolder);
                }
            }

            var linqPadPath = Path.GetDirectoryName(Locations.LinqPadExeFileNamePath);

            if (!Directory.Exists(Locations.LinqPadDestinationFolder))
            {
                Log.Write("Creating LinqPad directory {0}", Locations.LinqPadDestinationFolder);
                Directory.CreateDirectory(Locations.LinqPadDestinationFolder);
            }


            //Copy the BridgeBuildTask.targets to the default .NET 4.0v framework location
            File.Copy(Locations.LinqBridgeTargetFileNamePath, Path.Combine(Locations.DotNet40FrameworkPath, Locations.LinqBridgeTargetFileName), true);
            File.Copy(Locations.LinqBridgeTargetFileNamePath, Path.Combine(Locations.DotNet40Framework64Path, Locations.LinqBridgeTargetFileName), true);
            Log.Write("LinqBridge Targets copied to {0} - {1}", Locations.DotNet40FrameworkPath, Locations.DotNet40Framework64Path);

            SetPermissions();


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

            var allProjectReferences = Crawler.FindProjectDependencies(SelectedProject.FullName);
            var foundProjects = allProjectReferences as IList<Dependency.Project> ?? allProjectReferences.ToList();
            var projectReferences = foundProjects.Where(project => project.DependencyType == DependencyType.ProjectReference);
            // var assemblyReferences = allProjectReferences.Where(project => project.DependencyType == DependencyType.AssemblyReference);

            var references = projectReferences as IList<Dependency.Project> ?? projectReferences.ToList();
            switch (action)
            {
                case CommandAction.Enable:
                    EnableProject(SelectedAssemblyName);
                    MessageBox.Show(string.Format("LINQBridge on {0} has been Enabled...", SelectedAssemblyName), "Success", MessageBoxButtons.OK);

                    if (references.Any(project => IsBridgeDisabled(project.AssemblyName)))
                    {
                        var projectDependencies = new ProjectDependencies(() => references.ForEach(project => EnableProject(project.AssemblyName)));
                        projectDependencies.ShowDependencies(projectReferences);
                    }
                    break;
                case CommandAction.Disable:
                    DisableProject(SelectedAssemblyName);
                    MessageBox.Show(string.Format("LINQBridge on {0} has been Disabled...", SelectedAssemblyName), "Success", MessageBoxButtons.OK);

                    if (references.Any(project => IsBridgeEnabled(project.AssemblyName)))
                    {
                        var projectDependencies = new ProjectDependencies(() => references.ForEach(project => DisableProject(project.AssemblyName)));
                        projectDependencies.ShowDependencies(projectReferences);
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

        private static void EnableProject(string assemblyName)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(Resources.EnabledProjectsRegistryKey))
            {
                if (key != null)
                    key.SetValue(assemblyName, true);
            }
        }

        private static void DisableProject(string assemblyName)
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

        private static void SetPermissions()
        {
            Log.Write("SetPermission Starts");
            var processOutputs = new StringBuilder();

            var process = new Process
            {
                StartInfo = { UseShellExecute = false, RedirectStandardError = true, RedirectStandardInput = true, RedirectStandardOutput = true, FileName = "icacls.exe", Arguments = Locations.IcaclsArguments }
            };
            var processX64 = new Process
            {
                StartInfo = { UseShellExecute = false, RedirectStandardError = true, FileName = "icacls.exe", Arguments = Locations.IcaclsArgumentsX64, LoadUserProfile = true }
            };
            var processCommonTarget = new Process
            {
                StartInfo = { UseShellExecute = false, RedirectStandardError = true, RedirectStandardInput = true, RedirectStandardOutput = true, FileName = "icacls.exe", Arguments = Locations.IcaclsArgumentsCommonTarget, LoadUserProfile = true }
            };
            var processX64CommonTarget = new Process
            {
                StartInfo = { UseShellExecute = false, RedirectStandardError = true, RedirectStandardInput = true, RedirectStandardOutput = true, FileName = "icacls.exe", Arguments = Locations.IcaclsArgumentsX64CommonTarget, LoadUserProfile = true }
            };

            var takeownProcess = Process.Start("takeown", String.Format("/f {0}", Locations.MicrosoftCommonTargetFileNamePath));
            var takeownProcessx64 = Process.Start("takeown", String.Format("/f {0}", Locations.MicrosoftCommonTarget64FileNamePath));

            if (takeownProcess != null) takeownProcess.WaitForExit();
            if (takeownProcessx64 != null) takeownProcessx64.WaitForExit();

            process.Start();
            processCommonTarget.Start();
            processX64.Start();
            processX64CommonTarget.Start();


            Log.Write("Setting Permission to {0} and {1} ", Locations.IcaclsArguments, Locations.IcaclsArgumentsX64);
            Log.Write("Setting Permission to {0} and {1} ", Locations.IcaclsArgumentsCommonTarget, Locations.IcaclsArgumentsX64CommonTarget);

            process.WaitForExit();
            processCommonTarget.WaitForExit();
            processX64.WaitForExit();
            processX64CommonTarget.WaitForExit();

            if (process.ExitCode != 0)
                processOutputs.AppendLine(process.StandardOutput.ReadToEnd());
            if (process.ExitCode != 0 )
                processOutputs.AppendLine(processCommonTarget.StandardOutput.ReadToEnd());
            if (process.ExitCode != 0 )
                processOutputs.AppendLine(processX64.StandardOutput.ReadToEnd());
            if (process.ExitCode != 0)
                processOutputs.AppendLine(processX64CommonTarget.StandardOutput.ReadToEnd());



            Log.Write(processOutputs.ToString());


            Log.Write("SetPermission Done");

        }

    }
}
