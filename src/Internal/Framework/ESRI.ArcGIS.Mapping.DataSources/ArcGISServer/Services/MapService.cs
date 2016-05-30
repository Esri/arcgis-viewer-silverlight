/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer 
{
    internal class MapService : ServiceBase<MapServiceInfo>
    {
        private List<MapServiceLayerInfo> _layersInService = null;
        private List<int> _notSupportedLayerIds = null;
        private List<Resource> _childResources = null;
        public override List<Resource> ChildResources
        {
            get
            {
                if (_childResources == null)
                {
                    _childResources = new List<Resource>();

                    if (ServiceInfo != null && ServiceInfo.Layers != null)
                    {
                        foreach (MapServiceLayerInfo layerInfo in ServiceInfo.Layers)
                        {
                            if (layerInfo.ParentLayerId != -1) // only had layers at the root of the map service node (no parent)
                                continue;
                            if (_notSupportedLayerIds != null && _notSupportedLayerIds.Contains(layerInfo.ID))
                                continue;
                            if (layerInfo.SubLayerIds == null)
                            {
                                _childResources.Add(new Resource()
                                {
                                    DisplayName = layerInfo.Name,
                                    ResourceType = ResourceType.Layer,
                                    Url = string.Format("{0}/{1}", Uri, layerInfo.ID),
                                    ProxyUrl = ProxyUrl,
                                    Tag = layerInfo.ID,
                                });
                            }
                            else
                            {
                                _childResources.Add(new Resource()
                                {
                                    DisplayName = layerInfo.Name,
                                    ResourceType = ResourceType.GroupLayer,
                                    Url = string.Format("{0}/{1}", Uri, layerInfo.ID),
                                    ProxyUrl = ProxyUrl,
                                    Tag = layerInfo.ID,
                                });
                            }
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

        public MapService(string uri, string proxyUrl)
            : base(uri, proxyUrl, ResourceType.MapServer)
        {
            Uri = uri;
        }

        public override bool IsServiceInfoNeededToApplyThisFilter(Filter filter)
        {
            // As you can see below, the only time we need service information is when the filter is for cached
            // resources. As long as that is not part of the filter, we can determine whether this service should
            // be filtered or not based upon the filter itself without service info.
            if ((filter & Filter.CachedResources) == Filter.CachedResources)
                return true;
            else
                return false;
        }

        public override bool IsFilteredIn(Filter filter)
        {
            if ((filter & Filter.CachedResources) == Filter.CachedResources)
            {
                if (ServiceInfo != null && (ServiceInfo.TileInfo == null || !ServiceInfo.SingleFusedMapCache))
                    return false;
            }

            return (filter & Filter.None) == Filter.None ||
                (filter & Filter.SpatiallyEnabledResources) == Filter.SpatiallyEnabledResources;
           
        }

        private ArcGISWebClient webClient;
        protected async override void OnServiceDetailsDownloadCompleted(ServiceDetailsDownloadCompletedEventArgs args)
        {
            // Workaround for the fact that DataContractJsonSerializer doesn't call the default contructor
            // http://msdn.microsoft.com/en-us/library/system.runtime.serialization.datacontractserializer.aspx 
            if (ServiceInfo != null && ServiceInfo.SpatialReference != null)
            {
                if (ServiceInfo.SpatialReference.WKID == default(int))
                    ServiceInfo.SpatialReference.WKID = -1;
            }

            Uri finalUrl = new Uri(string.Format("{0}/layers?f=json", Uri));

            if (webClient == null)
                webClient = new ArcGISWebClient();
            webClient.ProxyUrl = ProxyUrl;

            try
            {
                ArcGISWebClient.DownloadStringCompletedEventArgs result =
                    await webClient.DownloadStringTaskAsync(finalUrl, args);
                processResult(result);
            }
            catch
            {
                base.OnServiceDetailsDownloadCompleted(args);
            }
        }

        private void processResult(ArcGISWebClient.DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;
            ServiceDetailsDownloadCompletedEventArgs eventArgs = e.UserState as ServiceDetailsDownloadCompletedEventArgs;
            if (e.Error != null)
            {
                base.OnServiceDetailsDownloadCompleted(eventArgs);
                return;
            }

            string json = e.Result;
            Exception ex = Utils.CheckJsonForException(json);
            if (ex != null) // if the service is pre-10.x then it doesn't support /layers operation
            {
                base.OnServiceDetailsDownloadCompleted(eventArgs);
                return;
            }

            try
            {
                AllLayersResponse response = JsonSerializer.Deserialize<AllLayersResponse>(e.Result);
                _layersInService = response.Layers;
                if (_layersInService != null)
                {
                    _notSupportedLayerIds = new List<int>();
                    foreach (MapServiceLayerInfo mapServiceLayerInfo in _layersInService)
                    {
                        if (Utility.RasterLayer.Equals(mapServiceLayerInfo.Type) || Utility.ImageServerLayer.Equals(mapServiceLayerInfo.Type))
                            _notSupportedLayerIds.Add(mapServiceLayerInfo.ID);
                    }
                }
            }
            catch (Exception)
            {
                // Ok to swallow
            }
            base.OnServiceDetailsDownloadCompleted(eventArgs);
        }

        public override void Cancel()
        {
            base.Cancel();

            if (webClient != null && webClient.IsBusy)
                webClient.CancelAsync();
        }

        internal static bool MapServiceFeatureAccessEnabled(Service mapService, Catalog catalog, string serverUri, Filter filter)
        {
            if (mapService != null && (filter & Filter.FeatureServices) == Filter.FeatureServices)
            {
                foreach (Service service in catalog.Services)
                {
                    ResourceType type = ResourceType.Undefined;
                    if (Enum.TryParse<ResourceType>(service.Type, true, out type))
                    if (Utility.EnumTryParse<ResourceType>(service.Type, true, out type))
                    {
                        if (type == ResourceType.FeatureServer && string.Compare(service.Name, mapService.Name) == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }        
    }

    [DataContract]
    public class AllLayersResponse
    {
        [DataMember(Name = "layers")]
        public List<MapServiceLayerInfo> Layers { get; set; }
    }
}
