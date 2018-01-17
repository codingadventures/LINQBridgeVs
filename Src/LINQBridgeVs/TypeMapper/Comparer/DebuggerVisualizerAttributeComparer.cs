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

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace LINQBridgeVs.TypeMapper.Comparer
{
    internal class DebuggerVisualizerAttributeComparer :
        IEqualityComparer<CustomAttribute>
    {
        private const string TargetTypeProperty = "Target";
        private const string TargetTypeNameProperty = "TargetTypeName";

        public bool Equals(CustomAttribute x, CustomAttribute y)
        {
            if (!x.AttributeType.FullName.Equals(y.AttributeType.FullName)) return false;

            if (!x.Properties.Any(argument => argument.Name.Equals(TargetTypeProperty)) && !y.Properties.Any(argument => argument.Name.Equals(TargetTypeProperty)))
                if ((!x.Properties.Any(argument => argument.Name.Equals(TargetTypeNameProperty)) && !y.Properties.Any(argument => argument.Name.Equals(TargetTypeNameProperty))))
                    return false;

            var typeX =
                x.Properties.SingleOrDefault(argument => argument.Name.Equals(TargetTypeProperty)).Argument.Value as TypeReference;
            var typeY =
                y.Properties.SingleOrDefault(argument => argument.Name.Equals(TargetTypeProperty)).Argument.Value as TypeReference;

            var typeNameX =
                x.Properties.SingleOrDefault(argument => argument.Name.Equals(TargetTypeNameProperty)).Argument.Value;
            var typeNameY =
                y.Properties.SingleOrDefault(argument => argument.Name.Equals(TargetTypeNameProperty)).Argument.Value;

            if (typeX != null && typeY != null)
                return typeX.FullName == typeY.FullName;


            return typeNameX.Equals(typeNameY);

        }

        public int GetHashCode(CustomAttribute obj)
        {
            var hash = 0;

            var typeReference =
                obj.Properties.SingleOrDefault(argument => argument.Name.Equals(TargetTypeProperty))
                   .Argument.Value as TypeReference;


            if (typeReference == null)
            {
                var typeValue = obj.Properties.SingleOrDefault(argument => argument.Name.Equals(TargetTypeNameProperty)).Argument.Value as string;
                if (typeValue != null) hash = typeValue.GetHashCode();
            }
            else
            {
                hash = typeReference.FullName.GetHashCode();
            }



            return hash;
        }


    }
}
