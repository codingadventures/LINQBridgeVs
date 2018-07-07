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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BridgeVs.Build.TypeMapper;
using BridgeVs.DynamicVisualizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;

namespace BridgeVs.Build.Test
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
        [TestCategory("IntegrationTest")]
        public void Visualizer_MapType_Should_Create_DegguberVisualizerAttribute_OfType_This_In_Target_Assembly()
        {
            string debuggerVisualizerTargetName = Path.Combine(_thisAssemblyDirectoryName, "TestA.dll");
            string visualizerLocation = typeof(DynamicObjectSource).Assembly.Location;
            VisualizerAttributeInjector injector = new VisualizerAttributeInjector(visualizerLocation, "11.0");
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
        [TestCategory("IntegrationTest")]
        public void Visualizer_MapType_Should_Create_DegguberVisualizerAttribute_OfType_IList_In_Target_Assembly()
        {
            string debuggerVisualizerTargetName = Path.Combine(_thisAssemblyDirectoryName, "TestB.dll");
            string visualizerLocation = typeof(DynamicObjectSource).Assembly.Location;
            VisualizerAttributeInjector injector = new VisualizerAttributeInjector(visualizerLocation, "11.0");
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
        [TestCategory("IntegrationTest")]
        public void Visualizer_MapTypeFromAssembly_Should_Map_All_Types()
        {
            string debuggerVisualizerTargetName = Path.Combine(_thisAssemblyDirectoryName, "TestC.dll");
            string visualizerLocation = typeof(DynamicObjectSource).Assembly.Location;
            VisualizerAttributeInjector injector = new VisualizerAttributeInjector(visualizerLocation, "11.0");

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