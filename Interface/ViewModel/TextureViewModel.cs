using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Stalker_Studio.Common;
using System.Diagnostics;
using System;
using System.Windows.Forms.Integration;
using System.Threading;
using Stalker_Studio.StalkerClass;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using PropertyTools.Wpf;
using System.Windows.Media.Media3D;

namespace Stalker_Studio.ViewModel
{
	class TextureViewModel : FileViewModel
	{
        BitmapImage _imageSource = null;
        double _zoomValue = 1;
        ScaleTransform _scaleTransform = null;
        FrameworkElement _floatControl = null;
        FrameworkElement _editControl = null;

        public TextureViewModel(TextureModel file) : base(file)
		{
            Initialize();
		}
		public TextureViewModel(string fullName) : this(new TextureModel(fullName)) { }
		public TextureViewModel() : this(new TextureModel()) { }

        public BitmapImage ImageSource
        {
            get 
            {
                if(_imageSource == null)
                    _imageSource = InterfaceHelper.BitmapToImageSource((_file as StalkerClass.TextureModel).Bitmap);
                return _imageSource;
            }
        }

        public double ZoomValue
        {
            get => _zoomValue;
            set
            {
                if (value <= 0)
                    value = 1;
                _zoomValue = value; 
                OnPropertyChanged();
                UpdateTransform();
            }
        }

        public ScaleTransform ScaleTransform 
        {
            get
            {
                if(_scaleTransform == null)
                    _scaleTransform = new ScaleTransform();
                return _scaleTransform;
            }
            set 
            {
                _scaleTransform = value; 
                OnPropertyChanged(); 
            } 
        }

        public FrameworkElement FloatControl 
        { 
            get => _floatControl; 
            set 
            {
                if (_floatControl == value)
                    return;

                _floatControl = value;
                if (_floatControl != null)
                {
                    _floatControl.Width = ImageSource.PixelWidth;
                    _floatControl.Height = ImageSource.PixelHeight;
                }
                OnPropertyChanged(); 
            } 
        }

        public FrameworkElement EditControl 
        { 
            get => _editControl;
            set
            {
                if (_editControl == value)
                    return;
                _editControl = value;

                OnPropertyChanged();
            }
        }

        protected override void OnSetMainControl()
        {
            if(_mainControl is ScrollViewer)
                _mainControl.Loaded += MainControl_Loaded;
        }

        private void MainControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ScrollViewer scrollViewer = (sender as ScrollViewer);

            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
            scrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            scrollViewer.MouseMove += OnMouseMove;

            UpdateTransform();

            scrollViewer.Loaded -= MainControl_Loaded;
        }

        private void Initialize() 
		{

		}

        Point? lastCenterPositionOnTarget;
        Point? lastMousePositionOnTarget;
        Point? lastDragPoint;

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            ScrollViewer scrollViewer = (sender as ScrollViewer);
            if (lastDragPoint.HasValue)
            {
                Point posNow = e.GetPosition(scrollViewer);

                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
            }
        }

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is ContentControl)
                return;
            ScrollViewer scrollViewer = (sender as ScrollViewer);
            var mousePos = e.GetPosition(scrollViewer);
            if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y <
                scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
            {
                scrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                Mouse.Capture(scrollViewer);
            }
        }

        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            lastMousePositionOnTarget = Mouse.GetPosition(_floatControl);

            if (e.Delta > 0)
                ZoomValue /= 0.8;
            if (e.Delta < 0)
                ZoomValue *= 0.8;

            e.Handled = true;
        }

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ScrollViewer scrollViewer = (sender as ScrollViewer);
            scrollViewer.Cursor = Cursors.Arrow;
            scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }

        void UpdateTransform()
        {
            Point mouse = Mouse.GetPosition(_mainControl);
            ScaleTransform.ScaleX = _zoomValue;
            ScaleTransform.ScaleY = _zoomValue;

            var centerOfViewport = new Point((_mainControl as ScrollViewer).ViewportWidth / 2,
                                             (_mainControl as ScrollViewer).ViewportHeight / 2);
            lastCenterPositionOnTarget = (_mainControl as ScrollViewer).TranslatePoint(centerOfViewport, _floatControl);
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scrollViewer = (sender as ScrollViewer);
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2,
                                                         scrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow =
                              scrollViewer.TranslatePoint(centerOfViewport, _floatControl);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(_floatControl);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / _floatControl.Width;
                    double multiplicatorY = e.ExtentHeight / _floatControl.Height;

                    double newOffsetX = scrollViewer.HorizontalOffset -
                                        dXInTargetPixels * multiplicatorX;
                    double newOffsetY = scrollViewer.VerticalOffset -
                                        dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }
    }
}
