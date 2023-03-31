using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using ModelData;
using ModelData.Setting;
using MySql.Data.MySqlClient;
using static CryptoProtect.Encryption;
using static CryptoProtect.HashFunction;
using static Logging.Logging;

namespace LocalWorkingChat.CommonModule
{
    /// <summary>
    /// Класс для подключения к БД
    /// </summary>
    public class DBConnectClient
    {
        /// <summary>
        /// Подключение к БД
        /// </summary>
        private MySqlConnection connection;
        /// <summary>
        /// Конструктор подключения к БД-при инициализации класса
        /// </summary>
        public DBConnectClient(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
        }
        /// <summary>
        /// Проверка регистрации пользователя на сервере
        /// </summary>
        /// <param name="user">Информация о пользователе</param>
        /// <returns>Результат проверки-пользователь зарегистрирован (true) или нет (false)</returns>
        public User CheckUserRegistration(User user)
        {
            User getUser = new User();
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
                    $"WHERE name_user = '{user.nameUser}' AND " +
                    $"password = '{GetHash(user.password)}';";
                var command = new MySqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };
                var result = command.ExecuteReader();
                while (result.Read())
                {
                    getUser.id = result.IsDBNull(0) ? null :  result.GetString(0);
                    getUser.nameUser = result.IsDBNull(1) ? null :  result.GetString(1);
                    getUser.isActive = !result.IsDBNull(2) && result.GetBoolean(2);
                    getUser.isAdmin = !result.IsDBNull(3) && result.GetBoolean(3);
                }
                connection.Close();
                if (!string.IsNullOrEmpty(getUser.id))
                {
                    return getUser;
                }
            }
            catch (Exception ex)
            {
                connection.Close();
                MessageBox.Show("Ошибка чтения БД CheckRegistration-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                                ". Трассировка стека: " + ex.StackTrace, "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                LogError("Ошибка чтения БД CheckRegistration-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                         ". Трассировка стека: " + ex.StackTrace);
            }
            return getUser;
        }
        /// <summary>
        /// Проверка регистрации пользователя на сервере
        /// </summary>
        /// <param name="nameUser">Имя</param>
        /// <param name="password">Пароль</param>
        /// <returns>Результат проверки-пользователь зарегистрирован (true) или нет (false)</returns>
        public string GetUserId(string nameUser, string password)
        {
            string id = null;
            try
            {
                connection.Open();
                var sql = 
                    $"SELECT " +
                    $"id " +
                    $"FROM table_users " +
                    $"WHERE name_user = '{nameUser}' AND " +
                    $"password = '{GetHash(password)}';";
                var command = new MySqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };
                var result = command.ExecuteReader();
                while (result.Read())
                {
                    id = result.IsDBNull(0) ? null :  result.GetString(0);
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                connection.Close();
                MessageBox.Show("Ошибка чтения БД CheckRegistration-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                                ". Трассировка стека: " + ex.StackTrace, "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                LogError("Ошибка чтения БД CheckRegistration-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                         ". Трассировка стека: " + ex.StackTrace);
            }
            return id;
        }
        /// <summary>
        /// Регистрация пользователя
        /// </summary>
        /// <param name="user">Информация о пользователе</param>
        public bool RegistrationUser(User user)
        {
            try
            {
                connection.Open();
                string sql = $"INSERT INTO table_users (id, name_user, password) " +
                             $"VALUES ('{user.id}','{user.nameUser}','{GetHash(user.password)}');";
                var command = new MySqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };
                command.ExecuteNonQuery();
                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                connection.Close();
                MessageBox.Show("Ошибка чтения БД RegistrationUser-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                                ". Трассировка стека: " + ex.StackTrace, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                LogError("Ошибка чтения БД RegistrationUser-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                         ". Трассировка стека: " + ex.StackTrace);
            }
            return false;
        }
        /// <summary>
        /// Получение списка пользователей онлайн
        /// </summary>
        /// <returns>Список пользователей</returns>
        public ObservableCollection <UserOnline> GetListUsersOnline(User user)
        {
            ObservableCollection <UserOnline> usersOnline = new ObservableCollection<UserOnline>();
            try
            {
                connection.Open();
                var sql = 
                    $"SELECT " +
                    $"users_id, " +
                    $"(SELECT name_user FROM table_users WHERE id=users_id) AS name_user," +
                    $"DATE_FORMAT(date_time, '%d.%m.%Y-%H:%i') AS date_time " +
                    $"FROM table_users_online;";
                var command = new MySqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var nameUserTemp = user.nameUser == reader.GetString(1) ? "Избранное" : reader.GetString(1);
                            usersOnline.Add(new UserOnline()
                            {
                                id = reader.IsDBNull(0) ? null : reader.GetString(0),
                                nameUser = reader.IsDBNull(1) ? null : nameUserTemp,
                                dateTimeOnline = reader.IsDBNull(2) ? null : reader.GetString(2)
                            });
                        }
                    }
                }
                connection.Close();
                return usersOnline;
            }
            catch (Exception ex)
            {
                connection.Close();
                MessageBox.Show(
                    "Ошибка чтения БД GetListUsersOnline-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite + 
                    ". Трассировка стека: " + ex.StackTrace, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                LogError("Ошибка чтения БД GetListUsersOnline-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                         ". Трассировка стека: " + ex.StackTrace);
                return usersOnline;
            }
        }
        /// <summary>
        /// Получение истории сообщений общего чата
        /// </summary>
        /// <returns>Архив сообщений</returns>
        public ObservableCollection<Message> GetMessageHistory(string idUser, string idAdmin)
        {
            ObservableCollection <Message> messages = new ObservableCollection<Message>();
            try
            {
                connection.Open();
                var sql = 
                    $"SELECT id," +
                    $"text_message," +
                    $"users_id_sender," +
                    $"(SELECT name_user FROM table_users WHERE id=users_id_sender) AS sender," +
                    $"users_id_recipient," +
                    $"(SELECT name_user FROM table_users WHERE id=users_id_recipient) AS recipient," +
                    $"DATE_FORMAT(date_time, '%d.%m.%Y-%H:%i') AS date_time, " +
                    $"type_message " +
                    $"FROM table_message " +
                    $"WHERE users_id_sender = '{idAdmin}' OR " +
                    $"users_id_sender = '{idUser}' OR " +
                    $"users_id_recipient = '{idUser}' " +
                    $"ORDER BY date_time;";
                var command = new MySqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            bool isIncomingTemp = reader.GetString(2) != idUser;
                            Enum.TryParse(reader.IsDBNull(7) ? null : reader.GetString(7), out TypeMessage typeMessage);
                            messages.Add(new Message()
                            {
                                id = reader.IsDBNull(0) ? null : reader.GetString(0),
                                textMessage = reader.IsDBNull(1) ? null : reader.GetString(1),
                                idSender = reader.IsDBNull(2) ? null : reader.GetString(2),
                                isIncoming = isIncomingTemp,
                                idSenderText = reader.IsDBNull(3) ? null : reader.GetString(3),
                                idRecipient = reader.IsDBNull(4) ? null : reader.GetString(4),
                                idRecipientText = reader.IsDBNull(5) ? null : reader.GetString(5),
                                dateTime = reader.IsDBNull(6) ? null : reader.GetString(6),
                                typeMessage = typeMessage
                            });
                        }
                    }
                }
                connection.Close();
                return messages;
            }
            catch (Exception ex)
            {
                connection.Close();
                MessageBox.Show("Ошибка чтения БД GetMessageHistory-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite + ". Трассировка стека: " + ex.StackTrace, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                LogError("Ошибка чтения БД GetMessageHistory-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                         ". Трассировка стека: " + ex.StackTrace);
                return messages;
            }
        }
        /// <summary>
        /// Регистрация пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        public void RegistrationUserOnline(User user)
        {
            try
            {
                connection.Open();
                string sql = $"INSERT INTO table_users_online (users_id, date_time) " +
                             $"VALUES ('{user.id}','{DateTime.Now:s}');";
                var command = new MySqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                connection.Close();
                MessageBox.Show("Ошибка чтения БД RegistrationUserOnline-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                                ". Трассировка стека: " + ex.StackTrace, "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                LogError("Ошибка чтения БД RegistrationUserOnline-Исключение: " + ex.Message + ". Метод: " + ex.TargetSite +
                         ". Трассировка стека: " + ex.StackTrace);
            }
        }
    }
}