/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Application.Layout;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Mapping.Controls;
using System.Windows.Controls.Primitives;
using ESRI.ArcGIS.Mapping.Controls.MapContents;
using System.Windows.Data;

namespace ESRI.ArcGIS.Mapping.Builder.Controls
{
    public class MapContentLayerConfigurationHost : Control
    {
        private static List<MapContentLayerConfigurationHost> _instances = new List<MapContentLayerConfigurationHost>();

        private const string PART_MAP_CONTENT_CONTROL = "BuilderMapContentsControl";
        private const string PART_MAP_CONTENT_CONTAINER_CONTROL = "MapContentsControlContainer";
        private const string PART_MAP_CONTENT_CONTROL_LAYOUT = "MapContentsControlLayout";
        private const string PART_CONFIGURE_LAYER_CONTROL = "LayerConfiguration";
        private const string PART_CONFIGURE_LAYER_CONTROL_LAYOUT = "LayerConfigurationLayout";
        private const string PART_CONFIGURE_COMPLETE_BUTTON = "btnConfigurationComplete";
        private const string PART_MAP_CONTENTS_LAYER_CONFIGURATION_LAYOUT = "MapContentLayerConfigurationLayout";
        private const string PART_TOOLPANEL = "LayerConfigToolbar";
        private const string PART_ALLLAYERSTOOLPANEL = "AllLayerConfigToolbar";
        private const string PART_LAYERCONFIGPANEL = "LayerConfigPanel";
        private static string PART_EDIT_MODE_LAYER_CONFIGURATION_CONTEXT_MENU = "EditModeLayerConfigurationContextMenu";
        
        private Grid layerConfigurationLayout;
        private Grid mapContentsLayout;
        private Grid mapContentLayerConfigurationLayout;
        private Button btnConfigurationComplete;
        private ToolPanel _layerConfigToolbar;
        private ToolPanel _allLayersConfigToolbar;
        private FrameworkElement _layerConfigPanel;
        private ToolPanel _mapContentsContenxtMenuToolbar;
        private Grid _layerConfigurationGrid;
        private int _layerConfigurationIndex = 0;

        // These members were made public so they could be accessed by the Builder application. The toolbar control was
        // added so that it can be used instead of the traditional one used when this control resides within a Viewer
        // application and has its content determined by an external XML file
        public MapContents MapContentControl;
        public ContentControl MapContentControlContainer;
        public LayerConfiguration LayerConfiguration;
        public ToolPanel ContextMenuToolbar { get; set; }

        public MapContentLayerConfigurationHost()
        {
            this.DefaultStyleKey = typeof(MapContentLayerConfigurationHost);
            GoToMapContentsCommand = new DelegateCommand(goToMapContentsClick, canGoToMapContentsClick);
            GoToLayerConfigurationCommand = new DelegateCommand(goToConfigurationClick, canGoToConfigurationClick);
            _instances.Add(this);
        }

        public override void OnApplyTemplate()
        {
            MapContentControlContainer = GetTemplateChild(PART_MAP_CONTENT_CONTAINER_CONTROL) as ContentControl;
            if (MapContentControlContainer != null)
            {
                MapContentControl = new MapContents();
                MapContentControl.Name = PART_MAP_CONTENT_CONTROL;
                MapContentControl.Map = View.Map;
                MapContentControl.Configuration = new MapContentsConfiguration { ContextMenuToolPanelName = "EditModeLayerConfigurationContextMenu", Mode=Mode.TopLevelLayersOnly };

                if (MapContentControlContainer.Resources != null)
                {
                    MapContentControl.ScrollViewerStyle = MapContentControlContainer.Resources["BuilderMapContentsScrollViewerStyle"] as Style;
                    MapContentControl.SelectionColorBrush = MapContentControlContainer.Resources["BuilderSelectedLayerColorBrush"] as Brush;
                    MapContentControl.SelectionOutlineColorBrush = MapContentControlContainer.Resources["BuilderSelectedLayerOutlineColorBrush"] as Brush;
                    MapContentControl.ToggleButtonStyle = MapContentControlContainer.Resources["MapContentsControlNodeToggleButton"] as Style;
                }

                // Push foreground and background to map contents
                Binding b = new Binding("Foreground") { Source = this };
                MapContentControl.SetBinding(MapContents.ForegroundProperty, b);
                b = new Binding("Background") { Source = this };
                MapContentControl.SetBinding(MapContents.BackgroundProperty, b);

                MapContentControlContainer.Content = MapContentControl;
            }

            layerConfigurationLayout = GetTemplateChild(PART_CONFIGURE_LAYER_CONTROL_LAYOUT) as Grid;
            LayerConfiguration = GetTemplateChild("LayerConfigurationDialog") as LayerConfiguration;
            if (LayerConfiguration != null)
                LayerConfiguration.View = View.Instance;

            mapContentLayerConfigurationLayout = GetTemplateChild(PART_MAP_CONTENTS_LAYER_CONFIGURATION_LAYOUT) as Grid;
            if (mapContentLayerConfigurationLayout != null)
            {
                mapContentLayerConfigurationLayout.Loaded -= mapContentLayerConfigurationLayout_Loaded;
                mapContentLayerConfigurationLayout.Loaded += mapContentLayerConfigurationLayout_Loaded;
                mapContentLayerConfigurationLayout.SizeChanged -= mapContentLayerConfigurationLayout_SizeChanged;
                mapContentLayerConfigurationLayout.SizeChanged += mapContentLayerConfigurationLayout_SizeChanged;
            }

            mapContentsLayout = GetTemplateChild(PART_MAP_CONTENT_CONTROL_LAYOUT) as Grid;
            btnConfigurationComplete = GetTemplateChild(PART_CONFIGURE_COMPLETE_BUTTON) as Button;
            _layerConfigToolbar = GetTemplateChild(PART_TOOLPANEL) as ToolPanel;
            _allLayersConfigToolbar = GetTemplateChild(PART_ALLLAYERSTOOLPANEL) as ToolPanel;
            _mapContentsContenxtMenuToolbar = GetTemplateChild(PART_EDIT_MODE_LAYER_CONFIGURATION_CONTEXT_MENU) as ToolPanel;
            _layerConfigPanel = GetTemplateChild(PART_LAYERCONFIGPANEL) as FrameworkElement;
            InitializeToolbars();

            // hide the layer configuration elements until we need them
            RemoveLayerConfigurationVisualElements();
        }

