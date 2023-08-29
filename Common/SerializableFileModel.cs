using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Stalker_Studio.Common
{
    /// <summary>
    /// Состояния сериализации (потребности связанные с сериализацией)
    /// </summary>
    public enum SerializationState 
    {
        None,
        NeedSerialize,
        NeedDeserialize
    }

    /// <summary>
    /// Класс работы с сериализуемым файлом
    /// (можно использовать, например, для текстового файла с внутренней стркутрой, допустим, XML)
    /// Присутствует поддержка отложенной сериализации для оптимизации
    /// Отложенная сериализация - это выполнение сериализации или десериализации только тогда, 
    /// когда понадобятся данные(например, метод get у свойств), которые зависят от сериализации или десериализации.
    /// </summary>
    public abstract class SerializableFileModel : FileModel
    {
        SerializationState _serializationState = SerializationState.None;
        bool _autoSerialization = false;
        bool _isUpdatingSerialization = false;

        public SerializableFileModel() : base() { }
        public SerializableFileModel(FileInfo file) : base(file) { }
        public SerializableFileModel(string fullName) : base(fullName) { }

        /// <summary>
        /// Текущее состояние сериализации, при автосериализации присвоение вызывает OnSetSerializationState()
        /// </summary>
        protected virtual SerializationState SerializationState { 
            get { return _serializationState; } 
            set {
                if (_serializationState != value)
                {
                    _serializationState = value;
                    if (_autoSerialization)
                        OnSetSerializationState();
                }
            } 
        }
        /// <summary>
        /// Режим автоматического управления сериализацией, по умолчанию false
        /// Отложенная сериализация работает только в режиме автоматического управления сериализацией
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public bool AutoSerialization
        {
            get { return _autoSerialization; }
            set {
                if (value != _autoSerialization)
                {
                    _autoSerialization = value;
                    _serializationState = SerializationState.None; // сброс состояния
                }
            }
        }
        /// <summary>
        /// Выполняется ли сериализация\десериализация механизмом обновления (UpdateSerialization()) в данный момент
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public bool IsUpdatingSerialization { get => _isUpdatingSerialization;  }

        #region Автоматическое управление сериализацией (сокр. автосериализация)

        /// <summary>
        /// Переопределяемая функция. Вызывается после установки состояния сериализации
        /// </summary>
        protected abstract void OnSetSerializationState();
        /// <summary>
        /// Переопределяемая функция. Вызывается после сериализации в режиме автосериализации
        /// По сути обратная функция GetSerializedString()
        /// <param name="serializedString">Строка, полученная в результате сериализации</paramref>
        /// </summary>
        protected abstract void OnAutoSerialized(string serializedString);
        /// <summary>
        /// Переопределяемая функция. Вызывается при автоматическом вызове десериализации(UpdateSerialization) для получения сериализованной строки
        /// По сути обратная функция OnAutoSerialized(string serializedString)
        /// </summary>
        protected abstract string GetSerializedString();
        /// <summary>
        /// Проверяет нужна ли сериализация или десериализация и выполняет их, если не установлен флаг IsUpdatingSerialization
        /// </summary>
        public void UpdateSerialization()
        {
            if (_isUpdatingSerialization)
                return;

            _isUpdatingSerialization = true;

            if (_autoSerialization)
            {
                if (_serializationState == SerializationState.NeedSerialize)
                    Serialize();
                else if (_serializationState == SerializationState.NeedDeserialize)
                    Deserialize(GetSerializedString());
            }
            _serializationState = SerializationState.None;

            _isUpdatingSerialization = false;
        }

        #endregion 

        /// <summary>
        /// Переопределяемая функция конвертации в строку
        /// </summary>
        protected abstract string ConvertToString();
        /// <summary>
        /// Основная функция сериализации
        /// </summary>
        public string Serialize()
        {
            string result = ConvertToString();

            if (_autoSerialization)
                OnAutoSerialized(result);

            _serializationState = SerializationState.None;

            return result;
        }


        /// <summary>
        /// Основная функция конвертации из строки
        /// </summary>
        /// <param name="serializedString">Строка, из которой конвертировать</param>
        public abstract void Deserialize(string serializedString);
    }
}
