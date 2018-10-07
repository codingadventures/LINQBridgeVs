using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using BridgeVs.DynamicVisualizers.Helper;
using BridgeVs.Shared.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeMock.ArrangeActAssert;

namespace BridgeVs.DynamicVisualizers.Test.Helper
{
    [TestClass]
    public class AssemblyFinderHelperTests
    {
        private const string SourcePath2 = @"c:\Root1\Version1\B\FakeB.dll";

        private const string StartPath = @"c:\Root1\Version1\C\Version2\Nest";

        private static Dictionary<string, MockFileData> _mockData;

        [ClassInitialize]
        public static void Initialize(TestContext c)
        {
            string[] paths = new []
            {
                @"c:\Root1\Version1\A\FakeA.dll",
                @"c:\Root1\Version1\B\FakeB.dll",
                @"c:\Root1\Version1\C\Version2\Nest\FakeC.dll",
                @"c:\Root1\Version1\C\Version2\FakeD.dll",
                @"c:\Root1\Version1\C\FakeE"
            };

            _mockData = paths.ToDictionary(t => t, t => new MockFileData(string.Empty));
        }

        [TestInitialize]
        [TestCategory("UnitTest")]
        public void Init()
        {
            Isolate.WhenCalled(() => FileSystemFactory.FileSystem).WillReturn(new MockFileSystem(_mockData));
        }

        [TestCleanup]
        [TestCategory("UnitTest")]
        public void Cleanup()
        {
            Isolate.CleanUp();
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Test_FindPath()
        {
            //Act
            string actual = AssemblyFinderHelper.FindPath("FakeB", StartPath);
            //Assert
            Assert.AreEqual(SourcePath2, actual);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Test_Find_Path_Should_Exit_After_Four_Iterations()
        {
            //Act
            string actual = AssemblyFinderHelper.FindPath("Nothing", StartPath);
            //Assert
            Assert.AreEqual(@"c:\Root1", actual);
        }
    }
}
