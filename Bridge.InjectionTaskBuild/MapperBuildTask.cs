using System;
using System.Collections.Generic;
using System.IO;
using Bridge.TypeMapper;
using Bridge.Visualizers;

using Microsoft.Build.Framework;


namespace Bridge.InjectionBuildTask
{
    public class MapperBuildTask : ITask
    {
        /// <summary>
        /// Executes an ITask. It creates a LINQPadDebuggerVisualizer mapping all the types of a given assembly
        /// 
        /// </summary>
        /// <returns>
        /// true if the task executed successfully; otherwise, false.
        /// </returns>
        public bool Execute()
        {
#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif
            var typeMapper = new VisualizerTypeMapper<LINQPadDebuggerVisualizer>(Assembly, Visualizers.Properties.Resources.VisualizerName);

            typeMapper.Create(new List<Type> { typeof(Dictionary<,>), typeof(List<>) });

            VisualStudioOptions.VisualStudioPaths[VisualStudioVersion.VS2010].ForEach(typeMapper.Save);

            //Copy the resources associated with the visualizer
            VisualStudioOptions.VisualStudioPaths[VisualStudioVersion.VS2010].ForEach(path => File.Copy(Visualizers.Properties.Resources.LINQPadExe, path + Visualizers.Properties.Resources.LINQPadExe, true));
            VisualStudioOptions.VisualStudioPaths[VisualStudioVersion.VS2010].ForEach(path => File.Copy(Visualizers.Properties.Resources.LINQPadExe, path + Visualizers.Properties.Resources.LINQPadQuery, true));


            return true;
        }

        [Required]
        public string Assembly { private get; set; }

        [Required]
        public string VisualStudioVer { private get; set; }

        public string Resources { private get; set; }

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }
    }
}
