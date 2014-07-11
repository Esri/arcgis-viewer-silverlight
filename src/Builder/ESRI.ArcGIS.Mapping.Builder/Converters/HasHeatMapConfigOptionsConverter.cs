/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Data;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Builder
{
    /// <summary>
    /// Converts a layer to a visibility value based on whether the layer supports generation of a heat map
    /// or heat map configuration.  Returns Visible if so, Collapsed if not.
    /// </summary>
    public class HasHeatMapConfigOptionsConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Layer))
                return null;

            Layer layer = (Layer)value;
            if (HeatMapFeatureLayerHelper.SupportsLayer(layer)
            || layer is HeatMapFeatureLayer
            || layer is HeatMapLayer)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
