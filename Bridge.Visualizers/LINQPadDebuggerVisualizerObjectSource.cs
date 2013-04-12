using System.IO;
using Bridge.Grapple;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace Bridge.Visualizers
{

    public class LINQPadDebuggerVisualizerObjectSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData)
        {
            var busChannel = Bus.Instance;
            busChannel.Add(target);
            busChannel.BroadCast();

        }
    }
}
