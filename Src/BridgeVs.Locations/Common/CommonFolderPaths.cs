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
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace BridgeVs.Shared.Common
{
    /// <summary>
    /// This static class is a container for all the folders needed in the extension
    /// </summary>
    public static class CommonFolderPaths
    {
        private const string CustomAfterTargets = @"Targets\Custom.After.Microsoft.Common.targets";
        private const string CustomBeforeTargets = @"Targets\Custom.Before.Microsoft.Common.targets";

        public static readonly string ProgramFilesFolderPath = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
        private static readonly string SpecialWindowsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        private static readonly string LocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static readonly string ApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static readonly string LogFolderPath = Path.Combine(LocalApplicationData, "BridgeVs");
        public static readonly string GrappleFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Grapple");

        public static readonly string VisualStudio2017Path = Path.Combine(ProgramFilesFolderPath, @"Microsoft Visual Studio\2017");
        public static readonly string VisualStudio2017EntPath = Path.Combine(ProgramFilesFolderPath, VisualStudio2017Path, "enterprise");
        public static readonly string VisualStudio2017CommPath = Path.Combine(ProgramFilesFolderPath, VisualStudio2017Path, "community");
        public static readonly string VisualStudio2017ProPath = Path.Combine(ProgramFilesFolderPath, VisualStudio2017Path, "professional");

        public static readonly string VisualStudio2015Path = Path.Combine(ProgramFilesFolderPath, @"Microsoft Visual Studio 14.0");
        public static readonly string VisualStudio2013Path = Path.Combine(ProgramFilesFolderPath, @"Microsoft Visual Studio 12.0");
        public static readonly string VisualStudio2012Path = Path.Combine(ProgramFilesFolderPath, @"Microsoft Visual Studio 11.0");

        public static string InstallFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static readonly string Documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public static readonly string LinqPad4DestinationFolder = Path.Combine(ProgramFilesFolderPath, "LINQPad4");
        public static readonly string LinqPad5DestinationFolder = Path.Combine(ProgramFilesFolderPath, "LINQPad5");
        public static readonly string LinqPadCustomQueryFolderConfigurationFile = Path.Combine(ApplicationData, "LINQPad", "QueryLocations.txt");
        public static readonly string LinqPadCustomPluginFolderConfigurationFile = Path.Combine(ApplicationData, "LINQPad", "PluginLocations.txt");

        public static readonly string CustomAfterTargetFileNamePath = Path.Combine(InstallFolder, CustomAfterTargets);
        public static readonly string CustomAfterTargetFileName = Path.GetFileName(CustomAfterTargets);

        public static readonly string CustomBeforeTargetFileNamePath = Path.Combine(InstallFolder, CustomBeforeTargets);
        public static readonly string CustomBeforeTargetFileName = Path.GetFileName(CustomBeforeTargets);

        public static readonly string MsBuildPath = Path.Combine(ProgramFilesFolderPath, "MSBuild");

        public static readonly string MsBuildPath2017 = Path.Combine(ProgramFilesFolderPath, $@"{VisualStudio2017Path}\{{0}}\MSBuild");

        public static readonly string DotNet40FrameworkPath = Path.Combine(SpecialWindowsFolderPath, @"Microsoft.NET\Framework\v4.0.30319");

        public static readonly string DotNet45FrameworkPath = Path.Combine(ProgramFilesFolderPath, @"MSBuild\12.0\Bin");

        public static readonly string DotNet40Framework64Path = Path.Combine(SpecialWindowsFolderPath, @"Microsoft.NET\Framework64\v4.0.30319");

        public static readonly string MicrosoftCommonTargetFileNamePath = Path.Combine(DotNet40FrameworkPath, "Microsoft.Common.targets");

        public static readonly string MicrosoftCommonTarget45FileNamePath = Path.Combine(DotNet45FrameworkPath, "Microsoft.Common.targets");

        public static readonly string MicrosoftCommonTargetX64FileNamePath = Path.Combine(DotNet40Framework64Path, "Microsoft.Common.targets");

        public static readonly string CommonReferenceAssembliesPath = @"Common7\IDE\ReferenceAssemblies\v2.0";

        public static readonly string Vs2012DebuggerVisualizerDestinationFolder = Documents + @"\Visual Studio 2012\Visualizers\";
        public static readonly string Vs2013DebuggerVisualizerDestinationFolder = Documents + @"\Visual Studio 2013\Visualizers\";
        public static readonly string Vs2015DebuggerVisualizerDestinationFolder = Documents + @"\Visual Studio 2015\Visualizers\";
        public static readonly string Vs2017DebuggerVisualizerDestinationFolder = Documents + @"\Visual Studio 2017\Visualizers\";

        public static string DefaultLinqPadQueryFolder
        {
            get
            {
#if !TEST
                //I could cache the file here
                if (File.Exists(LinqPadCustomQueryFolderConfigurationFile))
                {
                    string customQueryFolderPath = File.ReadLines(LinqPadCustomQueryFolderConfigurationFile).FirstOrDefault();
                    if (!string.IsNullOrEmpty(customQueryFolderPath))
                        return Path.Combine(customQueryFolderPath, "BridgeVs");
                }
#endif
                return Path.Combine(Documents, "LINQPad Queries", "BridgeVs");
            }
        }

        public static List<string> AllLinqPadPluginFolders
        {
            get
            {
                List<string> folders = new List<string>(10);
#if !TEST
                if (File.Exists(LinqPadCustomPluginFolderConfigurationFile))
                {
                    folders.AddRange(File.ReadLines(LinqPadCustomPluginFolderConfigurationFile));
                }
#endif
                folders.Add(Path.Combine(Documents, "LINQPad Plugins"));
                return folders;
            }
        }

        static CommonFolderPaths()
        {

        }
    }
}
