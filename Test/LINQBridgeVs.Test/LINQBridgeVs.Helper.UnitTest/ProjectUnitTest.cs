using LINQBridgeVs.Helper.Dependency;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LINQBridgeVs.Helper.UnitTest
{
    [TestClass]
    public class ProjectUnitTest
    {
        readonly Project _projectSample = new Project();

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ToString_Should_Return_DependencyTypeAssemblyReference_AssemblyName_AssemblyPath()
        {
            _projectSample.DependencyType = DependencyType.AssemblyReference;
            _projectSample.AssemblyName = "FakeA.dll";
            _projectSample.AssemblyPath = "/this/is/a/path";

            //Arrange
            var expected = string.Format("{0} {1} {2}", DependencyType.AssemblyReference, "FakeA.dll", "/this/is/a/path");

            //Act
            var actual = _projectSample.ToString();

            //Assert
            Assert.AreEqual(expected,actual,"Project.ToString() doesn't return the expected formed string");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ToString_Should_Return_DependencyTypeProjectReference_AssemblyName_AssemblyPath()
        {
            _projectSample.DependencyType = DependencyType.ProjectReference;
            _projectSample.AssemblyName = "FakeA.dll";
            _projectSample.AssemblyPath = "/this/is/a/path";

            //Arrange
            var expected = string.Format("{0} {1} {2}", DependencyType.ProjectReference, "FakeA.dll", "/this/is/a/path");

            //Act
            var actual = _projectSample.ToString();

            //Assert
            Assert.AreEqual(expected, actual, "Project.ToString() doesn't return the expected formed string");
        }

    }
}
