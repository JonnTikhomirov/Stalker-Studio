using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stalker_Studio.ViewModel
{
    class PropertiesViewModel : ToolViewModel
    {
        IEnumerable _sourceList = null;
        ObservableCollection<object> _lastSources = new ObservableCollection<object>();
        object _source = null;

        ExtendedRelayCommand _goToCommand = null;

        public PropertiesViewModel(string name) : base(name)
        {
            InitLocationName = "DetailAnchorablePane";
        }

        public object Source
        {
            get => _source;
            set
            {
                if (_source == value || value is System.Windows.FrameworkElement)
                    return;

                _source = value;

                if (value != null)
                {
                    string typeName = InterfaceHelper.GetString(value.GetType());
                    Title = Name + (typeName != "" ? ": " + typeName : "");

                    int index = _lastSources.IndexOf(value);
                    if (index != -1)
                        _lastSources.Move(index, 0);
                    else
                        _lastSources.Insert(0, value);

                    if (_lastSources.Count > 40)
                        _lastSources.RemoveAt(_lastSources.Count - 1);
                }
                else
                    Title = Name;

                OnPropertyChanged();
            }
        }
        public IEnumerable SourceList
        {
            get => _sourceList;
            set
            {
                _sourceList = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<object> LastSources 
        { 
            get => _lastSources;
        }

        public ExtendedRelayCommand GoToCommand {
            get 
            {
                if (_goToCommand == null)
                    _goToCommand = new ExtendedRelayCommand((p) => OnGoTo(p), (p) => CanGoTo(p),
                        "Перейти к...", "Перейти к свойствам", "icon_Open");
                return _goToCommand;
            }
        }

        private bool CanGoTo(object parameter)
        {
            return parameter != null;
        }

        private void OnGoTo(object parameter)
        {
            Source = parameter;
            Workspace.This.Browser.SelectedItem = parameter;
        }
    }
}
