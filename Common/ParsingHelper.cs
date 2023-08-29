using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stalker_Studio.Common
{
    static class ParsingHelper
    {
        /// <summary>
        /// Считывает блок до одного из указанных символов endChars
        /// </summary>
        /// <param name="reader">Откуда считывать блок символов</param>
        /// <param name="blockBuilder">Куда записывать символы</param>
        /// <param name="endChars">Массив завершающих символов</param>
        public static void ReadBlockUpTo(TextReader reader, StringBuilder blockBuilder, char[] endChars)
        {
            int n;
            char c;
            while ((n = reader.Peek()) != -1)
            {
                c = (char)n;
                if (endChars.Contains(c))
                    break;

                blockBuilder.Append(c);
                reader.Read();
            }
        }

        /// <summary>
        /// Считывает блок до первого слова endString
        /// </summary>
        /// <param name="reader">Откуда считывать блок символов</param>
        /// <param name="blockBuilder">Куда записывать символы</param>
        /// <param name="endString">Завершающая слово</param>
        public static void ReadBlockUpTo(TextReader reader, StringBuilder blockBuilder, string endString)
        {

        }
    }
}
