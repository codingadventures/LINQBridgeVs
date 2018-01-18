using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using LINQBridgeVs.DynamicCore;
using LINQBridgeVs.DynamicVisualizer.V14;
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


        private const string SolutionRegistryKey = @"Software\LINQBridgeVs\12.0\Solutions\LINQBridgeVsTestSolution";
    
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
                key.SetValue(typeof(DynamicDebuggerVisualizerV14Test).Assembly.GetName().Name, new[] { "True", typeof(DynamicDebuggerVisualizerV14Test).Assembly.Location });
                key.SetValue(typeof(VisualizationTestClass).Assembly.GetName().Name, new[] { "True", typeof(VisualizationTestClass).Assembly.Location });
            }

            using (var key = Registry.CurrentUser.CreateSubKey(DynamicDebuggerVisualizerV14.TestRegistryKey))
            {
                if (key != null)
                    key.SetValue("Test", true);
            }

            if (Directory.Exists(LINQPadScriptFolder))
                Directory.Delete(LINQPadScriptFolder, true);
        }

        [TestInitialize]
        public   void CloseLINQPadProcess()
        {
            ProcessKiller.Kill("LINQPad");
        }
     
        [TestMethod]
        [TestCategory("Integration")]
        public void ShowVisualizerWithLinqQueryTest()
        {
            var query = from i in Enumerable.Range(1, 10)
                        select i;

            var myHost = new VisualizerDevelopmentHost(query, typeof(DynamicDebuggerVisualizerV14), typeof(DynamicDebuggerVisualizerObjectSourceV14));
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
            var myHost = new VisualizerDevelopmentHost(enumerable, typeof(DynamicDebuggerVisualizerV14), typeof(DynamicDebuggerVisualizerObjectSourceV14));

            myHost.ShowVisualizer();


        }

        [ClassCleanup]
        public static void ClassDispose()
        {
            Registry.CurrentUser.DeleteSubKey(DynamicDebuggerVisualizerV14.TestRegistryKey);
            ProcessKiller.Kill("LINQPad");            
        }

    }
}
