using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

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
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
            return JsonSerializer.Serialize(data,options);
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