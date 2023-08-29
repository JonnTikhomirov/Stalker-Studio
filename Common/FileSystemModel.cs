using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace Stalker_Studio.Common
{

    /// <summary>
    /// Класс работы с элементами файловой системы
    /// </summary>
    public abstract class FileSystemNode : Hierarchical, INammed
    {
        protected string _fullName = "";
        protected bool _isLoaded = false;

        public FileSystemNode() : base() { }
        public FileSystemNode(string path) : base()
        {
            _fullName = path;
        }
        public FileSystemNode(FileSystemInfo file) : base()
        {
            _fullName = file.FullName;
        }

        // если Nodes возвращает null - значит документ не загружен
        public override IEnumerable<IHierarchical> Nodes
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// Возвращает полное имя
        /// </summary>
        [
            System.ComponentModel.DisplayName("Путь"),
            System.ComponentModel.Description("Путь на диске")
        ]
        public string FullName
        {
            get { return _fullName; }
            set
            {
                IEnumerable<IHierarchical> nodes = Nodes;
                if (nodes != null)
                {
                    foreach (IHierarchical node in nodes)
                    {
                        if (!(node is FileSystemNode))
                            continue;
                        FileSystemNode fileSystemNode = node as FileSystemNode;
                        fileSystemNode.FullName = fileSystemNode.FullName.Replace(_fullName, value);
                    }
                }
                _fullName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Name));
            }
        }
        /// <summary>
        /// Возвращает имя из полного имени
        /// </summary>
        [
            System.ComponentModel.DisplayName("Имя"),
            System.ComponentModel.Description("Имя объекта"),
            PropertyTools.DataAnnotations.SortIndex(-1)
        ]
        public virtual string Name
        {
            get {  return Path.GetFileNameWithoutExtension(_fullName); }
            set
            {
                string normalName = value.Replace(Path.DirectorySeparatorChar,' ');
                if(value.Length - normalName.Length > 1)
                    throw new IOException($"Ошибка установки имени {value}. Строка содержит недопустимые символы");

                int lastIndex = _fullName.LastIndexOf(Path.DirectorySeparatorChar);
                FullName = _fullName.Substring(0, lastIndex + 1) + value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Возвращает информацию
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public abstract FileSystemInfo Info { get; }
        /// <summary>
        /// Файл загружен
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set
            {
                _isLoaded = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Устанавливает FullName и вызывает Load()
        /// </summary>
        public void Load(string fullName)
        {
            FullName = fullName;
            Load();
        }
        /// <summary>
        /// Загрузить данные из файла в данный объект, FullName должен быть заполнен
        /// </summary>
        public virtual void Load() 
        {
            OnLoad();
            IsLoaded = true;
        }
        /// <summary>
        /// Сохранить данные из данного объекта в файл, FullName должен быть заполнен
        /// </summary>
        public virtual void Save() 
        {
            OnSave();
        }
        /// <summary>
        /// Обработчик загрузки данных из файла
        /// </summary>
        protected abstract void OnLoad();
        /// <summary>
        /// Обработчик сохранения данных из данного объекта в файл
        /// </summary>
        protected abstract void OnSave();

        public override string ToString() 
        {
            return Name;
        }
    }

    /// <summary>
    /// Класс работы с файлом
    /// </summary>
    public class FileModel : FileSystemNode
    {

        public FileModel() { }
        public FileModel(string fullName) : base(fullName) { }
        public FileModel(FileInfo file) : base(file) { }

        public override FileSystemInfo Info
        {
            get { return new FileInfo(_fullName); }
        }
        public override string Name
        {
            get { return base.Name; }
            set
            {
                string normalName = value.Replace(Path.DirectorySeparatorChar, ' ');
                if (value.Length - normalName.Length > 1)
                    throw new IOException($"Ошибка установки имени {value}. Строка содержит недопустимые символы");

                int lastIndex = _fullName.LastIndexOf(Path.DirectorySeparatorChar);
                FullName = _fullName.Substring(0, lastIndex + 1) + value + Extension;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Расширение из полного имени файла, c точкой
        /// </summary>
        [
            System.ComponentModel.DisplayName("Расширение"), 
            System.ComponentModel.Description("Расширение файла")
        ]
        public string Extension
        {
            get
            {
                int index = _fullName.LastIndexOf('.');
                if (index == -1)
                    return "";
                return _fullName.Substring(index);
            }
            set
            {
                int index = _fullName.LastIndexOf('.');
                if (index == -1)
                    FullName = _fullName + '.' + value;
                else
                    FullName = _fullName.Substring(0, index + 1) + value;
                OnPropertyChanged();
            }
        }

        protected override void OnLoad() { }
        protected override void OnSave() { }

        #region Определения Hierarchical

        // в файле неизвестного типа не бывает структуры, то есть нет Nodes

        public override IEnumerable<Type> GetNodeTypes() { return new Type[0]; }

        public override IHierarchical this[int index]
        {
            get { return null; }
            set { }
        }

        protected override void OnAddingNode(IHierarchical node) { }
        protected override void OnAddingNodeAt(IHierarchical node, int index) { }
        protected override void OnRemoveNode(IHierarchical node, bool recursively) { }
        protected override void OnRemoveNodeAt(int index) { }

        #endregion

        public override string ToString()
        {
            return Name + Extension;
        }
    }

    /// <summary>
    /// Класс работы с директорией
    /// </summary>
    public class DirectoryModel : FileSystemNode
    {
        protected ObservableCollection<FileSystemNode> _nodes = null;

        public DirectoryModel() { }
        public DirectoryModel(string fullName) : base(fullName) { }
        public DirectoryModel(DirectoryInfo directory) : base(directory) { }
        public DirectoryModel(FileSystemInfo directory) : base(directory) { }

        public override IEnumerable<IHierarchical> Nodes
        {
            get 
            {
                if (!_isLoaded)
                    Load();
                return _nodes;
            }
        }
        public override bool IsLast
        {
            get 
            {
                if (!_isLoaded)
                    Load(); 
                return _nodes == null || _nodes.Count == 0; 
            }
        }
        public override FileSystemInfo Info
        {
            get {
                if (_fullName != null && _fullName != "")
                    return new DirectoryInfo(_fullName);
                else
                    return null;
            }
        }

        protected override void OnLoad() 
        {
            _nodes = new ObservableCollection<FileSystemNode>();

            if (_fullName == null || _fullName == "")
                return;

            DirectoryInfo info = new DirectoryInfo(_fullName);
            FileSystemInfo[] files = info.GetFileSystemInfos();
            foreach (FileSystemInfo file in files)
            {
                if (file.Attributes.HasFlag(FileAttributes.Directory))
                    _nodes.Add(new DirectoryModel(file.FullName));
                else 
                {
                    FileModel fileObject = StalkerClass.GamedataManager.CreateFileSystemNodeFromExtension(file as FileInfo);
                    fileObject.Load();
                    _nodes.Add(fileObject);
                }
            }
            _isLoaded = true;
            OnPropertyChanged(nameof(Nodes));
        }

        protected override void OnSave() 
        { 

        }

        #region Определения Hierarchical

        public override IEnumerable<Type> GetNodeTypes() { return new Type[]{ typeof(FileSystemNode) }; }

        public override IHierarchical this[int index] { 
            get { return Nodes.ElementAt(index); } 
            set {
                CheckNodeAndThrowException(value);
                if (!_isLoaded)
                    Load();

                _nodes[index] = value as FileSystemNode;
                OnPropertyChanged(nameof(Nodes));
            } 
        }

        protected override void OnAddingNode(IHierarchical node) 
        {
            if (!_isLoaded)
                Load();
            _nodes.Add(node as FileSystemNode);
        }
        protected override void OnAddingNodeAt(IHierarchical node, int index)
        {
            if (!_isLoaded)
                Load();
            _nodes.Insert(index, node as FileSystemNode);
        }
        protected override void OnRemoveNode(IHierarchical node, bool recursively) 
        {
            _nodes.Remove(node as FileSystemNode);
        }
        protected override void OnRemoveNodeAt(int index)
        {
            _nodes.RemoveAt(index);
        }

        #endregion
    }
}
