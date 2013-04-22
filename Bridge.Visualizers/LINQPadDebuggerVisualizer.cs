using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Bridge.Visualizers.Properties;
using Bridge.Visualizers.Template;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace Bridge.Visualizers
{
    /// <summary>
    /// 
    /// </summary>
    public class LINQPadDebuggerVisualizer : DialogDebuggerVisualizer
    {
        internal void DeployScripts(Message message)
        {

            var dstScriptPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                             "LINQPad Queries");

            var dst = Path.Combine(dstScriptPath, string.Format(message.FileName,message.TypeFullName));
            if (File.Exists(dst)) return;

            var linqQuery = new Inspection(new List<string> { message.TypeLocation }, message.TypeFullName, message.TypeNamespace);
            var linqQueryText = linqQuery.TransformText();

            using (var streamWriter = new StreamWriter(dst, false))
            {
                streamWriter.Write(linqQueryText);
                streamWriter.Flush();
            }
        }

        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            var formatter = new BinaryFormatter();
            var message = (Message)formatter.Deserialize(objectProvider.GetData());
            DeployScripts(message);
            using (var process = new Process())
            {

                var startInfo = new ProcessStartInfo
                                    {
                                        WindowStyle = ProcessWindowStyle.Normal,
                                        LoadUserProfile = true,
                                        FileName = Resources.LINQPadExe,
                                        Arguments =
                                            Path.Combine(
                                                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                                Resources.LINQPadQuery, message.FileName) + " " + Resources.LINQPadCommands,

                                    };

                process.StartInfo = startInfo;
                process.Start();
            }
        }
    }


}
