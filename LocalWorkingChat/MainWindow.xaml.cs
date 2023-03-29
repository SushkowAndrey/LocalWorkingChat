using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using LocalWorkingChat.CommonModule;
using Microsoft.Win32;
using ModelData;
using SerializationData;
using Tulpep.NotificationWindow;
using static SerializationData.WorkingJson;
using static LocalWorkingChat.CommonModule.SettingParameters;
using Color = System.Drawing.Color;
using static CryptoProtect.Encryption;
using static CryptoProtect.HashFunction;
using static Logging.Logging;
using static LocalServerChat.Setting;
using static LocalServerChat.DBServer.DBMessage;

namespace LocalWorkingChat
{
    /// <summary>
    /// Основное окно пользователя
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Сообщения
        /// </summary>
        public ObservableCollection <Message> messages { get; set; }
        /// <summary>
        /// Свойство класса - ip адрес сервера
        /// </summary>
        private const string ipAddress = "localhost";
        /// <summary>
        /// Свойство класса - порт сервера
        /// </summary>
        private const int port = 8008;
        /// <summary>
        /// Строка подключения
        /// </summary>
        static string connectionString;
        /// <summary>
        /// Свойство класса - класс для создания клиентской программы, работающей по протоколу TCP
        /// </summary>
        private static TcpClient client;
        /// <summary>
        /// Свойство класса - взаимодействие с сервером-через данный объект можно передавать сообщения серверу или, наоборот, получать данные с сервера
        /// </summary>
        private static NetworkStream stream;
        /// <summary>
        /// Свойство класса - модель данных пользователя
        /// </summary>
        private User user;
        /// <summary>
        /// Свойство класса - модель данных пользователя
        /// </summary>
        private User admin;
        /// <summary>
        /// Свойство класса - сообщение
        /// </summary>
        private Message message;
        /// <summary>
        /// Класс подключения к БД
        /// </summary>
        private DBConnectClient dbConnectClient;
        /// <summary>
        /// Класс подключения к БД-прикрепленные файлы
        /// </summary>
        private DBAttachmentFile dbAttachmentFile;
        /// <summary>
        /// Общий модуль рабоыт с БД
        /// </summary>
        private DBConnect dbConnect;
        /// <summary>
        /// Класс работы с сетью
        /// </summary>
        private NetworkWorking networkWorking;
        /// <summary>
        /// Буфер для хранения прикрепленного
        /// </summary>
        private byte[] bufferAttachmentFile;
        /// <summary>
        /// Конструктор класса инициализации окна
        /// </summary>
        public MainWindow()
        {
            messages = new ObservableCollection<Message>();
            InitializeComponent();
            InitializeData();
            Loaded += OnLoaded;
            Closing += OnClosing;
        }
        # region FORM EVENTS
        /// <summary>
        /// События при загрузке окна
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            GetUserData();
            admin = dbConnect.GetAdmin();
            Button_send.IsEnabled = false;
            TextBox_message.IsEnabled = false;
            bufferAttachmentFile = null;
        }
        /// <summary>
        /// События при выходе
        /// </summary>
        private void OnClosing(object sender, CancelEventArgs e)
        {
            networkWorking.Disconnect(client, stream);
        }
        /// <summary>
        /// Отправка сообщения при нажатии на кнопку Enter
        /// </summary>
        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter&&Button_send.IsEnabled)
            {
                SendMessages();
            }
        }
        # endregion region
        # region FORM ELEMENTS EVENTS
        /// <summary>
        /// Отправка сообщения при нажатии на кнопку Enter
        /// </summary>
        private void TextBox_message_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                SendMessages();
            }
        }
        /// <summary>
        /// Выбор получателя
        /// </summary>
        private void DataGrid_usersTable_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var selectedUser = (UserOnline)DataGrid_usersTable.SelectedItem;
            if (selectedUser == null)
            {
                DataGrid_messages.ItemsSource = null;
                return; 
            }
            IEnumerable <Message> temp;
            if (selectedUser.id == admin.id)
            {
                temp = from element in messages
                    where element.idSender == admin.id || element.idRecipient == admin.id
                    select element; 
            }
            else
            {
                temp = from element in messages
                    where (element.idSender == user.id && element.idRecipient == selectedUser.id) ||
                          (element.idSender == selectedUser.id && element.idRecipient == user.id) 
                    select element; 
            }
            DataGrid_messages.ItemsSource = null;
            if (temp != null) 
                DataGrid_messages.ItemsSource = new ObservableCollection<Message>(temp);
            DataGrid_messages.ScrollIntoView(DataGrid_messages.Items[^1]);
            
            TextBlock_currentRecepient.Text = $"Получатель-{selectedUser.nameUser}";
        }
        /// <summary>
        /// Скачать прикрепленный файл
        /// </summary>
        private void DataGrid_messages_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedMessage = (Message)DataGrid_messages.SelectedItem;
            if (selectedMessage == null) 
                return; 
            if (selectedMessage.typeMessage != TypeMessage.attachmentFile) 
                return; 
            byte[] getAttachmentFile = dbAttachmentFile.GetAttachmentFile(selectedMessage);
            if (getAttachmentFile== null) 
                return;
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.FileName = selectedMessage.textMessage;
            if (savefile.ShowDialog() == true)
            {
                string path = savefile.FileName;
                FileInfo fi = new FileInfo(path);
                using (FileStream fs = fi.OpenWrite()) {
                    fs.Write(getAttachmentFile, 0, getAttachmentFile.Length);
                }
            }
        }
        # endregion region
        # region FORM BUTTON
        /// <summary>
        /// Кнопка регистрации пользователя в БД
        /// </summary>
        private void Button_registration_OnClick(object sender, RoutedEventArgs e)
        {
            user.nameUser = TextBox_nameUser.Text;
            user.password = PasswordBox_passwordUser.Password;
            if (user.nameUser==String.Empty||user.password==String.Empty)
            {
                MessageBox.Show("Проверьте наличие данных", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if(string.IsNullOrEmpty(dbConnectClient.CheckUserRegistration(user).id))
            {
                Guid guid = Guid.NewGuid();
                user.id = guid.ToString();
                user.isActive = true;
                user.nameUser = TextBox_nameUser.Text;
                user.password = PasswordBox_passwordUser.Password;
                var resultRegistrationUserDb = dbConnectClient.RegistrationUser(user);
                if (resultRegistrationUserDb)
                {
                    MessageBox.Show("Пользователь зарегистрирован", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    WritingFileUserData(user); 
                }
            }
            else
            {
                MessageBox.Show("Пользователь уже зарегистрирован", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                WritingFileUserData(user);
            }
        }
        /// <summary>
        /// Кнопка авторизации пользователя на сервере онлайн
        /// </summary>
        private void Button_autorization_OnClick(object sender, RoutedEventArgs e)
        {
            user.nameUser = TextBox_nameUser.Text;
            user.password = PasswordBox_passwordUser.Password;
            if (user.nameUser==String.Empty||user.password==String.Empty)
            {
                MessageBox.Show("Проверьте наличие данных", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if(dbConnectClient.CheckUserRegistration(user).id == null)
            {
                MessageBox.Show("Проверьте правильность имени и пароля пользователя", 
                    "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Application.Current.Properties["user"] = user;
            ImageGif_loadinListUsers.Visibility = Visibility.Visible;
            ImageGif_loadinListMessages.Visibility = Visibility.Visible;
            IsEnabled = false;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += worker_DoWorkStartClient; 
            worker.RunWorkerCompleted += worker_RunWorkerCompletedStartClient; 
            worker.RunWorkerAsync();
        }
        /// <summary>
        /// Выполняемый метод-старт клиента
        /// </summary>
        void worker_DoWorkStartClient(object sender, DoWorkEventArgs e)
        {
            client = new TcpClient();
            try
            {
                client.Connect(ipAddress, port); //подключение клиента к ip и порту-в данном случае к серверу
                stream = client.GetStream(); // получаем поток
                networkWorking.Connect(user, stream);
                dbConnectClient.RegistrationUserOnline(user);
                Thread receiveThread = new Thread(() =>
                {
                    networkWorking.ReceiveMessage(stream, GetListUsers, PopupNotifierMessage, UpdatePanelMessage);
                });
                receiveThread.Start(); 
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() 
                    {
                        GetListUsers(); 
                        GetMessageHistory();
                    }
                    );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка подключения к серверу", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Действия после загрузки-завершение старта клиента
        /// </summary>
        private void worker_RunWorkerCompletedStartClient(object sender, RunWorkerCompletedEventArgs e)
        {
            SetVisibilityAccessibility(true);
            ImageGif_loadinListUsers.Visibility = Visibility.Collapsed;
            ImageGif_loadinListMessages.Visibility = Visibility.Collapsed;
            System.Timers.Timer timerGetListUsers = new System.Timers.Timer();
            timerGetListUsers.Elapsed += FillingListUsers;
            timerGetListUsers.Interval = 5000;
            IsEnabled = true;
        }
        /// <summary>
        /// Обновление списка пользователей
        /// </summary>
        private void FillingListUsers(object source, ElapsedEventArgs e)
        {
            var workerGetListUsers = new BackgroundWorker();
            workerGetListUsers.DoWork += worker_DoWorkGetListUsers;
            workerGetListUsers.RunWorkerAsync();
        }
        /// <summary>
        /// Выполняемый метод
        /// </summary>
        private void worker_DoWorkGetListUsers(object sender, DoWorkEventArgs e)
        {
            GetListUsers();
        }
        /// <summary>
        /// Кнопка отправки сообщения в общий чат через сервер
        /// </summary>
        private void Button_send_OnClick(object sender, RoutedEventArgs e)
        {
            SendMessages();
        }
        /// <summary>
        /// Добавить прикрепленный файл
        /// </summary>
        private void Button_attachmentFile_OnClick(object sender, RoutedEventArgs e)
        {
            TextBlock_warning.Text = String.Empty;
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                try
                {
                    if(ValidFile(dialog.FileName, 1048576))
                    {
                        bufferAttachmentFile = File.ReadAllBytes(dialog.FileName);
                        TextBox_message.Text = $"Прикрепленный файл-{dialog.SafeFileName}";
                        TextBox_message.IsEnabled = false;
                        Button_attachmentFile.IsEnabled = false;
                    }
                    else
                    {
                        MessageBox.Show("Превышен размер файла. Максимальный размер - 1Мб", "Ошибка", MessageBoxButton.OK,MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка чтения файла-Исключение: "+ex.Message+". Метод: "+
                                    ex.TargetSite+". Трассировка стека: "+ex.StackTrace, "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
        /// <summary>
        /// Удалить прикрепленный файл
        /// </summary>
        private void Button_deleteFile_OnClick(object sender, RoutedEventArgs e)
        {
            if (bufferAttachmentFile == null)
            {
                TextBlock_warning.Text = "Прикрепленный файл отсутствует";
                return;
            }
            bufferAttachmentFile = null;
            TextBox_message.Text = String.Empty;
            Button_attachmentFile.IsEnabled = true;
            TextBox_message.IsEnabled = true;
        }
        # endregion region
        # region FORM METHODS
        /// <summary>
        /// Инициализация данных
        /// </summary>
        private void InitializeData()
        {
            connectionString = GetConnectionString();
            user = new User();
            dbConnectClient = new DBConnectClient(connectionString);
            dbAttachmentFile = new DBAttachmentFile(connectionString);
            dbConnect = new DBConnect(connectionString);
            networkWorking = new NetworkWorking();
        }
        /// <summary>
        /// Получение имени пользователя и пароля из файла при загрузке программы
        /// </summary>
        private void GetUserData()
        {
            try
            {
                user = ImportUserData("User.json");
                TextBox_nameUser.Text = Decrypt(user.nameUser);
                PasswordBox_passwordUser.Password = Decrypt(user.password);
                user.id = dbConnectClient.GetUserId(TextBox_nameUser.Text,PasswordBox_passwordUser.Password);
            }
            catch (Exception ex)
            {
                LogError($"Ошибка GetUserData-{ex.Message}");
            }
        }
        /// <summary>
        /// Получение списка пользователей из БД в режиме онлайн
        /// </summary>
        private void GetListUsers()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() 
                { 
                    DataGrid_usersTable.ItemsSource = null; 
                    DataGrid_usersTable.ItemsSource = dbConnectClient.GetListUsersOnline();
                }
                );
        }
        /// <summary>
        /// Получение полной истории
        /// </summary>
        private void GetMessageHistory()
        {
            var temp = dbConnectClient.GetMessageHistory(user.id, admin.id);
            foreach (var element in temp)
            {
                messages.Add(element);
            }
            DataGrid_messages.ScrollIntoView(messages[messages.Count-1]);
            DataGrid_usersTable_OnSelectedCellsChanged(null, null);
        }
        /// <summary>
        /// Установить видимость доступность
        /// </summary>
        private void SetVisibilityAccessibility(bool resAutorization)
        {
            TextBox_nameUser.IsEnabled = !resAutorization;
            PasswordBox_passwordUser.IsEnabled = !resAutorization;
            Button_registration.IsEnabled = !resAutorization;
            Button_autorization.IsEnabled = !resAutorization;
            Button_send.IsEnabled = resAutorization;
            TextBox_message.IsEnabled = resAutorization; 
        }
        /// <summary>
        /// Метод отправки сообщений
        /// </summary>
        private void SendMessages()
        {
            var selectedUserRecipient = (UserOnline)DataGrid_usersTable.SelectedItem;
            if (selectedUserRecipient == null)
            {
                TextBlock_warning.Text = "Выберите получателя";
                return; 
            }
            if (TextBox_message.Text == String.Empty)
            {
                TextBlock_warning.Text = "Введите сообщение";
                return; 
            }
            message = new Message(user,selectedUserRecipient,TextBox_message.Text, 
                bufferAttachmentFile == null? TypeMessage.messages:TypeMessage.attachmentFile);
            UpdatePanelMessage(message);
            
            networkWorking.SendMessage(message,stream);
            RegistrationMessagesAsync(message,connectionString,bufferAttachmentFile);
            TextBox_message.Clear();
            TextBlock_warning.Text = String.Empty;
        }
        /// <summary>
        /// Обновление панели сообщений
        /// </summary>
        /// <param name="getMessage">Новое сообщений</param>
        void PopupNotifierMessage(Message getMessage)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() 
                { 
                    PopupNotifier popup = new PopupNotifier(); 
                    popup.TitleText = $"Новое уведомление от пользователя {getMessage.idSenderText}"; 
                    popup.ContentText = getMessage.textMessage; 
                    popup.HeaderColor = Color.Azure; 
                    popup.TitleColor = Color.Green; 
                    popup.BodyColor = Color.Aqua; 
                    popup.Popup();
                }
                );
        }

        /// <summary>
        /// Добавление строки сообщения
        /// </summary>
        /// <param name="getMessage">Новое сообщений</param>
        /// <param name="addMessage">Признак добавления сообщения</param>
        private void UpdatePanelMessage(Message getMessage, bool addMessage = true)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() 
                { 
                    if(addMessage) 
                        messages.Add(getMessage); 
                    DataGrid_messages.ScrollIntoView(messages[^1]);
                    DataGrid_usersTable_OnSelectedCellsChanged(null, null);
                    Button_attachmentFile.IsEnabled = true;
                }
                );
        }
        /// <summary>
        /// Размер файла - проверка
        /// </summary>
        /// <param name="filename">Имя файла</param>
        /// <param name="limitInBytes">Максимальный размер</param>
        /// <returns></returns>
        private bool ValidFile(string filename, long limitInBytes)
        {
            var fileSizeInBytes = new FileInfo(filename).Length;
            if(fileSizeInBytes > limitInBytes) 
                return false;
            return true;
        }
        # endregion region
    }
}