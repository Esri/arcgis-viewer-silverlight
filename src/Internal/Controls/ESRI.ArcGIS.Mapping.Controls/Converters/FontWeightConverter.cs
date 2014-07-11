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

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class FontWeightConverter : IValueConverter
    {
        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return FontWeights.Normal;
            bool bold = (bool)value;
            return bold ? FontWeights.Bold : FontWeights.Normal;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FontWeight weight = (FontWeight)value;
            return (weight == FontWeights.Bold);
        }

        #endregion
    }

    public class ReverseFontWeightConverter : IValueConverter
    {
        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return FontWeights.Normal;
            bool bold = (bool)value;
            return bold ? FontWeights.Normal : FontWeights.Bold;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FontWeight weight = (FontWeight)value;
            return (weight == FontWeights.Normal);
        }

        #endregion
    }
}
