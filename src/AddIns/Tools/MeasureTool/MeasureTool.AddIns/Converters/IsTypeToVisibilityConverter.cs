/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using MeasureTool.Addins.Resources;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Checks the bound value against the type specified by the ConverterParameter and returns a Visibility
    /// value based on whether the types match.  Type string must be in the format 
    /// &lt;Assembly&gt;~&lt;Namespace&gt;~&lt;Type&gt;
    /// </summary>
    public class IsTypeToVisibilityConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && parameter != null &&
            value.GetType() == Utils.GetTypeFromTypeInfoString(parameter.ToString()))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            throw new InvalidOperationException(Strings.ConverterCannotConvertBack);
        }
    }
}
