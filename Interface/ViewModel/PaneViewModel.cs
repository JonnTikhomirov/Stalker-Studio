using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PropertyTools.DataAnnotations;

namespace Stalker_Studio.ViewModel
{
    /// <summary>
    /// Панель
    /// </summary>
    class PaneViewModel : ViewModelBase
	{
        protected string _title = null;
        protected string _contentId = null;
        protected bool _isSelected = false;
        protected bool _isActive = false;
        protected bool _isVisible = true;
        protected ImageSource _iconSource = null;
		protected FrameworkElement _mainControl = null;
        protected ExtendedRelayCommand _closeCommand = null;

		public PaneViewModel() { }

        #region Properties
        /// <summary>
        /// Заголовок
        /// </summary>
        public virtual string Title
		{
			get => _title;
			set
			{
				if (_title == value)
                    return;
                _title = value;
				OnPropertyChanged(nameof(Title));
			}
		}
        /// <summary>
        /// Зачем то надо, не знаю
        /// </summary>
        public string ContentId
		{
			get => _contentId;
			set
			{
				if (_contentId == value)
                    return;
                _contentId = value;
				OnPropertyChanged(nameof(ContentId));
			}
		}
        /// <summary>
        /// Выбрана ли панель
        /// </summary>
        public bool IsSelected
		{
			get => _isSelected;
			set
			{
				if (_isSelected == value)
                    return;
                _isSelected = value;
				OnPropertyChanged(nameof(IsSelected));
			}
		}
        /// <summary>
        /// Признак активности
        /// </summary>
        public bool IsActive
		{
			get => _isActive;
			set
			{
				if (_isActive == value)
                    return;
                _isActive = value;
				OnPropertyChanged(nameof(IsActive));
			}
		}
        /// <summary>
        /// Признак видимости
        /// </summary>
        public bool IsVisible
		{
			get => _isVisible;
			set
			{
				if (_isVisible == value)
					return;
				_isVisible = value;
				OnPropertyChanged(nameof(IsVisible));
			}
		}
        /// <summary>
        /// Иконка
        /// </summary>
        public ImageSource IconSource
		{
			get => _iconSource;
			set
			{
				_iconSource = value;
				OnPropertyChanged();
			}
		}
        /// <summary>
        /// Основной элемент интерфейса
        /// </summary>
        public virtual FrameworkElement MainControl
		{
			get => _mainControl;
			set
			{
				if (_mainControl == value)
					return;
				_mainControl = value;
				OnSetMainControl();
				OnPropertyChanged();
			}
		}
        /// <summary>
        /// Команда закрытия панели
        /// </summary>
        public ExtendedRelayCommand CloseCommand
		{
			get
			{
				if (_closeCommand == null)
					_closeCommand = new ExtendedRelayCommand((p) => OnClose(p), (p) => CanClose(p), "Закрыть", null, "icon_Close");
				return _closeCommand;
			}
		}

        #endregion Properties
        /// <summary>
        /// Вызывается при установке основного элемента интерфейса
        /// </summary>
        protected virtual void OnSetMainControl() { }

		private bool CanClose(object parameter) => true;

        protected virtual void OnClose(object parameter) { }
	}
}
