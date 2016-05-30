/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
    /// Converts an <see cref="ESRI.ArcGIS.Client.FeatureService.AttachmentInfo"/> object to 
    /// a thumbnail Uri for the attachment.  
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AttachmentInfoToImageUriConverter : IValueConverter
    {
        /// <summary>
        /// Perform the conversion to a thumbnail Uri
        /// </summary>
        /// <param name="value">The source AttachmentInfo being passed to the target.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The Uri value to be passed to the target dependency property.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // default attachment icon
            Uri DisplayUri = new Uri("/ESRI.ArcGIS.Client.Application.Controls;component/Images/attachment.png",UriKind.RelativeOrAbsolute);
            AttachmentInfo ai = value as AttachmentInfo;
            if (ai != null)
            {
                string name = ai.Name.ToLower();
                // if the content type is image and can be displayed by Silverlight, use the image url
                if (ai.ContentType.Contains("image")
                    && (name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                        || name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                        || name.EndsWith(".png", StringComparison.OrdinalIgnoreCase)))
                {
                    DisplayUri = ai.Uri;
                }

            }
            return DisplayUri;
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
