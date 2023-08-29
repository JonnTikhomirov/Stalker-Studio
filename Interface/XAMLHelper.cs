using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Markup;
using System.Xaml;
using System.Windows.Data;

namespace Stalker_Studio
{
    /// <summary>
    /// Присоединенные свойства для элементов xaml.
    /// </summary>
    public class Attached
    {
        /// <summary>
        /// Свойство для элементов xaml. Тип значения должен быть ViewModel.ViewModelBase
        /// Нужно для передачи предыдущего родительского ViewModel в дочерние элементы для привязки к свойствам.
        /// Автоматически наследуются для дочерних элементов
        /// </summary>
        public static readonly DependencyProperty PreviousViewModelProperty = DependencyProperty.RegisterAttached(
            "PreviousViewModel", typeof(object), typeof(Attached), 
            new FrameworkPropertyMetadata(default(object), 
                FrameworkPropertyMetadataOptions.Inherits));
        public static void SetPreviousViewModel(UIElement element, object value)
        {
            element.SetValue(PreviousViewModelProperty, value);
        }
        public static object GetPreviousViewModel(UIElement element)
        {       
            return (object)element.GetValue(PreviousViewModelProperty); ;
        }

        /// <summary>
        /// Свойство для элементов xaml. Тип значения должен быть ViewModel.ViewModelBase
        /// Нужно для передачи родительского ViewModel в дочерние элементы для привязки к свойствам.
        /// Автоматически наследуются для дочерних элементов
        /// </summary>
        public static readonly DependencyProperty CurrentViewModelProperty = DependencyProperty.RegisterAttached(
            "CurrentViewModel", typeof(object), typeof(Attached), 
            new FrameworkPropertyMetadata(default(object), 
                FrameworkPropertyMetadataOptions.Inherits, 
                new PropertyChangedCallback(CurrentPropertyChanged)));
        public static void SetCurrentViewModel(UIElement element, object value)
        {
            element.SetValue(CurrentViewModelProperty, value);
        }
        public static object GetCurrentViewModel(UIElement element)
        {
            return (object)element.GetValue(CurrentViewModelProperty);
        }

        public static void CurrentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if (e.NewValue is ViewModel.ToolViewModel && d is FrameworkElement)
            //{
            //    FrameworkElement element = d as FrameworkElement;
            //    if (element.Name == "ViewModelHost")
            //    {
            //        ViewModel.ToolViewModel viewModel = e.NewValue as ViewModel.ToolViewModel;
            //        viewModel.MainControl = element;
            //    }

            //}
        }

