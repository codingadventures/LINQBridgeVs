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
