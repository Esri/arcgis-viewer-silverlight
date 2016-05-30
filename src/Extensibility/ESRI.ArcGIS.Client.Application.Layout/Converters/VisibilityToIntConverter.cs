/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.ComponentModel;

namespace ESRI.ArcGIS.Client.Application.Layout.Converters
{
    /// <summary>
    /// Converts a Visibility value to an Integer.  By default, returns 1 if Visible, 0 if Collapsed.  Alternate
    /// values can be specified via ConverterParameter.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class VisibilityToIntConverter : IValueConverter
    {
        /// <summary>
        /// Perform the conversion to an Integer
        /// </summary>
        /// <param name="value">The source Visibility value being passed to the target</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property</param>
        /// <param name="parameter">An optional parameter used to specify alternate integer values to convert to.  Specify the integer for Visible, then Collapsed, with a comma separating them</param>
        /// <param name="culture">The culture of the conversion</param>
        /// <returns>
        /// The Integer to be passed to the target dependency property
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int visibleInt = 1;
            int collapsedInt = 0;

            string conversionValues = parameter as string;
            if (conversionValues != null)
            {
                string[] valsArray = conversionValues.Split(',');
                if (valsArray.Length == 2)
                {
                    int.TryParse(valsArray[0], out visibleInt);
                    int.TryParse(valsArray[1], out collapsedInt);
                }
            }

            Visibility visibility = (Visibility)value;
            return visibility == Visibility.Visible ? visibleInt : collapsedInt;
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
