/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class WatermarkColorConverter : IValueConverter
    {
        #region IValueConverter Members
        //Requires parameter to be the brush and the value to be the text
        //because silverlight does not allow for binding in the converter parameter
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string text = value as string;
            SolidColorBrush brush = parameter as SolidColorBrush;
            if (brush != null && string.IsNullOrWhiteSpace(text) )
            {
                Color color = new Color()
                {
                    A = (byte)(brush.Color.A / (byte)2),
                    R = brush.Color.R,
                    G = brush.Color.G,
                    B = brush.Color.B
                };
                SolidColorBrush newBrush = new SolidColorBrush(color);
                return newBrush;
            }
            return parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Not supported");
        }

        #endregion
    }
}
