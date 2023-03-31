using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using LocalServerChat.DBServer;
using ModelData;
using ModelData.Setting;
using SerializationData;
using static SerializationData.WorkingJson;
using static LocalServerChat.UpdateProgram;
using static Logging.Logging;
using static LocalServerChat.Setting;

namespace LocalServerChat
{
    /// <summary>
    /// Основной класс сервера
    /// </summary>
    class Program
    {
        /// <summary>
        /// Строка подключения
        /// </summary>
        static string connectionString;
        /// <summary>
        /// Свойство класса - константа-версия новая при обновлении
        /// </summary>   
        private const string VERSIONPROGRAM = "1.0.0.05";
        /// <summary>
        /// Свойство класса - константа-дата обновления
        /// </summary>  
        private const string DATAPROGRAM = "29.03.2023";
        /// <summary>
        /// Свойство класса - версия программы из файла-текущая
        /// </summary>  
        private static VersionProgram newVersion;
        /// <summary>
        /// Свойство класса - версия программы из файла-текущая
        /// </summary>  
        private static VersionProgram currentVersion;
        /// <summary>
        /// Класс подключения к БД
        /// </summary>
        private static DBConnectServer dbConnectServer;
        /// <summary>
        /// Общий модуль работы с БД
        /// </summary>
        private static DBConnect dbConnect;
        /// <summary>
        /// Свойство класса - сервер
        /// </summary>
        private static ServerObject server;
        /// <summary>
        /// Свойство класса - поток для прослушивания
        /// </summary>
        private static Thread listenThread;
        /// <summary>
        /// Признак запуска сервера
        /// </summary>
        private static bool isStartServer = true;
        /// <summary>
        /// Исходная точка программы сервера
        /// </summary>
        static void Main(string[] args)
        {
            connectionString = GetConnectionString();
            dbConnectServer = new DBConnectServer(connectionString);
            dbConnect = new DBConnect(connectionString);
            GetCurrentVersion();
            CheckingVersion();
            if (!isStartServer)
            {
                ConsoleWarning($"{DateTime.Now:u}-Запрет запуска сервера. Перезапустите программу");
                return;
            }
            ConsoleSystem($"{DateTime.Now:u}-Сервер запущен");
            try
            {
                dbConnectServer.DeleteUserOnline();
                User admin = dbConnect.GetAdmin();
                dbConnectServer.RegistrationUserOnline(admin);
                server = new ServerObject();
                listenThread = new Thread(server.ListenConnect);
                listenThread.Start(admin);
            }
            catch (Exception ex)
            {
                server.Disconnect();
                ConsoleWarning($"{DateTime.Now:u}-Ошибка сервера-{ex.Message}");
            }
        }
        # region PROGRAM INTERFACE
        /// <summary>
        /// Получить текущую версию
        /// </summary>
        private static void GetCurrentVersion()
        {
            using (StreamReader reader = new StreamReader("version.txt"))
            {
                string version = reader.ReadToEnd();
                try
                {
                    currentVersion = DeserializationJson<VersionProgram>(version);
                }
                catch (Exception ex)
                {
                    isStartServer = false;
                    ConsoleWarning($"{DateTime.Now:u}-Ошибка чтения версии-{ex.Message}");
                    currentVersion.version = "0";
                }
            }
        }
        /// <summary>
        /// Проверка версии в программе и версии в файле
        /// </summary>
        private static void CheckingVersion()
        {
            ConsoleSystem($"{DateTime.Now:u}-Версия Сервера-{currentVersion.version} от {currentVersion.date}");
            newVersion = new VersionProgram();
            newVersion.version = VERSIONPROGRAM;
            newVersion.date = DATAPROGRAM;
            if (VERSIONPROGRAM != currentVersion.version)
            {
                UpdateVersions(currentVersion, newVersion, VERSIONPROGRAM, DATAPROGRAM,connectionString);
                isStartServer = false;
            }
        }
        # endregion region
    }
}