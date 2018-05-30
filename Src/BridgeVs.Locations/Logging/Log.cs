#region License
// Copyright (c) 2013 - 2018 Coding Adventures
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

using BridgeVs.Shared.Common;
using System;
using System.Globalization;
using System.IO;

namespace BridgeVs.Shared.Logging
{
    public static class Log
    {
        private static string _logTxtFilePath = Path.Combine(CommonFolderPaths.LogFolderPath, "logs.txt");

        public static void Write(Exception ex, string context = null)
        {
            try
            {
                string text = string.Concat(ex.GetType().Name, ": ", ex.Message, "\r\n", ex.StackTrace ?? "");
                if (ex.InnerException != null)
                {
                    string text2 = text;
                    text = string.Concat(text2, "\r\nINNER: ", ex.InnerException.GetType().Name, ex.InnerException.Message, (ex.InnerException.StackTrace ?? "").Replace("\n", "\n  "));
                }
                if (!string.IsNullOrEmpty(context))
                {
                    text = context + " - \r\n" + text;
                }

                InternalWrite(text);
            }
            catch
            {
                // ignored
            }
        }

        private static void InternalWrite(string msg, params object[] args)
        {
            if (args != null && args.Length > 0)
                msg = string.Format(CultureInfo.InvariantCulture, msg, args);

            File.AppendAllText(_logTxtFilePath, string.Concat(new[]
                {
                    DateTime.Now.ToString("o"), " ", msg.Trim(), Environment.NewLine
                }));
        }

        /// <summary>
        /// Writes a formatted message  
        /// </summary>
        /// <param name="msg">A composite format string (see Remarks) that contains text intermixed with zero or more format items, which correspond to objects in the <paramref name="args"/> array.</param><param name="args">An object array that contains zero or more objects to format. </param>
        public static void Write(string msg, params object[] args)
        {
            try
            {
                InternalWrite(msg, args);
            }
            catch
            {
                // ignored
            }
        }

        public static void WriteIf(bool condition, string msg, params object[] args)
        {
            if (!condition)
                return;

            Write(msg, args);
        }

        public static void WriteIf(bool condition, Exception exception, string context)
        {
            if (!condition)
                return;

            Write(exception, context);
        }
    }
}
