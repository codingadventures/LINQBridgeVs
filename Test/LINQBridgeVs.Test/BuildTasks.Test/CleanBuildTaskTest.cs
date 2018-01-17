using System.Reflection;
using LINQBridgeVs.BuildTasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.UnitTest;

namespace BuildTasks.UnitTest
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
