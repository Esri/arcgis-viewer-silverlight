/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client.Application.Layout;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Application.Layout.Converters;
using ESRI.ArcGIS.Client.Toolkit.Primitives;
using ESRI.ArcGIS.Mapping.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    public class ContextMenuOpenCloseBehavior : Behavior<FrameworkElement>
    {
        private ToolPanel _toolPanel = null;
        private static ImageUrlResolver _urlResolver = null;

        private ContextMenu _contextMenu;

        #region ContextMenuStyle
        public Style ContextMenuStyle
        {
            get { return (Style)GetValue(ContextMenuStyleProperty); }
            set { SetValue(ContextMenuStyleProperty, value); }
        }
        public static readonly DependencyProperty ContextMenuStyleProperty =
            DependencyProperty.Register(
                "ContextMenuStyle",
                typeof(Style),
                typeof(ContextMenuOpenCloseBehavior),
                new PropertyMetadata(null));

        #endregion

        #region ContextMenuBackground

        /// <summary>
        /// Backing DependencyProperty for the <see cref="ContextMenuBackground"/> property
        /// </summary>
        public static readonly DependencyProperty ContextMenuBackgroundProperty = DependencyProperty.Register(
            "ContextMenuBackground", typeof(Brush), typeof(ContextMenuOpenCloseBehavior), 
            new PropertyMetadata(new SolidColorBrush(Colors.Gray)));

        /// <summary>
        /// Gets or sets the brush used for the background of the context menu
        /// </summary>
        public Brush ContextMenuBackground
        {
            get { return GetValue(ContextMenuBackgroundProperty) as Brush; }
            set { SetValue(ContextMenuBackgroundProperty, value); }
        }

        #endregion

        #region ContextMenuForeground

        /// <summary>
        /// Backing DependencyProperty for the <see cref="ContextMenuForeground"/> property
        /// </summary>
        public static readonly DependencyProperty ContextMenuForegroundProperty = DependencyProperty.Register(
            "ContextMenuForeground", typeof(Brush), typeof(ContextMenuOpenCloseBehavior),  
            new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        /// <summary>
        /// Gets or sets the brush used for the foreground of the context menu
        /// </summary>
        public Brush ContextMenuForeground
        {
            get { return GetValue(ContextMenuForegroundProperty) as Brush; }
            set { SetValue(ContextMenuForegroundProperty, value); }
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.MouseRightButtonUp -= AssociatedObject_MouseRightButtonUp;
                this.AssociatedObject.MouseRightButtonUp += AssociatedObject_MouseRightButtonUp;
            }

            if (_urlResolver == null)
                _urlResolver = new ImageUrlResolver();
        }

        void AssociatedObject_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MapContents cont = ControlTreeHelper.FindAncestorOfType<MapContents>(AssociatedObject);
            if (cont == null)
                return;

            MapContentsConfiguration conf = cont.DataContext as MapContentsConfiguration;
            if (conf == null)
                return;

            if (ToolPanels.Current != null && !string.IsNullOrWhiteSpace(conf.ContextMenuToolPanelName))
                _toolPanel = ToolPanels.Current[conf.ContextMenuToolPanelName];

            if (_toolPanel == null)
                return;

            _contextMenu = new ContextMenu();
            if (ContextMenuStyle != null)
                _contextMenu.Style = ContextMenuStyle;

            // Bind background and foreground properties to context menu
            Binding b = new Binding("ContextMenuBackground") { Source = this };
            _contextMenu.SetBinding(ContextMenu.BackgroundProperty, b);

            b = new Binding("ContextMenuForeground") { Source = this };
            _contextMenu.SetBinding(ContextMenu.ForegroundProperty, b);

            // set the control to the current culture settings (LTR/RTL)
            RTLHelper helper = System.Windows.Application.Current.Resources["RTLHelper"] as RTLHelper;
            Debug.Assert(helper != null);
            if (helper != null)
                _contextMenu.FlowDirection = helper.FlowDirection;

            foreach (FrameworkElement button in _toolPanel.ToolPanelItems)
            {
                System.Windows.Controls.Primitives.ButtonBase btnBase = button as System.Windows.Controls.Primitives.ButtonBase;
                if (btnBase != null)
                {
                    MenuItem item = new MenuItem();
                    ButtonDisplayInfo info = btnBase.DataContext as ButtonDisplayInfo;
                    if (info != null)
                    {
                        item.Header = info.Label;
                        if (!string.IsNullOrEmpty(info.Icon))
                        {
                            Image image = new Image();
                            Binding binding = new Binding("Icon") { Converter = _urlResolver };
                            image.SetBinding(Image.SourceProperty, binding);
                            image.DataContext = info;
                            item.Icon = image;
                        }
                        item.Command = btnBase.Command;
                        _contextMenu.Items.Add(item);
                    }
                }
                else
                    _contextMenu.Items.Add(new Separator());
            }
            
            Point point = e.GetPosition(null);
            if (point != null)
            {
                _contextMenu.HorizontalOffset = point.X;
                _contextMenu.VerticalOffset = point.Y;
            }  

            LayerItemViewModel layerViewModel = AssociatedObject.DataContext as LayerItemViewModel;
            if (layerViewModel != null && layerViewModel.Layer != null)
            {
                if (!CoreExtensions.GetIsSelected(layerViewModel.Layer))
                    SetSelectedLayer(layerViewModel.Layer);
            }
            if (_contextMenu.Items.Count > 0)
            {
                _contextMenu.IsOpen = true;

                if (_contextMenu.FlowDirection == FlowDirection.RightToLeft)
                {
                    // Now that the popup is open, we can update the layout measurements so that
                    // the ActualWidth is available.
                    _contextMenu.UpdateLayout();
                    _contextMenu.HorizontalOffset -= _contextMenu.ActualWidth;
                }
            }
        }

        private void SetSelectedLayer(Layer selectedLayer)
        {
            if (selectedLayer != null && MapApplication.Current != null &&
                selectedLayer != MapApplication.Current.SelectedLayer)
                MapApplication.Current.SelectedLayer = selectedLayer;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
                this.AssociatedObject.MouseRightButtonUp -= AssociatedObject_MouseRightButtonUp;
        }
    }
}
