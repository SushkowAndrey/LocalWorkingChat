using System;
using System.IO;
using System.Threading.Tasks;
using ModelData;
using MySql.Data.MySqlClient;
using static Logging.Logging;

namespace LocalServerChat.DBServer
{
    /// <summary>
    /// Класс работы с сообщениями на сервере асинхронно
    /// </summary>
    public class DBMessage
    {
        /// <summary>
        /// Подключение к БД
        /// </summary>
        private static MySqlConnection connection;

        /// <summary>
        /// Асинхнонная операция записи сообщения на сервер
        /// </summary>
        /// <param name="message"></param>
        /// <param name="connectionString">Строка подключения</param>
        /// <param name="bufferAttachmentFile">Файл</param>
        public static void RegistrationMessagesAsync(Message message, string connectionString, byte [] bufferAttachmentFile = null)
        {
            try
            {
                Task.Run(() => RegistrationMessages(message, connectionString, bufferAttachmentFile));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка чтения БД RegistrationMessagesAsync-Исключение: " + ex.Message + ". Метод: " + 
                                  ex.TargetSite + ". Трассировка стека: " + ex.StackTrace);
                LogError("Ошибка чтения БД RegistrationMessagesAsync-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                         ". Трассировка стека: " + ex.StackTrace);
            }
        }
        /// <summary>
        /// Регистрация сообщений пользователя
        /// </summary>
        /// <param name="message"></param>
        /// <param name="connectionString">Строка подключения</param>
        /// <param name="bufferAttachmentFile">Файл</param>
        private static void RegistrationMessages(Message message, string connectionString, byte [] bufferAttachmentFile = null)
        {
            MySqlTransaction transaction = null;
            connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                string sql = $"INSERT INTO table_message (id,text_message, users_id_sender, users_id_recipient, date_time, type_message) " +
                             $"VALUES ('{message.id}'," +
                             $"'{message.textMessage}'," +
                             $"'{message.idSender}'," +
                             $"IF({message.idRecipient != null},'{message.idRecipient}',NULL)," +
                             $"'{DateTime.Now:s}','{message.typeMessage}');";
                var command = new MySqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };
                if (bufferAttachmentFile != null)
                {
                    sql += $"INSERT INTO information_register_message_attached_files (name_file, file_pdf,id_message) " +
                           $"VALUES ('{message.textMessage}',@file,'{message.id}');";
                    command.Parameters.Add(new MySqlParameter($"@file", bufferAttachmentFile));
                    command.CommandText = sql;
                }
               
                command.ExecuteNonQuery();
                transaction.Commit();
                connection.Close();
            }
            catch (Exception ex)
            {
                if (transaction != null) 
                    transaction.Rollback();
                connection.Close();
                Console.WriteLine("Ошибка чтения БД RegistrationUser-Исключение: "+ex.Message+". Метод: "
                                  +ex.TargetSite+". Трассировка стека: "+ex.StackTrace);
                LogError("Ошибка чтения БД RegistrationUser-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                         ". Трассировка стека: " + ex.StackTrace);
            }
        }
    }
}