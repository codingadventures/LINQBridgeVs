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

using BridgeVs.DynamicVisualizers.Helper;
using BridgeVs.DynamicVisualizers.Template;
using BridgeVs.Shared.Common;
using BridgeVs.Shared.Logging;
using BridgeVs.Shared.Options;
using BridgeVs.Shared.Serialization;
using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace BridgeVs.DynamicVisualizers
{
    public class DynamicObjectSource : VisualizerObjectSource
    {
        public void BroadCastData(object target, Stream outgoingData)
        {
            //configure once the vs version for logging and raven
            string vsVersion = VisualStudioVersionHelper.FindCurrentVisualStudioVersion();
            Log.VisualStudioVersion = vsVersion;

            try
            {
                string truckId = Guid.NewGuid().ToString();

                SerializationOption? serializationOption = Truck.SendCargo(target, truckId, CommonRegistryConfigurations.GetSerializationOption(vsVersion));

                if (serializationOption.HasValue)
                {
                    Message message = new Message(truckId, serializationOption.Value, target.GetType());

                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(outgoingData, message);
                }
                else
                {
                    //should throw an error message in a friendly way
                }


            }
            catch (Exception exception)
            {
                Log.Write(exception, "Error in BroadCastData");
                exception.Capture(vsVersion, message: "Error broadcasting the data to LINQPad");
                throw;
            }
        }

        public override void GetData(object target, Stream outgoingData)
        {
            BroadCastData(target, outgoingData);
        }
    }
}