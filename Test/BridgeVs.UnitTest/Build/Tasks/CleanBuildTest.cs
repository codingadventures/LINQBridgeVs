#region License
// Copyright (c) 2013 - 2018 Coding Adventures
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion


using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using BridgeVs.Build.Tasks;
using BridgeVs.Build.Util;
using BridgeVs.Shared.Common;
using BridgeVs.Shared.Options;
using BridgeVs.UnitTest.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeMock.ArrangeActAssert;
using FS = BridgeVs.Shared.FileSystem.FileSystemFactory;
namespace BridgeVs.UnitTest.Build.Tasks
{
    /// <summary>
    /// This should become unit test and using the system.io.abstractions
    /// </summary>
    [TestClass]
    [Isolated]
    public class CleanBuildTest
    {
        private static Assembly _assemblyModel;

        private static void CreateDllAndPdb(string visualizerFullPath, string visualizerPdbFullPath)
        {
            string path = Path.GetDirectoryName(visualizerFullPath);
            FS.FileSystem.Directory.CreateDirectory(path);

            using (FS.FileSystem.File.Create(visualizerFullPath))
            {
            }

            path = Path.GetDirectoryName(visualizerPdbFullPath);
            FS.FileSystem.Directory.CreateDirectory(path);
            using (FS.FileSystem.File.Create(visualizerPdbFullPath))
            {
            }
        }
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            _assemblyModel = typeof(CustomType1).Assembly;
        }

        [TestInitialize]
        public void Init()
        {
            Isolate.WhenCalled(() => FS.FileSystem).WillReturn(new MockFileSystem());
            Isolate.WhenCalled(() =>CommonRegistryConfigurations.IsErrorTrackingEnabled("")).WillReturn(false);
            Isolate.WhenCalled(() =>CommonRegistryConfigurations.IsLoggingEnabled("")).WillReturn(false);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Clean_BuildTask_Test_V11_Should_Succeed()
        {
            const string vsVersion = "11.0";
            string visualizerAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            string targetInstallationPath = VisualStudioOption.GetVisualizerDestinationFolder(vsVersion);
            string visualizerFullPath = Path.Combine(targetInstallationPath, visualizerAssemblyName);
            string visualizerPdbFullPath = Path.ChangeExtension(visualizerFullPath,"pdb");
            CreateDllAndPdb(visualizerFullPath, visualizerPdbFullPath);


            CleanBuildTask cleanBuildTask = new CleanBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = vsVersion
            };

            bool result = cleanBuildTask.Execute();

            Assert.IsTrue(result, $"Clean build task V{vsVersion} failed");
            Assert.IsFalse(FS.FileSystem.File.Exists(visualizerFullPath), $"{visualizerFullPath} hasn't been deleted correctly");
            Assert.IsFalse(FS.FileSystem.File.Exists(visualizerPdbFullPath), $"{visualizerPdbFullPath} hasn't been deleted correctly");
        }


        [TestMethod]
        [TestCategory("UnitTest")]
        public void Clean_BuildTask_Test_V12_Should_Succeed()
        {
            const string vsVersion = "12.0";
            string visualizerAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            string targetInstallationPath = VisualStudioOption.GetVisualizerDestinationFolder(vsVersion);
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
            Assert.IsFalse(FS.FileSystem.File.Exists(visualizerFullPath), $"{visualizerFullPath} hasn't been deleted correctly");
            Assert.IsFalse(FS.FileSystem.File.Exists(visualizerPdbFullPath), $"{visualizerPdbFullPath} hasn't been deleted correctly");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Clean_BuildTask_Test_V14_Should_Succeed()
        {
            const string vsVersion = "14.0";
            string visualizerAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            string targetInstallationPath = VisualStudioOption.GetVisualizerDestinationFolder(vsVersion);
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
            Assert.IsFalse(FS.FileSystem.File.Exists(visualizerFullPath), $"{visualizerFullPath} hasn't been deleted correctly");
            Assert.IsFalse(FS.FileSystem.File.Exists(visualizerPdbFullPath), $"{visualizerPdbFullPath} hasn't been deleted correctly");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Clean_BuildTask_Test_V15_Should_Succeed()
        {
            const string vsVersion = "15.0";
            string visualizerAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            string targetInstallationPath = VisualStudioOption.GetVisualizerDestinationFolder(vsVersion);
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
            Assert.IsFalse(FS.FileSystem.File.Exists(visualizerFullPath), $"{visualizerFullPath} hasn't been deleted correctly");
            Assert.IsFalse(FS.FileSystem.File.Exists(visualizerPdbFullPath), $"{visualizerPdbFullPath} hasn't been deleted correctly");
        }
    }
}
