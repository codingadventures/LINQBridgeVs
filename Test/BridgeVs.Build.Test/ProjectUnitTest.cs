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


using Microsoft.VisualStudio.TestTools.UnitTesting;
using BridgeVs.Build.Dependency;

namespace BridgeVs.Build.Test
{
    [TestClass]
    public class ProjectUnitTest
    {
        private readonly ProjectDependency _projectSample = new ProjectDependency();

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
            Assert.AreEqual(expected, actual, "Project.ToString() doesn't return the expected formed string");
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
