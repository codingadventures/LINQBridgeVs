using System.Reflection;
using BridgeVs.Build.Tasks;
using BridgeVs.Build.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.UnitTest;
using System.IO;

namespace BridgeVs.Build.UnitTest
{
    [TestClass]
    public class MapperBuildTest
    {
        private static Assembly _assemblyModel;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            _assemblyModel = typeof(CustomType1).Assembly;

        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Mapper_Build_Test_V11_Should_Succeed()
        {
            const string vsVersion = "11.0";
            var targetAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            var targetInstallationPath = VisualStudioOptions.GetInstallationPath(vsVersion);

            var mapper = new MapperBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = vsVersion
            };

            var result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");
            Assert.IsTrue(File.Exists(Path.Combine(targetInstallationPath,targetAssemblyName)));

            File.Delete(Path.Combine(targetInstallationPath, targetAssemblyName));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Mapper_Build_Test_V12_Should_Succeed()
        {
            const string vsVersion = "12.0";
            var targetAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            var targetInstallationPath = VisualStudioOptions.GetInstallationPath(vsVersion);

            var mapper = new MapperBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = vsVersion
            };

            var result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");
            Assert.IsTrue(File.Exists(Path.Combine(targetInstallationPath, targetAssemblyName)));

            File.Delete(Path.Combine(targetInstallationPath, targetAssemblyName));
        }
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Mapper_Build_Test_V14_Should_Succeed()
        {
            const string vsVersion = "14.0";
            var targetAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            var targetInstallationPath = VisualStudioOptions.GetInstallationPath(vsVersion);

            var mapper = new MapperBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = vsVersion
            };

            var result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");
            Assert.IsTrue(File.Exists(Path.Combine(targetInstallationPath, targetAssemblyName)));

            File.Delete(Path.Combine(targetInstallationPath, targetAssemblyName));
        }
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Mapper_Build_Test_V15_Should_Succeed()
        {
            const string vsVersion = "15.0";
            var targetAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            var targetInstallationPath = VisualStudioOptions.GetInstallationPath(vsVersion);

            var mapper = new MapperBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = vsVersion
            };

            var result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");
            Assert.IsTrue(File.Exists(Path.Combine(targetInstallationPath, targetAssemblyName)));

            File.Delete(Path.Combine(targetInstallationPath, targetAssemblyName));
        }
    }
}