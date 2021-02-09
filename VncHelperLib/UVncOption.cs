using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;

namespace VncHelperLib
{
    public class UVncOption
    {
        public static string AdminUser { get; set; }
        public static string AdminPasswd { get; set; }
        public static string UVncPassword { get; set; }
        public static bool IsAfterReboot { get; set; }

        public static readonly string HostName = Dns.GetHostName();

        public static readonly string HostIP = Dns.GetHostEntry(HostName).AddressList.FirstOrDefault(p => p.AddressFamily.ToString() == "InterNetwork")?.ToString();

        #region static InitialVncPath
        private static readonly string _vncServiceName = "uvnc_service";
        private static readonly string _vncProcessName = "winvnc";

        private static readonly string _winvnc = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramW6432%"), @"uvnc bvba\UltraVNC\winvnc.exe");
        private static readonly string _setpasswd = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramW6432%"), @"uvnc bvba\UltraVNC\setpasswd.exe");
        private static readonly string _ultravncIni = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramW6432%"), @"uvnc bvba\UltraVNC\ultravnc.ini");

        private static readonly string _winvncX86 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"uvnc bvba\UltraVNC\winvnc.exe");
        private static readonly string _setpasswdX86 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"uvnc bvba\UltraVNC\setpasswd.exe");
        private static readonly string _ultravncIniX86 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"uvnc bvba\UltraVNC\ultravnc.ini");

        public string VncServiceName { get; set; }

        public string WinVncProcessName { get; set; }

        public string WinvncExe { get; set; }

        public string SetpasswordExe { get; set; }

        public string UltravncIni { get; set; }

        private static UVncOption InitialVncPath()
        {
            var option = new UVncOption();
            if (File.Exists(_winvnc) && File.Exists(_setpasswd) && File.Exists(_ultravncIni))
            {
                option.WinvncExe = _winvnc;
                option.SetpasswordExe = _setpasswd;
                option.UltravncIni = _ultravncIni;
                option.VncServiceName = _vncServiceName;
                option.WinVncProcessName = _vncProcessName;
                return option;
            }

            if (File.Exists(_winvncX86) && File.Exists(_setpasswdX86) && File.Exists(_ultravncIniX86))
            {
                option.WinvncExe = _winvncX86;
                option.SetpasswordExe = _setpasswdX86;
                option.UltravncIni = _ultravncIniX86;
                option.VncServiceName = _vncServiceName;
                option.WinVncProcessName = _vncProcessName;
                return option;
            }

            return InitialVncPathUseEmbed();
        }

        /// <summary>
        /// 嵌入資源 "winvnc.exe","ultravnc.ini" 寫入 temp 文件夾
        /// </summary>
        /// <returns></returns>
        private static UVncOption InitialVncPathUseEmbed()
        {
            var tempWinvnc = Path.Combine(Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.User), "winvnc.exe");
            var tempIni = Path.Combine(Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.User), "ultravnc.ini");

            var option = new UVncOption
            {
                WinvncExe = tempWinvnc,
                UltravncIni = tempIni,
                VncServiceName = _vncServiceName,
                WinVncProcessName = _vncProcessName
            };

            if (File.Exists(tempWinvnc))
            {
                return option;
            }

            File.WriteAllBytes(tempWinvnc, VncHelperLib.Properties.Resources.winvnc);
            File.WriteAllText(tempIni, VncHelperLib.Properties.Resources.UltraVNC);          

            return option;
        }
        #endregion

        #region static GetUVncInstance
        /// <summary>
        /// 獲取 初始化後的 UVncOption 實例接口 <br/>
        /// 預設路徑未安裝ultravnc, 則 釋放嵌入資源 "winvnc.exe","ultravnc.ini" 寫入 temp 文件夾
        /// </summary>
        /// <returns></returns>
        public static IUVnc GetUVncInstance()
        {
            try
            {
                var option = InitialVncPath();

                if (IsCurrentUserInAdministrators())
                {
                    return new UVncAsAdmin(option);
                }
                else
                {
                    return new UVncNonAdmin(option);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region static IsCurrentUserInAdministrators
        /// <summary>
        /// 判斷當前 Windows account 是否具有 administrator 權限
        /// </summary>
        /// <returns></returns>
        public static bool IsCurrentUserInAdministrators()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            return isElevated;
        }
        #endregion

        #region static ValidateInputAccount
        /// <summary>
        /// 驗證輸入的 Windows 賬戶密碼 是否正確
        /// </summary>
        /// <returns>正確賦值給 UVncOption.AdminUser, UVncOption.AdminPasswd</returns>
        public static bool ValidateInputAccount()
        {
            LoadAdminPasswdInUserTempFile();

            if (string.IsNullOrWhiteSpace(AdminUser))
            {
                return false;
            }

            var val = Win32AccountValidate.Validate(null, AdminUser, AdminPasswd);
            return val;
        }
        #endregion

        #region static CreateStartupBatchFile 
        private static readonly string _mainProcessExeFullPath = Process.GetCurrentProcess().MainModule.FileName;
        private static readonly string _startupFullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "RemoteSupport.bat");

        /// <summary>
        /// shell:startup 裡建立 啟動 batch
        /// </summary>
        /// <param name="vncPasswd"></param>
        public static void CreateStartupBatchFile(string vncPasswd)
        {
            try
            {
                string arg = $"start \"title\" \"{_mainProcessExeFullPath}\" -r {vncPasswd}" + " exit 0";

                if (IsCurrentUserInAdministrators())
                {
                    File.WriteAllText(_startupFullPath, arg);
                    return;
                }

                if (ValidateInputAccount())
                {
                    CreateAndWriteAdminPasswdToUserTempFile();
                }

                File.WriteAllText(_startupFullPath, arg);
                return;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public static void CreateStartupBatchFile(string vncPasswd)
        //{
        //    try
        //    {
        //        string arg;

        //        if (IsCurrentUserInAdministrators())
        //        {
        //            arg = $"start \"title\" \"{_mainProcessExeFullPath}\" -r {vncPasswd}" + " exit 0";
        //            File.WriteAllText(_startupFullPath, arg);
        //            return;
        //        }

        //        if (ValidateInputAccount())
        //        {
        //            var passdes = DESEncryptHelper.Encrypt(AdminPasswd);
        //            arg = $"start \"title\" \"{_mainProcessExeFullPath}\" -r {vncPasswd} -u {AdminUser} {passdes}" + " exit 0";
        //        }
        //        else
        //        {
        //            arg = $"start \"title\" \"{_mainProcessExeFullPath}\" -r {vncPasswd}" + " exit 0";
        //        }

        //        File.WriteAllText(_startupFullPath, arg);
        //        return;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        #endregion

        #region static DeleteStartupBatchFile
        /// <summary>
        /// 刪除啟動 batch
        /// </summary>
        public static void DeleteStartupBatchFile()
        {
            try
            {
                if (File.Exists(_startupFullPath))
                {
                    File.Delete(_startupFullPath);
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region static ToLauchRemoteSupportGetArguments
        /// <summary>
        /// 獲取啟動參數
        /// </summary>
        /// <param name="args"></param>
        public static void ToLauchRemoteSupportGetArguments(string[] args)
        {
            IsAfterReboot = false;
            if (args.Length <= 0)
            {
                return;
            }

            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-r")
                    {
                        IsAfterReboot = true;
                        UVncPassword = args[i + 1];
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region static CreateAndWritePasswdToUserTempDir
        /// <summary>
        /// 獲取 當前用戶 temp 文件夾下的 臨時保存文件路徑
        /// </summary>
        /// <returns></returns>
        private static string GetUserTempFile()
        {
            var temp = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.User);
            if (string.IsNullOrWhiteSpace(temp))
            {
                return null;
            }

            return Path.Combine(temp, "remotesupport.txt");
        }


        public static void CreateAndWriteAdminPasswdToUserTempFile()
        {
            try
            {
                var temp = GetUserTempFile();
                if (string.IsNullOrWhiteSpace(temp))
                {
                    return;
                }

                var passdes = DESEncryptHelper.Encrypt(AdminPasswd);
                string txt = AdminUser + "," + passdes + Environment.NewLine;
                File.WriteAllText(temp, txt);
            }
            catch (Exception)
            {
                return;
            }
        }
        #endregion

        #region static LoadAdminPasswdInUserTempFile
        /// <summary>
        /// 從當前用戶 temp 文件夾裡 獲取 保存的臨時加密的 管理員賬戶密碼,
        /// </summary>
        private static void LoadAdminPasswdInUserTempFile()
        {
            try
            {
                var temp = GetUserTempFile();
                if (!File.Exists(temp))
                {
                    return;
                }

                var txt = File.ReadAllText(temp);
                if (!string.IsNullOrWhiteSpace(txt))
                {
                    var strs = txt.Split(',');
                    AdminUser = strs[0].Trim();
                    AdminPasswd = DESEncryptHelper.Decrypt(strs[1].Trim());
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        #endregion

        #region static DeleteUserTempFile
        /// <summary>
        /// 刪除 當前用戶 temp 文件夾下的 保存管理員賬戶密碼的臨時文件
        /// </summary>
        public static void DeleteUserTempFile()
        {
            try
            {
                var temp = GetUserTempFile();
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        #endregion
    }
}
