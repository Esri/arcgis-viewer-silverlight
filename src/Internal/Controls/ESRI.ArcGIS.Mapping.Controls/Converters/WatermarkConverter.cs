/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client.Application.Layout.Converters;
using System;
using System.Windows.Data;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public abstract class WatermarkConverterBase : IValueConverter
    {
        public virtual LocalizationConverter ResourceManager { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value as string;
            if (string.IsNullOrWhiteSpace(str) )
                return ResourceManager.Get(parameter as string);
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value as string;
        }

        #endregion
    }

    public class WatermarkConverter : WatermarkConverterBase
    {
        public WatermarkConverter()
        {
            ResourceManager = StringResourcesManager.Instance;
        }
    }
}
