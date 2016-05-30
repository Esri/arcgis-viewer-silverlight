/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace PrintTool.AddIns
{
    /// <summary>
    /// Converts a bound boolean value to its reverse Visiblity value - visible if the boolean is false, collapsed if true
    /// </summary>
    public class ReverseBoolToVisibilityConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            return (bool)value ? Visibility.Collapsed : Visibility.Visible; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            Visibility visibility = (Visibility)value; 
            return (visibility == Visibility.Collapsed); 
        }
    }
}
