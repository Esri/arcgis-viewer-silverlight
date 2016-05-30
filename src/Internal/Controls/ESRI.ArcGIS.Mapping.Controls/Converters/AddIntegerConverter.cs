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
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Controls
{
    /// <summary>
    /// Adds the integer passed as the converter parameter to the bound value
    /// </summary>
    public class AddIntegerConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int val;
            int valToAdd;
            if (!int.TryParse(value.ToString(), out val) || !int.TryParse(parameter.ToString(), out valToAdd))
                return null;

            return val + valToAdd;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int val;
            int valToSubtract;
            if (!int.TryParse(value.ToString(), out val) || !int.TryParse(parameter.ToString(), out valToSubtract))
                return null;

            return val - valToSubtract;
        }

        #endregion

    }
}
