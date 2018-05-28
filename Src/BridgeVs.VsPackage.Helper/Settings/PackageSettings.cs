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
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using BridgeVs.Shared.Common;
using BridgeVs.Shared.Options;
using BridgeVs.Shared.Settings;

namespace BridgeVs.VsPackage.Helper.Settings
{
    /// <inheritdoc />
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    public sealed class PackageSettings : DialogPage
    {
        private const string ErrorMessage = "Please insert a valid path to LINQPad";

        private string _linqPadInstallationPath = Defaults.LINQPadInstallationPath;

        [Category("LINQPad")]
        [DisplayName("Installation Path")]
        [Description("Sets the path to the LINQPad exe")]
        public string LINQPadInstallationPath
        {
            get
            {
                return _linqPadInstallationPath;
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
                    _linqPadInstallationPath = filterInsertedDirectory;
                }
            }
        }


        private bool _isErrorTrackingEnabled = Defaults.ErrorTrackingEnabled;

        [Category("Feedback")]
        [DisplayName("Enable Error Tracking")]
        [Description("When enabled, LINQBridgeVs will send anonymous exception errors to Sentry. " +
                     "This will allow the improvement of LINQBridgeVs and keep it bug free. NO IP address, NO machine name and NO other personal data are in any way sent. " +
                     "If you're still concerned, you can turn off this feature at any time.")]
        public bool ErrorTrackingEnabled
        {
            get
            {
                return _isErrorTrackingEnabled;
            }
            set
            {
                var dte = (DTE)GetService(typeof(SDTE));
                if (dte != null)
                {
                    CommonRegistryConfigurations.SetErrorTracking(dte.Version, value);
                    _isErrorTrackingEnabled = value;
                }
            }
        }

        private SerializationOption _serializationOption = Defaults.SerializationMethod;

        [Category("Serialization")]
        [DisplayName("Serialization Type")]
        [Description("Sets the serialization method used to transmits the variable to LINQPad. Binary or Json.Net")]
        public SerializationOption SerializationMethod
        {
            get
            {
                return _serializationOption;
            }
            set
            {
                var dte = (DTE)GetService(typeof(SDTE));
                if (dte != null)
                {
                    CommonRegistryConfigurations.SetSerializationMethod(dte.Version, value.ToString());
                }
                _serializationOption = value;
            }
        }
    }
}