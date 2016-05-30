/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Windows;
using System;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Provides access to URL-accessible endpoints used by the application
    /// </summary>
    public class ApplicationUrls : DependencyObject
    {
        /// <summary>
        /// Gets or sets the base URL of the application.  Allows retrieval of resources that use the same 
        /// relative path although the base URL used to reference the applicaiton may be different, for instance
        /// when an application is being edited versus when it is being used by end users.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Obsolete.  Use the <see cref="ProxyUrl"/> property.
        /// </summary>
        [Obsolete("Use the ProxyUrl property")]
        public Dictionary<string, string> ProxyUrls { get; set; }

        #region Dependency Properties
        /// <summary>
        /// Identifies the <see cref="GeometryServiceUrl"/> DependencyProperty
        /// </summary>
        public static DependencyProperty GeometryServiceUrlProperty = DependencyProperty.Register("GeometryServiceUrlProperty", typeof(string), typeof(ApplicationUrls), null);

        /// <summary>
        /// Gets or sets the URL to the ArcGIS Server Geometry Service used by the application
        /// </summary>
        public string GeometryServiceUrl
        {
            get { return GetValue(GeometryServiceUrlProperty) as string; }
            set { SetValue(GeometryServiceUrlProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ProxyUrl"/> DependencyProperty
        /// </summary>
        public static DependencyProperty ProxyUrlProperty = 
            DependencyProperty.Register("ProxyUrl", typeof(string), typeof(ApplicationUrls), null);

        /// <summary>
        /// Gets or sets the URL to use as a proxy for web requests.  When defined, application
        /// components using a proxy will send web requets to the proxy URL, with the original
        /// request URL appended as a query string.
        /// </summary>
        public string ProxyUrl
        {
            get { return GetValue(ProxyUrlProperty) as string; }
            set { SetValue(ProxyUrlProperty, value); }
        }
        #endregion

        /// <summary>
        /// Obsolete.  Use the <see cref="ProxyUrl"/> property.
        /// </summary>
        [Obsolete("Use the ProxyUrl property")]
        public string GetProxyUrl(string key = "Default")
        {
            return ProxyUrl;
        }
    }
}
