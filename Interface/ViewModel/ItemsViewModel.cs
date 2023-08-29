using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stalker_Studio.ViewModel
{
    class ItemsViewModel : ToolViewModel
    {
        protected string _search = "";
        protected ObservableCollection<string> _searchHistory = new ObservableCollection<string>();
        protected ObservableCollection<ExtendedRelayCommand> _itemCommands = new ObservableCollection<ExtendedRelayCommand>();
        protected ExtendedRelayCommand _openValueCommand = null;
        protected ExtendedRelayCommand _addCommand = null;
        protected ExtendedRelayCommand _removeCommand = null;

        protected object _selectedItem = null;

        public ItemsViewModel(string name) : base(name)
        {
            Initialize();
        }

        /// <summary>
        /// Строка поиска по представлению элементов
        /// </summary>
        public virtual string Search
        {
            get
            {
                return _search;
            }
            set
            {
                if (_search == value)
                    return;

                value = value.ToLower();
                _search = value;

                int index = _searchHistory.IndexOf(value);
                if (index != -1)
                    _searchHistory.Move(index, 0);
                else
                    _searchHistory.Insert(0, value);

                OnPropertyChanged();
            }
        }
        /// <summary>
        /// История поиска
        /// </summary>
        public ReadOnlyObservableCollection<string> SearchHistory
        {
            get
            {
                return new ReadOnlyObservableCollection<string>(_searchHistory);
            }
        }
        /// <summary>
        /// Выбранный элемент
        /// </summary>
        public virtual object SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Команды, применимые для элементов
        /// </summary>
        public ObservableCollection<ExtendedRelayCommand> ItemCommands
        {
            get
            { return _itemCommands; }
        }
        /// <summary>
        /// Команда открытия элемента
        /// </summary>
        public ExtendedRelayCommand OpenValueCommand
        {
            get
            {
                if (_openValueCommand == null)
                    _openValueCommand = new ExtendedRelayCommand((p) => OnOpenValue(p), (p) => CanOpenValue(p), "Открыть", "Открыть элемент", "icon_Open");
                return _openValueCommand;
            }
        }
        /// <summary>
        /// Команда добавления элемента
        /// </summary>
        public ExtendedRelayCommand AddCommand
        {
            get
            {
                if (_addCommand == null)
                    _addCommand = new ExtendedRelayCommand((p) => OnAdd(p), (p) => CanAdd(p), "Добавить", "Добавить в выбранный элемент", "icon_NewFile");
                return _addCommand;
            }
        }
        /// <summary>
        /// Команда удаления элемента
        /// </summary>
        public ExtendedRelayCommand RemoveCommand
        {
            get
            {
                if (_removeCommand == null)
                    _removeCommand = new ExtendedRelayCommand((p) => OnRemove(p), (p) => CanRemove(p), "Удалить", "Удалить выбранный элемент", "icon_Remove");
                return _removeCommand;
            }
        }

        protected virtual bool CanOpenValue(object parameter) => parameter != null;
        protected virtual void OnOpenValue(object parameter) { }

        protected virtual bool CanAdd(object parameter) => true;
        protected virtual void OnAdd(object parameter) { }

        protected virtual bool CanRemove(object parameter) => parameter != null;
        protected virtual void OnRemove(object parameter) { }

        private void Initialize()
        {
            _itemCommands.Add(OpenValueCommand);
            _itemCommands.Add(RemoveCommand);
        }
    }
}
