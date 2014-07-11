/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Toolkit.Primitives;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    public class MapContents : ContentControl, ISupportsConfiguration
    {
        private MapContentsConfiguration _configuration;
        private MapContentsConfigControl _configControl;
        private Legend _legend;
        private const string PART_LEGEND = "Legend";

        public MapContents()
        {
            this.DefaultStyleKey = typeof(MapContents);

            if (MapApplication.Current != null)
                MapApplication.Current.SelectedLayerChanged += Current_SelectedLayerChanged;

            this.DataContext = Configuration;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ConfigureButton = GetTemplateChild(PART_CONFIGUREBUTTON) as Button;
            if (ConfigureButton != null)
                ConfigureButton.Click += ConfigureButton_Click;

            _legend = GetTemplateChild(PART_LEGEND) as Legend;
 
            if (Map == null && MapApplication.Current != null)
                Map = MapApplication.Current.Map;
        }

        private void Current_SelectedLayerChanged(object sender, EventArgs e)
        {
            IMapApplication app = sender as IMapApplication;
            if(app != null)
                SetSelectedLayer(app.SelectedLayer);
        }

        private void SetSelectedLayer(Layer selected)
        {
            if (Map != null)
            {
                foreach (Layer layer in Map.Layers)
                {
                    layer.SetValue(CoreExtensions.IsSelectedProperty, false);
                }
                if (selected != null)
                    selected.SetValue(CoreExtensions.IsSelectedProperty, true);
            }
        }

        #region Properties

        #region Map
        /// <summary>
        /// 
        /// </summary>
        public Map Map
        {
            get { return GetValue(MapProperty) as Map; }
            set { SetValue(MapProperty, value); }
        }

        /// <summary>
        /// Identifies the Map dependency property.
        /// </summary>
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register(
                "Map",
                typeof(Map),
                typeof(MapContents),
                new PropertyMetadata(null, OnMapPropertyChanged));

        public static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapContents source = d as MapContents;
            if (source != null)
            {
                source.HandleLayersChangedSubscription(e);
                source.OnMapChanged();
            }
        }

        private void HandleLayersChangedSubscription(DependencyPropertyChangedEventArgs e)
        {
            Map map = e.OldValue as Map;
            if (map != null && map.Layers != null)
                map.Layers.CollectionChanged -= Layers_CollectionChanged;

            map = e.NewValue as Map;
            if (map != null && map.Layers != null)
                map.Layers.CollectionChanged += Layers_CollectionChanged;
        }

        private void OnMapChanged()
        {
            if (MapApplication.Current != null && MapApplication.Current.SelectedLayer != null)
            {
                Configuration.LayerIds = GetLayerIds(Configuration.ExcludedLayerIds, Configuration.HideBasemaps);

                if (MapApplication.Current.SelectedLayer != null)
                    SetSelectedLayer(MapApplication.Current.SelectedLayer);
            }
        }

        private void Layers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MapContentsConfiguration conf = DataContext as MapContentsConfiguration;
            if (conf != null)
            {
                conf.LayerIds = GetLayerIds(conf.ExcludedLayerIds, conf.HideBasemaps);
                conf.LayerInfos = GetLayerInfos(conf.ExcludedLayerIds);
            }
        }
        #endregion

        public MapContentsConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                    _configuration = new MapContentsConfiguration();

                return _configuration;
            }
            set
            {
                if(_configuration != null)
                    _configuration.PropertyChanged -= Configuration_PropertyChanged;

                _configuration = value;
                this.DataContext = _configuration;

                if (_configuration != null)
                {
                    _configuration.LayerIds = GetLayerIds(_configuration.ExcludedLayerIds, _configuration.HideBasemaps);

                    if (Map != null && _configuration.AllowLayerSelection && !string.IsNullOrWhiteSpace(_configuration.SelectedLayerId))
                    {
                        Layer layer = Map.Layers[_configuration.SelectedLayerId];
                        if (layer != null)
                            MapApplication.Current.SelectedLayer = layer;
                    }
                    _configuration.PropertyChanged += Configuration_PropertyChanged;
                }

            }
        }

        internal void EnableRenameForLayer(Layer layer)
        {
            if (!Configuration.AllowLayerSelection)
                return;

            if (layer == null)
                return;

            if (_legend != null)
            {
                try
                {
                    List<TreeViewItemExtended> treeViewItems = ControlTreeHelper.FindChildrenOfType<TreeViewItemExtended>(_legend, 1);
                    if (treeViewItems != null)
                    {
                        foreach (TreeViewItemExtended item in treeViewItems)
                        {
                            LayerItemViewModel model = item.DataContext as LayerItemViewModel;
                            if (model != null && layer == model.Layer)
                            {
                                StackPanel panel = ControlTreeHelper.FindChildOfType<StackPanel>(item, 6);
                                if (panel != null && panel.Name == "layerStackPanel")
                                    panel.SetValue(CoreExtensions.IsEditProperty, true);

                                return;
                            }
                        }
                    }
                }
                catch { }
            }
        }
        private ICommand _enableRenameSelectedLayerCommand;
        public ICommand EnableRenameSelectedLayerCommand
        {
            get
            {
                if (_enableRenameSelectedLayerCommand == null)
                    _enableRenameSelectedLayerCommand = new RenameSelectedLayerCommand() { MapContents = this };

                return _enableRenameSelectedLayerCommand;
            }
        }

        #region SelectedLayer
        /// <summary>
        /// 
        /// </summary>
        public Layer SelectedLayer
        {
            get { return MapApplication.Current.SelectedLayer; }
        }

        #endregion

        #region Styles
        #region ScrollViewerStyle
        /// <summary>
        /// 
        /// </summary>
        public Style ScrollViewerStyle
        {
            get { return (Style)GetValue(ScrollViewerStyleProperty); }
            set { SetValue(ScrollViewerStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Map dependency property.
        /// </summary>
        public static readonly DependencyProperty ScrollViewerStyleProperty =
            DependencyProperty.Register(
                "ScrollViewerStyle",
                typeof(Style),
                typeof(MapContents), null);
        #endregion

        #region ToggleButtonStyle
        /// <summary>
        /// 
        /// </summary>
        public Style ToggleButtonStyle
        {
            get { return (Style)GetValue(ToggleButtonStyleProperty); }
            set { SetValue(ToggleButtonStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Map dependency property.
        /// </summary>
        public static readonly DependencyProperty ToggleButtonStyleProperty =
            DependencyProperty.Register(
                "ToggleButtonStyle",
                typeof(Style),
                typeof(MapContents), null);
        #endregion

        #region SelectionOutlineColorBrush
        /// <summary>
        /// 
        /// </summary>
        public Brush SelectionOutlineColorBrush
        {
            get { return (Brush)GetValue(SelectionOutlineColorBrushProperty); }
            set { SetValue(SelectionOutlineColorBrushProperty, value); }
        }

        public static readonly DependencyProperty SelectionOutlineColorBrushProperty =
            DependencyProperty.Register(
                "SelectionOutlineColorBrush",
                typeof(Brush),
                typeof(MapContents),
                new PropertyMetadata(null));

        #endregion

        #region SelectionColorBrush
        /// <summary>
        /// 
        /// </summary>
        public Brush SelectionColorBrush
        {
            get { return (Brush)GetValue(SelectionColorBrushProperty); }
            set { SetValue(SelectionColorBrushProperty, value); }
        }

        public static readonly DependencyProperty SelectionColorBrushProperty =
            DependencyProperty.Register(
                "SelectionColorBrush",
                typeof(Brush),
                typeof(MapContents),
                new PropertyMetadata(null));

        #endregion

        #endregion

        #endregion

        #region ISupportsConfiguration

        private const string PART_CONFIGUREBUTTON = "ConfigureButton";
        internal Button ConfigureButton { get; private set; }

        public bool IsEditMode
        {
            get
            {
                if (MapApplication.Current != null)
                    return MapApplication.Current.IsEditMode && (bool)this.GetValue(ElementExtensions.IsConfigurableProperty);

                return false;
            }
        }

        public void Configure()
        {
            ConfigureButton_Click(null, null);
        }

        public void LoadConfiguration(string configData)
        {
            Configuration = MapContentsConfiguration.FromString(configData);
        }

        void Configuration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MapContentsConfiguration conf = sender as MapContentsConfiguration;
            if (conf != null)
            {

                if (e.PropertyName == "ExcludedLayerIds" || e.PropertyName == "HideBasemaps")
                {
                   conf.LayerIds = GetLayerIds(conf.ExcludedLayerIds, conf.HideBasemaps);
                }
                else if (e.PropertyName == "Mode" || e.PropertyName == "ShowLayersVisibleAtScale")
                {
                    if (_legend != null)
                        _legend.Refresh();
                }
            }
        }

        private string[] GetLayerIds(string[] exludedList, bool hideBasemaps)
        {
            if (Map != null && Map.Layers != null)
            {
                List<string> layerIds = new List<string>();
                foreach (Layer lay in Map.Layers)
                {
                     //always filter out layers without ID
                    if (!string.IsNullOrWhiteSpace(lay.ID) && LayerProperties.GetIsVisibleInMapContents(lay))
                    {
                        //only filter out basemaps for configurable MapContents controls
                        if ((bool)this.GetValue(ElementExtensions.IsConfigurableProperty) &&
                            hideBasemaps && (bool)lay.GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty))
                        continue;
                        else
                            layerIds.Add(lay.ID);
                    }

                }
                //only filter using excludedList for configurable MapContents controls
                if ((bool)this.GetValue(ElementExtensions.IsConfigurableProperty) && exludedList != null)
                    return layerIds.Except<string>(exludedList).ToArray();
                else
                    return layerIds.ToArray();
            }

            return null;
        }
        private List<LayerInfo> GetLayerInfos(string[] exludedList)
        {
            //do not set layer infos as there will never map contents configuration if not isconfigurable
            if (!(bool)this.GetValue(ElementExtensions.IsConfigurableProperty))
                return null;

            if (Map != null && Map.Layers != null)
            {
                List<LayerInfo> layers = new List<LayerInfo>();
                foreach (Layer lay in Map.Layers)
                {
                    if (!(bool)lay.GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty))
                        layers.Add(new LayerInfo() { IsExcluded = exludedList != null && exludedList.Contains(lay.ID), Layer = lay });
                }
                return layers;
            }
            return null;
        }

        public string SaveConfiguration()
        {
            return Configuration.ToString();
        }

        void ConfigureButton_Click(object sender, RoutedEventArgs e)
        {
            if (_configControl == null)
            {
                _configControl = new MapContentsConfigControl()
                    {
						OkButtonText = ESRI.ArcGIS.Mapping.Controls.MapContents.Resources.Strings.Done,
                    };
                _configControl.OkButtonCommand = new DelegateCommand((args) =>
                {
                    MapApplication.Current.HideWindow(_configControl);
                });
            }
            _configControl.DataContext = Configuration;
            Configuration.LayerInfos = GetLayerInfos(Configuration.ExcludedLayerIds);
            MapApplication.Current.ShowWindow(ESRI.ArcGIS.Mapping.Controls.MapContents.Resources.Strings.ConfigureMapContents, _configControl, true, null, null, WindowType.DesignTimeFloating);
        }

        #endregion
    }
}
