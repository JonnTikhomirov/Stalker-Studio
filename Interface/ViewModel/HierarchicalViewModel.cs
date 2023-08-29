using System.Collections.Generic;
using System.Linq;
using Stalker_Studio.Common;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace Stalker_Studio.ViewModel
{
    /// <summary>
    /// Модель представления для иерархии
    /// </summary>
    partial class HierarchicalViewModel : ItemsViewModel
    {
        Hierarchical _root = null;        
        bool _isFiltered = false;
        TreeNode<IHierarchical> _filteredTree = null;

        public HierarchicalViewModel(string name) : base(name) 
        {
            Initialize();
        }
        public HierarchicalViewModel(string name, Hierarchical root = null) : base(name)
        {
            if(root != null)
                _root = root;
            Initialize();
        }
        /// <summary>
        /// Корневой элемент иерархии либо IHerarchical, либо TreeNode<IHierarchical> в зависимости от настроек
        /// </summary>
        public object Root 
        {
            get {
                if (_filteredTree == null)
                    return _root;
                else
                    return _filteredTree;
            }
            set {
                _root = value as Hierarchical;
                OnPropertyChanged(nameof(Root));
            }
        }
        public override string Search
        {
            get
            {
                return _search;
            }
            set
            {
                if (_search == value)
                    return;

                _search = value;

                if (_search != "")
                {
                    IsFiltered = true;

                    int index = _searchHistory.IndexOf(value);
                    if (index != -1)
                        _searchHistory.Move(index, 0);
                    else
                        _searchHistory.Insert(0, value);
                }
                else
                    IsFiltered = false;

                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Строка поиска по представлению элементов
        /// </summary>
        public bool IsFiltered
        {
            get
            {
                return _isFiltered;
            }
            set
            {               
                _isFiltered = value;
                if (!_isFiltered)
                {
                    _search = "";
                    OnPropertyChanged(nameof(Search));
                }

                ApplyFilter();
                OnPropertyChanged();
            }
        }
        public override System.Windows.FrameworkElement MainControl
        {
            set
            {
                if (value != null && value is TreeView)
                {
                    TreeView treeView = value as TreeView;
                    treeView.SelectedItemChanged += (object sender, System.Windows.RoutedPropertyChangedEventArgs<object> args) =>
                    {
                        if ((sender as TreeView).SelectedItem is ITreeNode)
                            _selectedItem = ((sender as TreeView).SelectedItem as ITreeNode).Value;
                        else
                            _selectedItem = (sender as TreeView).SelectedItem;
                        OnPropertyChanged(nameof(SelectedItem));
                    };
                }

                base.MainControl = value;
            }
        }
        /// <summary>
        /// Выбранный элемент
        /// </summary>
        public override object SelectedItem 
        { 
            get => _selectedItem;
            set
            {
                bool needEvent = _selectedItem != value;

                TreeViewItem treeView = (InterfaceHelper.FindElementAtDataContextRecursively(_mainControl as ItemsControl, _selectedItem) as TreeViewItem);
                if (treeView == null)
                    return;

                _selectedItem = value;
                treeView.IsSelected = true;
                treeView.Focus();

                if(needEvent)
                    OnPropertyChanged();
            } 
        }

        private void Initialize()
        {
            _commands.Add(ExpandAllCommand);
            _commands.Add(CollapseAllCommand);
            _commands.Add(FilterCommand);
        }

        /// <summary>
        /// Применить фильтр к иерархии
        /// </summary>
        protected void ApplyFilter() 
        {
            _filteredTree = null;

            if (IsFiltered)
            {
                if (_search != "")
                {
                    _filteredTree = new TreeNode<IHierarchical>(_root.FilteredNodes(x => x.ToString().Contains(_search), true, true));
                    _filteredTree.Value = _root;
                }
            }
            //else
            //    ApplyOrder();
            OnPropertyChanged(nameof(Root));
        }
        /// <summary>
        /// Применить порядок, сортировку к иерархии
        /// </summary>
        protected void ApplyOrder()
        {
            if (Workspace.This.FixedFiles.Count == 0)
                return;

            _filteredTree = new TreeNode<IHierarchical>(_root.Nodes);
            _filteredTree.Value = _root;
            IEnumerable<IHierarchical> fixedNodes = _root.FindNodes(x =>
            {
                if (!(x is Common.FileModel))
                    return false;
                else
                    return Workspace.This.FixedFiles.Contains(x);
            },
                true);
            foreach (IHierarchical node in fixedNodes)
            {
                _filteredTree.GetNodes().Insert(0, new TreeNode<IHierarchical>(node));
            }
        }
    }
}
