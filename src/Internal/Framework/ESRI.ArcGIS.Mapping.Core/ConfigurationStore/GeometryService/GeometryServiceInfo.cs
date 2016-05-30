/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.Core
{
    [DataContract]
    public class GeometryServiceInfo
    {
        [DataMember(Name="Url", Order=0, IsRequired=false)]
        public string Url { get; set; }
    }
}
