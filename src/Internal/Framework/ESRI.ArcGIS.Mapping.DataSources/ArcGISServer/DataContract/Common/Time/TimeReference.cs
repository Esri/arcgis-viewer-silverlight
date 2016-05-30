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
    public class TimeReference
    {
        // Properties
        [DataMember(Name = "timeZone")]
        public string TimeZone { get; set; }
        [DataMember(Name = "respectsDaylightSaving")]
        public bool RespectsDaylightSaving { get; set; }
    }
}
