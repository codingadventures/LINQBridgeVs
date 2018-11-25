#region License
// Copyright (c) 2013 - 2018 Coding Adventures
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

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BridgeVs.Shared.Dependency;
using BridgeVs.Shared.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BridgeVs.VsPackage.Helper.Test
{
    [TestClass]
    public class CrawlerUnitTest
    {
        private const string CsProject = @"Shared\Dependency\Model.UnitTest.xml";
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Files_Needed_For_Testing_Should_Be_Deployed()
        {
            Assert.IsTrue(File.Exists(CsProject), "deployment failed: " + CsProject +
                " did not get deployed");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Crawler_Should_Return_One_Project_Dependency()
        {
            var projects = Crawler.FindDependencies(CsProject).ToList();

            //Assert
            var containsAnotherModel = projects.Any(p => p.Equals("AnotherModel.UnitTest.dll"));

            Assert.IsTrue(containsAnotherModel, "AnotherModel.UnitTest is not included in the references");
        } 

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Crawler_Should_Return_An_Empty_Collection_When_Input_Is_Empty()
        { 
            var projects = Crawler.FindDependencies(string.Empty).ToList();

            Assert.IsTrue(projects.Count == 0, "Error! no projects should be loaded");
        }
        
    }
}