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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using LINQBridgeVs.Logging;

namespace LINQBridgeVs.TypeMapper
{
    /// <summary>
    /// Maps all the types of a given assembly to the type T of the debugger visualizer.
    /// It can map all the Basic DotNet Framework types like: System.Linq.*, System.*, System.Collection.Generic.*
    /// </summary>
    public class VisualizerTypeMapper
    {
        private const string DotNetFrameworkVisualizerName = "DotNetDynamicVisualizerType.V{0}.dll";

        private readonly VisualizerAttributeInjector _visualizerAttributeInjector;


        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizerTypeMapper"/> class.
        /// </summary>
        /// <param name="sourceVisualizerAssemblyLocation"></param>
        public VisualizerTypeMapper(string sourceVisualizerAssemblyLocation)
        {
            Log.Configure("LINQBridgeVs", "Type Mapper");
            _visualizerAttributeInjector = new VisualizerAttributeInjector(sourceVisualizerAssemblyLocation);
        }

        /// <summary>
        /// Maps the dot net framework types. If the file already exists for a given vs version it won't be
        /// regenerated.
        /// </summary>
        /// <param name="targetVisualizerInstallationPath">The target visualizer installation path.</param>
        /// <param name="vsVersion">The vs version.</param>
        /// <param name="sourceVisualizerAssemblyLocation">The source visualizer assembly location.</param>
        /// <returns></returns>
        public static void MapDotNetFrameworkTypes(IEnumerable<string> targetVisualizerInstallationPath,
            string vsVersion, string sourceVisualizerAssemblyLocation)
        {
            if (targetVisualizerInstallationPath == null)
                throw new ArgumentException(@"Installation Path/s cannot be null", "targetVisualizerInstallationPath");

            if (string.IsNullOrEmpty(vsVersion))
                throw new ArgumentException(@"Visual Studio Version cannot be null", "vsVersion");

            if (string.IsNullOrEmpty(sourceVisualizerAssemblyLocation))
                throw new ArgumentException(@"Visualizer Assembly Location cannot be null",
                    "sourceVisualizerAssemblyLocation");


            var visualizerFileName = string.Format(DotNetFrameworkVisualizerName, vsVersion);

            var visualizerInstallationPath = targetVisualizerInstallationPath as IList<string> ??
                                             targetVisualizerInstallationPath.ToList();

            var visualizerInjector = new VisualizerAttributeInjector(sourceVisualizerAssemblyLocation);

            //Map all the possible System  types
            var systemLinqTypes = typeof(IOrderedEnumerable<>).Assembly
                .GetTypes()
                .Where(type => type != null
                               && (
                                   (type.IsClass && type.IsSerializable)
                                   ||
                                   type.IsInterface
                                   ||
                                   type.Name.Contains("Iterator")
                                  )
                               && !(type.Name.Contains("Func") || type.Name.Contains("Action"))
                               && !string.IsNullOrEmpty(type.Namespace));

            //Map all the possible list types
            var systemGenericsTypes = typeof(IList<>).Assembly
                .GetTypes()
                .Where(type => type != null
                               && (
                                   (type.IsClass && type.IsSerializable)
                                   ||
                                   type.IsInterface
                                  )
                               && !string.IsNullOrEmpty(type.Namespace)
                               && type.IsPublic)
                .Where(type =>
                    !type.Name.Contains("ValueType")
                    && !type.Name.Contains("IFormattable")
                    && !type.Name.Contains("IComparable")
                    && !type.Name.Contains("IConvertible")
                    && !type.Name.Contains("IEquatable")
                    && !type.Name.Contains("Object")
                    && !type.Name.Contains("ICloneable")
                    && !type.Name.Contains("String")
                    && !type.Name.Contains("IDisposable"));


            systemLinqTypes.ForEach(visualizerInjector.MapType);
            systemGenericsTypes.ForEach(visualizerInjector.MapType);

            visualizerInstallationPath.ForEach(debuggerVisualizerPath =>
            {
                CreateDirWithPermission(debuggerVisualizerPath);
                var location = Path.Combine(debuggerVisualizerPath, visualizerFileName);
                visualizerInjector.SaveDebuggerVisualizer(location);
            });
        }

        /// <summary>
        /// Maps the assembly.
        /// </summary>
        /// <param name="targetAssemblyToMap">The target assembly to map.</param>
        public void MapAssembly(string targetAssemblyToMap)
        {
            _visualizerAttributeInjector.MapTypesFromAssembly(targetAssemblyToMap);
        }


        /// <summary>
        /// Saves the specified debugger visualizer assembly to a given Path.
        /// </summary>
        /// <param name="debuggerVisualizerPath">The debugger visualizer assembly location.</param>
        /// <param name="fileName"></param>
        private void Save(string debuggerVisualizerPath, string fileName)
        {
            var debuggerVisualizerAssemblyLocation = debuggerVisualizerPath + fileName;

            CreateDirWithPermission(debuggerVisualizerPath);

            _visualizerAttributeInjector.SaveDebuggerVisualizer(debuggerVisualizerAssemblyLocation);
        }

        /// <summary>
        /// Saves the specified debugger visualizer to a given set of Paths.
        /// </summary>
        /// <param name="debuggerVisualizerPaths">The debugger visualizer Paths.</param>
        /// <param name="fileName"></param>
        public void Save(IEnumerable<string> debuggerVisualizerPaths, string fileName)
        {
            debuggerVisualizerPaths.ForEach(debuggerVisualizerPath => Save(debuggerVisualizerPath, fileName));
        }

        private static void CreateDirWithPermission(string folder)
        {

            var sec = new DirectorySecurity();
            // Using this instead of the "Everyone" string means we work on non-English systems.
            var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var rule = new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None,
                AccessControlType.Allow);


            if (Directory.Exists(folder))
            {
                var di = new DirectoryInfo(folder);
                var security = di.GetAccessControl();
                security.AddAccessRule(rule);
                security.SetAccessRule(rule);
                di.SetAccessControl(security);
                return;

            }
            sec.AddAccessRule(rule);
            Directory.CreateDirectory(folder, sec);
        }
    }
}
