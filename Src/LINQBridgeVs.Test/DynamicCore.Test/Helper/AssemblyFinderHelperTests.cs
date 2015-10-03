using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using LINQBridgeVs.DynamicCore.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicCore.UnitTest.Helper
{
    [TestClass]
    public class AssemblyFinderHelperTests
    {
        private const string SourcePath1 = @"c:\Root1\Version1\A\FakeA.dll";
        private const string SourcePath2 = @"c:\Root1\Version1\B\FakeB.dll";

        private const string StartPath = @"c:\Root1\Version1\C\Version2\Nest";

        private static readonly List<FileInfo> Files = new List<FileInfo>();

        [ClassInitialize]
        public static void Initialize(TestContext c)
        {
            var paths = new string[]
            {
                @"c:\Root1\Version1\A\FakeA.dll",
                @"c:\Root1\Version1\B\FakeB.dll",
                @"c:\Root1\Version1\C\Version2\Nest\FakeC.dll",
                @"c:\Root1\Version1\C\Version2\FakeD.dll",
                @"c:\Root1\Version1\C\FakeE"
            };

            var dictionary = paths.ToDictionary(t => t, t => new MockFileData(string.Empty));

            AssemblyFinderHelper.FileSystem = new MockFileSystem(dictionary);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Test_FindPath()
        {
            //Arrange

            //Act
            var actual = AssemblyFinderHelper.FindPath("FakeB", StartPath);

            //Assert
            Assert.AreEqual(SourcePath2, actual);
        }

        [TestMethod]
        [TestCategory("UnitTest")]

        public void Test_Find_Path_Should_Exit_After_Four_Iterations()
        {
            //Arrange

            //Act
            var actual = AssemblyFinderHelper.FindPath("Nothing", StartPath);

            //Assert
            Assert.AreEqual(@"c:\Root1", actual);
        }
    }
}
