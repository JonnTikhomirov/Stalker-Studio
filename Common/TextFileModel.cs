using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Stalker_Studio.Common
{
    /// <summary>
    /// Класс работы с текстовым файлом 
    /// </summary>
    public class TextFileModel : FileModel
    {
        protected string _text = "";
        protected Encoding _encoding = Encoding.Unicode;

        public TextFileModel(Encoding encoding = null, string text = "") : base() 
        {
            _text = text;
            if (encoding != null)
                _encoding = encoding;
        }
        public TextFileModel(FileInfo file, Encoding encoding = null) : base(file)
        {
            if(encoding != null)
                _encoding = encoding;
        }
        public TextFileModel(string fullName, Encoding encoding = null) : base(fullName)
        {
            if (encoding != null)
                _encoding = encoding;
        }

        /// <summary>
        /// Текст
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public virtual string Text { 
            get 
            {
                if (!_isLoaded)
                    Load();
                return _text; 
            } 
            set 
            {
                _text = value;
                OnPropertyChanged();
            } 
        }
        /// <summary>
        /// Кодировка
        /// </summary>
        [System.ComponentModel.DisplayName("Кодировка")]
        [System.ComponentModel.ReadOnly(true)]
        public Encoding Encoding
        {
            get { return _encoding; }
            set
            {
                _encoding = value;
                OnPropertyChanged();
            }
        }

        protected override void OnLoad()
        {
            StreamReader streamReader = new StreamReader(_fullName, _encoding);
            _text = streamReader.ReadToEnd();
            streamReader.Close();
        }
        protected override void OnSave()
        {
            StreamWriter streamWriter = new StreamWriter(_fullName, false, _encoding);
            streamWriter.Write(_text);
            streamWriter.Close();
        }
    }
}
