using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using Stalker_Studio.Common;

namespace Stalker_Studio.StalkerClass
{

    /// <summary>
    /// Класс работы с Ltx файлом 
    /// </summary>
    public partial class LtxModel : ObjectTextFileModel
    {
        static readonly Type[] _nodeTypes = new Type[]
            {
                typeof(LtxSection),
                typeof(TextLineBreak),
                typeof(LtxParameter),
                typeof(TextInclude),
                typeof(TextComment),
                typeof(TextTabulation)
            };
        
        private ObservableCollection<TextInclude> _includes = null;
        private ObservableCollection<LtxSection> _sections = null;

        public LtxModel() : base(MainWindow.ProgramData.Encoding_LTX)
        {
            Initialize();
        }
        public LtxModel(FileInfo file) : base(file, MainWindow.ProgramData.Encoding_LTX)
        {
            Initialize();
        }
        public LtxModel(string fullName) : base(fullName, MainWindow.ProgramData.Encoding_LTX)
        {
            Initialize();
        }

        [
            System.ComponentModel.Category("Секции"),
            System.ComponentModel.DisplayName("Секции"),
            PropertyTools.DataAnnotations.HeaderPlacement(PropertyTools.DataAnnotations.HeaderPlacement.Collapsed),
            PropertyTools.DataAnnotations.SortIndex(100)
        ]
        public ObservableCollection<LtxSection> Sections
        {
            get
            {
                return _sections;
            }
        }
        [System.ComponentModel.Browsable(false)]
        public ObservableCollection<TextInclude> Includes 
        {
            get 
            {
                return _includes;
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

                _nodes[index] = value as TextNode;
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

    public partial class LtxSection : TextObject<TextNode>
    {
        static readonly Type[] _nodeTypes = new Type[]
        {
            typeof(TextLineBreak),
            typeof(LtxParameter),
            typeof(TextComment),
            typeof(TextTabulation)
        };

        protected ObservableCollection<string> _parents = null;
        protected ObservableCollection<LtxParameter> _parameters = new ObservableCollection<LtxParameter>();

        public LtxSection(ISerializable owner, int offset, int startLineIndex = -1, string name = null, string[] parents = null)
            : base(owner, offset, startLineIndex, name)
        {
            if (parents != null)
                _parents = new ObservableCollection<string>(parents);
        }
        public LtxSection(ISerializable owner, int offset, int length, int startLineIndex, int endLineIndex, string name = null, string[] parents = null)
            : base(owner, offset, length, startLineIndex, endLineIndex, name)
        {
            if (parents != null)
                _parents = new ObservableCollection<string>(parents);
        }

        [System.ComponentModel.Browsable(false)]
        public bool IsHeir => _parents.Count != 0;

        [
            System.ComponentModel.Category("Основные"),
            System.ComponentModel.DisplayName("Наследуется от")
        ]
        public ObservableCollection<string> Parents
        {
            get { return _parents; }
            set
            {
                _parents = value;
                OnPropertyChanged();
            }
        }
        [
            System.ComponentModel.Category("Параметры"),
            System.ComponentModel.DisplayName("Параметры"),
            PropertyTools.DataAnnotations.HeaderPlacement(PropertyTools.DataAnnotations.HeaderPlacement.Collapsed),
            PropertyTools.DataAnnotations.SortIndex(100)
        ]
        public ObservableCollection<LtxParameter> Parameters
        {
            get
            {
                return _parameters;
            }
        }

        public override IEnumerable<Type> GetNodeTypes() => _nodeTypes;
    }

    public partial class LtxParameter : TextObject<TextNode>
    {
        static readonly Type[] _nodeTypes = new Type[]
        {
            typeof(TextComment),
            typeof(TextTabulation)
        };

        string _operator = null;// например bind
        string _value = null;
        string _equalSpace = null;

        public LtxParameter(ISerializable owner, int offset, int startLineIndex = -1) : base(owner, offset, startLineIndex) { }

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
        public string Operator { 
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
}
