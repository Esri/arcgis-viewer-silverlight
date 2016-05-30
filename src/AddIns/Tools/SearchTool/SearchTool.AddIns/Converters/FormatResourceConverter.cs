/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows.Data;
using SearchTool.Resources;
using System.Globalization;

namespace SearchTool
{
    /// <summary>
    /// Formats a value according to the format resource specified by the ConverterParameter
    /// </summary>
    public class FormatResourceConverter : IValueConverter
    {
        private StringResourcesManager stringResources = new StringResourcesManager();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value != null) && (parameter != null))
            {
                // Get the format resource
                string formatStringKey = parameter.ToString();
                string formatString = StringResourcesManager.GetResource(formatStringKey);

                // format the value into the resource string
                return string.Format(formatString, value);
            }
            else
                return null;
        }

        public object ConvertBack(Object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException(Strings.CannotConvertBack);
        }
    }
}
