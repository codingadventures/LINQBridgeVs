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

using BridgeVs.Build.TypeMapper;
using BridgeVs.Build.Util;
using BridgeVs.Shared.Common;
using BridgeVs.Shared.Logging;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BridgeVs.Shared.Options;
using BridgeVs.Build.Dependency;

namespace BridgeVs.Build.Tasks
{
    public class MapperBuildTask : ITask
    {
        #region [ Properties ]

        [Required]
        public string Assembly { private get; set; }

        [Required]
        public string VisualStudioVer { private get; set; }

        [Required]
        public string SolutionName { get; set; }

        private string TargetVisualizerAssemblyName
            => VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(VisualStudioVer, Assembly);
        private string DotNetVisualizerAssemblyName
           => VisualizerAssemblyNameFormat.GetDotNetVisualizerName(VisualStudioVer);
        private string VisualizerDestinationFolder 
            => VisualStudioOption.GetVisualizerDestinationFolder(VisualStudioVer);

        public IBuildEngine BuildEngine { get; set; }

        public ITaskHost HostObject { get; set; }

        #endregion

        /// <inheritdoc />
        /// <summary>
        ///     Executes an ITask. It creates a DynamicDebuggerVisualizer mapping all the types of a given assembly
        /// </summary>
        /// <returns>
        ///     true if the task executed successfully; otherwise, false.
        /// </returns>
        public bool Execute()
        {
            Log.VisualStudioVersion = VisualStudioVer;
            
            if (!CommonRegistryConfigurations.IsSolutionEnabled(SolutionName, VisualStudioVer))
            {
                return true;
            }

            try
            {
                //this is where the current assembly being built is saved
                string currentBuildingFolder = Path.GetDirectoryName(Assembly);

                Create3rdPartyVisualizers();

                Log.Write($"Visualizer Destination Folder Path {VisualizerDestinationFolder}");

                string dynamicVisualizerSourceAssemblyPath = typeof(DynamicVisualizers.DynamicDebuggerVisualizer).Assembly.Location;

                //if dot net visualizer exists already don't create it again
                if (!File.Exists(Path.Combine(VisualizerDestinationFolder, DotNetVisualizerAssemblyName)))
                {
                    //it creates a mapping for all of the .net types that are worth exporting
                    CreateDotNetFrameworkVisualizer(currentBuildingFolder, dynamicVisualizerSourceAssemblyPath);
                }

                CreateDebuggerVisualizer(dynamicVisualizerSourceAssemblyPath);

                return true;
            }
            catch (Exception e)
            {
                const string errorMessage = "Error Executing MSBuild Task MapperBuildTask";
                Log.Write(e, errorMessage);

                e.Capture(VisualStudioVer, message: errorMessage);

                return false;
            }
        }

        private void Create3rdPartyVisualizers()
        {
           if (CommonRegistryConfigurations.Map3rdPartyAssembly(SolutionName, VisualStudioVer))
            {
                var references = Crawler.FindDependencies(BuildEngine.ProjectFileOfTaskNode);
                foreach (ProjectDependency assReference in references)
                {
                    VisualizerAttributeInjector attributeInjector = new VisualizerAttributeInjector(dynamicVisualizerSourceAssemblyPath, VisualStudioVer);

                    attributeInjector.MapTypesFromAssembly(assReference.AssemblyPath);

                    string targetInstallationFilePath = Path.Combine(VisualizerDestinationFolder, TargetVisualizerAssemblyName);

                    attributeInjector.SaveDebuggerVisualizer(targetInstallationFilePath);
                }
            }
        }

