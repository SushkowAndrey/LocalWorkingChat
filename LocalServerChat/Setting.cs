using System;
using System.IO;
using static CryptoProtect.Encryption;

namespace LocalServerChat
{
    /// <summary>
    /// Общие методы
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// Получение строки подключения
        /// </summary>
        public static string GetConnectionString()
        {
            StreamReader sr = new StreamReader("dbconnect.txt");
            return Decrypt(sr.ReadToEnd());
        }
        /// <summary>
        /// Информация в консоле
        /// </summary>
        /// <param name="message">Сообщение</param>
        public static void ConsoleInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
        }
        /// <summary>
        /// Системная информация в консоле
        /// </summary>
        /// <param name="message">Сообщение</param>
        public static void ConsoleSystem(string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(message);
        }
        /// <summary>
        /// Предупредение в консоле
        /// </summary>
        /// <param name="message">Сообщение</param>
        public static void ConsoleWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(message);
        }
    }
}