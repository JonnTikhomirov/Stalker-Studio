using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stalker_Studio.Common
{
    /// <summary>
    /// Контекст чтения символов. По сути, это TextReader, с возможностью изменения текущего индекса
    /// </summary>
    public class ReadingContext
    {
        string _text = null;
        public int CurrentIndex = 0;

        public ReadingContext(string text, int startIndex = 0)
        {
            _text = text;
            //StringInfo.;
            CurrentIndex = startIndex;
        }

        public char this[int index] 
        { 
            get => _text[index];
        }

        /// <summary>
        /// Считывает текущий символ не изменяя индекс
        /// </summary>
        public int Peek()
        {
            if (CurrentIndex == _text.Length)
                return -1;
            return _text[CurrentIndex];
        }
        /// <summary>
        /// Считывает текущий символ и сдвигает индекс вперед
        /// </summary>
        public int Read() 
        {
            if (CurrentIndex == _text.Length)
                return -1;
            return _text[CurrentIndex++];
        }

        public void ReadTo(StringBuilder blockBuilder, char[] endChars)
        {
            char c = '\0';
            for (; CurrentIndex < _text.Length; CurrentIndex++)
            {
                c = _text[CurrentIndex];
                if (endChars.Contains(c))
                    break;

                blockBuilder.Append(c);
            }
        }
        public void ReadTo(StringBuilder blockBuilder, char endChar)
        {
            char c = '\0';
            for (; CurrentIndex < _text.Length; CurrentIndex++)
            {
                c = _text[CurrentIndex];
                if (endChar == c)
                    break;

                blockBuilder.Append(c);
            }
        }
        public char[] ReadBlock(int count)
        {
            if (CurrentIndex + count > _text.Length)
                count = _text.Length - CurrentIndex;
            char[] chars = new char[count];
            _text.CopyTo(CurrentIndex, chars, 0, count);
            CurrentIndex += count;
            return chars;
        }
    }
}
