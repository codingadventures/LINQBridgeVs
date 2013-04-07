using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bridge.TypeMapper;
using Bridge.Visualizers;
using Microsoft.Build.Framework;

namespace Bridge.BuildTasks
{
    public class MapperBuildTask : ITask
    {
        [Required]
        public string Assembly { private get; set; }

        [Required]
        public string VisualStudioVer { private get; set; }


        public string Resources { private get; set; }

        /// <summary>
        ///     Executes an ITask. It creates a LINQPadDebuggerVisualizer mapping all the types of a given assembly
        /// </summary>
        /// <returns>
        ///     true if the task executed successfully; otherwise, false.
        /// </returns>
        public bool Execute()
        {
            var vsVer = (VisualStudioVersion)Enum.Parse(typeof(VisualStudioVersion), VisualStudioVer);

            var typeMapper = new VisualizerTypeMapper<LINQPadDebuggerVisualizer>(Assembly,
                                                                                 Visualizers.Properties.Resources
                                                                                            .VisualizerName);

            typeMapper.Create(new List<Type> { typeof(Dictionary<,>), typeof(List<>) });

            typeMapper.Save(VisualStudioOptions.VisualStudioPaths[vsVer]);

            return true;
        }

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }
    }
}