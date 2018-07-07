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
using System.Collections;
using System.Linq;
using System.Reflection;
using BridgeVs.Grapple.Contracts;
using BridgeVs.Grapple.Serialization;

namespace BridgeVs.Grapple.Grapple
{
    internal class BinaryGrapple : IGrapple
    {
        private readonly IServiceSerializer _serviceSerializer;
        public BinaryGrapple(IServiceSerializer serviceSerializer)
        {
            _serviceSerializer = serviceSerializer;
        }

        #region [ IGrapple Methods ]
        public Sand Grab<T>(T item)
        {
            Type @type = item.GetType();
            byte[] byteStream = { };
            string serializedType = @type.AssemblyQualifiedName;

            if (@type.IsNestedPrivate && @type.Name.Contains("Iterator") &&
                @type.FullName.Contains("System.Linq.Enumerable") && item is IEnumerable)
            {
                MethodInfo toList = typeof(Enumerable).GetMethod("ToList");
                Type baseType = item.GetType().BaseType;
                if (baseType == null)
                    return new Sand { Type = serializedType, Content = byteStream, SerializationMethod = _serviceSerializer.ToString() };

                MethodInfo constructedToList = toList.MakeGenericMethod(baseType.GetGenericArguments()[0]);
                object castList = constructedToList.Invoke(null, new object[] { item });

                serializedType = castList.GetType().AssemblyQualifiedName;
                byteStream = _serviceSerializer.Serialize(castList);
            }
            else
            {
                byteStream = _serviceSerializer.Serialize(item);
            }

            return new Sand { Type = serializedType, Content = byteStream };
        }

        public T Release<T>(byte[] item)
        {
            return _serviceSerializer.Deserialize<T>(item);
        }

        public object Release(byte[] item, string type)
        {
            return _serviceSerializer.Deserialize(item, Type.GetType(type));
        }
        #endregion
    }
}