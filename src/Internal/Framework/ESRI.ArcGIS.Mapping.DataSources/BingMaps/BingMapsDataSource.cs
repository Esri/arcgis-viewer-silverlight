/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.DataSources.BingMaps
{
    internal class BingMapsDataSource : ESRI.ArcGIS.Mapping.Core.DataSources.BaseMapDataSource
    {
        #region Helper Functions
        private TileLayer.LayerType getTileLayerTypeForBingMaps(Core.BaseMapInfo bingBaseMapInfo)
        {
            TileLayer.LayerType layerType = TileLayer.LayerType.Road;
            switch (bingBaseMapInfo.Name)
            {
                case "Roads":
                    layerType = TileLayer.LayerType.Road;
                    break;
                case "Aerial":
                    layerType = TileLayer.LayerType.Aerial;
                    break;
                case "Hybrid":
                    layerType = TileLayer.LayerType.AerialWithLabels;
                    break;
            }
            return layerType;
        }
        #endregion

        public override string ID
        {
            get
            {
                return Constants.BingMaps;
            }
        }

        public override Client.TiledMapServiceLayer CreateBaseMapLayer(Core.BaseMapInfo baseMapInfo)
        {
            return new ESRI.ArcGIS.Client.Bing.TileLayer() {                   
                  LayerStyle = getTileLayerTypeForBingMaps(baseMapInfo),
                  ServerType = ServerType.Production,
            };
        }

        public override void GetMapServiceMetaDataAsync(string mapServiceUrl, object userState)
        {
            Envelope env = new Envelope() { XMin = -20037507.0671618, XMax = 20037507.0671618, YMin = -25183752.0516242, YMax = 25183752.0516242 };
            OnGetMapServiceMetaDataCompleted(new GetMapServiceMetaDataCompletedEventArgs()
                {
                    SpatialReference = new Client.Geometry.SpatialReference(102100),
                    MapUnit = MapUnit.Meters,
                    UserState = userState,                   
                    FullExtent = env,
                    InitialExtent = env,
                    IsCached = true,
                });
        }        

        public override bool CanSwitchBaseMapLayer(Client.Layer layer)
        {
            return layer is TileLayer;
        }

        public override void SwitchBaseMapLayer(Client.Layer oldLayer, Core.BaseMapInfo newBaseMapInfo)
        {
            TileLayer tileLayer = oldLayer as TileLayer;
            if (tileLayer != null)
            {
                tileLayer.LayerStyle = getTileLayerTypeForBingMaps(newBaseMapInfo);
                tileLayer.SetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty, newBaseMapInfo.DisplayName);
            }
        }

        #region Future Code
        //public override void CancelAllCurrentRequests()
        //{
        //    // NO OP        
        //}

        //public override void CreateLayerAsync(Resource layerResource, object userState)
        //{            
        //    throw new NotSupportedException("Bing Maps datasource does not support creating layers");
        //}        

        //public override void GetChildResourcesAsync(Resource parentResource, Filter filter, object userState)
        //{
        //    throw new NotSupportedException("Bing Maps datasource does not support child resources");
        //}
        #endregion
    }
}
