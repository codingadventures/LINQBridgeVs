using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
 
using Bridge.VSExtension.Utils;
using EnvDTE;
using EnvDTE80;
using System.Xml.XPath;

namespace Bridge.VSExtension
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

    public class BridgeExtension
    {
        private readonly DTE2 _application;
        private readonly Dictionary<string, ProjectInfo> _projects = new Dictionary<string, ProjectInfo>(StringComparer.InvariantCultureIgnoreCase);

        private static readonly XName Import = XName.Get("Import", "http://schemas.microsoft.com/developer/msbuild/2003");
        private static readonly XNamespace Namespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        private static string InstallFolder
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        private static readonly string Target = Path.Combine(InstallFolder, Resources.Targets);
        private static readonly string LinqPadQueryPath = Path.Combine(InstallFolder, Resources.Query);
        private static readonly string LinqPadExePath = Path.Combine(InstallFolder, Resources.LINQPad);


        public BridgeExtension(DTE2 app)
        {
            _application = app;
            SetTargets();
            SetEnvironment();
          
        }

       
        private static void SetEnvironment()
        {
            var linqPadPath = Path.GetDirectoryName(LinqPadExePath);

            var path = Environment.GetEnvironmentVariable("Path") ?? string.Empty;

            if (linqPadPath == null || path.IndexOf(linqPadPath, StringComparison.InvariantCultureIgnoreCase) != -1) return;

            Environment.SetEnvironmentVariable("Path", path + ";" + linqPadPath);
            Environment.SetEnvironmentVariable("Path", path + ";" + linqPadPath, EnvironmentVariableTarget.Machine);
        }

        private static void SetTargets()
        {
            var e = XDocument.Load(Target);

            var usingTaskElement = e.XPathSelectElements("/Project/UsingTask");
            var mapperBuildTaskElement = e.XPathSelectElement("/Project/Target/MapperBuildTask");
            if (VSVersion.VS2010)
            {

                mapperBuildTaskElement.SetAttributeValue("VisualStudioVer", "VS2010");
            }
            else if (VSVersion.VS2012)
            {
                mapperBuildTaskElement.SetAttributeValue("VisualStudioVer", "VS2012");
            }

            foreach (var xElement in usingTaskElement)
            {
                var assemblyName = xElement.Attribute("AssemblyFile").Value;

                xElement.SetAttributeValue("AssemblyFile", Path.Combine(InstallFolder, assemblyName));
            }


            e.Save(Target);

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

            foreach (var proj in SupportedProjects.Where(proj => proj.IsDirty))
                proj.Save();

            if (action == CommandAction.Enable)
                SupportedProjects.ForEach(Enable);
            else
                SupportedProjects.ForEach(Disable);

            foreach (var proj in SupportedProjects)
                _projects.Remove(proj.FullName);
        }

        public void UpdateCommand(MenuCommand cmd, CommandAction action)
        {
            var states = GetStatus(action);
            cmd.Visible = (CommandStates.Visible & states) != 0;
            cmd.Enabled = (CommandStates.Enabled & states) != 0;
        }

        private static void RemoveImports(XElement e)
        {
            var imports = FindImport(e, false).ToList();

            foreach (var import in imports)
                import.Remove();
        }



        private static void Enable(Project project)
        {
            var e = XElement.Load(project.FullName);
            var targetToAdd = XDocument.Load(Target);
            //RemoveImports(e);

            if (targetToAdd.Root != null)
            {

                var xTargets = targetToAdd.Root.Element("Target");
                var xTasks = targetToAdd.Root.Elements("UsingTask").ToList();
                targetToAdd.Root.SetDefaultXmlNamespace(Namespace);

                e.Add(xTasks);
                e.Add(xTargets);
            }

            e.Save(project.FullName);

        }

        private static void Disable(Project project)
        {
            var e = XElement.Load(project.FullName);

            RemoveImports(e);

            e.Save(project.FullName);

        }

        private static bool GetStatus(string projectFile)
        {
            try
            {
                return FindImport(XElement.Load(projectFile), true).Any();
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

        private CommandStates GetCommandStatus(int status, CommandAction action)
        {
            if (status == 0)
                return CommandStates.None;

            var result = ((action == CommandAction.Disable ? status >> 1 : status) & 1) != 0;

            if (result)
                return CommandStates.Enabled | CommandStates.Visible;

            return CommandStates.None;
        }



        static IEnumerable<XElement> FindImport(XElement root, bool strict)
        {
            if (root == null) throw new ArgumentNullException("root");

            var candidates = from e in root.Elements(Import)
                             let a = e.Attribute("Project")
                             where a != null
                             select new { Element = e, Project = (string)a };

            if (strict)
                return from i in candidates
                       where string.Equals(i.Project, Target, StringComparison.InvariantCultureIgnoreCase)
                       select i.Element;

            return from i in candidates
                   let file = Path.GetFileName(i.Project)
                   where string.Equals(file, Target, StringComparison.InvariantCultureIgnoreCase)
                   select i.Element;
        }
    }
}
