using System;
using System.Text.RegularExpressions;
using System.Windows;
using LocalWorkingChat.CommonModule;
using ModelData;
using ModelData.Setting;
using SerializationData;

namespace LocalWorkingChat
{
    /// <summary>
    /// Настройка пользователя
    /// </summary>
    public partial class SettingUser : Window
    {
        # region FEATURES
        /// <summary>
        /// Пользователь
        /// </summary>
        private User user;
        /// <summary>
        /// Тип настройки
        /// </summary>
        private TypeSettingUser typeSettingUser;
        /// <summary>
        /// Шаблон почты
        /// </summary>
        private string pattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                                 @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$";
        /// <summary>
        /// Общий модуль рабоыт с БД
        /// </summary>
        private DBConnect dbConnect;
        # endregion region
        # region CONSTRUCTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        public SettingUser(User user, TypeSettingUser typeSettingUser)
        {
            InitializeComponent();
            this.user = user;
            this.typeSettingUser = typeSettingUser;
            var connectionString = (string) Application.Current.Properties["connectionString"];
            dbConnect = new DBConnect(connectionString);
            Loaded += OnLoaded;
        }
        # endregion region
        # region FORM EVENTS
        /// <summary>
        /// События при загрузке окна
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            switch (typeSettingUser)
            {
                case TypeSettingUser.setEmail:
                    Grid_setEmail.Visibility = Visibility.Visible;
                    break;
                case TypeSettingUser.passwordRecovery:
                    Grid_passwordRecovery.Visibility = Visibility.Visible;
                    break;
                case TypeSettingUser.passwordChange:
                    Grid_passwordChange.Visibility = Visibility.Visible;
                    break;
            }
        }
        # endregion region
        # region FORM ELEMENTS EVENTS
        # endregion region
        # region FORM TABLE EVENTS
        # endregion region
        # region FORM BUTTON
        /// <summary>
        /// Сохранение почты
        /// </summary>
        private void Button_saveEmailUser_OnClick(object sender, RoutedEventArgs e)
        {
            var email = TextBox_email_setEmail.Text;
            var formatEmail = Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Проверьте наличие данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (!formatEmail)
            {
                MessageBox.Show("Некорректный email. Повторите ввод", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var resSetEmailUser = dbConnect.SetEmailUser(user, email);
                if (resSetEmailUser.res)
                {
                    MessageBox.Show("Электронная почта добавлена", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                else
                {
                    MessageBox.Show(resSetEmailUser.error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        /// <summary>
        /// Шаг 1 восстановления пароля
        /// </summary>
        private void Button_further_passwordRecovery_OnClick(object sender, RoutedEventArgs e)
        {
            var email = TextBox_email_passwordRecovery.Text;
            var formatEmail = Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Проверьте наличие данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (!formatEmail)
            {
                MessageBox.Show("Некорректный email. Повторите ввод", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var result = MessageBox.Show($"Сформировать и направить код для восстановления пароля на электронную почту {email}?", 
                    "Восстановление пароля", MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Random rnd = new Random();
                    int codeEmail = rnd.Next(1000, 9999);
                    PasswordRecoveryEmail passwordRecoveryEmail = new PasswordRecoveryEmail(codeEmail);
                    var sendEmailCode = passwordRecoveryEmail.SendCode(email);
                    sendEmailCode.Wait();
                    //TODO
                    
                }
            }
        }
        /// <summary>
        /// Смена пароля
        /// </summary>
        private void Button_passwordChange_OnClick(object sender, RoutedEventArgs e)
        {
            string newPassword = PasswordBox_newPassword.Password.Trim();
            string newRepeatPassword = PasswordBox_newRepeatPassword.Password.Trim();
            if (string.IsNullOrEmpty(newPassword)||string.IsNullOrEmpty(newRepeatPassword))
            {
                MessageBox.Show("Проверьте наличие данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (newPassword!=newRepeatPassword)
            {
                MessageBox.Show("Введенные пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var resChangePassword = dbConnect.ChangePassword(user, newPassword);
                if (resChangePassword.res)
                {
                    MessageBox.Show("Пароль успешно изменен", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                else
                {
                    MessageBox.Show(resChangePassword.error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        /// <summary>
        /// Отмена
        /// </summary>
        private void Button_cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
        # endregion region
        # region FORM METHODS
        # endregion region
    }
}