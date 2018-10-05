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
using System.CodeDom;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using BridgeVs.Shared.Common;
using BridgeVs.Shared.Logging;
using BridgeVs.Shared.Options;

namespace BridgeVs.Shared.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    public static class Truck
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The object to serialize</param>
        /// <param name="truckId">the unique id used to save the serialize item to disk</param>
        /// <param name="serializationOption"> the proposed serialization strategy</param>
        /// <returns>Returns the actual serialization strategy used to serialize the <paramref name="item"/> </returns>
        public static SerializationOption? SendCargo<T>(T item, string truckId, SerializationOption serializationOption)
        {
            byte[] byteStream = null;

            Log.Write($"SendCargo - Type {typeof(T).FullName}");

            IServiceSerializer serializer = CreateSerializationStrategy(serializationOption);
            
            do
            {
                try
                {
                    byteStream = serializer.Serialize(item);
                }
                catch (Exception e)
                {
                    Log.Write(e, $"SendCargo - Error During serializing cargo with this strategy: {serializer}");
                    serializer = serializer.Next;
                }
            }
            while (byteStream == null || byteStream.Length == 0 && serializer != null);

            if (byteStream.Length > 0)
            {
                string filePath = Path.Combine(CommonFolderPaths.GrappleFolder, truckId);
                File.WriteAllBytes(filePath, byteStream);
                if (serializer == null)
                    return null;

                SerializationOption successfulSerialization = (SerializationOption)Enum.Parse(typeof(SerializationOption), serializer.ToString());
                Log.Write($"SendCargo - Cargo Sent - Byte sent: {byteStream.Length} - File Created: {filePath} - Serialization Used: {successfulSerialization}");

                return successfulSerialization;
            } 

            Log.Write($"SendCargo - It was not possible to serialize at all the type {typeof(T).FullName}");
           

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ReceiveCargo<T>(string truckId, SerializationOption serializationOption)
        {
            Log.Write("ReceiveCargo - Receiving Cargo of Type {0}", typeof(T).FullName);

            try
            {
                byte[] byteStream = File.ReadAllBytes(Path.Combine(CommonFolderPaths.GrappleFolder, truckId));
                IServiceSerializer serviceSerializer = CreateDeserializationStrategy(serializationOption);
                return serviceSerializer != null ? serviceSerializer.Deserialize<T>(byteStream) : default(T);
            }
            catch (Exception e)
            {
                Log.Write(e, $"ReceiveCargo - Error while deserializing type {typeof(T).FullName}");

                throw;
            }
        }

        public static object ReceiveCargo(string truckId, SerializationOption serializationOption)
        {
            Log.Write("ReceiveCargo - UnLoading Cargo for Typeless object");
            try
            {
                byte[] byteStream = File.ReadAllBytes(Path.Combine(CommonFolderPaths.GrappleFolder, truckId));

                IServiceSerializer serviceSerializer = CreateDeserializationStrategy(serializationOption);

                return serviceSerializer?.Deserialize(byteStream);
            }
            catch (Exception e)
            {
                Log.Write(e, $"ReceiveCargo - Error while deserializing");

                throw;
            }
        }

        /// <summary>
        /// The serialization strategy tries to serialize the type with the first serializer and if
        /// it fails it uses the next one. 
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private static IServiceSerializer CreateSerializationStrategy(SerializationOption option)
        {
            IServiceSerializer serviceSerializer = null;

            switch (option)
            {
                case SerializationOption.JsonSerializer:
                    serviceSerializer = new JsonSerializer()
                    {
                        Next = new DefaultSerializer()
                    };
                    break;
                case SerializationOption.BinarySerializer:
                    serviceSerializer = new DefaultSerializer()
                    {
                        Next = new JsonSerializer()
                    };
                    break;
            }

            return serviceSerializer;
        }

        /// <summary>
        /// The deserialization strategy strictly uses the strategy chosen and doesn't try to deserialize
        /// with a different <seealso cref="IServiceSerializer"/>
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private static IServiceSerializer CreateDeserializationStrategy(SerializationOption option)
        { 
            switch (option)
            {
                case SerializationOption.JsonSerializer:
                    return new JsonSerializer();
                case SerializationOption.BinarySerializer:
                    return new DefaultSerializer();
            }

            Log.Write("CreateDeserializationStrategy - SerializationOption is not valid Value: {0}", option);

            return null;
        }
    }
}