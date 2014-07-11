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
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class LayerDisplayNameConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object obj = Convert(value as Layer);
            if(obj == null)
                return value;
            return obj;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static string Convert(Layer layer)
        {
            if (layer != null)
            {
                string displayName = layer.GetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty) as string;
                if (!string.IsNullOrEmpty(displayName))
                {
                    return displayName;
                }
                return layer.ID;
            }
            return null;
        }
    }
}
