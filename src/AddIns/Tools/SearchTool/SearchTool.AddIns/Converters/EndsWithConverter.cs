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
    /// Checks whether a string ends with the specified parameter.  Returns true if so, false otherwise.
    /// </summary>
    public class EndsWithConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string input = value as string;
            string end = parameter as string;
            if (input != null && end != null)
                return input.ToLower().EndsWith(end.ToLower(), StringComparison.Ordinal);
            else if (value == null)
                return false;
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
