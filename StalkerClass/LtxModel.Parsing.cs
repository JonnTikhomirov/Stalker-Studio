using Stalker_Studio.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;

namespace Stalker_Studio.StalkerClass
{
    partial class LtxModel
    {
        public static readonly Tuple<string, string> CommentOperators = new Tuple<string, string>(";", null);
        public static readonly Tuple<string, string> IncludeOperators = new Tuple<string, string>("#include \"", "\"");

        public override void Serialize(StringBuilder stringBuilder)
        {
            if (_nodes == null)
                return;
            int lastLineIndex = 0;
            foreach (TextNode node in _nodes)
            {
                if (node.StartLineIndex != lastLineIndex && !(node is TextLineBreak))
                {
                    //stringBuilder.Length = node.Offset - 2;
                    stringBuilder.Append("\r\n");
                }
                //else
                    stringBuilder.Length = node.Offset;
                node.Serialize(stringBuilder);
                lastLineIndex = node.EndLineIndex;
            }
        }

        public override void Deserialize(ReadingContext reader)
        {
            // Оптимизация по выделению памяти
            int initCapacity = _sections?.Count ?? 0;
            if (initCapacity == 0 && _text != null)
                if (_text.Length > 5000)
                    initCapacity = _text.Length / 1000;// считаем что одна секция в среднем занимает 1000 символов для больших файлов
                else
                    initCapacity = _text.Length / 200;// считаем что одна секция в среднем занимает 5 строк для маленьких файлов
            List<LtxSection> listSections = new List<LtxSection>(initCapacity);

            // Оптимизация по выделению памяти
            initCapacity = _nodes?.Count ?? 0;
            if (initCapacity == 0 && _text != null)
                if (_text.Length > 5000)
                    // считаем что один файл содержит различаемых элементов первого уровня в 25 раз меньше чем строк для больших файлов
                    initCapacity = _text.Length / 700;
                else
                    // считаем что один файл содержит различаемых первого уровня элементов в 10 раза меньше чем строк для маленьких файлов
                    initCapacity = _text.Length / 200;

            List<TextNode> listNodes = new List<TextNode>(initCapacity);

            // Оптимизация по выделению памяти
            initCapacity = _includes?.Count ?? 0;
            if (initCapacity == 0 && _text != null)
                if (_text.Length > 5000)
                    initCapacity = 10;// пусть так
                else
                    initCapacity = 4;// пусть так
            List<TextInclude> listIncludes = new List<TextInclude>(initCapacity);

            int n;
            int currentLineIndex = 0;
            char includeStart = '#';
            char commentStart = ';';
            char sectionStart = '[';
            char tabulationStart = '\t';
            char[] nonParametersSymbols = { ' ', '\0', '\t' };
            TextNode last = null;

            while ((n = reader.Read()) != -1)
            {
                char c = (char)n;

                if (c == '\r')
                {
                    bool needNewLine = false;
                    if (last != null)
                        needNewLine = last.EndLineIndex != currentLineIndex;
                    else
                        needNewLine = true;

                    if (needNewLine)
                    {
                        reader.CurrentIndex--;
                        last = new TextLineBreak(this, reader.CurrentIndex, currentLineIndex);
                        try
                        {
                            last.Deserialize(reader);
                        }
                        catch (Exception exception)
                        {
                            ViewModel.Workspace.This.MessageList.Add(this, $"Строка {currentLineIndex + 1}: {exception.Message}", ViewModel.ErrorCategory.Error, this);
                        }
                        listNodes.Add(last);
                    }
                    else
                    {
                        reader.CurrentIndex++;
                    }
                    currentLineIndex++;
                }
                else if (c == includeStart)
                {
                    reader.CurrentIndex--;
                    last = new TextInclude(this, reader.CurrentIndex, currentLineIndex, IncludeOperators);
                    try
                    {
                        last.Deserialize(reader);
                    }
                    catch (Exception exception)
                    {
                        ViewModel.Workspace.This.MessageList.Add(this, $"Строка {currentLineIndex + 1}: {exception.Message}", ViewModel.ErrorCategory.Error, this);
                        while ((n = reader.Read()) != -1)// проматываем до начала следующей стоки, то есть пропускаем строку
                        {
                            if ((char)n == '\r')
                                break;
                        }
                        currentLineIndex++;
                        continue;
                    }
                    listNodes.Add(last);
                    listIncludes.Add(last as TextInclude);
                    currentLineIndex = last.EndLineIndex;
                }
                else if (c == commentStart)
                {
                    reader.CurrentIndex--;
                    last = new TextComment(this, reader.CurrentIndex, currentLineIndex, CommentOperators);
                    try
                    {
                        last.Deserialize(reader);
                    }
                    catch (Exception exception)
                    {
                        ViewModel.Workspace.This.MessageList.Add(this, $"Строка {currentLineIndex + 1}: {exception.Message}", ViewModel.ErrorCategory.Error, this);
                        break;
                    }
                    listNodes.Add(last);
                    currentLineIndex = last.EndLineIndex;
                }
                else if (c == sectionStart)
                {
                    reader.CurrentIndex--;
                    last = new LtxSection(this, reader.CurrentIndex, currentLineIndex);
                    try
                    {
                        last.Deserialize(reader);
                    }
                    catch (Exception exception)
                    {
                        ViewModel.Workspace.This.MessageList.Add(this, $"Строка {currentLineIndex + 1}: {exception.Message}", ViewModel.ErrorCategory.Error, this);
                        break;
                    }
                    listSections.Add(last as LtxSection);
                    listNodes.Add(last);
                    currentLineIndex = last.EndLineIndex + 1;
                }
                else if (c == tabulationStart)
                {
                    reader.CurrentIndex--;
                    last = new TextTabulation(this, reader.CurrentIndex, currentLineIndex);
                    try
                    {
                        last.Deserialize(reader);
                    }
                    catch (Exception exception)
                    {
                        ViewModel.Workspace.This.MessageList.Add(this, $"Строка {currentLineIndex + 1}: {exception.Message}", ViewModel.ErrorCategory.Error, this);
                        while ((n = reader.Read()) != -1)// проматываем до начала следующей стоки, то есть пропускаем строку
                        {
                            if ((char)n == '\r')
                                break;
                        }
                        currentLineIndex++;
                        continue;
                    }
                    listNodes.Add(last);
                }
                else if (!nonParametersSymbols.Contains(c))
                {
                    reader.CurrentIndex--;
                    last = new LtxParameter(this, reader.CurrentIndex, currentLineIndex);
                    try
                    {
                        last.Deserialize(reader);
                    }
                    catch (Exception exception)
                    {
                        ViewModel.Workspace.This.MessageList.Add(this, $"Строка {currentLineIndex + 1}: {exception.Message}", ViewModel.ErrorCategory.Error, this);
                        while ((n = reader.Read()) != -1)// проматываем до начала следующей стоки, то есть пропускаем строку
                        {
                            if ((char)n == '\r')
                                break;
                        }
                        currentLineIndex++;
                        continue;
                    }
                    listNodes.Add(last);
                    currentLineIndex = last.EndLineIndex;
                }
            }

            _sections = new ObservableCollection<LtxSection>(listSections);
            _includes = new ObservableCollection<TextInclude>(listIncludes);
            _nodes = new ObservableCollection<TextNode>(listNodes);

            OnPropertyChanged(nameof(Includes));
            OnPropertyChanged(nameof(Sections));
            OnPropertyChanged(nameof(Nodes));
        }

