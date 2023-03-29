using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using LocalServerChat.DBServer;
using ModelData;
using ModelData.Setting;
using static SerializationData.WorkingJson;

namespace LocalServerChat
{
    /// <summary>
    /// Обновление программы
    /// </summary>
    public class UpdateProgram
    {
        /// <summary>
        /// Процесс обновления версий
        /// </summary>
        public static void UpdateVersions(VersionProgram currentVersion,VersionProgram newVersion, 
            string VERSIONPROGRAM, string DATAPROGRAM, string connectionString)
        {
            var versionProgram = VERSIONPROGRAM.Replace(".", "");
            currentVersion.version = currentVersion.version.Replace(".", "");
            var versionProgramInt = Convert.ToInt32(versionProgram);
            var versionLocalInt = Convert.ToInt32(currentVersion.version);
            DBCreateUpdateTables dbCreateTable = new DBCreateUpdateTables(connectionString);
            while (versionLocalInt < versionProgramInt)
            {
                versionLocalInt++;
                dbCreateTable.UpdatingTables(versionLocalInt);
            }
            newVersion.version = VERSIONPROGRAM;
            newVersion.date = DATAPROGRAM;
            UpdateVersion(newVersion);
        }
        /// <summary>
        /// Запись новой версии в файл (сериализация)
        /// </summary>
        /// <param name="newVersion">Модель данных версии для записи</param>
        private static void UpdateVersion(VersionProgram newVersion)
        {
            try
            {
                using var file = new StreamWriter("version.txt", false);
                file.WriteLine(SerializationJson(newVersion));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now:u}-Ошибка сохранения новой версии версии-{ex.Message}");
            }
        }
    }
}