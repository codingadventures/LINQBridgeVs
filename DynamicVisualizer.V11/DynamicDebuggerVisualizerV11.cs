using LINQBridge.DynamicCore;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace LINQBridge.DynamicVisualizer.V11
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicDebuggerVisualizerV11 : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            var dynamicDebuggerVisualizer = new DynamicDebuggerVisualizer();

            var dataStream = objectProvider.GetData();
            dynamicDebuggerVisualizer.ShowVisualizer(dataStream);
        }
    }


}
