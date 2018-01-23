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
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using LINQBridgeVs.DynamicCore;
using LINQBridgeVs.DynamicVisualizer.V15;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using Model.UnitTest;

namespace DynamicVisualizer.V15.IntegrationTest
{
    [TestClass]
    public class DynamicDebuggerVisualizerV15Test
    {

        private static class ProcessKiller
        {
            public static void Kill(string processName)
            {
                try
                {
                    foreach (var proc in Process.GetProcessesByName(processName))
                    {
                        proc.Kill();
                    }
                }
                catch (Exception)
                {
                    //do nothing
                }
            }
        }

        private const string SolutionRegistryKey = @"Software\LINQBridgeVs\15.0\Solutions\DynamicVisualizer.V15.IntegrationTest";
        private const string SolutionRegistryKeyModel = @"Software\LINQBridgeVs\15.0\Solutions\UnitTest.Model";

        private static readonly string LINQPadScriptFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LINQPad Queries");

        [ClassInitialize]
        public static void Init(TestContext ctx)
        {
            DynamicDebuggerVisualizer.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing is meh.") },
                { @"c:\demo\jQuery.js", new MockFileData("some js") },
                { @"c:\demo\image.gif", new MockFileData(new byte[] { 0x12, 0x34, 0x56, 0xd2 }) }
            });

            using (var key = Registry.CurrentUser.CreateSubKey(SolutionRegistryKey))
            {
                if (key == null) return;
                key.SetValue(typeof(DynamicDebuggerVisualizerV15Test).Assembly.GetName().Name, "True");
                key.SetValue(typeof(DynamicDebuggerVisualizerV15Test).Assembly.GetName().Name + "_location", typeof(DynamicDebuggerVisualizerV15Test).Assembly.Location);
            }

            using (var key = Registry.CurrentUser.CreateSubKey(SolutionRegistryKeyModel))
            {
                if (key == null) return;

                key.SetValue(typeof(VisualizationTestClass).Assembly.GetName().Name, "True");
                key.SetValue(typeof(VisualizationTestClass).Assembly.GetName().Name+ "_location", typeof(VisualizationTestClass).Assembly.Location);

            }

            using (var key = Registry.CurrentUser.CreateSubKey(DynamicDebuggerVisualizerV15.TestRegistryKey))
            {
                if (key != null)
                    key.SetValue("Test", true);
            }

            if (Directory.Exists(LINQPadScriptFolder))
                Directory.Delete(LINQPadScriptFolder, true);
        }

        [TestInitialize]
        public void CloseLINQPadProcess()
        {
            ProcessKiller.Kill("LINQPad");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ShowVisualizerWithLinqQueryTest()
        {
            var query = from i in Enumerable.Range(1, 10)
                        select i;

            var myHost = new VisualizerDevelopmentHost(query, typeof(DynamicDebuggerVisualizerV15), typeof(DynamicDebuggerVisualizerObjectSourceV15));
            myHost.ShowVisualizer();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ShowVisualizerWithLinqQueryAndCustomTypeTest()
        {
            var enumerable = new List<VisualizationTestClass>
            {
                new VisualizationTestClass{CustomType1 = new CustomType1{IntField2 = 1, SField1 = "Hello"}},
                new VisualizationTestClass{CustomType1 = new CustomType1{IntField2 = 2, SField1 = "World"}},
                new VisualizationTestClass{CustomType1 = new CustomType1{IntField2 = 3, SField1 = "!"}},
            };
            var myHost = new VisualizerDevelopmentHost(enumerable, typeof(DynamicDebuggerVisualizerV15), typeof(DynamicDebuggerVisualizerObjectSourceV15));
            myHost.ShowVisualizer();
        }

        [ClassCleanup]
        public static void ClassDispose()
        {
            Registry.CurrentUser.DeleteSubKey(SolutionRegistryKey);
            Registry.CurrentUser.DeleteSubKey(SolutionRegistryKeyModel);
            Registry.CurrentUser.DeleteSubKey(DynamicDebuggerVisualizerV15.TestRegistryKey);
            ProcessKiller.Kill("LINQPad");
        }

    }
}
