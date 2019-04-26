#region License
// Copyright (c) 2013 - 2018 Coding Adventures
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit psticlersons to whom the
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
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BridgeVs.VsPackage.Helper;
using BridgeVs.VsPackage.Helper.Command;
using BridgeVs.VsPackage.Helper.Configuration;
using EnvDTE;
using Microsoft.VisualStudio;
using Project = EnvDTE.Project;

namespace BridgeVs.VisualStudio.AsyncExtension
{
    public class BridgeVsExtension
    {
        private readonly DTE _application;

        public BridgeVsExtension(DTE app)
        {
            _application = app;
        }

        private static bool IsSupported(string uniqueName)
        {
            return
                uniqueName.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase) || uniqueName.EndsWith(".vbproj", StringComparison.InvariantCultureIgnoreCase);
        }

        private IEnumerable<Project> AllProjects
        {
            get
            {
                List<Project> projects = new List<Project>();//AllProjects.ToList();
                foreach (Project proj in _application.Solution.Projects)
                {
                    _findProjects(projects, proj);
                }
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                if (projects.Count == 0)
                    return Enumerable.Empty<Project>();

                return from project in projects.Cast<Project>()
                       where (IsSupported(project.UniqueName) || IsSupported(project.FullName))
                       select project;
            }
        }

        IEnumerable<Project> _findProjects(IList<Project> projects, Object projectItem)
        {
            var proj = projectItem as Project;
            var projItm = projectItem as ProjectItem;
            if (proj != null)
            {
                Debug.WriteLine($"Project => {proj.Name} {proj.Kind}");
            }

            if (projItm != null)
            {
                Debug.WriteLine($"ProjectItem => {projItm.Name} {projItm.Kind}");
            }
            if (proj == null && projItm != null)
            {
                proj = projItm.SubProject;
            }
            if (proj != null)
            {
                Debug.WriteLine($"Resolved Project => {proj.Name} {proj.Kind}");
            }
            if (proj != null && (proj.Kind == VSConstants.UICONTEXT.CSharpProject_string
                                 || proj.Kind == VSConstants.UICONTEXT.VBProject_string
                                 || IsSupported(proj.UniqueName)))
            {
                projects.Add(proj);
                return projects;
            }
            else if (proj != null && (proj.Kind == VSConstants.ItemTypeGuid.VirtualFolder_string
                                      || proj.Kind == "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}")
            )
            {
                foreach (ProjectItem itm in proj.ProjectItems)
                {
                    _findProjects(projects, itm);
                }

                return projects;
            }
            else if (proj == null && projItm != null && projItm.ProjectItems != null)
            {
                foreach (ProjectItem pi in projItm.ProjectItems)
                {
                    _findProjects(projects, pi);
                }
            }

            return projects;
        }

        private string SolutionName => Path.GetFileNameWithoutExtension(_application.Solution.FileName);

        public void Execute(CommandAction action)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            List<Project> projects = AllProjects.ToList();

            if (projects.Count == 0)
                return;

            if (BridgeCommand.IsEveryProjectSupported(projects, _application.Version, _application.Edition))
            {
                BridgeCommand.ActivateBridgeVsOnSolution(action, projects, SolutionName, _application.Version,
                    _application.Edition, Path.GetDirectoryName(_application.Solution.FileName));
            }
            else
            {
                string message = $@"Solution {SolutionName} contains one or more un-supported projects. ASP.NET Core, .NET Core, .NET standard and UAP are not supported by LINQBridgeVs.";
                System.Windows.MessageBox.Show(message);
            }
        }

        public void UpdateCommand(MenuCommand cmd, CommandAction action)
        {
            CommandStates states = GetStatus(action);
            cmd.Visible = (CommandStates.Visible & states) == CommandStates.Visible;
            cmd.Enabled = (CommandStates.Enabled & states) == CommandStates.Enabled;
        }

        private CommandStates GetStatus(CommandAction action)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            CommandStates result = CommandStates.Visible;

            bool isBridgeVsConfigured = PackageConfigurator.IsBridgeVsConfigured(_application.Version);

            if (!isBridgeVsConfigured)
                return result; //just show it as visible

            string solutionDir = Path.GetDirectoryName(_application.Solution.FileName);
            string directoryTarget = Path.Combine(solutionDir, "Directory.Build.targets");
            bool isSolutionEnabled = File.Exists(directoryTarget);

            if (isSolutionEnabled && action == CommandAction.Disable || !isSolutionEnabled && action == CommandAction.Enable)
                result |= CommandStates.Enabled;

            return result;
        }
    }
}