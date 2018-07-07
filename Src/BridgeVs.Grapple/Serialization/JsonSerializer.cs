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
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Serialization;

namespace BridgeVs.Grapple.Serialization
{
    internal class JsonSerializer : IServiceSerializer
    {
        private const int MaxDepth = 8;

        private readonly Newtonsoft.Json.JsonSerializer _jsonSerializer = new Newtonsoft.Json.JsonSerializer
        {
            MaxDepth = MaxDepth,
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            ObjectCreationHandling = ObjectCreationHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new DefaultContractResolver
            {
                IgnoreSerializableInterface = false,
                IgnoreSerializableAttribute = false,
            },
            TypeNameHandling = TypeNameHandling.All
        };

        public void Serialize<T>(Stream aStream, T objToSerialize)
        {
            using (BsonWriter sw = new BsonWriter(aStream))
            {
                _jsonSerializer.Serialize(sw, objToSerialize);
            }
        }

        public byte[] Serialize<T>(T objToSerialize)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BsonWriter sw = new BsonWriter(stream))
            {
                _jsonSerializer.Serialize(sw, objToSerialize);
                return stream.ToArray();
            }
        }

        public T Deserialize<T>(Stream aStream)
        {
            using (BsonReader reader = new BsonReader(aStream))
            {
                return _jsonSerializer.Deserialize<T>(reader);
            }
        }

        public T Deserialize<T>(byte[] objToDeserialize)
        {
            using (MemoryStream stream = new MemoryStream(objToDeserialize))
            using (BsonReader br = new BsonReader(stream))
            {
                //br.ReadRootValueAsArray = IsCollectionType(typeof (T));
                return _jsonSerializer.Deserialize<T>(br);
            }
        }

        public object Deserialize(byte[] objToDeserialize, Type type = null)
        {

            using (MemoryStream stream = new MemoryStream(objToDeserialize))
            using (BsonReader sw = new BsonReader(stream))
            {
                return @type != null ? _jsonSerializer.Deserialize(sw, type) : _jsonSerializer.Deserialize(sw);
            }
        }

        public override string ToString()
        {
            return "Json.NET Serializer";
        }
    }
}