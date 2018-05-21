using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeVs.VsPackage.Helper
{
    public static class ActivityLogger
    {
        public static void VsLog(this IVsActivityLog log, string message)
        {
            if (log == null) return;

            int hr = log.LogEntry((UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, message, "Error in LINQBridgeVs");
        }
    }
}
