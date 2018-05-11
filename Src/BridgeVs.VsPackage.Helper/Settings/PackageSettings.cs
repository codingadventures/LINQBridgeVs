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

namespace BridgeVs.VsPackage.Helper.Settings
{
    /// <inheritdoc />
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    public sealed class PackageSettings : DialogPage
    {

        private const string ErrorMessage = "Please insert a valid path to LINQPad";
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
                    bool isPathInValid = string.IsNullOrEmpty(value)
                        || value.IndexOfAny(Path.GetInvalidPathChars()) != -1;

                    if (isPathInValid)
                    {
                        MessageBox.Show(ErrorMessage);
                        return;
                    }

                    //check the inserted path is a valid directory that contains linqpad.exe
                    //if the inserted value is a file then check for its directory otherwise a directory
                    //has been inserted
                    var insertedDirectory = File.Exists(value) ? Path.GetDirectoryName(value) : value;

                    var isInvalidDirectory =
                        !Directory.Exists(insertedDirectory)
                        || !Directory.GetFiles(insertedDirectory, "*.exe", SearchOption.TopDirectoryOnly)
                             .Any(p => p.Contains("LINQPad.exe"));

                    if (isInvalidDirectory)
                    {
                        MessageBox.Show(ErrorMessage);
                        return;
                    }

                    string filterInsertedDirectory = Path.GetFullPath(insertedDirectory);
                    CommonRegistryConfigurations.SetLINQPadInstallationPath(dte.Version, filterInsertedDirectory);
                }
            }
        }

        [Category("Feedback")]
        [DisplayName("Enable Error Tracking")]
        [Description("When enabled, LINQBridgeVs will automatically send exception details to a Sentry server. " +
                     "This will allow the improvement of LINQBridgeVs and keep it bug free."+
                     "for concern about privacy you can turn off this feature. Please refer to: [link] ")]
        public bool ErrorTrackingEnabled
        {
            get
            {
                var dte = (DTE)GetService(typeof(SDTE));
                if (dte != null)
                    return CommonRegistryConfigurations.IsErrorTrackingEnabled(dte.Version);

                return false;
            }
            set
            {
                var dte = (DTE)GetService(typeof(SDTE));
                if (dte != null)
                {
                    CommonRegistryConfigurations.SetErrorTracking(dte.Version, value);
                }
            }
        }
    }
}
