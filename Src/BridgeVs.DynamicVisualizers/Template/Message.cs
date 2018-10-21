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

using BridgeVs.DynamicVisualizers.Helper;
using BridgeVs.Shared.Options;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BridgeVs.DynamicVisualizers.Template
{
    [Serializable]
    internal class Message
    {
        public readonly string FileName;
        public readonly string TypeName;
        public readonly string TypeFullName;

        public readonly string TypeNamespace;
        public readonly string AssemblyQualifiedName;
        public readonly string AssemblyName;
        public readonly string TruckId;
        public readonly SerializationOption SerializationOption;
        public readonly List<string> ReferencedAssemblies = new List<string>(30);

        private readonly Regex _pattern1 = new Regex("[<]");
        private readonly Regex _pattern2 = new Regex("[>]");
        private readonly Regex _pattern3 = new Regex("[,]");
        private readonly Regex _pattern4 = new Regex("[`]");
        private readonly Regex _pattern5 = new Regex("[ ]");

        public Message(string truckId, SerializationOption serializationOption, Type type)
        {
            Type targetType = GetInterfaceTypeIfIsIterator(type);

            SerializationOption = serializationOption;
            FileName = $"{CalculateFileNameFromType(targetType)}_{truckId}.linq";
            TypeName = CalculateTypeNameFromType(targetType).Trim();
            TypeFullName = targetType.GetDisplayName(fullName: true);
            TypeNamespace = targetType.Namespace;
            AssemblyQualifiedName = targetType.AssemblyQualifiedName;
            AssemblyName = targetType.Assembly.GetName().Name;
            TruckId = truckId;
        }

        private string CalculateTypeNameFromType(Type type)
        {
            string targetTypeName = type.GetDisplayName(fullName: false);

            string typeName = _pattern1.Replace(targetTypeName, string.Empty);
            typeName = _pattern2.Replace(typeName, string.Empty);
            typeName = _pattern3.Replace(typeName, string.Empty);
            typeName = _pattern4.Replace(typeName, string.Empty);
            typeName = _pattern5.Replace(typeName, string.Empty);

            return typeName;
        }

        private static Type GetInterfaceTypeIfIsIterator(Type type)
        {
            string typeFullName = type.FullName;

            bool isObjectEnumerable = !string.IsNullOrEmpty(typeFullName) && typeFullName.Contains("System.Linq.Enumerable");

            if (!type.IsNestedPrivate || !type.Name.Contains("Iterator") || !isObjectEnumerable)
                return type;

            bool isBaseTypeEnumerable = type.BaseType == null || (!string.IsNullOrEmpty(type.BaseType.FullName) &&
                                        type.BaseType.FullName.Contains("Object"));
            //try with the base type now
            return isBaseTypeEnumerable ? type.GetInterface("IEnumerable`1") : type.BaseType.GetInterface("IEnumerable`1");
        }

        private string CalculateFileNameFromType(Type type)
        {
            string targetTypeFullName = type.GetDisplayName(false);

            string fileName = _pattern1.Replace(targetTypeFullName, "(");
            fileName = _pattern2.Replace(fileName, ")");

            return TypeNameHelper.RemoveSystemNamespaces(fileName);
        }

        public override string ToString()
        {
            return $"FileName: {FileName}"
                + Environment.NewLine
                + $"TypeFullName: {TypeFullName}"
                + Environment.NewLine
                + $"TypeName: {TypeName}"
                + Environment.NewLine
                + $"TypeNamespace: {TypeNamespace}"
                + Environment.NewLine
                + $"TruckId: {TruckId}"
                + Environment.NewLine
                + $"SerializationType: {SerializationOption}";
        }
    }
}
