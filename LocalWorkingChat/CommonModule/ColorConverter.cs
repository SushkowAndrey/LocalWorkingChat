using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LocalWorkingChat.CommonModule
{
    /// <summary>
    /// Выделение строк цветом
    /// </summary>
    public class ColorConverter : IValueConverter
    {
        /// <summary>
        /// Конвертация цвета
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null && (bool)value)
            {
                return new SolidColorBrush(Colors.Aquamarine);
            }
            if (value != null && !(bool)value)
            {
                return new SolidColorBrush(Colors.Bisque);
            }
            return new SolidColorBrush(Colors.Azure);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}