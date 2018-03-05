#region License
// Copyright (c) 2013 - 2018 Giovanni Campo
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

namespace BridgeVs.SInject.Reflection
{ 
    internal static class TypeReferenceExt
    {
        private static MethodReference _CloneMethodWithDeclaringType(MethodDefinition methodDef, TypeReference declaringTypeRef)
        {
            if (!declaringTypeRef.IsGenericInstance || methodDef == null)
            {
                return methodDef;
            }

            var methodRef = new MethodReference(methodDef.Name, methodDef.ReturnType, declaringTypeRef)
            {
                CallingConvention = methodDef.CallingConvention,
                HasThis = methodDef.HasThis,
                ExplicitThis = methodDef.ExplicitThis
            };

            foreach (var paramDef in methodDef.Parameters)
            {
                methodRef.Parameters.Add(new ParameterDefinition(paramDef.Name, paramDef.Attributes, declaringTypeRef.Module.Import(paramDef.ParameterType)));
            }

            foreach (var genParamDef in methodDef.GenericParameters)
            {
                methodRef.GenericParameters.Add(new GenericParameter(genParamDef.Name, methodRef));
            }

            return methodRef;
        }

        public static MethodReference ReferenceMethod(this TypeReference typeRef, Func<MethodDefinition, bool> methodSelector)
        {
            return _CloneMethodWithDeclaringType(typeRef.Resolve().Methods.FirstOrDefault(methodSelector), typeRef);
        }

        public static MethodReference ReferenceMethod(this TypeReference typeRef, string methodName)
        {
            return ReferenceMethod(typeRef, m => m.Name == methodName);
        }

        public static MethodReference ReferenceMethod(this TypeReference typeRef, string methodName, int paramCount)
        {
            return ReferenceMethod(typeRef, m => m.Name == methodName && m.Parameters.Count == paramCount);
        } 

        public static FieldReference ReferenceField(this TypeReference typeRef, Func<FieldDefinition, bool> fieldSelector)
        {
            var fieldDef = typeRef.Resolve().Fields.FirstOrDefault(fieldSelector);
            if (!typeRef.IsGenericInstance || fieldDef == null)
            {
                return fieldDef;
            }

            return new FieldReference(fieldDef.Name, fieldDef.FieldType, typeRef);
        }

        public static FieldReference ReferenceField(this TypeReference typeRef, string fieldName)
        {
            return ReferenceField(typeRef, f => f.Name == fieldName);
        }

        private static MethodReference ReferencePropertyGetter(this TypeReference typeRef, Func<PropertyDefinition, bool> propertySelector)
        {
            var propDef = typeRef.Resolve().Properties.FirstOrDefault(propertySelector);
            if (propDef == null || propDef.GetMethod == null)
            {
                return null;
            }

            return _CloneMethodWithDeclaringType(propDef.GetMethod, typeRef);
        }

        public static MethodReference ReferencePropertyGetter(this TypeReference typeRef, string propertyName)
        {
            return ReferencePropertyGetter(typeRef, p => p.Name == propertyName);
        }

        private static MethodReference ReferencePropertySetter(this TypeReference typeRef, Func<PropertyDefinition, bool> propertySelector)
        {
            var propDef = typeRef.Resolve().Properties.FirstOrDefault(propertySelector);
            if (propDef == null || propDef.SetMethod == null)
            {
                return null;
            }

            return _CloneMethodWithDeclaringType(propDef.SetMethod, typeRef);
        }

        public static MethodReference ReferencePropertySetter(this TypeReference typeRef, string propertyName)
        {
            return ReferencePropertySetter(typeRef, p => p.Name == propertyName);
        }
    }
}