#region License
// Copyright (c) 2013 - 2018 Coding Adventures
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
// NON INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using BridgeVs.Shared.Common;
using BridgeVs.Shared.FileSystem;
using BridgeVs.VsPackage.Helper.Configuration;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using VSLangProj;

namespace BridgeVs.VsPackage.Helper.Command
{
    public static class BridgeCommand
    {
        private const string DirectoryBuildTargets = "Directory.Build.targets";
        private static readonly List<string> UnsupportedFrameworks = new List<string>(20)
        {
            "netstandard",
            "netcoreapp",
            "netcore",
            "netmf",
            "sl4",
            "sl5",
            "wp",
            "uap",
            "uap"
        };

        public static void ActivateBridgeVsOnSolution(CommandAction action, List<Project> projects, string solutionName,
            string vsVersion,
            string vsEdition,
            string solutionFolder)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            List<BridgeProjectInfo> executeParams = new List<BridgeProjectInfo>();

            //enable each individual project by mapping the assembly name and location to a registry entry
            foreach (Project project in projects)
            {
                string path = project.Properties.Item("FullPath").Value.ToString();
                string outputPath = project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value
                    .ToString();
                string fileName = project.Properties.Item("OutputFileName").Value.ToString();
                string projectOutputPath = Path.Combine(path, outputPath, fileName);

                string assemblyName = project.Properties.Item("AssemblyName").Value.ToString();
                IEnumerable<string> references = Enumerable.Empty<string>();

                if (project.Object is VSProject vsProject && vsProject.References != null)
                {
                    references = from Reference reference in vsProject.References
                                 where reference.SourceProject == null //it means it's an assembly reference
                                 where !reference.Path.Contains(".NETFramework") && !reference.Path.Contains("Microsoft") //no .net framework assembly
                                 select reference.Path;
                }

                executeParams.Add(new BridgeProjectInfo(project.FullName, solutionName, assemblyName,
                    projectOutputPath, vsVersion, vsEdition, references.ToList()));

            }
            switch (action)
            {
                case CommandAction.Enable:
                    CommonRegistryConfigurations.BridgeSolution(solutionName, vsVersion, executeParams);
                    //copy directory build target to the solution folder
                    string target = PackageConfigurator.GetInstallationFolder(vsVersion);
                    File.Copy(Path.Combine(target, "Targets", DirectoryBuildTargets), Path.Combine(solutionFolder, DirectoryBuildTargets), true);
                    break;
                case CommandAction.Disable:
                    CommonRegistryConfigurations.UnBridgeSolution(solutionName, vsVersion);
                    //delete directory build target
                    File.Delete(Path.Combine(solutionFolder, DirectoryBuildTargets));
                    break;
            }

            string result = action == CommandAction.Enable ? "Bridged" : "Un-Bridged";
            string userAction = action == CommandAction.Enable ? "Please rebuild your solution." : string.Empty;
            string message = $@"Solution {solutionName} has been {result}. {userAction}";
            MessageBox.Show(message);
        }
         

        public static bool IsEveryProjectSupported(List<Project> projects, string applicationVersion,
            string applicationEdition)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            foreach (Project project in projects)
            {
                string targetFramework = project.Properties.Item("TargetFrameworkMoniker").Value.ToString();
                if (string.IsNullOrEmpty(targetFramework))
                    continue;

                string tfm = targetFramework.Split(',').FirstOrDefault();

                if (!string.IsNullOrEmpty(tfm) && UnsupportedFrameworks.Any(s => Contains(tfm, s, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool Contains(string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

    }
}