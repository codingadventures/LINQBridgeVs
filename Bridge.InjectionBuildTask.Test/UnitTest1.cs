using System;
using System.Linq;
using Bridge.BuildTasks;
using Bridge.Test.AssemblyModel;
using System.Reflection;
using Bridge.Visualizers;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bridge.InjectionBuildTask.Test
{
    [TestClass]
    public class DebuggerVisualizerMapperTest
    {
        private static Assembly _assemblyModel;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            _assemblyModel = typeof(CustomType1).Assembly;

        }

        [TestMethod]
        public void TestMethod1()
        {

            var mapper = new MapperBuildTask()
                             {
                                 Assembly = _assemblyModel.Location,
                                 Resources = "LINQPad\\LINQPAD.exe;LINQPAD\\debugging.linq",
                                 VisualStudioVer = "VS2010"
                             };


            mapper.Execute();

        }

        [TestMethod]
        public void TestMethod2()
        {

            var sInjectionBuildTask = new SInjectionBuildTask()
                                          {
                                              Assembly = _assemblyModel.Location
                                          };


            sInjectionBuildTask.Execute();

        }

        [TestMethod]
        public void TestVisualizer()
        {

            VisualizationTestClass c = new VisualizationTestClass();

            var myHost = new VisualizerDevelopmentHost(c, typeof(LINQPadDebuggerVisualizer));
            myHost.ShowVisualizer();
        }
    }
}



