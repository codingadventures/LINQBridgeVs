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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using BridgeVs.Logging;
using Microsoft.Build.Evaluation;

namespace BridgeVs.Helper.Dependency
{
    public class Crawler
    {
        private const string MsbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        /// <summary>
        /// Finds the project dependencies given a csproj or vbproj file
        /// </summary>
        /// <param name="csvbProjectName">Name of the CS or VB project.</param>
        /// <param name="solutionName">Name of the solution.</param>
        /// <returns></returns>
        public static IEnumerable<Dependency> FindDependencies(string csvbProjectName, string solutionName)
        {
            if (string.IsNullOrEmpty(csvbProjectName))
            {
                Log.Write("BridgeVs.Helper.Dependency.Crawler: csvbProjectName is null or empty");
                return Enumerable.Empty<Dependency>();
            }

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("aw", MsbuildNamespace);


            Project loadedProject =
                ProjectCollection.GlobalProjectCollection.LoadedProjects
                .FirstOrDefault(p => CompareProjectFileName(p, csvbProjectName))
                ??
                new Project(csvbProjectName);

            return
                from proj in loadedProject.Items
                where proj.ItemType.Equals("ProjectReference") || proj.ItemType.Equals("Reference")
                where !proj.EvaluatedInclude.Contains("Microsoft") && !proj.EvaluatedInclude.Contains("System")
                where proj.ItemType.Equals("ProjectReference")
                let referenceProjectFileName = Path.Combine(loadedProject.DirectoryPath, proj.Xml.Include)
                let referenceProjectPath = Path.GetDirectoryName(referenceProjectFileName)
                let referenceProjectXDocument = XDocument.Load(referenceProjectFileName)
                let assemblyName = referenceProjectXDocument.XPathSelectElement("/aw:Project/aw:PropertyGroup/aw:AssemblyName", namespaceManager)?.Value
                let outputType = referenceProjectXDocument.XPathSelectElement("/aw:Project/aw:PropertyGroup/aw:OutputType", namespaceManager)?.Value
                let outputPath = referenceProjectXDocument.XPathSelectElement("/aw:Project/aw:PropertyGroup/aw:OutputPath", namespaceManager)?.Value
                select new Dependency
                {
                    DependencyType =
                        proj.ItemType.Equals("Reference") ? DependencyType.AssemblyReference : DependencyType.ProjectReference,
                    AssemblyName = assemblyName,
                    AssemblyPath = Path.GetFullPath(Path.Combine(referenceProjectPath, outputPath, assemblyName + (outputType.Equals("Library") ? ".dll" : ".exe"))),
                    SolutionName = solutionName
                };
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