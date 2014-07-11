/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    [DataContract]
    public class LODInfo
    {
        // Properties
        [DataMember(Name = "level")]
        public int Level { get; set; }
        [DataMember(Name = "resolution")]
        public double Resolution { get; set; }
        [DataMember(Name = "scale")]
        public double Scale { get; set; }
    }
}
