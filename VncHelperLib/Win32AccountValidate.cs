using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace VncHelperLib
{
    ////reference https://docs.microsoft.com/en-us/dotnet/api/system.security.principal.windowsimpersonationcontext?view=netframework-4.8

    public class Win32AccountValidate
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
                                            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private extern static bool CloseHandle(IntPtr handle);

        public static bool Validate(string domainName, string userName, string passwd)
        {
            SafeTokenHandle safeTokenHandle = null;

            const int LOGON32_PROVIDER_DEFAULT = 0;
            //This parameter causes LogonUser to create a primary token.
            const int LOGON32_LOGON_INTERACTIVE = 2;

            // Call LogonUser to obtain a handle to an access token.
            try
            {
                bool returnValue = LogonUser(userName, domainName, passwd,
                                         LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                                         out safeTokenHandle);
                return returnValue;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                safeTokenHandle?.Dispose();
            }
        }
    }

    #region SafeTokenHandle Class
    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle()
            : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }
    }
    #endregion
}
