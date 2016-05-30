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
    /// Converts a string to Visibility - empty or null string becomes Visibility.Collapsed.
    /// </summary>
    public class EmptyTextVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
                return string.IsNullOrEmpty(((string)value).Trim()) ? Visibility.Collapsed : Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
