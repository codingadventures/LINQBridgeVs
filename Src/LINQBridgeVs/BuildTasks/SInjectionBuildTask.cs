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
using LINQBridgeVs.Logging;
using Microsoft.Build.Framework;
using SInject;

namespace LINQBridgeVs.BuildTasks
{
    public class SInjectionBuildTask : ITask
    {
        [Required]
        public string Assembly { get; set; }

        [Required]
        public string Snk { get; set; }

        public bool Execute()
        {
            Log.Configure("SinjectionBuildTask");

            try
            {
                Log.Configure("SInjectionBuildTask");
                var snkCertificate = File.Exists(Snk) ? Snk : null;
                var sInjection = new SInjection(Assembly, mode: PatchMode.Debug, snkCertificatePath: snkCertificate);
                sInjection.Patch(SerializationTypes.BinarySerialization);
            }
            catch (Exception e)
            {
                Log.Write(e, @"Error Executing MSBuild Task SInjectionBuildTask ");
            }

            return true;
        }


        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }
    }
}