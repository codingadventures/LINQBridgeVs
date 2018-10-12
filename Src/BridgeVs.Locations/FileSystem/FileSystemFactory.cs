using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace BridgeVs.Shared.FileSystem
{
    public static class FileSystemFactory
    {
        private static readonly Lazy<IFileSystem> RealFileSystem = new Lazy<IFileSystem>(() => new System.IO.Abstractions.FileSystem());

        public static IFileSystem FileSystem => RealFileSystem.Value;
    }
}
