using Microsoft.Win32;

namespace BridgeVs.Locations
{
    public class CommonRegistryConfigurations
    {
        // ReSharper disable once InconsistentNaming
        private const string LINQPadInstallationPathRegistryValue = "LINQPadInstallationPath";
        // ReSharper disable once InconsistentNaming
        private const string LINQPadVersionPathRegistryValue = "LINQPadVersion";

        // ReSharper disable once InconsistentNaming
        public static string GetLINQPadInstallationPath(string vsVersion)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey($@"Software\LINQBridgeVs\{vsVersion}"))
            {
                return key?.GetValue(LINQPadInstallationPathRegistryValue) as string;
            }
        }
        public static void SetLINQPadInstallationPath(string vsVersion, string installationPath)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"Software\LINQBridgeVs\{vsVersion}"))
            {
                key?.SetValue(LINQPadInstallationPathRegistryValue, installationPath);
            }
        }
    }
}
