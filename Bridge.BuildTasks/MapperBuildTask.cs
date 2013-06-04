 
using LINQBridge.TypeMapper;
using LINQBridge.VisualStudio;
using Microsoft.Build.Framework;

namespace LINQBridge.BuildTasks
{
    public class MapperBuildTask : ITask
    {
        [Required]
        public string Assembly { private get; set; }

        [Required]
        public string VisualStudioVer { private get; set; }


        public string Resources { private get; set; }

        private const string VisualizerName = "LINQBridge Visualizer";
        /// <summary>
        ///     Executes an ITask. It creates a DynamicDebuggerVisualizer mapping all the types of a given assembly
        /// </summary>
        /// <returns>
        ///     true if the task executed successfully; otherwise, false.
        /// </returns>
        public bool Execute()
        {
            var targetVisualizerAssemblyName = VisualStudioOptions.GetVisualizerAssemblyName(VisualStudioVer);
            var targetVisualizerAssemblyLocation = VisualStudioOptions.GetVisualizerAssemblyLocation(VisualStudioVer);

            var installationPath = VisualStudioOptions.GetInstallationPath(VisualStudioVer);
            var typeMapper = new VisualizerTypeMapper(targetVisualizerAssemblyLocation, Assembly, VisualizerName);

            typeMapper.Create();
            typeMapper.Save(installationPath, targetVisualizerAssemblyName);

            return true;
        }

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }
    }
}