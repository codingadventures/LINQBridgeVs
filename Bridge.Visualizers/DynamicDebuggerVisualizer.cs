using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using LINQBridge.DynamicVisualizers.Properties;
using LINQBridge.DynamicVisualizers.Template;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace LINQBridge.DynamicVisualizers
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicDebuggerVisualizer : DialogDebuggerVisualizer
    {


        private static readonly string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        internal static void DeployScripts(Message message)
        {
            try
            {
                Debug.WriteLine("Entered in DeployScript");
                Debug.WriteLine("Message: {0}", message);

                var dstScriptPath = Path.Combine(MyDocuments, Resources.LINQPadQuery);
                Debug.WriteLine("dstScriptPath: {0}", dstScriptPath);

                var dst = Path.Combine(dstScriptPath, string.Format(message.FileName, message.TypeFullName));
                Debug.WriteLine("dst: {0}", dst);

                var refAssemblies = new List<string> { message.TypeLocation };
                refAssemblies.AddRange(message.ReferencedAssemblies);

                if (File.Exists(dst))
                {
                    Debug.WriteLine("File Already Exists");
                    return;
                }

                var linqQuery = new Inspection(refAssemblies, message.TypeFullName, message.TypeNamespace);
                var linqQueryText = linqQuery.TransformText();

                Debug.WriteLine("LinqQuery file generated");


                using (var streamWriter = new StreamWriter(dst, false))
                {
                    streamWriter.Write(linqQueryText);
                    streamWriter.Flush();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                throw;
            }
        }

        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            var formatter = new BinaryFormatter();
            var message = (Message)formatter.Deserialize(objectProvider.GetData());

            DeployScripts(message);
            Debug.WriteLine("LinqQuery Successfully deployed");

            using (var process = new Process())
            {
                var startInfo = new ProcessStartInfo
                                    {
                                        WindowStyle = ProcessWindowStyle.Normal,
                                        FileName = Resources.LINQPadExe,
                                        WorkingDirectory = Environment.GetEnvironmentVariable("ProgramFiles") + @"\LINQPad4",
                                        Arguments = Path.Combine(MyDocuments, Resources.LINQPadQuery, message.FileName) + " " + Resources.LINQPadCommands
                                    };



                process.StartInfo = startInfo;

                Debug.WriteLine("About to start LINQPad with these parameters: {0}, {1}", startInfo.FileName, startInfo.Arguments);

                try
                {
                    process.Start();
                    Debug.WriteLine("LINQPad Successfully started");

                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error during LINQPad execution");
                    Debug.WriteLine(e.Message);

                    throw;
                }
            }
        }
    }


}
