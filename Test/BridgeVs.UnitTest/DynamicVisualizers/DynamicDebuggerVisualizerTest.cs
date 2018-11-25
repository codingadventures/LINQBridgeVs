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
using System.Linq;
using System.Text.RegularExpressions;
using BridgeVs.DynamicVisualizers;
using BridgeVs.DynamicVisualizers.Template;
using BridgeVs.Shared.Common;
using BridgeVs.Shared.FileSystem;
using BridgeVs.Shared.Options;
using BridgeVs.UnitTest.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeMock.ArrangeActAssert;

namespace BridgeVs.UnitTest.DynamicVisualizers
{
    [TestClass]
    public class DynamicDebuggerVisualizerTest
    {
        private readonly Message _message = new Message(Guid.NewGuid().ToString(), SerializationOption.BinarySerializer, typeof(CustomType1));

        private static IFileSystem _fileSystem;

        private const string AnonymousListLinqScript = @"
        <Query Kind=""Program"">
        <Namespace>System</Namespace>
        <Namespace>System.Dynamic</Namespace>
        <Namespace>System.Runtime.Serialization.Formatters</Namespace>
        <Namespace>System.Xml.Linq</Namespace>
        <Namespace>BridgeVs.Shared.Serialization</Namespace>
        <Namespace>BridgeVs.Shared</Namespace>
        <Namespace>BridgeVs.Shared.Options</Namespace>
        <Namespace>System.Collections.Generic</Namespace>
        </Query>

        void Main()
        {{
            Truck.ReceiveCargo(""{0}"", SerializationOption.BinarySerializer, typeof(object)).Dump(""List<AnonymousType<String, String>>"", 2);
        }}";

        private const string IntListLinqScript = @"
        <Query Kind=""Program"">
        <Namespace>System</Namespace>
        <Namespace>System.Dynamic</Namespace>
        <Namespace>System.Runtime.Serialization.Formatters</Namespace>
        <Namespace>System.Xml.Linq</Namespace>
        <Namespace>BridgeVs.Shared.Serialization</Namespace>
        <Namespace>BridgeVs.Shared</Namespace>
        <Namespace>BridgeVs.Shared.Options</Namespace>
        <Namespace>System.Collections.Generic</Namespace>
        </Query>

        void Main()
        {{
            Truck.ReceiveCargo(""{0}"", SerializationOption.BinarySerializer, typeof(List<Int32>)).Dump(""List<Int32>"", 2);
        }}";

        [ClassInitialize]
        public static void Init(TestContext ctx)
        {
            _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing is meh.") },
                { @"c:\demo\jQuery.js", new MockFileData("some js") },
                { @"c:\demo\image.gif", new MockFileData(new byte[] { 0x12, 0x34, 0x56, 0xd2 }) }
            });
            Isolate.WhenCalled(() => FileSystemFactory.FileSystem).WillReturn(_fileSystem);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DeployLinqScriptTest()
        {
            DynamicDebuggerVisualizer cVisualizerObjectSource = new DynamicDebuggerVisualizer();
            cVisualizerObjectSource.DeployLinqScript(_message, "15.0");

            string dstScriptPath = CommonFolderPaths.DefaultLinqPadQueryFolder;

            string fileNamePath = Path.Combine(dstScriptPath, _message.AssemblyName, _message.FileName);

            Assert.IsTrue(_fileSystem.File.Exists(fileNamePath));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void InspectionTransformTest_AnonymousType()
        {
            var a = new { str1 = Guid.NewGuid().ToString(), str2 = Guid.NewGuid().ToString() };

            var res = (from i in Enumerable.Range(0, 2)
                       select a).ToList();

            Message message = new Message(Guid.NewGuid().ToString(), SerializationOption.BinarySerializer, res.GetType());
            string linqToCompare = string.Format(AnonymousListLinqScript, message.TruckId);


            Inspection linqQuery = new Inspection(message);
            string linqQueryText = linqQuery.TransformText();

            bool linqIsEqual = CompareNormalisedString(linqQueryText, linqToCompare);
            Assert.IsTrue(linqIsEqual);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void InspectionTransformTest_StrongType()
        {
            List<int> res = (from i in Enumerable.Range(0, 2)
                             select i).ToList();

            Message message = new Message(Guid.NewGuid().ToString(), SerializationOption.BinarySerializer, res.GetType());
            string linqToCompare = string.Format(IntListLinqScript, message.TruckId);

            Inspection linqQuery = new Inspection(message);
            string linqQueryText = linqQuery.TransformText();

            bool linqIsEqual = CompareNormalisedString(linqQueryText, linqToCompare);
            Assert.IsTrue(linqIsEqual);
        }
        
        private static bool CompareNormalisedString(string str1, string str2)
        {
            string normalized1 = Regex.Replace(str1, @"\s", "");
            string normalized2 = Regex.Replace(str2, @"\s", "");

            return normalized1.Equals(normalized2, StringComparison.OrdinalIgnoreCase);
        }
    }
}
