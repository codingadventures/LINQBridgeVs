using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using LINQBridge.DynamicCore.Template;
using LINQBridge.DynamicCore.Utils;
using LINQBridge.Grapple;

namespace LINQBridge.DynamicCore
{
    public static  class DynamicObjectSource
    {
        internal const string FileNameFormat = "ddMMyy_{0}.linq";

        public static void BroadCastData(object target, Stream outgoingData)
        {
            var scriptFileName = DateTime.Now.ToString(FileNameFormat);
            var targetType = target.GetType();
            var targetTypeFullName = TypeNameHelper.GetDisplayName(targetType, true);
            var pattern = new Regex("[<>]");

            var message = new Message
            {
                FileName = string.Format(scriptFileName, pattern.Replace(targetTypeFullName, string.Empty)),
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
