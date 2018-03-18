using System.IO;
using System.Reflection;
using BridgeVs.Build.Tasks;
using BridgeVs.Build.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.UnitTest;

namespace BridgeVs.Build.UnitTest
{
    [TestClass]
    public class CleanBuildTest
    {
        private static Assembly _assemblyModel;

        private static void CreateDllAndPdb(string visualizerFullPath, string visualizerPdbFullPath)
        {
            using (File.Create(visualizerFullPath))
            {
            }

            using (File.Create(visualizerPdbFullPath))
            {
            }
        }
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            _assemblyModel = typeof(CustomType1).Assembly;
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Clean_BuildTask_Test_V11_Should_Succeed()
        {
            const string vsVersion = "11.0";
            string visualizerAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            string targetInstallationPath = VisualStudioOptions.GetInstallationPath(vsVersion);
            string visualizerFullPath = Path.Combine(targetInstallationPath, visualizerAssemblyName);
            string visualizerPdbFullPath = visualizerFullPath.Replace(".dll", ".pdb");
            CreateDllAndPdb(visualizerFullPath, visualizerPdbFullPath);


            CleanBuildTask cleanBuildTask = new CleanBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = vsVersion
            };

            bool result = cleanBuildTask.Execute();

            Assert.IsTrue(result, $"Clean build task V{vsVersion} failed");
            Assert.IsFalse(File.Exists(visualizerFullPath), $"{visualizerFullPath} hasn't been deleted correctly");
            Assert.IsFalse(File.Exists(visualizerPdbFullPath), $"{visualizerPdbFullPath} hasn't been deleted correctly");
        }


        [TestMethod]
        [TestCategory("UnitTest")]
        public void Clean_BuildTask_Test_V12_Should_Succeed()
        {
            const string vsVersion = "12.0";
            string visualizerAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            string targetInstallationPath = VisualStudioOptions.GetInstallationPath(vsVersion);
            string visualizerFullPath = Path.Combine(targetInstallationPath, visualizerAssemblyName);
            string visualizerPdbFullPath = visualizerFullPath.Replace(".dll",".pdb");
            CreateDllAndPdb(visualizerFullPath, visualizerPdbFullPath);

            CleanBuildTask cleanBuildTask = new CleanBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = vsVersion
            };

            bool result = cleanBuildTask.Execute();

            Assert.IsTrue(result, $"Clean build task V{vsVersion} failed");
            Assert.IsFalse(File.Exists(visualizerFullPath), $"{visualizerFullPath} hasn't been deleted correctly");
            Assert.IsFalse(File.Exists(visualizerPdbFullPath), $"{visualizerPdbFullPath} hasn't been deleted correctly");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Clean_BuildTask_Test_V14_Should_Succeed()
        {
            const string vsVersion = "14.0";
            string visualizerAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            string targetInstallationPath = VisualStudioOptions.GetInstallationPath(vsVersion);
            string visualizerFullPath = Path.Combine(targetInstallationPath, visualizerAssemblyName);
            string visualizerPdbFullPath = visualizerFullPath.Replace(".dll", ".pdb");
            CreateDllAndPdb(visualizerFullPath, visualizerPdbFullPath);


            CleanBuildTask cleanBuildTask = new CleanBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = vsVersion
            };

            bool result = cleanBuildTask.Execute();

            Assert.IsTrue(result, $"Clean build task V{vsVersion} failed");
            Assert.IsFalse(File.Exists(visualizerFullPath), $"{visualizerFullPath} hasn't been deleted correctly");
            Assert.IsFalse(File.Exists(visualizerPdbFullPath), $"{visualizerPdbFullPath} hasn't been deleted correctly");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Clean_BuildTask_Test_V15_Should_Succeed()
        {
            const string vsVersion = "15.0";
            string visualizerAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            string targetInstallationPath = VisualStudioOptions.GetInstallationPath(vsVersion);
            string visualizerFullPath = Path.Combine(targetInstallationPath, visualizerAssemblyName);
            string visualizerPdbFullPath = visualizerFullPath.Replace(".dll", ".pdb");

            CreateDllAndPdb(visualizerFullPath, visualizerPdbFullPath);

            CleanBuildTask cleanBuildTask = new CleanBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = vsVersion
            };

            bool result = cleanBuildTask.Execute();

            Assert.IsTrue(result, $"Clean build task V{vsVersion} failed");
            Assert.IsFalse(File.Exists(visualizerFullPath), $"{visualizerFullPath} hasn't been deleted correctly");
            Assert.IsFalse(File.Exists(visualizerPdbFullPath), $"{visualizerPdbFullPath} hasn't been deleted correctly");
        }
    }
}
