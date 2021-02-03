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
