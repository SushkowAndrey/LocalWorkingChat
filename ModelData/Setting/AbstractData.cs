using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModelData.Setting
{
    /// <summary>
    /// Изменение коллекции
    /// </summary>
    public class AbstractData : INotifyPropertyChanged
    {
        /// <summary>
        /// Событие изменения
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Метод изменения свойства
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Установить значение
        /// </summary>
        /// <param name="field">Поле</param>
        /// <param name="value">Значение</param>
        /// <param name="propertyName">Имя свойства</param>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <returns></returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}