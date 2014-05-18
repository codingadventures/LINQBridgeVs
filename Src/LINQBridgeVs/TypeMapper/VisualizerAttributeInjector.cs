
#region License
// Copyright (c) 2013 Giovanni Campo
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
using System.Threading;
using LINQBridgeVs.Logging;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using Mono.Cecil.Rocks;

namespace LINQBridgeVs.TypeMapper
{
    /// <summary>
    /// This class injects the DebuggerVisualizerAttribute into a given assembly
    /// </summary>
    internal class VisualizerAttributeInjector : IDisposable
    {
        #region [ Private Properties ]

        private string _assemblyDescriptionAttributeValue;

        #region [ Static Readonly Func ]

        private static readonly Func<TypeDefinition, bool> AbstractTypesFilter
            = typeDef => typeDef.IsPublic && !typeDef.IsInterface && !typeDef.IsAbstract;

        private static readonly Func<TypeDefinition, string, bool> BaseTypeFilter
            = (typeDef, toCompare) => typeDef.BaseType != null && typeDef.BaseType.Name.Contains(toCompare);

        #endregion

        #region [ Fields ]

        private readonly AssemblyDefinition _debuggerVisualizerAssembly;
        private readonly WriterParameters _writerParameters = new WriterParameters { WriteSymbols = true, SymbolWriterProvider = new PdbWriterProvider() };
        private MethodDefinition _debuggerVisualizerAttributeCtor;
        private CustomAttributeArgument _customDebuggerVisualizerAttributeArgument;
        private CustomAttributeArgument _visualizerObjectSourceCustomAttributeArgument;
        #endregion

        #region [ Constants ]
        private const string SystemType = "System.Type";
        private const string SystemDiagnosticsDebuggerVisualizerAttribute = "System.Diagnostics.DebuggerVisualizerAttribute";

        private const string SystemReflectionAssemblyDescriptionAttribute = "System.Reflection.AssemblyDescriptionAttribute";
        #endregion

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizerAttributeInjector"/> class.
        /// </summary>
        /// <param name="assemblyDebuggerVisualizerLocation">The assembly debugger visualizer location.</param>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        public VisualizerAttributeInjector(string assemblyDebuggerVisualizerLocation)
        {
            if (!File.Exists(assemblyDebuggerVisualizerLocation))
                throw new FileNotFoundException(string.Format("Assembly doesn't exist at location {0}", assemblyDebuggerVisualizerLocation));

            _debuggerVisualizerAssembly = AssemblyDefinition.ReadAssembly(assemblyDebuggerVisualizerLocation, GetReaderParameters(assemblyDebuggerVisualizerLocation));

            InitializeDebuggerAssembly();
        }

        private void InitializeDebuggerAssembly()
        {
            TypeReference systemTypeReference;

            _debuggerVisualizerAssembly.MainModule.TryGetTypeReference(SystemType, out systemTypeReference);

            var debuggerVisualizerAttributeTypeReference = _debuggerVisualizerAssembly.MainModule.GetType(SystemDiagnosticsDebuggerVisualizerAttribute, true);

            var customDebuggerVisualizerTypeReference = _debuggerVisualizerAssembly
                .MainModule
                .Types
                .SingleOrDefault(definition => BaseTypeFilter(definition, "DialogDebuggerVisualizer"));
            var customDebuggerVisualizerObjectSourceTypeReference = _debuggerVisualizerAssembly
                .MainModule
                .Types
                .SingleOrDefault(definition => BaseTypeFilter(definition, "VisualizerObjectSource"));

            _debuggerVisualizerAttributeCtor = debuggerVisualizerAttributeTypeReference.Resolve().GetConstructors().SingleOrDefault(definition =>
                                                                       definition.Parameters.Count == 2 &&
                                                                       definition.Parameters.All(
                                                                           parameterDefinition =>
                                                                           parameterDefinition.ParameterType.Name.Equals(systemTypeReference.Name)));

            _customDebuggerVisualizerAttributeArgument = new CustomAttributeArgument(systemTypeReference, customDebuggerVisualizerTypeReference);
            _visualizerObjectSourceCustomAttributeArgument = new CustomAttributeArgument(systemTypeReference, customDebuggerVisualizerObjectSourceTypeReference);


            var assemblyDescriptionAttribute = _debuggerVisualizerAssembly.CustomAttributes.FirstOrDefault(p => p.AttributeType.FullName.Equals(SystemReflectionAssemblyDescriptionAttribute));

            if (assemblyDescriptionAttribute != null)
                _assemblyDescriptionAttributeValue = assemblyDescriptionAttribute.ConstructorArguments[0].Value.ToString();
            else
                throw new Exception(string.Format("Assembly Description Attribute not set in the Assembly {0}", _debuggerVisualizerAssembly));
        }

