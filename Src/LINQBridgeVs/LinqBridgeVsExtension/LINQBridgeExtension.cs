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
using System.Linq;
using System.Windows.Forms;
using EnvDTE;
using LINQBridgeVs.Extension.Configuration;
using LINQBridgeVs.Extension.Dependency;
using LINQBridgeVs.Extension.Forms;
using LINQBridgeVs.Logging;
using Project = EnvDTE.Project;

namespace LINQBridgeVs.Extension
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

    public class LINQBridgeVsExtension
    {
        #region [ Private Properties ]
        private readonly DTE _application;


        #endregion

        public LINQBridgeVsExtension(DTE app)
        {
            Log.Configure("LINQBridgeVs", "LINQBridgeVsExtension");

            _application = app;
            PackageConfigurator.Configure(_application.Version);
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
                    const string enableMessage = "Following project dependencies have been found...LINQBridge them? (Recommended)";
                    PackageConfigurator.EnableProject(SelectedAssemblyName);
                    var enabledDependencies = new List<string>();
                    enabledDependencies.Insert(0, SelectedAssemblyName);

                    if (references.Any(project => PackageConfigurator.IsBridgeDisabled(project.AssemblyName)))
                    {
                        var projectDependencies = new ProjectDependencies(references, enableMessage);
                        enabledDependencies.AddRange(projectDependencies.ShowDependencies(PackageConfigurator.EnableProject).ToList());
                    }

                    MessageBox.Show(string.Format("LINQBridge on {0} has been Enabled...", string.Join(", ", enabledDependencies)), "Success", MessageBoxButtons.OK);

                    break;
                case CommandAction.Disable:
                    const string disableMessage = "Following project dependencies have been found...Un-LINQBridge them? (Recommended)";

                    PackageConfigurator.DisableProject(SelectedAssemblyName);
                    var disableDependencies = new List<string>();
                    disableDependencies.Insert(0, SelectedAssemblyName);

                    if (references.Any(project => PackageConfigurator.IsBridgeEnabled(project.AssemblyName)))
                    {
                        var projectDependencies = new ProjectDependencies(references, disableMessage);
                        disableDependencies.AddRange(projectDependencies.ShowDependencies(PackageConfigurator.DisableProject));
                    }

                    MessageBox.Show(string.Format("LINQBridge on {0} has been Disabled...", string.Join(", ", disableDependencies)), "Success", MessageBoxButtons.OK);

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

     

        private CommandStates GetStatus(CommandAction action)
        {
            return GetCommandStatus(GetMultiStatus(), action);
        }

        private int GetMultiStatus()
        {
            var result = 0;

            if (SelectedProject == null)
                return result;

            if (PackageConfigurator.IsBridgeDisabled(SelectedAssemblyName))
                result |= 1;

            if (PackageConfigurator.IsBridgeEnabled(SelectedAssemblyName))
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
    }
}
