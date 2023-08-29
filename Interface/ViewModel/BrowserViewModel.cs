using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        IEnumerable<IHierarchical> _fixedNodes = null;
        bool _fixedNodesGroupVisible = true;
        bool _textNodesVisible = true;

        public BrowserViewModel(string name, Hierarchical root) : base(name, root)
        {
            ContentId = "Browser";
            InitLocationName = "MainAnchorablePane";
            Initialize();
        }

        /// <summary>
        /// Закрепленные элементы иерархии или особые, отдельно обособленные
        /// </summary>
        public IEnumerable<IHierarchical> FixedNodes
        {
            get
            {
                if (_fixedNodes == null)
                    _fixedNodes = new ObservableCollection<IHierarchical>();
                return _fixedNodes;
            }
            set
            {
                _fixedNodes = value;
            }
        }
        /// <summary>
        /// Признак видимости группы закрепленных элементов
        /// </summary>
        public bool FixedNodesGroupVisible
        {
            get { return _fixedNodesGroupVisible; }
            set
            {
                _fixedNodesGroupVisible = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Признак видимости элементов файлов, не являющихся объектами
        /// </summary>
        public bool TextNodesVisible { 
            get => _textNodesVisible;
            set
            {
                _textNodesVisible = value;
                OnPropertyChanged();
            }
        }

        private void Initialize()
        {
            Commands.Add(ChangeFixedNodesVisibilityCommand);
            Commands.Add(ChangeTextNodesVisibilityCommand);
            ItemCommands.Add(FixNodeCommand);
            ItemCommands.Add(RenameNodeCommand);
            ItemCommands.Add(GoToPropertiesCommand);
        }
    }
}
