using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LINQBridgeVs.DynamicVisualizer.V11;
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
                    foreach (var proc in Process.GetProcessesByName(processName))
                    {
                        proc.Kill();
                    }
                }
                catch (Exception ex)
                {
                    //do nothin
                }
            }
        }

        private const string SolutionRegistryKey = @"Software\LINQBridgeVs\11.0\Solutions\LINQBridgeVsTestSolution";
    
        private static readonly string LINQPadScriptFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LINQPad Queries");

        [ClassInitialize]
        public static void Init(TestContext ctx)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(SolutionRegistryKey))
            {
                if (key == null) return;
                key.SetValue(typeof(DynamicDebuggerVisualizerV11Test).Assembly.GetName().Name, new[] { "True", typeof(DynamicDebuggerVisualizerV11Test).Assembly.Location });
                key.SetValue(typeof(VisualizationTestClass).Assembly.GetName().Name, new[] { "True", typeof(VisualizationTestClass).Assembly.Location });
            }

            using (var key = Registry.CurrentUser.CreateSubKey(DynamicDebuggerVisualizerV11.TestRegistryKey))
            {
                if (key != null)
                    key.SetValue("Test", true);
            }

            if (Directory.Exists(LINQPadScriptFolder))
                Directory.Delete(LINQPadScriptFolder, true);
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
            var query = from i in Enumerable.Range(1, 10)
                        select i;

            var myHost = new VisualizerDevelopmentHost(query, typeof(DynamicDebuggerVisualizerV11), typeof(DynamicDebuggerVisualizerObjectSourceV11));
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
            var myHost = new VisualizerDevelopmentHost(enumerable, typeof(DynamicDebuggerVisualizerV11), typeof(DynamicDebuggerVisualizerObjectSourceV11));

            myHost.ShowVisualizer();
        }

        [ClassCleanup]
        public static void ClassDispose()
        {
            Registry.CurrentUser.DeleteSubKey(DynamicDebuggerVisualizerV11.TestRegistryKey);
            ProcessKiller.Kill("LINQPad");
        }

    }
}
