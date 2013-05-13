using System;
using System.Collections.Generic;
using System.IO;
using Bridge.TypeMapper.Comparer;
using Mono.Cecil.Rocks;
using Mono.Cecil.Pdb;
using Mono.Cecil;
using System.Linq;

namespace Bridge.TypeMapper
{
    internal class VisualizerAttributeInjector
    {
        private readonly string _visualizerDescriptionName;

        #region [ Private Properties ]

        #region [ Static Readonly Func ]

        private static readonly Func<TypeDefinition, bool> AbstractTypesFilter
            = typeDef => typeDef.IsPublic && !typeDef.IsInterface && !typeDef.IsAbstract;

        private static readonly Func<TypeDefinition, string, bool> BaseTypeFilter
            = (typeDef, toCompare) => typeDef.BaseType != null && typeDef.BaseType.Name.Contains(toCompare);

        //private static readonly Func<TypeDefinition, string, bool> TypeFilter
        //    = (typeDef, toCompare) => typeDef.FullName.Contains(toCompare);

        #endregion

        #region [ Fields ]
        private readonly AssemblyDefinition _assemblyToMap;
        private readonly AssemblyDefinition _assembly;
        private readonly WriterParameters _writerParameters = new WriterParameters { WriteSymbols = true, SymbolWriterProvider = new PdbWriterProvider() };

        #endregion

        #region [ Constants ]
        private const string SystemType = "System.Type";
        private const string SystemDiagnosticsDebuggerVisualizerAttribute = "System.Diagnostics.DebuggerVisualizerAttribute";
        
        #endregion

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizerAttributeInjector"/> class.
        /// </summary>
        /// <param name="assemblyDebuggerVisualizerLocation">The assembly debugger visualizer location.</param>
        /// <param name="assemblyToMap">The assembly to map.</param>
        /// <param name="visualizerDescriptionName"></param>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        public VisualizerAttributeInjector(string assemblyDebuggerVisualizerLocation, string assemblyToMap, string visualizerDescriptionName)
        {
            _visualizerDescriptionName = visualizerDescriptionName;

            if (!File.Exists(assemblyDebuggerVisualizerLocation))
                throw new FileNotFoundException(string.Format("Assembly doesn't exist at location {0}", assemblyDebuggerVisualizerLocation));

            if (!File.Exists(assemblyToMap))
                throw new FileNotFoundException(string.Format("Assembly doesn't exist at location {0}", assemblyToMap));

            _assembly = AssemblyDefinition.ReadAssembly(assemblyDebuggerVisualizerLocation, GetReaderParameters(assemblyDebuggerVisualizerLocation));
            _assemblyToMap = AssemblyDefinition.ReadAssembly(assemblyToMap, GetReaderParameters(assemblyToMap));
        }

        public void MapSystemType(Type type)
        {
            TypeReference systemType;

            _assembly.MainModule.TryGetTypeReference(SystemType, out systemType);

            var debuggerAttributeType = _assembly.MainModule.Import(typeof(System.Diagnostics.DebuggerVisualizerAttribute));

            TypeReference
              customDebuggerVisualizerType =
                      _assembly
                          .MainModule
                          .Types
                          .SingleOrDefault(definition => BaseTypeFilter(definition, "DialogDebuggerVisualizer")),
              customDebuggerVisualizerObjectSource =
                      _assembly
                      .MainModule
                      .Types
                      .SingleOrDefault(definition => BaseTypeFilter(definition, "VisualizerObjectSource"));


            var ctor = debuggerAttributeType.Resolve().GetConstructors().SingleOrDefault(definition =>
                                                                    definition.Parameters.Count == 2 &&
                                                                    definition.Parameters.All(
                                                                        parameterDefinition =>
                                                                        parameterDefinition.ParameterType.Name.Equals(systemType.Name)));


            var customAttribute = new CustomAttribute(_assembly.MainModule.Import(ctor));
            var customDebuggerVisualizer = new CustomAttributeArgument(systemType, customDebuggerVisualizerType);

            var visualizerObjectSource =
                new CustomAttributeArgument(systemType, customDebuggerVisualizerObjectSource);


            _assembly.MainModule.Import(type);

            var targetType = new CustomAttributeArgument(_assembly.MainModule.TypeSystem.String, type.AssemblyQualifiedName);
            var descriptionType = new CustomAttributeArgument(_assembly.MainModule.TypeSystem.String, _visualizerDescriptionName);
            var targetTypeProperty = new CustomAttributeNamedArgument("TargetTypeName", targetType);
            var descriptionTypeProperty = new CustomAttributeNamedArgument("Description", descriptionType);


            customAttribute.ConstructorArguments.Add(customDebuggerVisualizer);
            customAttribute.ConstructorArguments.Add(visualizerObjectSource);

            customAttribute.Properties.Add(targetTypeProperty);
            customAttribute.Properties.Add(descriptionTypeProperty);

            _assembly.CustomAttributes.Add(customAttribute);


        }

        /// <summary>
        /// Syncronizes the specified source assembly with the target.
        /// </summary>
        /// <param name="source">The source assembly to syncronize with the current opened .</param>
        public void SyncronizeMappedTypes(string source)
        {
            using (var streamSource = File.OpenRead(source))
                SyncronizeMappedTypes(streamSource);
        }

