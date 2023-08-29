using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using Stalker_Studio.Common;
using System.Xml.Linq;

namespace Stalker_Studio.StalkerClass
{
    /// <summary>
    /// Класс работы с Lua сриптовым файлом 
    /// </summary>
    public partial class LuaModel : ObjectTextFileModel
    {
        static readonly Type[] _nodeTypes = new Type[]
        {
            typeof(LuaFunction),
            typeof(LuaVariable),
            typeof(LuaClass),
            typeof(TextLineBreak),
            typeof(TextInclude),
            typeof(TextComment),
            typeof(TextTabulation)
        };
        public static readonly Tuple<string, string> CommentOperators = new Tuple<string, string>("--", null);
        public static readonly Tuple<string, string> CommentMultiLineOperators = new Tuple<string, string>("--[[", "]]");

        private ObservableCollection<LuaFunction> _functions = null;
        private ObservableCollection<LuaClass> _classes = null;
        private ObservableCollection<LuaVariable> _variables = null;

        public LuaModel() : base(MainWindow.ProgramData.Encoding_Script)
        {
            Initialize();
        }
        public LuaModel(FileInfo file) : base(file, MainWindow.ProgramData.Encoding_Script)
        {
            Initialize();
        }
        public LuaModel(string fullName) : base(fullName, MainWindow.ProgramData.Encoding_Script)
        {
            Initialize();
        }

        [
            System.ComponentModel.Category("Секции"),
            System.ComponentModel.DisplayName("Секции"),
            PropertyTools.DataAnnotations.HeaderPlacement(PropertyTools.DataAnnotations.HeaderPlacement.Collapsed),
            PropertyTools.DataAnnotations.SortIndex(100)
        ]
        public ObservableCollection<LuaFunction> Functions
        {
            get
            {
                return _functions;
            }
        }
        [System.ComponentModel.Browsable(false)]
        public ObservableCollection<LuaClass> Classes
        {
            get 
            {
                return _classes;
            }
        }
        [System.ComponentModel.Browsable(false)]
        public ObservableCollection<LuaVariable> Variables
        {
            get
            {
                return _variables;
            }
        }
        protected override void OnSave()
        {
            StreamWriter streamWriter = new StreamWriter(_fullName, false, _encoding);
            streamWriter.Write(Serialize());
            streamWriter.Close();
        }

        #region Определения Hierarchical

        public override IEnumerable<Type> GetNodeTypes() => _nodeTypes;

        public override IHierarchical this[int index]
        {
            get { return Nodes.ElementAt(index); }
            set
            {
                CheckNodeAndThrowException(value);

                _nodes[index] = value as LtxSection;
                OnPropertyChanged(nameof(Nodes));
            }
        }

        public override IEnumerable<IHierarchical> Nodes
        {
            get
            {
                if (!_isLoaded)
                    Load();
                return _nodes;
            }
        }

        protected override void OnAddingNode(IHierarchical node)
        {
            if (!_isLoaded)
                Load();
            _nodes.Add(node as LtxSection);
        }
        protected override void OnAddingNodeAt(IHierarchical node, int index)
        {
            if (!_isLoaded)
                Load();
            _nodes.Insert(index, node as LtxSection);
        }
        protected override void OnRemoveNode(IHierarchical node, bool recursively)
        {
            _nodes.Remove(node as LtxSection);
        }
        protected override void OnRemoveNodeAt(int index)
        {
            _nodes.RemoveAt(index);
        }

        #endregion

        public void SaveFile(string pathFile = null, Encoding enc = null, bool UseDescrHeir = false)
        {
            //string fls = "";
            //if (pathFile != null)
            //    fls = pathFile;
            //else
            //    throw new System.IO.IOException($"Stalker_Studio.StalkerClass.LtxModel - PathFile не указан!");

            //if (enc == null)
            //    enc = MainWindow.ProgramData.Encoding_LTX;

            //File.WriteAllText(fls, ConvertToStringParams(UseDescrHeir), enc);
        }

        void Initialize()
        {

            //Sections.CollectionChanged +=
            //    (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            //    => SerializationState = SerializationState.NeedSerialize;
        }
    }
    /// <summary>
    /// Функция или метод Lua скрипта 
    /// </summary>
    public partial class LuaFunction : TextBlockObject<TextNode>
    {
        static readonly Type[] _nodeTypes = new Type[]
        {
            typeof(TextLineBreak),
            typeof(LuaVariable),
            typeof(TextComment),
            typeof(TextTabulation)
        };
        public static readonly Tuple<string, string> StartEndOperators = new Tuple<string, string>("function", "end");

