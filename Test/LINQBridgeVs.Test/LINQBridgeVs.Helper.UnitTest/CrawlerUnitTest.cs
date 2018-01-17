using System;
using System.IO;
using System.Linq;
using System.Reflection;
using LINQBridgeVs.Helper.Dependency;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LINQBridgeVs.Helper.UnitTest
{
    [TestClass]
    [DeploymentItem(@"CsProj\Model.UnitTest.xml")]
    [DeploymentItem(@"CsProj\AnotherModel.UnitTest.xml")]
    public class CrawlerUnitTest
    {
        private const string CsProject = @"Model.UnitTest.xml";
        private const string Solution = @"..\..\LINQBridgeVs.sln";
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
            var projects = Crawler.FindProjectDependencies(CsProject, @"..\..\LINQBridgeVs.sln").ToList();

            //Assert
            var containsAnotherModel = projects.Any(p => p.AssemblyName.Equals("AnotherModel.UnitTest") && p.DependencyType == DependencyType.ProjectReference);

            Assert.IsTrue(containsAnotherModel, "AnotherModel.UnitTest is not included in the references");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Crawler_Should_Not_Return_Any_Assembly_Reference()
        {
            var projects = Crawler.FindProjectDependencies(CsProject, @"..\..\LINQBridgeVs.sln").ToList();


            var noAssemblyDependencyTypes = projects.All(p => p.DependencyType != DependencyType.AssemblyReference);

            Assert.IsTrue(noAssemblyDependencyTypes, "Error there are assembly references!");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Crawler_Should_Return_An_Empty_Collection_When_Input_Is_Empty()
        {
            var projects = Crawler.FindProjectDependencies(string.Empty, string.Empty).ToList();


            Assert.IsTrue(projects.Count == 0, "Error! no projects should be loaded");
        }
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Crawler_Should_Return_False_When_CompareProjectFileName_Is_Called_With_Null_Projects()
        {
            var info = typeof(Crawler).GetField("CompareProjectFileName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var compareProjectFileName =  (Func<Microsoft.Build.Evaluation.Project, string, bool>) info.GetValue(null);
            var result = compareProjectFileName(null, null);

            Assert.IsFalse(result,"Error! Passing a null project should return false");
        }

    }
}
