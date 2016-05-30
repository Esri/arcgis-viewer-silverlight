/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
using ESRI.ArcGIS.Mapping.Core;
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ThemeColorSetToCollectionConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ThemeColorSet colorSet = value as ThemeColorSet;
            if (colorSet == null)
                return null;

            Collection<Color> colors = new Collection<Color>();
            colors.Add(colorSet.TextBackgroundDark1);
            colors.Add(colorSet.TextBackgroundLight1);
            colors.Add(colorSet.TextBackgroundDark2);
            colors.Add(colorSet.TextBackgroundLight2);
            colors.Add(colorSet.Accent1);
            colors.Add(colorSet.Accent2);
            colors.Add(colorSet.Accent3);
            colors.Add(colorSet.Accent4);
            colors.Add(colorSet.Accent5);
            colors.Add(colorSet.Accent6);
            colors.Add(colorSet.Hyperlink);
            colors.Add(colorSet.FollowedHyperlink);

            return colors;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
