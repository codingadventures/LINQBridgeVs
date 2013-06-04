using System.Reflection;
using LINQBridge.BuildTasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Test;

namespace InjectionBuildTask.Test
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
        public void MapperBuildTaskTest_V11()
        {

            var mapper = new MapperBuildTask()
                             {
                                 Assembly = _assemblyModel.Location,
                                 VisualStudioVer = "11.0"
                             };


            var result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");

        } 
        
        [TestMethod]
        public void MapperBuildTaskTest_V10()
        {

            var mapper = new MapperBuildTask()
                             {
                                 Assembly = _assemblyModel.Location,
                                 VisualStudioVer = "10.0"
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

        
    }
}



