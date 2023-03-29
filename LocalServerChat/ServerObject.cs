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
                tcpListener = new TcpListener(IPAddress.Any, port);//прослушивание всех входящих подключений для порта
                tcpListener.Start();//запуск прослушивания
                ConsoleSystem($"{DateTime.Now:u}-Ожидание подключений...");
                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(tcpClient, this,connectionString);//создание экземпляра клиента
                    clients.Add(clientObject);
                    Thread clientThread = new Thread(clientObject.ProcessClient);//создание потока для клиента
                    clientThread.Start(admin);//запуск клиента
                    
                    Console.WriteLine($"{DateTime.Now:u}-Создание потока для клиента-{clientThread.GetHashCode()}");
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
        //TODO
        /// <summary>
        /// Метод класса - трансляция сообщения подключенным клиентам-персональная рассылка
        /// </summary>
        public void BroadcastMessage(Message message, string recipientId)
        {
            string sengMessage = SerializationJson(message);
            byte[] data = Encoding.Unicode.GetBytes(sengMessage);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].user.id == recipientId) 
                {
                    clients[i].Stream.Write(data, 0, data.Length);
                    break;
                }
            }
        }
        /// <summary>
        /// Метод класса - удаление соединения при отключении из списка
        /// </summary>
        public void RemoveConnection(User user)
        {
            // получаем по id закрытое подключение
            ClientObject client = clients.FirstOrDefault(c => c.user.id == user.id);
            // и удаляем его из списка подключений
            if (client != null)
                clients.Remove(client);
            ShowListUser($"Отключение клиента {user.nameUser}");
        }
        /// <summary>
        /// Отображение списка пользователей при изменении
        /// </summary>
        private void ShowListUser(string currentProccess)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{DateTime.Now:u}-Обновлен список пользователей (операция-{currentProccess})");
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
            tcpListener.Stop(); //остановка сервера
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
        }
        # endregion region
    }
}