        private void MapType(TypeReference typeReference)
        {
            var scope = string.Empty;

            switch (typeReference.Scope.MetadataScopeType)
            {
                case MetadataScopeType.AssemblyNameReference:
                    scope = FromCILToTypeName(typeReference.FullName) + ", " + typeReference.Scope;
                    break;
                case MetadataScopeType.ModuleReference:
                    break;
                case MetadataScopeType.ModuleDefinition:
                    var moduleDefinition = typeReference.Scope as ModuleDefinition;
                    if (moduleDefinition != null) scope = FromCILToTypeName(typeReference.FullName) + ", " + moduleDefinition.Assembly;
                    break;
                default:
                    throw new Exception(string.Format("Assembly Scope Null. Check assembly {0}", typeReference.FullName));
            }

            var customAttribute = new CustomAttribute(_debuggerVisualizerAssembly.MainModule.Import(_debuggerVisualizerAttributeCtor));

            var targetType = new CustomAttributeArgument(_debuggerVisualizerAssembly.MainModule.TypeSystem.String, scope);
            var descriptionType = new CustomAttributeArgument(_debuggerVisualizerAssembly.MainModule.TypeSystem.String, _assemblyDescriptionAttributeValue);

            var targetTypeProperty = new CustomAttributeNamedArgument("TargetTypeName", targetType);
            var descriptionTypeProperty = new CustomAttributeNamedArgument("Description", descriptionType);


            customAttribute.ConstructorArguments.Add(_customDebuggerVisualizerAttributeArgument);
            customAttribute.ConstructorArguments.Add(_visualizerObjectSourceCustomAttributeArgument);

            customAttribute.Properties.Add(targetTypeProperty);
            customAttribute.Properties.Add(descriptionTypeProperty);

            _debuggerVisualizerAssembly.CustomAttributes.Add(customAttribute);
        }

        /// <summary>
        /// Maps a System.Type.
        /// </summary>
        /// <param name="type">The type.</param>
        public void MapType(Type type)
        {
            var typeReference = _debuggerVisualizerAssembly.MainModule.Import(type);

            MapType(typeReference);
        }

        /// <summary>
        /// Maps the types from a given assembly.
        ///  <param name="assemblyToMapTypesFromLocation"></param>
        /// </summary>
        public void MapTypesFromAssembly(string assemblyToMapTypesFromLocation)
        {
            if (!File.Exists(assemblyToMapTypesFromLocation))
                throw new FileNotFoundException(string.Format("Assembly doesn't exist at location {0}", assemblyToMapTypesFromLocation));

            var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyToMapTypesFromLocation, GetReaderParameters(assemblyToMapTypesFromLocation));

            assemblyDefinition
               .MainModule
               .Types
               .Where(AbstractTypesFilter)
               .ForEach(MapType);
        }

        /// <summary>
        /// Saves the injected debugger visualizer.
        /// </summary> 
        /// <param name="debuggerVisualizerDst"> The debugger visualizer destination path </param>
        /// <param name="references">The Assembly references</param>
        public void SaveDebuggerVisualizer(string debuggerVisualizerDst, IEnumerable<string> references = null)
        {
            var success = false;
            const int maxCount = 3;
            var i = 0;
            var fileName = Path.GetFileNameWithoutExtension(debuggerVisualizerDst);

            //I would have used a goto....can't forget my professor's quote: "Each GOTO can be superseeded by a Repeat Until...God bless Pascal!"
            while (!success && i++ < maxCount)
            {
                try
                {
                    _debuggerVisualizerAssembly.Name.Name = fileName;
                    _debuggerVisualizerAssembly.Write(debuggerVisualizerDst, _writerParameters);
                    success = true;
                }
                catch (IOException ioe)
                {
                    Log.Write(ioe);
                    Thread.Sleep(75);
                }
            }

            if (references != null)
                DeployReferences(references, debuggerVisualizerDst);
        }

        private static void DeployReferences(IEnumerable<string> references, string location)
        {
            references.ForEach(reference => File.Copy(reference, location, true));
        }

        /*
                /// <summary>
                /// Saves the debugger visualizer.
                /// </summary>
                /// <param name="aStream">A stream.</param>
                public void SaveDebuggerVisualizer(Stream aStream)
                { 
                    _debuggerVisualizerAssembly.Write(aStream, _writerParameters);
                }
        */

        private static ReaderParameters GetReaderParameters(string assemblyPath)
        {
            var assemblyResolver = new DefaultAssemblyResolver();
            var assemblyLocation = Path.GetDirectoryName(assemblyPath);
            assemblyResolver.AddSearchDirectory(assemblyLocation);

            var readerParameters = new ReaderParameters { AssemblyResolver = assemblyResolver };

            var pdbName = Path.ChangeExtension(assemblyPath, "pdb");
            if (!File.Exists(pdbName)) return readerParameters;
            var symbolReaderProvider = new PdbReaderProvider();
            readerParameters.SymbolReaderProvider = symbolReaderProvider;
            readerParameters.ReadSymbols = true;

            return readerParameters;
        }

        /// <summary>
        /// Fixes the cecil type reference. Importing a private nested class like 
        /// System.Linq.Enumerable+WhereListIterator`1 into Mono.Cecil changes the symbol
        /// that refers to a private class to a slash '/'
        /// This helper method change it back to a plus
        /// </summary>
        /// <param name="fullName">The full name.</param>
        private static string FromCILToTypeName(string fullName)
        {
            return fullName.Replace('/', '+');
        }

        public void Dispose()
        {

        }
    }
}
