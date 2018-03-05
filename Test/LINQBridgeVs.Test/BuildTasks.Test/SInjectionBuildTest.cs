using System.Reflection;
using BridgeVs.Build.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.UnitTest;

namespace BridgeVs.Build.UnitTest
{
    [TestClass]
    public class SInjectionBuildTest
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

            SInjectionBuildTask sInjectionBuildTask = new SInjectionBuildTask
            {
                Assembly = _assemblyModel.Location
            };

            var result = sInjectionBuildTask.Execute();

            Assert.IsTrue(result, "SInjection Build Task Execute return false.");
        }
    }
}