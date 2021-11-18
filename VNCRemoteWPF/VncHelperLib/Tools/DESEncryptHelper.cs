using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VncHelperLib
{
    public static class DESEncryptHelper
    {
        //必須8位
        private static readonly string sKey = "12345678";

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="str">需要加密的</param>
        /// <returns></returns>
        public static string Encrypt(string encryptString)
        {
            if (string.IsNullOrWhiteSpace(encryptString))
            {
                return null;
            }

            using DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            byte[] sor = Encoding.UTF8.GetBytes(encryptString);
            //传入key、iv
            des.Key = Encoding.UTF8.GetBytes(sKey);
            des.IV = Encoding.UTF8.GetBytes(sKey);

            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);

            cs.Write(sor, 0, sor.Length);
            cs.FlushFinalBlock();

            StringBuilder sb = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                sb.AppendFormat("{0:X2}", b);
            }
            return sb.ToString();
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="pToDecrypt">需要解密的</param>
        /// <returns></returns>
        public static string Decrypt(string pToDecrypt)
        {
            if (string.IsNullOrWhiteSpace(pToDecrypt))
            {
                return null;
            }

            using DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = new byte[pToDecrypt.Length / 2];
            for (int x = 0; x < pToDecrypt.Length / 2; x++)
            {
                int i = (Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }

            des.Key = Encoding.UTF8.GetBytes(sKey);//***************key与加密时的Key保持一致
            des.IV = Encoding.UTF8.GetBytes(sKey);//*****************skey与加密时的IV保持一致

            using MemoryStream ms = new MemoryStream();

            using CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            StringBuilder ret = new StringBuilder();
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
