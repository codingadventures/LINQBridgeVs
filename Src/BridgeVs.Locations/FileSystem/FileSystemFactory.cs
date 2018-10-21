using System;
using System.IO.Abstractions;

namespace BridgeVs.Shared.FileSystem
{
    public static class FileSystemFactory
    {
        private static readonly Lazy<IFileSystem> RealFileSystem = new Lazy<IFileSystem>(() => new System.IO.Abstractions.FileSystem());

        public static IFileSystem FileSystem => RealFileSystem.Value;
    }
}
