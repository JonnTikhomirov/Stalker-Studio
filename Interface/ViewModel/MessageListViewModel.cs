using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Stalker_Studio.Common;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.Remoting.Messaging;

namespace Stalker_Studio.ViewModel
{
    /// <summary>
    /// Категория сообщения 
    /// </summary>
    public enum ErrorCategory
    {
        /// <summary>
        /// Информация 
        /// </summary>
        Info,
        /// <summary>
        /// Предупреждение
        /// </summary>
        Warning,
        /// <summary>
        /// Ошибка
        /// </summary>
        Error
    }

    /// <summary>
    /// Сообщение
    /// </summary>
    class Message 
    {
        WeakReference _source = null;
        Type _sourceType = null;
        string _text = "";
        ErrorCategory _category = ErrorCategory.Info;
        DateTime _dateTime;
        WeakReference _file = null;

        public Message(object source, string text, ErrorCategory category = ErrorCategory.Info, FileModel file = null)
        {
            _source = new WeakReference(source);
            _sourceType = source.GetType();
            _text = text;
            _category = category;
            _dateTime = DateTime.Now;
            _file = new WeakReference(file);
            //DateTime.Now.ToString("HH:mm:ss");
        }

        public object Source { get => _source.Target; }
        public string Text { get => _text; }
        public Type SourceType { get => _sourceType; }
        public ErrorCategory Category { get => _category; }
        public DateTime DateTime { get => _dateTime; }
        public string TimeString { get => _dateTime.ToString("HH:mm:ss"); }
        public FileModel File { get => _file.Target as FileModel; }
    }

    class MessageListViewModel : ItemsViewModel
    {
        ObservableCollection<Message> _messages = new ObservableCollection<Message>();
        protected ExtendedRelayCommand _clearAllCommand = null;
        bool _categoryInfoVisible = true;
        bool _categoryWarningVisible = true;
        bool _categoryErrorVisible = true;
        CollectionViewSource _collectionView = null;

        public MessageListViewModel(string name) : base(name)
        {
            InitLocationName = "BottomAnchorablePane";
            ContentId = "MessageList";
            Initialize();
        }

        public override string Search
        {
            get => base.Search;
            set
            {
                base.Search = value;
                _collectionView?.View.Refresh();
            }
        }
        public ReadOnlyObservableCollection<Message> Messages
        { 
            get => new ReadOnlyObservableCollection<Message>(_messages);  
        }
        /// <summary>
        /// Команда удаления всех элементов
        /// </summary>
        public ExtendedRelayCommand ClearAllCommand
        {
            get
            {
                if (_clearAllCommand == null)
                    _clearAllCommand = new ExtendedRelayCommand((p) => OnClearAll(p), (p) => CanClearAll(p), "Очистить всё", "Удалить все сообщения", "icon_ClearWindowContent");
                return _clearAllCommand;
            }
        }

        public bool CategoryInfoVisible 
        { 
            get => _categoryInfoVisible;
            set
            {
                _categoryInfoVisible = value;
                _collectionView?.View.Refresh();
                OnPropertyChanged();
            }
        }
        public bool CategoryErrorVisible { 
            get => _categoryErrorVisible;
            set
            {
                _categoryErrorVisible = value;
                _collectionView?.View.Refresh();
                OnPropertyChanged();
            }
        }
        public bool CategoryWarningVisible { 
            get => _categoryWarningVisible;
            set
            {
                _categoryWarningVisible = value;
                _collectionView?.View.Refresh();
                OnPropertyChanged();
            }
        }
        public CollectionViewSource CollectionView
        {
            get
            {
                if (_collectionView == null)
                {
                    _collectionView = new CollectionViewSource();
                    _collectionView.Source = _messages;
                    _collectionView.LiveFilteringProperties.Add(nameof(Message.Category));
                    _collectionView.IsLiveFilteringRequested = true;
                    _collectionView.Filter += _collectionView_Filter;
                }
                return _collectionView;
            }
        }

        private void _collectionView_Filter(object sender, FilterEventArgs e)
        {
            if(!(e.Item is Message))
                return;
            var message = (Message)e.Item;
            e.Accepted = false;
            if (_categoryInfoVisible)
                e.Accepted = message.Category == ErrorCategory.Info;
            if (_categoryErrorVisible)
                e.Accepted = message.Category == ErrorCategory.Error || e.Accepted;
            if (_categoryWarningVisible)
                e.Accepted = message.Category == ErrorCategory.Warning || e.Accepted;

            if (_search != "")
                e.Accepted = message.Text.ToLower().Contains(_search) && e.Accepted;
        }

        /// <summary>
        /// Добавляет сообщение в список
        /// </summary>
        public void Add(Message error) 
        {
            _messages.Add(error);
        }
        /// <summary>
        /// Добавляет сообщение в список
        /// </summary>
        public void Add(object source, string text, ErrorCategory category = ErrorCategory.Info, FileModel file = null)
        {
            Add(new Message(source, text, category, file));
        }
        /// <summary>
        /// Очищает список по источнику
        /// </summary>
        public void Clear(object source)
        {
            Clear(x => x.Source == source);
        }
        /// <summary>
        /// Очищает список по файлу
        /// </summary>
        public void Clear(FileModel file)
        {
            Clear(x => x.File == file);
        }
        /// <summary>
        /// Очищает список по выражению
        /// </summary>
        public void Clear(Func<Message, bool> predicate)
        {
            IEnumerable<Message> items = _messages.Where(predicate);
            foreach (Message item in _messages)
                _messages.Remove(item);
        }
        /// <summary>
        /// Очищает весь список
        /// </summary>
        public void Clear()
        {
            _messages.Clear();  
        }

        private void Initialize()
        {
            Commands.Add(ClearAllCommand);
        }

        protected virtual bool CanClearAll(object parameter) 
        {
            return _messages.Count != 0;
        }
        protected virtual void OnClearAll(object parameter) 
        {
            Clear();
            OnPropertyChanged(nameof(Messages));
        }

        protected override bool CanOpenValue(object parameter)
        {
            return _selectedItem is Message;
        }
        protected override void OnOpenValue(object parameter)
        {
            if (_selectedItem is Message)
            {
                Message message = (Message) _selectedItem;

                if (message.File != null)
                {
                    FileViewModel fileVM = Workspace.This.Open(message.File);

                    if (message.Source is TextNode)
                    {
                        (fileVM as TextFileViewModel).SelectNode(message.Source as TextNode);
                    }
                }
            }
        }

        protected override bool CanRemove(object parameter)
        {
            return _selectedItem is Message;
        }
        protected override void OnRemove(object parameter)
        {
            _messages.Remove(_selectedItem as Message);
        }
    }
}
