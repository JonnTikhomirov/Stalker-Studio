using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Reflection.Emit;

namespace Stalker_Studio.Common
{
    /// <summary>
    /// Класс работы с текстовым файлом, описывающим объекты
    /// </summary>
    public abstract class ObjectTextFileModel : TextFileModel, ISerializable
    {
        protected ObservableCollection<TextNode> _nodes = null;
        protected IntervalTree.IntervalTree<int, TextNode> _offsetTree = new IntervalTree.IntervalTree<int, TextNode>();
        protected int _modifiedOffset = -1;

        public ObjectTextFileModel(Encoding encoding = null, string text = "") : base(encoding) { }
        public ObjectTextFileModel(FileInfo file, Encoding encoding = null) : base(file, encoding) { }
        public ObjectTextFileModel(string fullName, Encoding encoding = null) : base(fullName, encoding) { }

        /// <summary>
        /// Выполняет сериализацию, десериализацию и возвращает текст
        /// </summary>
        public override string Text { 
            get {
                if (!_isLoaded)
                {
                    StreamReader streamReader = new StreamReader(_fullName, _encoding);
                    string text = streamReader.ReadToEnd();
                    Deserialize(text);
                    return text;
                }
                return Serialize(); 
            } 
            set {
                Deserialize(value);
                OnPropertyChanged();
            } 
        }
        /// <summary>
        /// Последнее актуальное смещение, все смещения больше чем ModifiedOffset не актуальны.
        /// </summary>
        /// <returns>Последнее актуальное смещение, -1 если все смещения актуальны</returns>
        public int ModifiedOffset 
        { 
            get => _modifiedOffset;
            protected set
            {
                _modifiedOffset = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Возвращает все элементы текста, которые находятся в позиции offset
        /// </summary>
        public IEnumerable<TextNode> GetNodesByOffset(int offset, int endOffset = -1) 
        {
            if (ModifiedOffset != -1)
                UpdateOffsets();

            if (endOffset == -1)
                return _offsetTree.Query(offset);
            else
                return _offsetTree.Query(offset, endOffset);
        }
        /// <summary>
        /// Обновляет смещения у подчиненных элементов
        /// </summary>
        public void UpdateOffsets()
        {
            IEnumerable<TextNode> _oldNodes = _offsetTree.Query(_modifiedOffset, _offsetTree.Max);

            if (_oldNodes.Count() == 0)
            {
                _modifiedOffset = -1;
                return;
            }

            _offsetTree.Remove(_oldNodes);

            int currentOffset = _modifiedOffset;
            int lastLineIndex = 0;
            foreach (TextNode old in _oldNodes)
            {
                if (lastLineIndex != old.StartLineIndex && !(old is TextLineBreak))
                    currentOffset += 2;
                old.Offset = currentOffset;
                currentOffset += old.Length;

                _offsetTree.Add(old.Offset, old.Offset + old.Length, old);

                lastLineIndex = old.EndLineIndex;
            }

            _modifiedOffset = -1;
        }

        protected override void OnLoad()
        {
            StreamReader streamReader = new StreamReader(_fullName, _encoding);
            _text = streamReader.ReadToEnd();
            streamReader.Close();
            Deserialize(_text);
            _text = null;
            
            OnPropertyChanged(nameof(Nodes));
        }
        protected override void OnSave()
        {
            StreamWriter streamWriter = new StreamWriter(_fullName, false, _encoding);
            streamWriter.Write(Serialize());
            streamWriter.Close();
        }

        #region Определения ISerializable

        public abstract void Serialize(StringBuilder stringBuilder);

        public virtual string Serialize() 
        {
            StringBuilder stringBuilder = new StringBuilder();
            Serialize(stringBuilder);
            stringBuilder.Replace('\0', ' ');
            return stringBuilder.ToString();
        }

        public abstract void Deserialize(ReadingContext reader);

        public virtual void Deserialize(string text) 
        {
            Deserialize(new ReadingContext(text));
        }
        public virtual bool ExceptionPush(SerializationException exception)
        {
            return false;
        }

        public void Update(ISerializable source)
        {
            if (!(source is TextNode))
                throw new Exception($"Не допустимый тип элемента текста {source.GetType().FullName}");

            TextNode _node = source as TextNode;

            if (ModifiedOffset > _node.Offset)
                ModifiedOffset = _node.Offset;

            _offsetTree.Add(_node.Offset, _node.Offset + _node.Length, _node);
        }

        #endregion
    }
}
