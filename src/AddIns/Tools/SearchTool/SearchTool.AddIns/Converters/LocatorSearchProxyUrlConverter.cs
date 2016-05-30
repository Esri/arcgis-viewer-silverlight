/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Globalization;
using System.Windows.Data;
using ESRI.ArcGIS.Client.Geometry;
using SearchTool.Resources;

namespace SearchTool
{
    /// <summary>
    /// Converts a bound ArcGISLocatorSearchProvider to the provider's locator URL, accounting for 
    /// proxy if one is being used
    /// </summary>
    public class LocatorSearchProxyUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Get the provider
            ArcGISLocatorPlaceSearchProvider provider = value as ArcGISLocatorPlaceSearchProvider;
            if (provider != null)
            {
                // If the provider uses proxy, incorporate that into the URL
                if (provider.UseProxy && !string.IsNullOrEmpty(provider.ProxyUrl))
                    return string.Format("{0}?{1}", provider.ProxyUrl, provider.LocatorServiceUrl);
                else
                    return provider.LocatorServiceUrl;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(Object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException(Strings.CannotConvertBack);
        }
    }
}
