using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CryptoProtect
{
    /// <summary>
    /// Шифрование
    /// </summary>
    public class Encryption
    {
        /// <summary>
        /// Ключ шифрования
        /// </summary>
        private static string EncryptionKey = "hji7j8pH9H6f3w4I43dYesRo";
        /// <summary>
        /// Ключ XOR операции
        /// </summary>
        private static ushort secretKey = 54366;
        /// <summary>
        /// Производим XOR операцию
        /// </summary>
        /// <param name="character">Символ</param>
        /// <param name="secretKey">Ключ</param>
        /// <returns></returns>
        private static char TopSecret(char character, ushort secretKey)
        {
            character = (char)(character ^ secretKey);
            return character;
        }
        /// <summary>
        /// Шифрование/дешифрование XOR
        /// </summary>
        /// <param name="str">Строка</param>
        /// <returns>Результат</returns>
        public static string EncodeDecrypt(string str)
        {
            var ch = str.ToArray(); //преобразуем строку в символы
            string newStr = ""; //переменная которая будет содержать зашифрованную строку
            foreach (var c in ch) //выбираем каждый элемент из массива символов нашей строки
            {
                newStr += TopSecret(c,
                    secretKey); //производим шифрование каждого отдельного элемента и сохраняем его в строку
            }

            return newStr;
        }
        /// <summary>
        /// Шифрование
        /// </summary>
        /// <param name="clearText">Строка</param>
        /// <returns>Результат</returns>
        public static string Encrypt(string clearText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey,
                    new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }

                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        /// <summary>
        /// Дешифрование
        /// </summary>
        /// <param name="cipherText">Строка</param>
        /// <returns>Результат</returns>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return null;
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey,
                    new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }

                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}