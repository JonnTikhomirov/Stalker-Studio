using AvalonDock.Themes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Stalker_Studio.Common;
using System.Windows.Media;

namespace Stalker_Studio.ViewModel
{
	internal partial class Workspace : ViewModelBase
	{
		static int _maxLastFiles = 10;

        private ObservableCollection<ToolViewModel> _tools = null;
		private ObservableCollection<string> _lastFiles = new ObservableCollection<string>();
		private ObservableCollection<FileViewModel> _files = new ObservableCollection<FileViewModel>();
		private ObservableCollection<FileModel> _fixedFiles = new ObservableCollection<FileModel>();

		private object _activeContent = null;
		private FileViewModel _activeDocument = null;
		private BrowserViewModel _browser = null;
		private PropertiesViewModel _propertiesTool = null;
		private MessageListViewModel _messageList = null;
		private Tuple<string, Theme> _selectedTheme;

		protected Workspace()
		{
			Initialize();
			LoadLastFiles();
        }

        public event EventHandler ActiveDocumentChanged;

        #region properties

        public static Workspace This { get; } = new Workspace();

		/// <summary>
		/// Текущие открытые файлы
		/// </summary>
		public ReadOnlyObservableCollection<FileViewModel> Files
		{
			get => new ReadOnlyObservableCollection<FileViewModel>(_files);
		}
		/// <summary>
		/// Текущие закрепленные файлы
		/// </summary>
		public ObservableCollection<FileModel> FixedFiles
		{
			get => _fixedFiles;
		}
		/// <summary>
		/// Инструменты
		/// </summary>
		public IEnumerable<ToolViewModel> Tools
		{
			get
			{
                if (_tools == null)
                    _tools = new ObservableCollection<ToolViewModel> { Browser, PropertiesTool, MessageList };
                return _tools;
			}
		}
		/// <summary>
		/// Текущий менеджер геймдаты (для привязки)
		/// </summary>
		public StalkerClass.GamedataManager Gamedata
		{
			get => StalkerClass.GamedataManager.This;
		}
		/// <summary>
		/// Последние файлы
		/// </summary>
		public ReadOnlyObservableCollection<string> LastFiles
		{
			get => new ReadOnlyObservableCollection<string>(_lastFiles);
		}
		/// <summary>
		/// Текущий файл
		/// </summary>
		public FileViewModel ActiveDocument
		{
			get => _activeDocument;
			set
			{
				if (_activeDocument != value)
				{
					_activeDocument = value;
					OnPropertyChanged();
					if (ActiveDocumentChanged != null)
						ActiveDocumentChanged(this, EventArgs.Empty);
				}
			}
		}
		/// <summary>
		/// Доступные темы
		/// </summary>
		public List<Tuple<string, Theme>> Themes { get; set; }
		/// <summary>
		/// Текущая тема
		/// </summary>
		public Tuple<string, Theme> SelectedTheme
		{
			get => _selectedTheme;
			set
			{
				_selectedTheme = value;
				OnPropertyChanged();
			}
		}
		/// <summary>
		/// Обозреватель
		/// </summary>
		public BrowserViewModel Browser
		{
			get
			{
				if (_browser == null)
				{
					_browser = new BrowserViewModel("Обозреватель", StalkerClass.GamedataManager.This.Root);
					_browser.FixedNodes = _fixedFiles;
					_browser.IconSource = App.Current.Resources["icon_Explorer"] as ImageSource;
					_browser.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
					{
						if (e.PropertyName == nameof(HierarchicalViewModel.SelectedItem))
							PropertiesTool.Source = (sender as HierarchicalViewModel).SelectedItem;
					};
					_browser.IsVisible = true;
				}
				return _browser;
			}
		}
		/// <summary>
		/// Обозреватель свойств
		/// </summary>
		public PropertiesViewModel PropertiesTool
		{
			get
			{
				if (_propertiesTool == null)
				{
					_propertiesTool = new PropertiesViewModel("Свойства");
					_propertiesTool.IconSource = App.Current.Resources["icon_Property"] as ImageSource;
					_propertiesTool.IsVisible = true;
				}
				return _propertiesTool;
			}
		}
		/// <summary>
		/// Список сообщений, ошибок
		/// </summary>
		public MessageListViewModel MessageList
		{
			get
			{
				if (_messageList == null)
				{
					_messageList = new MessageListViewModel("Сообщения");
					_messageList.IconSource = App.Current.Resources["icon_Log"] as ImageSource;
					_messageList.IsVisible = true;
				}
				return _messageList;
			}
		}
		public object ActiveContent 
		{ 
			get => _activeContent;
			set
			{
				if (_activeContent is PaneViewModel)
					(_activeContent as PaneViewModel).IsSelected = false;

				_activeContent = value;

				if (_activeContent is PaneViewModel)
					(_activeContent as PaneViewModel).IsSelected = true;

				OnPropertyChanged();
			}
		}
        #endregion properties

