using System;
using System.Collections.Generic;
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
            var typeMapper = new VisualizerTypeMapper<LINQPadDebuggerVisualizer>(Assembly, Visualizers.Properties.Resources.VisualizerName);

            typeMapper.Create(new List<Type> { typeof(Dictionary<,>), typeof(List<>) });

            VisualStudioOptions.VisualStudioPaths[VisualStudioVersion.VS2010].ForEach(typeMapper.Save);

            return true;
        }

        [Required]
        public string Assembly { get; set; }

        [Required]
        public string References { get; set; }

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }
    }
}
