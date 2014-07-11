/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Runtime.Serialization;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer 
{
    [DataContract]
    public class FeatureServiceInfo 
    {
        // Properties
        [DataMember(Name = "serviceDescription")]
        public string ServiceDescription { get; set; }
        [DataMember(Name = "layers")]
        public FeatureServiceLayerInfo[] Layers { get; set; }
        //[DataMember(Name = "tables")]
        //public TableInfo[] Tables { get; set; }
    }
}
