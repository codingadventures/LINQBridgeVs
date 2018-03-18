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
using System.Linq;
using BridgeVs.DynamicVisualizer.V11;
using BridgeVs.Locations;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using Model.UnitTest;

namespace DynamicVisualizer.V11.IntegrationTest
{
    [TestClass]
    public class DynamicDebuggerVisualizerV11Test
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
                    //do nothin
                }
            }
        }

        private const string SolutionRegistryKey = @"Software\LINQBridgeVs\11.0\Solutions\LINQBridgeVsTestSolution";
    
        [ClassInitialize]
        public static void Init(TestContext ctx)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(SolutionRegistryKey))
            {
                if (key == null) return;
                key.SetValue(typeof(DynamicDebuggerVisualizerV11Test).Assembly.GetName().Name, new[] { "True", typeof(DynamicDebuggerVisualizerV11Test).Assembly.Location });
                key.SetValue(typeof(VisualizationTestClass).Assembly.GetName().Name, new[] { "True", typeof(VisualizationTestClass).Assembly.Location });
            }

            if (!Directory.Exists(CommonFolderPaths.LinqPadQueryFolder))
                Directory.CreateDirectory(CommonFolderPaths.LinqPadQueryFolder);
        }

        [TestInitialize]
        public  void CloseLINQPadProcess()
        {
            ProcessKiller.Kill("LINQPad");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ShowVisualizerWithLinqQueryTest()
        {
            IEnumerable<int> query = from i in Enumerable.Range(1, 10)
                        select i;

            VisualizerDevelopmentHost myHost = new VisualizerDevelopmentHost(query, typeof(DynamicDebuggerVisualizerV11), typeof(DynamicDebuggerVisualizerObjectSourceV11));
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
            VisualizerDevelopmentHost myHost = new VisualizerDevelopmentHost(enumerable, typeof(DynamicDebuggerVisualizerV11), typeof(DynamicDebuggerVisualizerObjectSourceV11));

            myHost.ShowVisualizer();
        }

        [ClassCleanup]
        public static void ClassDispose()
        {
            ProcessKiller.Kill("LINQPad");
        }
    }
}