using BridgeVs.AnotherModel.Test;
using BridgeVs.DynamicVisualizers.Helper;
using BridgeVs.Model.Test;
using BridgeVs.Shared.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using TypeMock.ArrangeActAssert;

namespace BridgeVs.DynamicVisualizers.Test.Helper
{
    [TestClass]
    public class ReferencedAssemblyFinderHelperTests
    {
        private Dictionary<string, CustomType4> _dictionary;
        private const string SourcePath = @"c:\Root1\Version1\A\AnotherModel.Test.dll";
        private static MockFileSystem _mockFileSystem;
       [ClassInitialize]
        [TestCategory("UnitTest")]
        public static void Initialize(TestContext c)
        {

            _mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                                                        {
                                                            {
                                                               SourcePath ,
                                                                new MockFileData(string.Empty)
                                                                    {
                                                                        LastAccessTime =   DateTime.Now
                                                                    }
                                                            }
                                                        });
        }

        [TestCategory("UnitTest")]
        [TestCleanup]
        public void Cleanup()
        {
            Isolate.CleanUp();
        }

        [TestInitialize]
        [TestCategory("UnitTest")]
        public void Init()
        {
            Isolate.WhenCalled(() => FileSystemFactory.FileSystem).WillReturn(_mockFileSystem);

            CustomType4 customType4 = new CustomType4 { { "1", new AnotherModelTest() } };
            _dictionary = new Dictionary<string, CustomType4> { { "2", customType4 } };
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FindReferencedAssembliesTest()
        {
            List<string> refAss = _dictionary.GetType().GetReferencedAssemblies(SourcePath);

            Assert.IsTrue(refAss.Count > 0);
            Assert.AreEqual(refAss[0], SourcePath);
        }
    }
}
