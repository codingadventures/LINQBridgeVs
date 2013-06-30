using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;


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

    internal struct ProjectInfo
    {
        readonly bool _isEnabled;
        readonly WeakReference _project;

        public ProjectInfo(Project project, bool isEnabled)
        {
            _project = new WeakReference(project);
            _isEnabled = isEnabled;
        }

        public Project Project { get { return _project.Target as Project; } }
        public bool IsEnabled { get { return _isEnabled; } }
    }

    public class LINQBridgeExtension
    {
        private readonly DTE2 _application;

        private readonly Dictionary<string, ProjectInfo> _projects = new Dictionary<string, ProjectInfo>(StringComparer.InvariantCultureIgnoreCase);

        private static readonly XName Import = XName.Get("Import", "http://schemas.microsoft.com/developer/msbuild/2003");

        private static string InstallFolder
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        private static readonly string LinqPadDestinationFolder = Path.Combine(
            Environment.GetEnvironmentVariable("ProgramFiles"), "LINQPad4");

        private static readonly string Target = Path.Combine(InstallFolder, Resources.Targets);
        private static readonly string LinqPadExePath = Path.Combine(InstallFolder, Resources.LINQPad);


        private string SolutionName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(_application.Solution.FullName);
            }
        }

        public LINQBridgeExtension(DTE2 app)
        {
            _application = app;
            SetEnvironment();
        }


        private static void SetEnvironment()
        {
            var linqPadPath = Path.GetDirectoryName(LinqPadExePath);

            if (!Directory.Exists(LinqPadDestinationFolder))
                Directory.CreateDirectory(LinqPadDestinationFolder);

            if (linqPadPath != null)
                foreach (var file in Directory.GetFiles(linqPadPath))
                {
                    if (file == null) continue;
                    var destinationFileName = Path.Combine(LinqPadDestinationFolder, Path.GetFileName(file));
                    if (!File.Exists(destinationFileName))
                        File.Copy(file, destinationFileName, false);
                }
        }

        private static bool IsSupported(Project proj)
        {
            return proj.UniqueName.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase);
        }

        private List<Project> SupportedProjects
        {
            get
            {
                var items = _application.ActiveSolutionProjects as IEnumerable;
                if (items == null)
                    return null;

                var result = items.OfType<Project>().ToList();
                if (!result.Any() || !result.All(IsSupported))
                    return null;

                return result;
            }
        }

        private bool IsBridgeEnabled(Project project)
        {

            try
            {
                var fullName = project.FullName;
            }
            catch (NotImplementedException)
            {
                return false;
            }

            ProjectInfo result;
            if (_projects.TryGetValue(project.FullName, out result) && result.Project == project)
                return result.IsEnabled;

            _projects[project.FullName] = result = new ProjectInfo(project, GetStatus(project.FullName));

            return result.IsEnabled;
        }

        private bool IsBridgeDisabled(Project project)
        {
            return !IsBridgeEnabled(project);
        }

        public void Execute(CommandAction action)
        {

            if (SupportedProjects == null)
                return;


            if (action == CommandAction.Enable)
                SupportedProjects.RemoveAll(IsBridgeEnabled);
            else
                SupportedProjects.RemoveAll(IsBridgeDisabled);

            var supportedProjectsNames = SupportedProjects
                .Select(project => project.FullName)
                .ToList();


            foreach (var proj in SupportedProjects.Where(proj => proj.IsDirty))
                proj.Save();

            if (action == CommandAction.Enable)
                SupportedProjects.ForEach(Enable);
            else
                SupportedProjects.ForEach(Disable);

            supportedProjectsNames.ForEach(s => _projects.Remove(s));

        }

        public void UpdateCommand(MenuCommand cmd, CommandAction action)
        {
            var states = GetStatus(action);
            cmd.Visible = (CommandStates.Visible & states) != 0;
            cmd.Enabled = (CommandStates.Enabled & states) != 0;
        }

        private static void RemoveImports(XContainer e)
        {
            var imports = FindImport(e);

            foreach (var import in imports)
                import.Remove();
        }

        private static UIHierarchyItem FindItem(UIHierarchyItems items, string projectToSearch)
        {
            UIHierarchyItem retValue = null;

            if (items == null || items.Count == 0) return retValue;

            foreach (var item in items.OfType<UIHierarchyItem>())
            {
                if (item.Name.Contains(projectToSearch)) return item;

                retValue = FindItem(item.UIHierarchyItems, projectToSearch);

                if (retValue != null) break;
            }

            return retValue;
        }

        private void ReloadProject(string projectName)
        {
            var solExp = _application.ToolWindows.SolutionExplorer.Parent; // Get the Solution Explorer Window
            solExp.Activate(); // Activate Solution Explorer Window

            var items = _application.ToolWindows.SolutionExplorer.UIHierarchyItems;

            var itemFound = FindItem(items, projectName);
            
            itemFound.Select(vsUISelectionType.vsUISelectionTypeSelect);

            _application.ExecuteCommand("Project.UnloadProject"); // Unload the first project
            System.Threading.Thread.Sleep(1000);
            _application.ExecuteCommand("Project.ReloadProject"); // Reload 
        }

        private void Enable(Project project)
        {
            var projectFullName = project.FullName;
            var projectName = project.Name;

            var e = XElement.Load(projectFullName);

            RemoveImports(e);

            e.Add(new XElement(Import, new XAttribute("Project", Target)));
           // ReloadProject(projectName);

            MessageBox.Show(string.Format("LINQBridge on {0} has been enabled...", projectName), "Success", MessageBoxButtons.OK);
            e.Save(projectFullName);
        }

        private void Disable(Project project)
        {
            var projectFullName = project.FullName;
            var projectName = project.Name;

            var e = XElement.Load(projectFullName);

            RemoveImports(e);


          //  ReloadProject(projectName);

            MessageBox.Show(string.Format("LINQBridge on {0} has been disabled...", projectName), "Success", MessageBoxButtons.OK);
            e.Save(projectFullName);
        }

        private static bool GetStatus(string projectFile)
        {
            try
            {
                return FindImport(XElement.Load(projectFile)).Any();
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        private CommandStates GetStatus(CommandAction action)
        {
            return GetCommandStatus(GetMultiStatus(), action);
        }

        private int GetMultiStatus()
        {
            var result = 0;

            var projects = SupportedProjects;
            if (projects == null)
                return result;

            if (projects.Any(IsBridgeDisabled))
                result |= 1;

            if (projects.Any(IsBridgeEnabled))
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



        static IEnumerable<XElement> FindImport(XContainer root)
        {
            if (root == null) throw new ArgumentNullException("root");

            var candidates = from e in root.Elements(Import)
                             let a = e.Attribute("Project")
                             where a != null
                             select new { Element = e, Project = (string)a };

            var imports = from i in candidates
                          where i.Project.Contains(Target)
                          select i.Element;


            return imports;


        }
    }
}
