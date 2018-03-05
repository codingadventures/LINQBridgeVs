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
using System.IO;
using System.Reflection;

namespace BridgeVs.Helper
{
    /// <summary>
    /// This static class is a container for all the folders needed in the extension
    /// </summary>
    public static class Locations
    {
        private static readonly string SpecialWindowsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        public static readonly string ProgramFilesFolderPath = Environment.GetEnvironmentVariable("PROGRAMFILES");

        public static string InstallFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


        public static readonly string LinqPad4DestinationFolder = Path.Combine(ProgramFilesFolderPath, "LINQPad4");
        public static string LinqPad5DestinationFolder = Path.Combine(ProgramFilesFolderPath, "LINQPad5");

        public static readonly string CustomAfterTargetFileNamePath = Path.Combine(InstallFolder, Resources.CustomAfterTargets);
        public static readonly string CustomAfterTargetFileName = Path.GetFileName(Resources.CustomAfterTargets);

        public static readonly string CustomBeforeTargetFileNamePath = Path.Combine(InstallFolder, Resources.CustomBeforeTargets);
        public static readonly string CustomBeforeTargetFileName = Path.GetFileName(Resources.CustomBeforeTargets);

        public static readonly string MsBuildPath = Path.Combine(ProgramFilesFolderPath, "MSBuild");

        public static readonly string MsBuildPath2017 = Path.Combine(ProgramFilesFolderPath, @"Microsoft Visual Studio\2017\{0}\MSBuild");

        public static readonly string DotNet40FrameworkPath = Path.Combine(SpecialWindowsFolderPath, @"Microsoft.NET\Framework\v4.0.30319");

        public static readonly string DotNet45FrameworkPath = Path.Combine(ProgramFilesFolderPath, @"MSBuild\12.0\Bin");

        public static readonly string DotNet40Framework64Path = Path.Combine(SpecialWindowsFolderPath, @"Microsoft.NET\Framework64\v4.0.30319");

        public static readonly string MicrosoftCommonTargetFileNamePath = Path.Combine(DotNet40FrameworkPath, "Microsoft.Common.targets");

        public static readonly string MicrosoftCommonTarget45FileNamePath = Path.Combine(DotNet45FrameworkPath, "Microsoft.Common.targets");

        public static readonly string MicrosoftCommonTargetX64FileNamePath = Path.Combine(DotNet40Framework64Path, "Microsoft.Common.targets");

        static Locations()
        {
            
        }
    }
}
