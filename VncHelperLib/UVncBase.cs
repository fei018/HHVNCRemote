using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace VncHelperLib
{
    internal class UVncBase
    {
        protected string _vncServiceName;
        protected string _winvncProcessName;
        protected string _winvncExe;
        protected string _setpasswordExe;
        protected string _ultravncIni;

        public event UVncEventHandler ValidateAccountOrOpenInputAccountBox;

        public UVncBase(UVncOption option)
        {
            _vncServiceName = option.VncServiceName;
            _winvncProcessName = option.WinVncProcessName;
            _winvncExe = option.WinvncExe;
            _setpasswordExe = option.SetpasswordExe;
            _ultravncIni = option.UltravncIni;
        }

        #region VncServiceExist
        public bool VncServiceExist()
        {
            var serives = ServiceController.GetServices();
            var has = serives.Any(s => s.ServiceName == _vncServiceName);
            if (has)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region WinVncProcessExist
        public bool WinVncProcessExist()
        {
            var ps = Process.GetProcessesByName(_winvncProcessName);
            if (ps.Length > 0)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region SetPasswordToUltraVNCini
        public void SetPasswordToUltraVNCini(string password)
        {
            var cmd = new CMDHelper();
            cmd.CreateProcessExe(_setpasswordExe, password + " " + password);
        }
        #endregion

        #region CreateWinVncProcess
        public void CreateWinVncProcessWait(int waitSecond)
        {
            if (!WinVncProcessExist())
            {
                var cmd = new CMDHelper();
                cmd.CreateProcessExe(_winvncExe, "-run");

                for (int i = 0; i < waitSecond; i++)
                {
                    Thread.Sleep(1000);
                    if (WinVncProcessExist())
                    {
                        return;
                    }
                }
            }
        }
        #endregion

        #region KillWinVncProcessWait
        public void KillWinVncProcessWait(int waitSecond)
        {
            if (WinVncProcessExist())
            {
                var cmd = new CMDHelper();
                cmd.CreateProcessExe(_winvncExe, "-kill");

                for (int i = 0; i < waitSecond; i++)
                {
                    Thread.Sleep(1000);
                    if (!WinVncProcessExist())
                    {
                        return;
                    }
                }
            }
        }
        #endregion

        #region EditUltravncini
        public void EditUltravncini(string item, string value)
        {
            // ConnectPriority;

            string newline = item + "=" + value;
            try
            {
                if (File.Exists(_ultravncIni))
                {
                    var allLines = File.ReadAllLines(_ultravncIni);
                    for (int i = 0; i < allLines.Length; i++)
                    {
                        if (allLines[i].StartsWith(item))
                        {
                            if (allLines[i] == newline)
                            {
                                return;
                            }

                            allLines[i] = newline;
                            break;
                        }
                    }
                    File.WriteAllLines(_ultravncIni, allLines);
                }
                return;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region IsVncServiceStatus_RunningOrStartpending
        public bool IsVncService_RunningOrStartpending()
        {
            if (VncServiceExist())
            {
                using var service = new ServiceController(_vncServiceName);
                if (service.Status == ServiceControllerStatus.Running || service.Status == ServiceControllerStatus.StartPending)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region IsVncServiceStatus_StoppedOrStopPending
        public bool IsVncService_StoppedOrStopPending()
        {
            if (VncServiceExist())
            {
                using var service = new ServiceController(_vncServiceName);
                if (service.Status == ServiceControllerStatus.Stopped || service.Status == ServiceControllerStatus.StopPending)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region InvokeAccountDialogBoxAndValidate
        protected bool InvokeValidateAccountOrOpenInputAccountBox()
        {
            if (UVncOption.ValidateInputAccount())
            {
                return true;
            }

            if (ValidateAccountOrOpenInputAccountBox != null)
            {
                return ValidateAccountOrOpenInputAccountBox.Invoke(null, null);
            }
            else
            {
                throw new Exception("用戶輸入框事件未訂閱.");
            }
        }
        #endregion
    }
}
