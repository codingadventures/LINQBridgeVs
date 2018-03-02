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
using BridgeVs.Logging;
using BridgeVs.TypeMapper;
using BridgeVs.VisualStudio;
using Microsoft.Win32;

namespace BridgeVs.Helper.Configuration
{
    public static class PackageConfigurator
    {
        private static string _runningVisualStudioVersion;
        private static string _vsEdition;
        private static readonly string CurrentAssemblyVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        public static readonly bool IsFramework45Installed = Directory.Exists(Locations.DotNet45FrameworkPath);

        private const string VersionRegistryValue = "LINQBridgeVsVersion";
        private const string LINQPadInstallationPathRegistryValue = "LINQPadInstallationPath";
        private const string ConfiguredRegistryValue = "IsLINQBridgeVsConfigured";
        private const string IsFTUERegistryValue = "IsFTUE";
        private const string InstallFolderPathRegistryValue = "InstallFolderPath";

        #region [ Obsolete ]
        private static XDocument _microsoftCommonTargetDocument;
        public static XDocument MicrosoftCommonTargetDocument
        {
            get
            {
                _microsoftCommonTargetDocument = _microsoftCommonTargetDocument ?? XDocument.Load(Locations.MicrosoftCommonTargetFileNamePath);
                return _microsoftCommonTargetDocument;
            }

        }

        private static XDocument _microsoftCommonTargetX64Document;
        public static XDocument MicrosoftCommonTargetX64Document
        {
            get
            {
                _microsoftCommonTargetX64Document = _microsoftCommonTargetX64Document ?? XDocument.Load(Locations.MicrosoftCommonTargetX64FileNamePath);
                return _microsoftCommonTargetX64Document;
            }

        }

        private static XDocument _microsoftCommonTarget45Document;
        public static XDocument MicrosoftCommonTarget45Document
        {
            get
            {
                _microsoftCommonTarget45Document = _microsoftCommonTarget45Document ?? XDocument.Load(Locations.MicrosoftCommonTarget45FileNamePath);
                return _microsoftCommonTarget45Document;
            }
        }
        #endregion

        #region [ Private Methods ]

