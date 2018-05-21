#region License
// Copyright (c) 2013 - 2018 Coding Adventures
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using BridgeVs.Logging;
using BridgeVs.DynamicVisualizers.Forms;
using BridgeVs.DynamicVisualizers.Helper;
using BridgeVs.DynamicVisualizers.Template;
using BridgeVs.Locations;
using Message = BridgeVs.DynamicVisualizers.Template.Message;
using SharpRaven;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace BridgeVs.DynamicVisualizers
{
    /// <summary>
    /// Core of Dynamic Visualizer. This class is used by all four DynamicVisualizerVx (for VS2012, VS2013, VS2015, VS2017)
    /// It opens LINQPad and create dynamically a linqscript.
    /// </summary>
    public class DynamicDebuggerVisualizer : DialogDebuggerVisualizer
    {
        private static IFileSystem _fileSystem;
        internal static IFileSystem FileSystem
        {
            get => _fileSystem ?? (_fileSystem = new FileSystem());
            set => _fileSystem = value;
        }

        #region [ Consts ]
        private const UInt32 WmKeydown = 0x0100;
        private const int VkF5 = 0x74;
        private const int SwShownormal = 1;
        #endregion

        #region [ Constructors ]
        public DynamicDebuggerVisualizer()
        {
            AssemblyFinderHelper.FileSystem = FileSystem;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDebuggerVisualizer"/> class. Internal for testing purpose only.
        /// </summary>
        /// <param name="fileSystem">The file system. System.IO.Abstraction can be used to Mock the file System. </param>
        internal DynamicDebuggerVisualizer(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
            AssemblyFinderHelper.FileSystem = FileSystem;
        }

        #endregion

        /// <summary>
        /// Deploys the dynamically generated linqscript.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void DeployLinqScript(Message message)
        {
            try
            {
                Log.Write("Entered in DeployLinqScript");

                string dstScriptPath = CommonFolderPaths.DefaultLinqPadQueryFolder;

                Log.Write("dstScriptPath: {0}", dstScriptPath);
                string targetFolder = Path.Combine(dstScriptPath, message.AssemblyName);

                if (!FileSystem.Directory.Exists(targetFolder))
                    FileSystem.Directory.CreateDirectory(targetFolder);

                string linqPadScriptPath = Path.Combine(targetFolder, message.FileName);
                Log.Write("linqPadScriptPath: {0}", linqPadScriptPath);

                List<string> refAssemblies = new List<string>();
                refAssemblies.AddRange(message.ReferencedAssemblies);
                Inspection linqQuery = new Inspection(refAssemblies, message.TypeFullName, message.TypeNamespace, message.TypeName);
                string linqQueryText = linqQuery.TransformText();

                Log.Write("LinqQuery file Transformed");

                using (Stream memoryStream = FileSystem.File.Open(linqPadScriptPath, FileMode.Create))
                using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                {
                    streamWriter.Write(linqQueryText);
                    streamWriter.Flush();
                    memoryStream.Flush();
                }
                Log.Write("LinqQuery file Generated");
            }
            catch (Exception e)
            {
                string vsVersion = VisualStudioVersionHelper.FindCurrentVisualStudioVersion();

                if (CommonRegistryConfigurations.IsErrorTrackingEnabled(vsVersion))
                    RavenWrapper.Instance.Capture(e, vsVersion, message: "Error deploying the linqpad script");

                Log.Write(e, "DynamicDebuggerVisualizer.DeployLinqScript");
                throw;
            }
        }

        /// <summary>
        /// Shows the visualizer.
        /// </summary>
        /// <param name="inData">The in data.</param>
        /// <param name="vsVersion">The vs version.</param>
        /// <returns></returns>
        public Form ShowLINQPad(Stream inData, string vsVersion)
        {
            Log.Write("ShowVisualizer Started...");

            Log.Write("Vs Targeted Version ", vsVersion);

            BinaryFormatter formatter = new BinaryFormatter();
            Message message = (Message)formatter.Deserialize(inData);

            Log.Write("Message deserialized");
            Log.Write($"Message content /n {message}");

            Type type = Type.GetType(message.AssemblyQualifiedName);

            string originalTypeLocation = CommonRegistryConfigurations.GetOriginalAssemblyLocation(type, vsVersion);

            List<string> referencedAssemblies = type.GetReferencedAssemblies(originalTypeLocation);

            message.ReferencedAssemblies.AddRange(referencedAssemblies);

            DeployLinqScript(message);
            Log.Write("LinqQuery Successfully deployed");

            string linqQueryfileName = Path.Combine(CommonFolderPaths.DefaultLinqPadQueryFolder, message.AssemblyName, message.FileName);
            string linqPadInstallationPath = CommonRegistryConfigurations.GetLINQPadInstallationPath(vsVersion);
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = "LINQPad.exe",
                WorkingDirectory = Path.GetFullPath(linqPadInstallationPath),
                Arguments = linqQueryfileName + " -run"
            };

            Log.Write("About to start LINQPad with these parameters: {0}, {1}", startInfo.FileName, startInfo.Arguments);

            Process process = Process.Start(startInfo);
            if (process != null)
            {
                process.WaitForInputIdle(-1);
                process.Dispose();
            }
            string linqPadExePath = Path.Combine(linqPadInstallationPath, "LINQPad.exe");
            string linqPadVersion = FileVersionInfo.GetVersionInfo(linqPadExePath).FileDescription;

            Process foundProcess = Process.GetProcessesByName("LINQPad").FirstOrDefault(p => linqPadVersion.Equals(p.MainWindowTitle));

            SendInputToProcess(foundProcess ?? process);

            Log.Write("LINQPad Successfully started");

            return new TemporaryForm();
        }

        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            Log.Configure("LINQBridgeVs", "DynamicDebuggerVisualizer");

            string vsVersion = VisualStudioVersionHelper.FindCurrentVisualStudioVersion();

            try
            {
                Stream dataStream = objectProvider.GetData();

                if (dataStream.Length == 0)
                    return;

                Form formToShow = ShowLINQPad(dataStream, vsVersion);

#if !TEST
                windowService.ShowDialog(formToShow);
#endif
            }
            catch (Exception exception)
            {
                const string context = "Error during LINQPad execution";
                Log.Write(exception, context);

                if (CommonRegistryConfigurations.IsErrorTrackingEnabled(vsVersion))
                    RavenWrapper.Instance.Capture(exception, vsVersion, message: context);

            }
        }

        #region [ Private Static Methods ]


        /// <summary>
        /// Sends the input to LINQPad. Simulates key inputs to run a linqscript (F5)
        /// </summary>
        private static void SendInputToProcess(Process process)
        {
            try
            {
                int index = 0;
                while (process.MainWindowHandle == IntPtr.Zero && index < 3)
                {
                    // Discard cached information about the process
                    // because MainWindowHandle might be cached.

                    Log.Write("Waiting MainWindowHandle... - Iteration: {0}", ++index);
                    process.Refresh();
                    index++;
                    Thread.Sleep(10);
                }

                ShowWindowAsync(process.MainWindowHandle, SwShownormal);
                Log.Write("LINQPad ShowWindowAsync {0}", process.MainWindowHandle);

                SetForegroundWindow(process.MainWindowHandle);

                PostMessage(process.MainWindowHandle, WmKeydown, VkF5, 0);

                Log.Write("LINQPad PostMessage {0}", VkF5);
            }
            catch (Exception e)
            {
                string vsVersion = VisualStudioVersionHelper.FindCurrentVisualStudioVersion();

                //don't really need to capture this in sentry, however it can tell me I'm doing the wrong thing with the process
                if (CommonRegistryConfigurations.IsErrorTrackingEnabled(vsVersion))
                    RavenWrapper.Instance.Capture(e, vsVersion, message: "Error while sending inputs to linqpad");

                Log.Write(e, "Error during LINQPad Sending inputs");
            }
        }

        #endregion

        #region [ DllImport ]
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32")]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, UInt32 msg, int wParam, int lParam);
        #endregion
    }
}