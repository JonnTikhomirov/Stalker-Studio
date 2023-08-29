using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Stalker_Studio
{
    public class CommandDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Button { get; set; }
        public DataTemplate ToggleButton { get; set; }
        public DataTemplate ToolBarButton { get; set; }
        public DataTemplate ToolBarToggleButton { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ContentPresenter || !(item is ViewModel.ExtendedRelayCommand) || container is System.Windows.Controls.Primitives.MenuBase)
                return null;

            ViewModel.ExtendedRelayCommand command = item as ViewModel.ExtendedRelayCommand;

            if (container is ContentPresenter)
            {
                if (command is ViewModel.ToggleRelayCommand)
                    return ToolBarToggleButton;
                else
                    return ToolBarButton;
            }
            else
            {
                if (command is ViewModel.ToggleRelayCommand)
                    return ToggleButton;
                else
                    return Button;
            }
        }
    }

    public class CommandStyleSelector : StyleSelector
    {
        public Style Button { get; set; }
        public Style ToggleButton { get; set; }
        public Style MenuItem { get; set; }
        public Style CheckableMenuItem { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is ContentPresenter || !(item is ViewModel.ExtendedRelayCommand))
                return null;

            ViewModel.ExtendedRelayCommand command = item as ViewModel.ExtendedRelayCommand;

            if (command is ViewModel.ToggleRelayCommand)
            {
                if (container is MenuItem)
                    return CheckableMenuItem;
                return ToggleButton;
            }
            else
            {
                if (container is MenuItem)
                    return MenuItem;
                else
                    return Button;
            }
        }
    }
}
