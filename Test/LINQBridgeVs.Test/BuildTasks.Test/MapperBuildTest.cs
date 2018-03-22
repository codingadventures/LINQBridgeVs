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
            string targetAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            string targetInstallationPath = VisualStudioOptions.GetVisualizerDestinationFolder(vsVersion);

            MapperBuildTask mapper = new MapperBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = vsVersion
            };

            bool result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");
            Assert.IsTrue(File.Exists(Path.Combine(targetInstallationPath,targetAssemblyName)));

            File.Delete(Path.Combine(targetInstallationPath, targetAssemblyName));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Mapper_Build_Test_V12_Should_Succeed()
        {
            const string vsVersion = "12.0";
            string targetAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            string targetInstallationPath = VisualStudioOptions.GetVisualizerDestinationFolder(vsVersion);

            MapperBuildTask mapper = new MapperBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = vsVersion
            };

            bool result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");
            Assert.IsTrue(File.Exists(Path.Combine(targetInstallationPath, targetAssemblyName)));

            File.Delete(Path.Combine(targetInstallationPath, targetAssemblyName));
        }
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Mapper_Build_Test_V14_Should_Succeed()
        {
            const string vsVersion = "14.0";
            string targetAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            string targetInstallationPath = VisualStudioOptions.GetVisualizerDestinationFolder(vsVersion);

            MapperBuildTask mapper = new MapperBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = vsVersion
            };

            bool result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");
            Assert.IsTrue(File.Exists(Path.Combine(targetInstallationPath, targetAssemblyName)));

            File.Delete(Path.Combine(targetInstallationPath, targetAssemblyName));
        }
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Mapper_Build_Test_V15_Should_Succeed()
        {
            const string vsVersion = "15.0";
            string targetAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, _assemblyModel.Location);
            string targetInstallationPath = VisualStudioOptions.GetVisualizerDestinationFolder(vsVersion);

            MapperBuildTask mapper = new MapperBuildTask
            {
                Assembly = _assemblyModel.Location,
                VisualStudioVer = vsVersion
            };

            bool result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");
            Assert.IsTrue(File.Exists(Path.Combine(targetInstallationPath, targetAssemblyName)));

            File.Delete(Path.Combine(targetInstallationPath, targetAssemblyName));
        }
    }
}