        #region methods

        private void Initialize()
        {
            Themes = new List<Tuple<string, Theme>>
            {
                new Tuple<string, Theme>(nameof(AvalonDockDarkTheme), new AvalonDockDarkTheme()),
            };
            _selectedTheme = Themes.First();

            _commands.Add(NewCommand);
            _commands.Add(OpenFileCommand);
            _commands.Add(OpenGamedataCommand);
            _commands.Add(SaveAllCommand);
        }
        /// <summary>
        /// Закрывает файл
        /// </summary>
        internal void Close(FileViewModel fileToClose)
		{
			if (fileToClose.IsModified)
			{
				var res = MessageBox.Show(string.Format("Сохранить изменения для файла '{0}'?", fileToClose.Title), "Сохранение файла", MessageBoxButton.YesNoCancel);
				if (res == MessageBoxResult.Cancel)
					return;
				if (res == MessageBoxResult.Yes)
                    fileToClose.Save();
			}

			_files.Remove(fileToClose);
		}
        /// <summary>
        /// Сохраняет  файл
        /// </summary>
        internal void Save(FileViewModel file, bool isSaveAs = false)
        {
			file.Save();
        }
        /// <summary>
        /// Сохраняет все измененные открытые файлы
        /// </summary>
        internal void SaveAll()
		{
			
		}
		/// <summary>
		/// Отркывает файл
		/// </summary>
		internal FileViewModel Open(string filepath)
		{
			FileViewModel fileViewModel = _files.SingleOrDefault(x => x.File.FullName == filepath);
			if (fileViewModel == default)
			{
				FileModel file = StalkerClass.GamedataManager.This.GetFileAtPath(filepath);
				if (file == default)
					file = StalkerClass.GamedataManager.CreateFileSystemNodeFromExtension(filepath);
				fileViewModel = Open(file);
			}
			return fileViewModel;
		}
		/// <summary>
		/// Отркывает файл
		/// </summary>
		internal FileViewModel Open(FileModel file)
		{
			FileViewModel fileViewModel = _files.SingleOrDefault(x => x.File == file);
			if (fileViewModel == default)
			{
				if (file is TextFileModel)
					fileViewModel = new TextFileViewModel(file as TextFileModel);
				else if (file is StalkerClass.OGFFile)
					fileViewModel = new OGFViewModel(file as StalkerClass.OGFFile);
                else if (file is StalkerClass.TextureModel)
                    fileViewModel = new TextureViewModel(file as StalkerClass.TextureModel);
                else
					fileViewModel = new FileViewModel(file);

				_files.Add(fileViewModel);
			}

			ActiveDocument = fileViewModel;
			ActiveContent = fileViewModel;
			AddLastFile(file.FullName);
			return fileViewModel;
		}
		/// <summary>
		/// Добавляет путь в последние файлы и сохраняет это в настройках
		/// </summary>
		internal void AddLastFile(string filepath)
		{
			_lastFiles.Remove(filepath);
			_lastFiles.Insert(0, filepath);
			if(_lastFiles.Count > _maxLastFiles)
                _lastFiles.RemoveAt(_lastFiles.Count - 1);

			SaveLastFiles();

            OnPropertyChanged(nameof(LastFiles));
		}
        /// <summary>
        /// Cохраняет последние файлы в настройках
        /// </summary>
        public void SaveLastFiles()
		{
            string lastOpenFiles = "";
            foreach (string file in _lastFiles)
                lastOpenFiles += file + ';';
            Properties.Settings.Default.LastOpenIndex = lastOpenFiles;
            Properties.Settings.Default.Save();
        }
        /// <summary>
        /// Загружает последние файлы из настроек
        /// </summary>
        public void LoadLastFiles()
        {
            string[] elements = Properties.Settings.Default.LastOpenIndex.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            int count = 0;
            foreach (string vEl in elements)
            {
                _lastFiles.Add(vEl);
                count++;
                if (count > _maxLastFiles)
                    break;
            }
        }

        #endregion methods
    }
}
