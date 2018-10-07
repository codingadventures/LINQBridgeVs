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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using BridgeVs.Build.Util;
using BridgeVs.Shared.Common;
using BridgeVs.Shared.Logging;
using Mono.Cecil;
using Mono.Cecil.Pdb;

namespace BridgeVs.Build
{
    /// <summary>
    /// Type of serialization to enable
    /// </summary>
    [Flags]
    public enum SerializationTypes
    {
        /// <summary>
        /// BinarySerialization
        /// </summary>
        BinarySerialization = 0x01,
        /// <summary>
        /// DataContractSerialization
        /// </summary>
        DataContractSerialization = 0x02
    }

    /// <summary>
    /// Type of the Patch for Debug or Release mode.
    /// </summary>
    public enum PatchMode
    {
        /// <summary>
        /// The debug
        /// </summary>
        Debug,
        /// <summary>
        /// The release
        /// </summary>
        Release
    }

    /// <summary>
    /// This class inject the Serializable Attribute to all public class types in a given assembly, and adds
    /// default deserialization constructor for those type which implement ISerializable interface
    /// </summary>
    internal class SInjection
    {
        #region [ Private Properties ]
        private readonly string _assemblyLocation;
        private readonly AssemblyDefinition _assemblyDefinition;
        private static readonly Func<string, bool> IsSystemAssembly = name => name.Contains("Microsoft") || name.Contains("System") || name.Contains("mscorlib");

        private const string Marker = "SInjected";
        private readonly string _snkCertificatePath;

