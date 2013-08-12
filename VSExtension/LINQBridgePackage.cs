using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Bridge.VSExtension;
using EnvDTE80;
using LINQBridge.VSExtension.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace LINQBridge.VSExtension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [Guid(GuidList.GuidBridgeVsExtensionPkgString)]
    public sealed class LINQBridgePackage : Package
    {


        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            var dte = (DTE2)GetService(typeof(SDTE));
            var bridge = new LINQBridgeExtension(dte);

            //// Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null == mcs) return;

            //// Create the command for the menu item.
            var enableCommand = new CommandID(GuidList.GuidBridgeVsExtensionCmdSet, (int)PkgCmdIdList.CmdIdEnableBridge);
            var menuItemEnable = new OleMenuCommand((s, e) => bridge.Execute(CommandAction.Enable), enableCommand);
            menuItemEnable.BeforeQueryStatus += (s, e) => bridge.UpdateCommand(menuItemEnable, CommandAction.Enable);


            var disableCommand = new CommandID(GuidList.GuidBridgeVsExtensionCmdSet, (int)PkgCmdIdList.CmdIdDisableBridge);
            var menuItemDisable = new OleMenuCommand((s, e) => bridge.Execute(CommandAction.Disable), disableCommand);
            menuItemDisable.BeforeQueryStatus += (s, e) => bridge.UpdateCommand(menuItemDisable, CommandAction.Disable);

            var aboutCommand = new CommandID(GuidList.GuidBridgeVsExtensionCmdSet, (int)PkgCmdIdList.CmdIdAbout);
            var menuItemAbout = new OleMenuCommand((s, e) => { var about = new About(); about.ShowDialog(); }, aboutCommand);


            mcs.AddCommand(menuItemEnable);
            mcs.AddCommand(menuItemDisable);
            mcs.AddCommand(menuItemAbout);
        }
        #endregion
    }
}
