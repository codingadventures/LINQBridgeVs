namespace BridgeVs.Helper
{
    public class ExecuteParams
    {
        public ExecuteParams(CommandAction action, string projectName, string solutionName, string assemblyName, string projectOutput, string vsVersion, string vsEdition)
        {
            Action = action;
            ProjectName = projectName;
            SolutionName = solutionName;
            AssemblyName = assemblyName;
            ProjectOutput = projectOutput;
            VsVersion = vsVersion;
            VsEdition = vsEdition;
        }

        public CommandAction Action { get; }

        public string ProjectName { get; }

        public string SolutionName { get; }

        public string AssemblyName { get; }

        public string ProjectOutput { get; }

        public string VsVersion { get; }

        public string VsEdition { get; }
    }
}
