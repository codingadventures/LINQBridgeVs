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


using System.Collections.Generic;
using System.IO;
using System.Linq;
using LINQBridge.Logging;


namespace LINQBridge.TypeMapper
{
    /// <summary>
    /// Maps all the types of a given assembly to the type T of the debugger visualizer 
    /// </summary>

    public class VisualizerTypeMapper
    {
 
         
        private readonly VisualizerAttributeInjector _visualizerAttributeInjector;

         
        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizerTypeMapper"/> class.
        /// </summary>
        /// <param name="sourceVisualizerAssemblyLocation"></param>
        /// <param name="targetAssemblyToMap">The target assembly to Map with the Visualizer.</param>
        /// <param name="visualizerDescriptionName">Visualizer description.</param>
        public VisualizerTypeMapper(string sourceVisualizerAssemblyLocation, string targetAssemblyToMap, string visualizerDescriptionName)
        {
            Log.Configure("Type Mapper");
            _visualizerAttributeInjector = new VisualizerAttributeInjector(sourceVisualizerAssemblyLocation, targetAssemblyToMap, visualizerDescriptionName);
        }

        private static bool IsAlreadyDeployed(string location)
        {
            return File.Exists(location);
        }

        /// <summary>
        /// Creates the specified types to exclude.
        /// </summary>
        public void Create()
        {
            _visualizerAttributeInjector.MapSystemType(typeof(Dictionary<,>));
            _visualizerAttributeInjector.MapSystemType(typeof(List<>));
            _visualizerAttributeInjector.MapSystemType(typeof(IEnumerable<>));
            _visualizerAttributeInjector.MapSystemType(typeof(IOrderedEnumerable<>));

            _visualizerAttributeInjector.MapTypesFromAssembly();
        }

        /// <summary>
        /// Saves the specified debugger visualizer assembly to a given Path.
        /// </summary>
        /// <param name="debuggerVisualizerPath">The debugger visualizer assembly location.</param>
        /// <param name="fileName"></param>
        private void Save(string debuggerVisualizerPath,string fileName)
        {
            var debuggerVisualizerAssemblyLocation = debuggerVisualizerPath + fileName;

            if (!Directory.Exists(debuggerVisualizerPath))
                Directory.CreateDirectory(debuggerVisualizerPath);

            if (IsAlreadyDeployed(debuggerVisualizerAssemblyLocation))
                //Get all the custom attributes that map other type and import them into the current visualizer
                _visualizerAttributeInjector.SyncronizeMappedTypes(debuggerVisualizerAssemblyLocation);

            _visualizerAttributeInjector.SaveDebuggerVisualizer(debuggerVisualizerAssemblyLocation);
        }

        /// <summary>
        /// Saves the specified debugger visualizer to a given set of Paths.
        /// </summary>
        /// <param name="debuggerVisualizerPaths">The debugger visualizer Paths.</param>
        /// <param name="fileName"></param>
        public void Save(IEnumerable<string> debuggerVisualizerPaths, string fileName)
        {
            foreach (var debuggerVisualizerPath in debuggerVisualizerPaths)
            {
                Save(debuggerVisualizerPath,fileName);

            }
        }

    }
}
