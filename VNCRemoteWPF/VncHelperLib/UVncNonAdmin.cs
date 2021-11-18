using System;
using System.ServiceProcess;
using System.Threading;

namespace VncHelperLib
{
    internal class UVncNonAdmin : UVncBase, IUVnc
    {
        private string _scExe = "C:\\Windows\\System32\\sc.exe";
        private TimeSpan _waitServiceTime = TimeSpan.FromSeconds(20); 


        public UVncNonAdmin(UVncOption option) : base(option)
        {
        }

        #region InstallStartVncServiceWait
        /// <summary>
        /// 安裝 uvnc_service 服務 <br/>
        /// 權限不夠時: <br/>
        /// 1. 從當前user temp文件夾下讀取文件 <br/>
        /// 2. 檢查 UVncOption.AdminUser 和 UVncOption.AdminPasswd <br/>
        /// 3. 驗證不通過 則 彈出 賬戶密碼 輸入框
        /// </summary>
        /// <param name="waitSecond">等待秒數</param>
        /// <returns></returns>
        public bool InstallVncServiceWait_Authorize(int waitSecond)
        {
            if (VncServiceExist())
            {
                return true;
            }

            if (InvokeValidateAccountOrOpenInputAccountBox())
            {
                var cmd = new CMDHelper();
                if (cmd.CreateProcessExeAsUser(_winvncExe, "-install", UVncOption.AdminUser, UVncOption.AdminPasswd))
                {
                    for (int i = 0; i < waitSecond; i++)
                    {
                        Thread.Sleep(1000);
                        if (VncServiceExist())
                        {
                            using var service = new ServiceController(_vncServiceName);
                            if (service.Status == ServiceControllerStatus.Running || service.Status == ServiceControllerStatus.StartPending)
                            {
                                return true;
                            }

                            string arg = "start " + _vncServiceName;
                            cmd.CreateProcessExeAsUser(_scExe, arg, UVncOption.AdminUser, UVncOption.AdminPasswd);
                            service.WaitForStatus(ServiceControllerStatus.Running, _waitServiceTime);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        #region UninstallVncServiceWait
        /// <summary>
        /// 刪除 uvnc_service 服務 <br/>
        /// 權限不夠時: <br/>
        /// 1. 從當前user temp文件夾下讀取文件 <br/>
        /// 2. 檢查 UVncOption.AdminUser 和 UVncOption.AdminPasswd <br/>
        /// 3. 驗證不通過 則 彈出 賬戶密碼 輸入框
        /// </summary>
        /// <param name="waitSecond">等待秒數</param>
        /// <returns></returns>
        public bool UninstallVncServiceWait_Authorize(int waitSecond)
        {
            if (!VncServiceExist())
            {
                return true;
            }

            if (InvokeValidateAccountOrOpenInputAccountBox())
            {
                var cmd = new CMDHelper();
                if (cmd.CreateProcessExeAsUser(_winvncExe, "-uninstall", UVncOption.AdminUser, UVncOption.AdminPasswd))
                {
                    for (int i = 0; i < waitSecond; i++)
                    {
                        Thread.Sleep(1000);
                        if (!VncServiceExist())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        #region RestartVncService
        /// <summary>
        /// 重新啟動 uvnc_service 服務
        /// </summary>
        /// <returns></returns>
        public bool RestartVncService_Authorize()
        {
            var st = StopVncService_Authorize();
            var sp = StartVncService_Authorize();
            if (st && sp)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region StartVncService
        /// <summary>
        /// 啟動 uvnc_service 服務, 服務不存在 則跳過返回false <br/>
        /// 權限不夠時: <br/>
        /// 1. 從當前user temp文件夾下讀取文件 <br/>
        /// 2. 檢查 UVncOption.AdminUser 和 UVncOption.AdminPasswd <br/>
        /// 3. 驗證不通過 則 彈出 賬戶密碼 輸入框
        /// </summary>
        /// <returns></returns>
        public bool StartVncService_Authorize()
        {
            if (!VncServiceExist())
            {
                return false;
            }

            using var service = new ServiceController(_vncServiceName);
            if (service.Status == ServiceControllerStatus.Running || service.Status == ServiceControllerStatus.StartPending)
            {
                service.WaitForStatus(ServiceControllerStatus.Running, _waitServiceTime);
                return true;
            }

            if (service.Status == ServiceControllerStatus.StopPending)
            {
                service.WaitForStatus(ServiceControllerStatus.Stopped, _waitServiceTime);
            }

            if (InvokeValidateAccountOrOpenInputAccountBox())
            {
                string arg = "start " + _vncServiceName;
                new CMDHelper().CreateProcessExeAsUser(_scExe, arg, UVncOption.AdminUser, UVncOption.AdminPasswd);
                service.WaitForStatus(ServiceControllerStatus.Running, _waitServiceTime);
                return true;
            }

            return false;
        }
        #endregion

        #region StopVncService
        /// <summary>
        /// 停止 uvnc_service 服務, 服務不存在 則跳過 返回 false <br/>
        /// 權限不夠時: <br/>
        /// 1. 從當前user temp文件夾下讀取文件 <br/>
        /// 2. 檢查 UVncOption.AdminUser 和 UVncOption.AdminPasswd <br/>
        /// 3. 驗證不通過 則 彈出 賬戶密碼 輸入框
        /// </summary>
        /// <returns></returns>
        public bool StopVncService_Authorize()
        {
            if (!VncServiceExist())
            {
                return false;
            }

            using var service = new ServiceController(_vncServiceName);
            if (service.Status == ServiceControllerStatus.Stopped || service.Status == ServiceControllerStatus.StopPending)
            {
                service.WaitForStatus(ServiceControllerStatus.Stopped, _waitServiceTime);
                return true;
            }

            if (service.Status == ServiceControllerStatus.StartPending)
            {
                service.WaitForStatus(ServiceControllerStatus.Running, _waitServiceTime);
            }

            if (InvokeValidateAccountOrOpenInputAccountBox())
            {
                string arg = "stop " + _vncServiceName;
                new CMDHelper().CreateProcessExeAsUser(_scExe, arg, UVncOption.AdminUser, UVncOption.AdminPasswd);
                service.WaitForStatus(ServiceControllerStatus.Stopped, _waitServiceTime);
                return true;
            }

            return false;
        }
        #endregion       
        
    }
}
