using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using Bridge.Grapple;
using Bridge.Visualizers.Template;
using Bridge.Visualizers.Utils;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace Bridge.Visualizers
{

    public class LINQPadDebuggerVisualizerObjectSource : VisualizerObjectSource
    {
        internal const string FileNameFormat = "ddMMyy_{0}.linq";


        public override void GetData(object target, Stream outgoingData)
        {
            var scriptFileName = DateTime.Now.ToString(FileNameFormat);
            var targetType = target.GetType();
            var targetTypeFullName = TypeNameHelper.GetDisplayName(targetType, true);
            var pattern = new Regex("[<>]");
            ;
            var message = new Message
                              {
                                  FileName = string.Format(scriptFileName, pattern.Replace(targetTypeFullName, string.Empty)),
                                  TypeFullName = targetTypeFullName,
                                  TypeLocation = targetType.Assembly.Location,
                                  TypeNamespace = targetType.Namespace
                              };

            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(outgoingData, message);


            var busChannel = Bus.Instance;
            busChannel.Add(target);
            busChannel.BroadCast();

        }
    }
}
