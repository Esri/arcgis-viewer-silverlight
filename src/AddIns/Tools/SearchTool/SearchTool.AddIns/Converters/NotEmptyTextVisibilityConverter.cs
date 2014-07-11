/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SearchTool
{
    /// <summary>
    /// Converts a string to Visibility - empty or null string becomes Visibility.Visible.
    /// </summary>
    public class NotEmptyTextVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
                return string.IsNullOrEmpty(((string)value).Trim()) ? Visibility.Visible : Visibility.Collapsed;
            else if (value == null)
                return Visibility.Visible;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
