/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class IsPointLayer : IValueConverter
    {
        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SimpleMarkerSymbol)
                return Visibility.Visible;

            ConfigurableFeatureLayer featureLayer = value as ConfigurableFeatureLayer;
            if (featureLayer != null)
            {
                return featureLayer.GeometryType == GeometryType.Point ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value is ConfigurableGraphicsLayer) // SharePoint list layer
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("IsPointLayer does not support two-way binding");
        }

        #endregion
    }

    public class IsPolygonLayer : IValueConverter
    {
        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SimpleFillSymbol)
                return Visibility.Visible;

            ConfigurableFeatureLayer featureLayer = value as ConfigurableFeatureLayer;
            if (featureLayer != null)
            {
                return featureLayer.GeometryType == GeometryType.Polygon ? Visibility.Visible : Visibility.Collapsed;
            }

            ClassBreakInfo classBreak = value as ClassBreakInfo;
            if (classBreak != null)
            {
                return classBreak.Symbol is SimpleFillSymbol ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("IsPolygonLayer does not support two-way binding");
        }

        #endregion
    }

    public class IsLineLayer : IValueConverter
    {
        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SimpleLineSymbol)
                return Visibility.Visible;

            ConfigurableFeatureLayer featureLayer = value as ConfigurableFeatureLayer;
            if (featureLayer != null)
            {
                return featureLayer.GeometryType == GeometryType.PolyLine ? Visibility.Visible : Visibility.Collapsed;
            }

            ClassBreakInfo classBreak = value as ClassBreakInfo;
            if (classBreak != null)
            {
                return classBreak.Symbol is SimpleLineSymbol ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("IsLineLayer does not support two-way binding");
        }

        #endregion
    }

    public class EnabledBasedOnRenderAsHeatMapConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(value is ConfigurableHeatMapLayer);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("EnabledBasedOnRenderAsHeatMapConverter does not support two-way binding");
        }

        #endregion
    }

    public class IsClassBreaksRenderer : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GraphicsLayer graphicsLayer = value as GraphicsLayer;
            if (graphicsLayer != null)
            {
                return graphicsLayer.Renderer is ClassBreaksRenderer;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class IsNotClassBreaksRenderer : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GraphicsLayer graphicsLayer = value as GraphicsLayer;
            if (graphicsLayer != null)
            {
                return !(graphicsLayer.Renderer is ClassBreaksRenderer);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class GetDefaultSymbolForLayer : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return SymbologyUtils.GetDefaultSymbolForLayer(value as Layer);
        }       

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
