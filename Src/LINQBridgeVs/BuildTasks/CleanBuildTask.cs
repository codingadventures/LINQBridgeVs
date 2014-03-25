using System.IO;
using Microsoft.Build.Framework;

namespace LINQBridgeVs.BuildTasks
{
    public class CleanBuildTask : ITask
    {
        public bool Execute()
        {
            var visualizerAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(VisualStudioVer, Assembly);

            if (File.Exists(visualizerAssemblyName))
                File.Delete(visualizerAssemblyName);

            return true;
        }

        [Required]
        public string VisualStudioVer { private get; set; }

        [Required]
        public string Assembly { private get; set; }

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }
    }
}
