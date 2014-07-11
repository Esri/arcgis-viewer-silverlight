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
    public class GeoprocessingServiceInfo
    {
        // Properties
        [DataMember(Name = "serviceDescription")]
        public string ServiceDescription { get; set; }
        [DataMember(Name = "tasks")]
        public string[] Tasks { get; set; }
        [DataMember(Name = "resultMapServerName")]
        public string ResultMapServerName { get; set; }
        [DataMember(Name = "currentVersion")]
        public string CurrentVersion { get; set; }
    }
}
