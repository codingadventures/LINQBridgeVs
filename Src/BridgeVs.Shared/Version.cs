using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BridgeVs.Shared
{
    public static class AssemblyVersion
    {
        public static string VersionNumber(this Assembly assembly)
        { 
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }
    }
}
