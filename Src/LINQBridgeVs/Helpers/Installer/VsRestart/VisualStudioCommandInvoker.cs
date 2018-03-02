using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace FirstTimeConfigurator.VsRestart
{
    internal class VisualStudioCommandInvoker
    {
        private readonly ProcessStartInfo _startInfo;
        private readonly CommandEvents _closeCommandEvents;
        private readonly DTEEvents _dTEEvents;

        public VisualStudioCommandInvoker(DTEEvents dTEEvents, CommandEvents closeCommandEvents, ProcessStartInfo processStartInfo)
        {
            _dTEEvents = dTEEvents;
            _closeCommandEvents = closeCommandEvents;
            _startInfo = processStartInfo;

            dTEEvents.OnBeginShutdown += DTEEvents_OnBeginShutdown;
            if (closeCommandEvents != null)
            {
                closeCommandEvents.AfterExecute += CommandEvents_AfterExecute;
            }
        }

        void CommandEvents_AfterExecute(string guid, int id, object customIn, object customOut)
        {
            _dTEEvents.OnBeginShutdown -= DTEEvents_OnBeginShutdown;
            if (_closeCommandEvents != null)
            {
                _closeCommandEvents.AfterExecute -= CommandEvents_AfterExecute;
            }
        }

        void DTEEvents_OnBeginShutdown()
        {
            if (StartProcessSafe(_startInfo, DisplayError) == ProcessExecutionResult.Ok)
            {
                //currentProcess.Kill();
            }
        }

        private static ProcessExecutionResult StartProcessSafe(ProcessStartInfo startInfo, Action<Exception, ProcessExecutionResult> exceptionHandler)
        {
            ProcessExecutionResult result = ProcessExecutionResult.Ok;

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                result = ProcessExecutionResult.Exception;
                var winex = ex as System.ComponentModel.Win32Exception;

                // User has denied auth through UAC
                if (winex != null && winex.NativeErrorCode == 1223)
                {
                    result = ProcessExecutionResult.AuthDenied;
                }

                exceptionHandler(ex, result);
            }

            return result;
        }

        private static void DisplayError(Exception ex, ProcessExecutionResult status)
        {
            IVsOutputWindowPane outputPane = Package.GetGlobalService(typeof(SVsGeneralOutputWindowPane)) as IVsOutputWindowPane;

            outputPane.Activate();

            if (status == ProcessExecutionResult.AuthDenied)
            {
                outputPane.OutputString("Visual Studio restart operation was cancelled by the user." + Environment.NewLine);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("An exceptions has been trown while trying to start an elevated Visual Studio, see details below.");
                sb.AppendLine(ex.ToString());

                string diagnostics = sb.ToString();

                outputPane.OutputString(diagnostics);
                IVsActivityLog log = Package.GetGlobalService(typeof(SVsActivityLog)) as IVsActivityLog;
                log.LogEntry((uint)__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, "MidnightDevelopers.VsRestarter", diagnostics);
            }

            //EnvDTE.OutputWindow.OutputWindow.Parent.Activate();
        }
    }
}