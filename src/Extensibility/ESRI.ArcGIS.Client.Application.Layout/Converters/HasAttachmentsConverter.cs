/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ESRI.ArcGIS.Client.FeatureService;
using System.ComponentModel;

namespace ESRI.ArcGIS.Client.Application.Layout.Converters
{
    /// <summary>
    /// Converts a <see cref="ESRI.ArcGIS.Client.Layer"/> to a <see cref="ESRI.ArcGIS.Client.FeatureLayer"/> if
    /// it is a FeatureLayer with attachments
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class HasAttachmentsConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Perform the conversion to a FeatureLayer
        /// </summary>
        /// <param name="value">The source Layer being passed to the target</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic</param>
        /// <param name="culture">The culture of the conversion</param>
        /// <returns>
        /// The FeatureLayer to be passed to the target dependency property
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FeatureLayer attachmentLayer = null;
            FeatureLayer layer = value as FeatureLayer;
            if (layer != null && layer.LayerInfo.HasAttachments)
                attachmentLayer = layer;

            return attachmentLayer;           
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
