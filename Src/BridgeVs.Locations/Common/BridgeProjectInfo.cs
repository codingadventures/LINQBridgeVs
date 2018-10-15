using System.Collections.Generic;

namespace BridgeVs.VsPackage.Helper.Command
{
    public class BridgeProjectInfo
    {
        public BridgeProjectInfo(string projectName, string solutionName, string assemblyName, string projectOutput, string vsVersion, string vsEdition, List<string> references)
        {
            ProjectName = projectName;
            SolutionName = solutionName;
            AssemblyName = assemblyName;
            ProjectOutput = projectOutput;
            VsVersion = vsVersion;
            VsEdition = vsEdition;
            References = references;
        }
         

        public string ProjectName { get; }

        public List<string> References;

        public string SolutionName { get; }

        public string AssemblyName { get; }

        public string ProjectOutput { get; }

        public string VsVersion { get; }

        public string VsEdition { get; }
    }
}
