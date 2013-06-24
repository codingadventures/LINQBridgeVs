using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LINQBridge.DynamicCore.Utils
{
    public static class AssemblyFinder
    {
        private const string SearchPattern = "*{0}*{1}";

        private static readonly Func<string, bool> IsSystemAssembly =
            name => name.Contains("Microsoft") || name.Contains("System") || name.Contains("mscorlib");

        public static IFileSystem FileSystem { get; set; }

        public static IEnumerable<string> GetReferencedAssembliesPath(this _Assembly assembly, bool includeSystemAssemblies = false)
        {
            var retPaths = new List<string>();

            var referencedAssemblies = assembly.GetReferencedAssemblies()
                                               .Where(name => includeSystemAssemblies || !IsSystemAssembly(name.Name))
                                               .Select(name => name.Name)
                                               .ToList();

            if (!referencedAssemblies.Any()) return retPaths;



            var currentAssemblyPath = FileSystem.DirectoryInfo.FromDirectoryName(assembly.Location);
            Logging.Log.Write("currentAssemblyPath: {0}", currentAssemblyPath);

            if (currentAssemblyPath == null) return Enumerable.Empty<string>();


            referencedAssemblies
                .ForEach(s =>
                             {
                                 retPaths.Add(FindPath(s, currentAssemblyPath.FullName));
                                 Logging.Log.Write("Assembly Path {0}", s);
                             });


            return retPaths.Where(s => !string.IsNullOrEmpty(s));
        }

        internal static string FindPath(string fileToSearch, string rootPath)
        {
            if (rootPath == null) return string.Empty;

            try
            {
                var fileName = FileSystem.Path.GetFileNameWithoutExtension(fileToSearch);
                var extension = FileSystem.Path.GetExtension(fileToSearch);
                Logging.Log.Write("SearchPattern: {0}", string.Format(SearchPattern, fileToSearch, extension));

                var file = FileSystem.Directory
                    .EnumerateFiles(rootPath, string.Format(SearchPattern, fileName, extension), System.IO.SearchOption.AllDirectories)
                    .AsParallel()
                    .OrderByDescending(info => FileSystem.FileInfo.FromFileName(info).LastAccessTime)
                    .FirstOrDefault();

                if (file == null)
                {
                    var parent = FileSystem.DirectoryInfo.FromDirectoryName(rootPath).Parent;
                    return parent != null ? FindPath(fileToSearch, parent.FullName) : string.Empty;
                }


                Logging.Log.Write("File Found {0}", file);
                return file;
            }
            catch (Exception e)
            {
                Logging.Log.Write(e, "FindPath");
                throw;
            }


        }


    }
}
