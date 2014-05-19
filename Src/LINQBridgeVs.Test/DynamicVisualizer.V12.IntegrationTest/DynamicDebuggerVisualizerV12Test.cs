using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LINQBridgeVs.DynamicVisualizer.V12;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using Model.UnitTest;

namespace DynamicVisualizer.V12.IntegrationTest
{
    [TestClass]
    public class DynamicDebuggerVisualizerV12Test
    {
        private const string SolutionRegistryKey = @"Software\LINQBridgeVs\12.0\Solutions\LINQBridgeVsTestSolution";
    
        private static readonly string LINQPadScriptFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LINQPad Queries");

        [ClassInitialize]
        public static void Init(TestContext ctx)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(SolutionRegistryKey))
            {
                if (key == null) return;
                key.SetValue(typeof(DynamicDebuggerVisualizerV12Test).Assembly.GetName().Name, new[] { "True", typeof(DynamicDebuggerVisualizerV12Test).Assembly.Location });
                key.SetValue(typeof(VisualizationTestClass).Assembly.GetName().Name, new[] { "True", typeof(VisualizationTestClass).Assembly.Location });
            }

            using (var key = Registry.CurrentUser.CreateSubKey(DynamicDebuggerVisualizerV12.TestRegistryKey))
            {
                if (key != null)
                    key.SetValue("Test", true);
            }
            if (Directory.Exists(LINQPadScriptFolder))
                Directory.Delete(LINQPadScriptFolder, true);
        }

     
        [TestMethod]
        public void ShowVisualizerWithLinqQueryTest()
        {
            var query = from i in Enumerable.Range(1, 10)
                        select i;

            var myHost = new VisualizerDevelopmentHost(query, typeof(DynamicDebuggerVisualizerV12), typeof(DynamicDebuggerVisualizerObjectSourceV12));
            myHost.ShowVisualizer();
        }

        [TestMethod]
        public void ShowVisualizerWithLinqQueryAndCustomTypeTest()
        {
            var enumerable = new List<VisualizationTestClass>
            {
                new VisualizationTestClass{CustomType1 = new CustomType1{IntField2 = 1, SField1 = "Hello"}},
                new VisualizationTestClass{CustomType1 = new CustomType1{IntField2 = 2, SField1 = "World"}},
                new VisualizationTestClass{CustomType1 = new CustomType1{IntField2 = 3, SField1 = "!"}},
            };
            var myHost = new VisualizerDevelopmentHost(enumerable, typeof(DynamicDebuggerVisualizerV12), typeof(DynamicDebuggerVisualizerObjectSourceV12));

            myHost.ShowVisualizer();
        }

        [ClassCleanup]
        public static void ClassDispose()
        {
            Registry.CurrentUser.DeleteSubKey(DynamicDebuggerVisualizerV12.TestRegistryKey);
        }

    }
}
