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
using System.IO;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace BridgeVs.Grapple.Serialization
{
    internal class JsonSerializer : SerializationHandler
    {
        private const int MaxDepth = 10;

        private readonly Newtonsoft.Json.JsonSerializer _jsonSerializer = new Newtonsoft.Json.JsonSerializer
        {
            MaxDepth = MaxDepth,
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };

        public override void Serialize<T>(Stream aStream, T objToSerialize)
        {
            using (BsonWriter sw = new BsonWriter(aStream))
            {
                _jsonSerializer.Serialize(sw, objToSerialize);
            }
        }

        public override byte[] Serialize<T>(T objToSerialize)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BsonWriter sw = new BsonWriter(stream))
            {
                _jsonSerializer.Serialize(sw, objToSerialize);
                return stream.ToArray();
            }
        }

        public override T Deserialize<T>(Stream aStream)
        {
            using (BsonReader reader = new BsonReader(aStream))
            {
                return _jsonSerializer.Deserialize<T>(reader);
            }
        }

        public override T Deserialize<T>(byte[] objToDeserialize)
        {
            using (MemoryStream stream = new MemoryStream(objToDeserialize))
            using (BsonReader br = new BsonReader(stream))
            {
                br.ReadRootValueAsArray = IsCollectionType(typeof (T));
                return _jsonSerializer.Deserialize<T>(br);
            }
        }


        public override object Deserialize(byte[] objToDeserialize, Type type = null)
        {

            using (MemoryStream stream = new MemoryStream(objToDeserialize))
            using (BsonReader sw = new BsonReader(stream))
            {
                return @type != null ? _jsonSerializer.Deserialize(sw, type) : _jsonSerializer.Deserialize(sw);
            }
        }

        static bool IsCollectionType(Type type)
        {
            return (type.GetInterface("ICollection") != null);
        }

        public override string ToString()
        {
            return "JsonSerializer";
        }
    }
}
