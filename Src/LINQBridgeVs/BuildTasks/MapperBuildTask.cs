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

using System;
using System.Linq;
using LINQBridgeVs.Logging;
using LINQBridgeVs.TypeMapper;
using LINQBridgeVs.VisualStudio;
using Microsoft.Build.Framework;

namespace LINQBridgeVs.BuildTasks
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
                  
                var installationPaths = VisualStudioOptions.GetInstallationPath(VisualStudioVer).ToList();

                var visualizerAssemblyLocation = VisualStudioOptions.GetVisualizerAssemblyLocation(VisualStudioVer);

                VisualizerTypeMapper.MapDotNetFrameworkTypes(installationPaths, VisualStudioVer,
                    visualizerAssemblyLocation);

                var typeMapper = new VisualizerTypeMapper(visualizerAssemblyLocation);

                typeMapper.MapAssembly(Assembly);

                typeMapper.Save(installationPaths, VisualizerAssemblyName);


                return true;
            }
            catch (Exception e)
            {
                Log.Write(e);
                Console.WriteLine(@"Error Executing MSBuild Task MapperBuildTask " + e.Message);
                return false;
            }
        }

        #region [ Properties ]

        [Required]
        public string Assembly { private get; set; }

        [Required]
        public string VisualStudioVer { private get; set; }

        private string VisualizerAssemblyName
        {
            get
            {
                return VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(VisualStudioVer, Assembly);
            }
        }
        public IBuildEngine BuildEngine { get; set; }

        public ITaskHost HostObject { get; set; }

        #endregion
    }
}