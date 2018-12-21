using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BridgeVs.VsPackage.Helper;
using BridgeVs.VsPackage.Helper.Configuration;
using BridgeVs.VsPackage.Helper.Settings;
using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Task = System.Threading.Tasks.Task;

namespace BridgeVs.VisualStudio.AsyncExtension
{

    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(GuidList.GuidBridgeVsExtensionPkgString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.EmptySolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideOptionPage(typeof(PackageSettings), "Bridge Vs", "General", 0, 0, true)]
    public sealed class VsAsyncPackage : AsyncPackage
    {  
        /// <summary>
        /// Initializes a new instance of the <see cref="VsAsyncPackage"/> class.
        /// </summary>
        public VsAsyncPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Query service asynchronously from the UI thread
            var dte = await GetServiceAsync(typeof(DTE)) as DTE;
            Assumes.Present(dte);
            BridgeVsExtension bridge = new BridgeVsExtension(dte);
            // Add our command handlers for menu(commands must exist in the.vsct file)
            OleMenuCommandService mcs = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Assumes.Present(mcs);

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
            OleMenuCommand menuItemSendFeedback = new OleMenuCommand((s, e) => System.Diagnostics.Process.Start("https://marketplace.visualstudio.com/items?itemName=codingadventures.linqbridgevs#review-details"), sendFeedback);

            mcs.AddCommand(menuItemEnable);
            mcs.AddCommand(menuItemDisable);
            mcs.AddCommand(menuItemGettingStarted);
            mcs.AddCommand(menuItemSendFeedback);

            bool isLinqBridgeVsConfigured = PackageConfigurator.IsBridgeVsConfigured(dte.Version);

            //if first time user 
            if (isLinqBridgeVsConfigured)
            {
                return;
            }

            //Initialize Object Exporter settings 
            PackageSettings packageSettings = null;
        Package:
            try
            {
                packageSettings = (PackageSettings) GetDialogPage(typeof(PackageSettings));
            }
            catch
            {
                goto Package;
            }

            DialogResult messageResult = MessageBox.Show("Do you want to send anonymous error reports to LINQBridgeVs?", "LINQBridgeVs: Error Tracking", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            packageSettings.ErrorTrackingEnabled = messageResult == DialogResult.Yes;
            packageSettings.SaveSettingsToStorage();


            PackageConfigurator.Install(dte.Version, dte.Edition);
        }

        #endregion
    }
}
