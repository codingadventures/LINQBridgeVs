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
using System.Collections.Generic;
using System.Reflection;

namespace BridgeVs.Build
{
    internal class Settings
    {
        public string InstallationPaths;

        public Assembly Assembly;

        public string AssemblyLocation;

        public string MsBuildVersion;
    }

    public static class VisualStudioOptions
    {
        private static readonly string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string Vs2012Path1 = MyDocuments + @"\Visual Studio 2012\Visualizers\";
        private static readonly string Vs2013Path1 = MyDocuments + @"\Visual Studio 2013\Visualizers\";
        private static readonly string Vs2015Path1 = MyDocuments + @"\Visual Studio 2015\Visualizers\";
        private static readonly string Vs2017Path1 = MyDocuments + @"\Visual Studio 2017\Visualizers\";


        private static readonly Dictionary<string, Settings> VisualStudioPaths;

        static VisualStudioOptions()
        {
            VisualStudioPaths = new Dictionary<string, Settings>
            {
                {
                    "11.0", new Settings
                    {
                        InstallationPaths = Vs2012Path1,
                        AssemblyLocation = typeof(DynamicVisualizer.V11.Settings).Assembly.Location,
                        Assembly = typeof(DynamicVisualizer.V11.Settings).Assembly,
                        MsBuildVersion = "v4.0"

                    }
                },
                {
                    "12.0", new Settings
                    {
                        InstallationPaths = Vs2013Path1,
                        AssemblyLocation = typeof(DynamicVisualizer.V12.Settings).Assembly.Location,
                        Assembly = typeof(DynamicVisualizer.V12.Settings).Assembly,
                        MsBuildVersion = "v12.0"
                    }
                },
                {
                    "14.0", new Settings
                    {
                        InstallationPaths = Vs2015Path1,
                        AssemblyLocation =typeof(DynamicVisualizer.V14.Settings).Assembly.Location,
                        Assembly = typeof(DynamicVisualizer.V14.Settings).Assembly,
                        MsBuildVersion = "v14.0"
                    }
                },
                {
                    "15.0", new Settings
                    {
                        InstallationPaths = Vs2017Path1,
                        Assembly = typeof(LINQBridgeVs.DynamicVisualizer.V15.Settings).Assembly,
                        AssemblyLocation = typeof(LINQBridgeVs.DynamicVisualizer.V15.Settings).Assembly.Location,
                        MsBuildVersion = "v15.0"
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

        public static string GetInstallationPath(string visualStudioVersion)
        {
            CheckVersion(visualStudioVersion);

            return VisualStudioPaths[visualStudioVersion].InstallationPaths;
        }
    }
}