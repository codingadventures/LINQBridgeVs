using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using LINQBridge.Logging;

namespace LINQBridge.DynamicCore.Utils
{
    public static class AssemblyFinder
    {
        private const string SearchPattern = "*{0}*.dll";

        private static readonly Func<string, bool> IsSystemAssembly =
            name => name.Contains("Microsoft") || name.Contains("System") || name.Contains("mscorlib");

        public static IFileSystem FileSystem { private get; set; }

        public static IEnumerable<string> GetReferencedAssembliesPath(this _Assembly assembly, bool includeSystemAssemblies = false)
        {
            var retPaths = new List<string>();

            var referencedAssemblies = assembly.GetReferencedAssemblies()
                                               .Where(name => includeSystemAssemblies || !IsSystemAssembly(name.Name))
                                               .Select(name => name.Name)
                                               .ToList();

            if (!referencedAssemblies.Any()) return retPaths;
            Log.Write(string.Format("There are {0} referenced non system assemblies", referencedAssemblies.Count));


            var currentAssemblyPath = FileSystem.Path.GetDirectoryName(assembly.Location);
            if (currentAssemblyPath == null) return Enumerable.Empty<string>();

            Log.Write("currentAssemblyPath: {0}",  currentAssemblyPath);

           
            referencedAssemblies
                .ForEach(s =>
                             {
                                 Log.Write("Referenced Assembly {0} ", s);
                                 retPaths.Add(FindPath(s, currentAssemblyPath));
                             });


            return retPaths.Where(s => !string.IsNullOrEmpty(s));
        }

        internal static string FindPath(string fileToSearch, string rootPath)
        {
            if (rootPath == null) return string.Empty;

            try
            {
              
                Log.Write("SearchPattern: {0}", string.Format(SearchPattern, fileToSearch));

                var file = FileSystem.Directory
                    .EnumerateFiles(rootPath, string.Format(SearchPattern, fileToSearch), System.IO.SearchOption.AllDirectories)
                    .AsParallel()
                    .OrderByDescending(info => FileSystem.FileInfo.FromFileName(info).LastAccessTime)
                    .FirstOrDefault();

                if (file == null)
                {
                    var parent = FileSystem.DirectoryInfo.FromDirectoryName(rootPath).Parent;
                    return parent != null ? FindPath(fileToSearch, parent.FullName) : string.Empty;
                }


                Log.Write("File Found {0}", file);
                return file;
            }
            catch (Exception e)
            {
                Log.Write(e, "FindPath");
                throw;
            }


        }


    }
}
