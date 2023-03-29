using System;
using MySql.Data.MySqlClient;
using static Logging.Logging;

namespace LocalServerChat.DBServer
{
    /// <summary>
    /// Создание/обновление данных в БД
    /// </summary>
    public class DBCreateUpdateTables
    {
        /// <summary>
        /// Свойство класса - работа с БД
        /// </summary>
        private MySqlConnection connection;
        /// <summary>
        /// Конструктор класса для создания таблиц при обновлении
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        public DBCreateUpdateTables(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
        }
        /// <summary>
        /// Метод обновления таблиц в БД-при необходимости добавляются методы создания таблиц
        /// </summary>
        /// <param name="version">Передается номер текущей версии</param>
        public void UpdatingTables(int version)
        {
            switch (version)
            {
                case 10000:
                    Console.WriteLine($"{DateTime.Now:u}-Исходная версия-{version}");
                    break;
                case 10001:
                    CreateTable1_0_0_01(version);
                    break;
                case 10002:
                    UpdateTableUser1_0_0_02(version);
                    CreateUserAdmin1_0_0_02(version);
                    break;
                case 10003:
                    UpdateTableMessage1_0_0_03(version);
                    break;
                case 10004:
                    UpdateTableMessage1_0_0_04(version);
                    break;
            }
        }
        /// <summary>
        /// Обновление таблицы версии 1.0.0.01-Таблица пользователей
        /// </summary>
        /// <param name="version">Текущая версия для логирования</param>
        private void CreateTable1_0_0_01(int version)
        {
            try 
            {
                connection.Open();
                string sql = $"create table table_users" +
                             $"(id  varchar(50) primary key, " +
                             $"name_user varchar(150) not null unique, " +
                             $"password   varchar(100)   not null, " +
                             $"is_active   varchar(10) DEFAULT 'true');" +
                             $"create table table_users_online" +
                             $"(users_id  varchar(50)  primary key, " +
                             $"FOREIGN KEY (users_id)  REFERENCES table_users (id) ON DELETE RESTRICT, " +
                             $"date_time   DATETIME  not null);" +
                             $"create table table_message" +
                             $"(id  varchar(50) primary key, " +
                             $"text_message   text, " +
                             $"users_id_sender  varchar(50), " +
                             $"FOREIGN KEY (users_id_sender)  REFERENCES table_users (id) ON DELETE RESTRICT, " +
                             $"attached_files MEDIUMBLOB, users_id_recipient  varchar(50)," +
                             $"FOREIGN KEY (users_id_recipient)  REFERENCES table_users (id) ON DELETE RESTRICT, " +
                             $"date_time   DATETIME  not null);";
                var command = new MySqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };
                command.ExecuteNonQuery();
                connection.Close();
                Console.WriteLine($"{DateTime.Now:u}-Обновлена версия-{version}. CreateTable1_0_0_01");
            }
            catch (Exception ex)
            { 
                Console.WriteLine($"{DateTime.Now:u}-CreateTable1_0_0_01-Исключение: "+ex.Message+". Метод: "
                                  +ex.TargetSite+". Трассировка стека: "+ex.StackTrace);
                LogError("Ошибка чтения БД CreateTable1_0_0_01-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                         ". Трассировка стека: " + ex.StackTrace);
                connection.Close();
            }
        }
        /// <summary>
        /// Обновление таблицы версии 1.0.0.02-Таблица пользователь-тип пользователя
        /// </summary>
        /// <param name="version">Текущая версия для логирования</param>
        private void UpdateTableUser1_0_0_02(int version)
        {
            MySqlTransaction transaction = null;
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                string sql = $"ALTER TABLE " +
                             $"table_users " +
                             $"ADD COLUMN is_admin VARCHAR(10) not null DEFAULT 'false';";
                var command = new MySqlCommand
                {
                    Connection = connection, 
                    CommandText = sql
                };
                command.ExecuteNonQuery();
                transaction.Commit();
                connection.Close();
                Console.WriteLine($"{DateTime.Now:u}-Обновлена версия-{version}. Обновлена таблица UpdateTableUser1_0_0_02");
            }
            catch (Exception ex)
            {
                if (transaction != null) 
                    transaction.Rollback();
                Console.WriteLine($"{DateTime.Now:u}-UpdateTableUser1_0_0_02-Исключение: " + ex.Message + ". Метод: " 
                                  + ex.TargetSite + ". Трассировка стека: " + ex.StackTrace);
                LogError("Ошибка чтения БД UpdateTableUser1_0_0_02-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                         ". Трассировка стека: " + ex.StackTrace);
                connection.Close();
            }
        }
        /// <summary>
        /// Обновление таблицы версии 1.0.0.02-Обновление в БД-пользователь админ
        /// </summary>
        /// <param name="version">Текущая версия для логирования</param>
        private void CreateUserAdmin1_0_0_02(int version)
        {
            MySqlTransaction transaction = null;
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                string sql = $"INSERT INTO table_users (id,name_user,password,is_admin) " +
                             $"VALUES ('00000000-0000-0000-0000-000000000001','Server','Server','True');";
                var command = new MySqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };
                command.ExecuteNonQuery();

