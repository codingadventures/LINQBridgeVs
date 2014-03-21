#region License
// Copyright (c) 2013 Giovanni Campo
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using LINQBridge.DynamicCore.Forms;
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
using Message = LINQBridge.DynamicCore.Template.Message;

namespace LINQBridge.DynamicCore
{
    public class DynamicDebuggerVisualizer
    {
        private IFileSystem FileSystem { get; set; }

        private static readonly string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private const UInt32 WmKeydown = 0x0100;
        private const int VkF5 = 0x74;
        private const int SwShownormal = 1;

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

                //if (FileSystem.File.Exists(dst))
                //{
                //    Log.Write(string.Format("File Already Exists: {0}", dst));
                //    return;
                //}

                var linqQuery = new Inspection(refAssemblies, message.TypeFullName, message.TypeNamespace, message.TypeName);
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


        public Form ShowVisualizer(Stream inData)
        {
            Log.Configure("LINQBridgeVs");

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

            var linqQueryfileName = Path.Combine(MyDocuments, Resources.LINQPadQuery, message.FileName);

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = Resources.LINQPadExe,
                WorkingDirectory = Environment.GetEnvironmentVariable("ProgramFiles") + @"\LINQPad4",
                Arguments = linqQueryfileName + " " + Resources.LINQPadCommands
            };


            Log.Write("About to start LINQPad with these parameters: {0}, {1}", startInfo.FileName, startInfo.Arguments);

            try
            {

                var linqPadProcess = Process.Start(startInfo);

                Thread.Sleep(125);

                if (linqPadProcess.HasExited)
                    linqPadProcess = Process.GetProcessesByName("LINQPad").FirstOrDefault();
                else
                    linqPadProcess.WaitForInputIdle();


                if (linqPadProcess != null)
                {
                    while (linqPadProcess.MainWindowHandle == IntPtr.Zero)
                    {
                        // Discard cached information about the process
                        // because MainWindowHandle might be cached.
                        var index = 0;
                        Log.Write("Waiting MainWindowHandle... - Iteration: {0}", ++index);
                        linqPadProcess.Refresh();
                        Thread.Sleep(10);
                    }

                    Thread.Sleep(100);
                    ShowWindowAsync(linqPadProcess.MainWindowHandle, SwShownormal);
                    Log.Write("LINQPad ShowWindowAsync {0}", linqPadProcess.MainWindowHandle);

                    SetForegroundWindow(linqPadProcess.MainWindowHandle);
                    Log.Write("LINQPad SetForegroundWindow {0}", linqPadProcess.MainWindowHandle);
                    Thread.Sleep(125);
                    PostMessage(linqPadProcess.MainWindowHandle, WmKeydown, VkF5, 0);
                    Log.Write("LINQPad PostMessage {0}", VkF5);

                    Log.Write("LINQPad Successfully started");

                    linqPadProcess.Dispose();
                }
                else
                    Log.Write("LINQPad Process is null");

            }
            catch (Exception e)
            {
                Log.Write(e, "Error during LINQPad execution");
                throw;
            }

            return new TemporaryForm();
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32")]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 msg, int wParam, int lParam);
    }
}
