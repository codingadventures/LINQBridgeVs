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
using System.Collections;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using EnvDTE;
using EnvDTE80;
using LINQBridge.Logging;
using LINQBridgeVs.Extension.Forms;
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
    //  , IVsShellPropertyEvents
    // , IVsSolutionEvents
    // , IDisposable
    {

        private DTEEvents _dteEvents;
        private DTE _dte;
        private const string VisualStudioProcessName = "devenv";
        private DTE2 _mApplicationObject = null;
        DTEEvents _mPackageDteEvents = null;

        //  private static Icon solutionIcon;
        //  private uint propChangeCookie;
        //  private IVsSolution solution;
        //  private uint solutionEventsCookie;

        //  public event Action AfterSolutionLoaded;
        //  public event Action BeforeSolutionClosed;

        private static readonly XDocument MicrosoftCommonTargetDocument
            = XDocument.Load(Locations.MicrosoftCommonTargetFileNamePath);

        public DTE2 ApplicationObject
        {
            get
            {
                if (_mApplicationObject == null)
                {
                    // Get an instance of the currently running Visual Studio IDE
                    DTE dte = (DTE)GetService(typeof(DTE));
                    _mApplicationObject = dte as DTE2;
                }
                return _mApplicationObject;
            }
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            _dte = (DTE)GetService(typeof(SDTE));
            _dteEvents = _dte.Events.DTEEvents;

            _dteEvents.OnStartupComplete += OnStartupComplete;
           

            _mPackageDteEvents =  ApplicationObject.Events.DTEEvents;
            _mPackageDteEvents.OnBeginShutdown += HandleVisualStudioShutDown;
            // watch for VSSPROPID property changes
            //  var vsShell = (IVsShell)GetService(typeof(SVsShell));
            //  vsShell.AdviseShellPropertyChanges(this, out propChangeCookie);
            //solution = GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            //if (solution != null)
            //{
            //    solution.AdviseSolutionEvents(this, out solutionEventsCookie);
            //}

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
                //Any reference of LINQBridge
                if (Process.GetProcessesByName(VisualStudioProcessName).Length > 1) return;

                Log.Write("Disabling LINQBridgeVS. Only one VS instance opened");
                DisableLinqBridge();

                LINQBridgeVsExtension.IsEnvironmentConfigured = false;
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
                 
                EnableLinqBridge();

                Log.Write("OnStartupComplete End");

            }
            catch (Exception e)
            {
                Log.Write(e,"OnStartupComplete Error...");
            }
        }

        private static void DisableLinqBridge()
        {
            var linqBridgeTargetImportNode = GetTargetImportNode();

            if (linqBridgeTargetImportNode == null) return;

            linqBridgeTargetImportNode.Remove();

            MicrosoftCommonTargetDocument.Save(Locations.MicrosoftCommonTargetFileNamePath);
            MicrosoftCommonTargetDocument.Save(Locations.MicrosoftCommonTarget64FileNamePath);


        }


        private static void EnableLinqBridge()
        {

            var import = XName.Get("Import", "http://schemas.microsoft.com/developer/msbuild/2003");

            if (MicrosoftCommonTargetDocument.Root == null || GetTargetImportNode() != null) return;

// ReSharper disable once AssignNullToNotNullAttribute
            var linqBridgeTarget = new XElement(import, new XAttribute("Project", Path.GetFileName(Resources.Targets)));

            MicrosoftCommonTargetDocument.Root.Add(linqBridgeTarget);


            MicrosoftCommonTargetDocument.Save(Locations.MicrosoftCommonTargetFileNamePath);
            MicrosoftCommonTargetDocument.Save(Locations.MicrosoftCommonTarget64FileNamePath);
        }


        private static XElement GetTargetImportNode()
        {
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("aw", "http://schemas.microsoft.com/developer/msbuild/2003");

            var importProjectNode =
                (IEnumerable)
                    MicrosoftCommonTargetDocument.XPathEvaluate("/aw:Project/aw:Import[@Project='BridgeBuildTask.targets']",
                        namespaceManager);


            var linqBridgeTargetImportNode = importProjectNode.Cast<XElement>().FirstOrDefault();

            return linqBridgeTargetImportNode;
        }

       
        #endregion

        //public int OnShellPropertyChange(int propid, object var)
        //{
        //    if (propid != (int)__VSSPROPID4.VSSPROPID_ShellInitialized || Convert.ToBoolean(var) != true)
        //        return VSConstants.S_OK;

        //    //var solution = GetService(typeof(SVsSolution)) as IVsSolution;

        //    IVsHierarchy solHierarchy = null;

        //    if (solution != null)
        //        solution.GetProjectOfUniqueName("CustomType1", out solHierarchy);
        //    //    // set VSHPROPID_IconHandle for Solution Node
        //    //  //  var solHierarchy = (IVsHierarchy)GetService(typeof(SVsSolution));

        //    if (solHierarchy == null) return VSConstants.S_OK;

        //    var hr = solHierarchy.SetProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_IconHandle, solutionIcon.Handle);

        //    //    // stop listening for shell property changes
        //    //  var vsShell = (IVsShell)GetService(typeof(SVsShell));
        //    // hr = vsShell.UnadviseShellPropertyChanges(propChangeCookie);
        //    //   propChangeCookie = 0;
        //    return VSConstants.S_OK;
        //}

        //#region IVsSolutionEvents Members

        //int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        //{
        //    return VSConstants.S_OK;
        //}

        //int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        //{


        //    return VSConstants.S_OK;
        //}

        //int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        //{
        //    object icon;

        //    IVsHierarchy solHierarchy = null;

        //    if (solution != null)
        //        solution.GetProjectOfUniqueName(@"C:\Users\John\Documents\Visual Studio 2012\Projects\CompileTest1\CompileTest1\CompileTest1.Ciao.csproj", out solHierarchy);


        //    var hr = pHierarchy.SetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_IconHandle, solutionIcon.Handle);
        //    // stop listening for shell property changes
        //    //var vsShell = (IVsShell)GetService(typeof(SVsShell));
        //    //hr = vsShell.UnadviseShellPropertyChanges(propChangeCookie);
        //    //   propChangeCookie = 0;

        //    return VSConstants.S_OK;
        //}

        //int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        //{
        //    //  AfterSolutionLoaded();
        //    return VSConstants.S_OK;
        //}

        //int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        //{
        //    return VSConstants.S_OK;
        //}

        //int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        //{
        //    //  BeforeSolutionClosed();
        //    return VSConstants.S_OK;
        //}

        //int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        //{
        //    return VSConstants.S_OK;
        //}

        //int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        //{
        //    return VSConstants.S_OK;
        //}

        //int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        //{
        //    return VSConstants.S_OK;
        //}

        //int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        //{
        //    return VSConstants.S_OK;
        //}

        //#endregion


    }
}
