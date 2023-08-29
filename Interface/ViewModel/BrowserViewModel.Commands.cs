using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Stalker_Studio.Common;

namespace Stalker_Studio.ViewModel
{
    partial class BrowserViewModel : HierarchicalViewModel
    {
        ToggleRelayCommand _fixNodeCommand = null;
        ExtendedRelayCommand _renameNodeCommand = null;
        ExtendedRelayCommand _goToPropertiesCommand = null;
        ToggleRelayCommand _сhangeFixedNodesVisibilityCommand = null;
        ToggleRelayCommand _сhangeTextNodesVisibilityCommand = null;

        /// <summary>
        /// Команда закрпеления/открепления элемента иерархии
        /// </summary>
        public ToggleRelayCommand FixNodeCommand
        {
            get
            {
                if (_fixNodeCommand == null)
                {
                    _fixNodeCommand = new ToggleRelayCommand(this, (p) => OnFix(p), (p) => CanFix(p),
                    "Закрепить\\открепить элемент", "icon_Favorite", 
                    nameof(FixedNodes),
                    "Открепить",
                    "Закрепить");
                }
                return _fixNodeCommand;
            }
        }
        /// <summary>
        /// Команда переименования элемента иерархии
        /// </summary>
        public ExtendedRelayCommand RenameNodeCommand
        {
            get
            {
                if (_renameNodeCommand == null)
                {
                    _renameNodeCommand = new ExtendedRelayCommand((p) => OnRenameNode(p), (p) => CanRenameNode(p),
                        "Переименовать","Переименовать элемент", "icon_Rename");
                }
                return _renameNodeCommand;
            }
        }
        /// <summary>
        /// Команда перехода к свойства элемента
        /// </summary>
        public ExtendedRelayCommand GoToPropertiesCommand
        {
            get
            {
                if (_goToPropertiesCommand == null)
                {
                    _goToPropertiesCommand = new ExtendedRelayCommand((p) => OnGoToProperties(p), (p) => CanGoToProperties(p),
                        "Свойства", "Перейти к свойствам", "icon_GoToProperty");
                }
                return _goToPropertiesCommand;
            }
        }
        /// <summary>
        /// Команда изменения видимости закрепленных элементов в начале списка
        /// </summary>
        public ToggleRelayCommand ChangeFixedNodesVisibilityCommand
        {
            get
            {
                if (_сhangeFixedNodesVisibilityCommand == null)
                {
                    _сhangeFixedNodesVisibilityCommand = new ToggleRelayCommand(this, (p) => OnChangeFixedNodesVisibility(p), (p) => CanChangeFixedNodesVisibility(p),
                        "Включает\\выключает видимость закрепленных элементов в начале списка",
                        "icon_Favorite",
                        nameof(FixedNodesGroupVisible),
                        "Скрыть закрепленные элементы",
                        "Показать закрепленные элементы","123");
                }
                return _сhangeFixedNodesVisibilityCommand;
            }
        }
        /// <summary>
        /// Команда изменения видимости элементов файлов, не являющихся объектами
        /// </summary>
        public ToggleRelayCommand ChangeTextNodesVisibilityCommand
        {
            get
            {
                if (_сhangeTextNodesVisibilityCommand == null)
                {
                    _сhangeTextNodesVisibilityCommand = new ToggleRelayCommand(this, (p) => OnChangeTextNodesVisibility(p), (p) => CanChangeTextNodesVisibility(p),
                        "Включает\\выключает видимость текстовых элементы файлов",
                        "icon_CommentCode",
                        nameof(TextNodesVisible),
                        "Скрыть текстовые элементы файлов",
                        "Показать текстовые элементы файлов", "123");
                }
                return _сhangeTextNodesVisibilityCommand;
            }
        }

        protected override bool CanOpenValue(object parameter)
        {
            if (parameter == null)
                return false;

            if (parameter is ITreeNode)
                parameter = (parameter as ITreeNode).Value;

            return parameter is FileSystemNode || parameter is TextNode;
        }
        protected override void OnOpenValue(object parameter)
        {
            if (parameter is ITreeNode)
                parameter = (parameter as ITreeNode).Value;

            if (parameter is TextNode)
            {
                TextNode textNode = parameter as TextNode;
                object owner = textNode.GetTextHost();
                if (owner is ObjectTextFileModel)
                {
                    TextFileViewModel textFile = Workspace.This.Open(owner as FileModel) as TextFileViewModel;
                    if (textFile != null)
                    {
                        textFile.CurrentPosition = textNode.Offset;
                        textFile.SelectNode(textNode);
                    }
                }
            }
            else if (parameter is DirectoryModel)
                Process.Start((parameter as DirectoryModel).FullName);
            else if (parameter is FileModel)
                Workspace.This.Open(parameter as FileModel);
        }

        private bool CanFix(object parameter)
        {
            if (parameter == null)
                return false;
            else if (parameter is ITreeNode)
                return (parameter as ITreeNode).Value is FileModel;
            else return parameter is FileModel;
        }

        private void OnFix(object parameter)
        {
            FileModel file = null;
            if (parameter is ITreeNode)
                file = (parameter as ITreeNode).Value as FileModel;
            else
                file = parameter as FileModel;

            if (Workspace.This.FixedFiles.Contains(file))
                Workspace.This.FixedFiles.Remove(file);
            else
                Workspace.This.FixedFiles.Add(file);
            OnPropertyChanged(nameof(FixedNodes));
        }

        private bool CanChangeFixedNodesVisibility(object parameter)
        {
            return true;
        }

        private void OnChangeFixedNodesVisibility(object parameter)
        {
            FixedNodesGroupVisible = !FixedNodesGroupVisible;
        }

        private bool CanRenameNode(object parameter)
        {
            return true;
        }

        private void OnRenameNode(object parameter)
        {
            if (parameter == null)
                return;
            FrameworkElement element = InterfaceHelper.FindElementAtDataContextRecursively(_mainControl as ItemsControl, parameter);
            if (!(element is TreeViewItem))
                return;
            TreeViewItem item = element as TreeViewItem;
            item.IsSelected = true;
            FrameworkElement textBoxElement = InterfaceHelper.FindChildAtNameRecursively(item, "RenameableTextBox");
            if (textBoxElement == null || !(textBoxElement is TextBox))
                return;
            TextBox textBox = textBoxElement as TextBox;
            textBox.IsReadOnly = false;
            textBoxElement.Focus();
            
            (textBoxElement as TextBox).LostFocus += BrowserViewModel_LostFocus;
            //(textBoxElement as TextBox).LostFocus += (object sender, RoutedEventArgs e) => {
            //    textBlockElement.Visibility = Visibility.Visible;
            //    textBoxElement.Visibility = Visibility.Collapsed; 
            //};
        }

        private void BrowserViewModel_LostFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).IsReadOnly = true;
            (sender as TextBox).LostFocus -= BrowserViewModel_LostFocus;
        }

        private bool CanGoToProperties(object parameter)
        {
            return parameter != null;
        }
        private void OnGoToProperties(object parameter)
        {
            Workspace.This.PropertiesTool.Source = parameter;
            Workspace.This.PropertiesTool.IsVisible = true;
            Workspace.This.PropertiesTool.IsSelected = true;
            Workspace.This.PropertiesTool.IsActive = true;
            Workspace.This.ActiveContent = Workspace.This.PropertiesTool;
        }

        private bool CanChangeTextNodesVisibility(object parameter)
        {
            return true;
        }

        private void OnChangeTextNodesVisibility(object parameter)
        {
            TextNodesVisible = !TextNodesVisible;
        }
    }
}
