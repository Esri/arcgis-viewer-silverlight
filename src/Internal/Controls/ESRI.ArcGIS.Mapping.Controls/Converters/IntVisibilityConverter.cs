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

namespace ESRI.ArcGIS.Mapping.Controls
{
    /// <summary>
    /// Converts a bound integer value to a Visibility - Visible if the bound value is equal to one
    /// or the specified ConverterParameter, Collapsed otherwise
    /// </summary>
    public class IntVisibilityConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int compareValue = parameter == null ? 1 : System.Convert.ToInt32(parameter);
            return (int)value == compareValue ? Visibility.Visible : Visibility.Collapsed; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            throw new NotImplementedException(); 
        }
    }
}
