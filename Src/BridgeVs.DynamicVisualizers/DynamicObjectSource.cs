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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using BridgeVs.DynamicVisualizers.Helper;
using BridgeVs.DynamicVisualizers.Template;
using Microsoft.VisualStudio.DebuggerVisualizers;
using BridgeVs.Shared.Common;
using BridgeVs.Shared.Logging;
using BridgeVs.Shared.Options;
using BridgeVs.Shared.Serialization;

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
                Type targetType = GetInterfaceTypeIfIsIterator(target);
                string targetTypeFullName = TypeNameHelper.GetDisplayName(targetType, fullName: true);
                string targetTypeName = TypeNameHelper.GetDisplayName(targetType, fullName: false);
                
                CalculateFileNameAndTypeName(targetTypeFullName, targetTypeName, out string fileName, out string typeName);

                string truckId = Guid.NewGuid().ToString();
                IServiceSerializer serializationStrategy = CreateSerializationStrategy(CommonRegistryConfigurations.GetSerializationOption(vsVersion));
                SerializationOption? serializationOption = Truck.SendCargo(target, truckId, serializationStrategy);
               
                if (serializationOption.HasValue)
                {
                    Message message = new Message
                    {
                        FileName = $"{fileName}.linq",
                        TypeName = typeName.Trim(),
                        TypeFullName = targetTypeFullName,
                        TypeNamespace = targetType.Namespace,
                        AssemblyQualifiedName = targetType.AssemblyQualifiedName,
                        AssemblyName = targetType.Assembly.GetName().Name,
                        TruckId = truckId,
                        SerializationType = serializationOption.Value
                    };

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
                exception.Capture(vsVersion, message: "Error broadcasting the data to linqpad");
                throw;
            }
        }

        private static void CalculateFileNameAndTypeName(string targetTypeFullName, string targetTypeName, out string fileName, out string typeName)
        {
            Regex pattern1 = new Regex("[<]");
            Regex pattern2 = new Regex("[>]");
            Regex pattern3 = new Regex("[,]");
            Regex pattern4 = new Regex("[`]");
            Regex pattern5 = new Regex("[ ]");

            fileName = pattern1.Replace(targetTypeFullName, "(");
            fileName = pattern2.Replace(fileName, ")");

            typeName = pattern1.Replace(targetTypeName, string.Empty);
            typeName = pattern2.Replace(typeName, string.Empty);
            typeName = pattern3.Replace(typeName, string.Empty);
            typeName = pattern4.Replace(typeName, string.Empty);
            typeName = pattern5.Replace(typeName, string.Empty);

            fileName = TypeNameHelper.RemoveSystemNamespaces(fileName);
        }

        private IServiceSerializer CreateSerializationStrategy(SerializationOption option)
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
                default:
                    break;
            }

            return serviceSerializer;
        }

        private static Type GetInterfaceTypeIfIsIterator(object o)
        {
            Type @type = o.GetType();

            if (!@type.IsNestedPrivate || !@type.Name.Contains("Iterator") ||
                !@type.FullName.Contains("System.Linq.Enumerable") || !(o is IEnumerable)) return @type;

            if (@type.BaseType == null || @type.BaseType.FullName.Contains("Object"))
                return @type.GetInterface("IEnumerable`1");

            Log.Write("Iterator type, LINQ Query found {0}", @type.BaseType.ToString());
            return @type.BaseType.GetInterface("IEnumerable`1");
        }

        public override void GetData(object target, Stream outgoingData)
        {
            BroadCastData(target, outgoingData);
        }
    }
}