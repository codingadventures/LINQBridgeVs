using System;
using System.Collections.Generic;

using System.IO;
using Bridge.Grapple;
using Bridge.Visualizers.Template;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace Bridge.Visualizers
{

    public class LINQPadDebuggerVisualizerObjectSource : VisualizerObjectSource
    {
        internal const string FileNameFormat = "ddMMyy_Hmmss_{0}.linq";

        private string _generatedFileName;

        internal void DeployScripts(Type typeToMap)
        {
            var scriptFileName = DateTime.Now.ToString(FileNameFormat);

            var dstScriptPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                             "LINQPad Queries");

            _generatedFileName = string.Format(scriptFileName, typeToMap.FullName);
            var dst = Path.Combine(dstScriptPath, _generatedFileName);
            if (File.Exists(dst)) return;

            var linqQuery = new Inspection(new List<string> { typeToMap.Assembly.Location }, typeToMap);
            var linqQueryText = linqQuery.TransformText();

            using (var streamWriter = new StreamWriter(dst, false))
            {
                streamWriter.Write(linqQueryText);
                streamWriter.Flush();
            }
        }


        public override void GetData(object target, Stream outgoingData)
        {
            DeployScripts(target.GetType());
            using (var writer = new StreamWriter(outgoingData))
            {
                writer.Write(_generatedFileName);
            }
            
            var busChannel = Bus.Instance;
            busChannel.Add(target);
            busChannel.BroadCast();
           
        }
    }
}
