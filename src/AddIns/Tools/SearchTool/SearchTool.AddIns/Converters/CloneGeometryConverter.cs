/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Globalization;
using System.Windows.Data;
using ESRI.ArcGIS.Client.Geometry;
using SearchTool.Resources;

namespace SearchTool
{
    /// <summary>
    /// Creates a clone of a bound geometry
    /// </summary>
    public class CloneGeometryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Geometry)
                return ((Geometry)value).Clone();
            else
                return null;
        }

        public object ConvertBack(Object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException(Strings.CannotConvertBack);
        }
    }
}
