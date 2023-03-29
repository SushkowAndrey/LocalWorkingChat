using System;
using System.Windows;
using ModelData;
using MySql.Data.MySqlClient;
using static Logging.Logging;

namespace LocalWorkingChat.CommonModule
{
    /// <summary>
    /// Работа с прикрепленными файлами
    /// </summary>
    public class DBAttachmentFile
    {
        /// <summary>
        /// Подключение к БД
        /// </summary>
        private MySqlConnection connection;
        /// <summary>
        /// Конструктор подключения к БД-при инициализации класса
        /// </summary>
        public DBAttachmentFile(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
        }
        /// <summary>
        /// Получить файл
        /// </summary>
        /// <param name="message">Сообщение</param>
        public byte[] GetAttachmentFile(Message message)
        {
            byte[] bufferAttachmentFile = null;
            try
            {
                connection.Open();
                string sql = $"SELECT " +
                             $"file_pdf " +
                             $"FROM information_register_message_attached_files " +
                             $"WHERE id_message = '{message.id}';";
                var command = new MySqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };
                var result = command.ExecuteReader();
                while (result.Read())
                {
                    bufferAttachmentFile = result.IsDBNull(0) ? null : (byte[])result.GetValue(0);
                }
                LogSuccess("Текущий вызов на сервер метода GetAttachmentFile");
                connection.Close();
            }
            catch (Exception ex)
            {
                connection.Close();
                MessageBox.Show("Ошибка чтения БД GetAttachmentFile-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                                ". Трассировка стека: " + ex.StackTrace, "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                LogError("Ошибка чтения БД GetAttachmentFile-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                         ". Трассировка стека: " + ex.StackTrace);
            }
            return bufferAttachmentFile; 
        }
    }
}