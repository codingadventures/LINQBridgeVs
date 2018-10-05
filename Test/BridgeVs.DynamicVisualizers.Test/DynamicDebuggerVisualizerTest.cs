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
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BridgeVs.DynamicVisualizers.Template;
using BridgeVs.Model.Test;
using BridgeVs.Shared.Common;

namespace BridgeVs.DynamicVisualizers.Test
{
    [TestClass]
    public class DynamicDebuggerVisualizerTest
    {
        private readonly Message _message = new Message
        {
            FileName = DateTime.Now.ToShortDateString().Replace("/", ""),
            TypeFullName = typeof(CustomType1).FullName,
            TypeNamespace = typeof(CustomType1).Namespace,
            TypeName = typeof(CustomType1).Name,
            AssemblyName = typeof(CustomType1).Assembly.GetName().Name,
            TruckId = Guid.NewGuid().ToString()
        };

       
        private static IFileSystem _fileSystem;

        [ClassInitialize]
        public static void Init(TestContext ctx)
        {
            _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing is meh.") },
                { @"c:\demo\jQuery.js", new MockFileData("some js") },
                { @"c:\demo\image.gif", new MockFileData(new byte[] { 0x12, 0x34, 0x56, 0xd2 }) }
            });

        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DeployLinqScriptTest()
        {
            DynamicDebuggerVisualizer cVisualizerObjectSource = new DynamicDebuggerVisualizer(_fileSystem);
            cVisualizerObjectSource.DeployLinqScript(_message);

            string dstScriptPath = CommonFolderPaths.DefaultLinqPadQueryFolder;

            string fileNamePath = Path.Combine(dstScriptPath, _message.AssemblyName, _message.FileName);

            Assert.IsTrue(_fileSystem.File.Exists(fileNamePath));
        }
    }
}
