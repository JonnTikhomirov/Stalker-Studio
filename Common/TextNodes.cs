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
	/// <summary>
	/// Элемент, область, часть текста
	/// </summary>
	public class TextNode : Hierarchical, ISerializable
    {
		protected ISerializable _owner = null;
		protected int _offset = -1;
		protected int _length = 0;
		protected int _startLineIndex = -1;
		protected int _endLineIndex = -1;
		
		public TextNode(ISerializable owner, int offset, int startLineIndex = 0) 
		{
			_owner = owner;
			_offset = offset;
			_startLineIndex = _endLineIndex = startLineIndex;
		}
		public TextNode(ISerializable owner, int offset, int length, int startLineIndex, int endLineIndex = -1)
        {
			_owner = owner;
			_offset = offset;
			_length = length;
			_startLineIndex = startLineIndex;
			if(endLineIndex == -1)
				_endLineIndex = startLineIndex;
			else
				_endLineIndex = endLineIndex;
        }
		/// <summary>
		/// Владелец
		/// </summary>
		[
			System.ComponentModel.DisplayName("Владелец"),
			System.ComponentModel.Description("Объект, которому принадлежит данный элемент")
		]
		public ISerializable Owner
		{
			get => _owner;
		}
		/// <summary>
		/// Индекс c которого начинается элемент в тексте владельца (для текста сплошной строкой)
		/// </summary>
		public int Offset
		{
			get => _offset;
			set
			{
				_offset = value;
				OnPropertyChanged();
			}
		}
		/// <summary>
		/// Общее количество всех символов
		/// </summary>
		[
			System.ComponentModel.DisplayName("Символов"),
			System.ComponentModel.Description("Общее количество всех символов")
		]
		public int Length
		{
			get => _length;
			protected set
			{
				_length = value;
				OnPropertyChanged();
			}
		}
		/// <summary>
		/// Индекс строки c которой начинается элемент в тексте
		/// </summary>
		[
			System.ComponentModel.DisplayName("Начальная строка"),
			System.ComponentModel.Description("Индекс строки c которой начинается элемент в тексте")
		]
		public int StartLineIndex
		{
			get => _startLineIndex;
			set
			{
				_startLineIndex = value;
				OnPropertyChanged();
			}
		}
		/// <summary>
		/// Индекс строки в которой заканчивается элемент в тексте
		/// </summary>
		[
			System.ComponentModel.DisplayName("Конечная строка"),
			System.ComponentModel.Description("Индекс строки в которой заканчивается элемент в тексте")
		]
		public int EndLineIndex
		{
			get => _endLineIndex;
			set
			{
				_endLineIndex = value;
				OnPropertyChanged();
			}
		}
        /// <summary>
        /// Ищет объект, который является источником текста для данного элемента. Например TextFileModel или TextNode без владельца
        /// </summary>
        public virtual ISerializable GetTextHost()
		{
			if (_owner == null)
				return this;
			else if (!(_owner is TextNode))
				return _owner;
			else
				return (_owner as TextNode).GetTextHost();
        }

        #region Определения ISerializable

        public virtual string Serialize()
		{
			StringBuilder stringBuilder = new StringBuilder(_offset + _length + 1);
			Serialize(stringBuilder);
			stringBuilder.Replace('\0', ' ');
			return stringBuilder.ToString();
		}

		public virtual void Serialize(StringBuilder stringBuilder) { }

        public virtual void Deserialize(string text) 
		{
			Deserialize(new ReadingContext(text));
		}

        public virtual void Deserialize(ReadingContext reader) { }

        public virtual bool ExceptionPush(SerializationException exception)
        {
			_owner.ExceptionPush(exception);
			return false;
		}

        public void Update(ISerializable source)
		{
			_owner.Update(this);
		}

        #endregion

        #region Определения Hierarchical

        // в элементе текста не бывает структуры, то есть нет Nodes

        public override IEnumerable<Type> GetNodeTypes() { return new Type[0]; }

		public override IHierarchical this[int index]
		{
			get { return null; }
			set { }
		}
		public override IEnumerable<IHierarchical> Nodes
		{
			get { return null; }
			set { }
		}

        protected override void OnAddingNode(IHierarchical node) { }
		protected override void OnAddingNodeAt(IHierarchical node, int index) { }
		protected override void OnRemoveNode(IHierarchical node, bool recursively) { }
		protected override void OnRemoveNodeAt(int index) { }

        #endregion
    }
	/// <summary>
	/// Элемент переноса строки, или пустой стркои
	/// </summary>
	public partial class TextLineBreak : TextNode
	{
		string _lineBreakSymbols = null;

		public TextLineBreak(ISerializable owner, int offset, int lineIndex, string lineBreakSymbols = "\r\n")
			: base(owner, offset, lineIndex) 
		{
			LineBreakSymbols = lineBreakSymbols;

			if (lineBreakSymbols != null)
				_length = lineBreakSymbols.Length;
		}
		public TextLineBreak(ISerializable owner, int offset, int length, int lineIndex)
			: base(owner, offset, length, lineIndex, lineIndex) { }

		/// <summary>
		/// Символы переноса строки
		/// </summary>
		[
			System.ComponentModel.DisplayName("Символы переноса строки")
		]
		public string LineBreakSymbols
		{ 
			get => _lineBreakSymbols;
			set 
			{
				if (_lineBreakSymbols == value)
					return;
				_lineBreakSymbols = value;
				Length = _lineBreakSymbols.Length;
				OnPropertyChanged();
			} 
		}

		public override string ToString()
		{
			return "<Перенос строки>";
		}
	}

    /// <summary>
    /// Элемент, область, часть текста, имеющая открывающий и закрывающий тег
    /// </summary>
    public class TextBlock : TextNode
    {
		protected Tuple<string, string> _startEnd;

        public TextBlock(ISerializable owner, int offset, int startLineIndex = -1, Tuple<string, string> startEnd = null)
            : base(owner, offset, startLineIndex) 
		{
			_startEnd = startEnd;
        }
        public TextBlock(ISerializable owner, int offset, int length, int startLineIndex, int endLineIndex, Tuple<string, string> startEnd = null)
            : base(owner, offset, length, startLineIndex, endLineIndex) 
		{
            _startEnd = startEnd;
        }

        [
			System.ComponentModel.DisplayName("Операторы начала и окончания"),
			System.ComponentModel.Description("Операторы, которые обозначают начало или конец элемента")
		]
        public Tuple<string, string> StartEnd 
		{ 
			get => _startEnd; 
		}

        public override void Deserialize(ReadingContext reader) 
		{
			Deserialize(reader, false);
        }
        /// <summary>
        /// Десериализация с возможностью пропустить стартовый оператор блока
        /// </summary>
        public virtual void Deserialize(ReadingContext reader, bool _skipStartOperator = false) { }
    }

    /// <summary>
    /// Элемент комментария в тексте
    /// </summary>
    public partial class TextComment : TextBlock
    {
		string _text = null;

		public TextComment(ISerializable owner, int offset, int startLineIndex = -1, Tuple<string, string> startEnd = null)
			: base(owner, offset, startLineIndex, startEnd) { }
		public TextComment(ISerializable owner, int offset, int length, int startLineIndex, int endLineIndex, Tuple<string, string> startEnd = null)
			: base(owner, offset, length, startLineIndex, endLineIndex, startEnd) { }

        [
			System.ComponentModel.Category("Основные"),
			System.ComponentModel.DisplayName("Текст"),
			System.ComponentModel.Description("Текст комментария")
		]
        public string Text 
		{ 
			get => _text;
			set 
			{
				if (_text == value)
					return;

				_text = value;
				OnPropertyChanged();

                if (value == null)
                    Length = 0;
                else if (_startEnd != null)
				{
					if (_startEnd.Item2 == null)
						Length = _text.Length + _startEnd.Item1.Length;
					else
						Length = _text.Length + _startEnd.Item1.Length + _startEnd.Item2.Length;
				}
				else
					Length = _text.Length;
            }
		}

		public override string ToString()
		{
			string text = _text.Substring(0, _text.Length > 15 ? 15 : _text.Length);
			return "Комментарий: " + text + "...";
		}
	}
	/// <summary>
	/// Элемент include в тексте
	/// </summary>
	public partial class TextInclude : TextBlock
    {
		string _parameter = null;

        public TextInclude(ISerializable owner, int offset, int startLineIndex = -1, Tuple<string, string> startEnd = null)
            : base(owner, offset, startLineIndex, startEnd) { }
        public TextInclude(ISerializable owner, int offset, int length, int startLineIndex, int endLineIndex, Tuple<string, string> startEnd = null)
            : base(owner, offset, length, startLineIndex, endLineIndex, startEnd) { }

		[
			System.ComponentModel.Category("Основные"),
			System.ComponentModel.DisplayName("Путь к файлу"),
			System.ComponentModel.Description("Параметр дериктивы")
		]
		public string Parameter
		{
			get => _parameter;
			set
			{
				if (_parameter == value)
					return;

				_parameter = value;
				OnPropertyChanged();

				if (value == null)
					Length = 0;
				else if (_startEnd != null)
				{
					if (_startEnd.Item2 == null)
						Length = _parameter.Length + _startEnd.Item1.Length;
					else
						Length = _parameter.Length + _startEnd.Item1.Length + _startEnd.Item2.Length;
				}
				else
					Length = _parameter.Length;
            }
		}

		public override string ToString()
		{
			string text = _parameter.Substring(0, _parameter.Length > 15 ? 15 : _parameter.Length);
			return "include: " + text;
		}
	}
    /// <summary>
    /// Элемент табуляции в тексте
    /// </summary>
    public partial class TextTabulation : TextNode
    {
        public TextTabulation(ISerializable owner, int offset, int startLineIndex = -1)
            : base(owner, offset, startLineIndex) { }
        public TextTabulation(ISerializable owner, int offset, int length, int startLineIndex, int endLineIndex)
            : base(owner, offset, length, startLineIndex, endLineIndex) { }

        public override string ToString()
        {
			return $"Табуляция: {_length} симв.";
        }
    }
    /// <summary>
    /// Элемент, область, часть текста, описывающая объект
    /// </summary>
    public class TextObject : TextNode, INammed
	{
		protected string _name = null;
		protected TextComment _comment = null;

		public TextObject(ISerializable owner, int offset, int startLineIndex = -1, string name = null) 
			: base(owner, offset, startLineIndex) 
		{
			_name = name;
		}
		public TextObject(ISerializable owner, int offset, int length, int startLineIndex, int endLineIndex, string name = null) 
			: base(owner, offset, length, startLineIndex, endLineIndex)
		{
			_name = name;
		}
		/// <summary>
		/// Имя объекта
		/// </summary>
		[
			System.ComponentModel.Category("Основные"),
			System.ComponentModel.DisplayName("Имя"),
			System.ComponentModel.Description("Имя или ключ объекта"),
			PropertyTools.DataAnnotations.SortIndex(-1)
		]
		public string Name 
		{
			get => _name;
			set 
			{
				_name = value;
				OnPropertyChanged(null);
			} 
		}
		/// <summary>
		/// Комментарий, связанный с объектом
		/// </summary>
		[
			System.ComponentModel.Category("Основные"),
			System.ComponentModel.DisplayName("Комментарий"),
			System.ComponentModel.Description("Комментарий, связанный с объектом в тексте")
		]
		public TextNode Comment { get =>_comment; }

		public override string ToString()
		{
			return Name;
		}
	}

	/// <summary>
	/// Элемент, область, часть текста, описывающая объект с подчиненными элементами типа TNode
	/// </summary>
	public class TextObject<TNode> : TextObject
		where TNode : IHierarchical
	{
		protected ObservableCollection<TNode> _nodes = null;

		public TextObject(ISerializable owner, int offset, int startLineIndex = 0, string name = null, IEnumerable<TNode> nodes = null) 
			: base(owner, offset, startLineIndex, name)
		{
			if(nodes != null)
				_nodes = new ObservableCollection<TNode>(nodes);
		}
		public TextObject(ISerializable owner, int offset, int length, int startLineIndex, int endLineIndex, string name = null, IEnumerable<TNode> nodes = null) 
			: base(owner, offset, length, startLineIndex, endLineIndex, name)
		{
			if (nodes != null)
				_nodes = new ObservableCollection<TNode>(nodes);
		}

		#region Определения Hierarchical

		public override IEnumerable<Type> GetNodeTypes() { return new Type[] { typeof(TNode) }; }

		public override IHierarchical this[int index]
		{
			get => Nodes.ElementAt(index); 
			set
			{
				CheckNodeAndThrowException(value);

				_nodes[index] = (TNode)value;
				OnPropertyChanged("Nodes");
			}
		}
		public override IEnumerable<IHierarchical> Nodes
		{
			get => _nodes as IEnumerable<IHierarchical>;
		}

		protected override void OnAddingNode(IHierarchical node)
		{
			_nodes.Add((TNode)node);
		}
		protected override void OnAddingNodeAt(IHierarchical node, int index)
		{
			_nodes.Insert(index, (TNode)node);
		}
		protected override void OnRemoveNode(IHierarchical node, bool recursively)
		{
			_nodes.Remove((TNode)node);
		}
		protected override void OnRemoveNodeAt(int index)
		{
			_nodes.RemoveAt(index);
		}

		#endregion
	}

    /// <summary>
    /// Элемент, область, часть текста, имеющая открывающий и закрывающий тег, описывающая объект, с подчиненными элементами типа TNode
    /// </summary>
    public class TextBlockObject<TNode> : TextObject<TNode>
        where TNode : IHierarchical
    {
        protected Tuple<string, string> _startEnd;

        public TextBlockObject(ISerializable owner, int offset, int startLineIndex = 0, string name = null, Tuple<string, string> startEnd = null)
            : base(owner, offset, startLineIndex, name)
        {
            _startEnd = startEnd;
        }
        public TextBlockObject(ISerializable owner, int offset, int length, int startLineIndex, int endLineIndex, string name = null, Tuple<string, string> startEnd = null)
            : base(owner, offset, length, startLineIndex, endLineIndex, name)
        {
            _startEnd = startEnd;
        }

        /// <summary>
        /// Начальное слово и конечное слово 
        /// </summary>
        public Tuple<string, string> StartEnd { get => _startEnd; }

        public override void Deserialize(ReadingContext reader)
        {
            Deserialize(reader, false);
        }
        /// <summary>
        /// Десериализация с возможностью пропустить стартовый оператор блока
        /// </summary>
        public virtual void Deserialize(ReadingContext reader, bool _skipStartOperator = false) { }
    }
}
