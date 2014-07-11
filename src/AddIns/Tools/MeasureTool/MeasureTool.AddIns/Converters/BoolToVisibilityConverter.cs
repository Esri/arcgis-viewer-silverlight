/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Converts a bound boolean value to an equivalent Visiblity value - visible if the boolean is true, collapsed if false
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            return (bool)value ? Visibility.Visible : Visibility.Collapsed; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            Visibility visibility = (Visibility)value; 
            return (visibility == Visibility.Visible); 
        }
    }
}
