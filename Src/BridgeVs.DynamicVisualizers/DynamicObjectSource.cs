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
using BridgeVs.Grapple;
using Microsoft.VisualStudio.DebuggerVisualizers;
using BridgeVs.Shared.Common;
using BridgeVs.Shared.Logging;
using BridgeVs.Shared.Options;

namespace BridgeVs.DynamicVisualizers
{
    public class DynamicObjectSource : VisualizerObjectSource
    {
        internal const string FileNameFormat = "{0}.linq";

        public void BroadCastData(object target, Stream outgoingData)
        {
            //configure once the vs version
            string vsVersion = VisualStudioVersionHelper.FindCurrentVisualStudioVersion();
            RavenWrapper.VisualStudioVersion = vsVersion;

            try
            {
                Type targetType = GetInterfaceTypeIfIsIterator(target);
                string targetTypeFullName = TypeNameHelper.GetDisplayName(targetType, fullName: true);
                string targetTypeName = TypeNameHelper.GetDisplayName(targetType, fullName: false);
                //I'm lazy I know...
                Regex pattern1 = new Regex("[<]");
                Regex pattern2 = new Regex("[>]");
                Regex pattern3 = new Regex("[,]");
                Regex pattern4 = new Regex("[`]");
                Regex pattern5 = new Regex("[ ]");

                string fileName = pattern1.Replace(targetTypeFullName, "(");
                fileName = pattern2.Replace(fileName, ")");

                string typeName = pattern1.Replace(targetTypeName, string.Empty);
                typeName = pattern2.Replace(typeName, string.Empty);
                typeName = pattern3.Replace(typeName, string.Empty);
                typeName = pattern4.Replace(typeName, string.Empty);
                typeName = pattern5.Replace(typeName, string.Empty);

                fileName = TypeNameHelper.RemoveSystemNamespaces(fileName);

                Message message = new Message
                {
                    FileName = string.Format(FileNameFormat, fileName),
                    TypeName = typeName.Trim(),
                    TypeFullName = targetTypeFullName,
                    TypeNamespace = targetType.Namespace,
                    AssemblyQualifiedName = targetType.AssemblyQualifiedName,
                    AssemblyName = targetType.Assembly.GetName().Name
                };

                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(outgoingData, message);

                Log.Write("BroadCastData to LINQBridgeVsTruck");
                SerializationOption serializationOption = CommonRegistryConfigurations.GetSerializationOption(vsVersion);
                Truck truck = new Truck("LINQBridgeVsTruck", serializationOption);
                truck.LoadCargo(target);
                bool res = truck.DeliverTo(typeName);
                Log.Write("Data Succesfully Shipped to Grapple");

            }
            catch (Exception exception)
            {
                Log.Write(exception, "Error in BroadCastData");
                RavenWrapper.Instance.Capture(exception, message: "Error broadcasting the data to linqpad");
                throw;
            }
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