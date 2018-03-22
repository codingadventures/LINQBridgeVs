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
using BridgeVs.Locations;

namespace BridgeVs.Build
{
    internal class Settings
    {
        public string DebuggerVisualizerDestinationFolder;
        
        public string AssemblyLocation;

        public string MsBuildVersion;

        public List<string> CommonReferenceAssembliesLocation;
    }

    public static class VisualStudioOptions
    {
        private static readonly Dictionary<string, Settings> VisualStudioPaths;

        static VisualStudioOptions()
        {
            VisualStudioPaths = new Dictionary<string, Settings>
            {
                {
                    "11.0", new Settings
                    {
                        DebuggerVisualizerDestinationFolder = CommonFolderPaths.Vs2012DebuggerVisualizerDestinationFolder,
                        AssemblyLocation = typeof(DynamicVisualizer.V11.Settings).Assembly.Location,
                        MsBuildVersion = "v4.0",
                        CommonReferenceAssembliesLocation =new List<string> { Path.Combine(CommonFolderPaths.VisualStudio2012Path, CommonFolderPaths.CommonReferenceAssembliesPath)}
                    }
                },
                {
                    "12.0", new Settings
                    {
                        DebuggerVisualizerDestinationFolder = CommonFolderPaths.Vs2013DebuggerVisualizerDestinationFolder,
                        AssemblyLocation = typeof(DynamicVisualizer.V12.Settings).Assembly.Location,
                        MsBuildVersion = "v12.0",
                        CommonReferenceAssembliesLocation =new List<string> {  Path.Combine(CommonFolderPaths.VisualStudio2013Path, CommonFolderPaths.CommonReferenceAssembliesPath)}
                    }
                },
                {
                    "14.0", new Settings
                    {
                        DebuggerVisualizerDestinationFolder = CommonFolderPaths.Vs2015DebuggerVisualizerDestinationFolder,
                        AssemblyLocation =typeof(DynamicVisualizer.V14.Settings).Assembly.Location,
                        MsBuildVersion = "v14.0",
                        CommonReferenceAssembliesLocation = new List<string> { Path.Combine(CommonFolderPaths.VisualStudio2015Path, CommonFolderPaths.CommonReferenceAssembliesPath)}
                    }
                },
                {
                    "15.0", new Settings
                    {
                        DebuggerVisualizerDestinationFolder = CommonFolderPaths.Vs2017DebuggerVisualizerDestinationFolder,
                        AssemblyLocation = typeof(LINQBridgeVs.DynamicVisualizer.V15.Settings).Assembly.Location,
                        MsBuildVersion = "v15.0",
                        CommonReferenceAssembliesLocation =new List<string> {  Path.Combine(CommonFolderPaths.VisualStudio2017CommPath, CommonFolderPaths.CommonReferenceAssembliesPath), Path.Combine(CommonFolderPaths.VisualStudio2017EntPath, CommonFolderPaths.CommonReferenceAssembliesPath), Path.Combine(CommonFolderPaths.VisualStudio2017ProPath, CommonFolderPaths.CommonReferenceAssembliesPath)}
                    }
                }
            };
        }

        private static void CheckVersion(string vsInputVersion)
        {
            if (vsInputVersion == null) throw new ArgumentNullException(nameof(vsInputVersion));

            if (!VisualStudioPaths.ContainsKey(vsInputVersion))
                throw new ArgumentException("This Version of visual studio is not yet supported.", nameof(vsInputVersion));
        }

        public static string GetVisualizerAssemblyLocation(string visualStudioVersion)
        {
            CheckVersion(visualStudioVersion);

            return VisualStudioPaths[visualStudioVersion].AssemblyLocation;
        }

        public static string GetVisualizerDestinationFolder(string visualStudioVersion)
        {
            CheckVersion(visualStudioVersion);

            return VisualStudioPaths[visualStudioVersion].DebuggerVisualizerDestinationFolder;
        }

        public static List<string> GetCommonReferenceAssembliesPath(string visualStudioVersion)
        {
            CheckVersion(visualStudioVersion);

            return VisualStudioPaths[visualStudioVersion].CommonReferenceAssembliesLocation;
        }
    }
}