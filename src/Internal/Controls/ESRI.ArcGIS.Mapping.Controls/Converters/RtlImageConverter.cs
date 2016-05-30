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
using ESRI.ArcGIS.Client.Application.Layout;

namespace ESRI.ArcGIS.Mapping.Controls.Converters
{
    /// <summary>    
    /// Converter that returns the flip angle for images based on View FlowDirection
    /// </summary>
    public class RtlImageConverter : DependencyObject, IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(double))
                return value;

            RTLHelper helper = Application.Current.Resources["RTLHelper"] as RTLHelper;
            if (helper != null)
            {
                bool isRtl = helper.FlowDirection == FlowDirection.RightToLeft;
                return (isRtl) ? 180 : 0;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