        protected string _text = null;
        protected ObservableCollection<string> _parameters = null;
        protected ObservableCollection<LuaVariable> _variables = null;

        public LuaFunction(ISerializable owner, int offset, int startLineIndex = -1, string name = null)
            : base(owner, offset, startLineIndex, name, StartEndOperators) { }
        public LuaFunction(ISerializable owner, int offset, int length, int startLineIndex, int endLineIndex, string name = null)
            : base(owner, offset, length, startLineIndex, endLineIndex, name, StartEndOperators) { }

        [
            System.ComponentModel.Category("Основные"),
            System.ComponentModel.DisplayName("Наследуется от")
        ]
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }
        [
            System.ComponentModel.Category("Параметры"),
            System.ComponentModel.DisplayName("Параметры"),
            PropertyTools.DataAnnotations.HeaderPlacement(PropertyTools.DataAnnotations.HeaderPlacement.Collapsed),
            PropertyTools.DataAnnotations.SortIndex(100)
        ]
        public ObservableCollection<string> Parameters
        {
            get
            {
                return _parameters;
            }
        }

        public override IEnumerable<Type> GetNodeTypes() => _nodeTypes;
    }
    /// <summary>
    /// Переменная Lua скрипта 
    /// </summary>
    public partial class LuaVariable : TextObject<TextNode>
    {
        static readonly Type[] _nodeTypes = new Type[]
        {
            typeof(TextComment),
            typeof(TextTabulation)
        };

        string _operator = null;// например local
        string _value = null;
        string _equalSpace = null;

        public LuaVariable(ISerializable owner, int offset, int startLineIndex = -1) : base(owner, offset, startLineIndex) { }

        [
            System.ComponentModel.Category("Основные"),
            System.ComponentModel.DisplayName("Значение")
        ]
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
        [
            System.ComponentModel.Category("Основные"),
            System.ComponentModel.DisplayName("Оператор")
        ]
        public string Operator
        {
            get => _operator;
            set
            {
                _operator = value;
                OnPropertyChanged();
            }
        }

        public override string ToString()
        {
            if (_name == null)
                return _value;
            return Name;
        }

        public override IEnumerable<Type> GetNodeTypes() => _nodeTypes;
    }
    /// <summary>
    /// Класс Lua скрипта 
    /// </summary>
    public partial class LuaClass : TextBlockObject<TextNode>
    {
        static readonly Type[] _nodeTypes = new Type[]
        {
            typeof(LuaVariable),
            typeof(LuaFunction),
            typeof(TextLineBreak),
            typeof(TextComment),
            typeof(TextTabulation)
        };
        public static readonly Tuple<string, string> StartEndOperators = new Tuple<string, string>("class", null);

        protected string _parent = null;
        protected ObservableCollection<LuaFunction> _functions = null;
        protected ObservableCollection<LuaVariable> _variables = null;

        public LuaClass(ISerializable owner, int offset, int startLineIndex = -1, string name = null, string parent = null)
            : base(owner, offset, startLineIndex, name, StartEndOperators)
        {
            _parent = parent;
        }
        public LuaClass(ISerializable owner, int offset, int length, int startLineIndex, int endLineIndex, string name = null, string parent = null)
            : base(owner, offset, length, startLineIndex, endLineIndex, name, StartEndOperators)
        {
            _parent = parent;
        }

        [
            System.ComponentModel.Category("Основные"),
            System.ComponentModel.DisplayName("Наследуется от")
        ]
        public string Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                OnPropertyChanged();
            }
        }
        [
            System.ComponentModel.Category("Параметры"),
            System.ComponentModel.DisplayName("Параметры"),
            PropertyTools.DataAnnotations.HeaderPlacement(PropertyTools.DataAnnotations.HeaderPlacement.Collapsed),
            PropertyTools.DataAnnotations.SortIndex(100)
        ]
        public ObservableCollection<LuaFunction> Methods
        {
            get
            {
                return _functions;
            }
        }
        public ObservableCollection<LuaVariable> Variables
        {
            get
            {
                return _variables;
            }
        }

        public override IEnumerable<Type> GetNodeTypes() => _nodeTypes;
    }
}
