using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;
using GiovanniCampo.Bridge_VSExtension;
using Microsoft.VisualStudio.Shell;

namespace Bridge.VSExtension
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
    [Guid(GuidList.GuidBridgeVsExtensionPkgString)]
    public sealed class Bridge_VSExtensionPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public Bridge_VSExtensionPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
        //    Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            var dte = (DTE2)GetService(typeof(DTE));
            var bridge = new BridgeExtension(dte);

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null == mcs) return;

            // Create the command for the menu item.
            var enableCommand = new CommandID(GuidList.GuidBridgeVsExtensionCmdSet, (int)PkgCmdIdList.CmdIdEnableBridge);
            var menuItemEnable = new OleMenuCommand((s, e) => bridge.Execute(CommandAction.Enable), enableCommand);
            menuItemEnable.BeforeQueryStatus += (s, e) => bridge.UpdateCommand(menuItemEnable, CommandAction.Enable);


            var disableCommand = new CommandID(GuidList.GuidBridgeVsExtensionCmdSet, (int)PkgCmdIdList.CmdIdDisableBridge);
            var menuItemDisable = new OleMenuCommand((s, e) => bridge.Execute(CommandAction.Disable), disableCommand);

            mcs.AddCommand(menuItemEnable);
            mcs.AddCommand(menuItemDisable);
        }
        #endregion
    }
}
