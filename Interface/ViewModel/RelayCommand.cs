using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Media;

namespace Stalker_Studio.ViewModel
{
	public class RelayCommand : ICommand
	{
		readonly Action<object> _execute;
		readonly Predicate<object> _canExecute;

		public RelayCommand(Action<object> execute) : this(execute, null) {}

		public RelayCommand(Action<object> execute, Predicate<object> canExecute)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			return _canExecute?.Invoke(parameter) ?? true;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object parameter)
		{
			_execute(parameter);
		}
	}

    /// <summary>
    /// Расширенная команда
    /// </summary>
    public class ExtendedRelayCommand : RelayCommand
    {
        private string _name = null;
        private object _icon = null;
        private string _toolTip = null;

        public ExtendedRelayCommand(Action<object> execute) : this(execute, null) { }
        public ExtendedRelayCommand(Action<object> execute, Predicate<object> canExecute, string name = null, string toolTip = null, object iconOrKey = null) : base(execute, canExecute)
        {
            if (iconOrKey != null)
            {
                if (iconOrKey is string)
                    _icon = App.Current.Resources[iconOrKey];
                else
                    _icon = iconOrKey;
            }
            _toolTip = toolTip;
            _name = name;
        }

        /// <summary>
        /// Иконка
        /// </summary>
        public object Icon { get { return _icon; } }
        /// <summary>
        /// Текст всплывающей посказки
        /// </summary>
        public string ToolTip { get { return _toolTip; } }
        /// <summary>
        /// Имя, текст
        /// </summary>
        public string Name { get { return _name; } }
    }

    /// <summary>
    /// Команда переключатель
    /// </summary>
    public class ToggleRelayCommand : ExtendedRelayCommand
    {
        private ViewModelBase _owner = null;
        private string _isCheckedBindPath = ".";
        private string _toggleOnText = "Выключить";
        private string _toggleOffText = "Включить";

        public ToggleRelayCommand(Action<object> execute) : this(execute, null) { }
        public ToggleRelayCommand(Action<object> execute, Predicate<object> canExecute, string name = "", string toolTip = "", object iconOrKey = null) 
            : base(execute, canExecute, name, toolTip, iconOrKey) { }
        public ToggleRelayCommand(ViewModelBase owner, Action<object> execute, Predicate<object> canExecute, string toolTip = "",  object iconOrKey = null, string isCheckedBindPath = "", string toggleOnText = "", string toggleOffText = "", string name = "")
            : base(execute, canExecute, name, toolTip, iconOrKey) 
        {
            _owner = owner;
            _isCheckedBindPath = isCheckedBindPath;
            _toggleOnText = toggleOnText;
            _toggleOffText = toggleOffText;
        }

        /// <summary>
        /// Владелец команды, используется для биндинга 
        /// </summary>
        public ViewModelBase Owner { get { return _owner; } set { _owner = value; } }
        /// <summary>
        /// Путь к свойству состояния переключения (IsChecked) относительно владельца команды (Owner)
        /// </summary>
        public string IsCheckedBindPath { get { return _isCheckedBindPath; } set { _isCheckedBindPath = value; } }
        /// <summary>
        /// Текст команды когда включена (IsChecked == true)
        /// </summary>
        public string ToggleOnText { get => _toggleOnText; set => _toggleOnText = value; }
        /// <summary>
        /// Текст команды когда выключена (IsChecked == false)
        /// </summary>
        public string ToggleOffText { get => _toggleOffText; set => _toggleOffText = value; }

        public override string ToString()
        {
            return _toggleOnText + '\\' + _toggleOffText;
        }
    }
}
