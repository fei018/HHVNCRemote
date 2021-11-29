using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;
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
            if (string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            if (File.Exists(_ultravncIni))
            {
                var pass = EncryptVNC(password) + "00";

                var lines = File.ReadAllLines(_ultravncIni);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("passwd"))
                    {
                        lines[i] = "passwd=" + pass;

                        if (lines[i + 1].StartsWith("passwd2"))
                        {
                            lines[i + 1] = "passwd2=" + pass;
                        }

                        break;
                    }
                }
                File.WriteAllLines(_ultravncIni, lines);
                return;
            }
        }

        //public void SetPasswordToUltraVNCini(string password)
        //{
        //    var cmd = new CMDHelper();
        //    cmd.CreateProcessExe(_setpasswordExe, password + " " + password);
        //}
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

        #region TryGetVncPasswordFromUltravncIni
        public string TryGetVncPasswordFromUltravncIni()
        {
            string pass = null;
            try
            {
                if (File.Exists(_ultravncIni))
                {
                    var allLines = File.ReadAllLines(_ultravncIni);
                    foreach (var line in allLines)
                    {
                        if (line.ToLower().StartsWith("passwd"))
                        {
                            pass = line.Split('=')[1];
                            break;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(pass))
                    {
                        return DecryptVNC(pass.Substring(0, 16));
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
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
                if (ValidateAccountOrOpenInputAccountBox.Invoke(null, null))
                {
                    UVncOption.WriteAdminInfoToTempAdminTxt();
                    return true;
                }
                return false;
            }
            else
            {
                throw new Exception("用戶輸入框事件未訂閱.");
            }
        }
        #endregion

        #region EncryptVNC password
        public static string EncryptVNC(string password)
        {
            if (password.Length > 8)
            {
                password = password.Substring(0, 8);
            }
            if (password.Length < 8)
            {
                password = password.PadRight(8, '\0');
            }

            byte[] key = { 23, 82, 107, 6, 35, 78, 88, 7 };
            byte[] passArr = new ASCIIEncoding().GetBytes(password);
            byte[] response = new byte[passArr.Length];
            char[] chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

            // reverse the byte order
            byte[] newkey = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                // revert desKey[i]:
                newkey[i] = (byte)(
                    ((key[i] & 0x01) << 7) |
                    ((key[i] & 0x02) << 5) |
                    ((key[i] & 0x04) << 3) |
                    ((key[i] & 0x08) << 1) |
                    ((key[i] & 0x10) >> 1) |
                    ((key[i] & 0x20) >> 3) |
                    ((key[i] & 0x40) >> 5) |
                    ((key[i] & 0x80) >> 7)
                    );
            }
            key = newkey;
            // reverse the byte order

            DES des = new DESCryptoServiceProvider();
            des.Padding = PaddingMode.None;
            des.Mode = CipherMode.ECB;

            ICryptoTransform enc = des.CreateEncryptor(key, null);
            enc.TransformBlock(passArr, 0, passArr.Length, response, 0);

            string hexString = String.Empty;
            for (int i = 0; i < response.Length; i++)
            {
                hexString += chars[response[i] >> 4];
                hexString += chars[response[i] & 0xf];
            }
            return hexString.Trim();
        }

        public static string DecryptVNC(string password)
        {
            if (password.Length < 16)
            {
                return string.Empty;
            }

            byte[] key = { 23, 82, 107, 6, 35, 78, 88, 7 };
            byte[] passArr = ToByteArray(password);
            byte[] response = new byte[passArr.Length];

            // reverse the byte order
            byte[] newkey = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                // revert key[i]:
                newkey[i] = (byte)(
                    ((key[i] & 0x01) << 7) |
                    ((key[i] & 0x02) << 5) |
                    ((key[i] & 0x04) << 3) |
                    ((key[i] & 0x08) << 1) |
                    ((key[i] & 0x10) >> 1) |
                    ((key[i] & 0x20) >> 3) |
                    ((key[i] & 0x40) >> 5) |
                    ((key[i] & 0x80) >> 7)
                    );
            }
            key = newkey;
            // reverse the byte order

            DES des = new DESCryptoServiceProvider();
            des.Padding = PaddingMode.None;
            des.Mode = CipherMode.ECB;

            ICryptoTransform dec = des.CreateDecryptor(key, null);
            dec.TransformBlock(passArr, 0, passArr.Length, response, 0);

            return System.Text.ASCIIEncoding.ASCII.GetString(response);
        }

        public static byte[] ToByteArray(String HexString)
        {
            int NumberChars = HexString.Length;
            byte[] bytes = new byte[NumberChars / 2];

            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(HexString.Substring(i, 2), 16);
            }

            return bytes;
        }
        #endregion
    }
}
