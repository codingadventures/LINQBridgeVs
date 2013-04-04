using System;
using System.Linq;
using Bridge.Test.AssemblyModel;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bridge.InjectionBuildTask.Test
{
    [TestClass]
    public class DebuggerVisualizerMapperTest
    {
        private static Assembly _assemblyModel;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            _assemblyModel = typeof(CustomType1).Assembly;

        }

        [TestMethod]
        public void TestMethod1()
        {

            var mapper = new MapperBuildTask()
                        {
                            Assembly = _assemblyModel.Location,
                            Resources = " ",
                            VisualStudioVer = ""
                        };


            mapper.Execute();

        }

        [TestMethod]
        public void TestMethod2()
        {

            var sInjectionBuildTask = new SInjectionBuildTask()
                        {
                            Assembly = _assemblyModel.Location
                        };


            sInjectionBuildTask.Execute();

        }
    }
}
