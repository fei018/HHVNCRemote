using System.Runtime.InteropServices;

namespace VncHelperLib
{
    public class OSPowerHelper
    {
        #region Win32 DllImport
        [DllImport("user32.dll")]
        private static extern void LockWorkStation();

        [DllImport("user32.dll")]
        private static extern bool ExitWindowsEx(int uFlags, int dwReason);

        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
        #endregion

        public static void LockComputer()
        {
            LockWorkStation();
        }

        public static void LogOff()
        {
            ExitWindowsEx(0, 0);
        }

        public static void Shutdown()
        {
            System.Diagnostics.Process.Start("shutdown.exe", "/s /t 0");
        }

        public static void Reboot()
        {
            System.Diagnostics.Process.Start("shutdown.exe","/r /t 0");
            //System.Console.WriteLine("reboot-----");
        }

        /// <summary>
        /// 休眠
        /// </summary>
        public static void Hibernate()
        {
            SetSuspendState(true, true, true);
        }

        public static void Sleep()
        {
            SetSuspendState(false, true, true);
        }
    }
}
