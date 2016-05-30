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

namespace QueryTool.AddIns
{
    /// <summary>
    /// Converts the bound object to a Visibility value based on whether the object is null.  Returns Visible if the object
    /// is null, Collapsed otherwise.
    /// </summary>
    public class NullVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
			return value;
        }
    }
}
