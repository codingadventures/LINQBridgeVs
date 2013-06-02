using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using LINQBridge.DynamicCore.Properties;
using LINQBridge.DynamicCore.Template;
using LINQBridge.DynamicCore.Utils;

namespace LINQBridge.DynamicCore
{
    public class DynamicDebuggerVisualizer
    {
        public IFileSystem FileSystem { get; private set; }

        private static readonly string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public DynamicDebuggerVisualizer()
            : this(new FileSystem())
        {

        }

        public DynamicDebuggerVisualizer(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        internal void DeployLinqScripts(Message message)
        {
            try
            {
                Logging.Log.Write("Entered in DeployLinqScripts");
                Logging.Log.Write("Message: {0}", message);

                var dstScriptPath = Path.Combine(MyDocuments, Resources.LINQPadQuery);
                Logging.Log.Write("dstScriptPath: {0}", dstScriptPath);

                if (!FileSystem.Directory.Exists(dstScriptPath))
                {
                    FileSystem.Directory.CreateDirectory(dstScriptPath);
                    Logging.Log.Write("Directory Created");
                }

                var dst = Path.Combine(dstScriptPath, string.Format(message.FileName, message.TypeFullName));
                Logging.Log.Write("dst: {0}", dst);

                var refAssemblies = new List<string> { message.TypeLocation };
                refAssemblies.AddRange(message.ReferencedAssemblies);

                if (FileSystem.File.Exists(dst))
                {
                    Logging.Log.Write("File Already Exists");
                    return;
                }

                var linqQuery = new Inspection(refAssemblies, message.TypeFullName, message.TypeNamespace);
                var linqQueryText = linqQuery.TransformText();

                Logging.Log.Write("LinqQuery file Tranformed");


                using (var streamWriter = new StreamWriter(dst, false))
                {
                    streamWriter.Write(linqQueryText);
                    streamWriter.Flush();
                }
                Logging.Log.Write("LinqQuery file Generated");

            }
            catch (Exception e)
            {
                Logging.Log.Write(e, "DynamicDebuggerVisualizer.DeployLinqScripts");
                throw;
            }
        }


        public void ShowVisualizer(Stream inData)
        {
            Logging.Log.Configure("LINQBridge");

            Logging.Log.Write("Entered in Show...");

            var formatter = new BinaryFormatter();
            var message = (Message)formatter.Deserialize(inData);
            Logging.Log.Write("Message deserialized");


            var type = Type.GetType(message.AssemblyQualifiedName);
            if (type != null)
            {
                //TODO: add code for multiple generic arguments like: Dictionary<TK,TV>
                while (type.IsGenericType)
                    type = type.GetGenericArguments()[0];

                if (type.Assembly.Location != message.TypeLocation)
                    message.ReferencedAssemblies.Add(type.Assembly.Location);

                Logging.Log.WriteIf(!type.Assembly.Location.Equals(message.TypeLocation), "No Referenced Assemblies");

                var referencedAssemblyPaths = type.Assembly.GetReferencedAssembliesPath();

                message.ReferencedAssemblies.AddRange(referencedAssemblyPaths);
            }

            DeployLinqScripts(message);
            Logging.Log.Write("LinqQuery Successfully deployed");

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
                Logging.Log.Write("About to start LINQPad with these parameters: {0}, {1}", startInfo.FileName, startInfo.Arguments);

                try
                {
                    process.Start();
                    Logging.Log.Write("LINQPad Successfully started");

                }
                catch (Exception e)
                {
                    Logging.Log.Write(e, "Error during LINQPad execution");

                    throw;
                }
            }
        }

    }
}
