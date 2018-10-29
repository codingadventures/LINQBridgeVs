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

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;
using BridgeVs.VsPackage.Helper;
using BridgeVs.VsPackage.Helper.Configuration;
using BridgeVs.VsPackage.Helper.Installer;
using BridgeVs.VsPackage.Helper.Settings;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BridgeVs.VsPackage.Package
{
    /// <inheritdoc />
    ///  <summary>
    ///  This is the class that implements the package exposed by this assembly.
    ///  The minimum requirement for a class to be considered a valid package for Visual Studio
    ///  is to implement the IVsPackage interface and register itself with the shell.
    ///  This package uses the helper classes defined inside the Managed Package Framework (MPF)
    ///  to do it: it derives from the Package class that provides the implementation of the 
    ///  IVsPackage interface and uses the registration attributes defined in the framework to 
    ///  register itself and its components with the shell.
    ///  </summary>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.EmptySolution_string)]
    [Guid(GuidList.GuidBridgeVsExtensionPkgString)]
    [ProvideOptionPage(typeof(PackageSettings), "Bridge Vs", "General", 0, 0, true)]
    public sealed class BridgeVsPackage : Microsoft.VisualStudio.Shell.Package
    {
        private DTE _dte;
        private DTEEvents _dteEvents;

        //if this is not null means vs has to restart
        private Welcome _welcomePage;
        private bool? _installationResult;
        private class Error
        {
            public string Body;
            public string Title;
        }
        private Error _error;
        public static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        private PackageSettings _packageSettings;
        #region Package Members

        /// <inheritdoc />
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            _dte = (DTE)GetService(typeof(SDTE));

            _dteEvents = _dte.Events.DTEEvents;

            _dteEvents.OnStartupComplete += _dteEvents_OnStartupComplete;

            BridgeVsExtension bridge = new BridgeVsExtension(_dte);
            bool isLinqBridgeVsConfigured = PackageConfigurator.IsBridgeVsConfigured(_dte.Version);

            // Add our command handlers for menu(commands must exist in the.vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null == mcs) return;

            // Create the command for the menu item.
            CommandID enableCommand = new CommandID(GuidList.GuidBridgeVsExtensionCmdSet, (int)PkgCmdIdList.CmdIdEnableBridge);
            OleMenuCommand menuItemEnable = new OleMenuCommand((s, e) => bridge.Execute(CommandAction.Enable), enableCommand);
            menuItemEnable.BeforeQueryStatus += (s, e) => bridge.UpdateCommand(menuItemEnable, CommandAction.Enable);

            CommandID disableCommand = new CommandID(GuidList.GuidBridgeVsExtensionCmdSet,
                (int)PkgCmdIdList.CmdIdDisableBridge);
            OleMenuCommand menuItemDisable = new OleMenuCommand((s, e) => bridge.Execute(CommandAction.Disable), disableCommand);
            menuItemDisable.BeforeQueryStatus += (s, e) => bridge.UpdateCommand(menuItemDisable, CommandAction.Disable);

            CommandID gettingStarted = new CommandID(GuidList.GuidBridgeVsExtensionCmdSet, (int)PkgCmdIdList.CmdIdGettingStarted);
            OleMenuCommand menuItemGettingStarted = new OleMenuCommand((s, e) => System.Diagnostics.Process.Start("https://github.com/codingadventures/LINQBridgeVs#getting-started"), gettingStarted);

            CommandID sendFeedback = new CommandID(GuidList.GuidBridgeVsExtensionCmdSet, (int)PkgCmdIdList.CmdIdFeedback);
            OleMenuCommand menuItemSendFeedback = new OleMenuCommand((s, e) => System.Diagnostics.Process.Start("https://github.com/codingadventures/LINQBridgeVs/issues"), sendFeedback);

            mcs.AddCommand(menuItemEnable);
            mcs.AddCommand(menuItemDisable);
            mcs.AddCommand(menuItemGettingStarted);
            mcs.AddCommand(menuItemSendFeedback);
            //Initialize Object Exporter settings

            Package:
            try
            {
                _packageSettings = (PackageSettings)GetDialogPage(typeof(PackageSettings));
            }
            catch
            {
                goto Package;
            }

            try
            { 
                //if first time user 
                if (isLinqBridgeVsConfigured)
                {
                    return;
                }

                if (!IsElevated)
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (Application.ResourceAssembly == null)
                        // ReSharper disable once HeuristicUnreachableCode
                        Application.ResourceAssembly = typeof(Welcome).Assembly;

                    _welcomePage = new Welcome(_dte);
                }
                else
                {
                    _installationResult = PackageConfigurator.Install(_dte.Version, _dte.Edition);
                }
            }
            catch (Exception e)
            {
                _error = new Error();
                _error.Body = $"{e.Message} %0A {e.StackTrace}";
                _error.Title = "Error during installation. 1.4.6";
                Exception innerException = e.InnerException;
                while (innerException != null)
                {
                    _error.Body += $"%0A {innerException.Message} {innerException.StackTrace}";
                    innerException = innerException.InnerException;
                }
                _error.Body = _error.Body.Replace(Environment.NewLine, "%0A");
            }
        }

        private void _dteEvents_OnStartupComplete()
        {
            _welcomePage?.Show();

            if (_error != null)
            {
                var boxresult = MessageBox.Show($"Configuration unsuccessful. Do you want to open an issue on GitHub?", "LINQBridgeVs: Error during the configuration", MessageBoxButton.YesNo);
                if (boxresult == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start($"https://github.com/codingadventures/LINQBridgeVs/issues/new?title={_error.Title}&body={_error.Body}");
                }
                _error = null;
            }

            if (_installationResult == null)
                return;

            var messageResult = MessageBox.Show("Do you want to send anonymous error reports to LINQBridgeVs?", "LINQBridgeVs: Error Tracking", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            _packageSettings.ErrorTrackingEnabled = messageResult == MessageBoxResult.Yes;
            _packageSettings.SaveSettingsToStorage();

            MessageBox.Show(_installationResult.Value
               ? "LINQBridgeVs has been successfully configured."
               : "LINQBridgeVs wasn't successfully configured.");
        }

        #endregion
    }
}