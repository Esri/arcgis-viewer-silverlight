/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows.Media;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Shapes;

namespace ESRI.ArcGIS.Mapping.Core.Symbols
{
    public class GeometryConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = value as string;
            if (!string.IsNullOrEmpty(val))
            {
                string nsPath = string.Format("<Path xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Data=\"{0}\"/>",
                val);
            Path yPath = System.Windows.Markup.XamlReader.Load(nsPath) as Path;
               
                if (yPath != null)
                {
                    Geometry geometry = yPath.Data;
                    yPath.Data = null;//remove geometry from path before returning it
                    return geometry;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
           throw new NotImplementedException();
        }

        #endregion
    }
}
