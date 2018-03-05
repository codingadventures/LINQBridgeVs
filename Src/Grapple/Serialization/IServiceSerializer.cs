#region License
// Copyright (c) 2013 Giovanni Campo
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

namespace BridgeVs.Grapple.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    public interface IServiceSerializer
    {
        /// <summary>
        /// Serializes the specified a stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aStream">A stream.</param>
        /// <param name="objToSerialize">The obj to serialize.</param>
        void Serialize<T>(Stream aStream, T objToSerialize);
        /// <summary>
        /// Serializes the specified obj to serialize.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objToSerialize">The obj to serialize.</param>
        /// <returns></returns>
        byte[] Serialize<T>(T objToSerialize);
        /// <summary>
        /// Deserializes the specified a stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aStream">A stream.</param>
        /// <returns></returns>
        T Deserialize<T>(Stream aStream);
        /// <summary>
        /// Deserializes the specified obj to deserialize.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objToDeserialize">The obj to deserialize.</param>
        /// <returns></returns>
        T Deserialize<T>(byte[] objToDeserialize);

        /// <summary>
        /// Deserializes the specified obj to deserialize.
        /// </summary>
        /// <param name="objToDeserialize">The obj to deserialize.</param>
        /// <param name="type"></param>
        /// <returns></returns>
        object Deserialize(byte[] objToDeserialize, Type @type = null);
    }
}
