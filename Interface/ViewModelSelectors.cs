using AvalonDock.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Stalker_Studio
{

    class LayoutUpdate : ILayoutUpdateStrategy
    {
        //static Dictionary<string, string> _ContentIds = new Dictionary<PaneLocation, string>
        //{
        //    { PaneLocation.Left, "LeftPane" },
        //    { PaneLocation.Right, "RightPane" },
        //    { PaneLocation.Bottom, "BottomPane" },
        //};

        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            if (anchorableToShow.Content is ViewModel.ToolViewModel)
            {
                string paneName = (anchorableToShow.Content as ViewModel.ToolViewModel).InitLocationName;
                if (paneName != null)
                {
                    LayoutAnchorablePane anchPane = layout.Descendents()
                               .OfType<LayoutAnchorablePane>()
                               .FirstOrDefault(d => d.Name == paneName);
                    if (anchPane != default)
                    {
                        destinationContainer = anchPane;
                        anchPane.Children.Add(anchorableToShow);
                        anchPane.ComputeVisibility();
                        (anchPane.Parent as LayoutPanel).ComputeVisibility();
                        return true;
                    }
                }
            }
            //if((anchorableToShow.Content as ViewModel.ToolViewModel).ContentId == ViewModel.Workspace.This.Browser.ContentId)
                
            //if (anchorableToShow.Content is IAnchorable anch)
            //{
            //    var initLocation = anch.InitialLocation;
            //    string paneName = _paneNames[initLocation];
            //    var anchPane = layout.Descendents()
            //                   .OfType<LayoutAnchorablePane>()
            //                   .FirstOrDefault(d => d.Name == paneName);

            //    if (anchPane == null)
            //    {
            //        anchPane = CreateAnchorablePane(layout, Orientation.Horizontal, initLocation);
            //    }
            //    anchPane.Children.Add(anchorableToShow);
            //    return true;
            //}

            return false;
        }

        //static LayoutAnchorablePane CreateAnchorablePane(LayoutRoot layout, Orientation orientation,
        //            PaneLocation initLocation)
        //{
        //    var parent = layout.Descendents().OfType<LayoutPanel>().First(d => d.Orientation == orientation);
        //    string paneName = _paneNames[initLocation];
        //    var toolsPane = new LayoutAnchorablePane { Name = paneName };
        //    if (initLocation == PaneLocation.Left)
        //        parent.InsertChildAt(0, toolsPane);
        //    else
        //        parent.Children.Add(toolsPane);
        //    return toolsPane;
        //}

        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorable)
        {
            // here set the initial dimensions (DockWidth or DockHeight, depending on location) of your anchorable
        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {
            
        }
    }

    public class ViewModelDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate File { get; set; }
        public DataTemplate TextFile { get; set; }
        public DataTemplate OGFFile { get; set; }
        public DataTemplate TextureFile { get; set; }
        public DataTemplate Hierarchical { get; set; }
        public DataTemplate Browser { get; set; }
        public DataTemplate PropertiesTool { get; set; }
        public DataTemplate MessageList { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ContentPresenter)
                return null;

            if (item is ViewModel.TextFileViewModel)
                return TextFile;
            else if (item is ViewModel.OGFViewModel)
                return OGFFile;
            else if (item is ViewModel.TextureViewModel)
                return TextureFile;
            else if (item is ViewModel.BrowserViewModel)
                return Browser;
            else if (item is ViewModel.HierarchicalViewModel)
                return Hierarchical;
            else if (item is ViewModel.PropertiesViewModel)
                return PropertiesTool;
            else if (item is ViewModel.MessageListViewModel)
                return MessageList;
            else
            {
                return File;
            }
        }
    }

    public class HeaderDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Any { get; set; }
        public DataTemplate FileModel { get; set; }
        public DataTemplate INammed { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ContentPresenter)
                return null;

            if (item is Common.FileModel)
                return FileModel;
            else if (item is Common.INammed)
                return INammed;
            else
                return Any;
        }
    }

    public class ViewModelStyleSelector : StyleSelector
    {
        public Style Default { get; set; }
        public Style File { get; set; }
        public Style TextFile { get; set; }
        public Style Hierarchical { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is ContentPresenter)
                return null;

            if (item is ViewModel.TextFileViewModel)
                return File;
            else if (item is ViewModel.HierarchicalViewModel)
                return Hierarchical;
            else if (item is ViewModel.PaneViewModel)
                return Default;
            else
                return null;
        }
    }

    public class BrowserNodeStyleSelector : StyleSelector
    {
        public Style Any { get; set; }
        public Style TextNode { get; set; }
        public Style FileNode { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is ContentPresenter || item is Common.TextObject)
                return Any;
            else if (item is Common.FileSystemNode)
                return FileNode;
            else if (item is Common.TextNode)
                return TextNode;
            else
                return Any;
        }
    }
}
