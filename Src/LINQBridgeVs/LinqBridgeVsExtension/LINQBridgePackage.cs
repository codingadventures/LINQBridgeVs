#region License
// Copyright (c) 2013 Giovanni Campo
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
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using LINQBridgeVs.Helper.Configuration;
using LINQBridgeVs.Helper;
using LINQBridgeVs.Helper.Forms;
using LINQBridgeVs.Logging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Process = System.Diagnostics.Process;

namespace LINQBridgeVs.Extension
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
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]
    [Guid(GuidList.GuidBridgeVsExtensionPkgString)]
    public sealed class LINQBridgeVsPackage : Package
    {

        private DTEEvents _dteEvents;
        private DTE _dte;
        private const string VisualStudioProcessName = "devenv";
        private DTE2 _mApplicationObject;

        DTEEvents _mPackageDteEvents;

        public DTE2 ApplicationObject
        {
            get
            {
                if (_mApplicationObject != null) return _mApplicationObject;
                // Get an instance of the currently running Visual Studio IDE
                var dte = (DTE)GetService(typeof(DTE));
                _mApplicationObject = dte as DTE2;
                return _mApplicationObject;
            }
        }

        #region [ Obsolete ]
        private XDocument _microsoftCommonTargetDocument;
        public XDocument MicrosoftCommonTargetDocument
        {
            get
            {
                _microsoftCommonTargetDocument = _microsoftCommonTargetDocument ?? XDocument.Load(Locations.MicrosoftCommonTargetFileNamePath);
                return _microsoftCommonTargetDocument;
            }

        }

        private XDocument _microsoftCommonTargetX64Document;
        public XDocument MicrosoftCommonTargetX64Document
        {
            get
            {
                _microsoftCommonTargetX64Document = _microsoftCommonTargetX64Document ?? XDocument.Load(Locations.MicrosoftCommonTargetX64FileNamePath);
                return _microsoftCommonTargetX64Document;
            }

        }

        private XDocument _microsoftCommonTarget45Document;
        public XDocument MicrosoftCommonTarget45Document
        {
            get
            {
                _microsoftCommonTarget45Document = _microsoftCommonTarget45Document ?? XDocument.Load(Locations.MicrosoftCommonTarget45FileNamePath);
                return _microsoftCommonTarget45Document;
            }

        }
        #endregion

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            System.Diagnostics.Debugger.Break();
            base.Initialize();

            _dte = (DTE)GetService(typeof(SDTE));
            _dteEvents = _dte.Events.DTEEvents;

            _dteEvents.OnStartupComplete += OnStartupComplete;


            _mPackageDteEvents = ApplicationObject.Events.DTEEvents;
            _mPackageDteEvents.OnBeginShutdown += HandleVisualStudioShutDown;


            var bridge = new LINQBridgeVsExtension(_dte);

            //// Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null == mcs) return;

            //// Create the command for the menu item.
            var enableCommand = new CommandID(GuidList.GuidBridgeVsExtensionCmdSet, (int)PkgCmdIdList.CmdIdEnableBridge);
            var menuItemEnable = new OleMenuCommand((s, e) => bridge.Execute(CommandAction.Enable), enableCommand);
            menuItemEnable.BeforeQueryStatus += (s, e) => bridge.UpdateCommand(menuItemEnable, CommandAction.Enable);


            var disableCommand = new CommandID(GuidList.GuidBridgeVsExtensionCmdSet,
                (int)PkgCmdIdList.CmdIdDisableBridge);
            var menuItemDisable = new OleMenuCommand((s, e) => bridge.Execute(CommandAction.Disable), disableCommand);
            menuItemDisable.BeforeQueryStatus += (s, e) => bridge.UpdateCommand(menuItemDisable, CommandAction.Disable);

            var aboutCommand = new CommandID(GuidList.GuidBridgeVsExtensionCmdSet, (int)PkgCmdIdList.CmdIdAbout);
            var menuItemAbout = new OleMenuCommand((s, e) => { var about = new About(); about.ShowDialog(); }, aboutCommand);


            mcs.AddCommand(menuItemEnable);
            mcs.AddCommand(menuItemDisable);
            mcs.AddCommand(menuItemAbout);
        }

        private void HandleVisualStudioShutDown()
        {
            try
            {
                Log.Write("OnBeginShutdown");

                _mPackageDteEvents.OnBeginShutdown -= HandleVisualStudioShutDown;
                _dteEvents = null;
                _mPackageDteEvents = null;
                //Check if there's only one instance of VS running. if it's so then remove from Microsoft.Common.Target 
                //Any reference of LINQBridgeVs
                if (Process.GetProcessesByName(VisualStudioProcessName).Length > 1) return;

                //This simulate an uninstall. if it's the last instance of VS running disable LINQBridgeVs
                PackageConfigurator.IsLinqBridgeEnabled = false;
            }
            catch (Exception e)
            {
                Log.Write(e, "OnBeginShutdown Error...");

            }
        }

        private void OnStartupComplete()
        {
            try
            {
                Log.Write("OnStartupComplete");

                _dteEvents.OnStartupComplete -= OnStartupComplete;
                _dteEvents = null;

                PackageConfigurator.RemoveBridgeBuildTargetFromMicrosoftCommon(MicrosoftCommonTargetDocument, Locations.MicrosoftCommonTargetFileNamePath);
                Log.Write("BridgeBuild.targets Removed for x86 Operating System");

                if (Environment.Is64BitOperatingSystem)
                {
                    PackageConfigurator.RemoveBridgeBuildTargetFromMicrosoftCommon(MicrosoftCommonTargetX64Document, Locations.MicrosoftCommonTargetX64FileNamePath);
                    Log.Write("BridgeBuild.targets Removed for x64 Operating System");

                }
                if (PackageConfigurator.IsFramework45Installed)
                {
                    PackageConfigurator.RemoveBridgeBuildTargetFromMicrosoftCommon(MicrosoftCommonTarget45Document, Locations.MicrosoftCommonTarget45FileNamePath);
                    Log.Write("BridgeBuild.targets Removed for framework 4.5");
                }

                PackageConfigurator.IsLinqBridgeEnabled = true;
                Log.Write("OnStartupComplete End");

            }
            catch (Exception e)
            {
                Log.Write(e, "OnStartupComplete Error...");
            }
        }

        #endregion
    }
}
