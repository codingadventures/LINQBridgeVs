using System;
using System.IO;
using System.Reflection;

namespace LINQBridge.VSExtension
{
    internal static class Locations
    {

        public static string InstallFolder
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        public static readonly string LinqPadDestinationFolder = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), "LINQPad4");

        public static readonly string LinqBridgeTargetFileNamePath = Path.Combine(InstallFolder, Resources.Targets);
        public static readonly string LinqBridgeTargetFileName = Path.GetFileName(Resources.Targets);

        public static readonly string LinqPadExeFileNamePath = Path.Combine(InstallFolder, Resources.LINQPad);

        public static readonly string DotNet40FrameworkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                @"Microsoft.NET\Framework\v4.0.30319");

        public static readonly string DotNet40Framework64Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                       @"Microsoft.NET\Framework64\v4.0.30319");

        public static readonly string MicrosoftCommonTargetFileNamePath = Path.Combine(DotNet40FrameworkPath, "Microsoft.Common.targets");

        public static readonly string MicrosoftCommonTarget64FileNamePath = Path.Combine(DotNet40Framework64Path, "Microsoft.Common.targets");

        public static readonly string IcaclsArguments = String.Format("{0} /grant Everyone:F", Path.Combine(DotNet40FrameworkPath, LinqBridgeTargetFileName));
        public static readonly string IcaclsArgumentsX64 = String.Format("{0} /grant Everyone:F", Path.Combine(DotNet40Framework64Path, LinqBridgeTargetFileName));

        public static readonly string IcaclsArgumentsCommonTarget = String.Format("{0} /grant Everyone:F", MicrosoftCommonTargetFileNamePath);
        public static readonly string IcaclsArgumentsX64CommonTarget = String.Format("{0} /grant Everyone:F", MicrosoftCommonTarget64FileNamePath);
    }
}
