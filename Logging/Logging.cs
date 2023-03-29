using System;
using System.IO;
using System.Security;

namespace Logging
{
    /// <summary>
    /// Логирование
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// Свойство класса - путь сохранения
        /// </summary>
        private static string path = @"log.txt";
        # region PROGRAM INTERFACE
        /// <summary>
        /// Запись информации
        /// </summary>
        /// <param name="message">Текст информации</param>
        public static void LogInfo(string message, bool isEnabledLog = false)
        {
            if(isEnabledLog)
                WriteToFile($"{DateTime.Now:u}-({Environment.UserName}): INFO {message}");
        }
        /// <summary>
        /// Запись ошибки
        /// </summary>
        /// <param name="message">Текст ошибки</param>
        public static void LogError(string message)
        {
            WriteToFile($"{DateTime.Now:u}-({Environment.UserName}): ERROR {message}");
        }
        /// <summary>
        /// Запись успешной операции
        /// </summary>
        /// <param name="message">Текст успешной операции</param>
        public static void LogSuccess(string message, bool isEnabledLog = false)
        {
            if(isEnabledLog)
                WriteToFile($"{DateTime.Now:u}-({Environment.UserName}): SUCCESS {message}");
        }
        /// <summary>
        /// Иная запись
        /// </summary>
        /// <param name="type">Тип записи</param>
        /// <param name="message">Текст записи</param>
        public static void LogCustom(string type, string message, bool isEnabledLog = false)
        {
            if(isEnabledLog) 
                WriteToFile($"{DateTime.Now:u}-({Environment.UserName}): {type} {message}");
        }
        # endregion region
        # region FORM METHODS
        /// <summary>
        /// Метод записи операций логирования в файл
        /// </summary>
        /// <param name="message">Текст логирования</param>
        /// <exception cref="Exception">Исключения при работе с файлом</exception>
        private static void WriteToFile(string message)
        {
            try
            {
                using var file = new StreamWriter(path, true);
                file.WriteLine(message);
            }
            catch (UnauthorizedAccessException)
            {
                throw new Exception("Отказано в доступе");
            }
            catch (ArgumentException)
            {
                throw new Exception("Параметр path пуст или path содержит имя системного устройства (com1, com2 и т. д.)");
            }
            catch (DirectoryNotFoundException)
            {
                throw new Exception("Указан недопустимый путь (например, он ведет на несопоставленный диск)");
            }
            catch (IOException)
            {
                throw new Exception("Параметр path включает неверный или недопустимый синтаксис имени файла, имени каталога или метки тома");
            }
            catch (SecurityException)
            {
                throw new Exception("У вызывающего объекта отсутствует необходимое разрешение");
            }
        }
        # endregion region
    }
}

