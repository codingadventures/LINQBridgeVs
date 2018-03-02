using System;
using System.Runtime.InteropServices;

namespace MidnightDevelopers.VisualStudio.VsRestart
{
    internal static class ElevationChecker
    {
        public enum TokenInformation
        {
            TokenUser = 1,
            TokenGroups = 2,
            TokenPrivileges = 3,
            TokenOwner = 4,
            TokenPrimaryGroup = 5,
            TokenDefaultDacl = 6,
            TokenSource = 7,
            TokenType = 8,
            TokenImpersonationLevel = 9,
            TokenStatistics = 10,
            TokenRestrictedSids = 11,
            TokenSessionId = 12,
            TokenGroupsAndPrivileges = 13,
            TokenSessionReference = 14,
            TokenSandBoxInert = 15,
            TokenAuditPolicy = 16,
            TokenOrigin = 17,
            TokenElevationType = 18,
            TokenLinkedToken = 19,
            TokenElevation = 20,
            TokenHasRestrictions = 21,
            TokenAccessInformation = 22,
            TokenVirtualizationAllowed = 23,
            TokenVirtualizationEnabled = 24,
            TokenIntegrityLevel = 25,
            TokenUiAccess = 26,
            TokenMandatoryPolicy = 27,
            TokenLogonSid = 28,
            MaxTokenInfoClass = 29
        }

        enum TokenElevationType
        {
            TokenElevationTypeDefault = 1,
            TokenElevationTypeFull = 2,
            TokenElevationTypeLimited = 3
        }

        public struct TOKEN_ELEVATION
        {
            public UInt32 TokenIsElevated;
        }

        public const int TOKEN_QUERY = 0x00000008;

        public static bool IsElevated(IntPtr procHandle)
        {
            IntPtr tokenHandle;

            bool isOpenProcToken = NativeMethods.OpenProcessToken(procHandle, TOKEN_QUERY, out tokenHandle);

            try
            {
                if (isOpenProcToken)
                {

                    TOKEN_ELEVATION te;
                    te.TokenIsElevated = 0;

                    IntPtr tokenInformation = Marshal.AllocHGlobal(0);

                    int size = Marshal.SizeOf(Enum.GetUnderlyingType(typeof(TokenInformation)));

                    try
                    {
                        uint returnLength;
                        bool success = NativeMethods.GetTokenInformation(tokenHandle, TokenInformation.TokenElevationType, ref tokenInformation, size, out returnLength);
                        if (success && size == returnLength)
                        {
                            return (TokenElevationType)(int)tokenInformation == TokenElevationType.TokenElevationTypeFull;
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(tokenInformation);
                    }
                }
            }
            finally
            {
                NativeMethods.CloseHandle(tokenHandle);
            }

            return false;
        }

        public static bool CanCheckElevation
        {
            get
            {
                return IsVistaOrLater();
            }
        }

        private static bool IsVistaOrLater()
        {
            return (Environment.OSVersion.Version.Major >= 6);
        }
    }

    internal static class NativeMethods
    {
        [DllImport("Advapi32.dll")]
        public static extern bool OpenProcessToken(IntPtr processHandle, int desiredAccess, out IntPtr tokenHandle);

        [DllImport("Advapi32.dll")]
        public static extern bool GetTokenInformation(IntPtr tokenHandle, ElevationChecker.TokenInformation info, ref IntPtr tokenInformation, int tokenInformationLength, out uint returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);
    }
}
