using System;
using System.Security.Cryptography;
using System.Text;

namespace CryptoProtect
{
    /// <summary>
    /// Хеш-функция
    /// </summary>
    public class HashFunction
    {
        /// <summary>
        /// Контрольная сумма (соль)
        /// </summary>
        /// <param name="password">Пароль</param>
        /// <returns>Соль</returns>
        private static int SumControl(string password)
        {
            int sum = 0;
            if (password.Length % 2 != 0) password += password[password.Length / 2];
            for (int i = 0; i < password.Length; i+=2)
            {
                int mult = password[i] * password[i + 1];
                int div = password[i] / password[i + 1];
                sum += mult;
                sum += div;
            }
            return sum;
        }
        /// <summary>
        /// Хеширование
        /// </summary>
        /// <param name="input">Строка</param>
        /// <returns>Хеш</returns>
        public static string GetHash(string input)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input+SumControl(input)));
            return Convert.ToBase64String(hash);
        }
    } 
}