        /// <summary>
        /// Syncronizes the specified source assembly with the target.
        /// </summary>
        /// <param name="source">The source.</param>
        public void SyncronizeMappedTypes(Stream source)
        {
            var existingAssembly = AssemblyDefinition.ReadAssembly(source);


            //Import all the custom attributes from source to target
            var existingAttributes =
                existingAssembly
                .CustomAttributes
                //Get all the custom attributes from source of type DebuggerVisualizerAttribute
                .Where(
                    attribute => attribute.AttributeType.FullName.Equals(SystemDiagnosticsDebuggerVisualizerAttribute))
                   .ToList();

            var newAttributes =
                _assembly
                .CustomAttributes
                .Where(
                    attribute => attribute.AttributeType.FullName.Equals(SystemDiagnosticsDebuggerVisualizerAttribute))
                    .ToList()
                    ;

            //Get all the attributes from existingAssembly that are not in the current one (_assembly)
            //checking if the Target properties have the same Type 
            var typesToAdd = existingAttributes
                .Except(newAttributes, new DebuggerVisualizerAttributeComparer())
                .ToList();

            //Add to the current _assembly all the missing type already deployed in the source
            foreach (var customAttribute in typesToAdd)
            {
                var ctor = _assembly.MainModule.Import(customAttribute.Constructor);
                var newAtt = new CustomAttribute(ctor);
                customAttribute.ConstructorArguments.ToList().ForEach(newAtt.ConstructorArguments.Add);
                customAttribute.Properties.ToList().ForEach(newAtt.Properties.Add);


                _assembly.CustomAttributes.Add(newAtt);
            }
        }

        /// <summary>
        /// Maps the types from assembly.
        /// </summary>
        public void MapTypesFromAssembly( )
        {

            TypeReference systemType;
            
            _assembly.MainModule.TryGetTypeReference(SystemType, out systemType);

            var debuggerAttributeType = _assembly.MainModule.Import(typeof(System.Diagnostics.DebuggerVisualizerAttribute));


            TypeReference
                customDebuggerVisualizerType =
                        _assembly
                            .MainModule
                            .Types
                            .SingleOrDefault(definition => BaseTypeFilter(definition, "DialogDebuggerVisualizer")),
                customDebuggerVisualizerObjectSource =
                        _assembly
                        .MainModule
                        .Types
                        .SingleOrDefault(definition => BaseTypeFilter(definition, "VisualizerObjectSource"));


            var ctor = debuggerAttributeType.Resolve().GetConstructors().SingleOrDefault(definition =>
                                                                    definition.Parameters.Count == 2 &&
                                                                    definition.Parameters.All(
                                                                        parameterDefinition =>
                                                                        parameterDefinition.ParameterType.Name.Equals(systemType.Name)));

            //TODO this should be a search of already defined types
            var filteredTypes = _assemblyToMap
                .MainModule
                .Types
                .Where(AbstractTypesFilter);

            foreach (var type in filteredTypes)
            {
                var customAttribute = new CustomAttribute(_assembly.MainModule.Import(ctor));
                var customDebuggerVisualizer = new CustomAttributeArgument(systemType, customDebuggerVisualizerType);

                var visualizerObjectSource =
                    new CustomAttributeArgument(systemType, customDebuggerVisualizerObjectSource);
                 
                var targetType = new CustomAttributeArgument(systemType, _assembly.MainModule.Import(type));
                var descriptionType = new CustomAttributeArgument(_assembly.MainModule.TypeSystem.String, _visualizerDescriptionName);
                var targetTypeProperty = new CustomAttributeNamedArgument("Target", targetType);
                var descriptionTypeProperty = new CustomAttributeNamedArgument("Description", descriptionType);

                customAttribute.ConstructorArguments.Add(customDebuggerVisualizer);
                customAttribute.ConstructorArguments.Add(visualizerObjectSource);

                customAttribute.Properties.Add(targetTypeProperty);
                customAttribute.Properties.Add(descriptionTypeProperty);

                _assembly.CustomAttributes.Add(customAttribute);
            }
        }

        /// <summary>
        /// Saves the debugger visualizer.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="references">The Assembly references</param>
        public void SaveDebuggerVisualizer(string location, IEnumerable<string> references = null)
        {
           
            _assembly.Write(location, _writerParameters);
            if (references != null)
                DeployReferences(references, location);
        }

        private static void DeployReferences(IEnumerable<string> references, string location)
        {
            foreach (var reference in references)
            {
                File.Copy(reference, location, true);
            }

        }

        /// <summary>
        /// Saves the debugger visualizer.
        /// </summary>
        /// <param name="aStream">A stream.</param>
        public void SaveDebuggerVisualizer(Stream aStream)
        {
           
            _assembly.Write(aStream, _writerParameters);
        }

        private static ReaderParameters GetReaderParameters(string assemblyPath)
        {
            var assemblyResolver = new DefaultAssemblyResolver();
            var assemblyLocation = Path.GetDirectoryName(assemblyPath);
            assemblyResolver.AddSearchDirectory(assemblyLocation);

            var readerParameters = new ReaderParameters { AssemblyResolver = assemblyResolver };

            var pdbName = Path.ChangeExtension(assemblyPath, "pdb");
            if (File.Exists(pdbName))
            {
                var symbolReaderProvider = new PdbReaderProvider();
                readerParameters.SymbolReaderProvider = symbolReaderProvider;
                readerParameters.ReadSymbols = true;

            }

            return readerParameters;
        }
    }
}
