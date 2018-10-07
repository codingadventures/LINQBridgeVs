#region License
// Copyright (c) 2013 Coding Adventures
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using BridgeVs.VsPackage.Helper.Configuration;

namespace BridgeVs.VsPackage.Helper.Test
{
    [TestClass]
    public class MsBuildVersionHelperUnitTest
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void VS_Version_11_Should_Return_MsBuild_Version_V11()
        {
            string msBuildVersion = MsBuildVersionHelper.GetMsBuildVersion("11.0");
            Assert.IsTrue(msBuildVersion == "v4.0");
        }
        [TestMethod]
        [TestCategory("UnitTest")]
        public void VS_Version_12_Should_Return_MsBuild_Version_V12()
        {
            string msBuildVersion = MsBuildVersionHelper.GetMsBuildVersion("12.0");
            Assert.IsTrue(msBuildVersion == "v12.0");
        }
        [TestMethod]
        [TestCategory("UnitTest")]
        public void VS_Version_14_Should_Return_MsBuild_Version_V14()
        {
            string msBuildVersion = MsBuildVersionHelper.GetMsBuildVersion("14.0");
            Assert.IsTrue(msBuildVersion == "v14.0");
        }
        [TestMethod]
        [TestCategory("UnitTest")]
        public void VS_Version_15_Should_Return_MsBuild_Version_V15()
        {
            string msBuildVersion = MsBuildVersionHelper.GetMsBuildVersion("15.0");
            Assert.IsTrue(msBuildVersion == "v15.0");
        }
    }
}
