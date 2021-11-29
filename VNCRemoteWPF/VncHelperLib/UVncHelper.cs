using System;
using System.Linq;
using System.Threading.Tasks;

namespace VncHelperLib
{
    public class UVncHelper : NotifyPropertyObject
    {
        private readonly IUVnc _uvnc;
        private readonly int _waitTimeSecond = 10;

        private bool _isLogOff;
        private bool _isReboot;

        public event UVncEventHandler ValidateAccountOrOpenInputAccountBoxEvent;


        #region 構造函數
        /// <summary>
        /// check ultravnc path, throw excetion
        /// </summary>
        public UVncHelper(IUVnc uvnc)
        {
            _uvnc = uvnc;
        }
        #endregion

        #region 訂閱事件， 必須初始化之後
        public void SubscribeUVncHelperEvent(Action subscribeEvent)
        {
            if (subscribeEvent != null)
            {
                subscribeEvent.Invoke();

                _uvnc.ValidateAccountOrOpenInputAccountBox += ValidateAccountOrOpenInputAccountBoxEvent;
            }
        }
        #endregion

        #region UVncStatusNotity
        private string _uvncStatus;
        public string UVncStatusNotity
        {
            get => _uvncStatus;
            set
            {
                _uvncStatus = value;
                RaisePropertyChanged("UVncStatusNotity");
            }
        }

        public void SetUVncStatusNotity(string text)
        {
            UVncStatusNotity = text;
        }
        #endregion

        #region UVncPasswordNotity
        private string _uvncPassword;

        public string UVncPasswordNotity
        {
            get => _uvncPassword;
            set
            {
                _uvncPassword = value;
                RaisePropertyChanged("UVncPasswordNotity");
            }
        }
        #endregion

        #region Show ProgressBar
        public event UVncEventHandler ShowProgressBarEvent;

        public bool IsShowProgressBar { get; set; }
        public void InvokeShowProgressBar(bool show)
        {
            if (ShowProgressBarEvent != null)
            {
                IsShowProgressBar = show;
                ShowProgressBarEvent.Invoke(this, null);
            }
        }
        #endregion

