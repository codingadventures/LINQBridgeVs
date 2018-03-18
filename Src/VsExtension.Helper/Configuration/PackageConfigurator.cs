#region License
// Copyright (c) 2013 - 2018 Coding Adventures
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using BridgeVs.Locations;
using BridgeVs.Logging;
using Microsoft.Win32;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace BridgeVs.Helper.Configuration
{
    public static class PackageConfigurator
    {
        private static readonly string CurrentAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private const string VersionRegistryValue = "LINQBridgeVsVersion";
        private const string ConfiguredRegistryValue = "IsBridgeVsConfigured";
        private const string InstallFolderPathRegistryValue = "InstallFolderPath";
        private const string LinqPad5 = "LINQPad 5";
        private const string LinqPad4 = "LINQPad 4";

        #region [ Private Methods ]

        private static string InstalledExtensionVersion(string vsVersion)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(GetRegistryKey(Resources.ProductVersion, vsVersion)))
            {
                if (key == null) return string.Empty;

                object value = key.GetValue(VersionRegistryValue);

                if (value != null)
                    return value.ToString();
            }
            return string.Empty;

        }

        private static bool IsLINQPadInstalled()
        {
            if (Directory.Exists(CommonFolderPaths.LinqPad5DestinationFolder))
            {
                CommonRegistryConfigurations.LINQPadInstallationPath = CommonFolderPaths.LinqPad5DestinationFolder;
                CommonRegistryConfigurations.LINQPadVersion = LinqPad5;
                return true;
            }
            if (Directory.Exists(CommonFolderPaths.LinqPad4DestinationFolder))
            {
                CommonRegistryConfigurations.LINQPadInstallationPath = CommonFolderPaths.LinqPad4DestinationFolder;
                CommonRegistryConfigurations.LINQPadVersion = LinqPad4;
                return true;
            }

            DialogResult result = MessageBox.Show("Please Install LINQPad and then Restart Visual Studio or provide a folder", "LINQPad Not Found", MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                OpenFileDialog dialog = new OpenFileDialog()
                {
                    Multiselect = false,
                    Filter = "LINQPad 4, 5|*.exe",
                    InitialDirectory = CommonFolderPaths.ProgramFilesFolderPath
                };
                DialogResult dialogResult = dialog.ShowDialog();
                bool isLinqPadFound = dialogResult == DialogResult.OK && dialog.FileName.Contains("LINQPad.exe");
                if (!isLinqPadFound)
                {
                    //how much do I love prohibitive things...
                    goto openWebSite;
                }

                string linqPadDirectoryName = Path.GetDirectoryName(dialog.FileName);
                if (string.IsNullOrEmpty(linqPadDirectoryName))
                    throw new Exception("LINQPad file name not correct");

                CommonRegistryConfigurations.LINQPadInstallationPath = linqPadDirectoryName;
                CommonRegistryConfigurations.LINQPadVersion = linqPadDirectoryName.Contains("LINQPad 5") ? LinqPad5 : LinqPad4;

                return true;
            }
            openWebSite:
            System.Diagnostics.Process.Start("http://www.linqpad.net");
            return false;
        }

        private static void SetEnvironment(string vsVersion, string vsEdition)
        {
            string msBuildDir = CreateMsBuildTargetDirectory(vsVersion, vsEdition);
            //Copy the CustomAfter and CustomBefore to the default MSbuild v4.0 location
            File.Copy(CommonFolderPaths.CustomAfterTargetFileNamePath, Path.Combine(msBuildDir, CommonFolderPaths.CustomAfterTargetFileName), true);
            Log.Write("CustomAfterTargetFileName Targets copied to {0} ", Path.Combine(msBuildDir, CommonFolderPaths.CustomAfterTargetFileName));

            File.Copy(CommonFolderPaths.CustomBeforeTargetFileNamePath, Path.Combine(msBuildDir, CommonFolderPaths.CustomBeforeTargetFileName), true);
            Log.Write("CustomBeforeTargetFileName Targets copied to {0} ", Path.Combine(msBuildDir, CommonFolderPaths.CustomBeforeTargetFileName));

            Log.Write("Setting IsEnvironmentConfigured to True");
        }

        private static void SetInstallationFolder(string vsVersion)
        {
            //Set in the registry the installer location if it is has changed
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(GetRegistryKey(Resources.ProductRegistryKey, vsVersion)))
            {
                if (key == null) return;

                object value = key.GetValue(InstallFolderPathRegistryValue);
                if (value != null && value.Equals(CommonFolderPaths.InstallFolder)) return;
                Log.Write("Setting InstallFolderPath to {0}", CommonFolderPaths.InstallFolder);
                key.SetValue(InstallFolderPathRegistryValue, CommonFolderPaths.InstallFolder);
            }
        }

        public static string GetRegistryKey(string key, params object[] argStrings)
        {
            return string.Format(key, argStrings);
        }

        private static string CreateMsBuildTargetDirectory(string vsVersion, string vsEdition)
        {
            string msBuildVersion = MsBuildVersionHelper.GetMsBuildVersion(vsVersion);
            //if it's v15 it's Visual studio 2017
            string directoryToCreate = Path.Combine(msBuildVersion.Equals("v15.0")
                ? string.Format(CommonFolderPaths.MsBuildPath2017, vsEdition)
                : CommonFolderPaths.MsBuildPath, msBuildVersion);

            Log.Write("MsBuild Directory being created {0}", directoryToCreate);
            if (!Directory.Exists(directoryToCreate))
            {
                try
                {
                    DirectorySecurity sec = new DirectorySecurity();
                    // Using this instead of the "Everyone" string means we work on non-English systems.
                    SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                    sec.AddAccessRule(new FileSystemAccessRule(everyone,
                        FileSystemRights.Modify | FileSystemRights.Synchronize,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None,
                        AccessControlType.Allow));
                    Directory.CreateDirectory(directoryToCreate, sec);
                    return directoryToCreate;
                }
                catch (UnauthorizedAccessException uae)
                {
                    Log.Write(uae);
                    MessageBox.Show(
                        "It wasn't possible to complete the configuration of BridgeVs. Please restart Visual Studio as Administrator");
                    throw;
                }
                catch (Exception exception)
                {
                    Log.Write(exception);
                    Log.Write("Error creating MSBuild Path folder in {0}", CommonFolderPaths.MsBuildPath);
                    throw;
                }
            }
            Log.Write("MSBuild Path {0} already exists", directoryToCreate);
            return directoryToCreate;
        }

        #endregion

        #region [ Public Methods ]

        public static bool IsBridgeVsConfigured(string visualStudioVersion)
        {
            if (InstalledExtensionVersion(visualStudioVersion) != CurrentAssemblyVersion)
            {
                Log.Write("New LINQBridgeVs Extensions. Previous Version {0}. Current Version {1}", InstalledExtensionVersion(visualStudioVersion), CurrentAssemblyVersion);
                return false;
            }

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(GetRegistryKey(Resources.ProductVersion, visualStudioVersion)))
            {
                return key != null && Convert.ToBoolean(key.GetValue(ConfiguredRegistryValue));
            }
        }
        private static void MarkBridgeVsAsInstalled(string vsVersion)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(GetRegistryKey(Resources.ProductVersion, vsVersion)))
            {
                key?.SetValue(ConfiguredRegistryValue, true);
            }
        }
        private static void SetBridgeVsAssemblyVersion(string vsVersion)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(GetRegistryKey(Resources.ProductVersion, vsVersion)))
            {
                key?.SetValue(VersionRegistryValue, CurrentAssemblyVersion);
            }
        }
        public static bool Install(string vsVersion, string vsEdition)
        {
            Log.Write("Configuring LINQBridgeVs Extension");

            try
            {
                if (!IsLINQPadInstalled()) //ask the user to insert a custom location
                {
                    return false;
                }

                ObsoleteXmlConfiguration.RemoveOldTargets();

                SetBridgeVsAssemblyVersion(vsVersion);

                CreateLinqPadQueryFolder();

                //Always check if installation folder has changed
                SetInstallationFolder(vsVersion);

                Log.Write("Setting the Environment");

                SetEnvironment(vsVersion, vsEdition);

                MarkBridgeVsAsInstalled(vsVersion);

                return true;
            }
            catch (Exception e)
            {
                Log.Write(e, "Error Configuring LINQBridgeVs");
                return false;
            }
        }

        private static void CreateLinqPadQueryFolder()
        {
            string dstScriptPath = Path.Combine(CommonFolderPaths.LinqPadQueryFolder, "BridgeVs");

            if (Directory.Exists(dstScriptPath)) return;

            DirectorySecurity sec = new DirectorySecurity();
            // Using this instead of the "Everyone" string means we work on non-English systems.
            SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            sec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            Directory.CreateDirectory(dstScriptPath, sec);

            Log.Write($"Directory Created: {dstScriptPath}");
        }

        #endregion
    }
}
