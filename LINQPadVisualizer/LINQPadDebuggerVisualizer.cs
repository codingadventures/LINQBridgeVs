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

            var process = new System.Diagnostics.Process();

            var startInfo = new System.Diagnostics.ProcessStartInfo
                                {
                                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                                    FileName = Resources.LINQPadExe,
                                    Arguments = Resources.LINQPadQuery
                                };

            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit(2000);

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
            var buffer = new byte[debugStream.Length];
            using (var br = new BinaryReader(debugStream))
                br.Read(buffer, 0, (int)debugStream.Length);

            using (var mmf = MemoryMappedFile.CreateNew(@"Debug", debugStream.Length, MemoryMappedFileAccess.ReadWrite))
            {
                using (var accessor = mmf.CreateViewAccessor())
                {
                    accessor.WriteArray(0, buffer, 0, (int)debugStream.Length);
                }
            }
        }


    }
}
