/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows.Data;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Toolkit.Primitives;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{    
    public class SubLayerSupportsToggleVisibilityToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ArcGISTiledMapServiceLayer)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class MapContentsLayerDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object name = null;
            LayerItemViewModel layerModel = value as LayerItemViewModel;
            if (layerModel != null)
            {
                if (layerModel.Layer != null && MapContentsControlHelper.IsTopMostLayerType(layerModel.LayerType))
                {
                    string layerName = layerModel.Layer.GetValue(MapApplication.LayerNameProperty) as string;
                    if (string.IsNullOrWhiteSpace(layerName))
                    {
                        layerName = layerModel.Label;
                        if (string.IsNullOrWhiteSpace(layerName))
                            layerName = layerModel.Layer.ID;
                    }
                    name = layerName;
                }
                else
                    name = layerModel.Label;
            }
            else
            {
                LegendItemViewModel legendModel = value as LegendItemViewModel;
                if (legendModel != null)
                    name = legendModel.Label;
            }

            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class CanToggleLayerVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Mode legendMode = (Mode)value;

            if (legendMode != Mode.Legend)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ShowLegendSwatchesByModeToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Mode mode = (Mode)value;
            if (mode == Mode.LayerList) //|| mode == Mode.TopMostLayerList 
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class LegendItemLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string label = value as string;
            if (!string.IsNullOrEmpty(label) && label.StartsWith("EsriHeatMapLayer__", StringComparison.Ordinal))
            {
                return string.Empty;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
