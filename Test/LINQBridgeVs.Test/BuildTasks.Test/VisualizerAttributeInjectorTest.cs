using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BridgeVs.Build.TypeMapper;
using BridgeVs.DynamicVisualizer.V11;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;

namespace BridgeVs.Build.UnitTest
{
    [TestClass]
    public class VisualizerAttributeInjectorTest
    {
        private static string _thisAssemblyLocation;
        private static string _thisAssemblyDirectoryName;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            _thisAssemblyLocation = typeof(VisualizerAttributeInjectorTest).Assembly.Location;
            _thisAssemblyDirectoryName = Path.GetDirectoryName(typeof(VisualizerAttributeInjectorTest).Assembly.Location);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Visualizer_MapType_Should_Create_DegguberVisualizerAttribute_OfType_This_In_Target_Assembly()
        {
            string debuggerVisualizerTargetName = Path.Combine(_thisAssemblyDirectoryName, "TestA.dll");
            string visualizerLocation = typeof(DynamicDebuggerVisualizerObjectSourceV11).Assembly.Location;
            VisualizerAttributeInjector injector = new VisualizerAttributeInjector(visualizerLocation);
            injector.MapType(GetType());
            injector.SaveDebuggerVisualizer(debuggerVisualizerTargetName);

            AssemblyDefinition generatedAssembly = AssemblyDefinition.ReadAssembly(debuggerVisualizerTargetName);
            IEnumerable<object> mappedTypeNames = generatedAssembly
                .CustomAttributes
                .SelectMany(p =>
                    p.Properties.Where(n => n.Name.Equals("TargetTypeName"))
                    .Select(l => l.Argument.Value));

            Assert.IsTrue(mappedTypeNames.Contains(GetType().AssemblyQualifiedName), "Mapping was not successful");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Visualizer_MapType_Should_Create_DegguberVisualizerAttribute_OfType_IList_In_Target_Assembly()
        {
            string debuggerVisualizerTargetName = Path.Combine(_thisAssemblyDirectoryName, "TestB.dll");
            string visualizerLocation = typeof(DynamicDebuggerVisualizerObjectSourceV11).Assembly.Location;
            VisualizerAttributeInjector injector = new VisualizerAttributeInjector(visualizerLocation);
            injector.MapType(typeof(IList<>));
            injector.SaveDebuggerVisualizer(debuggerVisualizerTargetName);

            AssemblyDefinition generatedAssembly = AssemblyDefinition.ReadAssembly(debuggerVisualizerTargetName);
            IEnumerable<object> mappedTypeNames = generatedAssembly
                .CustomAttributes
                .SelectMany(p =>
                    p.Properties.Where(n => n.Name.Equals("TargetTypeName"))
                    .Select(l => l.Argument.Value));

            Assert.IsTrue(mappedTypeNames.Contains(typeof(IList<>).AssemblyQualifiedName), "Mapping was not successful");
        }


        [TestMethod]
        [TestCategory("UnitTest")]
        public void Visualizer_MapTypeFromAssembly_Should_Map_All_Types()
        {
            string debuggerVisualizerTargetName = Path.Combine(_thisAssemblyDirectoryName, "TestC.dll");
            string visualizerLocation = typeof(DynamicDebuggerVisualizerObjectSourceV11).Assembly.Location;
            VisualizerAttributeInjector injector = new VisualizerAttributeInjector(visualizerLocation);

            injector.MapTypesFromAssembly(_thisAssemblyLocation);
            injector.SaveDebuggerVisualizer(debuggerVisualizerTargetName);

            AssemblyDefinition generatedAssembly = AssemblyDefinition.ReadAssembly(debuggerVisualizerTargetName);
            IEnumerable<Type> currentAssemblytypes = 
                GetType()
                .Assembly
                .GetTypes()
                .Where(typeDef => typeDef.IsPublic && !typeDef.IsInterface && !typeDef.IsAbstract);

            IEnumerable<string> mappedTypeNames = generatedAssembly
                .CustomAttributes
                .SelectMany(p =>
                    p.Properties.Where(n => n.Name.Equals("TargetTypeName"))
                    .Select(l => l.Argument.Value.ToString()));

            bool allTypesArePresent = currentAssemblytypes
                .All(type =>
                    mappedTypeNames.Any(mappedType => mappedType.Equals(type.AssemblyQualifiedName)));

            Assert.IsTrue(allTypesArePresent);
        }
    }
}
