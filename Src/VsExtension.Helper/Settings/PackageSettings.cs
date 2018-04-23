using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using BridgeVs.Locations;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Forms;
using System.IO;
using System.Linq;

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
            get
            {
                var dte = (DTE)GetService(typeof(SDTE));
                if (dte != null)
                    return CommonRegistryConfigurations.GetLINQPadInstallationPath(dte.Version);

                return string.Empty;
            }
            set
            {
                var dte = (DTE)GetService(typeof(SDTE));
                if (dte != null)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        MessageBox.Show("Please insert a valid path to LINQPad");
                        return;
                    }
                    bool possiblePath = value.IndexOfAny(Path.GetInvalidPathChars()) == -1;

                    if (!possiblePath)
                    {
                        MessageBox.Show("Please insert a valid path to LINQPad");
                        return;
                    }

                    if (!Directory.GetFiles(value, "*.exe", SearchOption.TopDirectoryOnly).Any( p=> p.Contains("LINQPad.exe")))
                    {
                        MessageBox.Show("Please insert a valid path to LINQPad");
                        return;
                    }
                    CommonRegistryConfigurations.SetLINQPadInstallationPath(dte.Version, value);
                }
            }
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
