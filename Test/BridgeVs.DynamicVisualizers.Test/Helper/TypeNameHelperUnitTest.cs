using BridgeVs.DynamicVisualizers.Helper;
using BridgeVs.Model.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BridgeVs.DynamicVisualizers.Test.Helper
{
    [TestClass]
    public class TypeNameHelperUnitTest
    { 

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetDisplayName_ReturnsEmptyString()
        {
            // act
            string result = TypeNameHelper.GetDisplayName(null, false);

            // assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetDisplayName_ReturnsObject()
        {
            // act
            string result = typeof(object).GetDisplayName(false);

            // assert
            Assert.AreEqual("Object", result);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetDisplayName_ReturnsNotSerializableObject()
        {
            // act
            string result = typeof(NotSerializableObject).GetDisplayName(false);

            // assert
            Assert.AreEqual("NotSerializableObject", result);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetDisplayName_ReturnsFullNameNotSerializableObject()
        {
            // act
            string result = typeof(NotSerializableObject).GetDisplayName(true);

            // assert
            Assert.AreEqual("BridgeVs.Model.Test.NotSerializableObject", result);
        }
    }
}
