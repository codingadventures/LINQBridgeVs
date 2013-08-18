using System;
using System.Collections.Generic;
using System.IO;
using LINQBridge.DynamicCore.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Abstractions.TestingHelpers;

namespace DynamicCore.Test.Helper
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

            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                                                        {
                                                            {
                                                                SourcePath1,
                                                                new MockFileData(string.Empty)
                                                                    {
                                                                        LastAccessTime =
                                                                            DateTime.Now
                                                                    }
                                                            },
                                                            {
                                                                SourcePath2,
                                                                new MockFileData(string.Empty)
                                                                    {
                                                                        LastAccessTime =
                                                                            DateTime.Now
                                                                                    .AddHours(1)
                                                                    }
                                                            },
                                                        });

           

          
            AssemblyFinderHelper.FileSystem = mockFileSystem;
        }

        [TestMethod]
        public void Test_FindPath()
        {
            //Arrange
            
            //Act
            var actual = AssemblyFinderHelper.FindPath("Fake", StartPath);

            //Assert
            Assert.AreEqual(SourcePath2, actual);
        }
    }
}
