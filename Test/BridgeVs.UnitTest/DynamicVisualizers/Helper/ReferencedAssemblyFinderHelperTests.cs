using System.Collections.Generic;
using System.Linq;
using BridgeVs.Shared.Util;
using BridgeVs.UnitTest.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeMock.ArrangeActAssert;

namespace BridgeVs.UnitTest.DynamicVisualizers.Helper
{
    [TestClass]
    [Isolated]
    public class ReferencedAssemblyFinderHelperTests
    {
        private Dictionary<string, CustomType4> _dictionary; 
         

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
            Assert.IsTrue(refAss.Any(p => p.Equals(typeof(CustomType4).Assembly.GetName().Name)));
        }
    }
}
