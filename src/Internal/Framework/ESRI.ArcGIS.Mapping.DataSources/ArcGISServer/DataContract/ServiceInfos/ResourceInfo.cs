/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using System.Runtime.Serialization;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    /// <summary>
    /// Object that combines the information of ArcGIS service types.  Allows for generic deserialization of service
    /// information when the service type is unknown.
    /// </summary>
    [DataContract]
    public class ResourceInfo
    {
        [DataMember(Name = "bandCount", IsRequired = false)]
        public int BandCount { get; set; }

        [DataMember(Name = "copyrightText", IsRequired = false)]
        public string CopyrightText { get; set; }
        
        [DataMember(Name = "description", IsRequired = false)]
        public string Description { get; set; }
        
        [DataMember(Name = "extent", IsRequired = false)]
        public Envelope Extent { get; set; }
        
        [DataMember(Name = "fullExtent", IsRequired = false)]
        public Envelope FullExtent { get; set; }
        
        [DataMember(Name = "initialExtent", IsRequired = false)]
        public Envelope InitialExtent { get; set; }
        
        [DataMember(Name = "layers", IsRequired = false)]
        public MapServiceLayerInfo[] Layers { get; set; }
        
        [DataMember(Name = "mapName", IsRequired = false)]
        public string MapName { get; set; }
        
        [DataMember(Name = "pixelSizeX", IsRequired = false)]
        public double PixelSizeX { get; set; }
        
        [DataMember(Name = "pixelSizeY", IsRequired = false)]
        public double PixelSizeY { get; set; }
        
        [DataMember(Name = "serviceDataType", IsRequired = false)]
        public string ServiceDataType { get; set; }
        
        [DataMember(Name = "serviceDescription", IsRequired = false)]
        public string ServiceDescription { get; set; }
        
        [DataMember(Name = "singleFusedMapCache", IsRequired = false)]
        public bool SingleFusedMapCache { get; set; }
        
        [DataMember(Name = "spatialReference", IsRequired = false)]
        public SpatialReference SpatialReference { get; set; }
        
        [DataMember(Name = "tileInfo", IsRequired = false)]
        public TileInfoInfo TileInfo { get; set; }
        
        [DataMember(Name = "units", IsRequired = false)]
        public string Units { get; set; }
        
        [DataMember(Name = "capabilities", IsRequired = false)]
        public string Capabilities { get; set; }
        
        [DataMember(Name = "name", IsRequired = false)]
        public string Name { get; set; }
        
        [DataMember(Name = "timeInfo", IsRequired = false)]
        public TimeInfo TimeInfo { get; set; }
        
        [DataMember(Name = "pixelType", IsRequired = false)]
        public string PixelType { get; set; }
        
        [DataMember(Name = "minPixelSize", IsRequired = false)]
        public double MinPixelSize { get; set; }
        
        [DataMember(Name = "maxPixelSize", IsRequired = false)]
        public double MaxPixelSize { get; set; }
        
        [DataMember(Name = "minValues", IsRequired = false)]
        public double[] MinValues { get; set; }
        
        [DataMember(Name = "maxValues", IsRequired = false)]
        public double[] MaxValues { get; set; }
        
        [DataMember(Name = "meanValues", IsRequired = false)]
        public double[] MeanValues { get; set; }
        
        [DataMember(Name = "stdvValues", IsRequired = false)]
        public double[] StandardDeviationValues { get; set; }
        
        [DataMember(Name = "objectIdField", IsRequired = false)]
        public string ObjectIdField { get; set; }
        
        [DataMember(Name = "fields", IsRequired = false)]
        public FieldInfo[] Fields { get; set; }
        
        [DataMember(Name = "tables", IsRequired = false)]
        public TableInfo[] Tables { get; set; }

        [DataMember(Name = "supportedImageFormatTypes", IsRequired = false)]
        public string SupportedImageFormatTypes { get; set; }
    
        [DataMember(Name = "documentInfo", IsRequired = false)]
        public DocumentInfo DocumentInfo { get; set; }
}

}