        void mapContentLayerConfigurationLayout_Loaded(object sender, RoutedEventArgs e)
        {
            if (mapContentLayerConfigurationLayout != null)
            {
                mapContentLayerConfigurationLayout.Clip = new RectangleGeometry()
                {
                    Rect = new Rect(new Point(0, 0), new Size(mapContentLayerConfigurationLayout.ActualWidth, mapContentLayerConfigurationLayout.ActualHeight))
                };
            }
        }

        private void InitializeToolbars()
        {
            List<ButtonBase> toolbarCommands = null;
            if (_layerConfigToolbar != null)
            {
                toolbarCommands = CreateButtonsForLayerConfigurationToolbar();
                if (toolbarCommands != null)
                {
                    foreach (ButtonBase btnBase in toolbarCommands)
                    {
                        _layerConfigToolbar.AddToolButton(btnBase.DataContext as ButtonDisplayInfo, btnBase.Command, _layerConfigToolbar.ToolPanelItems);
                    }
                }
            }

            if (_allLayersConfigToolbar != null)
            {
                toolbarCommands = CreateButtonsForAllLayersConfigurationToolbar();
                if (toolbarCommands != null)
                {
                    foreach (ButtonBase btnBase in toolbarCommands)
                    {
                        _allLayersConfigToolbar.AddToolButton(btnBase.DataContext as ButtonDisplayInfo, btnBase.Command, _allLayersConfigToolbar.ToolPanelItems);
                    }
                }
            }

            if (_mapContentsContenxtMenuToolbar != null)
            {
                toolbarCommands = CreateButtonsForContextMenu();
                if (toolbarCommands != null)
                {
                    foreach (ButtonBase btnBase in toolbarCommands)
                    {
                        _mapContentsContenxtMenuToolbar.AddToolButton(btnBase.DataContext as ButtonDisplayInfo, btnBase.Command, _mapContentsContenxtMenuToolbar.ToolPanelItems);
                    }
                    AddToolPanels();
                }
            }
        }

        internal void AddToolPanels()
        {
            if (_mapContentsContenxtMenuToolbar != null && ToolPanels.Current[_mapContentsContenxtMenuToolbar.ContainerName] == null)
            {
                ToolPanels.Current.Add(_mapContentsContenxtMenuToolbar);
            }
        }

        private List<ButtonBase> CreateButtonsForLayerConfigurationToolbar()
        {
            List<ButtonBase> buttons = new List<ButtonBase>();

            buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ConfigureCurrentlySelectedLayer,
                "Images/toolbar/ConfigureLayerCommand16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.Configure,
                GoToLayerConfigurationCommand));

            buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.RemovesCurrentlySelectedLayer,
				"Images/toolbar/DeleteLayerCommand16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.Remove,
                new DeleteLayerCommand()));

            buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.BringsCurrentlySelectedLayerForward,
                "Images/toolbar/MoveLayerDownCommand16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.BringForward,
                new MoveLayerDownCommand()));

            buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.SendsCurrentlySelectedLayerBackward,
                "Images/toolbar/MoveLayerUpCommand16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.SendBackward,
                new MoveLayerUpCommand()));

            buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.RefreshCurrentlySelectedLayer,
                "Images/toolbar/RefreshLayerCommand16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.Refresh,
                new RefreshLayerCommand()));

            buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ZoomToCurrentlySelectedLayer,
                "Images/toolbar/GoToLayerCommand16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.GoTo,
                new GoToLayerCommand()));

            buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ViewInformationAboutSelectedLayerDatasource,
                "Images/toolbar/ServiceDetailsCommand16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ServiceDetails,
                new ServiceDetailsCommand()));

            return buttons;
        }

        private List<ButtonBase> CreateButtonsForAllLayersConfigurationToolbar()
        {
            List<ButtonBase> buttons = new List<ButtonBase>();

            buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ConfigureProxyUsedToAccessSecureServicesForApplication,
			"/ESRI.ArcGIS.Mapping.Controls;component/Images/toolbar/LayerLocked16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ConfigureSecureServices,
            new ConfigureProxyCommand()));


            return buttons;
        }

        private List<ButtonBase> CreateButtonsForContextMenu()
        {
            List<ButtonBase> buttons = new List<ButtonBase>();

            buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ConfigureCurrentlySelectedLayer,
				"Images/toolbar/ConfigureLayerCommand16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.Configure,
                GoToLayerConfigurationCommand));

            buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ZoomToCurrentlySelectedLayer,
                "Images/toolbar/GoToLayerCommand16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.GoTo,
                new GoToLayerCommand()));

            buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.RefreshCurrentlySelectedLayer,
                "Images/toolbar/RefreshLayerCommand16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.Refresh,
                new RefreshLayerCommand()));

            if (MapContentControl != null)
            {
                buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.RenameCurrentlySelectedLayer,
					"Images/toolbar/RenameLayerCommand16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.Rename,
                    MapContentControl.EnableRenameSelectedLayerCommand));
            }

            buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.RemovesCurrentlySelectedLayer,
				"Images/toolbar/DeleteLayerCommand16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.Remove,
                new DeleteLayerCommand()));

            buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.BringsCurrentlySelectedLayerForward,
                "Images/toolbar/MoveLayerDownCommand16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.BringForward,
                new MoveLayerDownCommand()));

            buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.SendsCurrentlySelectedLayerBackward,
                "Images/toolbar/MoveLayerUpCommand16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.SendBackward,
                new MoveLayerUpCommand()));

            buttons.Add(CreateLayerConfigurationToolbarButton(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ViewInformationAboutSelectedLayerDatasource,
				"Images/toolbar/ServiceDetailsCommand16.png", ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ServiceDetails,
                new ServiceDetailsCommand()));

            return buttons;
        }

        private ButtonBase CreateLayerConfigurationToolbarButton(string desc, string icon, string label, ICommand cmd, object parameter = null)
        {
            RTLHelper helper = Application.Current.Resources["RTLHelper"] as RTLHelper;
            Debug.Assert(helper != null);
            FlowDirection flowDirection = FlowDirection.LeftToRight;
            if (helper != null)
                flowDirection = helper.FlowDirection;

            ButtonBase btnBase = new Button()
            {
                DataContext = new ButtonDisplayInfo() { Description = desc, Icon = icon, Label = label },
                Command = cmd,
                FlowDirection = flowDirection,
            };
            return btnBase;
        }


        void mapContentLayerConfigurationLayout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (mapContentLayerConfigurationLayout != null)
            {
                mapContentLayerConfigurationLayout.Clip = new RectangleGeometry()
                {
                    Rect = new Rect(new Point(0, 0), new Size(mapContentLayerConfigurationLayout.ActualWidth, mapContentLayerConfigurationLayout.ActualHeight))
                };
            }
        }

        public void GoToMapContent(bool animate)
        {
            if (layerConfigurationLayout != null && mapContentsLayout != null
                && mapContentsLayout.Opacity == 0)
            {
                FlipFrameworkElementsCommand cmd = new FlipFrameworkElementsCommand()
                {
                    FrontElement = layerConfigurationLayout,
                    BackElement = mapContentsLayout,
                    Rotation = FlipFrameworkElementsCommand.RotationDirection.LeftToRight,
                    Duration = new Duration(new TimeSpan(0, 0, 0, 0, animate ? 1200 : 0)),
                };
                cmd.RotationCompleted += (s, e) => RemoveLayerConfigurationVisualElements();
                if (cmd.CanExecute(null))
                    cmd.Execute(null);

                if (_layerConfigPanel != null)
                {
                    ShowLayerConfigurationToolPanelCommand showLayerToolbar = new ShowLayerConfigurationToolPanelCommand();
                    showLayerToolbar.Duration = new Duration(new TimeSpan(0, 0, 0, 0, animate ? 1300 : 100));
                    showLayerToolbar.Show = true;
                    showLayerToolbar.LayerConfigurationToolPanel = _layerConfigPanel;
                    if (showLayerToolbar.CanExecute(null))
                        showLayerToolbar.Execute(null);
                }
            }
        }
        private void ShowLayerConfiguration(bool animate)
        {
            if (layerConfigurationLayout != null && mapContentsLayout != null
                && layerConfigurationLayout.Opacity == 0)
            {
                RestoreLayerConfigurationVisualElements();

                if (_layerConfigPanel != null)
                {
                    ShowLayerConfigurationToolPanelCommand showLayerToolbar = new ShowLayerConfigurationToolPanelCommand();
                    showLayerToolbar.Duration = new Duration(new TimeSpan(0, 0, 0, 0));
                    showLayerToolbar.Show = false;
                    showLayerToolbar.LayerConfigurationToolPanel = _layerConfigPanel;
                    if (showLayerToolbar.CanExecute(null))
                        showLayerToolbar.Execute(null);
                }

                FlipFrameworkElementsCommand cmd = new FlipFrameworkElementsCommand()
                {
                    FrontElement = mapContentsLayout,
                    BackElement = layerConfigurationLayout,
                    Rotation = FlipFrameworkElementsCommand.RotationDirection.RightToLeft,
                    Duration = new Duration(new TimeSpan(0, 0, 0, 0, animate ? 1200 : 0)),
                };
                if (cmd.CanExecute(null))
                    cmd.Execute(null);
            }
        }

        private void RemoveLayerConfigurationVisualElements()
        {
            Grid container = GetTemplateChild("MapContentLayerConfigurationLayout") as Grid;
            if (container == null) return;

            if (LayerConfiguration != null)
                LayerConfiguration.View = null;

            // Save off the layer configuration control elements and 
            // the index in the children collection
            for (int idx = 0; idx < container.Children.Count; idx++)
            {
                Grid config = container.Children[idx] as Grid;
                if (config != null && config.Name == "LayerConfigurationLayout")
                {
                    _layerConfigurationGrid = config;
                    _layerConfigurationIndex = idx;
                    container.Children.Remove(config);
                    return;
                }
            }
        }
        private void RestoreLayerConfigurationVisualElements()
        {
            if (_layerConfigurationGrid == null) return;

            if (LayerConfiguration != null)
                LayerConfiguration.View = View.Instance;

            Grid container = GetTemplateChild("MapContentLayerConfigurationLayout") as Grid;
            if (container == null) return;

            // if its there already, do nothing
            if (container.Children.Select(element => element as Grid)
                .Any(config => config != null && config.Name == "LayerConfigurationLayout"))
            {
                return;
            }
            container.Children.Insert(_layerConfigurationIndex, _layerConfigurationGrid);
        }

        #region View
        /// <summary>
        /// 
        /// </summary>
        public View View
        {
            get 
            {
                return View.Instance;
            }
        }

        #endregion

        internal virtual void OnLayerCongifurationStarted(LayerEventArgs args)
        {
            if (LayerConfigurationStarted != null)
                LayerConfigurationStarted(this, args);
        }

        public event EventHandler<LayerEventArgs> LayerConfigurationStarted;

        protected virtual void OnControlInitializationCompleted()
        {
            if (ControlInitializationCompleted != null)
                ControlInitializationCompleted(this, null);
        }

        public event EventHandler ControlInitializationCompleted;

        #region GoToMapContents Command
        private void goToMapContentsClick(object commandParameter)
        {
            GoToMapContent(true);
        }

        private bool canGoToMapContentsClick(object commandParameter)
        {
            return true;
        }

        public ICommand GoToMapContentsCommand
        {
            get { return (ICommand)GetValue(GoToMapContentsCommandProperty); }
            set { SetValue(GoToMapContentsCommandProperty, value); }
        }

        public static readonly DependencyProperty GoToMapContentsCommandProperty =
            DependencyProperty.Register("GoToMapContentsCommand", typeof(ICommand), typeof(MapContentLayerConfigurationHost), null);

        #endregion

        #region GoToLayerConfiguration Command
        private void goToConfigurationClick(object commandParameter)
        {
            ShowLayerConfiguration(true);
        }

        private bool canGoToConfigurationClick(object commandParameter)
        {
            return MapApplication.Current.SelectedLayer != null;
        }

        public ICommand GoToLayerConfigurationCommand
        {
            get { return (ICommand)GetValue(GoToLayerConfigurationCommandProperty); }
            set { SetValue(GoToLayerConfigurationCommandProperty, value); }
        }

        public static readonly DependencyProperty GoToLayerConfigurationCommandProperty =
            DependencyProperty.Register("GoToLayerConfigurationCommand", typeof(ICommand), typeof(MapContentLayerConfigurationHost), null);

        #endregion
    }
}
