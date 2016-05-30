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
    public class ImageServiceInfo 
    {
        // Properties
        [DataMember(Name = "serviceDescription")]
        public string ServiceDescription { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
        [DataMember(Name = "extent")]
        public Envelope Extent { get; set; }
        [DataMember(Name = "timeInfo")]
        public TimeInfo TimeInfo { get; set; }
        [DataMember(Name = "pixelSizeX")]
        public double PixelSizeX { get; set; }
        [DataMember(Name = "pixelSizeY")]
        public double PixelSizeY { get; set; }
        [DataMember(Name = "bandCount")]
        public double BandCount { get; set; }
        [DataMember(Name = "pixelType")]
        public string PixelType { get; set; }
        [DataMember(Name = "minPixelSize")]
        public double MinPixelSize { get; set; }
        [DataMember(Name = "maxPixelSize")]
        public double MaxPixelSize { get; set; }
        [DataMember(Name = "copyrightText")]
        public string CopyrightText { get; set; }
        [DataMember(Name = "serviceDataType")]
        public string ServiceDataType { get; set; }
        [DataMember(Name = "minValues")]
        public double[] MinValues { get; set; }
        [DataMember(Name = "maxValues")]
        public double[] MaxValues { get; set; }
        [DataMember(Name = "meanValues")]
        public double[] MeanValues { get; set; }
        [DataMember(Name = "stdvValues")]
        public double[] StandardDeviationValues { get; set; }
        [DataMember(Name = "objectIdField")]
        public string ObjectIdField { get; set; }
        [DataMember(Name = "fields")]
        public FieldInfo[] Fields { get; set; }
    }
}
