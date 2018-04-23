using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using BridgeVs.Locations;

namespace BridgeVs.Helper.Settings
{
    /// <inheritdoc />
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    public sealed class PackageSettings : DialogPage
    {
        [Category("LINQPad")]
        [DisplayName("Installation Path")]
        [Description("Sets the path to the LINQPad exe")]
        public string LINQPadInstallationPath
        {
            get => CommonRegistryConfigurations.LINQPadInstallationPath;
            set => CommonRegistryConfigurations.LINQPadInstallationPath = value;
        }

        //[Category("Feedback")]
        //[DisplayName("Enable Error Reporting")]
        //[Description("When enabled, Object Exporter will automatically send exception details to our servers. " +
        //             "This will allow us to improve Object Exporter and keep it bug free.")]
        //public bool ErrorReportingEnabled
        //{
        //    get { return _errorReportingEnabled; }
        //    set { _errorReportingEnabled = value; }
        //}
    }
}
