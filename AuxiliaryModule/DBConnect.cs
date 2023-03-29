using System;
using ModelData;
using MySql.Data.MySqlClient;
using static Logging.Logging;

namespace SerializationData
{
    /// <summary>
    /// Работа с БД
    /// </summary>
    public class DBConnect
    {
        /// <summary>
        /// Строка подключения к БД
        /// </summary>
        string connectionString;
        /// <summary>
        /// Подключение к БД
        /// </summary>
        private MySqlConnection connection;
        /// <summary>
        /// Конструктор подключения к БД-при инициализации класса
        /// </summary>
        public DBConnect(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
        }
        /// <summary>
        /// Получение данных администратора
        /// </summary>
        /// <returns>Данные администратора</returns>
        public User GetAdmin()
        {
            User admin = new User();
            try
            {
                connection.Open();
                var sql = 
                    $"SELECT " +
                    $"id," +
                    $"name_user," +
                    $"is_active," +
                    $"is_admin " +
                    $"FROM table_users " +
                    $"WHERE is_admin = 'true';";
                var command = new MySqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };
                var result = command.ExecuteReader();
                while (result.Read())
                {
                    admin.id = result.IsDBNull(0) ? null :  result.GetString(0);
                    admin.nameUser = result.IsDBNull(1) ? null :  result.GetString(1);
                    admin.isActive = !result.IsDBNull(2) && result.GetBoolean(2);
                    admin.isAdmin = !result.IsDBNull(3) && result.GetBoolean(3);
                }
                connection.Close();
                if (!string.IsNullOrEmpty(admin.id))
                {
                    return admin;
                }
            }
            catch (Exception ex)
            {
                connection.Close();
                LogError("Ошибка чтения БД GetAdmin-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                         ". Трассировка стека: " + ex.StackTrace);
            }
            return null;
        }
    }
}