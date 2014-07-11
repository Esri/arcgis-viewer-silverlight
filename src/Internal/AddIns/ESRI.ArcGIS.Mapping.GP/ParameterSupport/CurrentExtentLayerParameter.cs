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
    public class CurrentExtentLayerParameter : FeatureLayerParameterBase
    {
        ESRI.ArcGIS.Client.Tasks.GPFeatureRecordSetLayer gpfrsLayer;
        public override GPParameter Value
        {
            get
            {
                if (Map != null)
                {
                    if (gpfrsLayer == null)
                        getlayer();
                    return gpfrsLayer;
                }
                return null;
            }
            set
            {
                gpfrsLayer = value as ESRI.ArcGIS.Client.Tasks.GPFeatureRecordSetLayer;
            }
        }

        private void getlayer()
        {
            gpfrsLayer =
                new ESRI.ArcGIS.Client.Tasks.GPFeatureRecordSetLayer(Config.Name, new FeatureSet(envelopeToPolygon(Map.Extent)));
            gpfrsLayer.FeatureSet.SpatialReference = Map.SpatialReference;
        }

        Polygon envelopeToPolygon(Envelope env)
        {
            PointCollection points = new PointCollection();
            points.Add(new MapPoint(env.XMin, env.YMin, env.SpatialReference));
            points.Add(new MapPoint(env.XMin, env.YMax, env.SpatialReference));
            points.Add(new MapPoint(env.XMax, env.YMax, env.SpatialReference));
            points.Add(new MapPoint(env.XMax, env.YMin, env.SpatialReference));
            points.Add(new MapPoint(env.XMin, env.YMin, env.SpatialReference));
            Polygon polygon = new Polygon();
            polygon.Rings.Add(points);
            return polygon;
        }

        public override void AddUI(Grid grid)
        {
            if (Config != null && Config.ShownAtRunTime)
            {
                TextBlock tb = new TextBlock()
                {
                    Text = Resources.Strings.ZoomToExtentInput,
                    Margin = new System.Windows.Thickness(2)
                };
                tb.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                tb.SetValue(Grid.ColumnProperty, 1);
                tb.SetValue(ToolTipService.ToolTipProperty, Config.ToolTip);
                grid.Children.Add(tb);
            }
        }

        public override bool CanExecute
        {
            get
            {
                return Map != null;
            }
        }
    }
}

