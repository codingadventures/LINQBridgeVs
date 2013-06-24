using System;
using System.Collections.Generic;
using System.IO;
using LINQBridge.DynamicCore.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Abstraction = System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;


namespace DynamicVisualizers.Test.Utils
{
    [TestClass]
    public class AssemblyFinderTests
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

           

          
            AssemblyFinder.FileSystem = mockFileSystem;
        }

        [TestMethod]
        public void Test_FindPath()
        {
            //Arrange
            
            //Act
            var actual = AssemblyFinder.FindPath("Fake.dll", StartPath);

            //Assert
            Assert.AreEqual(SourcePath2, actual);
        }
    }
}
