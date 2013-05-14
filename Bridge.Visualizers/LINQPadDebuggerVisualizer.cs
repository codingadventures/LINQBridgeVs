using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
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


        private static readonly string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private static List<string> _logs = new List<string>();


        internal static void DeployScripts(Message message)
        {
            try
            {
                _logs.Add(message.ToString());
                _logs.Add("MyDocuments: " + MyDocuments);
                _logs.Add("Resources.LINQPadQuery: " + Resources.LINQPadQuery);
                var dstScriptPath = Path.Combine(MyDocuments, Resources.LINQPadQuery);
                _logs.Add("dstScriptPath: " + dstScriptPath);

                var dst = Path.Combine(dstScriptPath, string.Format(message.FileName, message.TypeFullName));


                if (File.Exists(dst)) return;

                var linqQuery = new Inspection(new List<string> { message.TypeLocation }, message.TypeFullName, message.TypeNamespace);
                var linqQueryText = linqQuery.TransformText();

                using (var streamWriter = new StreamWriter(dst, false))
                {
                    streamWriter.Write(linqQueryText);
                    streamWriter.Flush();
                }
            }
            catch (Exception e)
            {
                _logs.Add(e.Message);
                _logs.Add(e.StackTrace);
                Grapple.Bus.Instance.Add(_logs);
                Grapple.Bus.Instance.BroadCast();

                throw;
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
                                        FileName = Environment.GetEnvironmentVariable("LinqPadPath") + @"\" + Resources.LINQPadExe,
                                        Arguments =
                                            Path.Combine(MyDocuments, Resources.LINQPadQuery, message.FileName) + " " + Resources.LINQPadCommands,

                                    };

                process.StartInfo = startInfo;
                process.Start();
            }
        }
    }


}
