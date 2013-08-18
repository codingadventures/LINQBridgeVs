using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Mail;
using System.Security.AccessControl;
using System.Security.Principal;

namespace LINQBridge.Logging
{
    public static class Log
    {

        private static string _applicationName;
        private static readonly string LocalApplicationData =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private static readonly MailAddress MailAddressFrom = new MailAddress("linqbridge@gmail.com", "No Reply Log");
        private static string _logGzipFileName;


        private static string _logsDir;
        private static string _logTxtFilePath;
        private static SmtpClient _smtpClient;

        [Conditional("DEPLOY")]
        public static void Configure(string applicationName, SmtpClient smtpClient = null)
        {
            if (string.IsNullOrEmpty(applicationName))
                throw new ArgumentNullException("applicationName", "Name of the application must not be null!");

            _applicationName = applicationName;
            var logTxtFileName = string.Concat(_applicationName, ".txt");

            _logGzipFileName = string.Concat(_applicationName, ".gz");
            _logsDir = Path.Combine(LocalApplicationData, _applicationName);
            _logTxtFilePath = Path.Combine(_logsDir, logTxtFileName);

            _smtpClient = smtpClient;



        }

        [Conditional("DEPLOY")]
        public static void Write(Exception ex)
        {
            Write(ex, null);
        }

        [Conditional("DEPLOY")]
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

        [Conditional("DEPLOY")]
        public static void Write(string msg, params object[] args)
        {
            if (string.IsNullOrEmpty(_applicationName))
                throw new Exception("The Log is to be configured first. Call the Configure() method and pass the name of the application!");

            try
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
            catch
            {
            }
        }

        //   [Conditional("DEBUG")]
        [Conditional("DEPLOY")]
        public static void WriteIf(bool condition, string msg, params object[] args)
        {
            if (!condition) return;

            Write(msg, args);
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
                        var subject = string.Format("Log Report for {0}", _applicationName);

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
    }
}
