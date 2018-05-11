using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using BridgeVs.AnotherModel.Test;
using BridgeVs.DynamicVisualizers.Helper;
using BridgeVs.Model.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BridgeVs.DynamicVisualizers.Test.Helper
{
    [TestClass]
    public class ReferencedAssemblyFinderHelperTests
    {
        //private CustomType4 _customType4;
        private Dictionary<string, CustomType4> _dictionary;
        private const string SourcePath = @"c:\Root1\Version1\A\AnotherModel.Test.dll";
        private const string registryKeyPath = @"Software\LINQBridgeVs\0.0\Solutions";
        [ClassInitialize]
        [TestCategory("UnitTest")]
        public static void Initialize(TestContext c)
        {

            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                                                        {
                                                            {
                                                               SourcePath ,
                                                                new MockFileData(string.Empty)
                                                                    {
                                                                        LastAccessTime =   DateTime.Now
                                                                    }
                                                            }
                                                        });




            AssemblyFinderHelper.FileSystem = mockFileSystem;
        }

        [TestInitialize]
        [TestCategory("UnitTest")]
        public void Init()
        {
            var customType4 = new CustomType4 { { "1", new AnotherModelTest() } };


            _dictionary = new Dictionary<string, CustomType4> { { "2", customType4 } };
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FindReferencedAssembliesTest()
        {
            var refAss = _dictionary.GetType().GetReferencedAssemblies(SourcePath);

            Assert.IsTrue(refAss.Count > 0);
            Assert.AreEqual(refAss[0], SourcePath);
        }
    }
}
