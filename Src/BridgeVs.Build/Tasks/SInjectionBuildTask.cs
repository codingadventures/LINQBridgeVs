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
using BridgeVs.Shared.Logging;
using Microsoft.Build.Framework;

namespace BridgeVs.Build.Tasks
{
    public class SInjectionBuildTask : ITask
    {
        [Required]
        public string VisualStudioVer { private get; set; }

        [Required]
        public string Assembly { get; set; }

        [Required]
        public string Snk { get; set; }

        public bool Execute()
        {
            Log.VisualStudioVersion = VisualStudioVer;

            try
            {
                string snkCertificate = File.Exists(Snk) ? Snk : null;
                var sInjection = new SInjection(Assembly, mode: PatchMode.Debug, snkCertificatePath: snkCertificate);
                return sInjection.Patch(SerializationTypes.BinarySerialization);
            }
            catch (Exception e)
            {
                const string errorMessage = "Error Executing MSBuild Task SInjectionBuildTask";
                Log.Write(e, errorMessage);

                e.Capture(VisualStudioVer, message: errorMessage);

                return false;
            }
        }

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }
    }
}