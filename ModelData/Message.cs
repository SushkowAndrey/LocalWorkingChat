using System;
using System.Globalization;
using ModelData.Setting;

namespace ModelData
{
    /// <summary>
    /// Модель данных сообщения
    /// </summary>
    public class Message : AbstractData
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public Message() { }
        /// <summary>
        /// Конструктор с параметром
        /// </summary>
        public Message(User sender,User recipient,string textMessage, TypeMessage typeMessage)
        {
            Guid guid = Guid.NewGuid();
            id = guid.ToString();
            dateTime = DateTime.Now.ToString("dd.MM.yyyy-HH:mm");
            idSender = sender.id;
            idSenderText = sender.nameUser;
            if (recipient != null)
            {
                idRecipient = recipient.id; 
                idRecipientText = recipient.nameUser;
            }
            this.textMessage = textMessage;
            this.typeMessage = typeMessage;
        }
        /// <summary>
        /// Ссылка
        /// </summary>
        private string _id;
        public string id { 
            get => _id; 
            set => SetField(ref _id, value); 
        }
        /// <summary>
        /// Сообщение
        /// </summary>
        private string _textMessage;
        public string textMessage { 
            get => _textMessage; 
            set => SetField(ref _textMessage, value); 
        }
        /// <summary>
        /// Отправитель
        /// </summary>
        private string _idSender;
        public string idSender { 
            get => _idSender; 
            set => SetField(ref _idSender, value); 
        }
        /// <summary>
        /// Отправитель-представление
        /// </summary>
        private string _idSenderText;
        public string idSenderText { 
            get => _idSenderText; 
            set => SetField(ref _idSenderText, value); 
        }
        /// <summary>
        /// Получатель-представление
        /// </summary>
        private string _idRecipient;
        public string idRecipient { 
            get => _idRecipient; 
            set => SetField(ref _idRecipient, value); 
        }
        /// <summary>
        /// Получатель
        /// </summary>
        private string _idRecipientText;
        public string idRecipientText { 
            get => _idRecipientText; 
            set => SetField(ref _idRecipientText, value); 
        }
        /// <summary>
        /// Дата сообщения
        /// </summary>
        private string _dateTime;
        public string dateTime { 
            get => _dateTime; 
            set => SetField(ref _dateTime, value); 
        }
        /// <summary>
        /// Это входящее сообщение
        /// </summary>
        private bool _isIncoming;
        public bool isIncoming { 
            get => _isIncoming; 
            set => SetField(ref _isIncoming, value); 
        }
        /// <summary>
        /// Тип сообщения
        /// </summary>
        private TypeMessage _typeMessage;
        public TypeMessage typeMessage { 
            get => _typeMessage; 
            set => SetField(ref _typeMessage, value); 
        }
    }
}