        #region vnc 隨機密碼
        private static Random _random = new Random();
        private static string GetVncRandomPassword()
        {
            //const string chars = "ABCDEFGHIJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz0123456789";
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, 6).Select(s => s[_random.Next(s.Length)]).ToArray());
        }
        #endregion      

        #region Edit ConnectPriority=1 in Ultravnc.ini
        private void EditUltraIni()
        {
            _uvnc.EditUltravncini("ConnectPriority", "1");
        }
        #endregion

        #region Try Launch RemoteSupport
        private void TryLaunchRemoteSupport()
        {
            var isAdmin = UVncOption.IsCurrentUserInAdministrators();
            var serviceExist = _uvnc.VncServiceExist();

            var vncPass = GetVncRandomPassword();

            if (isAdmin)
            {
                _uvnc.SetPasswordToUltraVNCini(vncPass);
                UVncPasswordNotity = vncPass;

                if (serviceExist)
                {
                    if (_uvnc.RestartVncService_Authorize())
                    {
                        SetUVncStatusNotity("管理員, uvnc服務運行正常.");
                        return;
                    }

                    SetUVncStatusNotity("管理員, uvnc服務重啟無效.");
                    return;
                }
                else
                {
                    _uvnc.KillWinVncProcessWait(10);
                    _uvnc.CreateWinVncProcessWait(10);
                    SetUVncStatusNotity("管理員, 程式運行正常.");
                    return;
                }
            }

            if (!isAdmin)
            {
                if (serviceExist)
                {
                    UVncPasswordNotity = _uvnc.TryGetVncPasswordFromUltravncIni();
                    SetUVncStatusNotity("非管理員, uvnc服務已存在, 密碼可能無效.");
                    return;
                }
                else
                {
                    _uvnc.SetPasswordToUltraVNCini(vncPass);
                    UVncPasswordNotity = vncPass;
                    _uvnc.KillWinVncProcessWait(_waitTimeSecond);
                    _uvnc.CreateWinVncProcessWait(_waitTimeSecond);
                    SetUVncStatusNotity("非管理員, 程式運行正常.");
                }
            }
        }
        #endregion

        #region Try LaunchRemoteSupport AfterReboot
        private void TryLaunchRemoteSupport_AfterReboot()
        {
            UVncPasswordNotity = UVncOption.UVncPassword;

            if (_uvnc.IsVncService_RunningOrStartpending())
            {
                SetUVncStatusNotity("uvnc服務運行正常.");
                return;
            }

            var isAdmin = UVncOption.IsCurrentUserInAdministrators();

            if (isAdmin)
            {
                if (_uvnc.RestartVncService_Authorize())
                {
                    SetUVncStatusNotity("uvnc服務運行正常");
                    return;
                }

                SetUVncStatusNotity("uvnc服務重啟無效.");
                return;
            }
            else
            {
                if (UVncOption.ValidateInputAccount())
                {
                    if (_uvnc.RestartVncService_Authorize())
                    {
                        SetUVncStatusNotity("uvnc服務運行正常");
                        return;
                    }

                    SetUVncStatusNotity("uvnc服務重啟無效.");
                    return;
                }

                SetUVncStatusNotity("uvnc服務不能正常運行.");
                return;
            }
        }
        #endregion

        #region To Launch RemoteSupport
        public void ToLaunchRemoteSupport()
        {
            InvokeShowProgressBar(true);
            try
            {
                SetUVncStatusNotity("檢測中...");

                UVncOption.DeleteStartupBatchFile();
                EditUltraIni();

                if (UVncOption.IsAfterReboot)
                {
                    TryLaunchRemoteSupport_AfterReboot();
                }
                else
                {
                    TryLaunchRemoteSupport();
                }
            }
            catch (Exception ex)
            {
                SetUVncStatusNotity(ex.Message);
            }
            finally
            {
                InvokeShowProgressBar(false);
            }
        }
        #endregion

        #region To Close RemoteSupport
        /// <summary>
        /// Throw Excetion
        /// </summary>
        public void ToCloseRemoteSupport()
        {
            try
            {
                if (_isLogOff || _isReboot)
                {
                    return;
                }

                SetUVncStatusNotity("關閉中...");

                if (_uvnc.VncServiceExist())
                {
                    SetUVncStatusNotity("刪除服務中...");
                    _uvnc.UninstallVncServiceWait_Authorize(_waitTimeSecond);
                    UVncOption.DeleteTempAdminTxt();
                }

                _uvnc.KillWinVncProcessWait(_waitTimeSecond);

                SetUVncStatusNotity("刪除臨時文件.");
                UVncOption.DeleteTempAdminTxt();
                return;
            }
            catch (Exception ex)
            {
                SetUVncStatusNotity(ex.Message);
            }
        }
        #endregion

        #region To Reboot System
        public void ToRebootSystem()
        {
            _isReboot = false;
            try
            {
                SetUVncStatusNotity("安裝服務中...");
                if (_uvnc.InstallVncServiceWait_Authorize(_waitTimeSecond))
                {
                    SetUVncStatusNotity("寫入啟動文檔...");
                    UVncOption.CreateStartupBatchFile(UVncPasswordNotity);
                    _isReboot = true;
                    OSPowerHelper.Reboot();
                    return;
                }
                else
                {
                    SetUVncStatusNotity("uvnc服務安裝失敗.");
                }
            }
            catch (Exception ex)
            {
                SetUVncStatusNotity(ex.Message);
            }
        }
        #endregion

        #region To LogOff
        public void ToLogOff()
        {
            _isLogOff = false;
            try
            {
                SetUVncStatusNotity("安裝服務中...");
                if (_uvnc.InstallVncServiceWait_Authorize(_waitTimeSecond))
                {
                    SetUVncStatusNotity("寫入啟動文檔...");
                    UVncOption.CreateStartupBatchFile(UVncPasswordNotity);
                    _isReboot = true;
                    OSPowerHelper.LogOff();
                    return;
                }
                else
                {
                    SetUVncStatusNotity("uvnc服務安裝失敗.");
                }
            }
            catch (Exception ex)
            {
                SetUVncStatusNotity(ex.Message);
            }
        }
        #endregion

        #region To LockComputer
        public void ToLockComputer()
        {
            try
            {
                SetUVncStatusNotity("安裝服務中...");
                if (_uvnc.InstallVncServiceWait_Authorize(_waitTimeSecond))
                {
                    _uvnc.StopVncService_Authorize();

                    _uvnc.KillWinVncProcessWait(_waitTimeSecond);

                    _uvnc.StartVncService_Authorize();

                    OSPowerHelper.LockComputer();
                    SetUVncStatusNotity("uvnc服務正常.");
                    return;
                }
                else
                {
                    SetUVncStatusNotity("uvnc服務安裝失敗.");
                }
            }
            catch (Exception ex)
            {
                SetUVncStatusNotity(ex.Message);
            }
        }
        #endregion
    }
}
