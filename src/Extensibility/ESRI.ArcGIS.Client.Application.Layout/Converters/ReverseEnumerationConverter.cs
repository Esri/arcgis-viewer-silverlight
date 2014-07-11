/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;


namespace ESRI.ArcGIS.Client.Application.Layout.Converters
{
    /// <summary>
    /// Converts a <see cref="System.Collections.IEnumerable"/> to an IEnumerable with the same items in reverse
    /// order
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ReverseEnumerationConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Perform the conversion to an IEnumerable with the items reversed from the input IEnumerable
        /// </summary>
        /// <param name="value">The source IEnumerable being passed to the target</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic</param>
        /// <param name="culture">The culture of the conversion</param>
        /// <returns>
        /// The reversed IEnumerable to be passed to the target dependency property
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is IEnumerable && typeof(IEnumerable) == targetType)
                return (value as IEnumerable).OfType<object>().Reverse();

            return value;
        }

        /// <summary>
        /// Perform the conversion to an IEnumerable with the items reversed from the input IEnumerable
        /// </summary>
        /// <param name="value">The source IEnumerable being passed to the target</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic</param>
        /// <param name="culture">The culture of the conversion</param>
        /// <returns>
        /// The reversed IEnumerable to be passed to the target dependency property
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }

        #endregion
    }
}