        public override bool ExceptionPush(SerializationException exception)
        {
            ViewModel.Workspace.This.MessageList.Add(
                exception.Object, 
                $"Строка {exception.LineIndex + 1}: {exception.Message}",
                exception.IsCritical ? ViewModel.ErrorCategory.Error : ViewModel.ErrorCategory.Warning, 
                this);
            if (exception.InnerException is SerializationException)
            {
                SerializationException innerException = (SerializationException)exception.InnerException;
                ViewModel.Workspace.This.MessageList.Add(
                    innerException.Object, 
                    $"Строка {innerException.LineIndex + 1}: {innerException.Message}",
                    innerException.IsCritical ? ViewModel.ErrorCategory.Error : ViewModel.ErrorCategory.Warning, 
                    this);

                if (innerException.InnerException != null && !(innerException.InnerException is SerializationException))
                {
                    return true;
                }
            }
            else if(exception.InnerException != null)
                return true;
            return false;
        }
    }

    partial class LtxSection 
    {
        public override void Serialize(StringBuilder stringBuilder)
        {
            int lastLineIndex = _startLineIndex;
            stringBuilder.Append('[');
            stringBuilder.Append(_name);
            stringBuilder.Append(']');
            if (_parents != null)
            {
                stringBuilder.Append(':');
                
                for (int i = 0; i < _parents.Count; i++)
                {
                    //stringBuilder.Append(' ');
                    stringBuilder.Append(_parents[i]);
                    if (i + 1 < _parents.Count)
                        stringBuilder.Append(',');
                }
            }
            if (_comment != null)
            {
                stringBuilder.Length = _comment.Offset;
                _comment.Serialize(stringBuilder);
            }
            stringBuilder.Append("\r\n");
            lastLineIndex++;

            if (_nodes == null)
                return;
            for (int i = 0; i < _nodes.Count; i++)
            {
                TextNode node = _nodes[i];

                if (node.StartLineIndex != lastLineIndex)
                    stringBuilder.Append("\r\n");

                stringBuilder.Length = node.Offset;
                node.Serialize(stringBuilder);
                lastLineIndex = node.EndLineIndex;
            }
        }
        public override void Deserialize(ReadingContext reader)
        {
            int n;
            char commentStart = ';';
            char sectionStart = '[';
            char sectionNameEnd = ']';
            char parentsStart = ':';
            char tabulationStart = '\t';
            char[] nonParametersSymbols = { ' ', '\0', '\t', '\r' };
            TextNode last = null;

            while ((n = reader.Read()) != -1) // наименование
            {
                char c = (char)n;

                if (c == sectionStart)
                {
                    StringBuilder nameBuilder = new StringBuilder(50);
                    //while ((n = reader.Read()) != -1)
                    //{
                    //    c = (char)n;
                    //    if (c == sectionNameEnd)
                    //        break;
                    //    else if (nonParametersSymbols.Contains(c))
                    //    {
                    //        if (ExceptionPush(new SerializationException("Не допустимое имя", _startLineIndex, this)))
                    //            return;
                    //    }
                    //    else
                    //        nameBuilder.Append(c);
                    //}
                    reader.ReadTo(nameBuilder, sectionNameEnd);
                    if (nameBuilder.Length == 0)
                    {
                        if (ExceptionPush(new SerializationException("Не найдено имя секции", _startLineIndex, this)))
                            return;
                    }
                    reader.CurrentIndex++;// Пропускаем ]
                    _name = nameBuilder.ToString();
                    break;
                }
                else if (c == '\r')
                {
                    if(ExceptionPush(new SerializationException("Не найдено имя секции", _startLineIndex, this)))
                        return;
                }
            }

            while ((n = reader.Read()) != -1) // родители
            {
                char c = (char)n;
                if (c == parentsStart)
                {
                    List<string> parents = new List<string>(3);
                    StringBuilder parentBuilder = new StringBuilder(25);

                    while ((n = reader.Read()) != -1)
                    {
                        c = (char)n;

                        if (c == ',')
                        {
                            parents.Add(parentBuilder.ToString());
                            parentBuilder.Clear();
                        }
                        else if (c == commentStart)
                        {
                            reader.CurrentIndex -= 1;
                            _comment = new TextComment(this, reader.CurrentIndex, _endLineIndex, LtxModel.CommentOperators);
                            try
                            {
                                _comment.Deserialize(reader);
                            }
                            catch (Exception exception)
                            {
                                throw new SerializationException("Ошибка десериализации комментария", exception, _startLineIndex, this);
                            }
                            break;
                        }
                        else if (c == '\r')
                            break;
                        else if (c != ' ')
                            parentBuilder.Append(c);
                    }

                    if (parentBuilder.Length != 0)
                    {
                        parents.Add(parentBuilder.ToString());
                        parentBuilder.Length = 0;
                    }

                    _parents = new ObservableCollection<string>(parents);
                }
                else if (c == commentStart)
                {
                    reader.CurrentIndex -= 1;
                    _comment = new TextComment(this, reader.CurrentIndex, _endLineIndex, LtxModel.CommentOperators);
                    _comment.Deserialize(reader);
                    break;
                }
                else if (c == '\r')
                    break;
            }

            reader.CurrentIndex++;// Пропускаем сразу \n

            List<TextNode> listNodes;
            // Оптимизация по выделению памяти
            int initCapacity = _nodes?.Count ?? 0;
            if (initCapacity == 0)
                listNodes = new List<TextNode>();
            else
                listNodes = new List<TextNode>(initCapacity);

            List<LtxParameter> listParameters;
            // Оптимизация по выделению памяти
            initCapacity = _parameters?.Count ?? 0;
            if (initCapacity == 0)
                listParameters = new List<LtxParameter>();
            else
                listParameters = new List<LtxParameter>(initCapacity);

            while ((n = reader.Read()) != -1)// содержимое
            {
                char c = (char)n;

                if (c == '\r')
                {
                    bool needNewLine = false;
                    if (last != null)
                        needNewLine = last.EndLineIndex != _endLineIndex;
                    else
                        needNewLine = false;

                    if (needNewLine)
                    {
                        reader.CurrentIndex--;
                        last = new TextLineBreak(this, reader.CurrentIndex, _endLineIndex);
                        last.Deserialize(reader);
                        listNodes.Add(last);
                    }
                    else
                    {
                        reader.CurrentIndex++;
                    }
                    _endLineIndex++;
                }
                else if (c == commentStart)
                {
                    reader.CurrentIndex--;
                    last = new TextComment(this, reader.CurrentIndex, _endLineIndex, LtxModel.CommentOperators);
                    try
                    {
                        last.Deserialize(reader);
                    }
                    catch (Exception exception)
                    {
                        throw new SerializationException("Ошибка десериализации комментария", exception, _endLineIndex, this);
                    }
                    listNodes.Add(last);
                }
                else if (c == tabulationStart)
                {
                    reader.CurrentIndex--;
                    last = new TextTabulation(this, reader.CurrentIndex, _endLineIndex);
                    try
                    {
                        last.Deserialize(reader);
                    }
                    catch (Exception exception)
                    {
                        throw new SerializationException("Ошибка десериализации табуляции", exception, _endLineIndex, this);
                    }
                    listNodes.Add(last);
                }
                else if (c == sectionStart)
                {
                    if (last?.EndLineIndex != _endLineIndex)
                        _endLineIndex -= 1;
                    break;
                }
                else if (!nonParametersSymbols.Contains(c))
                {
                    reader.CurrentIndex--;
                    last = new LtxParameter(this, reader.CurrentIndex, _endLineIndex);
                    try
                    {
                        last.Deserialize(reader);
                    }
                    catch (Exception exception)
                    {
                        throw new SerializationException("Ошибка десериализации параметра", exception, _endLineIndex, this);
                    }
                    listNodes.Add(last);
                    listParameters.Add(last as LtxParameter);
                }
            }

            _length = reader.CurrentIndex - _offset;

            _parameters = new ObservableCollection<LtxParameter>(listParameters);
            _nodes = new ObservableCollection<TextNode>(listNodes);

            OnPropertyChanged(nameof(Parameters));
            OnPropertyChanged(nameof(Nodes));

            if (_parents?.Count == 0)
                ExceptionPush(new SerializationException("Не определены родители секции", _startLineIndex, this, false));
        }
    }

