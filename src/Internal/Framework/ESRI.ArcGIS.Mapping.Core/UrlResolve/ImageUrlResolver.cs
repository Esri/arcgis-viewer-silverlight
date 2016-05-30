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
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Core
{
    public static class ImageUrlResolver 
    {
        static IUrlResolver resolver = null;

        public static Uri ResolveUrl(string urlToBeResolved)
        {
            if (!string.IsNullOrEmpty(urlToBeResolved))
            {
                // Embedded image URL
                if (urlToBeResolved.StartsWith("/", StringComparison.Ordinal) && urlToBeResolved.Contains(";component/"))
                    return new Uri(urlToBeResolved, UriKind.Relative);

                if (urlToBeResolved.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || urlToBeResolved.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    return new Uri(urlToBeResolved, UriKind.Absolute);

                string resolvedUrl = urlToBeResolved;
                if (resolver != null)
                    resolvedUrl = resolver.ResolveUrl(urlToBeResolved);
                Uri imageUri;
                if (Uri.TryCreate(resolvedUrl, UriKind.RelativeOrAbsolute, out imageUri))
                    return imageUri;
            }
            return null;
        }

        public static BitmapImage ResolveUrlForImage(string urlToBeResolved)
        {
            Uri imageUri = ResolveUrl(urlToBeResolved);
            if (imageUri != null)
                return new BitmapImage(imageUri);
            return null;
        }

        public static void RegisterImageUrlResolver(IUrlResolver urlResolver)
        {
            resolver = urlResolver;
        }
    }
}
