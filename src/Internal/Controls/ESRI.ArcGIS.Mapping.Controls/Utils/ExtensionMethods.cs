using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.DataSources.ArcGISServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public static class ExtensionMethods
    {

        /// <summary>
        /// Checks whether the string represents a URL for a resource within the application environment's
        /// current ArcGIS Online or Portal instance
        /// </summary>
        public async static Task<bool> IsFederatedWithPortal(this string url, string proxyUrl = null)
        {
            string portalUrl = null;
            if (MapApplication.Current != null && MapApplication.Current.Portal != null)
                portalUrl = MapApplication.Current.Portal.Url;

            bool isFederatedWithPortal = url.ToLower().Contains(portalUrl.ToLower());

            if (!isFederatedWithPortal)
            {
                try
                {
                    // Check the requested URL's owning system URL to see if it matches that of the current portal
                    var serverInfo = await ArcGISServerDataSource.GetServerInfo(url, proxyUrl);
                    if (serverInfo != null)
                    {
                        if (!string.IsNullOrEmpty(serverInfo.OwningSystemUrl))
                        {
                            if (!string.IsNullOrEmpty(portalUrl))
                            {
                                string owningUrl = serverInfo.OwningSystemUrl;

                                // Convert to lower case, remove http/https and "/sharing" from URLs
                                portalUrl = portalUrl.ToLower().TrimEnd('/').Replace("http://", "").Replace("https://", "").Replace("/sharing", "");
                                owningUrl = owningUrl.ToLower().TrimEnd('/').Replace("http://", "").Replace("https://", "").Replace("/sharing", "");
                                isFederatedWithPortal = portalUrl == owningUrl;
                            }
                        }
                    }
                }
                catch { }
            }
            return isFederatedWithPortal;
        }

        /// <summary>
        /// Checks whether the passed-in URL refers to the application's current portal instance
        /// </summary>
        public static bool IsPortalUrl(this string url)
        {
            // Check the requested URL's owning system URL to see if it matches that of the current portal
            bool isPortalUrl = false;
            try
            {
                string portalUrl = null;
                if (MapApplication.Current != null && MapApplication.Current.Portal != null)
                    portalUrl = MapApplication.Current.Portal.Url;

                if (!string.IsNullOrEmpty(portalUrl))
                {
                    // Convert to lower case, remove http/https and "/sharing" from URLs
                    portalUrl = portalUrl.ToLower().TrimEnd('/').Replace("http://", "").Replace("https://", "").Replace("/sharing", "");
                    url = url.ToLower().TrimEnd('/').Replace("http://", "").Replace("https://", "").Replace("/sharing", "");
                    isPortalUrl = portalUrl == url;
                }
            }
            catch { }
            return isPortalUrl;
        }

    }
}
