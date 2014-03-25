using System.Reflection;
using LINQBridgeVs.BuildTasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Test;

namespace BuildTasks.Test
{
    [TestClass]
    public class MapperBuildTaskTest
    {
        private static Assembly _assemblyModel;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            _assemblyModel = typeof(CustomType1).Assembly;

        }

        [TestMethod]
        public void MapperBuildTaskTest_V11_Should_Succeed()
        {

            var mapper = new MapperBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = "11.0"
            };


            var result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");

        }

        [TestMethod]
        public void MapperBuildTaskTest_V10_Should_Succeed()
        {

            var mapper = new MapperBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = "10.0"
            };


            var result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");

        }
    }
}
