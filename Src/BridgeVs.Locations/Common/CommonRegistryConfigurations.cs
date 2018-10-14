using BridgeVs.Shared.Options;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BridgeVs.Shared.Common
{
    public class CommonRegistryConfigurations
    {
        private const string ErrorTrackingRegistryValue = "SentryErrorTracking";
        private const string Map3RdPartyAssemblyRegistryValue = "Map3rdPartyAssembly";
        private const string SerializationMethodRegistryValue = "SerializationMethod";
        private const string EnabledProjectsRegistryKey = @"Software\LINQBridgeVs\{0}\Solutions\{1}";
        private static readonly string ProjectReferencesRegistryKey = $@"{EnabledProjectsRegistryKey}\References";

        public const string LoggingRegistryValue = "Logging";

        // ReSharper disable once InconsistentNaming
        private const string LINQPadInstallationPathRegistryValue = "LINQPadInstallationPath";
        // ReSharper disable once InconsistentNaming

        public const string InstallationGuidRegistryValue = "UniqueId";

        public static string GetRegistryKey(string key, params object[] argStrings)
        {
            return string.Format(key, argStrings);
        }

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

        public static void EnableProject(string assemblyPath, string assemblyName, string solutionName, string vsVersion)
        {
            string keyPath = string.Format(GetRegistryKey(EnabledProjectsRegistryKey, vsVersion, solutionName));
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                key?.SetValue($"{assemblyName}", "True", RegistryValueKind.String);
                key?.SetValue($"{assemblyName}_location", Path.GetFullPath(assemblyPath), RegistryValueKind.String);
            }
        }

        public static void DisableProject(string assemblyName, string solutionName, string vsVersion)
        {
            string keyPath = string.Format(GetRegistryKey(EnabledProjectsRegistryKey, vsVersion, solutionName));

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, true))
            {
                key?.DeleteValue(assemblyName, false);
                key?.DeleteValue($"{assemblyName}_location", false);
            }
        }

        public static bool IsSolutionEnabled(string solutionName, string vsVersion)
        {
            string keyPath = string.Format(GetRegistryKey(EnabledProjectsRegistryKey, vsVersion, solutionName));

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, false))
            {
                if (key == null) return false;
                string value = (string)key.GetValue("SolutionEnabled");
                return value != null && Convert.ToBoolean(value);
            }
        }

        public static void EnableSolution(string solutionName, string vsVersion, bool enable)
        {
            string keyPath = string.Format(GetRegistryKey(EnabledProjectsRegistryKey, vsVersion, solutionName));

            //now create a general solution flag to mark the current solution as activated
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, true))
            {
                if (!enable)
                {
                    //to invalidate the cache I delete the value
                    key?.DeleteValue("SolutionEnabled", false);
                }
                else
                {
                    key?.SetValue("SolutionEnabled", "True", RegistryValueKind.String);
                }
            }

        }

        public static bool IsErrorTrackingEnabled(string vsVersion)
        {
            bool isTrackingEnabled = false;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey($@"Software\LINQBridgeVs\{vsVersion}"))
            {
                string errorTracking = key?.GetValue(ErrorTrackingRegistryValue) as string;
                if (!string.IsNullOrEmpty(errorTracking))
                    isTrackingEnabled = Convert.ToBoolean(errorTracking);
            }

            return isTrackingEnabled;
        }

        public static bool Map3RdPartyAssembly(string solutionName, string vsVersion)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey($@"Software\LINQBridgeVs\{vsVersion}"))
            {
                if (key?.GetValue(Map3RdPartyAssemblyRegistryValue) is string map3RdPartyAssembly)
                {
                    if (!string.IsNullOrEmpty(map3RdPartyAssembly))
                    {
                        return Convert.ToBoolean(map3RdPartyAssembly);
                    }
                }
            }

            return false;
        }

        public static void SetMap3RdPartyAssembly(string vsVersion, bool value)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"Software\LINQBridgeVs\{vsVersion}"))
            {
                key?.SetValue(Map3RdPartyAssemblyRegistryValue, value);
            }
        }

        public static void SetErrorTracking(string vsVersion, bool enabled)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"Software\LINQBridgeVs\{vsVersion}"))
            {
                key?.SetValue(ErrorTrackingRegistryValue, enabled);
            }
        }

        /// <summary>
        /// Gets the assembly location. If an assembly is loaded at Runtime or it's loaded within IIS context, Assembly.Location property could be null
        /// This method reads the original location of the assembly that was Bridged
        /// </summary>
        /// <param name="type">The Type.</param>
        /// <param name="vsVersion">The Visual Studio version.</param>
        /// <returns></returns>
        public static string GetOriginalAssemblyLocation(Type @type, string vsVersion)
        {
            bool IsSystemAssembly(string name) => name.Contains("Microsoft") || name.Contains("System") || name.Contains("mscorlib");

            string registryKeyPath = $@"Software\LINQBridgeVs\{vsVersion}\Solutions";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKeyPath))
            {
                if (key == null)
                    return string.Empty;

                string[] values = key.GetSubKeyNames();
                RegistryKey registryKey = key;

                foreach (string value in values)
                {
                    RegistryKey subKey = registryKey.OpenSubKey(value);

                    string name = subKey?.GetValueNames().FirstOrDefault(p =>
                    {
                        if (!type.IsGenericType)
                            return p == type.Assembly.GetName().Name;
                        Type genericType = type.GetGenericArguments()[0];

                        if (IsSystemAssembly(genericType.Assembly.GetName().Name))
                            return false;

                        return p == genericType.Assembly.GetName().Name;
                    });

                    if (string.IsNullOrEmpty(name)) continue;

                    string assemblyLoc = (string)subKey.GetValue(name + "_location");

                    return assemblyLoc;
                }
            }
            return string.Empty;
        }

        public static void SetSerializationMethod(string vsVersion, string value)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"Software\LINQBridgeVs\{vsVersion}"))
            {
                key?.SetValue(SerializationMethodRegistryValue, value);
            }
        }

        public static SerializationOption GetSerializationOption(string vsVersion)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey($@"Software\LINQBridgeVs\{vsVersion}"))
            {
                string serializationOption = key?.GetValue(SerializationMethodRegistryValue) as string;
                if (!string.IsNullOrEmpty(serializationOption))
                {
                    return (SerializationOption)Enum.Parse(typeof(SerializationOption), serializationOption);
                }

                return SerializationOption.BinarySerializer;
            }
        }

        public static bool IsLoggingEnabled(string vsVersion)
        {
            bool isLoggingEnabled = false;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey($@"Software\LINQBridgeVs\{vsVersion}"))
            {
                string logging = key?.GetValue(LoggingRegistryValue) as string;
                if (!string.IsNullOrEmpty(logging))
                    isLoggingEnabled = Convert.ToBoolean(logging);
            }

            return isLoggingEnabled;
        }

        public static void SetLogging(string vsVersion, bool enabled)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"Software\LINQBridgeVs\{vsVersion}"))
            {
                key?.SetValue(LoggingRegistryValue, enabled);
            }
        }

        public static string GetUniqueGuid(string vsVersion)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey($@"Software\LINQBridgeVs\{vsVersion}"))
            {
                return key?.GetValue(InstallationGuidRegistryValue) as string;

            }
        }

        public static void SetUniqueGuid(string vsVersion, string guid)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"Software\LINQBridgeVs\{vsVersion}"))
            {
                key?.SetValue(InstallationGuidRegistryValue, guid);
            }
        }

        public static void StoreProjectReferences(string executeParamsAssemblyName, string solutionName, string vsVersion, List<string> executeParamsReferences)
        {
            string keyPath = string.Format(GetRegistryKey(ProjectReferencesRegistryKey, vsVersion, solutionName));
            Registry.CurrentUser.DeleteSubKeyTree(keyPath); //clean first
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                key?.SetValue($"{assemblyName}", "True", RegistryValueKind.String);
                key?.SetValue($"{assemblyName}_location", Path.GetFullPath(assemblyPath), RegistryValueKind.String);
            }
        }
    }
}
