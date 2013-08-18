using LINQBridge.DynamicCore.Helper;
using LINQBridge.DynamicCore.Properties;
using LINQBridge.DynamicCore.Template;
using LINQBridge.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.Serialization.Formatters.Binary;

namespace LINQBridge.DynamicCore
{
    public class DynamicDebuggerVisualizer
    {
        private IFileSystem FileSystem { get; set; }

        private static readonly string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public DynamicDebuggerVisualizer()
            : this(new FileSystem())
        {
        }

        public DynamicDebuggerVisualizer(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
            AssemblyFinderHelper.FileSystem = FileSystem;

        }

        internal void DeployLinqScripts(Message message)
        {
            try
            {
                Log.Write("Entered in DeployLinqScripts");
                // Log.Write("Message: {0}", message);

                var dstScriptPath = Path.Combine(MyDocuments, Resources.LINQPadQuery);
                Log.Write("dstScriptPath: {0}", dstScriptPath);

                if (!FileSystem.Directory.Exists(dstScriptPath))
                {
                    FileSystem.Directory.CreateDirectory(dstScriptPath);
                    Log.Write(string.Format("Directory Created: {0}", dstScriptPath));
                }

                var dst = Path.Combine(dstScriptPath, string.Format(message.FileName, message.TypeFullName));
                Log.Write("dst: {0}", dst);

                var refAssemblies = new List<string> { message.TypeLocation };
                refAssemblies.AddRange(message.ReferencedAssemblies);

                if (FileSystem.File.Exists(dst))
                {
                    Log.Write(string.Format("File Already Exists: {0}", dst));
                    return;
                }

                var linqQuery = new Inspection(refAssemblies, message.TypeFullName, message.TypeNamespace);
                var linqQueryText = linqQuery.TransformText();

                Log.Write("LinqQuery file Tranformed");


                using (var memoryStream = FileSystem.File.OpenWrite(dst))
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    streamWriter.Write(linqQueryText);
                    streamWriter.Flush();
                    memoryStream.Flush();
                }
                Log.Write("LinqQuery file Generated");

            }
            catch (Exception e)
            {
                Log.Write(e, "DynamicDebuggerVisualizer.DeployLinqScripts");
                throw;
            }
        }


        public void ShowVisualizer(Stream inData)
        {
            Log.Configure("LINQBridge");

            Log.Write("ShowVisualizer Started...");

            var formatter = new BinaryFormatter();
            var message = (Message)formatter.Deserialize(inData);

            Log.Write("Message deserialized");
            Log.Write(string.Format("Message content /n {0}", message));

            var type = Type.GetType(message.AssemblyQualifiedName);
            var referencedAssemblies = type.GetReferencedAssemblies(message.TypeLocation);

            message.ReferencedAssemblies.AddRange(referencedAssemblies);

            DeployLinqScripts(message);
            Log.Write("LinqQuery Successfully deployed");

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
                Log.Write("About to start LINQPad with these parameters: {0}, {1}", startInfo.FileName, startInfo.Arguments);

                try
                {
                    process.Start();
                    Log.Write("LINQPad Successfully started");

                }
                catch (Exception e)
                {
                    Log.Write(e, "Error during LINQPad execution");

                    throw;
                }
            }
        }

    }
}
