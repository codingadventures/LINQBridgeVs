#region License
// Copyright (c) 2013 - 2018 Giovanni Campo
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
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Bridge.Logging;
using Microsoft.Win32;

namespace LINQBridgeVs.Helper.Configuration
{
    public static class PackageConfigurator
    {
        private static string _runningVisualStudioVersion;

        private static readonly string CurrentAssemblyVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        public static readonly bool IsFramework45Installed = Directory.Exists(Locations.DotNet45FrameworkPath);
        
        private const string VersionRegistryValue = "LINQBridgeVsVersion";
        private const string ConfiguredRegistryValue = "IsLINQBridgeVsConfigured";
        private const string EnabledRegistryValue = "IsLinqBridgeEnabled";
        private const string InstallFolderPathRegistryValue = "InstallFolderPath";

        #region [ Private Methods ]
        private static string InstalledExtensionVersion
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(GetRegistryKey(Resources.ProductVersion, _runningVisualStudioVersion)))
                {
                    if (key == null) return string.Empty;

                    var value = key.GetValue(VersionRegistryValue);

                    if (value != null)
                        return value.ToString();
                }
                return string.Empty;
            }

            set
            {
                using (var key = Registry.CurrentUser.CreateSubKey(GetRegistryKey(Resources.ProductVersion, _runningVisualStudioVersion)))
                {
                    key?.SetValue(VersionRegistryValue, value);
                }
            }
        }
        
        private static bool IsLINQPadInstalled()
        {
            if (Directory.Exists(Locations.LinqPad4DestinationFolder))
                return true;

            if (Directory.Exists(Locations.LinqPad4DestinationFolder86))
                return true;

            MessageBox.Show("Please Install LINQPad in order to Use LINQBridgeVs and then Restart Visual Studio");
            System.Diagnostics.Process.Start("http://www.linqpad.net");
            return true;
        }

        private static void SetEnvironment()
        {
            var msBuildDir = CreateMsBuildTargetDirectory();
            //Copy the CustomAfter and CustomBefore to the default MSbuild v4.0 location
            File.Copy(Locations.CustomAfterTargetFileNamePath, Path.Combine(msBuildDir, Locations.CustomAfterTargetFileName), true);
            Log.Write("CustomAfterTargetFileName Targets copied to {0} ", Path.Combine(msBuildDir, Locations.CustomAfterTargetFileName));

            File.Copy(Locations.CustomBeforeTargetFileNamePath, Path.Combine(msBuildDir, Locations.CustomBeforeTargetFileName), true);
            Log.Write("CustomBeforeTargetFileName Targets copied to {0} ", Path.Combine(msBuildDir, Locations.CustomBeforeTargetFileName));

            Log.Write("Setting IsEnvironmentConfigured to True");
            IsEnvironmentConfigured = true;
        }

        private static void SetInstallationFolder()
        {
            //Set in the registry the installer location if it is has changed
            using (var key = Registry.CurrentUser.CreateSubKey(GetRegistryKey(Resources.ProductRegistryKey, _runningVisualStudioVersion)))
            {
                if (key == null) return;

                var value = key.GetValue(InstallFolderPathRegistryValue);
                if (value != null && value.Equals(Locations.InstallFolder)) return;
                Log.Write("Setting InstallFolderPath to {0}", Locations.InstallFolder);
                key.SetValue(InstallFolderPathRegistryValue, Locations.InstallFolder);
            }
        }

        private static string GetRegistryKey(string key, params object[] argStrings)
        {
            return String.Format(key, argStrings);
        }

        private static string CreateMsBuildTargetDirectory()
        {
            var msBuildVersion = MsBuildVersion.GetMsBuildVersion(_runningVisualStudioVersion);
            var directoryToCreate = Path.Combine(Locations.MsBuildPath, msBuildVersion);
            Log.Write("MsBuild Directory being created {0}", directoryToCreate);
            if (!Directory.Exists(directoryToCreate))
            {
                try
                {
                    var sec = new DirectorySecurity();
                    // Using this instead of the "Everyone" string means we work on non-English systems.
                    var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                    sec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                    Directory.CreateDirectory(directoryToCreate, sec);
                    return directoryToCreate;
                }
                catch (Exception exception)
                {
                    Log.Write(exception);
                    Log.Write("Error creating MSBuild Path folder in {0}", Locations.MsBuildPath);
                    throw;
                }
            }
            Log.Write("MSBuild Path {0} already exists", directoryToCreate);
            return directoryToCreate;
        }

        #endregion

        #region [ Public Methods ]

        public static bool IsEnvironmentConfigured
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(GetRegistryKey(Resources.ProductVersion, _runningVisualStudioVersion)))
                {
                    return key != null && Convert.ToBoolean(key.GetValue(ConfiguredRegistryValue));
                }
            }
            set
            {
                using (var key = Registry.CurrentUser.CreateSubKey(GetRegistryKey(Resources.ProductVersion, _runningVisualStudioVersion)))
                {
                    key?.SetValue(ConfiguredRegistryValue, value);
                }
            }
        }

        public static bool IsLinqBridgeEnabled
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(GetRegistryKey(Resources.ProductVersion, _runningVisualStudioVersion)))
                {
                    return key != null && Convert.ToBoolean(key.GetValue(EnabledRegistryValue));
                }
            }
            set
            {
                using (var key = Registry.CurrentUser.CreateSubKey(GetRegistryKey(Resources.ProductVersion, _runningVisualStudioVersion)))
                {
                    key?.SetValue(EnabledRegistryValue, value);
                }
            }
        }

        public static void Configure(string vsVersion)
        {
            _runningVisualStudioVersion = vsVersion;

            Log.Write("Configuring LINQBridgeVs Extension");

            try
            {
                if (InstalledExtensionVersion != CurrentAssemblyVersion)
                {
                    Log.Write("New LINQBridgeVs Extensions. Previous Version {0}. Current Version {1}", InstalledExtensionVersion, CurrentAssemblyVersion);
                    IsEnvironmentConfigured = false;
                    InstalledExtensionVersion = CurrentAssemblyVersion;
                }

                //Always check if installation folder has changed
                SetInstallationFolder();

                if (!IsLINQPadInstalled())
                    return;

                //if (IsEnvironmentConfigured)
                //    return;

                Log.Write("Setting the Environment");

                //TODO: add logic to set the environment again for installed newer version
                SetEnvironment();
            }
            catch (Exception e)
            {
                Log.Write(e, "Error Configuring LINQBridgeVs");
            }
        }

        public static void EnableProject(string assemblyPath, string assemblyName, string solutionName)
        {
            var keyPath = string.Format(GetRegistryKey(Resources.EnabledProjectsRegistryKey, _runningVisualStudioVersion, solutionName));
            using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                key?.SetValue($"{assemblyName}", "True" , RegistryValueKind.String);
            }
        }

        public static void DisableProject(string assemblyPath, string assemblyName, string solutionName)
        {
            var keyPath = string.Format(GetRegistryKey(Resources.EnabledProjectsRegistryKey, _runningVisualStudioVersion, solutionName));

            using (var key = Registry.CurrentUser.OpenSubKey(keyPath, true))
            {
                key?.SetValue($"{assemblyName}",  "False" , RegistryValueKind.String);
            }
        }

        public static bool IsBridgeEnabled(string assemblyName, string solutionName)
        {
            var keyPath = string.Format(GetRegistryKey(Resources.EnabledProjectsRegistryKey, _runningVisualStudioVersion, solutionName));

            using (var key = Registry.CurrentUser.OpenSubKey(keyPath, false))
            {
                if (key == null) return false;
                var value = (string[])key.GetValue(assemblyName);
                return value != null && Convert.ToBoolean(value[0]);
            }
        }

        public static bool IsBridgeDisabled(string assemblyName, string solutionName)
        {
            return !IsBridgeEnabled(assemblyName, solutionName);
        }

        #endregion

        #region [ Obsolete Methods ]

        [Obsolete("Keep them for Backward compatibility. Microsoft.Common.targets should not be modifie anymore")]
        public static void RemoveBridgeBuildTargetFromMicrosoftCommon(XDocument document, string location)
        {
            var linqBridgeTargetImportNode = GetTargetImportNode(document);

            if (linqBridgeTargetImportNode == null) return;

            linqBridgeTargetImportNode.Remove();

            document.Save(location);
        }

        [Obsolete("Keep them for Backward compatibility. Microsoft.Common.targets should not be modifie anymore")]
        private static XElement GetTargetImportNode(XDocument document)
        {
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("aw", "http://schemas.microsoft.com/developer/msbuild/2003");

            var importProjectNode =
                (IEnumerable)
                    document.XPathEvaluate("/aw:Project/aw:Import[@Project='BridgeBuildTask.targets']",
                        namespaceManager);


            var linqBridgeTargetImportNode = importProjectNode.Cast<XElement>().FirstOrDefault();

            return linqBridgeTargetImportNode;
        }

        #endregion
    }
}
