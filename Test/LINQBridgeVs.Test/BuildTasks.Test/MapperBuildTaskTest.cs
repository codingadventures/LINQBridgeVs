using System.Reflection;
using BridgeVs.Build.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.UnitTest;

namespace BridgeVs.Build.UnitTest
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
        [TestCategory("UnitTest")]
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
    }
}