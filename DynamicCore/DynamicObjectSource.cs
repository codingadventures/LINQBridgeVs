using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using LINQBridge.DynamicCore.Template;
using LINQBridge.DynamicCore.Utils;
using LINQBridge.Grapple;

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
                TypeFullName = targetTypeFullName,
                TypeLocation = targetType.Assembly.Location,
                TypeNamespace = targetType.Namespace,
                AssemblyQualifiedName = targetType.AssemblyQualifiedName
            };

            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(outgoingData, message);


            var busChannel = Bus.Instance;
            busChannel.Add(target);
            busChannel.BroadCast();
        }

       
    }
}
