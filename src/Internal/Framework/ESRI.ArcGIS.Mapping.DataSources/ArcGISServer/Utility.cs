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
using ESRI.ArcGIS.Mapping.Core.DataSources;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    public static class Utility
    {
        public static List<Resource> GetResources(Catalog catalog, Filter filter, string serverUri, string proxyUrl)
        {
            return GetResources(catalog, filter, serverUri, proxyUrl, null);
        }

        public static List<Resource> GetResources(Catalog catalog, Filter filter, string serverUri, string proxyUrl, object lockObj)
        {
            List<Resource> resources = new List<Resource>();
            if (catalog.Services != null && catalog.Services.Count > 0)
            {
                foreach (Service service in catalog.Services)
                {
                    string serviceName = service.Name;
                    // remove folder prefix 
                    int pos = serviceName.IndexOf("/", StringComparison.Ordinal);
                    if (pos > -1)
                        serviceName = serviceName.Substring(pos + 1);
                    string serviceUrl = string.Format("{0}/{1}/{2}", serverUri, serviceName, service.Type);
                    // Create display name
                    string displayName = serviceName.Replace("_", " ");

                    ResourceType type = ResourceType.Undefined;
                    if (Enum.TryParse<ResourceType>(service.Type, true, out type))
                    {
                        //Ignore Map Service Layers if Feature Access has been enabled for the service
                        //(Map Service Layers will be added by the FeatureServer node expansion)
                        if (type == ResourceType.MapServer && MapService.MapServiceFeatureAccessEnabled(service, catalog, serverUri, filter))
                            continue;

                        IService ser = ServiceFactory.CreateService(type, serverUri, proxyUrl);
                        if (ser != null && ser.IsFilteredIn(filter))
                        {
                            if (lockObj != null)
                            {
                                lock (lockObj)
                                {
                                    resources.Add(new Resource() { Url = serviceUrl, DisplayName = displayName, ResourceType = type, ProxyUrl = proxyUrl });
                                }
                            }
                            else
                                resources.Add(new Resource() { Url = serviceUrl, DisplayName = displayName, ResourceType = type, ProxyUrl = proxyUrl });
                        }
                    }
                }
            }

            return resources;
        }

        public static bool EnumTryParse<T>(string strType, bool ignoreCase, out T result)
        {
            if (Enum.IsDefined(typeof(T), strType))
            {
                result = (T)Enum.Parse(typeof(T), strType, true);
                return true;
            }
            else
            {
                result = default(T);
                return false;
            }
        }

        /// <summary>
        /// Determines if the capabilties string indicates support for Query.
        /// </summary>
        /// <param name="capabilities">String containing all supported capabilities.</param>
        /// <returns>True if Query is explicitly indicated or if the capabilities string is empty, False otherwise.</returns>
        public static bool CapabilitiesSupportsQuery(string capabilities)
        {
            bool supportsQuery = true;
            if (!String.IsNullOrEmpty(capabilities))
            {
                supportsQuery = false;
                string[] capabilityList = capabilities.Split(new char[] { ',' });
                foreach (string cap in capabilityList)
                {
                    if (cap == "Query")
                    {
                        supportsQuery = true;
                        break;
                    }
                }
            }

            return supportsQuery;
        }

        public const string RasterLayer = "Raster Layer";
        public const string ImageServerLayer = "Image Server Layer";
    }
}
