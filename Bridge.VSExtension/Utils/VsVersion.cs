using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Bridge.VSExtension.Utils
{
    internal static class VSVersion
    {
        static readonly object MLock = new object();
        static Version _mVsVersion;
        static Version _mOsVersion;

        public static Version FullVersion
        {
            get
            {
                lock (MLock)
                {
                    if (_mVsVersion == null)
                    {
                        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "msenv.dll");

                        if (File.Exists(path))
                        {
                            var fvi = FileVersionInfo.GetVersionInfo(path);

                            var verName = fvi.ProductVersion;

                            for (var i = 0; i < verName.Length; i++)
                            {
                                if (!char.IsDigit(verName, i) && verName[i] != '.')
                                {
                                    verName = verName.Substring(0, i);
                                    break;
                                }
                            }
                            _mVsVersion = new Version(verName);
                        }
                        else
                            _mVsVersion = new Version(0, 0); // Not running inside Visual Studio!
                    }
                }

                return _mVsVersion;
            }
        }

        public static Version OSVersion
        {
            get { return _mOsVersion ?? (_mOsVersion = Environment.OSVersion.Version); }
        }

        public static bool VS2012OrLater
        {
            get { return FullVersion >= new Version(11, 0); }
        }

        public static bool VS2010OrLater
        {
            get { return FullVersion >= new Version(10, 0); }
        }

        public static bool VS2008OrOlder
        {
            get { return FullVersion < new Version(9, 0); }
        }

        public static bool VS2005
        {
            get { return FullVersion.Major == 8; }
        }

        public static bool VS2008
        {
            get { return FullVersion.Major == 9; }
        }

        public static bool VS2010
        {
            get { return FullVersion.Major == 10; }
        }

        public static bool VS2012
        {
            get { return FullVersion.Major == 11; }
        }
    }
}
