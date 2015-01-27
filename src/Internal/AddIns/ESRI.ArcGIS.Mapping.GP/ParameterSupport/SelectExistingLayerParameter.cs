/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows.Controls;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows;
using System.Linq;
using ESRI.ArcGIS.Client.Extensibility;
using System.Windows.Media;

namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    public class SelectExistingLayerParameter : FeatureLayerParameterBase
    {
          private ESRI.ArcGIS.Mapping.Core.GeometryType GetGeometryTypeFromGraphic(Graphic graphic)
        {
            if (graphic != null)
            {
                if (graphic.Geometry is ESRI.ArcGIS.Client.Geometry.MapPoint)
                {
                    return ESRI.ArcGIS.Mapping.Core.GeometryType.Point;
                }
                else if (graphic.Geometry is ESRI.ArcGIS.Client.Geometry.Polyline)
                {
                    return ESRI.ArcGIS.Mapping.Core.GeometryType.Polyline;
                }
                else if (graphic.Geometry is ESRI.ArcGIS.Client.Geometry.Polygon || graphic.Geometry is ESRI.ArcGIS.Client.Geometry.Envelope)
                {
                    return ESRI.ArcGIS.Mapping.Core.GeometryType.Polygon;
                }
            }
            return ESRI.ArcGIS.Mapping.Core.GeometryType.Unknown;
        }

          bool matchesGeomType(GraphicsLayer gLayer)
          {
              ParameterSupport.FeatureLayerParameterConfig flConfig = Config as ParameterSupport.FeatureLayerParameterConfig;

              ESRI.ArcGIS.Mapping.Core.GeometryType geomType = Core.LayerExtensions.GetGeometryType(gLayer);

              if (flConfig.GeometryType == geomType)
                  return true;
               return false;
          }

          string getGeometryType()
          {
              ParameterSupport.FeatureLayerParameterConfig flConfig = Config as ParameterSupport.FeatureLayerParameterConfig;
              switch (flConfig.GeometryType)
              {
                  case ESRI.ArcGIS.Mapping.Core.GeometryType.Point:
                      return "point ";
                  case ESRI.ArcGIS.Mapping.Core.GeometryType.MultiPoint:
                      return "multipoint ";
                  case ESRI.ArcGIS.Mapping.Core.GeometryType.Polyline:
                       return "line ";
                  case ESRI.ArcGIS.Mapping.Core.GeometryType.Polygon:
                      return "polygon ";
              }
              return string.Empty;
          }
          StackPanel layerPanel;
          ComboBox cb;
        public override void AddUI(Grid grid)
        {
            if (Config != null && Config.ShownAtRunTime && Map != null)
            {
                #region
                ParameterSupport.FeatureLayerParameterConfig flConfig = Config as ParameterSupport.FeatureLayerParameterConfig;
                #region Get layer list
                List<GraphicsLayer> inputLayers = GetLayerList();
                #endregion
                Map.Layers.CollectionChanged -= Layers_CollectionChanged;
                Map.Layers.CollectionChanged += Layers_CollectionChanged;
                layerPanel = new StackPanel();
                layerPanel.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                layerPanel.SetValue(Grid.ColumnProperty, 1);
                grid.Children.Add(layerPanel);
                populateLayerPanel(inputLayers);
                #endregion
            }
        }

        private void populateLayerPanel(List<GraphicsLayer> inputLayers)
        {
            layerPanel.Children.Clear();
            if (inputLayers.Count > 0)
            {
                cb = new ComboBox()
                    {
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        Width = 125,
                        Height = 24,
                        Foreground = new SolidColorBrush(Colors.Black)
                    };
                populateLayerList(inputLayers);
                cb.SetValue(Grid.ColumnProperty, 1);
                cb.SelectionChanged += (a, b) =>
                {
                    GraphicsLayer selLayer = cb.SelectedItem as GraphicsLayer;
                    if (selLayer != null)
                    {
                        InputLayerID = selLayer.ID;
                        FeatureSet features = new FeatureSet();
                        FeatureSet selectedFeatures = new FeatureSet();

                        foreach (Graphic g in selLayer.Graphics)
                        {
                            if (g.Geometry != null)
                            {
                                if (features.SpatialReference == null)
                                    features.SpatialReference = g.Geometry.SpatialReference;
                                Graphic newG = new Graphic();
                                newG.Geometry = g.Geometry;
                                features.Features.Add(newG);
                                if (g.Selected)
                                    selectedFeatures.Features.Add(newG);
                            }
                        }
                        if (selectedFeatures.Features.Count > 0)
                            Value = new GPFeatureRecordSetLayer(Config.Name, selectedFeatures);
                        else
                            Value = new GPFeatureRecordSetLayer(Config.Name, features);
                    }
                    else
                        Value = null;
                    RaiseCanExecuteChanged();

                };
                layerPanel.Children.Add(cb);
                RaiseCanExecuteChanged();
            }
            else
            {
                TextBlock tb = new TextBlock() { Text = string.Format(Resources.Strings.NoLayersAvailable, getGeometryType()) };
                ToolTipService.SetToolTip(tb, string.Format(Resources.Strings.AddLayersToMap, getGeometryType()));
                layerPanel.Children.Add(tb);
                RaiseCanExecuteChanged();
            }
        }

        private List<GraphicsLayer> GetLayerList()
        {
            ParameterSupport.FeatureLayerParameterConfig flConfig = Config as FeatureLayerParameterConfig;
            List<GraphicsLayer> inputLayers = new List<GraphicsLayer>();
            if (flConfig != null)
            {
                for (int i = Map.Layers.Count - 1; i >= 0; i--)
                {
                    Layer item = Map.Layers[i];
                    if (!(item is GraphicsLayer))
                        continue;
                    ESRI.ArcGIS.Mapping.Core.GeometryType geomType = Core.LayerExtensions.GetGeometryType(item as GraphicsLayer);
                    switch (geomType)
                    {
                        case ESRI.ArcGIS.Mapping.Core.GeometryType.Point:
                        case ESRI.ArcGIS.Mapping.Core.GeometryType.MultiPoint:
                        case ESRI.ArcGIS.Mapping.Core.GeometryType.Polygon:
                        case ESRI.ArcGIS.Mapping.Core.GeometryType.Polyline:
                            if (matchesGeomType(item as GraphicsLayer))
                                inputLayers.Add(item as GraphicsLayer);
                            break;
                        case ESRI.ArcGIS.Mapping.Core.GeometryType.Unknown:
                            GraphicsLayer gLayer = item as GraphicsLayer;
                            if (gLayer.Graphics.Count > 0)
                            {
                                ESRI.ArcGIS.Mapping.Core.GeometryType gType = GetGeometryTypeFromGraphic(gLayer.Graphics.ElementAtOrDefault(0));
                                Core.LayerExtensions.SetGeometryType(gLayer, gType);
                                if (matchesGeomType(item as GraphicsLayer))
                                    inputLayers.Add(item as GraphicsLayer);
                            }
                            else
                                gLayer.Graphics.CollectionChanged += Graphics_CollectionChanged;
                            break;
                    }
                }
            }
            return inputLayers;
        }

        void Graphics_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            GraphicCollection collection = sender as GraphicCollection;
            if (collection != null)
                collection.CollectionChanged -= Graphics_CollectionChanged;
            refreshLayers();
        }

        private void populateLayerList(List<GraphicsLayer> inputLayers)
        {
            GraphicsLayer selLayer = cb.SelectedItem as GraphicsLayer;
            int selectedIndex = -1;
            if (selLayer != null)
                selectedIndex = inputLayers.IndexOf(selLayer);

            cb.ItemsSource = inputLayers;
            string dataTemplate =
@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><TextBlock Text=""{Binding LayerName}""/></DataTemplate>";
            cb.ItemTemplate = System.Windows.Markup.XamlReader.Load(dataTemplate) as DataTemplate;

            if (selectedIndex > -1)
                cb.SelectedIndex = selectedIndex;
        }


        void Layers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            refreshLayers();
        }

        void refreshLayers()
        {
            if (cb != null)
                populateLayerList(GetLayerList());
            else
                populateLayerPanel(GetLayerList());
        }
    }
}
