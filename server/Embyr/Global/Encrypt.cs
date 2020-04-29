using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Embyr.Global
{
    class Encrypt
    {
        private static string plainText;
        private const string initVector = "pemgail9uzpgzl88";
        private const int keysize = 256;
        private const string passPhrase = "mwpHHP17937139//!";

        public static string encryptText
        {
            get { return Encode(plainText, passPhrase); }
            set { plainText = value; }
        }

        public static string GlobalVar
        {
            get { return Encode(plainText, passPhrase); }
            set { plainText = value; }
        }

        public static string Encode(string plainText, string passPhrase)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }
    }
}

