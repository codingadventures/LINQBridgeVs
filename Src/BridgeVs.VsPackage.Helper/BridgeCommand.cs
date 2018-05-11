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
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using BridgeVs.VsPackage.Helper.Configuration;
using EnvDTE;
using Microsoft.Win32;

namespace BridgeVs.VsPackage.Helper
{
    public static class BridgeCommand
    {
        private const string EnabledProjectsRegistryKey = @"Software\LINQBridgeVs\{0}\Solutions\{1}";
        private const string SolutionEnabled = "SolutionEnabled";

        private static void EnableProject(string assemblyPath, string assemblyName, string solutionName, string vsVersion)
        {
            string keyPath = string.Format(PackageConfigurator.GetRegistryKey(EnabledProjectsRegistryKey, vsVersion, solutionName));
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                key?.SetValue($"{assemblyName}", "True", RegistryValueKind.String);
                key?.SetValue($"{assemblyName}_location", Path.GetFullPath(assemblyPath), RegistryValueKind.String);
            }
        }

        private static void DisableProject(string assemblyName, string solutionName, string vsVersion)
        {
            string keyPath = string.Format(PackageConfigurator.GetRegistryKey(EnabledProjectsRegistryKey, vsVersion, solutionName));

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, true))
            {
                key?.DeleteValue(assemblyName, false);
                key?.DeleteValue($"{assemblyName}_location", false);
            }
        }

        public static void ActivateBridgeVsOnSolution(CommandAction action, List<Project> projects, string solutionName, string vsVersion,
            string vsEdition)
        {
            //enable each individual project by mapping the assembly name and location to a registry entry
            foreach (Project project in projects)
            {
                string path = project.Properties.Item("FullPath").Value.ToString();
                string outputPath = project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
                string fileName = project.Properties.Item("OutputFileName").Value.ToString();
                string projectOutputPath = Path.Combine(path, outputPath, fileName);

                string assemblyName = project.Properties.Item("AssemblyName").Value.ToString();
                ExecuteParams executeParams = new ExecuteParams(action, project.FullName, solutionName, assemblyName,
                    projectOutputPath, vsVersion, vsEdition);
                Execute(executeParams);
            }

            //now create a general solution flag to mark the current solution as activated
            string keyPath = string.Format(PackageConfigurator.GetRegistryKey(EnabledProjectsRegistryKey, vsVersion, solutionName));
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, true))
            {
                key?.SetValue(SolutionEnabled, action == CommandAction.Enable ? "True" : "False", RegistryValueKind.String);
            }

            string result = action == CommandAction.Enable ? "Bridged" : "Un-Bridged";
            string userAction = action == CommandAction.Enable ? "Please rebuild your solution." : string.Empty;
            string message = $@"Solution {solutionName} has been {result}. {userAction}";
            MessageBox.Show(message);
        }

        public static bool IsSolutionEnabled(string solutionName, string vsVersion)
        {
            string keyPath = string.Format(PackageConfigurator.GetRegistryKey(EnabledProjectsRegistryKey, vsVersion, solutionName));

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, false))
            {
                if (key == null) return false;
                string value = (string)key.GetValue(SolutionEnabled);
                return value != null && Convert.ToBoolean(value);
            }
        }

        private static void Execute(ExecuteParams executeParams)
        {
            switch (executeParams.Action)
            {
                case CommandAction.Enable:
                    EnableProject(executeParams.ProjectOutput, executeParams.AssemblyName, executeParams.SolutionName,
                        executeParams.VsVersion);
                    break;
                case CommandAction.Disable:
                    DisableProject(executeParams.AssemblyName, executeParams.SolutionName, executeParams.VsVersion);
                    break;
            }
        }
    }
}