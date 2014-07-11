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
using SearchTool.Resources;

namespace SearchTool
{
    /// <summary>
    /// Reverses a Visibility value
    /// </summary>
	public class ReverseVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Visibility)
				return ((Visibility)value) == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            return value;
		}

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
            throw new InvalidOperationException(Strings.CannotConvertBack);
        }

		#endregion
	}
}
