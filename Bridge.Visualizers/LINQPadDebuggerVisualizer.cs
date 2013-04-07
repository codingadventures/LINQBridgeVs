using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using Bridge.Visualizers.Properties;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace Bridge.Visualizers
{
    /// <summary>
    /// 
    /// </summary>
    public class LINQPadDebuggerVisualizer : DialogDebuggerVisualizer
    {
        private delegate void AsyncMethodCaller(IVisualizerObjectProvider objectProvider);

        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {

            var asyncVisualizerGetData = new AsyncMethodCaller(AsyncVisualizerGetData.ManipulateData);

            var result = asyncVisualizerGetData.BeginInvoke(objectProvider, delegate { }, 0);

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
                                                Resources.LINQPadQuery) + " " + Resources.LINQPadCommands,

                                    };

                process.StartInfo = startInfo;
                process.Start();
            }

            asyncVisualizerGetData.EndInvoke(result);

            result.AsyncWaitHandle.Close();


        }
    }

    public class LINQPadDebuggerVisualizerObjectSource : VisualizerObjectSource
    {

    }

    internal static class AsyncVisualizerGetData
    {

        public static void ManipulateData(IVisualizerObjectProvider objectProvider)
        {
            var debugStream = objectProvider.GetData();
            var streamLength = debugStream.Length;

            var buffer = new byte[streamLength];
            using (var br = new BinaryReader(debugStream))
                br.Read(buffer, 0, (int)streamLength);

            using (var mmf = MemoryMappedFile.CreateNew(@"Debug", streamLength, MemoryMappedFileAccess.ReadWrite))
            {
                using (var accessor = mmf.CreateViewAccessor())
                {
                    accessor.WriteArray(0, buffer, 0, (int)streamLength);
                }
            }
        }


    }
}
