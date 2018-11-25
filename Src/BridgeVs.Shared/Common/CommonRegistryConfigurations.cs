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
        private static readonly string ProjectReferencesRegistryKey = $@"{EnabledProjectsRegistryKey}\{{2}}";

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

        private static void StoreProjectAssemblyPath(string assemblyPath, string assemblyName, string solutionName, string vsVersion)
        {
            string keyPath = string.Format(GetRegistryKey(EnabledProjectsRegistryKey, vsVersion, solutionName));
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                key?.SetValue($"{assemblyName}", Path.GetFullPath(assemblyPath), RegistryValueKind.String);
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

        public static void BridgeSolution(string solutionName, string vsVersion, List<BridgeProjectInfo> parameters)
        {
            string keyPath = string.Format(GetRegistryKey(EnabledProjectsRegistryKey, vsVersion, solutionName));

            //now create a general solution flag to mark the current solution as activated
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                key?.SetValue("SolutionEnabled", "True", RegistryValueKind.String);
            }

            foreach (BridgeProjectInfo executeParams in parameters)
            {
                StoreProjectAssemblyPath(executeParams.ProjectOutput, executeParams.AssemblyName,
                                executeParams.SolutionName,
                                executeParams.VsVersion);
                StoreProjectReferences(executeParams.AssemblyName,
                    executeParams.SolutionName,
                    executeParams.VsVersion, executeParams.References);
            }
        }

        public static void UnBridgeSolution(string solutionName, string vsVersion)
        {
            string keyPath = string.Format(GetRegistryKey(EnabledProjectsRegistryKey, vsVersion, solutionName));

            //to invalidate the cache I delete the value
            Registry.CurrentUser.DeleteSubKeyTree(keyPath, false);
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

        
        //public static string GetOriginalAssemblyLocation(string projectName, string solutionName, string vsVersion)
        //{
        //    string registryKeyPath = $@"Software\LINQBridgeVs\{vsVersion}\Solutions\{solutionName}";

        //    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKeyPath))
        //    {
        //        return key == null ? string.Empty : (string)key.GetValue(projectName);
        //    }
        //}

        ///// <summary>
        ///// Get the location for a given list of assemblies. If an assembly is loaded at Runtime or it's loaded within IIS context, 
        ///// Assembly.Location property could be null so all the original locations are stored in the registry during Bridging
        ///// This method reads the original location of the assembly and its references
        ///// </summary>
        ///// <returns>the paths to the assemblies</returns>
        public static List<string> GetAssemblySolutionAndProject(List<string> assemblies, string vsVersion)
        {
            string registryKeyPath = $@"Software\LINQBridgeVs\{vsVersion}\Solutions";
            List<string> referencedAssemblyPaths = new List<string>();

            if (assemblies.Count == 0)
                return referencedAssemblyPaths;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKeyPath)) //E.g. Software\LINQBridgeVs\15.0\Solutions
            {
                if (key == null)
                {
                    return referencedAssemblyPaths;
                }

                string[] solutions = key.GetSubKeyNames(); //get all of the solutions enabled
                //reads every solution and project to check if the assembly has a path
                foreach (string solution in solutions)
                {
                    using (RegistryKey projectsKey = key.OpenSubKey(solution)) //E.g. Software\LINQBridgeVs\15.0\Solutions\VsSolution1
                    {
                        if (projectsKey == null)
                            continue;

                        string[] projects = projectsKey.GetValueNames(); //get SolutionEnabled and the project path
                        foreach (string project in projects)
                        {
                            string originalAssemblyLocation = projectsKey.GetValue(project).ToString();
                            string originalAssemblyName = Path.GetFileNameWithoutExtension(originalAssemblyLocation);

                            //if any assemblies contains the project assembly associated then add the original reference
                            bool originalAssemblyFound = assemblies.Any(assembly =>
                                Path.GetFileNameWithoutExtension(assembly) == originalAssemblyName);

                            if (originalAssemblyFound)
                            {
                                referencedAssemblyPaths.Add(originalAssemblyLocation);
                            }

                            using (RegistryKey referenceAssemblyKey = projectsKey.OpenSubKey(project)
                            ) // e.g. Software\LINQBridgeVs\15.0\Solutions\VsSolution1\Project1
                            {
                                //this sub-key contains all of the references to the project
                                if (referenceAssemblyKey == null)
                                    continue;

                                string[] references = referenceAssemblyKey.GetValueNames();

                                referencedAssemblyPaths.AddRange(
                                    from reference in references
                                    where originalAssemblyFound || assemblies.Any(assembly => Path.GetFileNameWithoutExtension(assembly) == Path.GetFileNameWithoutExtension(reference))
                                    select referenceAssemblyKey.GetValue(reference) 
                                    into assemblyValue
                                    where assemblyValue != null
                                    select assemblyValue.ToString());
                            }
                        }
                    }
                }
            }

            return referencedAssemblyPaths;
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

        private static void StoreProjectReferences(string assemblyName, string solutionName, string vsVersion, IEnumerable<string> references)
        {
            string keyPath = string.Format(GetRegistryKey(ProjectReferencesRegistryKey, vsVersion, solutionName, assemblyName));
            Registry.CurrentUser.DeleteSubKeyTree(keyPath, false); //clean first
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                foreach (string reference in references)
                {
                    string refAssemblyName = Path.GetFileNameWithoutExtension(reference);
                    key?.SetValue(refAssemblyName, reference);
                }
            }
        }
    }
}