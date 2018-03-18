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
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using BridgeVs.Helper;
using BridgeVs.Helper.Configuration;
using EnvDTE;
using Project = EnvDTE.Project;

namespace BridgeVs.Extension
{
    public class BridgeVsExtension
    {
        #region [ Private Properties ]
        private readonly DTE _application;
        #endregion

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
                Projects projects = _application.Solution.Projects;
                if (projects == null)
                    return Enumerable.Empty<Project>();

                return from project in projects.Cast<Project>()
                       where IsSupported(project.UniqueName)
                       select project;
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

        public void Execute(CommandAction action)
        {
            List<Project> projects = AllProjects.ToList();

            if (projects.Count == 0)
                return;

            BridgeCommand.ActivateBridgeVsOnSolution(projects, SolutionName, _application.Version, _application.Edition);
        }

        public void UpdateCommand(MenuCommand cmd, CommandAction action)
        {
            CommandStates states = GetStatus(action);
            cmd.Visible = (CommandStates.Visible & states) == CommandStates.Visible;
            cmd.Enabled = (CommandStates.Enabled & states) == CommandStates.Enabled;
        }

        private CommandStates GetStatus(CommandAction action)
        {
            CommandStates result = CommandStates.Visible;

            bool isBridgeVsConfigured = PackageConfigurator.IsBridgeVsConfigured(_application.Version);

            if (!isBridgeVsConfigured)
                return result; //just show it as visible

            bool isSolutionEnabled = BridgeCommand.IsSolutionEnabled(SolutionName, _application.Version);

            if (isSolutionEnabled && action == CommandAction.Disable || !isSolutionEnabled && action == CommandAction.Enable)
                result |= CommandStates.Enabled;

            return result;
        }

        private CommandStates GetMultiStatus()
        {
            CommandStates result = CommandStates.Visible;

            bool isBridgeVsConfigured = PackageConfigurator.IsBridgeVsConfigured(_application.Version);
            bool isSolutionEnabled = BridgeCommand.IsSolutionEnabled(SolutionName, _application.Version);
            //if (isBridgeVsEnabled)
            //    result |= CommandStates.Visible;

            if (isBridgeVsConfigured && isSolutionEnabled)
                result |= CommandStates.Enabled;

            return result;
        }

        //private static CommandStates GetCommandStatus(CommandStates status, CommandAction action)
        //{
        //    if (status == CommandStates.None)
        //        return CommandStates.None;

        //    bool result = ((action == CommandAction.Disable ? status >> 1 : status) & 1) != 0;

        //    if (result)
        //        return CommandStates.Enabled | CommandStates.Visible;

        //    return CommandStates.None;
        //}

    }
}