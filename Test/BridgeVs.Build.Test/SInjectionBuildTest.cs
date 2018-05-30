using System.IO;
using System.Reflection;
using BridgeVs.Build.Tasks;
using BridgeVs.Model.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BridgeVs.Build.Test
{
    [TestClass]
    public class SInjectionBuildTest
    {
        private static string _assemblyToInjectLocation;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            Assembly assemblyModel = typeof(CustomType1).Assembly;
            //need to copy the assembly because it is loaded in the app domain and cannot
            //be changed as it's readonly
            string fileName = Path.GetFileNameWithoutExtension(assemblyModel.Location);
            string newFileName = fileName + "_test";
            string location = Path.GetDirectoryName(assemblyModel.Location);
            _assemblyToInjectLocation = Path.Combine(location, newFileName) + ".dll";
            File.Copy(assemblyModel.Location, _assemblyToInjectLocation, true);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            File.Delete(_assemblyToInjectLocation);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SInjection_BuildTask_Test_Should_Succeed()
        {
            SInjectionBuildTask sInjectionBuildTask = new SInjectionBuildTask
            {
                Assembly = _assemblyToInjectLocation
            };

            bool result = sInjectionBuildTask.Execute();

            Assert.IsTrue(result, "SInjection Build Task Execute return false.");
        }
    }
}