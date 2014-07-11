/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Toolkit.DataSources;

namespace ESRI.ArcGIS.Mapping.DataSources.OpenStreetMap
{
    public class OpenStreetMapDataSource : ESRI.ArcGIS.Mapping.Core.DataSources.BaseMapDataSource
    {
        public override string ID
        {
            get { return Constants.OpenStreetMap; }
        }

        public override Client.TiledMapServiceLayer CreateBaseMapLayer(Core.BaseMapInfo baseMapInfo)
        {
            return new OpenStreetMapLayer() { 
                 Style = getMapStyle(baseMapInfo),                 
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
            return layer is OpenStreetMapLayer;                
        }

        public override void SwitchBaseMapLayer(Client.Layer oldLayer, Core.BaseMapInfo newBaseMapInfo)
        {
            OpenStreetMapLayer osmLayer = oldLayer as OpenStreetMapLayer;
            if (osmLayer != null)
            {
                osmLayer.Style = getMapStyle(newBaseMapInfo);
                osmLayer.SetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty, newBaseMapInfo.DisplayName);
            }
        }

        #region Helper Functions
        private ESRI.ArcGIS.Client.Toolkit.DataSources.OpenStreetMapLayer.MapStyle getMapStyle(Core.BaseMapInfo bingBaseMapInfo)
        {
            ESRI.ArcGIS.Client.Toolkit.DataSources.OpenStreetMapLayer.MapStyle mapStyle = ESRI.ArcGIS.Client.Toolkit.DataSources.OpenStreetMapLayer.MapStyle.Mapnik;
            switch (bingBaseMapInfo.Name)
            {
                case "Mapnik":
                    mapStyle = ESRI.ArcGIS.Client.Toolkit.DataSources.OpenStreetMapLayer.MapStyle.Mapnik;
                    break;
                case "CycleMap":
                    mapStyle = ESRI.ArcGIS.Client.Toolkit.DataSources.OpenStreetMapLayer.MapStyle.CycleMap;
                    break;
                case "NoName":
                    mapStyle = ESRI.ArcGIS.Client.Toolkit.DataSources.OpenStreetMapLayer.MapStyle.NoName;
                    break;
            }
            return mapStyle;
        }
        #endregion
    }
}
