using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Bridge.Visualizers.Properties;
using Bridge.Visualizers.Template;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace Bridge.Visualizers
{
    /// <summary>
    /// 
    /// </summary>
    public class LINQPadDebuggerVisualizer : DialogDebuggerVisualizer
    {

        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            string outputFileName;

            using (var stream = new StreamReader(objectProvider.GetData()))
            {
                outputFileName = stream.ReadToEnd();
            }
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
                                                Resources.LINQPadQuery, outputFileName) + " " + Resources.LINQPadCommands,

                                    };

                process.StartInfo = startInfo;
                process.Start();
            }
        }
    }


}
