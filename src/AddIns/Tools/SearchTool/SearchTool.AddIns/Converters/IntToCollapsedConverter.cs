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
using SearchTool.Resources;

namespace SearchTool
{
    /// <summary>
    /// Converts an integer to a Visibility value - by default 0 converts to Collapsed, other values convert to Visible. 
    /// If an integer is passed as the converter parameter, that value is used for converting to Collapsed instead of zero.
    /// </summary>
	public class IntToCollapsedConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            // Get the bound number
			int count = 0;
            if (value != null)
                int.TryParse(value.ToString(), out count);

            // Get the number passed as the converter parameter
            int collapsedValue = 0;
            if (parameter != null)
                int.TryParse(parameter.ToString(), out collapsedValue);

            // Return visible if the bound number does not equal the converter parameter
			if (count != collapsedValue)
				return Visibility.Visible;

            // Otherwise, return collapsed
			return Visibility.Collapsed;
		}

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
            throw new InvalidOperationException(Strings.CannotConvertBack);
        }

		#endregion
	}
}
