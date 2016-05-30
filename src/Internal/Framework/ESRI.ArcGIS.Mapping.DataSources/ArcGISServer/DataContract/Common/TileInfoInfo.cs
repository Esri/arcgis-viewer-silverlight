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
    public class TileInfoInfo 
    {
        // Properties
        [DataMember(Name = "cols")]
        public int Cols { get; set; }
        [DataMember(Name = "compressionQuality")]
        public string CompressionQuality { get; set; }
        [DataMember(Name = "dpi")]
        public int DPI { get; set; }
        [DataMember(Name = "format")]
        public string Format { get; set; }
        [DataMember(Name = "lods")]
        public LODInfo[] LODs { get; set; }
        [DataMember(Name = "origin")]
        public MapPoint Origin { get; set; }
        [DataMember(Name = "rows")]
        public int Rows { get; set; }
        [DataMember(Name = "spatialReference")]
        public SpatialReference SpatialReference { get; set; }
    }
}
