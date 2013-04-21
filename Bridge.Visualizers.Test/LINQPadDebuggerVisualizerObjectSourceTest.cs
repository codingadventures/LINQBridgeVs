using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Bridge.Visualizers.Test
{
    [TestClass]
    public class LINQPadDebuggerVisualizerObjectSourceTest
    {
        [TestMethod]
        public void DeployScriptTest()
        {
            var cVisualizerObjectSource = new LINQPadDebuggerVisualizerObjectSource();
            cVisualizerObjectSource.DeployScripts(typeof(string));

            var fileName = DateTime.Now.ToString(LINQPadDebuggerVisualizerObjectSource.FileNameFormat);

            var dstScriptPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                             "LINQPad Queries");

            var fileNamePath = Path.Combine(dstScriptPath,string.Format(fileName, typeof(string).FullName));

            Assert.IsTrue(File.Exists(fileNamePath));

        }


    }
}