        /// <summary>
        /// Свойство для элементов xaml. Тип значения должен быть FrameworkElement
        /// Нужно для возможности передачи FrameworkElement в view model через привязку. Использовать только OneWayToSource
        /// </summary>
        public static readonly DependencyProperty ThisProperty = DependencyProperty.RegisterAttached(
            "This", typeof(object), typeof(Attached),
            new FrameworkPropertyMetadata(default(FrameworkElement),
            new PropertyChangedCallback(ThisPropertyChanged), new CoerceValueCallback(CoerceValueCallback)));
        public static void SetThis(UIElement element, object value)
        {
            element.SetValue(CurrentViewModelProperty, value);
        }
        public static object GetThis(UIElement element)
        {
            return (object)element.GetValue(CurrentViewModelProperty);
        }
        public static object CoerceValueCallback(DependencyObject d, object baseValue) 
        {
            return d;// возвращаем самого себя
        }
        public static void ThisPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(ThisProperty, e.NewValue);// вызываем присвоение что бы сработал биндинг
        }

    }

    /// <summary>
    /// Возвращает корневой элемент
    /// </summary>
    [MarkupExtensionReturnType(typeof(ContentControl))]
    public class RootObject : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var rootObjectProvider = (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider));
            return rootObjectProvider?.RootObject;
        }
    }
    /// <summary>
    /// Дает возможность устанавливать значения Source и Path из привязки к объекту ViewModel.ToggleRelayCommand, 
    /// в дальнейшем можно сделать универсальным
    /// </summary>
    [MarkupExtensionReturnType(typeof(object))]
    public class PathBinding : Binding
    {
        public DataContextSpy Command {
            set 
            {
                BindingExpression bindingExpression = BindingOperations.GetBindingExpression(value, DataContextSpy.DataContextProperty);
                bindingExpression.UpdateTarget();
                object data = value.GetValue(DataContextSpy.DataContextProperty);
                if (data is ViewModel.ToggleRelayCommand)
                {
                    ViewModel.ToggleRelayCommand command = data as ViewModel.ToggleRelayCommand;
                    Source = command.Owner;
                    Path = new PropertyPath(command.IsCheckedBindPath);
                }
            }
        }
    }

    [MarkupExtensionReturnType(typeof(object))]
    public class StaticBinding : MarkupExtension
    {
        Binding _binding = null;
        DataContextSpy _pathSource;

        public StaticBinding() { }
        public StaticBinding(Binding binding, DataContextSpy pathSource) 
        {
            _binding = binding;
            _pathSource = pathSource;
        }

        public Binding Binding 
        { 
            get { return _binding; }
            set { _binding = value; } 
        }
        public DataContextSpy PathSource
        { 
            get { return _pathSource; }
            set { _pathSource = value; } 
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget provideTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            FrameworkElement element = provideTarget.TargetObject as FrameworkElement;
            if (element == null)
                throw new InvalidOperationException();

            if (_binding.Source == null)
            {
                //BindingExpression bindingExpression = BindingOperations.GetBindingExpression(_pathSource, DataContextSpy.DataContextProperty);
                //bindingExpression.UpdateTarget();
                //object data = _pathSource.GetValue(DataContextSpy.DataContextProperty);
                //if (data == null)
                //    return null;

                ViewModel.ToggleRelayCommand command = element.DataContext as ViewModel.ToggleRelayCommand;
                _binding.Source = command.Owner;
                _binding.Path = new PropertyPath(command.IsCheckedBindPath);
            }

            BindingExpressionBase bindingExpression2 = element.SetBinding(provideTarget.TargetProperty as DependencyProperty, _binding);

            return bindingExpression2;
        }
    }
    /// <summary>
    /// Прокси объект для привязки
    /// </summary>
    public class DataContextSpy : Freezable // Enable ElementName and DataContext bindings
    {
        public DataContextSpy()
        {
            // This binding allows the spy to inherit a DataContext.
            BindingOperations.SetBinding(this, DataContextProperty, new Binding());

            this.IsSynchronizedWithCurrentItem = true;
        }

        /// <summary>
        /// Gets/sets whether the spy will return the CurrentItem of the 
        /// ICollectionView that wraps the data context, assuming it is
        /// a collection of some sort. If the data context is not a 
        /// collection, this property has no effect. 
        /// The default value is true.
        /// </summary>
        public bool IsSynchronizedWithCurrentItem { get; set; }

        public object DataContext
        {
            get { return (object)GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }

        // Borrow the DataContext dependency property from FrameworkElement.
        public static readonly DependencyProperty DataContextProperty =
            FrameworkElement.DataContextProperty.AddOwner(
            typeof(DataContextSpy),
            new PropertyMetadata(null, null, OnCoerceDataContext));

        static object OnCoerceDataContext(DependencyObject depObj, object value)
        {
            DataContextSpy spy = depObj as DataContextSpy;
            if (spy == null)
                return value;

            if (spy.IsSynchronizedWithCurrentItem)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(value);
                if (view != null)
                    return view.CurrentItem;
            }

            return value;
        }

        protected override Freezable CreateInstanceCore()
        {
            // We are required to override this abstract method.
            throw new NotImplementedException();
        }
    }
}
