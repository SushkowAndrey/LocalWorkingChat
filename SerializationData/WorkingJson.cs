using System;
using System.Text.Json;

namespace SerializationData
{
    /// <summary>
    /// Класс работы с файлами Json
    /// </summary>
    public class WorkingJson
    {
        /// <summary>
        /// Метод класса - сериализация данных по шаблону
        /// </summary>
        /// <param name="data">Передаваемый класс</param>
        /// <typeparam name="T">Шаблон параметра</typeparam>
        /// <returns>Строка в виде json</returns>
        public static string SerializationJson <T> (T data)
        {
            return JsonSerializer.Serialize(data);
        }
        /// <summary>
        /// Метод десереиализации
        /// </summary>
        /// <param name="json">Строка json</param>
        /// <typeparam name="T">Шаблон параметра</typeparam>
        /// <returns></returns>
        public static T DeserializationJson <T> (string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}