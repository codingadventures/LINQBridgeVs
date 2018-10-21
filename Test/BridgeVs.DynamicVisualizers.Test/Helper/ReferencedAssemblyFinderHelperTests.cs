using BridgeVs.AnotherModel.Test;
using BridgeVs.Model.Test;
using BridgeVs.Shared.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using TypeMock.ArrangeActAssert;

namespace BridgeVs.DynamicVisualizers.Test.Helper
{
    [TestClass]
    public class ReferencedAssemblyFinderHelperTests
    {
        private Dictionary<string, CustomType4> _dictionary;
        private const string SourcePath = @"BridgeVs.Model.Test";


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
            CustomType4 customType4 = new CustomType4 { { "1", new AnotherModelTest() } };
            _dictionary = new Dictionary<string, CustomType4> { { "2", customType4 } };
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FindReferencedAssembliesTest()
        {
            List<string> refAss = _dictionary.GetType().FindAssemblyNames();

            Assert.IsTrue(refAss.Count > 0);
            Assert.IsTrue(refAss.Any(p => p.Equals(SourcePath)));
        }
    }
}
