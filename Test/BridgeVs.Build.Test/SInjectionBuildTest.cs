using System.Reflection;
using BridgeVs.Build.Tasks;
using BridgeVs.Model.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BridgeVs.Build.Test
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

            bool result = sInjectionBuildTask.Execute();

            Assert.IsTrue(result, "SInjection Build Task Execute return false.");
        }
    }
}