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
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ExtensionDisplayNameConverter : IValueConverter
    {
        public static string Convert(object value)
        {
            if (value == null)
                return null;

            object[] customattr = value.GetType().GetCustomAttributes(typeof(DisplayNameAttribute), true);
            if (customattr != null && customattr.Length > 0)
            {
                DisplayNameAttribute displayNameAttr = customattr[0] as DisplayNameAttribute;
                if (displayNameAttr != null && !string.IsNullOrEmpty(displayNameAttr.Name))
                    return displayNameAttr.Name;
            }
            return value.GetType().Name;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return value;

            return Convert(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
