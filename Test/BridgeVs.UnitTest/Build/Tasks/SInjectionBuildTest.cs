using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using BridgeVs.Build.Tasks;
using BridgeVs.Shared.Common;
using BridgeVs.UnitTest.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using TypeMock.ArrangeActAssert;
using FS = BridgeVs.Shared.FileSystem.FileSystemFactory;

namespace BridgeVs.UnitTest.Build.Tasks
{
    [TestClass]
    [Isolated]
    public class SInjectionBuildTest
    {
        private static string AssemblyToInjectLocation => typeof(CustomType1).Assembly.Location;
        private static byte[] AssemblyToInjectBytes => File.ReadAllBytes(AssemblyToInjectLocation);

        
        private static readonly MockFileSystem MockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { AssemblyToInjectLocation, new MockFileData(AssemblyToInjectBytes) }
        });
      

        [TestInitialize]
        public void Init()
        {
            Isolate.WhenCalled(() => FS.FileSystem).WillReturn(MockFileSystem);

            object[] args =
            {
                AssemblyToInjectLocation, FileMode.Open, FileAccess.ReadWrite,
                FileShare.Read
            };
            Stream access = FS.FileSystem.File.Open(AssemblyToInjectLocation, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite);

            Isolate.NonPublic.WhenCalled(typeof(ModuleDefinition), "GetFileStream")
                .WithExactArguments(args)
                .WillReturn(access); 

            Isolate.WhenCalled(() => CommonRegistryConfigurations.IsSolutionEnabled("", "")).WillReturn(true);
            Isolate.WhenCalled(() => CommonRegistryConfigurations.Map3RdPartyAssembly("", "")).WillReturn(false);
            Isolate.WhenCalled(() => CommonRegistryConfigurations.IsErrorTrackingEnabled("")).WillReturn(false);
            Isolate.WhenCalled(() => CommonRegistryConfigurations.IsLoggingEnabled("")).WillReturn(false);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SInjection_BuildTask_Test_Should_Succeed()
        {

            SInjectionBuildTask sInjectionBuildTask = new SInjectionBuildTask
            {
                Assembly = AssemblyToInjectLocation
            };

            bool result = sInjectionBuildTask.Execute();

            Assert.IsTrue(result, "SInjection Build Task Execute return false.");
        }
    }
}