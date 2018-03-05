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
using System.Diagnostics;
using System.IO;
using System.Linq;
using BridgeVs.Build.TypeMapper;
using BridgeVs.Build.Util;
using BridgeVs.Logging;
using ILMerging;
using Microsoft.Build.Framework;

namespace BridgeVs.Build.Tasks
{
    public class MapperBuildTask : ITask
    {
        #region [ Properties ]

        [Required]
        public string Assembly { private get; set; }

        [Required]
        public string VisualStudioVer { private get; set; }

        private string TargetVisualizerAssemblyName
            => VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(VisualStudioVer, Assembly);
        private string DotNetVisualizerAssemblyName
           => VisualizerAssemblyNameFormat.GetDotNetVisualizerName(VisualStudioVer);

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
            try
            {
                Log.Configure("LINQBridgeVs", "MapperBuildTask");

                //this is where the current assembly being built is saved
                var currentBuildingFolder = Path.GetDirectoryName(Assembly);

                //usually Documents\Visual Studio 20xx\Visualizers
                var targetInstallationPath = VisualStudioOptions.GetInstallationPath(VisualStudioVer);
                Log.Write("Installation Path {0}", targetInstallationPath);

                //it gets the source path for the visualizer to use (usually from the extension directory)
                //usually from the folder where the extension is installed (BridgeVs.DynamicVisualizer.V1x)
                var dynamicVisualizerSourceAssemblyPath = VisualStudioOptions.GetVisualizerAssemblyLocation(VisualStudioVer);

                //it creates a mapping for all of the .net types that are worth exporting
                CreateDotNetFrameworkVisualizer(currentBuildingFolder, targetInstallationPath, dynamicVisualizerSourceAssemblyPath);

                //this returns 
                string temporaryVisualizerFilePath = CreateDebuggerVisualizer(currentBuildingFolder, dynamicVisualizerSourceAssemblyPath);

                MergeVisualizerWithDependencies(targetInstallationPath, temporaryVisualizerFilePath);

                Log.Write("Assembly {0} Mapped", Assembly);

                //deletes the temporary visualizer, this is no longer needed
                File.Delete(temporaryVisualizerFilePath);

                return true;
            }
            catch (Exception e)
            {
                Log.Write(e, @"Error Executing MSBuild Task MapperBuildTask ");
                return false;
            }
        }

        private string CreateDebuggerVisualizer(string currentBuildingFolder, string dynamicVisualizerSourceAssemblyPath)
        {
            Log.Write("Visualizer Assembly location {0}", dynamicVisualizerSourceAssemblyPath);

            var attributeInjector = new VisualizerAttributeInjector(dynamicVisualizerSourceAssemblyPath);

            attributeInjector.MapTypesFromAssembly(Assembly);

            var temporaryVisualizerFilePath = Path.Combine(currentBuildingFolder, TargetVisualizerAssemblyName);

            attributeInjector.SaveDebuggerVisualizer(temporaryVisualizerFilePath);
            return temporaryVisualizerFilePath;
        }

        private void CreateDotNetFrameworkVisualizer(string targetFolder, string installationFolder, string sourceVisualizerAssemblyPath)
        {
            //this is the place where the mapped dot net visualizer will be saved and then read
            var sourceDotNetAssemblyVisualizerFilePath = Path.Combine(targetFolder, DotNetVisualizerAssemblyName);

            //map dot net framework types only if the assembly does not exist
            //it create such maps in the building folder
            MapDotNetFrameworkTypes(sourceDotNetAssemblyVisualizerFilePath, sourceVisualizerAssemblyPath);

            //this is the target location for the dot net visualizer
            var targetDotNetAssemblyVisualizerFilePath = Path.Combine(installationFolder, DotNetVisualizerAssemblyName);

            //now dependencies needs to be merged
            ILMerge merge = new ILMerge()
            {
                OutputFile = targetDotNetAssemblyVisualizerFilePath,
                DebugInfo = true,
                TargetKind = ILMerge.Kind.SameAsPrimaryAssembly,
                Closed = false
            };

            //the order of input assemblies does matter. the first assembly is used as a template (for assembly attributes also)
            //for merging all the rest into it
            merge.SetInputAssemblies(new[] {
                    sourceDotNetAssemblyVisualizerFilePath, //we need to self reference it
                    typeof(Newtonsoft.Json.DateFormatHandling).Assembly.Location,
                    typeof(Grapple.Truck).Assembly.Location,
                    typeof(Log).Assembly.Location,
                    typeof(System.IO.Abstractions.DirectoryBase).Assembly.Location
                });

            //delete the temporary visualizer to avoid it dangles in the output folder (Debug/Release)
            File.Delete(sourceDotNetAssemblyVisualizerFilePath);
        }

        private void MergeVisualizerWithDependencies(string visualizerInstallationPath, string temporaryVisualizerFilePath)
        {
            var customVisualizerTargetInstallationPath = Path.Combine(visualizerInstallationPath, TargetVisualizerAssemblyName);

            if (customVisualizerTargetInstallationPath == temporaryVisualizerFilePath)
            {
                throw new ArgumentException("Installation Path and temporary path cannot be the same", nameof(temporaryVisualizerFilePath));
            }

            Debug.Assert(customVisualizerTargetInstallationPath != temporaryVisualizerFilePath, "Source and destination cannot be the same");

            ILMerge merge = new ILMerge()
            {
                OutputFile = customVisualizerTargetInstallationPath,
                DebugInfo = true,
                TargetKind = ILMerge.Kind.SameAsPrimaryAssembly,
                Closed = false
            };

            //the order of input assemblies does matter. the first assembly is used as a template (for assembly attributes also)
            //for merging all the rest into it
            merge.SetInputAssemblies(new[] {
                    temporaryVisualizerFilePath,
                    typeof(DynamicCore.DynamicDebuggerVisualizer).Assembly.Location,
                    typeof(Newtonsoft.Json.DateFormatHandling).Assembly.Location,
                    typeof(Grapple.Truck).Assembly.Location,
                    typeof(Log).Assembly.Location,
                    typeof(System.IO.Abstractions.DirectoryBase).Assembly.Location
                });

            merge.SetSearchDirectories(new[] { Path.GetDirectoryName(this.GetType().Assembly.Location) });
            merge.Merge();
        }
        /// <summary>
        /// Maps the dot net framework types. If the file already exists for a given vs version it won't be
        /// regenerated.
        /// </summary>
        /// <param name="targetFilePath">The target visualizer installation path.</param>
        /// <param name="sourceVisualizerAssemblyLocation">The source visualizer assembly location.</param>
        public static void MapDotNetFrameworkTypes(string targetFilePath, string sourceVisualizerAssemblyLocation)
        {
            if (targetFilePath == null)
                throw new ArgumentException(@"Target folder cannot be null", nameof(targetFilePath));

            if (string.IsNullOrEmpty(sourceVisualizerAssemblyLocation))
                throw new ArgumentException(@"Visualizer Assembly Location cannot be null", nameof(sourceVisualizerAssemblyLocation));

            if (File.Exists(targetFilePath))
            {
                //file already exists don't create it again
                return;
            }

            var visualizerInjector = new VisualizerAttributeInjector(sourceVisualizerAssemblyLocation);

            //Map all the possible System  types
            var systemLinqTypes = typeof(IOrderedEnumerable<>).Assembly
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
            var systemGenericsTypes = typeof(IList<>).Assembly
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