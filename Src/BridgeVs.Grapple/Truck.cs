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
using BridgeVs.Grapple.Serialization;
using BridgeVs.Shared.Common;
using BridgeVs.Shared.Logging;

namespace BridgeVs.Grapple
{
    /// <summary>
    /// 
    /// </summary>
    public class Truck
    {
        private readonly string _truckId;
        private readonly IServiceSerializer _serviceSerializer;
        private string FilePath => Path.Combine(CommonFolderPaths.GrappleFolder, _truckId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="truckId"></param>
        /// <param name="serviceSerializer"></param>
        public Truck(string truckId, IServiceSerializer serviceSerializer = null )
        {
            Log.Write($"Setting Up the Truck {truckId}");
            if (string.IsNullOrEmpty(truckId))
            {
                throw new ArgumentException("truckId cannot be null", nameof(truckId));
            }

            _truckId = truckId;
            _serviceSerializer = serviceSerializer ?? new DefaultSerializer();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        public void SendCargo<T>(T item)
        {
            Type type = item.GetType();

            Log.Write($"SendCargo - Type {item.GetType()}");
            try
            {
                byte[] byteStream = _serviceSerializer.Serialize(item);
                
                File.WriteAllBytes(FilePath, byteStream);

                Log.Write($"SendCargo - Cargo Sent - Byte sent: {byteStream.Length} - File Created: {FilePath}");
            }
            catch (Exception e)
            {
                Log.Write(e, "SendCargo - Error During serializing cargo");
                throw;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ReceiveCargo<T>()
        {
            string typeName = typeof(T).FullName;
            Log.Write("ReceiveCargo - Receiving Cargo of Type {0}", typeName);
            try
            {
                byte[] byteStream = File.ReadAllBytes(FilePath);

                return _serviceSerializer.Deserialize<T>(byteStream);
            }
            catch (Exception e)
            {
                Log.Write(e, $"ReceiveCargo - Error while deserializing type {typeName}");

                throw;
            }
        }

        public object ReceiveCargo()
        {
            Log.Write("ReceiveCargo - UnLoading Cargo for Typeless object");
            try
            {
                byte[] byteStream = File.ReadAllBytes(FilePath);

                return _serviceSerializer.Deserialize(byteStream);
            }
            catch (Exception e)
            {
                Log.Write(e, $"ReceiveCargo - Error while deserializing");

                throw;
            }
        }
    }
}