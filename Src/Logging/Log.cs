#region License
// Copyright (c) 2013 - 2018 Giovanni Campo
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
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net.Mail;
using System.Security.AccessControl;
using System.Security.Principal;

namespace BridgeVs.Logging
{
    public static class Log
    {

        private static string _applicationName;
        private static readonly string LocalApplicationData =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private static readonly MailAddress MailAddressFrom = new MailAddress("linqbridge@gmail.com", "No Reply Log");
        private static string _logGzipFileName;

        private static string _moduleName;
        private static string _logsDir;
        private static string _logTxtFilePath;
        private static SmtpClient _smtpClient;

        public static void Configure(string applicationName, string moduleName, SmtpClient smtpClient = null)
        {
            if (string.IsNullOrEmpty(applicationName))
                throw new ArgumentNullException(nameof(applicationName), "Name of the application must not be null!");

            _applicationName = applicationName;
            _moduleName = moduleName;

            var logTxtFileName = string.Concat(moduleName, ".txt");

            _logGzipFileName = string.Concat(_applicationName, ".gz");
            _logsDir = Path.Combine(LocalApplicationData, _applicationName);

            _logTxtFilePath = Path.Combine(_logsDir, logTxtFileName);

            _smtpClient = smtpClient;
        }

        public static void Write(Exception ex, string context = null)
        {
            try
            {
                var text = string.Concat(ex.GetType().Name, ": ", ex.Message, "\r\n", ex.StackTrace ?? "");
                if (ex.InnerException != null)
                {
                    var text2 = text;
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

            if (!Directory.Exists(_logsDir))
            {
                try
                {
                    var sec = new DirectorySecurity();
                    // Using this instead of the "Everyone" string means we work on non-English systems.
                    var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                    sec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                    Directory.CreateDirectory(_logsDir, sec);
                }
                catch
                {
                    return;
                }
            }

            if (args != null && args.Length > 0)
                msg = string.Format(CultureInfo.InvariantCulture, msg, args);


            File.AppendAllText(_logTxtFilePath, string.Concat(new[]
                {

                    DateTime.Now.ToString("o"),
                    " ",
                    msg.Trim(),
                    "\r\n\r\n"
                }));
        }

        /// <summary>
        /// Writes a formatted message  
        /// </summary>
        /// <param name="msg">A composite format string (see Remarks) that contains text intermixed with zero or more format items, which correspond to objects in the <paramref name="args"/> array.</param><param name="args">An object array that contains zero or more objects to format. </param>


        public static void Write(string msg, params object[] args)
        {
            if (string.IsNullOrEmpty(_applicationName))
            {
                _applicationName = "LINQBridgeVs";
                //throw new Exception("The Log is to be configured first. Call the Configure() method and pass the name of the application!");
            }
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
            if (!condition) return;

            InternalWrite(msg, args);
        }

        public static bool SendLogFileAsEmail(string emailTo)
        {
            if (_smtpClient == null)
                throw new Exception("Set the SmtpClient in the Configure() method to enable the Log sending emails");

            try
            {
                using (var inFile = File.OpenRead(_logTxtFilePath))
                {
                    using (var outFile = new MemoryStream())
                    {
                        using (var compress = new GZipStream(outFile,
                                                             CompressionMode.Compress))
                        {
                            inFile.CopyTo(compress);
                        }
                        var attachment = new Attachment(outFile, _logGzipFileName);
                        var subject = $"Log Report for {_applicationName}";

                        _smtpClient.Send(BuildMailMessage(emailTo, attachment, subject));

                        return true;

                    }
                }
            }
            catch (Exception e)
            {
                Write(e, "Problem Sending the Email");
                return false;
            }



        }

        private static MailMessage BuildMailMessage(string emailTo, Attachment attachment, string subject)
        {

            var mailAddressTo = new MailAddress(emailTo);
            var mailMessage = new MailMessage(MailAddressFrom, mailAddressTo) { Subject = subject };

            mailMessage.Attachments.Add(attachment);

            return mailMessage;
        }
#if NET35
        // Only useful before .NET 4
        public static void CopyTo(this Stream input, Stream output)
        {
            byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
#endif
    }
}
