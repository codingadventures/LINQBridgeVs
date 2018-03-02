using System.Reflection;
using BridgeVs.BuildTasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.UnitTest;

namespace BuildTasks.UnitTest
{
    [TestClass]
    public class SInjectionBuildTaskTest
    {
        private static Assembly _assemblyModel;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            _assemblyModel = typeof(CustomType1).Assembly;

        }
         
        [TestMethod]
        [TestCategory("UnitTest")]
        public void SInjection_BuildTask_Test_Should_Succeed()
        {

            var sInjectionBuildTask = new SInjectionBuildTask
                                          {
                                              Assembly = _assemblyModel.Location
                                          };


            var result = sInjectionBuildTask.Execute();

            Assert.IsTrue(result, "SInjection Build Task Execute return false.");

        }

        
    }
}



