/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Globalization;
using System.Windows.Data;

namespace SearchTool
{
    /// <summary>
    /// Divides the bound value by the specified converter parameter
    /// </summary>
    public class DivideValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double dividend = -1;
            double divisor = -1;
            if (double.TryParse(value.ToString(), out dividend) &&
              double.TryParse(parameter.ToString(), out divisor))
                return dividend / divisor;
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double dividend = -1;
            double divisor = -1;
            if (double.TryParse(value.ToString(), out dividend) &&
              double.TryParse(parameter.ToString(), out divisor))
                return dividend * divisor;
            else
                return value;
        }

        #endregion
    }
}
