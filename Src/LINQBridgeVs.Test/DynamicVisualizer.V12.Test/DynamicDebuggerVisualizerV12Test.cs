using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LINQBridgeVs.DynamicVisualizer.V12;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using Model.Test;

namespace DynamicVisualizer.V12.Test
{
    [TestClass]
    public class DynamicDebuggerVisualizerV12Test
    {
        private const string SolutionRegistryKey = @"Software\LINQBridgeVs\12.0\Solutions\LINQBridgeVsTestSolution";

        private readonly string _linqPadScriptFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LINQPad Queries");

        [ClassInitialize]
        public static void Init(TestContext ctx)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(SolutionRegistryKey))
            {
                if (key != null)
                {
                    key.SetValue(typeof(DynamicDebuggerVisualizerV12Test).Assembly.GetName().Name, new[] { "True", typeof(DynamicDebuggerVisualizerV12Test).Assembly.Location });
                    key.SetValue(typeof(VisualizationTestClass).Assembly.GetName().Name, new[] { "True", typeof(VisualizationTestClass).Assembly.Location });
                }
            }

        }

        [TestInitialize]
        public void TestInit()
        {
            if (Directory.Exists(_linqPadScriptFolder))
                Directory.Delete(_linqPadScriptFolder, true);

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

        [TestCleanup]
        public void TestFinish()
        {
            IntPtr windowPtr = FindWindowByCaption(IntPtr.Zero, "TemporaryForm");
            if (windowPtr == IntPtr.Zero)
            {
                Console.WriteLine("Window not found");
                return;
            }

            SendMessage(windowPtr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// Find window by Caption only. Note you must pass IntPtr.Zero as the first parameter.
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        const UInt32 WM_CLOSE = 0x0010;
    }
}
