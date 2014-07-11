/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using System.Globalization;

namespace ESRI.ArcGIS.Mapping.Builder
{
    /// <summary>
    /// Converts a flow direction to horizontal alignment - left if LTR, right if RTL
    /// </summary>
    public class FlowDirectionToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlowDirection && ((FlowDirection)value) == FlowDirection.RightToLeft)
                return HorizontalAlignment.Right;
            else
                return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HorizontalAlignment && ((HorizontalAlignment)value) == HorizontalAlignment.Right)
                return FlowDirection.RightToLeft;
            else
                return FlowDirection.LeftToRight;
        }
    }
}
