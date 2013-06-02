using Microsoft.VisualStudio.DebuggerVisualizers;
using LINQBridge.DynamicCore;

namespace LINQBridge.DynamicVisualizer.V10
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicDebuggerVisualizerV10 : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            var dataStream = objectProvider.GetData();

            var dynamicDebuggerVisualizer = new DynamicDebuggerVisualizer();
            dynamicDebuggerVisualizer.ShowVisualizer(dataStream);
        }
    }


}
