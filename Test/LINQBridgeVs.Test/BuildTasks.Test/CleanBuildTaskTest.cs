using System.Reflection;
using BridgeVs.Build.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.UnitTest;

namespace BridgeVs.Build.UnitTest
{
    [TestClass]
    public class CleanBuildTaskTest
    {
        private static Assembly _assemblyModel;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            _assemblyModel = typeof(CustomType1).Assembly;

        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Clean_BuildTask_Test_Should_Succeed()
        {

            var cleanBuildTask = new CleanBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = "11.0"
            };

            cleanBuildTask.Execute();
        }
    }
}
