using System;
using System.Collections.Generic;
using LINQBridge.VisualStudio.Properties;

namespace LINQBridge.VisualStudio
{

    public enum VisualStudioVersion
    {
        VS2010,
        VS2012

    }

    internal struct Settings
    {
        public List<string> InstallationPaths;

        public string AssemblyName;
        public string AssemblyLocation;

    }

    public class VisualStudioOptions
    {
        private static readonly string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string Vs2010Path1 = MyDocuments + Resources.VS2010Path1;
        private static readonly string Vs2010Path2 = MyDocuments + Resources.VS2010Path2;
        private static readonly string Vs2012Path1 = MyDocuments + Resources.VS2012Path1;


        private static readonly Dictionary<string, Settings> VisualStudioPaths;

        static VisualStudioOptions()
        {
            VisualStudioPaths = new Dictionary<string, Settings>
                                    {
                                        {
                                            "10.0", new Settings()
                                                        {
                                                            InstallationPaths =
                                                                new List<string>{Vs2010Path1, Vs2010Path2},

                                                            AssemblyName = DynamicVisualizer.V10.Settings.AssemblyName,
                                                            AssemblyLocation = DynamicVisualizer.V10.Settings.AssemblyLocation
                                                        }
                                        },
                                        { 
                                             "11.0", new Settings()
                                                        {
                                                            InstallationPaths =
                                                              new List<string>{Vs2012Path1},
                                                            AssemblyName = DynamicVisualizer.V11.Settings.AssemblyName,
                                                            AssemblyLocation = DynamicVisualizer.V11.Settings.AssemblyLocation 
                                                        }
                                        }                             
                                    };
        }

        private static void CheckVersion(string vsInputVersion)
        {
            if (!VisualStudioPaths.ContainsKey(vsInputVersion))
                throw new ArgumentException("visualStudioVersion", "This Version of visual studio is not yet supported.");

        }

        public static string GetVisualizerAssemblyName(string visualStudioVersion)
        {
            CheckVersion(visualStudioVersion);
            
            return VisualStudioPaths[visualStudioVersion].AssemblyName;
        }

        public static string GetVisualizerAssemblyLocation(string visualStudioVersion)
        {
            CheckVersion(visualStudioVersion);

            return VisualStudioPaths[visualStudioVersion].AssemblyLocation;
        }

        public static List<string> GetInstallationPath(string visualStudioVersion)
        {
            CheckVersion(visualStudioVersion);

            return VisualStudioPaths[visualStudioVersion].InstallationPaths;
        }


    }
}
