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
        public void MapperBuildTaskTest()
        {

            var mapper = new MapperBuildTask()
                             {
                                 Assembly = _assemblyModel.Location,
                                 Resources = "LINQPad\\LINQPAD.exe;LINQPAD\\debugging.linq",
                                 VisualStudioVer = "VS2012"
                             };


            var result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");

        }

        [TestMethod]
        public void SInjectionBuildTaskTest()
        {

            var sInjectionBuildTask = new SInjectionBuildTask()
                                          {
                                              Assembly = _assemblyModel.Location
                                          };


            var result = sInjectionBuildTask.Execute();

            Assert.IsTrue(result, "SInjection Build Task Execute return false.");

        }

        [TestMethod]
        public void LINQPadDebuggerVisualizerTest()
        {

            VisualizationTestClass c = new VisualizationTestClass();

            var myHost = new VisualizerDevelopmentHost(c, typeof(LINQPadDebuggerVisualizer));
            myHost.ShowVisualizer();
        }
    }
}



