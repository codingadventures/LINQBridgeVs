using Microsoft.Win32;

namespace BridgeVs.Locations
{
    public class CommonRegistryConfigurations
    {
        private const string LINQPadInstallationPathRegistryValue = "LINQPadInstallationPath";
        private const string LINQPadVersionPathRegistryValue = "LINQPadVersion";

        // ReSharper disable once InconsistentNaming
        public static string LINQPadInstallationPath
        {
            get
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\LINQBridgeVs\"))
                {
                    return key?.GetValue(LINQPadInstallationPathRegistryValue) as string;
                }
            }
            set
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\LINQBridgeVs\"))
                {
                    key?.SetValue(LINQPadInstallationPathRegistryValue, value);
                }
            }
        }

        // ReSharper disable once InconsistentNaming
        public static string LINQPadVersion
        {
            get
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\LINQBridgeVs\"))
                {
                    return key?.GetValue(LINQPadVersionPathRegistryValue) as string;
                }
            }
            set
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\LINQBridgeVs\"))
                {
                    key?.SetValue(LINQPadVersionPathRegistryValue, value);
                }
            }
        }
    }
}
