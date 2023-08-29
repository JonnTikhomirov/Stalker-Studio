using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Stalker_Studio.Common;

namespace Stalker_Studio.ViewModel
{
	class FileViewModel : PaneViewModel
	{
		protected FileModel _file = null;
		private bool _isModified = false;
		private ExtendedRelayCommand _saveCommand = null;
		private ExtendedRelayCommand _saveAsCommand = null;

		public FileViewModel(FileModel file)
		{
			_file = file;
            _file.PropertyChanged += File_PropertyChanged;

			Initialize();
		}
		public FileViewModel(string fullName) : this(new FileModel(fullName)) { }

		#region Properties
		/// <summary>
		/// Заголовок
		/// </summary>
		public override string Title
        {
            get
            {
                if (_file.Name == null)
                    return "Noname" + (IsModified ? "*" : "");

                return _file.Name + _file.Extension + (IsModified ? "*" : "");
            }
        }
		/// <summary>
		/// True если файл был изменен
		/// </summary>
		public bool IsModified
		{
			get => _isModified;
			set
			{
				if (_isModified == value)
					return;
				_isModified = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(Title));
			}
		}
		/// <summary>
		/// Файл
		/// </summary>
		public FileModel File
		{
			get => _file;
			set
			{
				if (_file == value) return;
                _file.PropertyChanged -= File_PropertyChanged;
                _file = value;
                _file.PropertyChanged += File_PropertyChanged;
                OnPropertyChanged();
			}
		}
        /// <summary>
        /// Команда сохранения
        /// </summary>
        public ExtendedRelayCommand SaveCommand
		{
			get
			{
				if (_saveCommand == null)
					_saveCommand = new ExtendedRelayCommand((p) => OnSave(p), (p) => CanSave(p), "Сохранить", "Сохранить файл", "icon_Save");

				return _saveCommand;
			}
		}
        /// <summary>
        /// Команда сохранения с другим путем или форматом
        /// </summary>
        public ExtendedRelayCommand SaveAsCommand
		{
			get
			{
				if (_saveAsCommand == null)
					_saveAsCommand = new ExtendedRelayCommand((p) => OnSaveAs(p), (p) => CanSaveAs(p), "Сохранить как...", "Сохранить файл как...", "icon_SaveAs");

				return _saveAsCommand;
			}
		}

        #endregion  Properties

        #region Methods

        private void Initialize() 
		{
			_commands.Add(SaveCommand);
			_commands.Add(SaveAsCommand);
			_commands.Add(CloseCommand);
		}

		private void File_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			IsModified = true;
			OnPropertyChanged(nameof(File));
			OnPropertyChanged(nameof(Title));
		}

        /// <summary>
        /// Сохраняет файл, pathAs - путь для сохранения как, конвертирует данные в формат указанный в пути
        /// </summary>
        public void Save(string pathAs = null) 
		{
            IsModified = pathAs == null;
        }

        #endregion

		protected override void OnClose(object parameter) => Workspace.This.Close(this);

		protected virtual bool CanSave(object parameter) => IsModified;

		protected virtual void OnSave(object parameter) => Workspace.This.Save(this);

		protected virtual bool CanSaveAs(object parameter) => IsModified;

		protected virtual void OnSaveAs(object parameter) => Workspace.This.Save(this, true);
	}
}
