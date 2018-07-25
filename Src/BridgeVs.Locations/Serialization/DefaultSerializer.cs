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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using BridgeVs.Shared.Logging;

namespace BridgeVs.Shared.Serialization
{
    /// <summary>
    /// Default Binary Serializer. It uses the BinaryFormatter and it expects the types to be marked as Serializable
    /// </summary>
    public class DefaultSerializer : IServiceSerializer
    {
        private readonly BinaryFormatter _formatter = new BinaryFormatter();

        public IServiceSerializer Next { get; set; }

        /// <summary>
        /// Serializes the specified a stream.
        /// </summary>
        /// <typeparam name="T">Type T to be serialized</typeparam>
        /// <param name="aStream">A stream.</param>
        /// <param name="objToSerialize">The object to serialize.</param>
        public void Serialize<T>(Stream aStream, T objToSerialize)
        {
            aStream.Seek(0, SeekOrigin.Begin);
            Log.Write("Serialization Used: " + ToString());

            try
            {
                _formatter.Serialize(aStream, objToSerialize);
            }
            catch (SerializationException)
            {
                Log.Write("Binary Serialization unsuccessful");
                throw;
            }
        }

        /// <summary>
        /// Serializes the specified object to serialize.
        /// </summary>
        /// <typeparam name="T">Type T to be serialized</typeparam>
        /// <param name="objToSerialize">The object to serialize.</param>
        /// <returns></returns>
        public byte[] Serialize<T>(T objToSerialize)
        {
            byte[] retValue;
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    _formatter.Serialize(stream, objToSerialize);
                    retValue = new byte[stream.Length];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(retValue, 0, (int)stream.Length);
                }
            }
            catch (SerializationException)
            {
                throw;
            }
            return retValue;
        }

        /// <summary>
        /// Deserializes the specified a stream.
        /// </summary>
        /// <typeparam name="T">Type T to be serialized</typeparam>
        /// <param name="aStream">A stream.</param>
        /// <returns></returns>
        public T Deserialize<T>(Stream aStream)
        {
            try
            {
                aStream.Seek(0, SeekOrigin.Begin);
                return (T)_formatter.Deserialize(aStream);
            }
            catch (SerializationException se)
            {
                Log.Write(se);
                //log something here
                throw;
            }
        }

        /// <summary>
        /// Deserializes the specified object to deserialize.
        /// </summary>
        /// <typeparam name="T">Type T to be serialized</typeparam>
        /// <param name="objToDeserialize">The object to deserialize.</param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] objToDeserialize)
        {
            T retValue;
            try
            {
                using (MemoryStream stream = new MemoryStream(objToDeserialize))
                {
                    retValue = (T)_formatter.Deserialize(stream);
                }
            }
            catch (SerializationException serializationException)
            {
                Log.Write(serializationException);
                throw;
            }

            return retValue;
        }


        /// <summary>
        /// Deserializes the specified object to deserialize.
        /// </summary>
        /// <param name="objToDeserialize">The object to deserialize.</param>
        /// <param name="type">The type</param>
        /// <returns></returns>
        public object Deserialize(byte[] objToDeserialize, Type type = null)
        {
            object @object;
            try
            {
                using (MemoryStream stream = new MemoryStream(objToDeserialize))
                {
                    @object = _formatter.Deserialize(stream);
                }
            }
            catch (SerializationException e)
            {
                Log.Write(e);
                throw;
            }
            return @object;
        }

        public override string ToString()
        {
            return "Binary Serializer";
        }
    }
}
