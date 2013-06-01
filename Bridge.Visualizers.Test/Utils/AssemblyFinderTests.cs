using System;
using System.Collections.Generic;
using System.IO;
using LINQBridge.DynamicVisualizers.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Abstraction = System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;


namespace DynamicVisualizers.Test.Utils
{
    [TestClass]
    public class AssemblyFinderTests
    {
        private const string SourcePath1 = @"\Root1\Version1\A\FakeA.dll";
        private const string SourcePath2 = @"\Root1\Version2\B\FakeB.dll";

        private static readonly List<FileInfo> Files = new List<FileInfo>();

        [ClassInitialize]
        public static void Initialize(TestContext c)
        {
        
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { SourcePath1,  new MockFileData(string.Empty){LastAccessTime =   DateTime.Now}},
                    { SourcePath2,  new MockFileData(string.Empty){LastAccessTime = DateTime.Now.AddHours(1)}},
                });



            //mockFileSystem.Directory.GetFiles()
            //fileSystem.Setup(
            //    system =>
            //    system.EnumerateFiles(It.IsAny<DirectoryInfo>(), It.IsAny<string>(), SearchOption.AllDirectories))
            //    .Returns(() => mockFileSystem.FileInfo);


            AssemblyFinder.FileSystem = (Abstraction.IFileSystem) mockFileSystem;
        }

        [TestMethod]
        public void Test_FindPath()
        {
            //Arrange
            const string expectedFile = "FakeB.dll";

            //Act
            var actual = AssemblyFinder.FindPath("Fake.dll", SourcePath1);


            //Assert
            Assert.AreEqual(expectedFile, actual);
        }
    }
}
