/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.Core
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
        public int MinScale { get; set; }
        [DataMember(Name = "maxScale")]
        public int MaxScale { get; set; }
        [DataMember(Name = "extent")]
        public Envelope Extent { get; set; }
        [DataMember(Name = "displayField")]
        public string DisplayField { get; set; }
        [DataMember(Name = "fields")]
        public List<Field> Fields { get; set; }
        [DataMember(Name = "parentLayer")]
        public string parentLayer { get; set; }
    }

    [DataContract]
    public class Field
    {
        // Properties
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "type")]
        public string Type { get; set; }
        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        public static Core.FieldInfo FieldInfoFromField(Field field)
        {
            return new ESRI.ArcGIS.Mapping.Core.FieldInfo()
            {
                DisplayName = field.Alias,
                AliasOnServer = field.Alias,
                FieldType = mapFieldType(field.Type),
                Name = field.Name,
                VisibleInAttributeDisplay = true,
                VisibleOnMapTip = true,
            };
        }

        static FieldType mapFieldType(string fieldType)
        {
            if (fieldType == "esriFieldTypeDouble"
                || fieldType == "esriFieldTypeSingle")
            {
                return FieldType.DecimalNumber;
            }
            else if (fieldType == "esriFieldTypeInteger"
                || fieldType == "esriFieldTypeSmallInteger"
                || fieldType == "esriFieldTypeOID")
            {
                return FieldType.Integer;
            }
            else if (fieldType == "esriFieldTypeDate")
            {
                return FieldType.DateTime;
            }
            return FieldType.Text; // For now all other fields are treated as strings
        }
    }
}
