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
// NON INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using BridgeVs.DynamicVisualizers.Forms;
using BridgeVs.DynamicVisualizers.Helper;
using BridgeVs.DynamicVisualizers.Template;
using BridgeVs.Shared.Common;
using BridgeVs.Shared.Logging;
using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Win32Interop.WinHandles;
using Message = BridgeVs.DynamicVisualizers.Template.Message;
using FS = BridgeVs.Shared.FileSystem.FileSystemFactory;
using System.Text;

namespace BridgeVs.DynamicVisualizers
{
    /// <summary>
    /// Core of Dynamic Visualizer. This class is used by all four DynamicVisualizerVx (for VS2012, VS2013, VS2015, VS2017)
    /// It opens LINQPad and create dynamically a linq script.
    /// </summary>
    public class DynamicDebuggerVisualizer : DialogDebuggerVisualizer
    {
        private const int SwShowNormal = 1;
        
        /// <summary>
        /// Deploys the dynamically generated linq script.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="vsVersion">The visual studio version</param>
        internal void DeployLinqScript(Message message, string vsVersion)
        {
            try
            {
                Log.Write("Entered in DeployLinqScript");

                string dstScriptPath = CommonFolderPaths.DefaultLinqPadQueryFolder;

                Log.Write("dstScriptPath: {0}", dstScriptPath);
                string targetFolder = Path.Combine(dstScriptPath, message.AssemblyName);

                if (!FS.FileSystem.Directory.Exists(targetFolder))
                    FS.FileSystem.Directory.CreateDirectory(targetFolder);

                string fileName = FindAvailableFileName(targetFolder, message.FileName);

                string linqPadScriptFilePath = Path.Combine(targetFolder, message.FileName);
                Log.Write("linqPadScriptPath: {0}", linqPadScriptFilePath);

                Inspection linqQuery = new Inspection(message);
                string linqQueryText = linqQuery.TransformText();

                FS.FileSystem.File.WriteAllText(linqPadScriptFilePath, linqQueryText);
                
                Log.Write("LinqQuery Successfully deployed");
            }
            catch (Exception e)
            {
                e.Capture(vsVersion, message: "Error deploying the LINQPad script");
                Log.Write(e, "DynamicDebuggerVisualizer.DeployLinqScript");
                throw;
            }
        }

        private string FindAvailableFileName(string targetFolder, string fileName)
        {
            int fileCount = 0;
            string path = Path.Combine(targetFolder, fileName);
            const string linq = ".linq";
            const string sep = "_";
            StringBuilder b = new StringBuilder(path + linq);
            while (FS.FileSystem.File.Exists(b.ToString()))
            {
                ++fileCount;
                b.Clear();
                b.Append(path);
                b.Append(sep);
                b.Append(fileCount);
                b.Append(linq);
            }

            return b.ToString();
        }

        /// <summary>
        /// Shows the visualizer.
        /// </summary>
        /// <returns></returns>
        public void OpenLinqPad(string linqQueryFileName, string linqPadInstallationPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = "LINQPad.exe",
                WorkingDirectory = Path.GetFullPath(linqPadInstallationPath),
                Arguments = linqQueryFileName + " -run"
            };

            Log.Write("About to start LINQPad with these parameters: {0}, {1}", startInfo.FileName, startInfo.Arguments);

            Process process = Process.Start(startInfo);
            if (process != null)
            {
                process.WaitForInputIdle(-1);
                process.Dispose();
            }

            Log.Write("LINQPad Successfully started");
        }

        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            string vsVersion = VisualStudioVersionHelper.FindCurrentVisualStudioVersion();
            Log.VisualStudioVersion = vsVersion;
            Exception exception = null;
            try
            {
                Message message = GetMessage(objectProvider);

                DeployLinqScript(message, vsVersion);

                string linqQueryFileName = Path.Combine(CommonFolderPaths.DefaultLinqPadQueryFolder, message.AssemblyName, message.FileName);
                string linqPadInstallationPath = CommonRegistryConfigurations.GetLINQPadInstallationPath(vsVersion);

                OpenLinqPad(linqQueryFileName, linqPadInstallationPath);

                string linqPadExePath = Path.Combine(linqPadInstallationPath, "LINQPad.exe");
                string linqPadVersion = FileVersionInfo.GetVersionInfo(linqPadExePath).FileDescription;

                SendInputToLinqPad(linqPadVersion);
            }
            catch (ThreadAbortException)
            {
                // Catch exception and do nothing
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                const string context = "Error during LINQPad execution";
                Log.Write(ex, context);
                ex.Capture(vsVersion, message: context);
                exception = ex;
            }

            windowService.ShowDialog(new TemporaryForm(exception));
        }

        #region [ Private Static Methods ]

        /// <summary>
        /// Retrieves the message data from the object provider
        /// </summary>
        /// <param name="objectProvider"></param>
        /// <returns></returns>
        private static Message GetMessage(IVisualizerObjectProvider objectProvider)
        {
            Stream dataStream = objectProvider.GetData();

            if (dataStream.Length == 0)
                return null;

            BinaryFormatter formatter = new BinaryFormatter();
            Message message = (Message)formatter.Deserialize(dataStream);

            Log.Write($"Message content - \t {message}");

            return message;
        }

        /// <summary>
        /// Sends the input to LINQPad. Simulates key inputs to run a linq script (F5)
        /// </summary>
        private static void SendInputToLinqPad(string linqPadVersion)
        {
            try
            {
                WindowHandle linqPad = TopLevelWindowUtils.FindWindow(wh => wh.GetWindowText().Contains($"LINQPad {linqPadVersion}"));
                IntPtr intPtr = linqPad.RawPtr;
                ShowWindowAsync(intPtr, SwShowNormal);
                SetForegroundWindow(intPtr);
            }
            catch (Exception e)
            {
                Log.Write(e, "Error during LINQPad Sending inputs");
            }
        }

        #endregion

        #region [ DllImport ]
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32")]
        private static extern bool SetForegroundWindow(IntPtr hwnd);
        #endregion

#if TEST
        internal static void TestShowVisualizer(Message msg)
        {
            VisualizerDevelopmentHost visualizerHost = new VisualizerDevelopmentHost(msg, typeof(DynamicDebuggerVisualizer));
            visualizerHost.ShowVisualizer();
        }
#endif
    }
}