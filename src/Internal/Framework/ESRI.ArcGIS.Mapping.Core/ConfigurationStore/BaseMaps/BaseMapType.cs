/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.Core
{
    [DataContract]
    public enum BaseMapType
    {
        [EnumMember]
        ArcGISServer=0,
        [EnumMember]
        BingMaps=1,
        [EnumMember]
        OpenStreetMap=2
    }
}
