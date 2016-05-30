/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class ExtensionDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return value;

            object[] customattr = value.GetType().GetCustomAttributes(typeof(DisplayNameAttribute), true);
            if (customattr != null && customattr.Length > 0)
            {
                DisplayNameAttribute displayNameAttr = customattr[0] as DisplayNameAttribute;
                if (displayNameAttr != null && !string.IsNullOrEmpty(displayNameAttr.Name))
                    return displayNameAttr.Name;
            }
            return value.GetType().Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ExtensionDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return value;

            object[] customattr = value.GetType().GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (customattr != null && customattr.Length > 0)
            {
                DescriptionAttribute displayNameAttr = customattr[0] as DescriptionAttribute;
                if (displayNameAttr != null && !string.IsNullOrEmpty(displayNameAttr.Description))
                    return displayNameAttr.Description;
            }
            return value.GetType().Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
