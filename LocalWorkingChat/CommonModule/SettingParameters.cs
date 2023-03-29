using System.IO;
using System.Text.Json;
using ModelData;
using static CryptoProtect.Encryption;

namespace LocalWorkingChat.CommonModule
{
    /// <summary>
    /// 
    /// </summary>
    public static class SettingParameters
    {
        /// <summary>
        /// Метод получения данных пользователя из файла json
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <returns>Получаем класс с текущими данными</returns>
        public static User ImportUserData(string path)
        {
            var user = new User();
            try
            {
                using (var file = new FileStream(path, FileMode.Open))
                {
                    user = JsonSerializer.DeserializeAsync<User>(file).Result;
                }
                return user;
            }
            catch
            {
                return user;
            }
        }
        /// <summary>
        /// Метод записи/перезаписи параметров в файл
        /// </summary>
        /// <param name="user">Передаем класс с данными пользователя для записи</param>
        public static void WritingFileUserData(User user)
        {
            user.nameUser = Encrypt(user.nameUser);
            user.password = Encrypt(user.password);
            using (StreamWriter file = new StreamWriter("User.json", false))
            {
                string json = JsonSerializer.Serialize(user);
                file.WriteLine(json);
            }
        }
    }
}