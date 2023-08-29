using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Stalker_Studio.ViewModel
{
	class ToolViewModel : PaneViewModel
	{
		private string _initLocationName = null;

		public ToolViewModel(string name)
		{
			Name = name;
			Title = name;
		}

		#region Properties
		public string Name { get; private set; }

		public string InitLocationName { get => _initLocationName; set => _initLocationName = value; }

        #endregion Properties
    }
}
