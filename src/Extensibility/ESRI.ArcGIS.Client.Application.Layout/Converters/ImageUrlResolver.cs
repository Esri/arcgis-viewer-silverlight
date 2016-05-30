/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Client.Application.Layout.Converters
{
    /// <summary>
    /// Converts a URL to a <see cref="System.Windows.Media.Imaging.BitmapImage"/>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ImageUrlResolver : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Perform the conversion to a BitmapImage
        /// </summary>
        /// <param name="value">The source URL string being passed to the target</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic</param>
        /// <param name="culture">The culture of the conversion</param>
        /// <returns>
        /// The BitmapImage to be passed to the target dependency property
        /// </returns>        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string urlToBeResolved = value as string;
            if (string.IsNullOrWhiteSpace(urlToBeResolved))
            {
                urlToBeResolved = parameter as string;
            }
            BitmapImage image = resolveUrlForImage(urlToBeResolved);
            if (image != null)
                return image;
            else
                return value;
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Resolves a URL to a BitmapImage that is valid for the current <see cref="ESRI.ArcGIS.Client.Extensibility.MapApplication"/>
        /// </summary>
        /// <param name="urlToBeResolved">The URL to be resolved</param>
        /// <returns>A BitmapImage</returns>
        static BitmapImage resolveUrlForImage(string urlToBeResolved)
        {
            Uri imageUri;
            if (MapApplication.Current != null)
            {
                imageUri = MapApplication.Current.ResolveUrl(urlToBeResolved);
                if (imageUri != null)
                    return new BitmapImage(imageUri);
            }
            return null;
        }
    }
}
