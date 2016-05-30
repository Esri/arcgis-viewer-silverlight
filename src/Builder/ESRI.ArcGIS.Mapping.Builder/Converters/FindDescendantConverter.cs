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

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class FindDescendantConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(parameter as string) || !(value is FrameworkElement))
                return null;

            return ((FrameworkElement)value).FindDescendantByName((string)parameter);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
