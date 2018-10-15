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
using Mono.Cecil;

namespace BridgeVs.Build.Util
{ 
    internal static class TypeReferenceExtension
    {
        private static MethodReference _CloneMethodWithDeclaringType(MethodDefinition methodDef, TypeReference declaringTypeRef)
        {
            if (!declaringTypeRef.IsGenericInstance || methodDef == null)
            {
                return methodDef;
            }

            MethodReference methodRef = new MethodReference(methodDef.Name, methodDef.ReturnType, declaringTypeRef)
            {
                CallingConvention = methodDef.CallingConvention,
                HasThis = methodDef.HasThis,
                ExplicitThis = methodDef.ExplicitThis
            };

            foreach (ParameterDefinition paramDef in methodDef.Parameters)
            {
                methodRef.Parameters.Add(new ParameterDefinition(paramDef.Name, paramDef.Attributes, declaringTypeRef.Module.Import(paramDef.ParameterType)));
            }

            foreach (GenericParameter genParamDef in methodDef.GenericParameters)
            {
                methodRef.GenericParameters.Add(new GenericParameter(genParamDef.Name, methodRef));
            }

            return methodRef;
        }

        public static MethodReference ReferenceMethod(this TypeReference typeRef, Func<MethodDefinition, bool> methodSelector)
        {
            return _CloneMethodWithDeclaringType(typeRef.Resolve().Methods.FirstOrDefault(methodSelector), typeRef);
        }

        public static FieldReference ReferenceField(this TypeReference typeRef, Func<FieldDefinition, bool> fieldSelector)
        {
            FieldDefinition fieldDef = typeRef.Resolve().Fields.FirstOrDefault(fieldSelector);
            if (!typeRef.IsGenericInstance || fieldDef == null)
            {
                return fieldDef;
            }

            return new FieldReference(fieldDef.Name, fieldDef.FieldType, typeRef);
        }

        private static MethodReference ReferencePropertyGetter(this TypeReference typeRef, Func<PropertyDefinition, bool> propertySelector)
        {
            PropertyDefinition propDef = typeRef.Resolve().Properties.FirstOrDefault(propertySelector);
            if (propDef == null || propDef.GetMethod == null)
            {
                return null;
            }

            return _CloneMethodWithDeclaringType(propDef.GetMethod, typeRef);
        }

        private static MethodReference ReferencePropertySetter(this TypeReference typeRef, Func<PropertyDefinition, bool> propertySelector)
        {
            PropertyDefinition propDef = typeRef.Resolve().Properties.FirstOrDefault(propertySelector);
            if (propDef == null || propDef.SetMethod == null)
            {
                return null;
            }

            return _CloneMethodWithDeclaringType(propDef.SetMethod, typeRef);
        }
    }
}