        /// <summary>
        /// 
        /// </summary>
        public string SInjectVersion
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi.FileVersion;
            }
        }

        private bool? _snkFileExists;
        private bool SnkFileExists
        {
            get
            {
                if (_snkFileExists.HasValue) return _snkFileExists.Value;
                _snkFileExists = File.Exists(_snkCertificatePath);
                return _snkFileExists.Value;
            }
        }

        private string PdbName => Path.ChangeExtension(_assemblyLocation, "pdb");

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SInject"/> class.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="snkCertificatePath">The location of snk certificate</param>
        /// <exception cref="System.Exception"></exception>
        public SInjection(string assemblyLocation, string snkCertificatePath = null)
        {
            _assemblyLocation = assemblyLocation;
            _snkCertificatePath = snkCertificatePath;
         
            Log.Write("Assembly being Injected {0}", assemblyLocation);

            if (!File.Exists(assemblyLocation))
                throw new Exception($"Assembly at location {assemblyLocation} doesn't exist");

            _assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyLocation, GetReaderParameters());
        }
        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Patches the loaded assembly enabling the specified Serialization type.
        /// </summary>
        /// <param name="types">The Serialization Type.</param>
        /// <exception cref="System.Exception"></exception>
        public bool Patch(SerializationTypes types)
        {
            bool alreadySInjected = CheckIfAlreadySInjected();

            if (alreadySInjected)
            {
                Log.Write("Assembly already SInjected");

                return true;
            }

            List<TypeDefinition> typeToInjects = GetTypesToInject().ToList();

            InjectSerialization(typeToInjects);

            AddSinjectionAttribute();

            bool success = WriteAssembly();
            Log.Write(success
                    ? "Assembly {0} has been correctly Injected"
                    : "Assembly {0} was not correctly Injected",
                _assemblyDefinition.FullName);
            return success;
        }

        /// <summary>
        /// Gets the serializable types in the current assembly.
        /// </summary>
        /// <returns> returns a name list of serializable types</returns>
        public IEnumerable<string> GetSerializableTypes()
        {
            return
           _assemblyDefinition
               .MainModule
               .Types
               .Where(typeInAssembly => typeInAssembly.IsSerializable)
               .Select(definition => definition.FullName);
        }

        #endregion

        #region [ Private Methods ]

        private static void InjectSerialization(ICollection<TypeDefinition> typeToInjects)
        {
            if (typeToInjects.Count == 0)
                return;

            foreach (TypeDefinition typeInAssembly in typeToInjects)
            {
                try
                {
                    typeInAssembly.AddDefaultConstructor();

                    //if (!types.HasFlag(SerializationTypes.BinarySerialization)) continue;

                    typeInAssembly.IsSerializable = true;

                    //This disable the serialization of non serializable property belonging to Microsoft framework
                    //it acts directly on the backing field of the prop marking it as NonSerialized.
                    SetNonSerializedFields(typeInAssembly);
                }
                catch (Exception e)
                {
                    Log.Write(e, $"Type {typeInAssembly.FullName} wasn't marked as Serializable");
                }
            }
        }

        private IEnumerable<TypeDefinition> GetTypesToInject()
        {
            try
            {
                return _assemblyDefinition
                    .MainModule
                    .Types
                    .Where(
                        typeInAssembly =>
                            !typeInAssembly.IsInterface
                            && (typeInAssembly.IsClass || typeInAssembly.IsAnsiClass)
                            && typeInAssembly.BaseType != null
                    );
            }
            catch (Exception e)
            {
                Log.Write(e, "Error while iterating types to inject");
                throw;
            }
        }

        private void AddSinjectionAttribute()
        {
            try
            {
                TypeReference stringType = _assemblyDefinition.MainModule.TypeSystem.String;
                AssemblyNameReference corlib = (AssemblyNameReference)_assemblyDefinition.MainModule.TypeSystem.CoreLibrary;

                AssemblyDefinition system =
                    _assemblyDefinition.MainModule.AssemblyResolver.Resolve(
                        new AssemblyNameReference("System", corlib.Version)
                        {
                            PublicKeyToken = corlib.PublicKeyToken,
                        });

                TypeDefinition generatedCodeAttribute = system.MainModule.GetType("System.CodeDom.Compiler.GeneratedCodeAttribute");

                MethodDefinition generatedCodeCtor = generatedCodeAttribute.Methods.First(m => m.IsConstructor && m.Parameters.Count == 2);

                CustomAttribute result = new CustomAttribute(_assemblyDefinition.MainModule.ImportReference(generatedCodeCtor));
                result.ConstructorArguments.Add(new CustomAttributeArgument(stringType, Marker));
                result.ConstructorArguments.Add(new CustomAttributeArgument(stringType, SInjectVersion));

                _assemblyDefinition.MainModule.Assembly.CustomAttributes.Add(result);
            }
            catch (Exception e)
            {
                Log.Write(e, "Error while adding Sinjection attribute to the assembly");
                throw;
            }
        }

        private bool CheckIfAlreadySInjected()
        {
            return _assemblyDefinition.HasCustomAttributes
                   && _assemblyDefinition.CustomAttributes
                       .Any(attribute => attribute.HasConstructorArguments
                                         &&
                                         attribute.ConstructorArguments
                                             .Count(
                                                 argument =>
                                                     argument.Value.Equals(Marker) ||
                                                     argument.Value.Equals(SInjectVersion)) == 2);

        }

        private ReaderParameters GetReaderParameters()
        {
            DefaultAssemblyResolver assemblyResolver = new DefaultAssemblyResolver();
            string assemblyLocation = Path.GetDirectoryName(_assemblyLocation);
            assemblyResolver.AddSearchDirectory(assemblyLocation);

            ReaderParameters readerParameters = new ReaderParameters
            {
                AssemblyResolver = assemblyResolver,
                InMemory = true,
                ReadingMode = ReadingMode.Immediate
            };

            if (!File.Exists(PdbName))
                 return readerParameters;
            
            PdbReaderProvider symbolReaderProvider = new PdbReaderProvider();
            readerParameters.SymbolReaderProvider = symbolReaderProvider;

            return readerParameters;
        }

        private WriterParameters GetWriterParameters()
        {
            WriterParameters writerParameters = new WriterParameters();

            if (File.Exists(PdbName))
            {
                writerParameters.SymbolWriterProvider = new PdbWriterProvider();
                writerParameters.WriteSymbols = true;
            }

            if (string.IsNullOrEmpty(_snkCertificatePath) || !SnkFileExists)
                return writerParameters;

            using (FileStream file = File.OpenRead(_snkCertificatePath))
            {
                writerParameters.StrongNameKeyPair = new StrongNameKeyPair(file);
            }

            return writerParameters;
        }

        private static void SetNonSerializedFields(TypeDefinition typeDefinition)
        {
            IEnumerable<FieldDefinition> fields = typeDefinition.Fields
               .Where(field =>
               {
                   TypeDefinition fieldType = field.FieldType.Resolve();
                   return fieldType != null
                          && !fieldType.IsInterface //is interface is not checked by the FormatterService
                          && !fieldType.IsSerializable
                          && IsSystemAssembly(fieldType.FullName)
                          && !fieldType.IsPrimitive;
               });

            foreach (FieldDefinition fieldDefinition in fields)
                fieldDefinition.IsNotSerialized = true;
        }

        private bool WriteAssembly()
        {
            const int retry = 3;

            if (string.IsNullOrEmpty(_snkCertificatePath) || !SnkFileExists)
            {
                _assemblyDefinition.Name.HasPublicKey = false;
                _assemblyDefinition.Name.PublicKey = new byte[0];
                _assemblyDefinition.MainModule.Attributes &= ~ModuleAttributes.StrongNameSigned;
            }
            for (int i = 0; i < retry; i++)
            {
                try
                {
                    using (FileStream s = new FileStream(_assemblyLocation, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        _assemblyDefinition.Write(s, GetWriterParameters());
                       
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Log.Write(e, $"Error Saving Assembly {_assemblyLocation} - Attempt #{i}");

                    Thread.Sleep(25);
                }
            }

            return false;
        }
        #endregion
    }
}