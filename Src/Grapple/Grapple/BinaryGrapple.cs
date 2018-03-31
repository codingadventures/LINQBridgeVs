#region License
// Copyright (c) 2013 Coding Adventures
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
        #region [ IGrapple Methods ]
        public Tuple<Type, byte[]> Grab<T>(T item)
        {
            Type @type = item.GetType();
            byte[] byteStream = { };
            Type serializedType = @type;

            if (@type.IsNestedPrivate && @type.Name.Contains("Iterator") &&
                @type.FullName.Contains("System.Linq.Enumerable") && item is IEnumerable)
            {
                MethodInfo toList = typeof(Enumerable).GetMethod("ToList");
                Type baseType = item.GetType().BaseType;
                if (baseType == null) return new Tuple<Type, byte[]>(serializedType, byteStream);

                MethodInfo constructedToList = toList.MakeGenericMethod(baseType.GetGenericArguments()[0]);
                object castList = constructedToList.Invoke(null, new object[] { item });

                serializedType = castList.GetType();
                IServiceSerializer serviceSerializer = FactorySerializer.CreateServiceSerializer(serializedType);
                byteStream = serviceSerializer.Serialize(castList);
            }
            else
            {
                IServiceSerializer serviceSerializer = FactorySerializer.CreateServiceSerializer(serializedType);

                byteStream = serviceSerializer.Serialize(item);
            }

            return new Tuple<Type, byte[]>(serializedType, byteStream);
        }

        public T Release<T>(byte[] item)
        {
            IServiceSerializer serviceSerializer = FactorySerializer.CreateServiceSerializer(typeof(T));
            return serviceSerializer.Deserialize<T>(item);
        }

        public object Release(byte[] item, Type type)
        {
            IServiceSerializer serviceSerializer = FactorySerializer.CreateServiceSerializer(type);

            return serviceSerializer.Deserialize(item, type);
        }

        #endregion
    }
}