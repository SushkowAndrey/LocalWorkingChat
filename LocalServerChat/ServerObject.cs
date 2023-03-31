using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ModelData;
using static LocalServerChat.Setting;
using static SerializationData.WorkingJson;

namespace LocalServerChat
{
    /// <summary>
    /// Работа сервера
    /// </summary>
    public class ServerObject
    {
        # region FEATURES
        /// <summary>
        /// Строка подключения
        /// </summary>
        static string connectionString;
        /// <summary>
        /// Свойство класса - порт сервера
        /// </summary>
        private const int port = 8008;
        /// <summary>
        /// Свойство класса - сервер для прослушивания
        /// </summary>
        static TcpListener tcpListener;
        /// <summary>
        /// Свойство класса - все подключения клиентов
        /// </summary>
        List <ClientObject> clients; 
        # endregion region
        # region PROGRAM INTERFACE
        /// <summary>
        /// Метод класса - прослушивание входящих подключений
        /// </summary>
        public void ListenConnect(object? admin)
        {
            clients = new List<ClientObject>();
            connectionString = GetConnectionString();
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                ConsoleSystem($"{DateTime.Now:u}-Ожидание подключений...");
                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(tcpClient, this,connectionString);
                    clients.Add(clientObject);
                    Thread clientThread = new Thread(clientObject.ProcessClient);
                    clientThread.Start(admin);
                    
                    ConsoleInfo($"{DateTime.Now:u}-Создание потока для клиента-{clientThread.GetHashCode()}");
                    ShowListUser($"Подключение клиента {clientObject.user.nameUser}");
                }
            }
            catch (Exception ex)
            {
                ConsoleWarning($"{DateTime.Now:u}-Ошибка ListenConnect-{ex.Message}");
            }
        }
        # endregion region
        # region FORM METHODS
        /// <summary>
        /// Метод класса - трансляция сообщения подключенным клиентам-общая рассылка
        /// </summary>
        public void SendMessage(Message message)
        {
            string sengMessage = SerializationJson(message);
            byte[] data = Encoding.Unicode.GetBytes(sengMessage);
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Stream.Write(data, 0, data.Length); //передача данных всем
            }
        }
        /// <summary>
        /// Метод класса - трансляция сообщения подключенным клиентам-персональная рассылка
        /// </summary>
        public void BroadcastMessage(Message message, string recipientId)
        {
            string sengMessage = SerializationJson(message);
            byte[] data = Encoding.Unicode.GetBytes(sengMessage);
            var client = clients.First(s => s.user.id == recipientId);
            client.Stream.Write(data, 0, data.Length);
        }
        /// <summary>
        /// Метод класса - удаление соединения при отключении из списка
        /// </summary>
        public void RemoveConnection(User user)
        {
            ClientObject client = clients.FirstOrDefault(c => c.user.id == user.id);
            if (client != null)
                clients.Remove(client);
            ShowListUser($"Отключение клиента {user.nameUser}");
        }
        /// <summary>
        /// Отображение списка пользователей при изменении
        /// </summary>
        private void ShowListUser(string currentProccess)
        {
            ConsoleInfo($"{DateTime.Now:u}-Обновлен список пользователей (операция-{currentProccess})");
            foreach (var client in clients)
            {
                Console.WriteLine($"Клиент онлайн-{client.user.nameUser}");
            }
        }
        /// <summary>
        /// Метод класса - отключение всех клиентов
        /// </summary>
        public void Disconnect()
        {
            tcpListener.Stop();
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close();
            }
            Environment.Exit(0); 
        }
        # endregion region
    }
}