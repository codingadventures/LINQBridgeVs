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

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using BridgeVs.DynamicCore;
using BridgeVs.DynamicVisualizer.V14;
using BridgeVs.Locations;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using Model.UnitTest;

namespace DynamicVisualizer.V14.IntegrationTest
{
    [TestClass]
    public class DynamicDebuggerVisualizerV14Test
    {
        private static class ProcessKiller
        {
            public static void Kill(string processName)
            {
                try
                {
                    foreach (Process proc in Process.GetProcessesByName(processName))
                    {
                        proc.Kill();
                    }
                }
                catch
                {
                    //do nothing
                }
            }
        }

        private const string SolutionRegistryKey = @"Software\LINQBridgeVs\14.0\Solutions\DynamicVisualizer.V14.IntegrationTest";
        private const string SolutionRegistryKeyModel = @"Software\LINQBridgeVs\14.0\Solutions\UnitTest.Model";

        [ClassInitialize]
        public static void Init(TestContext ctx)
        {
            DynamicDebuggerVisualizer.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing is meh.") },
                { @"c:\demo\jQuery.js", new MockFileData("some js") },
                { @"c:\demo\image.gif", new MockFileData(new byte[] { 0x12, 0x34, 0x56, 0xd2 }) }
            });

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(SolutionRegistryKey))
            {
                if (key == null) return;
                key.SetValue(typeof(DynamicDebuggerVisualizerV14Test).Assembly.GetName().Name, "True");
                key.SetValue(typeof(DynamicDebuggerVisualizerV14Test).Assembly.GetName().Name + "_location", typeof(DynamicDebuggerVisualizerV14Test).Assembly.Location);
            }

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(SolutionRegistryKeyModel))
            {
                if (key == null) return;

                key.SetValue(typeof(VisualizationTestClass).Assembly.GetName().Name, "True");
                key.SetValue(typeof(VisualizationTestClass).Assembly.GetName().Name + "_location", typeof(VisualizationTestClass).Assembly.Location);

            }

            if (!Directory.Exists(CommonFolderPaths.LinqPadQueryFolder))
                Directory.CreateDirectory(CommonFolderPaths.LinqPadQueryFolder);
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
            IEnumerable<int> query = from i in Enumerable.Range(1, 10)
                                     select i;

            VisualizerDevelopmentHost myHost = new VisualizerDevelopmentHost(query, typeof(DynamicDebuggerVisualizerV14), typeof(DynamicDebuggerVisualizerObjectSourceV14));
            myHost.ShowVisualizer();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ShowVisualizerWithLinqQueryAndCustomTypeTest()
        {
            List<VisualizationTestClass> enumerable = new List<VisualizationTestClass>
            {
                new VisualizationTestClass{CustomType1 = new CustomType1{IntField2 = 1, SField1 = "Hello"}},
                new VisualizationTestClass{CustomType1 = new CustomType1{IntField2 = 2, SField1 = "World"}},
                new VisualizationTestClass{CustomType1 = new CustomType1{IntField2 = 3, SField1 = "!"}},
            };
            VisualizerDevelopmentHost myHost = new VisualizerDevelopmentHost(enumerable, typeof(DynamicDebuggerVisualizerV14), typeof(DynamicDebuggerVisualizerObjectSourceV14));
            myHost.ShowVisualizer();
        }

        [ClassCleanup]
        public static void ClassDispose()
        {
            Registry.CurrentUser.DeleteSubKey(SolutionRegistryKey);
            Registry.CurrentUser.DeleteSubKey(SolutionRegistryKeyModel);
            ProcessKiller.Kill("LINQPad");
        }
    }
}