        private void CreateDebuggerVisualizer(string dynamicVisualizerSourceAssemblyPath)
        {
            Log.Write("Visualizer Assembly location {0}", dynamicVisualizerSourceAssemblyPath);

            VisualizerAttributeInjector attributeInjector = new VisualizerAttributeInjector(dynamicVisualizerSourceAssemblyPath, VisualStudioVer);

            attributeInjector.MapTypesFromAssembly(Assembly);

            string targetInstallationFilePath = Path.Combine(VisualizerDestinationFolder, TargetVisualizerAssemblyName);

            attributeInjector.SaveDebuggerVisualizer(targetInstallationFilePath);

            Log.Write("Assembly {0} Mapped", Assembly);
        }

        private void CreateDotNetFrameworkVisualizer(string targetFolder, string sourceVisualizerAssemblyPath)
        {
            //this is the place where the mapped dot net visualizer will be saved and then read
            string sourceDotNetAssemblyVisualizerFilePath = Path.Combine(targetFolder, DotNetVisualizerAssemblyName);

            //this is the target location for the dot net visualizer
            string targetDotNetAssemblyVisualizerFilePath = Path.Combine(VisualizerDestinationFolder, DotNetVisualizerAssemblyName);

            //map dot net framework types only if the assembly does not exist
            //it create such maps in the building folder
            MapDotNetFrameworkTypes(targetDotNetAssemblyVisualizerFilePath, sourceVisualizerAssemblyPath);

            //delete the temporary visualizer to avoid it dangling in the output folder (Debug/Release)
            File.Delete(sourceDotNetAssemblyVisualizerFilePath);
            File.Delete(Path.ChangeExtension(sourceDotNetAssemblyVisualizerFilePath, "pdb"));
        }

        /// <summary>
        /// Maps the dot net framework types. If the file already exists for a given vs version it won't be
        /// regenerated.
        /// </summary>
        /// <param name="targetFilePath">The target visualizer installation path.</param>
        /// <param name="sourceVisualizerAssemblyLocation">The source visualizer assembly location.</param>
        public void MapDotNetFrameworkTypes(string targetFilePath, string sourceVisualizerAssemblyLocation)
        {
            if (targetFilePath == null)
                throw new ArgumentException(@"Target folder cannot be null", nameof(targetFilePath));

            if (string.IsNullOrEmpty(sourceVisualizerAssemblyLocation))
                throw new ArgumentException(@"Visualizer Assembly Location cannot be null", nameof(sourceVisualizerAssemblyLocation));

            VisualizerAttributeInjector visualizerInjector = new VisualizerAttributeInjector(sourceVisualizerAssemblyLocation, VisualStudioVer);

            //Map all the possible System  types
            IEnumerable<Type> systemLinqTypes = typeof(IOrderedEnumerable<>).Assembly
                .GetTypes()
                .Where(type => type != null
                               && (
                                   (type.IsClass && type.IsSerializable)
                                   ||
                                   type.IsInterface
                                   ||
                                   type.Name.Contains("Iterator")
                                  )
                               && !(type.Name.Contains("Func") || type.Name.Contains("Action"))
                               && !string.IsNullOrEmpty(type.Namespace));

            //Map all the possible list types
            IEnumerable<Type> systemGenericsTypes = typeof(IList<>).Assembly
                .GetTypes()
                .Where(type => type != null
                               && (
                                   (type.IsClass && type.IsSerializable)
                                   ||
                                   type.IsInterface
                                  )
                               && !string.IsNullOrEmpty(type.Namespace)
                               && type.IsPublic)
                .Where(type =>
                    !type.Name.Contains("ValueType")
                    && !type.Name.Contains("IFormattable")
                    && !type.Name.Contains("IComparable")
                    && !type.Name.Contains("IConvertible")
                    && !type.Name.Contains("IEquatable")
                    && !type.Name.Contains("Object")
                    && !type.Name.Contains("ICloneable")
                    && !type.Name.Contains("String")
                    && !type.Name.Contains("IDisposable"));

            systemLinqTypes.ForEach(visualizerInjector.MapType);
            systemGenericsTypes.ForEach(visualizerInjector.MapType);

            visualizerInjector.SaveDebuggerVisualizer(targetFilePath);
        }
    }
}