                transaction.Commit();
                connection.Close();
                Console.WriteLine($"{DateTime.Now:u}-Обновлена версия-{version}. Обновлена таблица CreateUserAdmin1_0_0_02");
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    transaction.Rollback();
                connection.Close();
                LogError("Ошибка чтения БД CreateUserAdmin1_0_0_02-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                         ". Трассировка стека: " + ex.StackTrace);
                Console.WriteLine($"{DateTime.Now:u}-CreateUserAdmin1_0_0_02-Исключение: " + ex.Message + ". Метод: " 
                                  + ex.TargetSite + ". Трассировка стека: " + ex.StackTrace);
            }
        }
        /// <summary>
        /// Обновление таблицы версии 1.0.0.03-Таблица сообщений-тип сообщения
        /// </summary>
        /// <param name="version">Текущая версия для логирования</param>
        private void UpdateTableMessage1_0_0_03(int version)
        {
            MySqlTransaction transaction = null;
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                string sql = $"ALTER TABLE " +
                             $"table_message " +
                             $"ADD COLUMN type_message varchar(50);";
                var command = new MySqlCommand
                {
                    Connection = connection, 
                    CommandText = sql
                };
                command.ExecuteNonQuery();
                transaction.Commit();
                connection.Close();
                Console.WriteLine($"{DateTime.Now:u}-Обновлена версия-{version}. Обновлена таблица UpdateTableMessage1_0_0_03");
            }
            catch (Exception ex)
            {
                if (transaction != null) 
                    transaction.Rollback();
                Console.WriteLine($"{DateTime.Now:u}-UpdateTableMessage1_0_0_03-Исключение: " + ex.Message + ". Метод: " 
                                  + ex.TargetSite + ". Трассировка стека: " + ex.StackTrace);
                LogError("Ошибка чтения БД UpdateTableMessage1_0_0_03-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                         ". Трассировка стека: " + ex.StackTrace);
                connection.Close();
            }
        }
        /// <summary>
        /// Обновление таблицы версии 1.0.0.04-Таблица сообщений
        /// </summary>
        /// <param name="version">Текущая версия для логирования</param>
        private void UpdateTableMessage1_0_0_04(int version)
        {
            MySqlTransaction transaction = null;
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                string sql = $"create table information_register_message_attached_files(" +
                             $"id int auto_increment primary key, " +
                             $"name_file   VARCHAR(250)          not null, " +
                             $"file_pdf MEDIUMBLOB not null, " +
                             $"id_message    VARCHAR(50) not null, " +
                             $"FOREIGN KEY (id_message)  REFERENCES table_message (id) ON DELETE CASCADE);" +
                             $"ALTER TABLE table_message DROP COLUMN attached_files";
                var command = new MySqlCommand
                {
                    Connection = connection, 
                    CommandText = sql
                };
                command.ExecuteNonQuery();
                transaction.Commit();
                connection.Close();
                Console.WriteLine($"{DateTime.Now:u}-Обновлена версия-{version}. Обновлена таблица UpdateTableMessage1_0_0_04");
            }
            catch (Exception ex)
            {
                if (transaction != null) 
                    transaction.Rollback();
                Console.WriteLine($"{DateTime.Now:u}-UpdateTableMessage1_0_0_04-Исключение: " + ex.Message + ". Метод: " 
                                  + ex.TargetSite + ". Трассировка стека: " + ex.StackTrace);
                LogError("Ошибка чтения БД UpdateTableMessage1_0_0_04-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                         ". Трассировка стека: " + ex.StackTrace);
                connection.Close();
            }
        }
    }
}