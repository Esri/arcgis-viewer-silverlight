/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    [DataContract]
    public class TimeInfo
    {
        // Properties
        [DataMember(Name = "timeExtent")]
        public double[] TimeExtent { get; set; }
        [DataMember(Name = "timeReference")]
        public TimeReference TimeReference { get; set; }
    }
}
