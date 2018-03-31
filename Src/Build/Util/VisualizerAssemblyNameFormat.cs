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

namespace BridgeVs.Build.Util
{
    internal static class VisualizerAssemblyNameFormat
    {
        private const string DotNetFrameworkVisualizerName = "";

        /// <summary>
        /// This method returns the file name for the visualizer assembly being generated for the source assembly being mapped
        /// based on the visual studio version
        /// </summary>
        /// <param name="vsVersion">The visual studio version. 11(VS2012),12 (VS2013),14 (VS2015), 15 (VS2017)</param>
        /// <param name="assembly">The source assembly being mapped by the visualizer</param>
        /// <returns>The visualizer file name.</returns>
        /// <example> Source Assembly: ConsoleApp.exe - VS Version: 15 - ConsoleApp.Visualizer.V15.dll </example>
        internal static string GetTargetVisualizerAssemblyName(string vsVersion, string assembly)
        {
            if (!vsVersion.Contains("."))
            {
                throw new ArgumentException(@"The passed visual studio version is not correct", nameof(vsVersion));
            }

            string versionNumber = vsVersion.Split('.')[0];
            return $"{Path.GetFileNameWithoutExtension(assembly)}.Visualizer.V{versionNumber}.dll";
        }

        /// <summary>
        ///  This methods returns the file name for the visualizer assembly being generated for the dot net framework.
        /// </summary>
        /// <param name="vsVersion">The visual studio version. 11(VS2012),12 (VS2013),14 (VS2015), 15 (VS2017).</param>
        /// <returns>he visualizer file name.</returns>
        /// <example>VS Version: 15 - DotNetDynamicVisualizerType.V15.dll</example>
        public static string GetDotNetVisualizerName(string vsVersion)
        {
            return $"DotNetDynamicVisualizerType.V{vsVersion}.dll";
        }
    }
}