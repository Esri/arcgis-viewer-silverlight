/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    internal class BaseMapServiceMetaDataLoader
    {
        private string baseMapServiceUrl;
        public BaseMapServiceMetaDataLoader(string baseMapServiceUrl)
        {
            this.baseMapServiceUrl = baseMapServiceUrl;
        }

        public string ProxyUrl { get; set; }

        public void GetBaseMapServiceMetaData(object userToken)
        {
            if (string.IsNullOrEmpty(baseMapServiceUrl) || !Uri.IsWellFormedUriString(baseMapServiceUrl, UriKind.Absolute))
                throw new InvalidOperationException(Resources.Strings.ExceptionMustSpecifyAbsoluteUriForBaseMapServiceUrl);
            MapService mapService = new MapService(baseMapServiceUrl, null) { ProxyUrl = this.ProxyUrl };
            mapService.ServiceDetailsDownloadFailed += (o, e) => {
                if (GetBaseMapServiceMetaDataFailed != null)
                    GetBaseMapServiceMetaDataFailed(this, e);
            };
            mapService.ServiceDetailsDownloadCompleted += (o, e) => {
                MapService service = o as MapService;
                if (service != null)
                {
                    MapServiceInfo mapServiceInfo = service.ServiceInfo;
                    if (mapServiceInfo != null)
                    {
                        // Set spatial reference
                        SpatialReference spatialRef = mapServiceInfo.SpatialReference;
                        MapUnit mapUnit = MapUnit.Meters;
                        // Set map unit
                        string mapUnits = mapServiceInfo.Units;
                        if (!string.IsNullOrEmpty(mapUnits))
                        {
                            mapUnits = mapUnits.Replace("esri", string.Empty); // remove esri prefix from map units
                            if (Enum.IsDefined(typeof(MapUnit), mapUnits))
                            {
                                mapUnit = (MapUnit)Enum.Parse(typeof(MapUnit), mapUnits, true);
                            }
                        }

                        if (GetBaseMapServiceMetaDataCompleted != null)
                            GetBaseMapServiceMetaDataCompleted(this, new GetMapServiceMetaDataCompletedEventArgs()
                            {
                                MapUnit = mapUnit,
                                SpatialReference = spatialRef,
                                UserState = e.UserState,
                                FullExtent = mapServiceInfo.FullExtent,
                                InitialExtent = mapServiceInfo.InitialExtent,
                                IsCached = mapServiceInfo.TileInfo != null && mapServiceInfo.SingleFusedMapCache
                            });
                    }
                    else
                    {
                        if (GetBaseMapServiceMetaDataFailed != null)
                            GetBaseMapServiceMetaDataFailed(this, new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnableToRetriveMapServiceInformation), e.UserState));
                    }
                }
                else
                {
                    if (GetBaseMapServiceMetaDataFailed != null)
						GetBaseMapServiceMetaDataFailed(this, new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnableToRetriveMapServiceInformation), e.UserState));
                }
            };
            mapService.GetServiceDetails(userToken);
        }

        public event EventHandler<GetMapServiceMetaDataCompletedEventArgs> GetBaseMapServiceMetaDataCompleted;
        public event EventHandler<ExceptionEventArgs> GetBaseMapServiceMetaDataFailed;
    }    
}
