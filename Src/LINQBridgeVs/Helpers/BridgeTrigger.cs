using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LINQBridgeVs.Helper.Configuration;
using LINQBridgeVs.Helper.Dependency;
using LINQBridgeVs.Helper.Forms;

namespace LINQBridgeVs.Helper
{
    public static class BridgeTrigger
    {
        public class ExecuteParams
        {
            private readonly CommandAction _action;
            private readonly string _projectName;
            private readonly string _solutionName;
            private readonly string _assemblyName;
            private readonly string _projectOutput;
            
            public ExecuteParams(CommandAction action, string projectName, string solutionName, string assemblyName, string projectOutput)
            {
                _action = action;
                _projectName = projectName;
                _solutionName = solutionName;
                _assemblyName = assemblyName;
                _projectOutput = projectOutput;
            }

            public CommandAction Action
            {
                get { return _action; }
            }

            public string ProjectName
            {
                get { return _projectName; }
            }

            public string SolutionName
            {
                get { return _solutionName; }
            }

            public string AssemblyName
            {
                get { return _assemblyName; }
            }  
            public string ProjectOutput
            {
                get { return _projectOutput; }
            }
        }

        public static void Execute(ExecuteParams executeParams)
        {
            var allProjectReferences = Crawler.FindProjectDependencies(executeParams.ProjectName, executeParams.SolutionName);
            var foundProjects = allProjectReferences as IList<Project> ?? allProjectReferences.ToList();
            var projectReferences = foundProjects.Where(project => project.DependencyType == DependencyType.ProjectReference);
            // var assemblyReferences = allProjectReferences.Where(project => project.DependencyType == DependencyType.AssemblyReference);

            var references = projectReferences as IList<Project> ?? projectReferences.ToList();
            switch (executeParams.Action)
            {
                case CommandAction.Enable:
                    const string enableMessage = "Following project dependencies have been found...LINQBridge them? (Recommended)";
                    PackageConfigurator.EnableProject(executeParams.ProjectOutput, executeParams.AssemblyName, executeParams.SolutionName);
                    var enabledDependencies = new List<string>();
                    enabledDependencies.Insert(0, executeParams.AssemblyName);

                    if (references.Any(project => PackageConfigurator.IsBridgeDisabled(project.AssemblyName, executeParams.SolutionName)))
                    {
                        var projectDependencies = new ProjectDependencies(references, enableMessage);
                        var dependencies = projectDependencies.ShowDependencies(PackageConfigurator.EnableProject);
                        enabledDependencies.AddRange(dependencies.Select(project => project.AssemblyName));
                    }

                    MessageBox.Show(string.Format("LINQBridge on {0} has been Enabled...", string.Join(", ", enabledDependencies)), "Success", MessageBoxButtons.OK);

                    break;
                case CommandAction.Disable:
                    const string disableMessage = "Following project dependencies have been found...Un-LINQBridge them? (Recommended)";

                    PackageConfigurator.DisableProject(executeParams.ProjectOutput, executeParams.AssemblyName, executeParams.SolutionName);
                    var disableDependencies = new List<string>();
                    disableDependencies.Insert(0, executeParams.AssemblyName);

                    if (references.Any(project => PackageConfigurator.IsBridgeEnabled(project.AssemblyName, executeParams.SolutionName)))
                    {
                        var projectDependencies = new ProjectDependencies(references, disableMessage);
                        var dependencies = projectDependencies.ShowDependencies(PackageConfigurator.DisableProject);
                        disableDependencies.AddRange(dependencies.Select(project => project.AssemblyName));
                    }

                    MessageBox.Show(string.Format("LINQBridge on {0} has been Disabled...", string.Join(", ", disableDependencies)), "Success", MessageBoxButtons.OK);

                    break;
                default:
                    throw new ArgumentOutOfRangeException("action");
            }



        }
    }
}
