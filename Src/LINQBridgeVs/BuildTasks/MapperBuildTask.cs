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
using BridgeVs.Logging;
using BridgeVs.TypeMapper;
using BridgeVs.VisualStudio;
using Microsoft.Build.Framework;
using ILMerging;
using System.IO;

namespace BridgeVs.BuildTasks
{
    public class MapperBuildTask : ITask
    {
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

                var visualizerInstallationPath = VisualStudioOptions.GetInstallationPath(VisualStudioVer);

                Log.Write("Installation Path {0}", visualizerInstallationPath);

                var templateVisualizerAssemblyPath = VisualStudioOptions.GetVisualizerAssemblyLocation(VisualStudioVer);

                //map dot net framework types only if the assembly does not exist
                var dotNetAssemblyVisualizerFilePath = Path.Combine(visualizerInstallationPath, templateVisualizerAssemblyPath);

                VisualizerTypeMapper.MapDotNetFrameworkTypes(visualizerInstallationPath, VisualStudioVer, templateVisualizerAssemblyPath);

                Log.Write("Visualizer Assembly location {0}", templateVisualizerAssemblyPath);

                var typeMapper = new VisualizerTypeMapper(templateVisualizerAssemblyPath);

                typeMapper.MapAssembly(Assembly);

                //var currentAssemblyPath = Path.GetDirectoryName(Assembly);
                var visualizerAssemblyPath = Path.Combine(visualizerInstallationPath, VisualizerAssemblyName);
                typeMapper.Save(visualizerAssemblyPath);

                ILMerge merge = new ILMerge()
                {
                    OutputFile = Path.Combine(visualizerInstallationPath, VisualizerAssemblyName),
                    DebugInfo = false,
                    TargetKind = ILMerge.Kind.SameAsPrimaryAssembly,
                    Closed = false
                };

                //the order of input assemblies does matter. the first assembly is used as a template (for assembly attributes also)
                //for merging all the rest into it
                merge.SetInputAssemblies(new[] {
                    visualizerAssemblyPath,
                    typeof(DynamicCore.DynamicDebuggerVisualizer).Assembly.Location,
                    typeof(Newtonsoft.Json.DateFormatHandling).Assembly.Location,
                    typeof(Grapple.Truck).Assembly.Location,
                    typeof(Log).Assembly.Location,
                    typeof(System.IO.Abstractions.DirectoryBase).Assembly.Location
                });

                merge.SetSearchDirectories(new[] { Path.GetDirectoryName(this.GetType().Assembly.Location) });
                merge.Merge();

                Log.Write("Assembly {0} Mapped", Assembly);

                return true;
            }
            catch (Exception e)
            {
                Log.Write(e, @"Error Executing MSBuild Task MapperBuildTask ");
                return false;
            }
            return true;
        }

        #region [ Properties ]

        [Required]
        public string Assembly { private get; set; }

        [Required]
        public string VisualStudioVer { private get; set; }

        private string VisualizerAssemblyName
            => VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(VisualStudioVer, Assembly);

        public IBuildEngine BuildEngine { get; set; }

        public ITaskHost HostObject { get; set; }

        #endregion
    }
}