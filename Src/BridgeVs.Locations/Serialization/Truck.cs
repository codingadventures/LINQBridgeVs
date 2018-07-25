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
        /// <param name="suggestedSerializer">Suggested strategy for serialization</param>
        /// <returns>Returns the type of serialization used for </returns>
        public static SerializationOption? SendCargo<T>(T item, string truckId, IServiceSerializer suggestedSerializer)
        {
            Type type = item.GetType();
            byte[] byteStream = null;

            Log.Write($"SendCargo - Type {type.FullName}");
            IServiceSerializer serializer = suggestedSerializer;
            do
            {
                try
                {
                    byteStream = serializer.Serialize(item);
                }
                catch (Exception e)
                {
                    Log.Write(e, $"SendCargo - Error During serializing cargo with this strategy: {serializer.ToString()}");
                    serializer = serializer.Next;
                }
            }
            while (byteStream == null || byteStream.Length == 0 && serializer != null);

            if (byteStream.Length > 0)
            {
                string filePath = Path.Combine(CommonFolderPaths.GrappleFolder, truckId);
                File.WriteAllBytes(filePath, byteStream);
                SerializationOption successfulSerialization = (SerializationOption)Enum.Parse(typeof(SerializationOption), serializer.ToString());
                Log.Write($"SendCargo - Cargo Sent - Byte sent: {byteStream.Length} - File Created: {filePath} - Serialization Used: {successfulSerialization}");

                return successfulSerialization;

            }
            else
            {
                Log.Write($"SendCargo - It was not possible to serialize at all the type {type.FullName}");
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ReceiveCargo<T>(string truckId, IServiceSerializer serviceSerializer)
        {
            Log.Write("ReceiveCargo - Receiving Cargo of Type {0}", typeof(T).FullName);

            try
            {
                byte[] byteStream = File.ReadAllBytes(Path.Combine(CommonFolderPaths.GrappleFolder, truckId));

                return serviceSerializer.Deserialize<T>(byteStream);
            }
            catch (Exception e)
            {
                Log.Write(e, $"ReceiveCargo - Error while deserializing type {typeof(T).FullName}");

                throw;
            }
        }

        public static object ReceiveCargo(string truckId, IServiceSerializer serviceSerializer)
        {
            Log.Write("ReceiveCargo - UnLoading Cargo for Typeless object");
            try
            {
                byte[] byteStream = File.ReadAllBytes(Path.Combine(CommonFolderPaths.GrappleFolder, truckId));

                return serviceSerializer.Deserialize(byteStream);
            }
            catch (Exception e)
            {
                Log.Write(e, $"ReceiveCargo - Error while deserializing");

                throw;
            }
        }
    }
}