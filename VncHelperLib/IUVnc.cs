namespace VncHelperLib
{
    public interface IUVnc
    {
        public event UVncEventHandler ValidateAccountOrOpenInputAccountBox;

        public bool VncServiceExist();
        public bool StartVncService_Authorize();
        public bool StopVncService_Authorize();
        public bool RestartVncService_Authorize();
        public bool InstallVncServiceWait_Authorize(int waitSecond);
        public bool UninstallVncServiceWait_Authorize(int waitSecond);
        public void EditUltravncini(string item, string value);
        public void SetPasswordToUltraVNCini(string password);
        public bool WinVncProcessExist();
        public void CreateWinVncProcessWait(int waitSecond);
        public void KillWinVncProcessWait(int waitSecond);
        public bool IsVncService_RunningOrStartpending();
        public bool IsVncService_StoppedOrStopPending();

    }
}
