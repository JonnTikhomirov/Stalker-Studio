using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PropertyTools.Wpf;

//
//  Источник https://github.com/PropertyTools/PropertyTools/tree/develop/Source/Examples/PropertyGrid/PropertyGridDemo
//

namespace Stalker_Studio
{

    public class CustomOperator : PropertyGridOperator
    {
        public CustomOperator()
        {           
            DefaultCategoryName = "Общие";
        }
    }

    public class CustomControlFactory : PropertyGridControlFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomControlFactory"/> class.
        /// </summary>
        public CustomControlFactory()
        {
            
            //this.Converters.Add(new PropertyConverter(typeof(Length), new LengthConverter()));
        }

        /// <inheritdoc />
        public override FrameworkElement CreateControl(PropertyItem property, PropertyControlFactoryOptions options)
        {

            if (property.Is(typeof(IEnumerable<StalkerClass.LtxParameter>)))
            {
                System.Windows.Controls.DataGrid dataGrid = new System.Windows.Controls.DataGrid();
                dataGrid.Style = App.Current.Resources["style_PropertiesDataGrid"] as Style;

                DataGridTextColumn nameColumn = new DataGridTextColumn();
                nameColumn.Header = "Имя";
                nameColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                nameColumn.Binding = new Binding(nameof(StalkerClass.LtxParameter.Name));
                dataGrid.Columns.Add(nameColumn);

                DataGridTextColumn valueColumn = new DataGridTextColumn();
                valueColumn.Header = "Значение";
                valueColumn.Binding = new Binding(nameof(StalkerClass.LtxParameter.Value));
                valueColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                dataGrid.Columns.Add(valueColumn);

                DataGridTemplateColumn goToColumn = new DataGridTemplateColumn();
                goToColumn.Width = new DataGridLength(0, DataGridLengthUnitType.Auto);
                goToColumn.MinWidth = 16;
                goToColumn.CanUserResize = false;
                goToColumn.CanUserSort = false;
                goToColumn.CellTemplate = App.Current.Resources["dataTemplate_GoToDataGridCell"] as DataTemplate;
                dataGrid.Columns.Add(goToColumn);

                Binding binding = new Binding(property.Descriptor.Name);
                dataGrid.SetBinding(ItemsControl.ItemsSourceProperty, binding);
                return dataGrid;
            }

            if (property.Is(typeof(IEnumerable<StalkerClass.LtxSection>)))
            {
                System.Windows.Controls.DataGrid dataGrid = new System.Windows.Controls.DataGrid();
                dataGrid.Style = App.Current.Resources["style_PropertiesDataGrid"] as Style;

                DataGridTextColumn nameColumn = new DataGridTextColumn();
                nameColumn.Header = "Имя";
                nameColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                nameColumn.Binding = new Binding(nameof(StalkerClass.LtxSection.Name));
                dataGrid.Columns.Add(nameColumn);

                DataGridTextColumn valueColumn = new DataGridTextColumn();
                valueColumn.Header = "Комментарий";
                valueColumn.Binding = new Binding(nameof(StalkerClass.LtxSection.Comment));
                valueColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                dataGrid.Columns.Add(valueColumn);

                DataGridTemplateColumn goToColumn = new DataGridTemplateColumn();
                goToColumn.Width = new DataGridLength(0, DataGridLengthUnitType.Auto);
                goToColumn.MinWidth = 16;
                goToColumn.CanUserResize = false;
                goToColumn.CanUserSort = false;
                goToColumn.CellTemplate = App.Current.Resources["dataTemplate_GoToDataGridCell"] as DataTemplate;
                dataGrid.Columns.Add(goToColumn);

                Binding binding = new Binding(property.Descriptor.Name);
                dataGrid.SetBinding(ItemsControl.ItemsSourceProperty, binding);
                return dataGrid;
            }

            if (property.Is(typeof(IEnumerable)) && !property.Is(typeof(string)))
            {
                TextBox control = new TextBox();
                control.HorizontalAlignment = HorizontalAlignment.Stretch;
                control.IsReadOnly = true;
                Binding binding = new Binding(property.Descriptor.Name)
                {
                    Converter = new CollectionToStringConverter(),
                    Mode = BindingMode.OneWay
                };
                control.SetBinding(TextBox.TextProperty, binding);
                return control;
            }

            return base.CreateControl(property, options);
        }

//        /// <inheritdoc />
//        public override ContentControl CreateErrorControl(PropertyItem pi, object instance, Tab tab, PropertyControlFactoryOptions options)
//        {
//            var dataErrorInfoInstance = instance as IDataErrorInfo;
//            var notifyDataErrorInfoInstance = instance as INotifyDataErrorInfo;

//            if (Application.Current.TryFindResource("ValidationErrorTemplateEx") != null)
//            {
//                options.ValidationErrorTemplate = (DataTemplate)Application.Current.TryFindResource("ValidationErrorTemplateEx");
//            }

//            var errorControl = new ContentControl
//            {
//                ContentTemplate = options.ValidationErrorTemplate,
//                Focusable = false
//            };

//            IValueConverter errorConverter;
//            string propertyPath;
//            object source = null;
//            if (dataErrorInfoInstance != null)
//            {
//                errorConverter = new DataErrorInfoConverter(dataErrorInfoInstance, pi.PropertyName);
//                propertyPath = pi.PropertyName;
//                source = instance;
//            }
//            else
//            {
//                errorConverter = new NotifyDataErrorInfoConverter(notifyDataErrorInfoInstance, pi.PropertyName);
//                propertyPath = nameof(tab.HasErrors);
//                source = tab;
//                notifyDataErrorInfoInstance.ErrorsChanged += (s, e) =>
//                {
//                    UpdateTabForValidationResults(tab, notifyDataErrorInfoInstance);
//                    //needed to refresh error control's binding also when error changes (i.e from Error to Warning)
//                    errorControl.GetBindingExpression(ContentControl.ContentProperty).UpdateTarget();
//                };

//            }

//            var visibilityBinding = new Binding(propertyPath)
//            {
//                Converter = errorConverter,
//                NotifyOnTargetUpdated = true,
//#if !NET40
//                ValidatesOnNotifyDataErrors = false,
//#endif
//                Source = source,
//            };

//            var contentBinding = new Binding(propertyPath)
//            {
//                Converter = errorConverter,
//#if !NET40
//                ValidatesOnNotifyDataErrors = false,
//#endif
//                Source = source,
//            };

//            var warningBinding = new Binding(nameof(tab.HasWarnings))
//            {
//                Converter = errorConverter,
//#if !NET40
//                ValidatesOnNotifyDataErrors = false,
//#endif
//                Source = source,
//            };

//            errorControl.SetBinding(UIElement.VisibilityProperty, visibilityBinding);

//            // When the visibility of the error control is changed, updated the HasErrors of the tab
//            errorControl.TargetUpdated += (s, e) =>
//            {
//                if (dataErrorInfoInstance != null)
//                    tab.UpdateHasErrors(dataErrorInfoInstance);
//            };
//            errorControl.SetBinding(ContentControl.ContentProperty, contentBinding);

//            errorControl.SetBinding(ContentControl.ContentProperty, warningBinding);
//            return errorControl;
//        }

//        /// <inheritdoc />
//        public override void UpdateTabForValidationResults(Tab tab, object errorInfo)
//        {
//            if (errorInfo is INotifyDataErrorInfo ndei)
//            {
//                tab.HasErrors = tab.Groups.Any(g => g.Properties.Any(p => ndei.GetErrors(p.PropertyName).Cast<object>()
//                   .Any(a => a != null && a.GetType() == typeof(ValidationResultEx) && ((ValidationResultEx)a).Severity == Severity.Error)));

//                tab.HasWarnings = tab.Groups.Any(g => g.Properties.Any(p => ndei.GetErrors(p.PropertyName).Cast<object>()
//                   .Any(a => a != null && a.GetType() == typeof(ValidationResultEx) && ((ValidationResultEx)a).Severity == Severity.Warning)));
//            }
//            else if (errorInfo is IDataErrorInfo dei)
//            {
//                tab.HasErrors = tab.Groups.Any(g => g.Properties.Any(p => !string.IsNullOrEmpty(dei[p.PropertyName])));
//            }
//        }

//        /// <inheritdoc />
//        public override void SetValidationErrorStyle(FrameworkElement control, PropertyControlFactoryOptions options)
//        {
//            if (Application.Current.TryFindResource("ErrorInToolTipStyleEx") != null)
//            {
//                options.ValidationErrorStyle = (Style)Application.Current.TryFindResource("ErrorInToolTipStyleEx");
//                control.Style = options.ValidationErrorStyle;
//            }
//        }
    }
}
