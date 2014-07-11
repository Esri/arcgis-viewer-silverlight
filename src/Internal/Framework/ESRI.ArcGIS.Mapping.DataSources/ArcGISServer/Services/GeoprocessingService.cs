/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer 
{
    internal class GeoprocessingService : ServiceBase<GeoprocessingServiceInfo>
    {
        public GeoprocessingService(string uri, string proxyUrl)
            : base(uri, proxyUrl, ResourceType.GPServer)
        {
        }

        private List<Resource> _childResources = null;
        public override List<Resource> ChildResources
        {
            get
            {

                if (_childResources == null)
                {
                    _childResources = new List<Resource>();

                    if (ServiceInfo != null && ServiceInfo.Tasks != null)
                    {
                        foreach (string task in ServiceInfo.Tasks)
                        {
                            Resource resource = new Resource()
                                {
                                    DisplayName = task,
                                    ProxyUrl = ProxyUrl,
                                    ResourceType = ResourceType.GPTool
                                };
                            //Create uri to correctly encode special characters (e.g. space) in the task name
                            //Using HttpUtility.UrlEncode converts a space to "+" whereas "%20" is expected in a url path
                            //UrlPathEncode is not available in Silverlight
                            Uri uri = new System.Uri(string.Format("{0}/{1}", Uri.TrimEnd('/'), task));
                            resource.Url = uri.AbsoluteUri;
                            _childResources.Add(resource);
                        }
                    }
                }
                return _childResources;
            }
            set
            {
                _childResources = value;
            }
        }

        public override bool IsServiceInfoNeededToApplyThisFilter(Filter filter)
        {
            // As you can see below, filtering logic never requires service info so return false so that it is not obtained.
            return false;
        }

        public override bool IsFilteredIn(Filter filter)
        {
            return (filter & Filter.None) == Filter.None || (filter & Filter.GeoprocessingServices) == Filter.GeoprocessingServices;
        }
    }
}
