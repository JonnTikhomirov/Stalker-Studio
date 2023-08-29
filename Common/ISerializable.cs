using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stalker_Studio.Common
{
    /// <summary>
    /// Интерфейс сериализуемого объекта
    /// </summary>
    public interface ISerializable
    {
        /// <summary>
        /// Конвертация объекта в строку
        /// </summary>
        string Serialize();
        /// <summary>
        /// Конвертация объекта в строку, которая записывается в StringBuilder
        /// </summary>
        void Serialize(StringBuilder stringBuilder);
        /// <summary>
        /// Конвертация объекта из строки text
        /// </summary>
        void Deserialize(string text);
        /// <summary>
        /// Конвертация объекта из строки, которая последовательно по символьно считывается из TextReader
        /// </summary>
        void Deserialize(ReadingContext reader);
        /// <summary>
        /// Функция обработки исключения при сериализации\десериализации. Замена try catch
        /// </summary>
        /// <returns>true - если необходимо прервать операцию, false - если можно продолжить</returns>
        bool ExceptionPush(SerializationException exception);

        void Update(ISerializable source);
    }
}
