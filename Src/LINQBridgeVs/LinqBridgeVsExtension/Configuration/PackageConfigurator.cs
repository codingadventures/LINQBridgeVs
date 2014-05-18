#region License
// Copyright (c) 2013 Giovanni Campo
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using LINQBridgeVs.Logging;
using Microsoft.Win32;

namespace LINQBridgeVs.Extension.Configuration
{
    internal static class PackageConfigurator
    {
        private static string _vsVersion;
        private static string _solutionName;

        private static readonly string CurrentAssemblyVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        public static readonly bool IsFramework45Installed = Directory.Exists(Locations.DotNet45FrameworkPath);

        private const string LINQBridgeVsVersionRegistryValue = "LINQBridgeVsVersion";
        private const string LINQBridgeVsConfiguredRegistryValue = "IsLINQBridgeVsConfigured";
        private const string LINQBridgeVsEnabledRegistryValue = "IsLinqBridgeEnabled";
        private const string LINQBridgeVsInstallFolderPathRegistryValue = "InstallFolderPath";

        private static string LINQBridgeVsExtensionVersion
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(GetRegistryKeyWithVersion(Resources.ProductVersion)))
                {
                    if (key == null) return string.Empty;

                    var value = key.GetValue(LINQBridgeVsVersionRegistryValue);

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
                        key.SetValue(LINQBridgeVsVersionRegistryValue, value);
                }
            }
        }

        public static bool IsEnvironmentConfigured
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(GetRegistryKeyWithVersion(Resources.ProductVersion)))
                {
                    return key != null && Convert.ToBoolean(key.GetValue(LINQBridgeVsConfiguredRegistryValue));
                }
            }
            set
            {
                using (var key = Registry.CurrentUser.CreateSubKey(GetRegistryKeyWithVersion(Resources.ProductVersion)))
                {
                    if (key != null)
                        key.SetValue(LINQBridgeVsConfiguredRegistryValue, value);
                }
            }
        }

        public static bool IsLinqBridgeEnabled
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(GetRegistryKeyWithVersion(Resources.ProductVersion)))
                {
                    return key != null && Convert.ToBoolean(key.GetValue(LINQBridgeVsEnabledRegistryValue));
                }
            }
            set
            {
                using (var key = Registry.CurrentUser.CreateSubKey(GetRegistryKeyWithVersion(Resources.ProductVersion)))
                {
                    if (key != null)
                        key.SetValue(LINQBridgeVsEnabledRegistryValue, value);
                }
            }
        }

        public static void Configure(string vsVersion, string solutionName)
        {
            _vsVersion = vsVersion;
            _solutionName = solutionName;
            Log.Write("Configuring LINQBridgeVs Extension");
          
            try
            {
                if (LINQBridgeVsExtensionVersion != CurrentAssemblyVersion)
                {
                    Log.Write("New LINQBridgeVs Extensions. Previous Version {0}. Current Version {1}", LINQBridgeVsExtensionVersion, CurrentAssemblyVersion);
                    IsEnvironmentConfigured = false;
                    LINQBridgeVsExtensionVersion = CurrentAssemblyVersion;
                }

                //Always check if installation folder has changed
                SetInstallationFolder();

                if (IsEnvironmentConfigured) return;

                if (!IsLINQPadInstalled()) return;

                Log.Write("Setting the Environment");

                SetEnvironment();
            }
            catch (Exception e)
            {
                Log.Write(e, "Error Configuring LINQBridgeVs");
            }
        }

        private static bool IsLINQPadInstalled()
        {
            if (Directory.Exists(Locations.LinqPadDestinationFolder)) return false;

            MessageBox.Show("Please Install LINQPad in order to Use LINQBridgeVs and then Restart Visual Studio");
            Process.Start("http://www.linqpad.net");
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
            using (var key = Registry.CurrentUser.CreateSubKey(GetRegistryKeyWithVersion(Resources.ProductRegistryKey)))
            {
                if (key == null) return;

                var value = key.GetValue(LINQBridgeVsInstallFolderPathRegistryValue);
                if (value != null && value.Equals(Locations.InstallFolder)) return;
                Log.Write("Setting InstallFolderPath to {0}", Locations.InstallFolder);
                key.SetValue(LINQBridgeVsInstallFolderPathRegistryValue, Locations.InstallFolder);
            }
        }

        private static string GetRegistryKeyWithVersion(string key)
        {
            return String.Format(key, _vsVersion);
        }

        public static void EnableProject(string assemblyPath, string assemblyName)
        {
            var keyPath = string.Format(GetRegistryKeyWithVersion(Resources.EnabledProjectsRegistryKey), _solutionName);
            using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                if (key != null)
                    key.SetValue(assemblyName, new[] { "True", assemblyPath }, RegistryValueKind.MultiString);
            }
        }



        public static void DisableProject(string assemblyPath, string assemblyName)
        {
            var keyPath = string.Format(GetRegistryKeyWithVersion(Resources.EnabledProjectsRegistryKey), _solutionName);

            using (var key = Registry.CurrentUser.OpenSubKey(keyPath, true))
            {
                if (key != null) key.SetValue(assemblyName, new[] { "False", assemblyPath }, RegistryValueKind.MultiString);
            }
        }

        public static bool IsBridgeEnabled(string assemblyName)
        {
            var keyPath = string.Format(GetRegistryKeyWithVersion(Resources.EnabledProjectsRegistryKey), _solutionName);

            using (var key = Registry.CurrentUser.OpenSubKey(keyPath, false))
            {
                if (key == null) return false;
                var value = (string[])key.GetValue(assemblyName);
                return value != null && Convert.ToBoolean(value[0]);
            }
        }

        public static bool IsBridgeDisabled(string assemblyName)
        {
            return !IsBridgeEnabled(assemblyName);
        }

        private static string CreateMsBuildTargetDirectory()
        {
            var msBuildVersion = MsBuildVersion.GetMsBuildVersion(_vsVersion);
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
    }
}
