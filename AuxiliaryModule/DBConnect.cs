using System;
using ModelData;
using MySql.Data.MySqlClient;
using static Logging.Logging;
using static CryptoProtect.Encryption;
using static CryptoProtect.HashFunction;

namespace SerializationData
{
    /// <summary>
    /// Работа с БД
    /// </summary>
    public class DBConnect
    {
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
        # region PROGRAM INTERFACE
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
        /// <summary>
        /// Установка электронной почты
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="email">Почта</param>
        /// <returns>Результат</returns>
        public (bool res, string error) SetEmailUser(User user, string email)
        { 
            MySqlTransaction transaction = null;
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                string sql = $"UPDATE information_register_user_email SET mail = '{email}', users_id= '{user.id}' " +
                             $"WHERE users_id = '{user.id}';";
                var command = new MySqlCommand
                { 
                    Connection = connection,
                    CommandText = sql
                };
                var result = command.ExecuteNonQuery();
                if (result == 0)
                {
                    sql = $"INSERT INTO information_register_user_email (mail,users_id)" +
                          $"VALUES ('{email}','{user.id}');";
                    command = new MySqlCommand
                    {
                        Connection = connection,
                        CommandText = sql
                    };
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
                connection.Close();
                LogSuccess("Текущий вызов на сервер метода SetEmailUser");
                return (res: true, error: "");
            }
            catch (MySqlException ex)
            {
                if (transaction != null) 
                    transaction.Rollback();
                connection.Close();
                LogError("Ошибка чтения БД SetEmailUser-Исключение: "+ex.Message+". Метод: "+ex.TargetSite+". Трассировка стека: "+ex.StackTrace);
                if (ex.Number == 1062)
                {
                    return (res: false, error: "Данная электронная почта уже используется другим пользователем");
                }
                return (res: false, error: "Ошибка чтения БД SetEmailUser-Исключение: "+ex.Message+". Метод: "+ex.TargetSite+". Трассировка стека: "+ex.StackTrace);
            }
            catch (Exception ex)
            {
                if (transaction != null) 
                    transaction.Rollback();
                connection.Close();
                LogError("Ошибка чтения БД SetEmailUser-Исключение: "+ex.Message+". Метод: "+ex.TargetSite+". Трассировка стека: "+ex.StackTrace);
                return (res: false, error: "Ошибка чтения БД SetEmailUser-Исключение: "+ex.Message+". Метод: "+ex.TargetSite+". Трассировка стека: "+ex.StackTrace);
            }
        }
        /// <summary>
        /// Смена пароля
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="newPassword">Новый пароль</param>
        /// <returns>Результат</returns>
        public (bool res, string error) ChangePassword(User user, string newPassword)
        {
            MySqlTransaction transaction = null;
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                var sql = $"UPDATE table_users SET password = '{GetHash(newPassword)}' WHERE id = '{user.id}'";
                var command = new MySqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };
                transaction.Commit();
                command.ExecuteNonQuery();
                connection.Close();
                return (res: true, error: "");
            }
            catch (Exception ex)
            {
                if (transaction != null) 
                    transaction.Rollback();
                connection.Close();
                LogError("Ошибка чтения БД SetEmailUser-Исключение: "+ex.Message+". Метод: "+ex.TargetSite+". Трассировка стека: "+ex.StackTrace);
                return (res: false, error: "Ошибка чтения БД SetEmailUser-Исключение: "+ex.Message+". Метод: "+ex.TargetSite+". Трассировка стека: "+ex.StackTrace);
            }
        }
        /// <summary>
        /// Получение электронной почты пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns>Почта</returns>
        public (string email, string error)  GetEmailUser(User user)
        {
            string email = null;
            try
            {
                connection.Open();
                var sql = 
                    $"SELECT " +
                    $"mail " + 
                    $"FROM information_register_user_email " +
                    $"WHERE users_id = '{user.id}';";
                var command = new MySqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };
                var result = command.ExecuteReader();
                while (result.Read())
                {
                    email = result.IsDBNull(0) ? null :  result.GetString(0);
                }
                connection.Close();
                return (res: email, error: null);
            }
            catch (Exception ex)
            {
                connection.Close();
                LogError("Ошибка чтения БД GetEmailUser-Исключение: "+ex.Message+". Метод: "+ex.TargetSite+". Трассировка стека: "+ex.StackTrace);
                return (res: null, error: "Ошибка чтения БД GetEmailUser-Исключение: "+ex.Message+". Метод: "+ex.TargetSite+". Трассировка стека: "+ex.StackTrace);
            }
        }
        # endregion region
    }
}