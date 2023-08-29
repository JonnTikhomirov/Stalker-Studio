using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace Stalker_Studio.ViewModel
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		protected ObservableCollection<ExtendedRelayCommand> _commands = new ObservableCollection<ExtendedRelayCommand>();
		public virtual ObservableCollection<ExtendedRelayCommand> Commands { get { return _commands; } }

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
