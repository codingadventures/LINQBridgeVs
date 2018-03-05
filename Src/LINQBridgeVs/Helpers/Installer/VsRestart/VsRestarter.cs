using System;
using BridgeVs.Helper.Installer.VsRestart.Arguments;
using EnvDTE;
using Process = System.Diagnostics.Process;
namespace BridgeVs.Helper.Installer.VsRestart
{
    internal static class VsRestarter
    {
        internal static void Restart(this DTE dte)
        {
            var currentProcess = Process.GetCurrentProcess();

            var parser = new ArgumentParser(dte.CommandLineArguments);

            var builder = new RestartProcessBuilder()
                .WithDevenv(currentProcess.MainModule.FileName)
                .WithArguments(parser.GetArguments());

            var openedItem = dte.GetOpenedItem();
            if (openedItem != OpenedItem.None)
            {
                if (openedItem.IsSolution)
                {
                    builder.WithSolution(openedItem.Name);
                }
                else
                {
                    builder.WithProject(openedItem.Name);
                }
            }

            builder.WithElevatedPermission();


            const string commandName = "File.Exit";
            var closeCommand = dte.Commands.Item(commandName);

            CommandEvents closeCommandEvents = null;
            if (closeCommand != null)
            {
                closeCommandEvents = dte.Events.CommandEvents[closeCommand.Guid, closeCommand.ID];
            }

            // Install the handler
            var handler = new VisualStudioCommandInvoker(dte.Events.DTEEvents, closeCommandEvents, builder.Build());

            if (closeCommand != null && closeCommand.IsAvailable)
            {
                // if the Exit commad is present, execute it with all gracefulls dialogs by VS
                dte.ExecuteCommand(commandName);
            }
            else
            {
                // Close brutally
                dte.Quit();
            }
        }

        private static OpenedItem GetOpenedItem(this DTE dte)
        {
            if (dte.Solution != null && dte.Solution.IsOpen)
            {
                if (string.IsNullOrEmpty(dte.Solution.FullName))
                {
                    Array activeProjects = (Array)dte.ActiveSolutionProjects;
                    if (activeProjects != null && activeProjects.Length > 0)
                    {
                        var currentOpenedProject = (Project)activeProjects.GetValue(0);
                        if (currentOpenedProject != null)
                        {
                            return new OpenedItem(currentOpenedProject.FullName, false);
                        }
                    }
                }
                else
                {
                    return new OpenedItem(dte.Solution.FullName, true);
                }
            }

            return OpenedItem.None;
        }

    }
}
