using System;
using System.ServiceProcess;
using System.Threading;

namespace VncHelperLib
{
    internal class UVncAsAdmin : UVncBase, IUVnc
    {
        private TimeSpan _waitServiceTime = TimeSpan.FromSeconds(10);


        public UVncAsAdmin(UVncOption option) : base(option)
        {
        }

        #region Start Service
        /// <summary>
        /// 啟動 uvnc_service 服務, 服務不存在 則跳過返回false
        /// </summary>
        /// <returns></returns>
        public bool StartVncService_Authorize()
        {
            if (!VncServiceExist())
            {
                return false;
            }

            try
            {
                using var service = new ServiceController(_vncServiceName);
                if (service.Status == ServiceControllerStatus.Running || service.Status == ServiceControllerStatus.StartPending)
                {
                    return true;
                }

                if (service.Status == ServiceControllerStatus.StopPending)
                {
                    service.WaitForStatus(ServiceControllerStatus.Stopped, _waitServiceTime);
                }

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, _waitServiceTime);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Stop Service
        /// <summary>
        /// 停止 uvnc_service 服務, 服務不存在 則跳過 返回 false
        /// </summary>
        /// <returns></returns>
        public bool StopVncService_Authorize()
        {
            if (!VncServiceExist())
            {
                return false;
            }

            try
            {
                using var service = new ServiceController(_vncServiceName);

                if (service.Status == ServiceControllerStatus.Running || service.Status == ServiceControllerStatus.StartPending)
                {
                    service.WaitForStatus(ServiceControllerStatus.Running, _waitServiceTime);
                }

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, _waitServiceTime);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region RestartService 
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

        #region InstallStartVncServiceWait
        /// <summary>
        /// 安裝 uvnc_service 服務
        /// </summary>
        /// <param name="waitSecond">等待秒數</param>
        /// <returns></returns>
        public bool InstallVncServiceWait_Authorize(int waitSecond)
        {
            if (VncServiceExist())
            {
                return true;
            }

            var cmd = new CMDHelper();
            if (cmd.CreateProcessExe(_winvncExe, "-installhelper"))
                for (int i = 0; i < waitSecond; i++)
                {
                    Thread.Sleep(1000);
                    if (VncServiceExist())
                    {
                        StartVncService_Authorize();
                        return true;
                    }
                }
            return false;
        }
        #endregion

        #region UninstallVncServiceWait
        /// <summary>
        /// 刪除 uvnc_service 服務 
        /// </summary>
        /// <param name="waitSecond">等待秒數</param>
        /// <returns></returns>
        public bool UninstallVncServiceWait_Authorize(int waitSecond)
        {
            if (VncServiceExist())
            {
                StopVncService_Authorize();
                var cmd = new CMDHelper();
                if (cmd.CreateProcessExe(_winvncExe, "-uninstallhelper"))
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
                return false;
            }

            return true;
        }
        #endregion
    }
}
