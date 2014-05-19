#region License
// Copyright (c) 2013 Giovanni Campo
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using LINQBridgeVs.DynamicCore;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace LINQBridgeVs.DynamicVisualizer.V10
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicDebuggerVisualizerV10 : DialogDebuggerVisualizer
    {
        internal const string VsReferencedVersion = "10.0";
        internal const string TestRegistryKey = @"Software\LINQBridgeVs\10.0\Test";

        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            var dynamicDebuggerVisualizer = new DynamicDebuggerVisualizer();
            var dataStream = objectProvider.GetData();

            if (dataStream.Length == 0) return;

            var formToShow = dynamicDebuggerVisualizer.ShowVisualizer(dataStream, VsReferencedVersion);

            if (!IsTest())
                windowService.ShowDialog(formToShow);
        }

        private static bool IsTest()
        {
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(TestRegistryKey))
            {
                return key != null;
            }
        }
    }


}
