using System;
using ModelData.Setting;

namespace ModelData
{
    /// <summary>
    /// Модель данных пользователя
    /// </summary>
    public class User : AbstractData
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public User() { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public User(string id,string nameUser)
        {
            this.id = id;
            this.nameUser = nameUser;
        }
        /// <summary>
        /// Конструктор копирования
        /// </summary>
        /// <param name="user">Исходный объект</param>
        public User(User user) {
            id = user.id;
            nameUser = user.nameUser;
            password = user.password;
            isActive = user.isActive;
            isAdmin = user.isAdmin;
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
        /// Имя пользователя
        /// </summary>
        private string _nameUser;
        public string nameUser { 
            get => _nameUser; 
            set => SetField(ref _nameUser, value); 
        }
        /// <summary>
        /// Пароль
        /// </summary>
        private string _password;
        public string password { 
            get => _password; 
            set => SetField(ref _password, value); 
        }
        /// <summary>
        /// Активность
        /// </summary>
        private bool _isActive;
        public bool isActive { 
            get => _isActive; 
            set => SetField(ref _isActive, value); 
        }
        /// <summary>
        /// Админ
        /// </summary>
        private bool _isAdmin;
        public bool isAdmin { 
            get => _isAdmin; 
            set => SetField(ref _isAdmin, value); 
        }
        /// <summary>
        /// Электронная почта
        /// </summary>
        private string _mail;
        public string mail { 
            get => _mail; 
            set => SetField(ref _mail, value); 
        }
    }
    /// <summary>
    /// Пользователь онлайн
    /// </summary>
    public class UserOnline : User
    {
        /// <summary>
        /// Дата/время в сети
        /// </summary>
        private string _dateTimeOnline;
        public string dateTimeOnline { 
            get => _dateTimeOnline; 
            set => SetField(ref _dateTimeOnline, value); 
        }
    }
}