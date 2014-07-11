/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;   
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit;

namespace Viewer.Layouts.SampleData
{
    /// <summary>
    /// Takes a bound Map and adds a graphics layer containing sample data to the Map's layer collection.
    /// Intended to be used as a converter for design-time data context.
    /// </summary>
    public class SampleGraphicsLayerConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Map map = value as Map;
            if (map != null)
            {
                GraphicsLayer sampleDataLayer = new GraphicsLayer() { ID = "SampleDataLayer" };
                for (int i = 0; i < 20; i++)
                    AddSampleGraphic(sampleDataLayer);
                map.Layers.Add(sampleDataLayer);
            }

            Dispatcher.BeginInvoke((Action)delegate
            {
                FeatureDataGrid fdg = map.FindName("FeatureDataGrid") as FeatureDataGrid;

                if (fdg == null)
                {
                    FrameworkElement parent = VisualTreeHelper.GetParent(map) as FrameworkElement;
                    while (fdg == null && parent != null)
                    {
                        fdg = parent.FindName("FeatureDataGrid") as FeatureDataGrid;
                        parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
                    }
                }

                if (fdg != null && fdg.Map == null && fdg.GraphicsLayer == null)
                {
                    fdg.Map = map;
                    fdg.GraphicsLayer = map.Layers[0] as GraphicsLayer;

                    AddColumnsToFeatureDataGrid(fdg);
                }
            });

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        private void AddSampleGraphic(GraphicsLayer gLyr)
        {
            SimpleMarkerSymbol sampleSymbol = new SimpleMarkerSymbol()
            {
                Color = new SolidColorBrush(Colors.Red),
                Size = 12,
                Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle
            };

            Graphic g = new Graphic()
            {
                Geometry = new MapPoint(-110, 35),
                Symbol = sampleSymbol
            };
            g.Attributes.Add("Attribute1", "Value1");
            g.Attributes.Add("Attribute2", "Value2");
            g.Attributes.Add("Attribute3", "Value3");
            g.Attributes.Add("Attribute4", "Value4");
            g.Attributes.Add("Attribute5", "Value5");
            g.Attributes.Add("Attribute6", "Value6");
            g.Attributes.Add("Attribute7", "Value7");
            g.Attributes.Add("Attribute8", "Value8");
            g.Attributes.Add("Attribute9", "Value9");
            g.Attributes.Add("Attribute10", "Value10");

            gLyr.Graphics.Add(g);
        }

        private void AddColumnsToFeatureDataGrid(FeatureDataGrid fdg)
        {
            fdg.Columns.Add(new DataGridTextColumn() { Header = "Attribute1", 
                Binding = new Binding("Attribute1") });
            fdg.Columns.Add(new DataGridTextColumn() { Header = "Attribute2", 
                Binding = new Binding("Attribute2") });
            fdg.Columns.Add(new DataGridTextColumn() { Header = "Attribute3", 
                Binding = new Binding("Attribute3") });
            fdg.Columns.Add(new DataGridTextColumn() { Header = "Attribute4", 
                Binding = new Binding("Attribute4") });
            fdg.Columns.Add(new DataGridTextColumn() { Header = "Attribute5", 
                Binding = new Binding("Attribute5") });
            fdg.Columns.Add(new DataGridTextColumn() { Header = "Attribute6", 
                Binding = new Binding("Attribute6") });
            fdg.Columns.Add(new DataGridTextColumn() { Header = "Attribute7", 
                Binding = new Binding("Attribute7") });
            fdg.Columns.Add(new DataGridTextColumn() { Header = "Attribute8", 
                Binding = new Binding("Attribute8") });
            fdg.Columns.Add(new DataGridTextColumn() { Header = "Attribute9", 
                Binding = new Binding("Attribute9") });
            fdg.Columns.Add(new DataGridTextColumn() { Header = "Attribute10", 
                Binding = new Binding("Attribute10") });
        }
    }
}
