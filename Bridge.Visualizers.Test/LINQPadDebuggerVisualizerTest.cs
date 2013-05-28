using System;
using System.Collections.Generic;
using System.IO;
using LINQBridge.DynamicVisualizers;
using LINQBridge.DynamicVisualizers.Template;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Test;

namespace DynamicVisualizers.Test
{
    [TestClass]
    public class DynamicVisualizerTest
    {
        private Message _message = new Message
                                       {
                                           FileName = DateTime.Now.ToString(DynamicDebuggerVisualizerObjectSource.FileNameFormat),
                                           TypeFullName = typeof(CustomType1).FullName,
                                           TypeNamespace = typeof(CustomType1).Namespace,
                                           TypeLocation = typeof(CustomType1).Assembly.Location
                                       };
        [TestMethod]
        public void LINQPadDebuggerVisualizerShowTest()
        {

            var c = new List<VisualizationTestClass>();
            c.Add(new VisualizationTestClass());
            var myHost = new VisualizerDevelopmentHost(c, typeof(DynamicDebuggerVisualizer), typeof(DynamicDebuggerVisualizerObjectSource));
            myHost.ShowVisualizer();
        }

        [TestMethod]
        public void DeployScriptTest()
        {
            //var cVisualizerObjectSource = new DynamicDebuggerVisualizer();
            DynamicDebuggerVisualizer.DeployLinqScripts(_message);

            
            var dstScriptPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                             "LINQPad Queries");

            var fileNamePath = Path.Combine(dstScriptPath, string.Format(_message.FileName, _message.TypeFullName));

            Assert.IsTrue(File.Exists(fileNamePath));

        }
    }

}
