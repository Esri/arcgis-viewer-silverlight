/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Runtime.Serialization;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer 
{
    [DataContract]
    public class MapServiceInfo 
    {
        // Properties
        [DataMember(Name = "bandCount")]
        public int BandCount { get; set; }
        [DataMember(Name = "copyrightText")]
        public string CopyrightText { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
        [DataMember(Name = "extent")]
        public Envelope Extent { get; set; }
        [DataMember(Name = "fullExtent")]
        public Envelope FullExtent { get; set; }
        [DataMember(Name = "initialExtent")]
        public Envelope InitialExtent { get; set; }
        [DataMember(Name = "layers")]
        public MapServiceLayerInfo[] Layers { get; set; }
        [DataMember(Name = "mapName")]
        public string MapName { get; set; }
        [DataMember(Name = "pixelSizeX")]
        public double PixelSizeX { get; set; }
        [DataMember(Name = "pixelSizeY")]
        public double PixelSizeY { get; set; }
        [DataMember(Name = "serviceDataType")]
        public string ServiceDataType { get; set; }
        [DataMember(Name = "serviceDescription")]
        public string ServiceDescription { get; set; }
        [DataMember(Name = "singleFusedMapCache")]
        public bool SingleFusedMapCache { get; set; }
        [DataMember(Name = "spatialReference")]
        public SpatialReference SpatialReference { get; set; }
        [DataMember(Name = "tileInfo")]
        public TileInfoInfo TileInfo { get; set; }
        [DataMember(Name = "units")]
        public string Units { get; set; }
        [DataMember(Name = "capabilities", IsRequired = false)]
        public string Capabilities { get; set; }
    }
}
