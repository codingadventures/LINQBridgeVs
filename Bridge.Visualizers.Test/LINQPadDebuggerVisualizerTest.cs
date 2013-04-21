using System;
using Bridge.Test.AssemblyModel;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bridge.Visualizers.Test
{
    [TestClass]
    public class LINQPadDebuggerVisualizerTest
    {
        [TestMethod]
        public void LINQPadDebuggerVisualizerShowTest()
        {

            var c = new VisualizationTestClass();

            var myHost = new VisualizerDevelopmentHost(c, typeof(LINQPadDebuggerVisualizer), typeof(LINQPadDebuggerVisualizerObjectSource));
            myHost.ShowVisualizer();
        }
    }

}
