using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Stalker_Studio.ViewModel
{
    internal partial class Workspace : ViewModelBase
	{
		private ExtendedRelayCommand _openFileCommand = null;
		private ExtendedRelayCommand _closePaneCommand = null;
		private ExtendedRelayCommand _openToolCommand = null;
		private ExtendedRelayCommand _openGamedataCommand = null;
		private ExtendedRelayCommand _newCommand = null;
        private ExtendedRelayCommand _saveAllCommand = null;

        public ExtendedRelayCommand OpenFileCommand
		{
			get
			{
				if (_openFileCommand == null)
					_openFileCommand = new ExtendedRelayCommand((p) => OnOpen(p), (p) => CanOpen(p), "Открыть файл", "Открыть файл", "icon_OpenFile");
				return _openFileCommand;
			}
		}
		public ExtendedRelayCommand ClosePaneCommand
		{
			get
			{
				if (_closePaneCommand == null)
					_closePaneCommand = new ExtendedRelayCommand((p) => OnClosePane(p), (p) => CanClosePane(p), "Закрыть", null, "icon_Close");
				return _closePaneCommand;
			}
		}
		public ExtendedRelayCommand OpenToolCommand
		{
			get
			{
				if (_openToolCommand == null)
					_openToolCommand = new ExtendedRelayCommand((p) => OnOpenTool(p), (p) => CanOpenTool(p), "Открыть", null, "icon_Open");
				return _openToolCommand;
			}
		}
		public ExtendedRelayCommand OpenGamedataCommand
		{
			get
			{
				if (_openGamedataCommand == null)
					_openGamedataCommand = new ExtendedRelayCommand((p) => OnOpenGamedata(p), (p) => CanOpenGamedata(p), "Открыть gamedata", "Открыть gamedata", "icon_OpenProjectFolder");
				return _openGamedataCommand;
			}
		}
		public ExtendedRelayCommand NewCommand
		{
			get
			{
				if (_newCommand == null)
					_newCommand = new ExtendedRelayCommand((p) => OnNew(p), (p) => CanNew(p), "Создать", "Создать файл", "icon_NewFile");
				return _newCommand;
			}
		}
        public ExtendedRelayCommand SaveAllCommand
        {
            get
            {
                if (_saveAllCommand == null)
                    _saveAllCommand = new ExtendedRelayCommand((p) => OnSaveAll(p), (p) => CanSaveAll(p), "Сохранить все", "Сохранить все измененные", "icon_SaveAll");
                return _saveAllCommand;
            }
        }

        private bool CanOpen(object parameter) => true;

		private void OnOpen(object parameter)
		{
			if (parameter is string)
				if (parameter != null && parameter as string != "")
				{
					var fileViewModel = Open(parameter as string);
					ActiveDocument = fileViewModel;
					return;
				}

			var dlg = new OpenFileDialog();
			if (StalkerClass.GamedataManager.This.Root != null)
				dlg.InitialDirectory = StalkerClass.GamedataManager.This.Root.FullName;

			string filter = "";
			foreach (string extension in StalkerClass.GamedataManager.FileExtentions)
				filter += $"(*.{extension})|*.{extension}|";
			filter += "Все файлы (*.*)|*.*";
			dlg.Filter = filter;

			if (dlg.ShowDialog().GetValueOrDefault())
			{
				var fileViewModel = Open(dlg.FileName);
				ActiveDocument = fileViewModel;
			}

		}

		private bool CanClosePane(object parameter) => true;

		private void OnClosePane(object parameter)
		{

		}
		private bool CanOpenTool(object parameter)
		{
			return parameter != null;
		}

		private void OnOpenTool(object parameter)
		{
			if (!(parameter is ToolViewModel))
				return;
			_tools.Remove(parameter as ToolViewModel);
			_tools.Add(parameter as ToolViewModel);
			(parameter as ToolViewModel).IsSelected = true;
			(parameter as ToolViewModel).IsActive = true;
			(parameter as ToolViewModel).IsVisible = true;
		}
		private bool CanOpenGamedata(object parameter) => true;

		private void OnOpenGamedata(object parameter)
		{
			System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog();
			folder.Description = "Выберите путь к gamedata для работы";
			if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				MessageList.Clear();
				StalkerClass.GamedataManager.This.SetRootAtPath(folder.SelectedPath);
				This.Browser.Root = StalkerClass.GamedataManager.This.Root;
			}
		}

		private bool CanNew(object parameter)
		{
			return true;
		}

		private void OnNew(object parameter)
		{
			FileViewModel fileVM = null;
			if (parameter is string)
				fileVM = new FileViewModel(StalkerClass.GamedataManager.CreateFileSystemNodeFromExtension(parameter as string));
			else
				fileVM = new FileViewModel(StalkerClass.GamedataManager.CreateFileSystemNodeFromExtension("txt"));

			_files.Add(fileVM);
			ActiveDocument = _files.Last();
		}

        private bool CanSaveAll(object parameter)
        {
            return _files.Any(x => x.IsModified == true);
        }

        private void OnSaveAll(object parameter)
        {
			foreach (FileViewModel fileView in _files)
				Save(fileView);
        }
    }
}
