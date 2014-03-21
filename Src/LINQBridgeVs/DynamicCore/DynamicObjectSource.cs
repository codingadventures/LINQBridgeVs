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

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using Grapple;
using LINQBridge.DynamicCore.Helper;
using Message = LINQBridge.DynamicCore.Template.Message;

namespace LINQBridge.DynamicCore
{
    public static class DynamicObjectSource
    {
        internal const string FileNameFormat = "{0}.linq";

        public static void BroadCastData(object target, Stream outgoingData)
        {
            var targetType = target.GetType();
            var targetTypeFullName = TypeNameHelper.GetDisplayName(targetType, true);
            var pattern1 = new Regex("[<]");
            var pattern2 = new Regex("[>]");

            var fileName = pattern1.Replace(targetTypeFullName, "(");

            fileName = pattern2.Replace(fileName, ")");

            var message = new Message
            {
                FileName = string.Format(FileNameFormat, TypeNameHelper.RemoveSystemNamespaces(fileName)),
                TypeName = targetType.Name,
                TypeFullName = targetTypeFullName,
                TypeLocation = targetType.Assembly.Location,
                TypeNamespace = targetType.Namespace,
                AssemblyQualifiedName = targetType.AssemblyQualifiedName
            };

            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(outgoingData, message);


            var truck = new Truck("LINQBridgeVsTruck");
            truck.LoadCargo(target);
            var res = truck.DeliverTo(fileName);
        }


    }
}
