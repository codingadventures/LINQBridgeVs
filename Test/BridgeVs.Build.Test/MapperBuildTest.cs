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

using BridgeVs.Build.Tasks;
using BridgeVs.Build.Util;
using BridgeVs.Model.Test;
using BridgeVs.Shared.Common;
using BridgeVs.Shared.Options;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using TypeMock.ArrangeActAssert;
using FS = BridgeVs.Shared.FileSystem.FileSystemFactory;

namespace BridgeVs.Build.Test
{
    [TestClass]
    public class MapperBuildTest
    {
        private const string VsVersion11 = "11.0";
        private const string VsVersion12 = "12.0";
        private const string VsVersion14 = "14.0";
        private const string VsVersion15 = "15.0";

        private static readonly MockFileSystem _mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
    {
        { DebuggerVisualizerAssemblyLocation, new MockFileData(DebuggerVisualizerAssemblyByte) }
    });

        private static string AssemblyModelLocation => typeof(CustomType1).Assembly.Location;

        private static string DebuggerVisualizerAssemblyLocation => typeof(DynamicVisualizers.DynamicDebuggerVisualizer).Assembly.Location;

        private static byte[] DebuggerVisualizerAssemblyByte => File.ReadAllBytes(DebuggerVisualizerAssemblyLocation);
            

        private static string TargetAssemblyName(string vsVersion)
        {
            return VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(vsVersion, AssemblyModelLocation);
        }

        private static string TargetInstallationPath(string vsVersion)
        {
            return VisualStudioOption.GetVisualizerDestinationFolder(vsVersion);
        }

        private static string DotNetAssemblyName(string vsVersion)
        {
            return VisualizerAssemblyNameFormat.GetDotNetVisualizerName(vsVersion);
        }

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            Isolate.WhenCalled(() => FS.FileSystem).WillReturn(_mockFileSystem);

            Isolate.WhenCalled(() => CommonRegistryConfigurations.IsSolutionEnabled("", "")).WillReturn(true);
            Isolate.WhenCalled(() => CommonRegistryConfigurations.IsErrorTrackingEnabled("")).WillReturn(false);
            Isolate.WhenCalled(() => CommonRegistryConfigurations.IsLoggingEnabled("")).WillReturn(false);

            if (!FS.FileSystem.Directory.Exists(TargetInstallationPath(VsVersion11)))
            {
                FS.FileSystem.Directory.CreateDirectory(TargetInstallationPath(VsVersion11));
            }

            if (!FS.FileSystem.Directory.Exists(TargetInstallationPath(VsVersion12)))
            {
                FS.FileSystem.Directory.CreateDirectory(TargetInstallationPath(VsVersion12));
            }

            if (!FS.FileSystem.Directory.Exists(TargetInstallationPath(VsVersion14)))
            {
                FS.FileSystem.Directory.CreateDirectory(TargetInstallationPath(VsVersion14));
            }

            if (!FS.FileSystem.Directory.Exists(TargetInstallationPath(VsVersion15)))
            {
                FS.FileSystem.Directory.CreateDirectory(TargetInstallationPath(VsVersion15));
            }
        }

        [TestInitialize]
        public void Init()
        {
            Isolate.WhenCalled(() => FS.FileSystem).WillReturn(_mockFileSystem);

            Isolate.WhenCalled(() => CommonRegistryConfigurations.IsSolutionEnabled("", "")).WillReturn(true);
            Isolate.WhenCalled(() => CommonRegistryConfigurations.Map3RdPartyAssembly("", "")).WillReturn(false);
            Isolate.WhenCalled(() => CommonRegistryConfigurations.IsErrorTrackingEnabled("")).WillReturn(false);
            Isolate.WhenCalled(() => CommonRegistryConfigurations.IsLoggingEnabled("")).WillReturn(false);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Isolate.CleanUp();
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Mapper_Build_Test_V11_Should_Succeed()
        {
            MapperBuildTask mapper = new MapperBuildTask
            {
                Assembly = AssemblyModelLocation,
                VisualStudioVer = VsVersion11
            };
            mapper.BuildEngine = Isolate.Fake.Instance<IBuildEngine>(Members.ReturnRecursiveFakes);

            bool result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");
            Assert.IsTrue(FS.FileSystem.File.Exists(Path.Combine(TargetInstallationPath(VsVersion11), TargetAssemblyName(VsVersion11))), $"Custom Debugger Visualizer {TargetAssemblyName(VsVersion11)} hasn't been created");
            Assert.IsTrue(FS.FileSystem.File.Exists(Path.Combine(TargetInstallationPath(VsVersion11), DotNetAssemblyName(VsVersion11))), $"DotNet Debugger Visualizer {DotNetAssemblyName(VsVersion11)} hasn't been created ");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Mapper_Build_Test_V12_Should_Succeed()
        {
            MapperBuildTask mapper = new MapperBuildTask
            {
                Assembly = AssemblyModelLocation,
                VisualStudioVer = VsVersion12
            };
            mapper.BuildEngine = Isolate.Fake.Instance<IBuildEngine>(Members.ReturnRecursiveFakes);

            bool result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");
            Assert.IsTrue(FS.FileSystem.File.Exists(Path.Combine(TargetInstallationPath(VsVersion12), TargetAssemblyName(VsVersion12))), $"Custom Debugger Visualizer {TargetAssemblyName(VsVersion12)} hasn't been created");
            Assert.IsTrue(FS.FileSystem.File.Exists(Path.Combine(TargetInstallationPath(VsVersion12), DotNetAssemblyName(VsVersion12))), $"DotNet Debugger Visualizer {DotNetAssemblyName(VsVersion12)} hasn't been created ");
        }
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Mapper_Build_Test_V14_Should_Succeed()
        {
            MapperBuildTask mapper = new MapperBuildTask
            {
                Assembly = AssemblyModelLocation,
                VisualStudioVer = VsVersion14
            };
            mapper.BuildEngine = Isolate.Fake.Instance<IBuildEngine>(Members.ReturnRecursiveFakes);

            bool result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");

            Assert.IsTrue(FS.FileSystem.File.Exists(Path.Combine(TargetInstallationPath(VsVersion14), TargetAssemblyName(VsVersion14))), $"Custom Debugger Visualizer {TargetAssemblyName(VsVersion14)} hasn't been created");
            Assert.IsTrue(FS.FileSystem.File.Exists(Path.Combine(TargetInstallationPath(VsVersion14), DotNetAssemblyName(VsVersion14))), $"DotNet Debugger Visualizer {DotNetAssemblyName(VsVersion14)} hasn't been created ");
        }
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Mapper_Build_Test_V15_Should_Succeed()
        {
            MapperBuildTask mapper = new MapperBuildTask
            {
                Assembly = AssemblyModelLocation,
                VisualStudioVer = VsVersion15
            };
            mapper.BuildEngine = Isolate.Fake.Instance<IBuildEngine>(Members.ReturnRecursiveFakes);

            bool result = mapper.Execute();

            Assert.IsTrue(result, "Mapper Build Task Execute return false.");

            Assert.IsTrue(FS.FileSystem.File.Exists(Path.Combine(TargetInstallationPath(VsVersion15), TargetAssemblyName(VsVersion15))), $"Custom Debugger Visualizer {TargetAssemblyName(VsVersion15)} hasn't been created");
            Assert.IsTrue(FS.FileSystem.File.Exists(Path.Combine(TargetInstallationPath(VsVersion15), DotNetAssemblyName(VsVersion15))), $"DotNet Debugger Visualizer {DotNetAssemblyName(VsVersion15)} hasn't been created ");
        }
    }
}