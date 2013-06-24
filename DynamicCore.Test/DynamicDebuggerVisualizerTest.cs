using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using LINQBridge.DynamicCore;
using LINQBridge.DynamicCore.Template;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Test;
using System;
using System.IO;

namespace DynamicCore.Test
{
    [TestClass]
    public class DynamicDebuggerVisualizerTest
    {
        private readonly Message _message = new Message
                                                {
                                                    FileName = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                                                    TypeFullName = typeof (CustomType1).FullName,
                                                    TypeNamespace = typeof (CustomType1).Namespace,
                                                    TypeLocation = @"\Level1\Level2\Level2\"
                                                };

        //[TestMethod]
        //public void LINQPadDebuggerVisualizerShowTest()
        //{

        //    var c = new List<VisualizationTestClass>();
        //    c.Add(new VisualizationTestClass());
        //    var myHost = new VisualizerDevelopmentHost(c, typeof(DynamicDebuggerVisualizer), typeof(DynamicDebuggerVisualizerObjectSource));
        //    myHost.ShowVisualizer();
        //}
        private IFileSystem _fileSystem;
        [ClassInitialize]
        public void Init(TestContext ctx)
        {
            _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing is meh.") },
                { @"c:\demo\jQuery.js", new MockFileData("some js") },
                { @"c:\demo\image.gif", new MockFileData(new byte[] { 0x12, 0x34, 0x56, 0xd2 }) }
            });

        }

    [TestMethod]
        public void DeployScriptTest()
        {
            
           var cVisualizerObjectSource = new DynamicDebuggerVisualizer(_fileSystem);
           cVisualizerObjectSource.DeployLinqScripts(_message);

            
            var dstScriptPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                             "LINQPad Queries");

            var fileNamePath = Path.Combine(dstScriptPath, string.Format(_message.FileName, _message.TypeFullName));

            Assert.IsTrue(_fileSystem.File.Exists(fileNamePath));

        }
    }

}
