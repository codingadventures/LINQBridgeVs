using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LINQBridgeVs.Logging;
using Microsoft.Win32;

namespace LINQBridgeVs.Extension.Configuration
{
    internal static class PackageConfigurator
    {
        private static string _vsVersion;
        public static bool IsEnvironmentConfigured
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(GetRegistryKeyWithVersion(Resources.ConfigurationRegistryKey)))
                {
                    return key != null && Convert.ToBoolean(key.GetValue("IsLINQBridgeVsConfigured"));
                }
            }
            set
            {
                using (var key = Registry.CurrentUser.CreateSubKey(GetRegistryKeyWithVersion(Resources.ConfigurationRegistryKey)))
                {
                    if (key != null)
                        key.SetValue("IsLINQBridgeVsConfigured", value);
                }
            }
        }

        private static bool ArePermissionsSet
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(GetRegistryKeyWithVersion(Resources.ConfigurationRegistryKey)))
                {
                    return key != null && Convert.ToBoolean(key.GetValue("ArePermissionsSet"));
                }
            }
            set
            {
                using (var key = Registry.CurrentUser.CreateSubKey(GetRegistryKeyWithVersion(Resources.ConfigurationRegistryKey)))
                {
                    if (key != null)
                        key.SetValue("ArePermissionsSet", value);
                }
            }
        }

        public static void Configure(string vsVersion)
        {

            _vsVersion = vsVersion;
            Log.Write("Configuring LINQBridgeVs Extension");

            //Always check if installation folder has changed
            SetInstallationFolder();

            if (IsEnvironmentConfigured) return;

            Log.Write("Setting the Environment");
            SetEnvironment();
        }

        private static void SetEnvironment()
        {
            if (!Directory.Exists(Locations.LinqPadDestinationFolder))
            {
                Log.Write("Creating LinqPad directory {0}", Locations.LinqPadDestinationFolder);
                MessageBox.Show("Please Install LINQPad in order to Use LINQBridgeVs and then Restart Visual Studio");
                Process.Start("http://www.linqpad.net");
                return;
            }


            //Copy the BridgeBuildTask.targets to the default .NET 4.0v framework location
            File.Copy(Locations.LinqBridgeTargetFileNamePath, Path.Combine(Locations.DotNet40FrameworkPath, Locations.LinqBridgeTargetFileName), true);
            Log.Write("LinqBridge Targets copied to {0} ", Locations.DotNet40FrameworkPath);

            if (Environment.Is64BitOperatingSystem)
            {
                File.Copy(Locations.LinqBridgeTargetFileNamePath,
                    Path.Combine(Locations.DotNet40Framework64Path, Locations.LinqBridgeTargetFileName), true);
                Log.Write("LinqBridge Targets copied to {0} ", Locations.DotNet40Framework64Path);
            }
            SetPermissions();

            Log.Write("Setting IsEnvironmentConfigured to True");
            IsEnvironmentConfigured = true;


        }

        private static void SetInstallationFolder()
        {
            //Set in the registry the installer location if it is has changed
            using (var key = Registry.CurrentUser.CreateSubKey(GetRegistryKeyWithVersion(Resources.ProductRegistryKey)))
            {
                if (key == null) return;

                var value = key.GetValue("InstallFolderPath");

                if (value != null && value.Equals(Locations.InstallFolder)) return;

                Log.Write("Setting InstallFolderPath to {0}", Locations.InstallFolder);
                key.SetValue("InstallFolderPath", Locations.InstallFolder);
            }
        }

        private static void SetPermissions()
        {
            if (ArePermissionsSet) return;

            Log.Write("SetPermission Starts");
            var processOutputs = new StringBuilder();

            var process = new Process
            {
                StartInfo = { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardError = true, RedirectStandardInput = true, RedirectStandardOutput = true, FileName = "icacls.exe", Arguments = Locations.IcaclsArguments }
            };
            var processX64 = new Process
            {
                StartInfo = { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardError = true, FileName = "icacls.exe", Arguments = Locations.IcaclsArgumentsX64, LoadUserProfile = true }
            };
            var processCommonTarget = new Process
            {
                StartInfo = { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardError = true, RedirectStandardInput = true, RedirectStandardOutput = true, FileName = "icacls.exe", Arguments = Locations.IcaclsArgumentsCommonTarget, LoadUserProfile = true }
            };
            var processX64CommonTarget = new Process
            {
                StartInfo = { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardError = true, RedirectStandardInput = true, RedirectStandardOutput = true, FileName = "icacls.exe", Arguments = Locations.IcaclsArgumentsX64CommonTarget, LoadUserProfile = true }
            };

            var takeownProcess = Process.Start("takeown", String.Format("/f {0}", Locations.MicrosoftCommonTargetFileNamePath));
            if (takeownProcess != null) takeownProcess.WaitForExit();

            if (Environment.Is64BitOperatingSystem)
            {
                var takeownProcessx64 = Process.Start("takeown",
                    String.Format("/f {0}", Locations.MicrosoftCommonTarget64FileNamePath));

                if (takeownProcessx64 != null) takeownProcessx64.WaitForExit();
            }
            process.Start();
            processCommonTarget.Start();

            if (Environment.Is64BitOperatingSystem)
            {
                processX64.Start();
                processX64CommonTarget.Start();
                Log.Write("Setting Permission to {0} and {1} ", Locations.IcaclsArguments, Locations.IcaclsArgumentsX64);
                Log.Write("Setting Permission to {0} and {1} ", Locations.IcaclsArgumentsCommonTarget, Locations.IcaclsArgumentsX64CommonTarget);

            }


            process.WaitForExit();
            processCommonTarget.WaitForExit();
            if (Environment.Is64BitOperatingSystem)
            {
                processX64.WaitForExit();
                processX64CommonTarget.WaitForExit();
            }

            if (process.ExitCode != 0)
                processOutputs.AppendLine(process.StandardOutput.ReadToEnd());
            if (process.ExitCode != 0)
                processOutputs.AppendLine(processCommonTarget.StandardOutput.ReadToEnd());
            if (Environment.Is64BitOperatingSystem)
            {
                if (process.ExitCode != 0)
                    processOutputs.AppendLine(processX64.StandardOutput.ReadToEnd());
                if (process.ExitCode != 0)
                    processOutputs.AppendLine(processX64CommonTarget.StandardOutput.ReadToEnd());
            }


            Log.Write(processOutputs.ToString());


            Log.Write("SetPermission Done");
            ArePermissionsSet = true;
        }

        private static string GetRegistryKeyWithVersion(string key)
        {
            return String.Format(key, _vsVersion);
        }

        public static void EnableProject(string assemblyName)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(GetRegistryKeyWithVersion(Resources.EnabledProjectsRegistryKey)))
            {
                if (key != null)
                    key.SetValue(assemblyName, true);
            }
        }

        public static void DisableProject(string assemblyName)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(GetRegistryKeyWithVersion(Resources.EnabledProjectsRegistryKey), true))
            {
                if (key != null) key.SetValue(assemblyName, false);
            }

        }

        public static bool IsBridgeEnabled(string assemblyName)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(GetRegistryKeyWithVersion(Resources.EnabledProjectsRegistryKey), true))
            {
                if (key == null) return false;
                var value = key.GetValue(assemblyName);
                return value != null && Convert.ToBoolean(value);
            }
        }

        public static bool IsBridgeDisabled(string assemblyName)
        {
            return !IsBridgeEnabled(assemblyName);
        }
    }
}
