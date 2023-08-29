using System.Collections.Generic;
using System.Linq;
using Stalker_Studio.Common;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Stalker_Studio.ViewModel
{
    /// <summary>
    /// Модель представления для иерархии
    /// </summary>
    partial class HierarchicalViewModel : ItemsViewModel
    { 
        ExtendedRelayCommand _collapseAllCommand = null;
        ExtendedRelayCommand _expandAllCommand = null;
        ToggleRelayCommand _filterCommand = null;

        
        /// <summary>
        /// Команда сворачивания всех элементов иерархии
        /// </summary>
        public ExtendedRelayCommand CollapseAllCommand
        {
            get
            {
                if (_collapseAllCommand == null)
                    _collapseAllCommand = new ExtendedRelayCommand((p) => OnCollapseAll(p), (p) => CanCollapseAll(p), "Свернуть все", "Свернуть все элементы", "icon_CollapseAll");
                return _collapseAllCommand;
            }
        }
        /// <summary>
        /// Команда развернуть всех элементов
        /// </summary>
        public ExtendedRelayCommand ExpandAllCommand
        {
            get
            {
                if (_expandAllCommand == null)
                    _expandAllCommand = new ExtendedRelayCommand((p) => OnExpandAll(p), (p) => CanExpandAll(p), "Развернуть все", "Развернуть все элементы", "icon_ExpandAll");
                return _expandAllCommand;
            }
        }
        /// <summary>
        /// Команда развернуть всех элементов
        /// </summary>
        public ToggleRelayCommand FilterCommand
        {
            get
            {
                if (_filterCommand == null)
                    _filterCommand = new ToggleRelayCommand(this, (p) => OnFilter(p), (p) => CanFilter(p), "Включить\\выключить фильтрацию", "icon_Filter", "IsFiltered", "Выключить фильтрацию", "Включить фильтрацию");
                return _filterCommand;
            }
        }

        protected virtual bool CanCollapseAll(object parameter)
        {
            return _mainControl != null;
        }
        protected virtual void OnCollapseAll(object parameter)
        {
            if (!(_mainControl is System.Windows.Controls.TreeView))
                return;
            InterfaceHelper.SetPropertyValueRecursively(_mainControl as System.Windows.Controls.TreeView, "IsExpanded", false);
        }
        protected virtual bool CanExpandAll(object parameter)
        {
            return _mainControl != null;
        }
        protected virtual void OnExpandAll(object parameter)
        {
            if (!(_mainControl is System.Windows.Controls.TreeView))
                return;

            InterfaceHelper.SetPropertyValueRecursively(_mainControl as System.Windows.Controls.TreeView, "IsExpanded", true);
        }
        protected virtual bool CanFilter(object parameter)
        {
            if(Root is ITreeNode)
                return !(Root as ITreeNode).IsLast;
            else if (Root is IHierarchical)
                return !(Root as IHierarchical).IsLast;

            return false;
        }
        protected virtual void OnFilter(object parameter)
        {
            IsFiltered = !_isFiltered;
        }
    }
}
