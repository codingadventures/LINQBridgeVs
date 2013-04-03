using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;

namespace GiovanniCampo.Bridge_VSExtension
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

    public class Bridge
    {
        private readonly DTE2 _application;
        private readonly Dictionary<string, ProjectInfo> _projects = new Dictionary<string, ProjectInfo>(StringComparer.InvariantCultureIgnoreCase);
        private const string Targets = "KindOfMagic.targets";

        private static readonly XName Import = XName.Get("Import", "http://schemas.microsoft.com/developer/msbuild/2003");
        private static readonly string Path = System.IO.Path.Combine(GetInstallDir(), Targets);


        public Bridge(DTE2 app)
        {
            _application = app;
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

        public void EnableBridge(CommandAction action)
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

        private static void Enable(Project project)
        {


        }

        private static void Disable(Project project)
        {


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

        static string GetInstallDir()
        {
            var x = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var home = Environment.GetEnvironmentVariable("KINDOFMAGIC");

            if (x != home)
            {
                Environment.SetEnvironmentVariable("KINDOFMAGIC", x, EnvironmentVariableTarget.User);
                Environment.SetEnvironmentVariable("KINDOFMAGIC", x);
            }

            return "$(KINDOFMAGIC)";
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
                       where string.Equals(i.Project, Path, StringComparison.InvariantCultureIgnoreCase)
                       select i.Element;

            return from i in candidates
                   let file = System.IO.Path.GetFileName(i.Project)
                   where string.Equals(file, Targets, StringComparison.InvariantCultureIgnoreCase)
                   select i.Element;
        }
    }
}
