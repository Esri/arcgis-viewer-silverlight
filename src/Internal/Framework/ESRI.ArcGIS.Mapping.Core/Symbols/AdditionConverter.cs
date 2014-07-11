/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows.Data;
using System.Collections.Generic;
using System.Globalization;

namespace ESRI.ArcGIS.Mapping.Core.Symbols
{
	/// <summary>
	/// Adds one number value to another value.
	/// </summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public sealed class AdditionConverter : IValueConverter
	{
		/// <summary>
		/// Modifies the source data before passing it to the target for display in the UI.
		/// </summary>
		/// <param name="value">The source data being passed to the target.</param>
		/// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
		/// <param name="parameter">An optional parameter to be used in the converter logic.</param>
		/// <param name="culture">The culture of the conversion.</param>
		/// <returns>
		/// The value to be passed to the target dependency property.
		/// </returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(double))
			{
				double d = (double)value;
				double frac = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
				return d + frac;
			}
			if (targetType == typeof(float))
			{
				float d = (float)value;
				float frac = System.Convert.ToSingle(parameter, CultureInfo.InvariantCulture);
				return d + frac;
			}
			if (targetType == typeof(int))
			{
				double d = (int)value;
				double frac = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);;
				return (int)Math.Round(d + frac);
			}
            throw new NotSupportedException(string.Format(Resources.Strings.ExceptionAdditionConverterDoesNotSupportType, targetType));
		}

		/// <summary>
		/// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
		/// </summary>
		/// <param name="value">The target data being passed to the source.</param>
		/// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
		/// <param name="parameter">An optional parameter to be used in the converter logic.</param>
		/// <param name="culture">The culture of the conversion.</param>
		/// <returns>
		/// The value to be passed to the source object.
		/// </returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
