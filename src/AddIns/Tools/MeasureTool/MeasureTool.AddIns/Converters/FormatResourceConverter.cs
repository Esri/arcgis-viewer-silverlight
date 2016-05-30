/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows.Data;
using MeasureTool.Addins.Resources;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Converts a value to the format resource string specified by the ConverterParameter, with the bound value 
    /// formatted into the string via <see cref="String.Format"/>
    /// </summary>
    public class FormatResourceConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((value != null) && (parameter != null))
            {
                string formatStringKey = parameter.ToString();
                string formatString = ResourceHelper.GetStringResource(formatStringKey);
                return string.Format(formatString, value);
            }
            else
                return null;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException(Strings.ConverterCannotConvertBack);
        }
    }
}
