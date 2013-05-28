using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using LINQBridge.DynamicVisualizers.Properties;
using LINQBridge.DynamicVisualizers.Template;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Message = LINQBridge.DynamicVisualizers.Template.Message;

namespace LINQBridge.DynamicVisualizers
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicDebuggerVisualizer : DialogDebuggerVisualizer
    {


        private static readonly string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private const string SearchPattern = "*{0}*.dll";

        internal static void DeployLinqScripts(Message message)
        {
            try
            {
                Logging.Log.Write("Entered in DeployLinqScripts");
                Logging.Log.Write("Message: {0}", message);

                var dstScriptPath = Path.Combine(MyDocuments, Resources.LINQPadQuery);
                Logging.Log.Write("dstScriptPath: {0}", dstScriptPath);

                if (!Directory.Exists(dstScriptPath))
                {
                    Directory.CreateDirectory(dstScriptPath);
                    Logging.Log.Write("Directory Created");
                }

                var dst = Path.Combine(dstScriptPath, string.Format(message.FileName, message.TypeFullName));
                Logging.Log.Write("dst: {0}", dst);

                var refAssemblies = new List<string> { message.TypeLocation };
                refAssemblies.AddRange(message.ReferencedAssemblies);

                if (File.Exists(dst))
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



        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            Logging.Log.Write("Entered in Show...");

            var formatter = new BinaryFormatter();
            var message = (Message)formatter.Deserialize(objectProvider.GetData());
            Logging.Log.Write("Message deserialized");

            var referencedAssemblies = new List<string>();

            var type = Type.GetType(message.AssemblyQualifiedName);
            if (type != null)
            {
                //TODO: add code for multiple generic arguments like: Dictionary<TK,TV>
                while (type.IsGenericType)
                    type = type.GetGenericArguments()[0];

                if (type.Assembly.Location != message.TypeLocation)
                    referencedAssemblies.Add(type.Assembly.Location);

                var refAssembliesExceptSystem = type.Assembly.GetReferencedAssemblies()
                                               .Where(name => !(name.Name.Contains("Microsoft") || name.Name.Contains("System") || name.Name.Contains("mscorlib")))
                                               .Select(name => name.Name)
                                               .ToList();

                if (refAssembliesExceptSystem.Any())
                {
                    var typePath = Path.GetDirectoryName(type.Assembly.Location);
                    Logging.Log.Write("typePath: {0}", typePath);

                    var rootPath = typePath;
                    Logging.Log.Write("rootPath: {0}", rootPath);
                    var found = false;
                    do
                    {
                        if (string.IsNullOrEmpty(rootPath)) break;

                        refAssembliesExceptSystem
                            .ForEach(s =>
                                         {
                                             Logging.Log.Write("SearchPattern: {0}", string.Format(SearchPattern, s));

                                             var files = Directory.EnumerateFiles(rootPath,
                                                                                  string.Format(SearchPattern, s),
                                                                                  SearchOption.AllDirectories);

                                             var collection = (files as IList<string> ?? files).ToList();
                                             Logging.Log.WriteIf(collection.Any(), "Files Found count {0}",
                                                                 files.Count());

                                             found = collection.Any();
                                             referencedAssemblies.AddRange(collection);
                                         });

                        var parentPath = Directory.GetParent(rootPath);
                        if (parentPath == null) break;

                        rootPath = parentPath.FullName;
                        Logging.Log.Write("New rootPath: {0}", rootPath);

                    } while (!found);
                    message.ReferencedAssemblies = referencedAssemblies.ToList();
                    Logging.Log.Write("Referenced Assemblies");
                    message.ReferencedAssemblies.ForEach(s => Logging.Log.Write("\t Assembly {0}", s));
                }
                else
                    Logging.Log.Write("No Referenced Assemblies");
          
            
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