        private static string LINQPadInstallationPath
        {
            set
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(Resources.LINQPadInstallationPath))
                {
                    key?.SetValue(LINQPadInstallationPathRegistryValue, value);
                }
            }
        }

        private static string InstalledExtensionVersion
        {
            get
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(GetRegistryKey(Resources.ProductVersion, _runningVisualStudioVersion)))
                {
                    if (key == null) return string.Empty;

                    object value = key.GetValue(VersionRegistryValue);

                    if (value != null)
                        return value.ToString();
                }
                return string.Empty;
            }

            set
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(GetRegistryKey(Resources.ProductVersion, _runningVisualStudioVersion)))
                {
                    key?.SetValue(VersionRegistryValue, value);
                }
            }
        }

        private static bool IsLINQPadInstalled()
        {
            if (Directory.Exists(Locations.LinqPad5DestinationFolder))
            {
                LINQPadInstallationPath = Locations.LinqPad4DestinationFolder;
                return true;
            }
            if (Directory.Exists(Locations.LinqPad4DestinationFolder))
            {
                LINQPadInstallationPath = Locations.LinqPad4DestinationFolder;
                return true;
            }

            var result = MessageBox.Show("Please Install LINQPad and then Restart Visual Studio or provide a folder", "LINQPad Not Found", MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                var dialog = new System.Windows.Forms.OpenFileDialog()
                {
                    Multiselect = false,
                    Filter = "LINQPad 4, 5|*.exe",
                    InitialDirectory = Locations.ProgramFilesFolderPath
                };
                var dialogResult = dialog.ShowDialog();
                bool isLinqPadFound = dialogResult == DialogResult.OK && dialog.FileName.Contains("LINQPad.exe");
                if (isLinqPadFound)
                {
                    LINQPadInstallationPath = Path.GetDirectoryName(dialog.FileName);
                    return true;
                }
                return false;
            }
            else
            {
                System.Diagnostics.Process.Start("http://www.linqpad.net");
                return false;
            }
        }

        private static void SetEnvironment()
        {
            string msBuildDir = CreateMsBuildTargetDirectory();
            //Copy the CustomAfter and CustomBefore to the default MSbuild v4.0 location
            File.Copy(Locations.CustomAfterTargetFileNamePath, Path.Combine(msBuildDir, Locations.CustomAfterTargetFileName), true);
            Log.Write("CustomAfterTargetFileName Targets copied to {0} ", Path.Combine(msBuildDir, Locations.CustomAfterTargetFileName));

            File.Copy(Locations.CustomBeforeTargetFileNamePath, Path.Combine(msBuildDir, Locations.CustomBeforeTargetFileName), true);
            Log.Write("CustomBeforeTargetFileName Targets copied to {0} ", Path.Combine(msBuildDir, Locations.CustomBeforeTargetFileName));

            Log.Write("Setting IsEnvironmentConfigured to True");
            //    IsEnvironmentConfigured = true;
        }

        private static void SetInstallationFolder()
        {
            //Set in the registry the installer location if it is has changed
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(GetRegistryKey(Resources.ProductRegistryKey, _runningVisualStudioVersion)))
            {
                if (key == null) return;

                object value = key.GetValue(InstallFolderPathRegistryValue);
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
            string msBuildVersion = MsBuildVersionHelper.GetMsBuildVersion(_runningVisualStudioVersion);
            string directoryToCreate = string.Empty;
            //if it's v15 it's Visual studio 2017
            if (msBuildVersion.Equals("V15.0"))
            {
                directoryToCreate = Path.Combine(string.Format(Locations.MsBuildPath2017, _vsEdition), msBuildVersion);
            }
            else
                directoryToCreate = Path.Combine(Locations.MsBuildPath, msBuildVersion);

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
                        "It hasn't been possible to complete the configuration of BridgeVs. Please restart Visual Studio as Administrator");
                    throw;
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

        public static bool IsLINQBridgeVsConfigured
        {
            get
            {
                if (InstalledExtensionVersion != CurrentAssemblyVersion)
                {
                    Log.Write("New LINQBridgeVs Extensions. Previous Version {0}. Current Version {1}", InstalledExtensionVersion, CurrentAssemblyVersion);
                    return false;
                }
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(GetRegistryKey(Resources.ProductVersion, _runningVisualStudioVersion)))
                {
                    return key != null && Convert.ToBoolean(key.GetValue(IsFTUERegistryValue));
                }
            }
            set
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(GetRegistryKey(Resources.ProductVersion, _runningVisualStudioVersion)))
                {
                    key?.SetValue(IsFTUERegistryValue, value);
                }
            }
        }
        private static void RemoveOldTargets()
        {
            PackageConfigurator.RemoveBridgeBuildTargetFromMicrosoftCommon(MicrosoftCommonTargetDocument, Locations.MicrosoftCommonTargetFileNamePath);
            Log.Write("BridgeBuild.targets Removed for x86 Operating System");

            if (System.Environment.Is64BitOperatingSystem)
            {
                PackageConfigurator.RemoveBridgeBuildTargetFromMicrosoftCommon(MicrosoftCommonTargetX64Document, Locations.MicrosoftCommonTargetX64FileNamePath);
                Log.Write("BridgeBuild.targets Removed for x64 Operating System");

            }
            if (IsFramework45Installed)
            {
                PackageConfigurator.RemoveBridgeBuildTargetFromMicrosoftCommon(MicrosoftCommonTarget45Document, Locations.MicrosoftCommonTarget45FileNamePath);
                Log.Write("BridgeBuild.targets Removed for framework 4.5");
            }
        }

        public static bool Install(string vsVersion, string vsEdition)
        {

            _runningVisualStudioVersion = vsVersion;
            _vsEdition = vsEdition;

            Log.Write("Configuring LINQBridgeVs Extension");

            try
            {
                if (!IsLINQPadInstalled()) //ask the user to insert a custom location
                {
                    return false;
                }
                RemoveOldTargets();

                InstalledExtensionVersion = CurrentAssemblyVersion;

                //Always check if installation folder has changed
                SetInstallationFolder();

                Log.Write("Setting the Environment");

                //TODO: add logic to set the environment again for installed newer version
                SetEnvironment();

                IsLINQBridgeVsConfigured = true;

                return true;
            }
            catch (Exception e)
            {
                Log.Write(e, "Error Configuring LINQBridgeVs");
                return false;
            }
        }

        public static void EnableProject(string assemblyPath, string assemblyName, string solutionName)
        {
            string keyPath = string.Format(GetRegistryKey(Resources.EnabledProjectsRegistryKey, _runningVisualStudioVersion, solutionName));
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                key?.SetValue($"{assemblyName}", "True", RegistryValueKind.String);
                key?.SetValue($"{assemblyName}_location", Path.GetFullPath(assemblyPath), RegistryValueKind.String);
            }
        }

        public static void DisableProject(string assemblyPath, string assemblyName, string solutionName)
        {
            string keyPath = string.Format(GetRegistryKey(Resources.EnabledProjectsRegistryKey, _runningVisualStudioVersion, solutionName));

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, true))
            {
                key?.DeleteValue(assemblyName);
                key?.DeleteValue($"{assemblyName}_location");
            }
        }

        public static bool IsBridgeEnabled(string assemblyName, string solutionName)
        {
            string keyPath = string.Format(GetRegistryKey(Resources.EnabledProjectsRegistryKey, _runningVisualStudioVersion, solutionName));

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, false))
            {
                if (key == null) return false;
                string value = (string)key.GetValue(assemblyName);
                return value != null && Convert.ToBoolean(value);
            }
        }

        public static bool IsBridgeDisabled(string assemblyName, string solutionName)
        {
            return !IsBridgeEnabled(assemblyName, solutionName);
        }

        private static void CreateDirWithPermission(string folder)
        {
            var sec = new DirectorySecurity();
            // Using this instead of the "Everyone" string means we work on non-English systems.
            var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var rule = new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None,
                AccessControlType.Allow);

            if (Directory.Exists(folder))
            {
                var di = new DirectoryInfo(folder);
                var security = di.GetAccessControl();
                security.AddAccessRule(rule);
                security.SetAccessRule(rule);
                di.SetAccessControl(security);
                return;
            }
            sec.AddAccessRule(rule);
            Directory.CreateDirectory(folder, sec);
        }
        #endregion

        #region [ Obsolete Methods ]

        [Obsolete("Keep them for Backward compatibility. Microsoft.Common.targets should not be modifie anymore")]
        public static void RemoveBridgeBuildTargetFromMicrosoftCommon(XDocument document, string location)
        {
            XElement linqBridgeTargetImportNode = GetTargetImportNode(document);

            if (linqBridgeTargetImportNode == null) return;

            linqBridgeTargetImportNode.Remove();

            document.Save(location);
        }

        [Obsolete("Keep them for Backward compatibility. Microsoft.Common.targets should not be modifie anymore")]
        private static XElement GetTargetImportNode(XDocument document)
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("aw", "http://schemas.microsoft.com/developer/msbuild/2003");

            IEnumerable importProjectNode =
                (IEnumerable)
                    document.XPathEvaluate("/aw:Project/aw:Import[@Project='BridgeBuildTask.targets']",
                        namespaceManager);


            XElement linqBridgeTargetImportNode = importProjectNode.Cast<XElement>().FirstOrDefault();

            return linqBridgeTargetImportNode;
        }

        #endregion
    }
}
