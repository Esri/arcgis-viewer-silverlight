/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows.Controls;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Media.Imaging;
using System;
using ESRI.ArcGIS.Mapping.Core;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;
namespace ESRI.ArcGIS.Mapping.GP.ParameterSupport
{
    public class SketchLayerParameter : FeatureLayerParameterBase
    {
        public override GPParameter Value
        {
            get
            {
                GraphicsLayer layer = getLayer();
                if (layer == null || layer.Graphics.Count < 1)
                    return null;
                ESRI.ArcGIS.Client.Tasks.GPFeatureRecordSetLayer gpfrsLayer =
                    new ESRI.ArcGIS.Client.Tasks.GPFeatureRecordSetLayer(Config.Name, new FeatureSet(layer.Graphics));
                gpfrsLayer.FeatureSet.SpatialReference = Map.SpatialReference;
                return gpfrsLayer;
            }
            set
            {
                base.Value = value;
            }
        }

        Draw draw;
        Draw Draw
        {
            get
            {
                if (draw == null)
                {
                    draw = new Draw(Map);
                    draw.DrawComplete += (s2, e2) =>
                    {
                        draw.IsEnabled = false;
                        addInput(e2.Geometry);
                        RaiseCanExecuteChanged();
                    };
                    if (config.GeometryType == ESRI.ArcGIS.Mapping.Core.GeometryType.Polygon)
                        draw.FillSymbol = config.GetDrawSymbol() as FillSymbol;
                    if (config.GeometryType == ESRI.ArcGIS.Mapping.Core.GeometryType.Polyline)
                        draw.LineSymbol = config.GetDrawSymbol() as LineSymbol;
                }
                return draw;
            }
        }

        GraphicsLayer getLayer()
        {
            if (!string.IsNullOrEmpty(InputLayerID))
                return Map.Layers[InputLayerID] as GraphicsLayer;
            return null;
        }
        void addInput(ESRI.ArcGIS.Client.Geometry.Geometry geometry)
        {
            GraphicsLayer layer = getLayer();

            #region Create layer if not already there and add to map
            if (layer == null)
            {
                InputLayerID = Guid.NewGuid().ToString("N");
                layer = new GraphicsLayer();
                if (config.Layer != null)
                {
                    layer.Renderer = config.Layer.Renderer;
                    Core.LayerExtensions.SetFields(layer, Core.LayerExtensions.GetFields(config.Layer));
                    Core.LayerExtensions.SetGeometryType(layer, Core.LayerExtensions.GetGeometryType(config.Layer));
                    Core.LayerExtensions.SetDisplayField(layer, Core.LayerExtensions.GetDisplayField(config.Layer));
                    Core.LayerExtensions.SetPopUpsOnClick(layer, Core.LayerExtensions.GetPopUpsOnClick(config.Layer));
                    LayerProperties.SetIsPopupEnabled(layer, LayerProperties.GetIsPopupEnabled(config.Layer));
                }
                layer.ID = InputLayerID;

                layer.SetValue(MapApplication.LayerNameProperty,
                    string.IsNullOrEmpty(config.LayerName) ? 
                        string.IsNullOrEmpty(config.Label) ? config.Name : config.Label
                        : config.LayerName);
                layer.Opacity = config.Opacity;
                Map.Layers.Add(layer);
            }
            #endregion

            #region Add geometry to layer
            if (config.GeometryType == Core.GeometryType.MultiPoint) // Special handling for MultiPoint geometry
            {
                if (layer.Graphics.Count == 0) 
                {
                    // Create a new MultiPoint geometry and add the passed-in point to it
                    var multipoint = new MultiPoint() { SpatialReference = geometry.SpatialReference };
                    multipoint.Points.Add((MapPoint)geometry);
                    var g = new Graphic() { Geometry = multipoint };
                    layer.Graphics.Add(g);
                }
                else
                {
                    // Get the sketch graphic's MultiPoint geometry and add the passed-in point to it
                    var multipoint = (MultiPoint)layer.Graphics[0].Geometry;
                    multipoint.Points.Add((MapPoint)geometry);
                    layer.Refresh();
                }
            }
            else
            {
                Graphic g = new Graphic() { Geometry = geometry };
                layer.Graphics.Add(g);
            }
            #endregion
        }

        public override void AddUI(Grid grid)
        {
            if (Config != null && Config.ShownAtRunTime)
            {
                #region
                ParameterSupport.FeatureLayerParameterConfig flConfig = Config as ParameterSupport.FeatureLayerParameterConfig;
                if (flConfig != null)
                {
                    #region Button for drawing
                    Button btn = new Button()
                    {
                        Width = 22,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        Style = System.Windows.Application.Current.Resources["SimpleButtonStyle"] as System.Windows.Style
                    };
                    btn.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                    btn.SetValue(Grid.ColumnProperty, 1);
                    grid.Children.Add(btn);
                    string name = Config.Name;
                    btn.Click += (s, e) =>
                    {
                        if (Map == null) return;
                        if (flConfig.GeometryType == ESRI.ArcGIS.Mapping.Core.GeometryType.Polyline)
                            Draw.DrawMode = DrawMode.Polyline;
                        else if (flConfig.GeometryType == ESRI.ArcGIS.Mapping.Core.GeometryType.Point
                            || flConfig.GeometryType == ESRI.ArcGIS.Mapping.Core.GeometryType.MultiPoint)
                            Draw.DrawMode = DrawMode.Point;
                        else if (flConfig.GeometryType == ESRI.ArcGIS.Mapping.Core.GeometryType.Polygon)
                            Draw.DrawMode = DrawMode.Polygon;
                        Draw.Map = Map;
                        Draw.IsEnabled = true;
                    };
                    switch (flConfig.GeometryType)
                    {
                        case ESRI.ArcGIS.Mapping.Core.GeometryType.Polygon:
                            btn.Content = new Image()
                            {
                                Source = new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.GP;component/Images/DrawPolygon16.png", UriKind.Relative)),
                                Stretch = System.Windows.Media.Stretch.None,
                                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                            };
                            break;
                        case ESRI.ArcGIS.Mapping.Core.GeometryType.Polyline:
                            btn.Content = new Image()
                            {
                                Source = new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.GP;component/Images/DrawPolyline16.png", UriKind.Relative)),
                                Stretch = System.Windows.Media.Stretch.None,
                                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                            };
                            break;
                        case ESRI.ArcGIS.Mapping.Core.GeometryType.Point:
                        case ESRI.ArcGIS.Mapping.Core.GeometryType.MultiPoint:
                            btn.Content = new Image()
                            {
                                Source = new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.GP;component/Images/DrawPoint16.png", UriKind.Relative)),
                                Stretch = System.Windows.Media.Stretch.None,
                                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                            };
                            break;
                        default:
                            btn.IsEnabled = false;
                            btn.Content = Resources.Strings.NotAvailable;
                            break;
                    }
                    btn.SetValue(ToolTipService.ToolTipProperty, flConfig.ToolTip);
                    #endregion
                }
                #endregion
            }
        }
    }
}

