using System.Collections.ObjectModel;

namespace ModelData.Setting
{
    /// <summary>
    /// Интерфейс подключения
    /// </summary>
    public interface IDBConnect
    {
        User CheckUserRegistration(User user);
        bool RegistrationUser(User user);
        ObservableCollection <UserOnline> GetListUsersOnline();
        ObservableCollection <Message> GetMessageHistory(string idUser, string idAdmin);
    }
}