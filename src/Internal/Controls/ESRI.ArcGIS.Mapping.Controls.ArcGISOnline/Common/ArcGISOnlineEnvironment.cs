/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Browser;
using System.Xml.Linq;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    /// <summary>
    /// Provides information about, and means to manipulate, the current runtime environment.
    /// </summary>
    public class ArcGISOnlineEnvironment
    {
        static ArcGISOnline _agol = null;

        /// <summary>
        /// Gets the ArcGIS Online server.
        /// </summary>
        public static ArcGISOnline ArcGISOnline
        {
            get
            {
                if (_agol == null)
                    _agol = new ArcGISOnline();
                return _agol;
            }
        }

        /// <summary>
        /// Get/sets the token used for accessing Bing services.
        /// </summary>
        public static string BingToken
        {
            get { return ""; }
        }

        /// <summary>
        /// Gets the domain name of the hosting server.
        /// </summary>
        public static string HostDomain
        {
            get
            {
                string host = Application.Current.Host.Source.Host;
                if (host.Contains("."))
                {
                    string[] tokens = host.Split(new char[] { '.' });
                    if (tokens.Length > 2)
                        return tokens[tokens.Length - 2] + "." + tokens[tokens.Length - 1];
                }

                return null;
            }
        }

        /// <summary>
        /// Returns the AGOL item type for web map searches.
        /// </summary>
        public static string WebMapTypeQualifier
        {
            get { return "type:\"Web Map\" -type:\"Web Mapping Application\""; }
        }

        /// <summary>
        /// Returns the AGOL item type for service searches.
        /// </summary>
        public static string ServiceTypeQualifier
        {
            get { return "(type:\"Map Service\" OR type:\"Image Service\" OR type:\"Feature Service\")"; }
        }

        static ConfigurationUrls configurationUrls;
        public static ConfigurationUrls ConfigurationUrls {
            get
            {
                if (configurationUrls == null)
                    configurationUrls = new ConfigurationUrls();
                return configurationUrls;
            }
        }

        public static string ArcGISOnlineConfigurationFileUrl { get; set; }

        public static bool LoadedConfig { get; set; }
        public static void LoadConfig(EventHandler callback)
        {
            // Source is null in design environments
            if (ESRI.ArcGIS.Client.Extensibility.MapApplication.Current != null)
            {
                if (String.IsNullOrEmpty(ArcGISOnlineConfigurationFileUrl))
                {
                    Uri baseUri = new Uri(MapApplication.Current.Urls.BaseUrl);
                    LoadConfig(new Uri(baseUri, "Config/Admin/ArcGISOnline.xml"), callback);
                }
                else
                    LoadConfig(new Uri(ArcGISOnlineConfigurationFileUrl), callback);
            }
        }
        public static void LoadConfig(Uri uri, EventHandler callback)
        {
            WebUtil.OpenReadAsync(uri, null, (sender2, e2) =>
            {
                if (e2.Error != null)
                    return;
                XDocument xml = XDocument.Load(e2.Result);
                XElement el = xml.Element("Configuration");
                LoadConfig(el, callback);
            });
        }

        public static void LoadConfig(XElement configuration, EventHandler callback)
        {
            ConfigurationUrls.Sharing = getElementValue(configuration.Element("Sharing"));
            ConfigurationUrls.Secure = getElementValue(configuration.Element("Secure"));
            ConfigurationUrls.ProxyServer = getElementValue(configuration.Element("ProxyServer"));
            ConfigurationUrls.ProxyServerEncoded = getElementValue(configuration.Element("ProxyServerEncoded"));
            if (string.IsNullOrEmpty(ConfigurationUrls.ProxyServerEncoded) && !string.IsNullOrEmpty(ConfigurationUrls.ProxyServer))
                ConfigurationUrls.ProxyServerEncoded = ConfigurationUrls.ProxyServer;
            else if (string.IsNullOrEmpty(ConfigurationUrls.ProxyServer) && 
                !string.IsNullOrEmpty(ConfigurationUrls.ProxyServerEncoded))
                ConfigurationUrls.ProxyServer = ConfigurationUrls.ProxyServerEncoded;
            ArcGISOnline.Initialize(ArcGISOnlineEnvironment.ConfigurationUrls.Sharing, ArcGISOnlineEnvironment.ConfigurationUrls.Secure);
            ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnlineEnvironment.ArcGISOnline.User.SignInFromLocalStorage(null);
            LoadedConfig = true;
            if (callback != null)
                callback(null, EventArgs.Empty);
        }

        public static void LoadConfig(string sharing, string secure, string proxy, bool useCachedSignIn = true,
            bool forceBrowserAuth = false, bool signOutCurrentUser = true)
        {
            ConfigurationUrls.Sharing = sharing;
            ConfigurationUrls.Secure = secure;
            ConfigurationUrls.ProxyServer = proxy;
            ConfigurationUrls.ProxyServerEncoded = proxy;
            ArcGISOnline.Initialize(ArcGISOnlineEnvironment.ConfigurationUrls.Sharing, 
                ArcGISOnlineEnvironment.ConfigurationUrls.Secure, forceBrowserAuth, signOutCurrentUser);
            if (useCachedSignIn)
                ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnlineEnvironment.ArcGISOnline.User.SignInFromLocalStorage(null);
            else
                ArcGISOnlineEnvironment.ArcGISOnline.User.DeleteTokenFromStorage();
            LoadedConfig = true;
        }

        private static string getElementValue(XElement element)
        {
            if (element != null)
                return element.Value;
            else
                return null;
        }
    }

    /// <summary>
    /// Holds the configurable urls for the application.
    /// </summary>
    public class ConfigurationUrls
    {
        public string Sharing { get; set; }
        public string Secure { get; set; }
        public string GeometryServer { get; set; }
        public string ProxyServer { get; set; }
        public string ProxyServerEncoded { get; set; }
    }
}
