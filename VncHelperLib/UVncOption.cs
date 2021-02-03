using System;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;

namespace VncHelperLib
{
    public class UVncOption : NotifyPropertyObject
    {
        public static string AdminUser { get; set; }
        public static string AdminPasswd { get; set; }
        public static string UVncPassword { get; set; }
        public static bool IsAfterReboot { get; set; }

        public static readonly string HostName = Dns.GetHostName();

        public static readonly string HostIP = Dns.GetHostEntry(HostName).AddressList.FirstOrDefault(p => p.AddressFamily.ToString() == "InterNetwork")?.ToString();

        #region InitialVncPath
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
            throw new Exception("未有安裝 UltraVNC.");
        }
        #endregion

        #region GetUVncInstance
        public static IUVnc GetUVncInstance()
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
        #endregion

        #region IsCurrentUserInAdministrators
        public static bool IsCurrentUserInAdministrators()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            return isElevated;
        }
        #endregion

        #region ValidateInputAccount
        public static bool ValidateInputAccount()
        {
            if (string.IsNullOrWhiteSpace(AdminUser))
            {
                return false;
            }

            var val = Win32AccountValidate.Validate(null, AdminUser, AdminPasswd);
            return val;
        }
        #endregion

        #region CreateStartupBatchFile 
        private static readonly string _mainProcessExeFullPath = Process.GetCurrentProcess().MainModule.FileName;
        private static readonly string _startupFullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "RemoteSupport.bat");

        public static void CreateStartupBatchFile(string vncPasswd)
        {
            try
            {
                string arg;

                if (IsCurrentUserInAdministrators())
                {
                    arg = $"start \"title\" \"{_mainProcessExeFullPath}\" -r {vncPasswd}" + " exit 0";
                    File.WriteAllText(_startupFullPath, arg);
                    return;
                }

                if (ValidateInputAccount())
                {
                    var passdes = DESEncryptHelper.Encrypt(AdminPasswd);
                    arg = $"start \"title\" \"{_mainProcessExeFullPath}\" -r {vncPasswd} -u {AdminUser} {passdes}" + " exit 0";
                }
                else
                {
                    arg = $"start \"title\" \"{_mainProcessExeFullPath}\" -r {vncPasswd}" + " exit 0";
                }

                File.WriteAllText(_startupFullPath, arg);
                return;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region DeleteStartupBatchFile
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

        #region ToLauchRemoteSupportGetArguments
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

                    if (args[i] == "-u")
                    {
                        AdminUser = args[i + 1];
                        AdminPasswd = args[i + 2];
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }
}
