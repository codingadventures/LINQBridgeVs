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

using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace BridgeVs.Shared.Dependency
{
    public class Crawler
    {
        private const string MsbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        /// <summary>
        /// Finds the project dependencies given a csproj or vbproj file
        /// </summary>
        /// <param name="projectFilePath">Path to the CS or VB project.</param>
        /// <returns>a list of dependencies - path to the assembly</returns>
        public static IEnumerable<string> FindDependencies(string projectFilePath)
        {
            if (string.IsNullOrEmpty(projectFilePath))
            {
                return Enumerable.Empty<string>();
            }

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("aw", MsbuildNamespace);

            Project loadedProject =
                ProjectCollection.GlobalProjectCollection.LoadedProjects
                .FirstOrDefault(p => CompareProjectFileName(p, projectFilePath))
                ?? new Project(projectFilePath);

            IEnumerable<string> assemblyReference =
                from proj in loadedProject.Items
                where proj.ItemType.Equals("Reference") && !proj.EvaluatedInclude.Contains("Microsoft") && !proj.EvaluatedInclude.Contains("System")
                let infos = proj.DirectMetadata.Select(p => p.EvaluatedValue)
                from info in infos
                where !string.IsNullOrEmpty(info) && !(info.Equals("True") || info.Equals("False")) //it could be true or false for some reason
                select info;

            return assemblyReference;
        }

        private static readonly Func<Project, string, bool> CompareProjectFileName =
            (project, projectToCompare) =>
            {
                if (project == null) return false;

                string proj1FileName = Path.GetFileName(project.FullPath);
                string proj2FileName = Path.GetFileName(projectToCompare);

                return
                    !string.IsNullOrEmpty(proj1FileName)
                    && !string.IsNullOrEmpty(proj2FileName)
                    && proj1FileName.Equals(proj2FileName);
            };
    }
}
