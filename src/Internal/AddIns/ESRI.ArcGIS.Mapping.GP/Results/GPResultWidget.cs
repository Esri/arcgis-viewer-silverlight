/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.GP.ParameterSupport;

namespace ESRI.ArcGIS.Mapping.GP
{
    [TemplatePart(Name = PART_ParamContainer, Type = typeof(Grid))]
    public class GPResultWidget : Control
    {
        private const string PART_ParamContainer = "ParamContainer";
        Grid ParamContainer;

        public GPResultWidget()
        {
            this.DefaultStyleKey = typeof(GPResultWidget);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ParamContainer = GetTemplateChild(PART_ParamContainer) as Grid;
            BuildUI();
        }


        #region Properties
        public List<GPParameter> Results
        {
            get { return (List<GPParameter>)GetValue(ResultsProperty); }
            set { SetValue(ResultsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LatestResults.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResultsProperty =
            DependencyProperty.Register("LatestResults", typeof(List<GPParameter>), typeof(GPResultWidget), new PropertyMetadata(OnPropertyChanged));

        public List<ParameterConfig> ParameterConfigs
        {
            get { return (List<ParameterConfig>)GetValue(ParameterConfigsProperty); }
            set { SetValue(ParameterConfigsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ParameterConfigs.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParameterConfigsProperty =
            DependencyProperty.Register("ParameterConfigs", typeof(List<ParameterConfig>), typeof(GPResultWidget), new PropertyMetadata(OnPropertyChanged));

        public List<Exception> Errors
        {
            get { return (List<Exception>)GetValue(ErrorsProperty); }
            set { SetValue(ErrorsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Errors.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorsProperty =
            DependencyProperty.Register("Errors", typeof(List<Exception>), typeof(GPResultWidget), new PropertyMetadata(OnPropertyChanged));

        
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GPResultWidget obj = (GPResultWidget)d;
            obj.BuildUI();
        }

        public Map Map
        {
            get { return (Map)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Map.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register("Map", typeof(Map), typeof(GPResultWidget), new PropertyMetadata(OnPropertyChanged));

        public bool HasSimpleResults
        {
            get { return (bool)GetValue(HasSimpleResultsProperty); }
            set { SetValue(HasSimpleResultsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasSimpleResults.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasSimpleResultsProperty =
            DependencyProperty.Register("HasSimpleResults", typeof(bool), typeof(GPResultWidget), null);

        public Dictionary<string,string> InputLayers
        {
            get { return (Dictionary<string,string>)GetValue(InputLayersProperty); }
            set { SetValue(InputLayersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InputLayers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputLayersProperty =
            DependencyProperty.Register("InputLayers", typeof(Dictionary<string,string>), typeof(GPResultWidget), new PropertyMetadata(OnPropertyChanged));

        public ObservableCollection<string> LayerOrder
        {
            get { return (ObservableCollection<string>)GetValue(LayerOrderProperty); }
            set { SetValue(LayerOrderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LayerOrder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayerOrderProperty =
            DependencyProperty.Register("LayerOrder", typeof(ObservableCollection<string>), typeof(GPResultWidget), new PropertyMetadata(OnPropertyChanged));

        public string JobID
        {
            get { return (string)GetValue(JobIDProperty); }
            set { SetValue(JobIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for JobID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty JobIDProperty =
            DependencyProperty.Register("JobID", typeof(string), typeof(GPResultWidget), null);

        public string TaskUrl
        {
            get { return (string)GetValue(TaskUrlProperty); }
            set { SetValue(TaskUrlProperty, value); }
        }

        public static readonly DependencyProperty TaskUrlProperty =
            DependencyProperty.Register("TaskUrl", typeof(string), typeof(GPResultWidget), null);

        #endregion

        bool inputLayerInfoAvailable()
        {
            foreach (ParameterConfig config in ParameterConfigs)
            {
                if (config is FeatureLayerParameterConfig && config.Input)
                {
                    if (InputLayers == null || InputLayers.Count < 1)
                        return false;
                    return true;
                }
            }
            return true;
        }

        public static bool AreDisplayable(List<ParameterConfig> parameterConfigs)
        {
            foreach (ParameterConfig config in parameterConfigs)
            {
                switch (config.Type)
                {
                    case GPParameterType.Boolean:
                    case GPParameterType.Double:
                    case GPParameterType.Long:
                    case GPParameterType.String:
                    case GPParameterType.Date:
                    case GPParameterType.LinearUnit:
                    case GPParameterType.RecordSet:
                    case GPParameterType.DataFile:
                    case GPParameterType.RasterData:
                    case GPParameterType.RasterDataLayer:
                        return true;
                }
            }
            return false;
 
        }

        private void BuildUI()
        {
            if (ParamContainer == null || ParameterConfigs == null ||
                ParameterConfigs.Count < 1) return;
            if ((Results == null || Results.Count < 1) && (Errors == null || Errors.Count < 1))
                return;
            if (!inputLayerInfoAvailable())
                return;
            ParamContainer.Children.Clear();
            ParamContainer.ColumnDefinitions.Clear();
            ParamContainer.RowDefinitions.Clear();
            ParamContainer.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            ParamContainer.ColumnDefinitions.Add(new ColumnDefinition() );// { Width = new GridLength(0, GridUnitType.Star) });
            HasSimpleResults = false;
            layerParamNameIDLookup = InputLayers != null ?
                new Dictionary<string, string>(InputLayers) : new Dictionary<string, string>();
            int pendingGPResultImageLayers = 0;
            #region Results
            if (Results != null && Results.Count > 0)
            {
                #region GP mapserver result
                MapServiceLayerParameterConfig mapServiceLayerParameterConfig = ParameterConfigs.FirstOrDefault(p => p.Type == GPParameterType.MapServiceLayer) as MapServiceLayerParameterConfig;
                if (mapServiceLayerParameterConfig != null && mapServiceLayerParameterConfig.SupportsJobResource)
                {
                    string t = "/rest/services/";
                    string url = string.Format("{0}/{1}/MapServer/jobs/{2}", TaskUrl.Substring(0, TaskUrl.IndexOf(t, StringComparison.OrdinalIgnoreCase) + t.Length - 1), mapServiceLayerParameterConfig.Name, JobID);

                    ArcGISDynamicMapServiceLayer layer = new ArcGISDynamicMapServiceLayer { Url = url,  };
                    layer.SetValue(ESRI.ArcGIS.Mapping.Core.LayerExtensions.ExcludeSerializationProperty, true);
                    addToMap(mapServiceLayerParameterConfig, layer);
                }
                #endregion

                #region display each result
                foreach (ParameterConfig config in ParameterConfigs)
                {
                    MapServiceLayerParameterConfig cfg;
                    if (config.Type == GPParameterType.MapServiceLayer && (cfg = config as MapServiceLayerParameterConfig) != null && !cfg.SupportsJobResource)
                    {
                        pendingGPResultImageLayers++;
                        Geoprocessor gp = new Geoprocessor(TaskUrl);
                        gp.OutputSpatialReference = Map.SpatialReference.Clone();
                        gp.GetResultImageLayerCompleted += (s, e) =>
                        {
                            GPResultImageLayer gpImageLayer = e.GPResultImageLayer;
                            setLayerProps(cfg, gpImageLayer);
                            Map.Layers.Add(gpImageLayer);
                            layerParamNameIDLookup.Add(cfg.Name, gpImageLayer.ID);
                            pendingGPResultImageLayers--;
                            if (layerParamNameIDLookup.Count > 1 && pendingGPResultImageLayers == 0)
                                LayerOrderer.OrderLayers(Map, LayerOrder, layerParamNameIDLookup);
                        };
                        gp.GetResultImageLayerAsync(JobID, cfg.Name);
                        continue;
                    }

                    GPParameter param = getParameter(config.Name);
                    if (param == null) continue;
                    string value = ParameterBase.ParameterToDisplayString(config.Type, param);
                    
                    switch (config.Type)
                    {
                        case GPParameterType.Boolean:
                        case GPParameterType.Double:
                        case GPParameterType.Long:
                        case GPParameterType.String:
                        case GPParameterType.Date:
                        case GPParameterType.LinearUnit:
                            if (value == null)
                                value = string.Empty;
                            addparamUI(config.Label, value, false);
                            break;
                        case GPParameterType.FeatureLayer:
                            addToMap(param as GPFeatureRecordSetLayer, config);
                            break;
                        case GPParameterType.RecordSet:
                            if (string.IsNullOrEmpty(value) && param is GPRecordSet)
                            {
                                GPRecordSet rs = param as GPRecordSet;
                                if (string.IsNullOrEmpty(rs.Url) && rs.FeatureSet != null)
                                    value = ESRI.ArcGIS.Mapping.GP.Resources.Strings.OnlyUrlOutputIsSupportedForRecordsets;
                                else
                                    value = string.Empty;
                                addparamUI(config.Label, value, false);
                            }
                            else
                                addparamUI(config.Label, value, true);
                            break;
                        case GPParameterType.DataFile:
                        case GPParameterType.RasterData:
                        case GPParameterType.RasterDataLayer:
                            if (value == null)
                                value = string.Empty;
                            addparamUI(config.Label, value, true);
                            break;
                    }
                }
                #endregion

                if (layerParamNameIDLookup.Count > 1 && pendingGPResultImageLayers ==0)
                    LayerOrderer.OrderLayers(Map, LayerOrder, layerParamNameIDLookup);
            }
            #endregion

            #region Errors
            if (Errors != null && Errors.Count > 0)
            {
                foreach (Exception error in Errors)
                {
                    addparamUI(ESRI.ArcGIS.Mapping.GP.Resources.Strings.LabelError + " ", error.Message, false);
                    HasSimpleResults = true;
                }
            }
            #endregion
        }

        GPParameter getParameter(string name)
        {
            if (Results == null) return null;
            foreach (GPParameter item in Results)
            {
                if (item != null && item.Name == name)
                    return item;
            }
            return null;
        }

        private void addparamUI( string labelText, string value, bool url)
        {
            #region
            ParamContainer.RowDefinitions.Add(new RowDefinition());
            TextBlock label = new TextBlock() { Text = labelText, VerticalAlignment = System.Windows.VerticalAlignment.Center, Padding= new Thickness(2,0,2,0) };
            label.SetValue(Grid.RowProperty, ParamContainer.RowDefinitions.Count - 1);
            label.MaxWidth = 200;
            label.TextTrimming = TextTrimming.WordEllipsis;
            ToolTipService.SetToolTip(label, labelText); 
            ParamContainer.Children.Add(label);
            TextBlock tb = new TextBlock() { Text = value, Padding = new Thickness(2, 0, 2, 0) };
            tb.MaxWidth = 240;
            tb.TextTrimming = TextTrimming.WordEllipsis;
            ToolTipService.SetToolTip(tb, value);
            if (url && !string.IsNullOrWhiteSpace(value))
            {
                System.Windows.Controls.HyperlinkButton hl = new System.Windows.Controls.HyperlinkButton() { NavigateUri = new Uri(value), Content = tb, TargetName = "_blank", Padding = new Thickness(2, 0, 2, 0) };
                hl.SetValue(Grid.RowProperty, ParamContainer.RowDefinitions.Count - 1);
                hl.SetValue(Grid.ColumnProperty, 1);
                ParamContainer.Children.Add(hl); 
            }
            else
            {
                tb.SetValue(Grid.RowProperty, ParamContainer.RowDefinitions.Count - 1);
                tb.SetValue(Grid.ColumnProperty, 1);
                ParamContainer.Children.Add(tb); 
            }
            #endregion
            HasSimpleResults = true;
        }

        private void addToMap(ParameterConfig config, ArcGISDynamicMapServiceLayer layer)
        {
            setLayerProps(config, layer);
            Map.Layers.Add(layer);
            layerParamNameIDLookup.Add(config.Name, layer.ID);
        }

        Dictionary<string, string> layerParamNameIDLookup;
        private void addToMap(GPFeatureRecordSetLayer result, ParameterConfig config)
        {
            if (result == null)
                return;

            SpatialReference sr = Map.SpatialReference.Clone();
            ESRI.ArcGIS.Mapping.Core.GeometryType geomType = Core.GeometryType.Unknown;
            FeatureLayerParameterConfig flConfig = config as FeatureLayerParameterConfig;
            foreach (Graphic item in result.FeatureSet.Features)
            {
                if (item.Geometry != null)
                {
                    item.Geometry.SpatialReference = sr;
                    if (geomType == Core.GeometryType.Unknown)
                    {
                        if (item.Geometry is MapPoint)
                            geomType = Core.GeometryType.Point;
                        else if (item.Geometry is MultiPoint)
                            geomType = Core.GeometryType.MultiPoint;
                        else if (item.Geometry is ESRI.ArcGIS.Client.Geometry.Polygon)
                            geomType = Core.GeometryType.Polygon;
                        else if (item.Geometry is ESRI.ArcGIS.Client.Geometry.Polyline)
                            geomType = Core.GeometryType.Polyline;
                    }
                    if (flConfig != null && flConfig.DoubleFields != null && flConfig.DoubleFields.Length > 0)
                    {
                        foreach (string name in flConfig.DoubleFields)
                        {
                            if (item.Attributes.ContainsKey(name) && item.Attributes[name] != null &&
                                (!(item.Attributes[name] is double)))
                            {
                                double d;
                                if (double.TryParse(item.Attributes[name].ToString(), System.Globalization.NumberStyles.Any, CultureHelper.GetCurrentCulture(), out d))
                                    item.Attributes[name] = d;
                            }
                        }
                    }
                    if (flConfig != null && flConfig.SingleFields != null && flConfig.SingleFields.Length > 0)
                    {
                        foreach (string name in flConfig.SingleFields)
                        {
                            if (item.Attributes.ContainsKey(name) && item.Attributes[name] != null &&
                                (!(item.Attributes[name] is Single)))
                            {
                                Single s;
                                if (Single.TryParse(item.Attributes[name].ToString(), out s))
                                    item.Attributes[name] = s;
                            }
                        }
                    }
                }
            }
            GraphicsLayer layer = GraphicsLayer.FromGraphics(result.FeatureSet.Features, new SimpleRenderer());
            if (flConfig != null)
            {
                if (flConfig.GeometryType == geomType)
                    layer.Renderer = flConfig.Layer.Renderer;
                else
                    layer.Renderer = FeatureLayerParameterConfig.GetSimpleRenderer(geomType);

                setLayerProps(config, layer);
                if (flConfig.Layer != null)
                {
                    Core.LayerExtensions.SetFields(layer, Core.LayerExtensions.GetFields(flConfig.Layer));
                    Core.LayerExtensions.SetDisplayField(layer, Core.LayerExtensions.GetDisplayField(flConfig.Layer));
                    Core.LayerExtensions.SetGeometryType(layer, Core.LayerExtensions.GetGeometryType(flConfig.Layer));

                    // Set whether pop-ups are enabled and whether to show them on click or on hover
                    LayerProperties.SetIsPopupEnabled(layer, LayerProperties.GetIsPopupEnabled(flConfig.Layer));
                    Core.LayerExtensions.SetPopUpsOnClick(layer, Core.LayerExtensions.GetPopUpsOnClick(flConfig.Layer));
                }
            }

            GraphicsLayerTypeFixer.CorrectDataTypes(layer.Graphics, layer);
            
            Map.Layers.Add(layer);
            layerParamNameIDLookup.Add(config.Name, layer.ID);
        }

        private static void setLayerProps(ParameterConfig config, Layer layer)
        {
            string layerTitle = null;

            FeatureLayerParameterConfig flConfig = config as FeatureLayerParameterConfig;
            if (flConfig != null)
            {
                layerTitle = flConfig.LayerName;
                layer.Opacity = flConfig.Opacity;
            }

            MapServiceLayerParameterConfig mapServiceLayerConfig = config as MapServiceLayerParameterConfig;
            if (mapServiceLayerConfig != null)
            {
                layerTitle = mapServiceLayerConfig.LayerName;
                layer.Opacity = mapServiceLayerConfig.Opacity;
                if (mapServiceLayerConfig.LayerInfos != null)
                    ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetGPResultMapServerLayerInfos(layer, mapServiceLayerConfig.LayerInfos);
            }

            if (string.IsNullOrEmpty(layerTitle))
                layerTitle = string.IsNullOrEmpty(config.Label) ? config.Name : config.Label;

            layer.ID = Guid.NewGuid().ToString("N");
            layer.SetValue(MapApplication.LayerNameProperty, layerTitle);
        }
    }
}
