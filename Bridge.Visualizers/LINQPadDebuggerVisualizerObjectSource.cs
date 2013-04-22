using System;
using System.Collections.Generic;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Bridge.Grapple;
using Bridge.Visualizers.Template;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace Bridge.Visualizers
{

    public class LINQPadDebuggerVisualizerObjectSource : VisualizerObjectSource
    {
        internal const string FileNameFormat = "ddMMyy_Hmmss_{0}.linq";


        public override void GetData(object target, Stream outgoingData)
        {
            var scriptFileName = DateTime.Now.ToString(FileNameFormat);

            var message = new Message
                              {
                                  FileName = string.Format(scriptFileName, target.GetType().FullName),
                                  TypeFullName = target.GetType().FullName,
                                  TypeLocation = target.GetType().Assembly.Location,
                                  TypeNamespace = target.GetType().Namespace
                              };

            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(outgoingData, message);


            var busChannel = Bus.Instance;
            busChannel.Add(target);
            busChannel.BroadCast();

        }
    }
}
