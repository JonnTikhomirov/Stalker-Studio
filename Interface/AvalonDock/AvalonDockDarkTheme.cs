using System;

//Источник
//https://github.com/Dirkster99/AvalonDock/tree/master/source/Components/AvalonDock.Themes.VS2013

namespace AvalonDock.Themes
{
	/// <inheritdoc/>
	public class AvalonDockDarkTheme : Theme
	{
		/// <inheritdoc/>
		public override Uri GetResourceUri()
		{
			return new Uri(
				"Interface/AvalonDock/DarkTheme.xaml",
				UriKind.Relative);
		}
	}
}
