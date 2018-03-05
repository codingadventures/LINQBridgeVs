#region License
// Copyright (c) 2013 - 2018 Giovanni Campo
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
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using BridgeVs.Helper;
using BridgeVs.Helper.Configuration;
using EnvDTE;
using Project = EnvDTE.Project;

namespace BridgeVs.Extension
{
    public class LINQBridgeVsExtension
    {
        #region [ Private Properties ]
        private readonly DTE _application;
        #endregion

        public LINQBridgeVsExtension(DTE app)
        {
            _application = app; 
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

                var project = items?.OfType<Project>().FirstOrDefault();
                if (project == null || !IsSupported(project.UniqueName))
                    return null;

                return project;
            }
        }

        private string _solutionName;
        private string SolutionName
        {
            get
            {
                if (string.IsNullOrEmpty(_solutionName))
                    _solutionName = Path.GetFileNameWithoutExtension(_application.Solution.FileName);

                return _solutionName;
            }
        }

        private string SelectedAssemblyName => SelectedProject.Properties.Item("AssemblyName").Value.ToString();

        private string SelectedProjectOutputPath
        {
            get
            {
                var path = SelectedProject.Properties.Item("FullPath").Value.ToString();
                var outputPath = SelectedProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
                var fileName = SelectedProject.Properties.Item("OutputFileName").Value.ToString();
                return Path.Combine(path, outputPath, fileName);
            }
        }

        public void Execute(CommandAction action)
        {
            if (SelectedProject == null)
                return;

            BridgeTrigger.Execute(new BridgeTrigger.ExecuteParams(action, SelectedProject.FullName, SolutionName, SelectedAssemblyName, SelectedProjectOutputPath));
        }

        public void UpdateCommand(MenuCommand cmd, CommandAction action)
        {
            var states = GetStatus(action);
            cmd.Visible = (CommandStates.Visible & states) != 0;
            cmd.Enabled = (CommandStates.Enabled & states) != 0 && PackageConfigurator.IsLINQBridgeVsConfigured;
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

            if (PackageConfigurator.IsBridgeDisabled(SelectedAssemblyName, SolutionName))
                result |= 1;

            if (PackageConfigurator.IsBridgeEnabled(SelectedAssemblyName, SolutionName))
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