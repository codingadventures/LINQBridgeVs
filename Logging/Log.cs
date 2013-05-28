using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace LINQBridge.Logging
{
    public static class Log
    {

        private static readonly string LocalApplicationData =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private static readonly string LINQBridgeLogsDir = Path.Combine(LocalApplicationData, "LINQBridge");


        private static readonly string LogFile = Path.Combine(LINQBridgeLogsDir, "LINQBridge.txt");

        [Conditional("DEBUG")]
        public static void Write(Exception ex)
        {
            Write(ex, null);
        }
        [Conditional("DEBUG")]
        public static void Write(Exception ex, string context)
        {
            try
            {
                var text = string.Concat(new[]
                {
                    ex.GetType().Name,
                    ": ",
                    ex.Message,
                    "\r\n",
                    ex.StackTrace ?? ""
                });
                if (ex.InnerException != null)
                {
                    var text2 = text;
                    text = string.Concat(new[]
                    {
                        text2,
                        "\r\nINNER: ",
                        ex.InnerException.GetType().Name,
                        ex.InnerException.Message,
                        (ex.InnerException.StackTrace ?? "").Replace("\n", "\n  ")
                    });
                }
                if (!string.IsNullOrEmpty(context))
                {
                    text = context + " - \r\n" + text;
                }

                Write(text);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Writes a formatted message  
        /// </summary>
        /// <param name="msg">A composite format string (see Remarks) that contains text intermixed with zero or more format items, which correspond to objects in the <paramref name="args"/> array.</param><param name="args">An object array that contains zero or more objects to format. </param>
        [Conditional("DEBUG")]
        public static void Write(string msg, params object[] args)
        {
            try
            {
                if (!Directory.Exists(LINQBridgeLogsDir))
                {
                    try
                    {
                        var sec = new DirectorySecurity();
                        // Using this instead of the "Everyone" string means we work on non-English systems.
                        var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                        sec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                        Directory.CreateDirectory(LINQBridgeLogsDir, sec);
                    }
                    catch
                    {
                        return;
                    }
                }

                if (args != null && args.Length > 0)
                    msg = string.Format(CultureInfo.InvariantCulture, msg, args);


                File.AppendAllText(LogFile, string.Concat(new[]
                {
                     
                    DateTime.Now.ToString("o"),
                    " ",
                    msg.Trim(),
                    "\r\n\r\n"
                }));
            }
            catch
            {
            }
        }

        [Conditional("DEBUG")]
        public static void WriteIf(bool condition, string msg, params object[] args)
        {
            if (!condition) return;

            Write(msg, args);
        }

    }
}
