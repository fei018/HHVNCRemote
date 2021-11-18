namespace VncHelperLib
{
    public interface IUVnc
    {
        public event UVncEventHandler ValidateAccountOrOpenInputAccountBox;

        /// <summary>
        /// 查詢 uvnc_service 服務是否存在
        /// </summary>
        /// <returns></returns>
        public bool VncServiceExist();

        /// <summary>
        /// 啟動 uvnc_service 服務, 服務不存在 則跳過返回false <br/>
        /// 權限不夠時: <br/>
        /// 1. 從當前user temp文件夾下讀取文件 <br/>
        /// 2. 檢查 UVncOption.AdminUser 和 UVncOption.AdminPasswd <br/>
        /// 3. 驗證不通過 則 彈出 賬戶密碼 輸入框
        /// </summary>
        /// <returns></returns>
        public bool StartVncService_Authorize();

        /// <summary>
        /// 停止 uvnc_service 服務, 服務不存在 則跳過 返回 false <br/>
        /// 權限不夠時: <br/>
        /// 1. 從當前user temp文件夾下讀取文件 <br/>
        /// 2. 檢查 UVncOption.AdminUser 和 UVncOption.AdminPasswd <br/>
        /// 3. 驗證不通過 則 彈出 賬戶密碼 輸入框
        /// </summary>
        /// <returns></returns>
        public bool StopVncService_Authorize();

        /// <summary>
        /// 重新啟動 uvnc_service 服務
        /// </summary>
        /// <returns></returns>
        public bool RestartVncService_Authorize();

        /// <summary>
        /// 安裝 uvnc_service 服務 <br/>
        /// 權限不夠時: <br/>
        /// 1. 從當前user temp文件夾下讀取文件 <br/>
        /// 2. 檢查 UVncOption.AdminUser 和 UVncOption.AdminPasswd <br/>
        /// 3. 驗證不通過 則 彈出 賬戶密碼 輸入框
        /// </summary>
        /// <param name="waitSecond">等待秒數</param>
        /// <returns></returns>
        public bool InstallVncServiceWait_Authorize(int waitSecond);

        /// <summary>
        /// 刪除 uvnc_service 服務 <br/>
        /// 權限不夠時: <br/>
        /// 1. 從當前user temp文件夾下讀取文件 <br/>
        /// 2. 檢查 UVncOption.AdminUser 和 UVncOption.AdminPasswd <br/>
        /// 3. 驗證不通過 則 彈出 賬戶密碼 輸入框
        /// </summary>
        /// <param name="waitSecond">等待秒數</param>
        /// <returns></returns>
        public bool UninstallVncServiceWait_Authorize(int waitSecond);

        /// <summary>
        /// 編輯 Ultravnc.ini 文件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="value"></param>
        public void EditUltravncini(string item, string value);

        /// <summary>
        /// 將加密後 vnc password 寫入 ultravnc.ini
        /// </summary>
        /// <param name="password">加密後的密碼</param>
        public void SetPasswordToUltraVNCini(string password);

        /// <summary>
        /// 查詢 進程裡 winvnc.exe 是否運作中
        /// </summary>
        /// <returns></returns>
        public bool WinVncProcessExist();

        /// <summary>
        /// 新建 winvnc.exe process, 如果存在則跳過
        /// </summary>
        /// <param name="waitSecond"></param>
        public void CreateWinVncProcessWait(int waitSecond);

        /// <summary>
        /// 結束 winvnc.exe 進程
        /// </summary>
        /// <param name="waitSecond"></param>
        public void KillWinVncProcessWait(int waitSecond);

        /// <summary>
        /// uvnc_service 服務是否 運作中
        /// </summary>
        /// <returns></returns>
        public bool IsVncService_RunningOrStartpending();

        /// <summary>
        /// uvnc_service 服務是否 停止
        /// </summary>
        /// <returns></returns>
        public bool IsVncService_StoppedOrStopPending();

        /// <summary>
        /// 讀取 ultravnc.ini 的 passwd, 並且解密
        /// </summary>
        /// <returns></returns>
        public string TryGetVncPasswordFromUltravncIni();
    }
}
