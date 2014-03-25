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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Build.Evaluation;

namespace LINQBridgeVs.Extension.Dependency
{
    internal class Crawler
    {
        private const string MsbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        /// <summary>
        /// Finds the project dependencies given a csproj or vbproj file
        /// </summary>
        /// <param name="csvbProjectName">Name of the CS or VB project.</param>
        /// <returns></returns>
        public static IEnumerable<Project> FindProjectDependencies(string csvbProjectName)
        {
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("aw", MsbuildNamespace);


            var loadedProject = ProjectCollection.GlobalProjectCollection.LoadedProjects.FirstOrDefault(
                p => p.FullPath.Equals(csvbProjectName)) ?? new Microsoft.Build.Evaluation.Project(csvbProjectName);

            return
                from proj in loadedProject.Items
                where proj.ItemType.Equals("ProjectReference") || proj.ItemType.Equals("Reference")
                where !proj.EvaluatedInclude.Contains("Microsoft") && !proj.EvaluatedInclude.Contains("System")
                select new Project
                {
                    DependencyType =
                        proj.ItemType.Equals("Reference") ? DependencyType.AssemblyReference : DependencyType.ProjectReference,
                    AssemblyName = proj.ItemType.Equals("ProjectReference")
                        ? XDocument.Load(Path.Combine(loadedProject.DirectoryPath, proj.Xml.Include))
                            .XPathSelectElement("/aw:Project/aw:PropertyGroup/aw:AssemblyName", namespaceManager)
                            .Value
                        : string.Empty
                   
                };
        }
    }
}
