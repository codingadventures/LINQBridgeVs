using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using LINQBridgeVs.Logging;
using Microsoft.Win32;

namespace LINQBridgeVs.Extension.Configuration
{
    internal static class PackageConfigurator
    {
        private static string _vsVersion;
        private static readonly string CurrentAssemblyVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        private static readonly bool IsFramework45Installed = Directory.Exists(Locations.DotNet45FrameworkPath);

        private static string LINQBridgeVsExtensionVersion
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(GetRegistryKeyWithVersion(Resources.ProductVersion)))
                {
                    if (key == null) return string.Empty;

                    var value = key.GetValue("LINQBridgeVsVersion");

                    if (value != null)
                        return value.ToString();
                }


                return string.Empty;
            }

            set
            {
                using (var key = Registry.CurrentUser.CreateSubKey(GetRegistryKeyWithVersion(Resources.ProductVersion)))
                {
                    if (key != null)
                        key.SetValue("LINQBridgeVsVersion", value);
                }
            }
        }

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

            if (LINQBridgeVsExtensionVersion != CurrentAssemblyVersion)
            {
                Log.Write("New LINQBridgeVs Extensions. Previous Version {0}. Current Version {1}", LINQBridgeVsExtensionVersion, CurrentAssemblyVersion);
                ArePermissionsSet
                    = IsEnvironmentConfigured = false;
                LINQBridgeVsExtensionVersion = CurrentAssemblyVersion;
            }

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

            SetPermissions();


            //Copy the BridgeBuildTask.targets to the default .NET 4.0v framework location
            File.Copy(Locations.LinqBridgeTargetFileNamePath, Path.Combine(Locations.DotNet40FrameworkPath, Locations.LinqBridgeTargetFileName), true);
            Log.Write("LinqBridge Targets copied to {0} ", Locations.DotNet40FrameworkPath);

            if (Environment.Is64BitOperatingSystem)
            {
                File.Copy(Locations.LinqBridgeTargetFileNamePath,
                    Path.Combine(Locations.DotNet40Framework64Path, Locations.LinqBridgeTargetFileName), true);
                Log.Write("LinqBridge Targets copied to {0} ", Locations.DotNet40Framework64Path);
            }

            if (IsFramework45Installed)
            {
                File.Copy(Locations.LinqBridgeTargetFileNamePath,
                  Path.Combine(Locations.DotNet45FrameworkPath, Locations.LinqBridgeTargetFileName), true);
                Log.Write("LinqBridge Targets copied to {0} ", Locations.DotNet45FrameworkPath);
            }

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

            var icaclsProcess45Folder = new Process
            {
                StartInfo = { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardError = true, RedirectStandardInput = true, RedirectStandardOutput = true, FileName = "icacls.exe", Arguments = Locations.IcaclsArguments45, LoadUserProfile = true }
            };

            var icaclsProcessCommonTarget = new Process
            {
                StartInfo = { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardError = true, RedirectStandardInput = true, RedirectStandardOutput = true, FileName = "icacls.exe", Arguments = Locations.IcaclsArgumentsCommonTarget, LoadUserProfile = true }
            };
            var icaclsProcessX64CommonTarget = new Process
            {
                StartInfo = { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardError = true, RedirectStandardInput = true, RedirectStandardOutput = true, FileName = "icacls.exe", Arguments = Locations.IcaclsArgumentsX64CommonTarget, LoadUserProfile = true }
            };

            var icaclsProcess45CommonTarget = new Process
            {
                StartInfo = { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardError = true, RedirectStandardInput = true, RedirectStandardOutput = true, FileName = "icacls.exe", Arguments = Locations.IcaclsArguments45CommonTarget, LoadUserProfile = true }
            };

            #region [ Take Own ]
            var takeownProcess = Process.Start("takeown", String.Format("/f {0}", Locations.MicrosoftCommonTargetFileNamePath));
            if (takeownProcess != null) takeownProcess.WaitForExit();

            if (Environment.Is64BitOperatingSystem)
            {
                var takeownProcessx64 = Process.Start("takeown",
                    String.Format("/f {0}", Locations.MicrosoftCommonTarget64FileNamePath));

                if (takeownProcessx64 != null) takeownProcessx64.WaitForExit();
            }
            if (IsFramework45Installed)
            {
                var takeownProcess45 = Process.Start("takeown",
                 String.Format("/f \"{0}\"", Locations.DotNet45FrameworkPath));

                if (takeownProcess45 != null) takeownProcess45.WaitForExit();
            }
            #endregion


            if (IsFramework45Installed)
            {
                icaclsProcess45Folder.Start();
                icaclsProcess45CommonTarget.Start();
            }

            icaclsProcessCommonTarget.Start();

            if (Environment.Is64BitOperatingSystem)
            {
                icaclsProcessX64CommonTarget.Start();
                Log.Write("Setting Permission to {0} and {1} ", Locations.IcaclsArgumentsCommonTarget, Locations.IcaclsArgumentsX64CommonTarget);
            }

            if (IsFramework45Installed)
            {
                icaclsProcess45Folder.WaitForExit();
                icaclsProcess45CommonTarget.WaitForExit();
            }

            icaclsProcessCommonTarget.WaitForExit();

            if (Environment.Is64BitOperatingSystem)
                icaclsProcessX64CommonTarget.WaitForExit();

            if (icaclsProcessCommonTarget.ExitCode != 0)
                processOutputs.AppendLine(icaclsProcessCommonTarget.StandardOutput.ReadToEnd());

            if (IsFramework45Installed && icaclsProcess45CommonTarget.ExitCode != 0)
                processOutputs.AppendLine(icaclsProcess45CommonTarget.StandardOutput.ReadToEnd());

            if (IsFramework45Installed && icaclsProcess45Folder.ExitCode != 0)
                processOutputs.AppendLine(icaclsProcess45Folder.StandardOutput.ReadToEnd());


            if (Environment.Is64BitOperatingSystem)
            {
                if (icaclsProcessX64CommonTarget.ExitCode != 0)
                    processOutputs.AppendLine(icaclsProcessX64CommonTarget.StandardOutput.ReadToEnd());
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
