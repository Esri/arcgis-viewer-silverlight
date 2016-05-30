/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer 
{
    [DataContract]
    public class LayerDetails
    {
        // Properties
        [DataMember(Name = "id")]
        public int ID { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "type")]
        public string Type { get; set; }
        [DataMember(Name = "geometryType")]
        public string GeometryType { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
        [DataMember(Name = "definitionExpression")]
        public string DefinitionExpression { get; set; }
        [DataMember(Name = "copyrightText")]
        public string CopyrightText { get; set; }
        [DataMember(Name = "minScale")]
        public double MinScale { get; set; }
        [DataMember(Name = "maxScale")]
        public double MaxScale { get; set; }
        [DataMember(Name = "extent")]
        public Envelope Extent { get; set; }
        [DataMember(Name = "displayField")]
        public string DisplayField { get; set; }
        [DataMember(Name = "fields")]
        public List<Field> Fields { get; set; }
        [DataMember(Name = "parentLayer")]
        public string parentLayer { get; set; }
        [DataMember(Name = "subLayers")]
        public List<SubLayer> SubLayers { get; set; }
        [DataMember(Name = "capabilities", IsRequired=false)]
        public string Capabilities { get; set; }
    }

    [DataContract]
    public class SubLayer
    {
        [DataMember(Name="id")]
        public string ID { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }

}
