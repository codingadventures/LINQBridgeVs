using Microsoft.VisualStudio.DebuggerVisualizers;

namespace Bridge.Visualizers
{

    internal static class AsyncVisualizerGetData
    {

        public static void ManipulateData(IVisualizerObjectProvider objectProvider)
        {
            objectProvider.GetData();
        }


    }
}
