using BridgeVs.Helper.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BridgeVs.Extension.Helper.UnitTest
{
    [TestClass]
    public class MsBuildVersionHelperUnitTest
    {
        [TestMethod]
        public void VS_Version_11_Should_Return_MsBuild_Version_V11()
        {
            var msBuildVersion = MsBuildVersionHelper.GetMsBuildVersion("11.0");
            Assert.IsTrue(msBuildVersion == "v4.0");
        }
        [TestMethod]
        public void VS_Version_12_Should_Return_MsBuild_Version_V12()
        {
            var msBuildVersion = MsBuildVersionHelper.GetMsBuildVersion("12.0");
            Assert.IsTrue(msBuildVersion == "v12.0");
        }
        [TestMethod]
        public void VS_Version_14_Should_Return_MsBuild_Version_V14()
        {
            var msBuildVersion = MsBuildVersionHelper.GetMsBuildVersion("14.0");
            Assert.IsTrue(msBuildVersion == "v14.0");
        }
        [TestMethod]
        public void VS_Version_15_Should_Return_MsBuild_Version_V15()
        {
            var msBuildVersion = MsBuildVersionHelper.GetMsBuildVersion("15.0");
            Assert.IsTrue(msBuildVersion == "v15.0");
        }
    }
}
