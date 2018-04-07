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
using System.Linq;
using System.Runtime.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace BridgeVs.SInject.Reflection
{
    internal static class ReflectionUtils
    {
        /// <summary>
        /// Adds the default constructor. For Binary Deserialization for a class that inherits from a types that implements
        /// ISerializable interface must override the constructor this signature:
        /// <code>
        /// protected Class ( SerializationInfo info,	 StreamingContext context  ) //On the base class
        ///
        /// public CTOR(SerializationInfo info,	 StreamingContext context) : base(info,context)
        /// </code>
        /// </summary>
        /// <param name="this">The TypeDefinition.</param>
        public static void AddDefaultConstructor(this TypeDefinition @this)
        {
            TypeReference baseType = @this.BaseType;

            bool isDefaultConstructorToBeAdded =
               @this
               .BaseType
               .Resolve()
               .Interfaces
               .Any(reference => reference.InterfaceType.FullName == typeof(ISerializable).FullName);

            if (!isDefaultConstructorToBeAdded) return;

            if (@this.Module == null) return;

            Func<MethodDefinition, bool> findCtor = new Func<MethodDefinition, bool>(definition =>
                                                           definition.Name.Equals(".ctor") &&
                                                           definition.Parameters.Count == 2 &&
                                                           definition.Parameters.Any(
                                                               par =>
                                                               par.Name.Equals("info") || par.Name.Equals("context")));

            bool isDefaultConstructorAlreadyDefined = @this.Methods.Any(findCtor);

            if (isDefaultConstructorAlreadyDefined) return;

            const MethodAttributes methodAttributes =
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName;

            MethodReference serializableContructor = baseType.ReferenceMethod(findCtor);

            if (serializableContructor==null) return;

            TypeReference serializationInfo = @this.Module.Import(serializableContructor.Parameters[0].ParameterType);

            TypeReference streamingContext = @this.Module.Import(serializableContructor.Parameters[1].ParameterType);

            MethodDefinition serializationConstr = new MethodDefinition(".ctor", methodAttributes, @this.Module.TypeSystem.Void);
            ParameterDefinition par1 = new ParameterDefinition(serializableContructor.Parameters[0].Name, serializableContructor.Parameters[0].Attributes, serializationInfo);
            ParameterDefinition par2 = new ParameterDefinition(serializableContructor.Parameters[1].Name, serializableContructor.Parameters[1].Attributes, streamingContext);

            serializationConstr.Parameters.Add(par1);
            serializationConstr.Parameters.Add(par2);

            serializationConstr.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            serializationConstr.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            serializationConstr.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
            serializationConstr.Body.Instructions.Add(Instruction.Create(OpCodes.Call, @this.Module.Import(serializableContructor)));
            serializationConstr.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
            serializationConstr.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
            serializationConstr.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            @this.Methods.Add(serializationConstr);
        }

#if !NET40
        //public static class EnumExt
        //{
        /// <summary>
        /// Check to see if a flags enumeration has a specific flag set.
        /// </summary>
        /// <param name="variable">Flags enumeration to check</param>
        /// <param name="value">Flag to check for</param>
        /// <returns></returns>
        public static bool HasFlag(this Enum variable, Enum value)
        {
            if (variable == null)
                return false;

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // Not as good as the .NET 4 version of this function, but should be good enough
            if (!Enum.IsDefined(variable.GetType(), value))
            {
                throw new ArgumentException($"Enumeration type mismatch.  The flag is of type '{value.GetType()}', was expecting '{variable.GetType()}'.");
            }

            ulong num = Convert.ToUInt64(value);
            return ((Convert.ToUInt64(variable) & num) == num);
        }
    }
#endif
}