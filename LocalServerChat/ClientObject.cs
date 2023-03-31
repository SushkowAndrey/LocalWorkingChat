using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using LocalServerChat.DBServer;
using ModelData;
using static LocalServerChat.DBServer.DBMessage;
using static SerializationData.WorkingJson;
using static LocalServerChat.Setting;

namespace LocalServerChat
{ 
    /// <summary>
    /// Класс работы с клиентом
    /// </summary>
    public class ClientObject
    {
        /// <summary>
        /// Свойство класса - модель данных пользователя-уникальный идентификатор клиента
        /// </summary>
        internal User user;
        /// <summary>
        /// Свойство класса - свойство Stream, хранящее поток для взаимодействия с клиентом
        /// </summary>
        internal NetworkStream Stream {get; set;}
        /// <summary>
        /// Свойство класса - создание клиента
        /// </summary>
        private TcpClient client;
        /// <summary>
        /// Свойство класса - объект сервера
        /// </summary>
        private ServerObject server;
        /// <summary>
        /// Свойство класса - подключение к БД
        /// </summary>
        private DBConnectServer dbConnectServer;
        /// <summary>
        /// Строка подключения
        /// </summary>
        private string connectionString;
        /// <summary>
        /// Свойство класса - сообщение
        /// </summary>
        private Message getSetMessage;
        /// <summary>
        /// Сериализованное сообщение из потока
        /// </summary>
        private string messageSerialization;
        # region PROGRAM INTERFACE
        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="client">Клиент</param>
        /// <param name="server">Объект сервера</param>
        /// <param name="connectionString">Строка подключения</param>
        public ClientObject(TcpClient client, ServerObject server, string connectionString)
        {
            dbConnectServer = new DBConnectServer(connectionString);
            this.client = client;
            this.server = server;
            this.connectionString = connectionString;
            Stream = client.GetStream();
            messageSerialization = GetMessage();
            user = new User();
            user = DeserializationJson <User> (messageSerialization);
        }
        /// <summary>
        /// Процесс работы сервера - прием и направление ответа на сообщения
        /// </summary>
        public void ProcessClient(object? adminData)
        {
            if (adminData is User admin)
            {
                try
                {
                    string message = $"{DateTime.Now:u}-{user.nameUser} вошел в чат";
                    getSetMessage = new Message(admin, null, message, TypeMessage.authorization);
                    RegistrationMessagesAsync(getSetMessage, connectionString);
                    server.SendMessage(getSetMessage);

                    ConsoleSystem(message);
                    while (true)
                    {
                        try
                        {
                            getSetMessage = DeserializationJson<Message>(GetMessage());
                            ConsoleInfo($"{getSetMessage.dateTime}-Отправитель-{getSetMessage.idSenderText}, получатель-{getSetMessage.idRecipientText}," +
                                        $" сообщение - {getSetMessage.textMessage}");
                            if (getSetMessage.idRecipient == admin.id)
                            {
                                server.SendMessage(getSetMessage);
                            }
                            else
                            {
                                server.BroadcastMessage(getSetMessage,getSetMessage.idRecipient);
                            }
                        }
                        catch
                        {
                            dbConnectServer.DeleteUserOnline(user.id);
                            message = $"{DateTime.Now:u}-{user.nameUser}: покинул чат";
                            ConsoleWarning(message);
                           
                            getSetMessage = new Message(admin, null, message, TypeMessage.exit);
                            RegistrationMessagesAsync(getSetMessage, connectionString);
                            server.SendMessage(getSetMessage);
                            server.RemoveConnection(user);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    ConsoleWarning($"{DateTime.Now:u}-Ошибка ProcessClient-{e.Message}");
                }
            }
        }
        # endregion region
        # region FORM METHODS
        /// <summary>
        /// Чтение входящего сообщения и преобразование в строку
        /// </summary>
        /// <returns>Получаем сообщение</returns>
        private string GetMessage()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);
            return builder.ToString();
        }
        /// <summary>
        /// Закрытие подключения
        /// </summary>
        public void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
        # endregion region
    }
}