/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.Core
{
    [DataContract]
    public class GeocodeServiceInfo
    {
        [DataMember(Name="serviceDescription")]
        public string ServiceDescription { get; set; }

        [DataMember(Name="addressFields")]
        public List<LocatorAddressField> AddressFields { get; set; }

        [DataMember(Name = "spatialReference")]
        public SpatialReference SpatialReference { get; set; }
    }
}
