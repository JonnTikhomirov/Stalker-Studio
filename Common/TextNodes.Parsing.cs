using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.IO;

namespace Stalker_Studio.Common
{
	public partial class TextLineBreak : TextNode
	{
		public override string Serialize()
		{
			return _lineBreakSymbols;
		}
		public override void Serialize(StringBuilder stringBuilder)
		{
			stringBuilder.Append(_lineBreakSymbols);
		}
		public override void Deserialize(ReadingContext reader)
		{
			char[] end = reader.ReadBlock(_lineBreakSymbols.Length);
            if (!_lineBreakSymbols.SequenceEqual(end))
				new SerializationException("Неправильно задан перенос строки", _startLineIndex, this, true);
		}
	}

	public partial class TextComment : TextBlock
    {
		public override string Serialize()
		{
			if (_startEnd == null)
				return _text ?? "";
			if(_startEnd.Item2 == null)
                return _startEnd.Item1 + _text ?? "";
            return _startEnd.Item1 + _text ?? "" + _startEnd.Item2;
		}
		public override void Serialize(StringBuilder stringBuilder)
		{
			if (_startEnd == null)
				stringBuilder.Append(_text ?? "");
			else
			{
				stringBuilder.Append(_startEnd.Item1);
				stringBuilder.Append(_text ?? "");
                if (_startEnd.Item2 != null)
                    stringBuilder.Append(_startEnd.Item2);
			}
		}
		public override void Deserialize(ReadingContext reader, bool _skipStartOperator = false)
		{
            string _startSymbols = _startEnd?.Item1;
            string _endSymbols = _startEnd?.Item2;
            StringBuilder stringBuilder = new StringBuilder(200);
			int n;

			if (!_skipStartOperator)
			{
				char[] start = reader.ReadBlock(_startSymbols.Length);
                if (!_startSymbols.SequenceEqual(start))
					throw new SerializationException($"Не известная конструкция", _startLineIndex, this, true);
			}

			if (_endSymbols == null)
			{
				reader.ReadTo(stringBuilder, new char[] { '\r' });

    //            while ((n = reader.Peek()) != -1)
				//{
				//	char c = (char)n;

				//	if (c == '\r' || c == '\n')
				//		break;
				//	stringBuilder.Append(c);
				//	reader.Read();
				//}
			}
			else
			{
				char firstEndSymbol = _endSymbols[0];
				while ((n = reader.Read()) != -1)
				{
					char c = (char)n;

					if (c == firstEndSymbol)
					{
						reader.CurrentIndex--;
						char[] end = reader.ReadBlock(_startSymbols.Length);
						if (_startSymbols.SequenceEqual(end))
							break;
					}
					else
						stringBuilder.Append(c);
					if (c == '\r' || c == '\n')
						EndLineIndex++;
				}
			}
            _text = stringBuilder.ToString();

            if (_skipStartOperator && _startEnd != null && _startEnd.Item1 != null)
                _offset -= _startEnd.Item1.Length;

			if (_startSymbols == null)
				_length = stringBuilder.Length;
            else if (_endSymbols == null)
				_length = stringBuilder.Length + _startSymbols.Length;
			else
				_length = stringBuilder.Length + _startSymbols.Length + _endSymbols.Length;

			Update(this);
		}
	}

	public partial class TextInclude : TextBlock
    {
		public override string Serialize()
		{
            if (_startEnd == null)
                return _parameter ?? "";
            if (_startEnd.Item2 == null)
                return _startEnd.Item1 + _parameter ?? "";
            return _startEnd.Item1 + _parameter ?? "" + _startEnd.Item2;
		}
		public override void Serialize(StringBuilder stringBuilder)
		{
            if (_startEnd == null)
                stringBuilder.Append(_parameter ?? "");
            else
            {
                stringBuilder.Append(_startEnd.Item1);
                stringBuilder.Append(_parameter ?? "");
                if (_startEnd.Item2 != null)
                    stringBuilder.Append(_startEnd.Item2);
            }
        }
		public override void Deserialize(ReadingContext reader, bool _skipStartOperator = false)
		{
            string _startSymbols = _startEnd?.Item1;
            string _endSymbols = _startEnd?.Item2;
            
            StringBuilder stringBuilder = new StringBuilder(200);
			int n;

			if (!_skipStartOperator)
			{
                char[] start = reader.ReadBlock(_startSymbols.Length);
                if (!_startSymbols.SequenceEqual(start))
					throw new SerializationException($"Не известная конструкция", _startLineIndex, this, true);
			}
            if (_endSymbols == null)
			{
				while ((n = reader.Read()) != -1)
				{
					char c = (char)n;

					stringBuilder.Append(c);
					if (c == '\r' || c == '\n')
					{
						break;
					}
				}
			}
			else
			{
				char firstEndSymbol = _endSymbols[0];
				while ((n = reader.Read()) != -1)
				{
					char c = (char)n;

					if (c == firstEndSymbol)
					{
						if (_endSymbols.Length == 1)
							break;
						reader.CurrentIndex -= 1;
						char[] end = reader.ReadBlock(_endSymbols.Length);
						if (_endSymbols.SequenceEqual(end))
							break;
					}
					else
						stringBuilder.Append(c);
					if (c == '\r' || c == '\n')
						EndLineIndex++;
				}
			}
			_parameter = stringBuilder.ToString();

            if (_skipStartOperator && _startEnd != null && _startEnd.Item1 != null)
                _offset -= _startEnd.Item1.Length;

            if (_startSymbols == null)
                _length = stringBuilder.Length;
            else if (_endSymbols == null)
				_length = stringBuilder.Length + _startSymbols.Length;
			else
				_length = stringBuilder.Length + _startSymbols.Length + _endSymbols.Length;
		}
	}

    public partial class TextTabulation : TextNode
    {
        public override string Serialize()
        {
            return new string('\t', _length);
        }
        public override void Serialize(StringBuilder stringBuilder)
        {
			stringBuilder.Append('\t', _length);
        }
        public override void Deserialize(ReadingContext reader)
        {
			int n;
			char tab = '\t';

            while ((n = reader.Peek()) != -1)
            {
                char c = (char)n;

                if (c != tab)
                    break;
                reader.Read();
				_length++;
            }

            Update(this);
        }
    } 
}
