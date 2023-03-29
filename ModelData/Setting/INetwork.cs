using System;
using System.Net.Sockets;

namespace ModelData.Setting
{
    /// <summary>
    /// Интерфейс работы с сетью
    /// </summary>
    public interface INetwork
    {
        void Connect(User user, NetworkStream stream);
        void SendMessage<T>(T message, NetworkStream stream);
        void ReceiveMessage(NetworkStream stream, Action getListUsers, Action <Message> popupNotifierMessage, Action <Message,bool> updatePanelMessage);
        void Disconnect(TcpClient client, NetworkStream stream);
    }
}