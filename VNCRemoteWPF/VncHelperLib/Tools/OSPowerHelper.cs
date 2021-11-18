using System;
using System.Runtime.InteropServices;

namespace VncHelperLib
{
    public class OSPowerHelper
    {
        #region Power Win32 Api
        [DllImport("user32.dll")]
        private static extern void LockWorkStation();

        [DllImport("user32.dll")]
        private static extern bool ExitWindowsEx(int uFlags, int dwReason);

        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
        #endregion

        #region 阻止鎖屏 win32 api
        [Flags]
        private enum ExecutionState : uint
        {
            /// <summary>
            /// Forces the system to be in the working state by resetting the system idle timer.
            /// </summary>
            SystemRequired = 0x01,

            /// <summary>
            /// Forces the display to be on by resetting the display idle timer.
            /// </summary>
            DisplayRequired = 0x02,

            /// <summary>
            /// This value is not supported. If <see cref="UserPresent"/> is combined with other esFlags values, the call will fail and none of the specified states will be set.
            /// </summary>
            [Obsolete("This value is not supported.")]
            UserPresent = 0x04,

            /// <summary>
            /// Enables away mode. This value must be specified with <see cref="Continuous"/>.
            /// <para />
            /// Away mode should be used only by media-recording and media-distribution applications that must perform critical background processing on desktop computers while the computer appears to be sleeping.
            /// </summary>
            AwaymodeRequired = 0x40,

            /// <summary>
            /// Informs the system that the state being set should remain in effect until the next call that uses <see cref="Continuous"/> and one of the other state flags is cleared.
            /// </summary>
            Continuous = 0x80000000,
        }

        /// <summary>
        /// Enables an application to inform the system that it is in use, thereby preventing the system from entering sleep or turning off the display while the application is running.
        /// </summary>
        [DllImport("kernel32")]
        private static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);
        #endregion


        #region 開關機休眠睡眠
        /// <summary>
        /// Lock Screen
        /// </summary>
        public static void LockComputer()
        {
            LockWorkStation();
        }

        /// <summary>
        /// 登出
        /// </summary>
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
            System.Diagnostics.Process.Start("shutdown.exe", "/r /t 0");
            //System.Console.WriteLine("reboot-----");
        }

        /// <summary>
        /// 休眠
        /// </summary>
        public static void Hibernate()
        {
            SetSuspendState(true, true, true);
        }

        /// <summary>
        /// 睡眠
        /// </summary>
        public static void Sleep()
        {
            SetSuspendState(false, true, true);
        }
        #endregion

        #region 阻止鎖屏功能
        /// <summary>
        /// 设置此线程此时开始一直将处于运行状态，此时计算机不应该进入睡眠状态。
        /// 此线程退出后，设置将失效。
        /// 如果需要恢复，请调用 <see cref="RestoreForCurrentThread"/> 方法。
        /// </summary>
        /// <param name="keepDisplayOn">
        /// 表示是否应该同时保持屏幕不关闭。
        /// 对于游戏、视频和演示相关的任务需要保持屏幕不关闭；而对于后台服务、下载和监控等任务则不需要。
        /// </param>
        public static void PreventForCurrentThread(bool keepDisplayOn = true)
        {
            SetThreadExecutionState(keepDisplayOn
                ? ExecutionState.Continuous | ExecutionState.SystemRequired | ExecutionState.DisplayRequired
                : ExecutionState.Continuous | ExecutionState.SystemRequired);
        }

        /// <summary>
        /// 恢复此线程的运行状态，操作系统现在可以正常进入睡眠状态和关闭屏幕。
        /// </summary>
        public static void RestoreForCurrentThread()
        {
            SetThreadExecutionState(ExecutionState.Continuous);
        }

        /// <summary>
        /// 重置系统睡眠或者关闭屏幕的计时器，这样系统睡眠或者屏幕能够继续持续工作设定的超时时间。
        /// </summary>
        /// <param name="keepDisplayOn">
        /// 表示是否应该同时保持屏幕不关闭。
        /// 对于游戏、视频和演示相关的任务需要保持屏幕不关闭；而对于后台服务、下载和监控等任务则不需要。
        /// </param>
        public static void ResetIdle(bool keepDisplayOn = true)
        {
            SetThreadExecutionState(keepDisplayOn
                ? ExecutionState.SystemRequired | ExecutionState.DisplayRequired
                : ExecutionState.SystemRequired);
        }
        #endregion
    }
}