    partial class LtxParameter 
    {
        public override void Serialize(StringBuilder stringBuilder)
        {
            if (_operator != null)
            {
                stringBuilder.Append(_operator);
                stringBuilder.Append(' ');
            }
            if (_name != null)
                stringBuilder.Append(_name);

            if (_equalSpace != null)
                stringBuilder.Append(_equalSpace);

            else if (_value != null)
                stringBuilder.Append(' ');

            if (_value != null)
                stringBuilder.Append(_value);

            if (_nodes != null)
            {
                foreach (TextNode node in _nodes)
                {
                    stringBuilder.Length = node.Offset;
                    node.Serialize(stringBuilder);
                }
            }

            if (_comment != null)
            {
                stringBuilder.Length = _comment.Offset;
                _comment.Serialize(stringBuilder);
            }

            stringBuilder.Length = _offset + _length;
        }

        public override void Deserialize(ReadingContext reader)
        {
            int n;
            char commentStart = ';';
            char tabulationStart = '\t';
            char equal = '=';
            char[] nonParametersSymbols = { ' ', '\0', '\t' };

            List<TextNode> listNodes;
            // Оптимизация по выделению памяти
            int initCapacity = _nodes?.Count ?? 0;
            if (initCapacity == 0)
                listNodes = new List<TextNode>(2);
            else
                listNodes = new List<TextNode>(initCapacity);

            StringBuilder[] partsBuilders = new StringBuilder[3];
            int partIndex = -1;
            StringBuilder spaceBuilder = new StringBuilder(10);
            bool haveAddSymbol = false;

            while ((n = reader.Read()) != -1 && partIndex < partsBuilders.Length)
            {
                char c = (char)n;

                if (c == '\r')
                    break;
                else if (!nonParametersSymbols.Contains(c) && c != equal && c != commentStart && c != '+')
                {
                    if (partIndex == -1)
                    {
                        partIndex = 0;
                    }
                    else
                    {
                        if (haveAddSymbol)
                        {
                            partIndex -= 1;
                            partsBuilders[partIndex].Append(spaceBuilder.ToString());
                            haveAddSymbol = false;
                        }
                    }

                    if (partsBuilders[partIndex] == null)
                        partsBuilders[partIndex] = new StringBuilder(40);

                    partsBuilders[partIndex].Append(c);
                }
                else
                {
                    if (partIndex != -1)
                    {
                        if (partsBuilders[partIndex] != null)
                        {
                            partIndex++;
                            spaceBuilder.Clear();
                        }

                        if (c == '+')
                            haveAddSymbol = true;

                        spaceBuilder.Append(c);

                        if (c == equal)
                            break;
                    }
                    else
                        _offset++;
                }
            }

            if ((char)n == equal)
            {
                while ((n = reader.Read()) != -1)
                {
                    char c = (char)n;
                    if (c == '\r')
                        break;
                    else if (c == commentStart)
                        break;
                    else if (nonParametersSymbols.Contains(c))
                        spaceBuilder.Append(c);
                    else
                        break;
                }
                _equalSpace = spaceBuilder.ToString();

                StringBuilder valueBuilder = new StringBuilder(100);
                reader.CurrentIndex--;

                while ((n = reader.Read()) != -1)
                {
                    char c = (char)n;

                    if (c == '\r')
                        break;
                    else if (c == tabulationStart)
                    {
                        reader.CurrentIndex--;
                        TextTabulation last = new TextTabulation(this, reader.CurrentIndex, _endLineIndex);
                        try
                        {
                            last.Deserialize(reader);
                        }
                        catch (Exception exception)
                        {
                            throw new SerializationException("Ошибка десериализации табуляции", exception, _startLineIndex, this);
                        }
                        listNodes.Add(last);
                    }
                    else if (c == commentStart)
                    {
                        reader.CurrentIndex--;
                        _comment = new TextComment(this, reader.CurrentIndex, _endLineIndex, LtxModel.CommentOperators);
                        try
                        {
                            _comment.Deserialize(reader);
                        }
                        catch (Exception exception)
                        {
                            throw new SerializationException("Ошибка десериализации комментария", exception, _startLineIndex, this);
                        }
                        listNodes.Add(_comment);
                        break;
                    }
                    else
                        valueBuilder.Append(c);
                }

                _value = valueBuilder.ToString().Trim();
            }

            if (partsBuilders[0] == null || partsBuilders[0].Length == 0)
                throw new SerializationException("Параметр не найден", _startLineIndex, this);

            _length = reader.CurrentIndex - _offset;

            if (partsBuilders[1] == null) // (<Имя параметра>   =   <Значение>)  ИЛИ  (<Значение>)
            {
                if (_value == null) // (<Значение>)
                    _value = partsBuilders[0].ToString();
                else // (<Имя параметра>   =   <Значение>)
                    _name = partsBuilders[0].ToString();
            }
            else if (partsBuilders[2] == null) // (<Имя параметра> <Значение>)  ИЛИ  (<Имя оператора> <Имя параметра> = <Значение>)
            {
                if (_value == null) // (<Имя параметра> <Значение>)
                {
                    _name = partsBuilders[0].ToString();
                    _value = partsBuilders[1].ToString();
                }
                else // (<Имя оператора> <Имя параметра> = <Значение>)
                {
                    Operator = partsBuilders[0].ToString();
                    _name = partsBuilders[1].ToString();
                }
            }
            else if (_value == null) // (<Имя оператора> <Имя параметра> <Значение>)
            {
                Operator = partsBuilders[0].ToString();
                _name = partsBuilders[1].ToString();
                _value = partsBuilders[2].ToString();
            }
            else
                ExceptionPush(new SerializationException("Не известная конструкция", _startLineIndex, this));

            _nodes = new ObservableCollection<TextNode>(listNodes);

            //if (_equalSpace?.Length != 0 && _value?.Length == 0)
            //    ExceptionPush(new SerializationException("Присвоено пустое значение", _startLineIndex, this));
        }
    }
}
