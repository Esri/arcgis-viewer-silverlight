/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Globalization;
using System.Windows.Data;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls.Converters
{
    public class FeatureLayerModeConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Convert QueryMode to bool.  
        /// </summary>
        /// <returns>True if QueryMode = OnDemand</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mode = (FeatureLayer.QueryMode) value;
            var param = parameter as string;
            if (param == "OnDemand")
            {
                return mode == FeatureLayer.QueryMode.OnDemand;
            }
            else if (param == "SnapShot")
            {
                return mode == FeatureLayer.QueryMode.Snapshot;
            }
            return false;
        }

        /// <summary>
        /// Converts bool to QueryMode.
        /// </summary>
        /// <returns>True=OnDemand, False=SnapShot</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FeatureLayer.QueryMode mode = FeatureLayer.QueryMode.Snapshot;
            var checkedVal = (bool) value;
            var param = parameter as string;
            if (param == "OnDemand")
            {
                mode = checkedVal ? FeatureLayer.QueryMode.OnDemand : FeatureLayer.QueryMode.Snapshot;
            }
            return mode;
        }

        #endregion
    }
}
