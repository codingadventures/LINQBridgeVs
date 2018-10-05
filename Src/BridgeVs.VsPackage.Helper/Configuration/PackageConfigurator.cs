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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using BridgeVs.Shared.Common;
using Microsoft.Win32;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace BridgeVs.VsPackage.Helper.Configuration
{
    public static class PackageConfigurator
    {
        private static readonly string CurrentAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private const string VersionRegistryValue = "LINQBridgeVsVersion";
        private const string InstallFolderPathRegistryValue = "InstallFolderPath";
        private const string LinqPad5 = "LINQPad 5";
        private const string LinqPad4 = "LINQPad 4";

        public static List<string> Dependencies => new List<string>
        {
            "BridgeVs.Grapple.dll",
            "BridgeVs.Shared.dll",
            "Newtonsoft.Json.dll",
            "System.IO.Abstractions.dll",
            "SharpRaven.dll"
        };

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

        // ReSharper disable once InconsistentNaming
        private static bool IsLINQPadInstalled(string vsVersion)
        {
            var currentInstalledVersionPath = CommonRegistryConfigurations.GetLINQPadInstallationPath(vsVersion);
            var alreadyInstalled = !string.IsNullOrEmpty(currentInstalledVersionPath);

            if (alreadyInstalled && Directory.Exists(currentInstalledVersionPath))
                return true;

            //otherwise set it up manually
            if (Directory.Exists(CommonFolderPaths.LinqPad5DestinationFolder))
            {
                CommonRegistryConfigurations.SetLINQPadInstallationPath(vsVersion, CommonFolderPaths.LinqPad5DestinationFolder);
                return true;
            }
            if (Directory.Exists(CommonFolderPaths.LinqPad4DestinationFolder))
            {
                CommonRegistryConfigurations.SetLINQPadInstallationPath(vsVersion, CommonFolderPaths.LinqPad4DestinationFolder);
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

                CommonRegistryConfigurations.SetLINQPadInstallationPath(vsVersion, linqPadDirectoryName);

                return true;
            }
            openWebSite:
            System.Diagnostics.Process.Start("http://www.linqpad.net");
            return false;
        }

        private static void DeployMsBuildTargets(string vsVersion, string vsEdition)
        {
            string msBuildDir = CreateMsBuildTargetDirectory(vsVersion, vsEdition);
            //Copy the CustomAfter and CustomBefore to the default MSbuild v4.0 location
            File.Copy(CommonFolderPaths.CustomAfterTargetFileNamePath, Path.Combine(msBuildDir, CommonFolderPaths.CustomAfterTargetFileName), true);

            File.Copy(CommonFolderPaths.CustomBeforeTargetFileNamePath, Path.Combine(msBuildDir, CommonFolderPaths.CustomBeforeTargetFileName), true);
        }

        private static void SetInstallationFolder(string vsVersion)
        {
            //Set in the registry the installer location if it is has changed
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(GetRegistryKey(Resources.ProductRegistryKey, vsVersion)))
            {
                if (key == null) return;

                object value = key.GetValue(InstallFolderPathRegistryValue);

                if (value != null && value.Equals(CommonFolderPaths.InstallFolder))
                    return;

                key.SetValue(InstallFolderPathRegistryValue, CommonFolderPaths.InstallFolder);
            }
        }

        private static string GetInstallationFolder(string vsVersion)
        {
            //Set in the registry the installer location if it is has changed
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(GetRegistryKey(Resources.ProductRegistryKey, vsVersion)))
            {
                object value = key?.GetValue(InstallFolderPathRegistryValue);
                return value?.ToString();
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
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(
                        "It wasn't possible to complete the configuration of BridgeVs. Please restart Visual Studio as Administrator");
                    throw;
                }
            }
            return directoryToCreate;
        }

        #endregion

        #region [ Public Methods ]

        public static bool IsBridgeVsConfigured(string visualStudioVersion)
        {
            if (InstalledExtensionVersion(visualStudioVersion) != CurrentAssemblyVersion)
            {
                return false;
            }

            string installationFolder = GetInstallationFolder(visualStudioVersion);

            return !string.IsNullOrEmpty(installationFolder) && installationFolder == CommonFolderPaths.InstallFolder;
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
            if (!IsLINQPadInstalled(vsVersion)) //ask the user to insert a custom location
            {
                return false;
            }

            SetBridgeVsAssemblyVersion(vsVersion);

            CreateLinqPadQueryFolder();

            CreateLinqPadPluginFolders();

            CreateVisualizerFolder(vsVersion);

            CreateGrappleFolder();

            CreateLogFolder();

            //Always check if installation folder has changed
            SetInstallationFolder(vsVersion);

            DeployMsBuildTargets(vsVersion, vsEdition);

            GenerateGuidForCurrentInstallation(vsVersion);

            DeleteExistingVisualizers(vsVersion);

            DeployDependencies(vsVersion);

            return true;
        }

        private static void GenerateGuidForCurrentInstallation(string vsVersion)
        {
            CommonRegistryConfigurations.SetUniqueGuid(vsVersion, Guid.NewGuid().ToString());
        }

        public static void DeployDependencies(string vsVersion)
        {
            string currentLocation = Path.GetDirectoryName(typeof(PackageConfigurator).Assembly.Location);
            if (string.IsNullOrEmpty(currentLocation))
                throw new Exception("Dll location is null");

            string debuggerVisualizerTargetFolder = DebuggerVisualizerTargetFolder(vsVersion);
            foreach (string dependency in Dependencies)
            {
                string sourceFile = Path.Combine(currentLocation, dependency);
                string targetFile = Path.Combine(debuggerVisualizerTargetFolder, dependency);

                if (!File.Exists(sourceFile))
                    continue; //could be an old dll

                File.Copy(sourceFile, targetFile, true);

                //it copies the plugins to all the folders found in either custom locations and default
                foreach (string targetPluginFolder in CommonFolderPaths.AllLinqPadPluginFolders)
                {
                    string targetFileLinqPadPluginFolder = Path.Combine(targetPluginFolder, dependency);
                    File.Copy(sourceFile, targetFileLinqPadPluginFolder, true);
                }
            }
        }

        private static void DeleteExistingVisualizers(string vsVersion)
        {
            string debuggerVisualizerTargetFolder = DebuggerVisualizerTargetFolder(vsVersion);
            string versionNumber = vsVersion.Split('.')[0];

            IEnumerable<string> visualizers = from file in Directory.EnumerateFiles(debuggerVisualizerTargetFolder)
                                              where !string.IsNullOrEmpty(file)
                                              let extension = Path.GetExtension(file)
                                              let fileName = Path.GetFileName(file)
                                              where extension.Equals(".dll") || extension.Equals(".pdb")
                                              where Dependencies.Contains(fileName) || fileName.Contains($"V{vsVersion}") || fileName.Contains($"V{versionNumber}")
                                              select file;

            foreach (string visualizer in visualizers)
            {
                if (File.Exists(visualizer))
                    File.Delete(visualizer);
            }
        }

        private static string DebuggerVisualizerTargetFolder(string vsVersion)
        {
            string debuggerVisualizerTargetFolder = string.Empty;
            switch (vsVersion)
            {
                case "11.0":
                    debuggerVisualizerTargetFolder = CommonFolderPaths.Vs2012DebuggerVisualizerDestinationFolder;
                    break;
                case "12.0":
                    debuggerVisualizerTargetFolder = CommonFolderPaths.Vs2013DebuggerVisualizerDestinationFolder;
                    break;
                case "14.0":
                    debuggerVisualizerTargetFolder = CommonFolderPaths.Vs2015DebuggerVisualizerDestinationFolder;
                    break;
                case "15.0":
                    debuggerVisualizerTargetFolder = CommonFolderPaths.Vs2017DebuggerVisualizerDestinationFolder;
                    break;
            }

            return debuggerVisualizerTargetFolder;
        }

        private static void CreateLinqPadQueryFolder()
        {
            //check first where LINQPad is installed
            string dstScriptPath = CommonFolderPaths.DefaultLinqPadQueryFolder;

            if (Directory.Exists(dstScriptPath))
                return;

            DirectorySecurity sec = new DirectorySecurity();
            // Using this instead of the "Everyone" string means we work on non-English systems.
            SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            sec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            Directory.CreateDirectory(dstScriptPath, sec);
        }

        private static void CreateLinqPadPluginFolders()
        {
            foreach (string pluginFolder in CommonFolderPaths.AllLinqPadPluginFolders)
            {
                if (Directory.Exists(pluginFolder))
                    continue;

                DirectorySecurity sec = new DirectorySecurity();
                // Using this instead of the "Everyone" string means we work on non-English systems.
                SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                sec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                Directory.CreateDirectory(pluginFolder, sec);
            }
        }

        private static void CreateVisualizerFolder(string vsVersion)
        {
            string debuggerVisualizerTargetFolder = DebuggerVisualizerTargetFolder(vsVersion);
            if (Directory.Exists(debuggerVisualizerTargetFolder)) return;

            DirectorySecurity sec = new DirectorySecurity();
            // Using this instead of the "Everyone" string means we work on non-English systems.
            SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            sec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            Directory.CreateDirectory(debuggerVisualizerTargetFolder, sec);
        }

        private static void CreateGrappleFolder()
        {
            if (Directory.Exists(CommonFolderPaths.GrappleFolder))
                return;

            //no need for security access
            Directory.CreateDirectory(CommonFolderPaths.GrappleFolder);
        }

        private static void CreateLogFolder()
        {
            if (!Directory.Exists(CommonFolderPaths.LogFolderPath))
            {
                try
                {
                    DirectorySecurity sec = new DirectorySecurity();
                    // Using this instead of the "Everyone" string means we work on non-English systems.
                    SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                    sec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                    Directory.CreateDirectory(CommonFolderPaths.LogFolderPath, sec);
                }
                catch
                {
                    return;
                }
            }
        }
        #endregion
    }
}