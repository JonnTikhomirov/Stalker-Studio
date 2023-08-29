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
    partial class LuaModel
    {
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
                else
                    stringBuilder.Length = node.Offset;
                node.Serialize(stringBuilder);
                lastLineIndex = node.EndLineIndex;
            }
        }

        public override void Deserialize(ReadingContext reader)
        {
            //return;
            // Оптимизация по выделению памяти
            int initCapacity = _functions?.Count ?? 0;
            if (initCapacity == 0 && _text != null)
                if (_text.Length > 5000)
                    initCapacity = _text.Length / 1000;// считаем что одна секция в среднем занимает 1000 символов для больших файлов
                else
                    initCapacity = _text.Length / 200;// считаем что одна секция в среднем занимает 5 строк для маленьких файлов
            List<LuaFunction> listFunctions = new List<LuaFunction>(initCapacity);

            // Оптимизация по выделению памяти
            initCapacity = _classes?.Count ?? 0;
            if (initCapacity == 0 && _text != null)
                if (_text.Length > 5000)
                    initCapacity = 10;// пусть так
                else
                    initCapacity = 4;// пусть так
            List<LuaClass> listClasses = new List<LuaClass>(initCapacity);

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
            initCapacity = _variables?.Count ?? 0;
            if (initCapacity == 0 && _text != null)
                if (_text.Length > 5000)
                    initCapacity = 10;// пусть так
                else
                    initCapacity = 4;// пусть так
            List<LuaVariable> listVariables = new List<LuaVariable>(initCapacity);

            int n;
            int currentLineIndex = 0;
            string commentStart = "--";
            char tabulationStart = '\t';
            char[] nonWordSymbols = { ' ', '\0', '\t', '=', ',', '(', '\r', '\n' };
            TextNode last = null;
            StringBuilder expressionBuilder = new StringBuilder();

            while ((n = reader.Peek()) != -1)
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
                        last = new TextLineBreak(this, reader.CurrentIndex, currentLineIndex);
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
                                {
                                    reader.Read();
                                    break;
                                }
                                else if ((char)n == '\n')
                                    break;
                            }
                            currentLineIndex++;
                            continue;
                        }
                        listNodes.Add(last);
                    }
                    else
                    {
                        reader.Read();
                        reader.Read();// пропускаем сразу \n
                    }
                    currentLineIndex++;
                }
                else if (c == LuaFunction.StartEndOperators.Item1[0])
                {
                    //ParsingHelper.ReadBlockUpTo(reader, expressionBuilder, nonWordSymbols);
                    reader.Read();
                    reader.ReadTo(expressionBuilder, nonWordSymbols);

                    if (expressionBuilder.ToString() == LuaFunction.StartEndOperators.Item1)
                    {
                        last = new LuaFunction(this, reader.CurrentIndex, currentLineIndex);
                        try
                        {
                            (last as LuaFunction).Deserialize(reader, true);
                        }
                        catch (Exception exception)
                        {
                            ViewModel.Workspace.This.MessageList.Add(this, $"Строка {currentLineIndex + 1}: {exception.Message}", ViewModel.ErrorCategory.Error, this);
                            while ((n = reader.Read()) != -1)// проматываем до начала следующей стоки, то есть пропускаем строку
                            {
                                if ((char)n == '\r')
                                {
                                    reader.Read();
                                    break;
                                }
                                else if ((char)n == '\n')
                                    break;
                            }
                            currentLineIndex++;
                            continue;
                        }
                        listNodes.Add(last);
                        listFunctions.Add(last as LuaFunction);
                        currentLineIndex = last.EndLineIndex;
                        expressionBuilder.Clear();
                    }
                }
                else if (c == commentStart[0])
                {
                    reader.Read();
                    if ((n = reader.Peek()) == -1)
                        break;
                    c = (char)n;

                    if (c == commentStart[1])
                    {
                        reader.Read();

                        last = new TextComment(this, reader.CurrentIndex, currentLineIndex, CommentOperators);
                        try
                        {
                            (last as TextComment).Deserialize(reader, true);
                        }
                        catch (Exception exception)
                        {
                            ViewModel.Workspace.This.MessageList.Add(this, $"Строка {currentLineIndex + 1}: {exception.Message}", ViewModel.ErrorCategory.Error, this);
                            break;
                        }
                        listNodes.Add(last);
                        currentLineIndex = last.EndLineIndex;
                    }
                }
                else if (c == LuaClass.StartEndOperators.Item1[0])
                {
                    //ParsingHelper.ReadBlockUpTo(reader, expressionBuilder, nonWordSymbols);
                    reader.Read();
                    reader.ReadTo(expressionBuilder, nonWordSymbols);

                    if (expressionBuilder.ToString() == LuaClass.StartEndOperators.Item1)
                    {
                        last = new LuaClass(this, reader.CurrentIndex, currentLineIndex);
                        try
                        {
                            (last as LuaClass).Deserialize(reader, true);
                        }
                        catch (Exception exception)
                        {
                            ViewModel.Workspace.This.MessageList.Add(this, $"Строка {currentLineIndex + 1}: {exception.Message}", ViewModel.ErrorCategory.Error, this);
                            break;
                        }
                        listClasses.Add(last as LuaClass);
                        listNodes.Add(last);
                        currentLineIndex = last.EndLineIndex + 1;
                        expressionBuilder.Clear();
                    }
                }
                else if (c == tabulationStart)
                {
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
                            {
                                reader.Read();
                                break;
                            }
                            else if ((char)n == '\n')
                                break;
                        }
                        currentLineIndex++;
                        continue;
                    }
                    listNodes.Add(last);
                }
                else if (expressionBuilder.Length != 0)
                {
                    last = new LuaVariable(this, reader.CurrentIndex, currentLineIndex);
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
                            {
                                reader.Read();
                                break;
                            }
                            else if ((char)n == '\n')
                                break;
                        }
                        currentLineIndex++;
                        continue;
                    }
                    listVariables.Add(last as LuaVariable);
                    currentLineIndex = last.EndLineIndex;
                }
                else if (!nonWordSymbols.Contains(c))
                {
                    last = new LuaVariable(this, reader.CurrentIndex, currentLineIndex);
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
                            {
                                reader.Read();
                                break;
                            }
                            else if ((char)n == '\n')
                                break;
                        }
                        currentLineIndex++;
                        continue;
                    }
                    listVariables.Add(last as LuaVariable);
                    currentLineIndex = last.EndLineIndex;
                }
                else
                {
                    reader.Read();
                }
            }

            _classes = new ObservableCollection<LuaClass>(listClasses);
            _functions = new ObservableCollection<LuaFunction>(listFunctions);
            _variables = new ObservableCollection<LuaVariable>(listVariables);
            _nodes = new ObservableCollection<TextNode>(listNodes);

            OnPropertyChanged(nameof(Classes));
            OnPropertyChanged(nameof(Functions));
            OnPropertyChanged(nameof(Variables));
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

    partial class LuaFunction
    {
        public override void Serialize(StringBuilder stringBuilder)
        {
            stringBuilder.Append(StartEndOperators.Item1);
            stringBuilder.Append(' ');
            stringBuilder.Append(_name);
            stringBuilder.Append('(');
            for (int i = 0; i < _parameters.Count; i++)
            {
                //stringBuilder.Append(' ');
                stringBuilder.Append(_parameters[i]);
                if (i + 1 < _parameters.Count)
                    stringBuilder.Append(',');
            }
            stringBuilder.Append(')');
            if (_comment != null)
            {
                stringBuilder.Length = _comment.Offset;
                _comment.Serialize(stringBuilder);
            }
            stringBuilder.Append("\r\n");
            stringBuilder.Append(_text);

            if (_nodes != null)
            {
                int lastLineIndex = 0;
                foreach (TextNode node in _nodes)
                {
                    if (node.StartLineIndex != lastLineIndex && !(node is TextLineBreak))
                    {
                        //stringBuilder.Length = node.Offset - 2;
                        stringBuilder.Append("\r\n");
                    }
                    else
                        stringBuilder.Length = node.Offset;
                    node.Serialize(stringBuilder);
                    lastLineIndex = node.EndLineIndex;
                }
            }
            if(StartEndOperators.Item2 != null)
                stringBuilder.Append(StartEndOperators.Item2);
        }

        public override void Deserialize(ReadingContext reader, bool _skipStartOperator = false)
        {
            List<TextNode> listNodes;
            // Оптимизация по выделению памяти
            int initCapacity = _nodes?.Count ?? 0;
            if (initCapacity == 0)
                listNodes = new List<TextNode>();
            else
                listNodes = new List<TextNode>(initCapacity);

            // Оптимизация по выделению памяти
            initCapacity = _parameters?.Count ?? 0;
                if (initCapacity == 0)
                    initCapacity = 6;// пусть так

            List<string> listParameters = new List<string>(initCapacity);

            int n;
            char c = '\0';
            int currentOffset = _offset;
            string commentStart = "--";
            string ifStart = "if";
            string cycleStart = "for";
            char tabulationStart = '\t';
            char[] nonWordSymbols = { ' ', '\0', '\t', '=', ',', '(', ')', '\r', '\n' };
            StringBuilder textBuilder = new StringBuilder();
            StringBuilder expressionBuilder = new StringBuilder();
            int _endsToSkip = 0;

            if (!_skipStartOperator)
            {
                while ((n = reader.Peek()) != -1)
                {
                    c = (char)n;

                    if (c == StartEndOperators.Item1[0])
                    {
                        //ParsingHelper.ReadBlockUpTo(reader, expressionBuilder, nonWordSymbols);
                        reader.ReadTo(expressionBuilder, nonWordSymbols);

                        if (expressionBuilder.ToString() == StartEndOperators.Item1)
                        {
                            currentOffset += expressionBuilder.Length;
                            expressionBuilder.Length = 0;
                        }
                        else
                            throw new SerializationException(_endLineIndex, this, true);
                    }
                }
            }

            while ((n = reader.Peek()) != -1)
            {
                c = (char)n;

                if (c == commentStart[0])
                {
                    //ParsingHelper.ReadBlockUpTo(reader, textBuilder, new char[] { '\r' });// считываем до конца строки
                    reader.ReadTo(expressionBuilder, '\r');
                }

                else if (c == '\r')
                {
                    reader.Read();
                    reader.Read();
                    currentOffset += 2;
                    _endLineIndex++;
                    break;
                }
                else if (!nonWordSymbols.Contains(c))
                {
                    //ParsingHelper.ReadBlockUpTo(reader, expressionBuilder, nonWordSymbols);
                    reader.ReadTo(expressionBuilder, nonWordSymbols);
                    _name = expressionBuilder.ToString();
                    currentOffset += expressionBuilder.Length;
                    expressionBuilder.Length = 0;
                    c = (char)(n = reader.Peek());
                    break;
                }
                else
                {
                    reader.Read();
                    currentOffset++;
                }
            }

            if (c == '(')// Параметры
            {
                reader.Read();
                while ((n = reader.Peek()) != -1)
                {
                    c = (char)n;

                    if (c == ')')
                    {
                        reader.Read();
                        currentOffset++;
                        //break;
                    }
                    else if (c == '\r')
                    {
                        reader.Read();
                        reader.Read();
                        currentOffset += 2;
                        _endLineIndex++;
                        break;
                    }
                    else if (!nonWordSymbols.Contains(c))
                    {
                        //ParsingHelper.ReadBlockUpTo(reader, expressionBuilder, nonWordSymbols);
                        reader.ReadTo(expressionBuilder, nonWordSymbols);
                        listParameters.Add(expressionBuilder.ToString());
                        currentOffset += expressionBuilder.Length;
                        expressionBuilder.Length = 0;
                    }
                    else
                    {
                        reader.Read();
                        currentOffset++;
                    }
                }
            }
            string expression = null;// Чтобы не повторять expressionBuilder.ToString()
            char lastChar = '\0';
            while ((n = reader.Peek()) != -1)
            {
                c = (char)n;

                if (c == '\r')
                    _endLineIndex++;

                if (nonWordSymbols.Contains(lastChar))
                {
                    if (c == StartEndOperators.Item2[0])
                    {
                        //ParsingHelper.ReadBlockUpTo(reader, expressionBuilder, nonWordSymbols);
                        reader.ReadTo(expressionBuilder, nonWordSymbols);
                        expression = expressionBuilder.ToString();

                        if (expression == StartEndOperators.Item2)
                        {
                            if (_endsToSkip == 0)
                            {
                                currentOffset += expressionBuilder.Length;
                                expressionBuilder.Length = 0;
                                break;
                            }
                            else
                                _endsToSkip -= 1;
                        }
                    }
                    else if (c == ifStart[0])
                    {
                        //ParsingHelper.ReadBlockUpTo(reader, expressionBuilder, nonWordSymbols);
                        reader.ReadTo(expressionBuilder, nonWordSymbols);
                        expression = expressionBuilder.ToString();

                        if (expression == ifStart)
                            _endsToSkip++;
                    }
                    else if (c == cycleStart[0])
                    {
                        //ParsingHelper.ReadBlockUpTo(reader, expressionBuilder, nonWordSymbols);
                        reader.ReadTo(expressionBuilder, nonWordSymbols);
                        expression = expressionBuilder.ToString();

                        if (expression == cycleStart)
                            _endsToSkip++;
                    }
                    c = (char)(n = reader.Peek());
                }
                //else
                //{

                //    //textBuilder.Append(c);
                //}

                if (expressionBuilder.Length != 0)
                {
                    currentOffset += expressionBuilder.Length;
                    textBuilder.Append(expression);
                    expressionBuilder.Length = 0;
                }

                textBuilder.Append(c);

                lastChar = c;
                reader.Read();
                currentOffset++;
            }

            _length = currentOffset - _offset;
            if (_skipStartOperator && StartEndOperators != null && StartEndOperators.Item1 != null)
            {
                _offset -= StartEndOperators.Item1.Length;
                _length += StartEndOperators.Item1.Length;
            }

            _text = textBuilder.ToString();

            _nodes = new ObservableCollection<TextNode>(listNodes);
            _parameters = new ObservableCollection<string>(listParameters);

            OnPropertyChanged(nameof(Nodes));
        }
    }

    partial class LuaVariable
    {
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
                else
                    stringBuilder.Length = node.Offset;
                node.Serialize(stringBuilder);
                lastLineIndex = node.EndLineIndex;
            }
        }

        public override void Deserialize(ReadingContext reader)
        {
            int n;
            char c;
            int currentOffset = _offset;
            StringBuilder stringBuilder = new StringBuilder();

            while ((n = reader.Peek()) != -1)
            {
                c = (char)n;

                if (c == '\r')
                {
                    break;
                }
                else
                {
                    stringBuilder.Append(c);
                    reader.Read();
                    currentOffset++;
                }
            }

            _length = stringBuilder.Length;
            _name = stringBuilder.ToString();

            Update(this);
        }
    }

    partial class LuaClass
    {
        public override void Serialize(StringBuilder stringBuilder)
        {
            stringBuilder.Append(StartEndOperators.Item1);
            stringBuilder.Append(" \"");
            stringBuilder.Append(_name);
            stringBuilder.Append("\" (");
            if(_parent != null)
                stringBuilder.Append(_parent);
            stringBuilder.Append(')');

            if (_comment != null)
            {
                stringBuilder.Length = _comment.Offset;
                _comment.Serialize(stringBuilder);
            }
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
                else
                    stringBuilder.Length = node.Offset;
                node.Serialize(stringBuilder);
                lastLineIndex = node.EndLineIndex;
            }
        }

        public override void Deserialize(ReadingContext reader, bool _skipStartOperator = false)
        {
            List<TextNode> listNodes;
            // Оптимизация по выделению памяти
            int initCapacity = _nodes?.Count ?? 0;
            if (initCapacity == 0)
                listNodes = new List<TextNode>();
            else
                listNodes = new List<TextNode>(initCapacity);

            int n;
            char c = '\0';
            int currentOffset = _offset;
            string commentStart = "--";
            char[] nonWordSymbols = { ' ', '\0', '\t', '=', ',', '(', ')', '\r', '\n' };
            StringBuilder expressionBuilder = new StringBuilder();

            if (!_skipStartOperator)
            {
                while ((n = reader.Peek()) != -1)
                {
                    c = (char)n;

                    if (c == StartEndOperators.Item1[0])
                    {
                        //ParsingHelper.ReadBlockUpTo(reader, expressionBuilder, nonWordSymbols);
                        reader.ReadTo(expressionBuilder, nonWordSymbols);
                        if (expressionBuilder.ToString() == StartEndOperators.Item1)
                        {
                            currentOffset += expressionBuilder.Length;
                            expressionBuilder.Length = 0;
                        }
                        else
                            throw new SerializationException(_endLineIndex, this, true);
                    }
                }
            }

            while ((n = reader.Peek()) != -1)
            {
                c = (char)n;

                if (c == '\r')
                    break;
                else if (c == '\"')
                {
                    reader.Read();
                    currentOffset++;

                    //ParsingHelper.ReadBlockUpTo(reader, expressionBuilder, new char[] { '\"' });
                    reader.ReadTo(expressionBuilder, '\"');
                    _name = expressionBuilder.ToString();
                    currentOffset += expressionBuilder.Length;
                    expressionBuilder.Length = 0;
                    reader.Read();
                    currentOffset++;
                }
                else if (c == '(')
                {
                    reader.Read();
                    currentOffset++;
                    //ParsingHelper.ReadBlockUpTo(reader, expressionBuilder, new char[] { ')' });
                    reader.ReadTo(expressionBuilder, ')');
                    _parent = expressionBuilder.ToString();
                    currentOffset += expressionBuilder.Length;
                    expressionBuilder.Length = 0;
                }
                else if (c == commentStart[0])
                {
                    reader.Read();
                    if ((n = reader.Peek()) == -1)
                        break;
                    c = (char)n;

                    currentOffset++;

                    if (c == commentStart[1])
                    {
                        reader.Read();
                        currentOffset++;

                        TextComment last = new TextComment(this, currentOffset, _endLineIndex, LuaModel.CommentOperators);
                        try
                        {
                            (last as TextComment).Deserialize(reader, true);
                        }
                        catch (Exception exception)
                        {
                            ExceptionPush(new SerializationException("Ошибка при десериализации коментария", exception, _endLineIndex, this, true));
                            break;
                        }
                        listNodes.Add(last);
                        currentOffset += last.Length - LuaModel.CommentOperators.Item1.Length;
                        _endLineIndex = last.EndLineIndex;
                    }
                    else
                    {
                        ExceptionPush(new SerializationException("Неизвестная конструкция", _endLineIndex, this, true));
                        break;
                    }
                }
                else
                {
                    reader.Read();
                    currentOffset++;
                }
            }

            _length = currentOffset - _offset;

            Update(this);
        }
    }
}
