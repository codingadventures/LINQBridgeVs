/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using LINQBridgeVs.Extension;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LINQBridgeVsExtension.UnitTests
{
    [TestClass()]
    public class PackageTest
    {
        [TestMethod]
        [TestCategory("Integration")]
        public void CreateInstance()
        {
            var package = new LINQBridgeVsPackage();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void IsIVsPackage()
        {
            var package = new LINQBridgeVsPackage();
            Assert.IsNotNull(package as IVsPackage, "The object does not implement IVsPackage");
        }

        //[TestMethod()]
        //public void SetSite()
        //{
        //    // Create the package
        //    var package = new LINQBridgeVsPackage() as IVsPackage;
        //    Assert.IsNotNull(package, "The object does not implement IVsPackage");

        //    // Create a basic service provider
        //    var serviceProvider = OleServiceProvider.CreateOleServiceProviderWithBasicServices();

        //    // Site the package
        //    Assert.AreEqual(0, package.SetSite(serviceProvider), "SetSite did not return S_OK");

        //    // Unsite the package
        //    Assert.AreEqual(0, package.SetSite(null), "SetSite(null) did not return S_OK");
        //}
    }
}
