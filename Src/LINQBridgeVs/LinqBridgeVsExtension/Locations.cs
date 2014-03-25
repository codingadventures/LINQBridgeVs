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
using System.IO;
using System.Reflection;

namespace LINQBridgeVs.Extension
{
    internal static class Locations
    {

        public static string InstallFolder
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        public static readonly string LinqPadDestinationFolder = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), "LINQPad4");

        public static readonly string LinqBridgeTargetFileNamePath = Path.Combine(InstallFolder, Resources.Targets);
        public static readonly string LinqBridgeTargetFileName = Path.GetFileName(Resources.Targets);

        public static readonly string DotNet40FrameworkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                @"Microsoft.NET\Framework\v4.0.30319");

        public static readonly string DotNet40Framework64Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                       @"Microsoft.NET\Framework64\v4.0.30319");

        public static readonly string MicrosoftCommonTargetFileNamePath = Path.Combine(DotNet40FrameworkPath, "Microsoft.Common.targets");

        public static readonly string MicrosoftCommonTarget64FileNamePath = Path.Combine(DotNet40Framework64Path, "Microsoft.Common.targets");

        public static readonly string IcaclsArguments = String.Format("{0} /grant Everyone:F", Path.Combine(DotNet40FrameworkPath, LinqBridgeTargetFileName));
        public static readonly string IcaclsArgumentsX64 = String.Format("{0} /grant Everyone:F", Path.Combine(DotNet40Framework64Path, LinqBridgeTargetFileName));

        public static readonly string IcaclsArgumentsCommonTarget = String.Format("{0} /grant Everyone:F", MicrosoftCommonTargetFileNamePath);
        public static readonly string IcaclsArgumentsX64CommonTarget = String.Format("{0} /grant Everyone:F", MicrosoftCommonTarget64FileNamePath);
    }
}
