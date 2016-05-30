/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.FeatureService;

namespace ESRI.ArcGIS.Mapping.Core
{
    internal static class ExtensionMethods
    {
        #region JSON Serialization

        /// <summary>
        /// Converts a Dictionary to JSON
        /// </summary>
        /// <param name="dictionary">The dictionary to convert</param>
        /// <param name="humanReadable">Whether the output string should include formatting, such as line breaks and spaces</param>
        /// <returns>A JSON representation of the input dictionary</returns>
        /// <remarks>
        /// This method will walk all dictionaries and lists contained in the input dictionary, but all
        /// property values are ultimately written out using ToString.  No reflection is carried out.
        /// </remarks>
        internal static string ToJson(this IDictionary dictionary, bool humanReadable = false)
        {
            using (StringWriter sw = new StringWriter(new StringBuilder()))
            {
                dictionary.ToJson(sw, humanReadable);
                return sw.ToString();
            }
        }

        /// <summary>
        /// Converts a Dictionary to JSON
        /// </summary>
        /// <param name="dictionary">The dictionary to convert</param>
        /// <param name="sw">The <see cref="StringWriter"/> to write JSON to</param>
        /// <param name="humanReadable">Whether the output string should include formatting, such as line breaks and spaces</param>
        /// <remarks>
        /// This method will walk all dictionaries and lists contained in the input dictionary, but all
        /// property values are ultimately written out using ToString.  No reflection is carried out.
        /// </remarks>
        internal static void ToJson(this IDictionary dictionary, StringWriter sw, 
            bool humanReadable = false)
        {
            sw.Write("{");

            if (dictionary.Count > 0)
            {
                if (humanReadable) sw.Write("\n");

                int count = 0;
                foreach (dynamic item in dictionary)
                {
                    sw.Write("\"");
                    sw.Write(item.Key.ToString());
                    sw.Write("\"");
                    if (humanReadable) sw.Write(" ");
                    sw.Write(" : ");
                    if (humanReadable) sw.Write(" ");

                    if (item.Value == null)
                        sw.Write("null");
                    else if (item.Value is IDictionary)
                        ((IDictionary)item.Value).ToJson(sw, humanReadable);
                    else if (item.Value is IList)
                        ((IList)item.Value).ToJson(sw, humanReadable);
                    else
                    {
                        if (item.Value is string && !((string)item.Value).IsJsonObject())
                        {
                            sw.Write("\"");
                            sw.Write(item.Value);
                            sw.Write("\"");
                        }
                        else if (item.Value is bool)
                        {
                            sw.Write(item.Value.ToString().ToLower());
                        }
                        else
                        {
                            sw.Write(item.Value.ToString());
                        }
                    }

                    count++;
                    if (count < dictionary.Count)
                    {
                        sw.Write(",");
                        if (humanReadable) sw.Write("\n");
                    }
                }

                if (humanReadable) sw.Write("\n");
            }
            sw.Write("}");
        }

        /// <summary>
        /// Converts a List to JSON
        /// </summary>
        /// <param name="dictionary">The list to convert</param>
        /// <param name="humanReadable">Whether the output string should include formatting, such as line breaks and spaces</param>
        /// <returns>A JSON representation of the input list</returns>
        /// <remarks>
        /// This method will walk all dictionaries and lists contained in the input list, but all
        /// property values are ultimately written out using ToString.  No reflection is carried out.
        /// </remarks>
        internal static string ToJson(this IList list, bool humanReadable = false)
        {
            using (StringWriter sw = new StringWriter(new StringBuilder()))
            {
                list.ToJson(sw, humanReadable);
                return sw.ToString();
            }
        }

        /// <summary>
        /// Converts a List to JSON
        /// </summary>
        /// <param name="dictionary">The list to convert</param>
        /// <param name="sw">The <see cref="StringWriter"/> to write JSON to</param>
        /// <param name="humanReadable">Whether the output string should include formatting, such as line breaks and spaces</param>
        /// <remarks>
        /// This method will walk all dictionaries and lists contained in the input list, but all
        /// property values are ultimately written out using ToString.  No reflection is carried out.
        /// </remarks>
        internal static void ToJson(this IList list, StringWriter sw, bool humanReadable = false)
        {
            sw.Write("[");

            if (list.Count > 0)
            {
                if (humanReadable) sw.Write("\n");

                int count = 0;
                foreach (dynamic item in list)
                {
                    if (item is IDictionary)
                        ((IDictionary)item).ToJson(sw, humanReadable);
                    else if (item is IList)
                        ((IList)item).ToJson(sw, humanReadable);
                    else
                    {
                        if (item is string && !((string)item).IsJsonObject())
                        {
                            sw.Write("\"");
                            sw.Write(item);
                            sw.Write("\"");
                        }
                        else if (item is bool)
                        {
                            sw.Write(item.ToString().ToLower());
                        }
                        else
                        {
                            sw.Write(item.ToString());
                        }
                    }


                    count++;
                    if (count < list.Count)
                    {
                        sw.Write(",");
                        if (humanReadable) sw.Write("\n");
                    }
                }

                if (humanReadable) sw.Write("\n");
            }
            sw.Write("]");
        }
        
        /// <summary>
        /// Checks whether or not a string represents an object in JSON
        /// </summary>
        internal static bool IsJsonObject(this string json)
        {
            if ((json.StartsWith("{") && json.EndsWith("}"))
            || (json.StartsWith("[") && json.EndsWith("]")))
                return true;
            else
                return false;
        }

        #endregion

        #region FeatureLayer to dictionary

        // FeatureLayerInfo
        internal static Dictionary<string, object> ToDictionary(this FeatureLayerInfo layerInfo)
        {
            try
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();

                #region Common Info
                dictionary.Add("id", layerInfo.Id);
                if (!string.IsNullOrEmpty(layerInfo.Name))
                    dictionary.Add("name", layerInfo.Name);
                if (!string.IsNullOrEmpty(layerInfo.Type))
                    dictionary.Add("type", layerInfo.Type);
                if (!string.IsNullOrEmpty(layerInfo.Description))
                    dictionary.Add("description", layerInfo.Description);
                if (!string.IsNullOrEmpty(layerInfo.DefinitionExpression))
                    dictionary.Add("definitionExpression", layerInfo.DefinitionExpression);
                #endregion

                // new to 10.1 Feature Service
                dictionary.Add("isDataVersioned", layerInfo.IsDataVersioned);

                #region Field Info
                if (!string.IsNullOrEmpty(layerInfo.ObjectIdField))
                    dictionary.Add("objectIdField", layerInfo.ObjectIdField);
                if (!string.IsNullOrEmpty(layerInfo.GlobalIdField))
                    dictionary.Add("globalIdField", layerInfo.GlobalIdField);
                if (!string.IsNullOrEmpty(layerInfo.TypeIdField))
                    dictionary.Add("typeIdField", layerInfo.TypeIdField);
                if (!string.IsNullOrEmpty(layerInfo.DisplayField))
                    dictionary.Add("displayField", layerInfo.DisplayField);

                List<object> fields = new List<object>();
                layerInfo.Fields.ForEach(f => fields.Add(f.ToDictionary()));
                dictionary.Add("fields", fields);
                #endregion

                #region Layer-Specific Info
                string geometryType = null;
                switch (layerInfo.GeometryType)
                {
                    case Client.Tasks.GeometryType.Point:
                        geometryType = "esriGeometryPoint";
                        break;
                    case Client.Tasks.GeometryType.MultiPoint:
                        geometryType = "esriGeometryMultipoint";
                        break;
                    case Client.Tasks.GeometryType.Polyline:
                        geometryType = "esriGeometryPolyline";
                        break;
                    case Client.Tasks.GeometryType.Polygon:
                        geometryType = "esriGeometryPolygon";
                        break;
                    case Client.Tasks.GeometryType.Envelope:
                        geometryType = "esriGeometryEnvelope";
                        break;
                }
                if (geometryType != null)
                    dictionary.Add("geometryType", geometryType);

                dictionary.Add("minScale", layerInfo.MinimumScale);
                dictionary.Add("maxScale", layerInfo.MaximumScale);

                if (!string.IsNullOrEmpty(layerInfo.CopyrightText))
                    dictionary.Add("copyrightText", layerInfo.CopyrightText);
                if (layerInfo.Extent != null)
                    dictionary.Add("extent", layerInfo.Extent);
                if (layerInfo.DefaultSpatialReference != null)
                    dictionary.Add("spatialReference", layerInfo.DefaultSpatialReference);

                dictionary.Add("hasAttachments", layerInfo.HasAttachments);

                if (layerInfo.Renderer != null)
                    dictionary.Add("drawingInfo", layerInfo.Renderer.ToDictionary());

                // TODO: SUPPORT TIME INFO - TimeInfo property is not accessible
                //if (dictionary.ContainsKey("timeInfo"))
                //    this.TimeInfo = TimeInfo.FromDictionary(dictionary["timeInfo"] as Dictionary<string, object>);

                if (layerInfo.Relationships != null)
                {
                    List<Dictionary<string, object>> relationships = new List<Dictionary<string, object>>();
                    foreach (Relationship relationship in layerInfo.Relationships)
                        relationships.Add(relationship.ToDictionary());

                    dictionary.Add("relationships", relationships);
                }

                if (layerInfo.Capabilities != null)
                    dictionary.Add("capabilities", string.Join(",", layerInfo.Capabilities.ToArray()));

                dictionary.Add("maxRecordCount", layerInfo.MaxRecordCount);
                dictionary.Add("supportsAdvancedQueries", layerInfo.SupportsAdvancedQueries);
                dictionary.Add("supportsStatistics", layerInfo.SupportsStatistics);
                dictionary.Add("canModifyLayer", layerInfo.CanModifyLayer);
                dictionary.Add("canScaleSymbols", layerInfo.CanScaleSymbols);
                dictionary.Add("hasLabels", layerInfo.HasLabels);

                #endregion

                #region FeatureTypes
                if (layerInfo.FeatureTypes != null)
                {
                    List<Dictionary<string, object>> types = new List<Dictionary<string, object>>();
                    foreach (KeyValuePair<object, FeatureType> type in layerInfo.FeatureTypes)
                        types.Add(type.Value.ToDictionary());

                    dictionary.Add("types", types);
                }
                #endregion

                #region FeatureTemplates

                if (layerInfo.Templates != null)
                {
                    List<Dictionary<string, object>> templates = new List<Dictionary<string, object>>();
                    foreach (KeyValuePair<string, FeatureTemplate> template in layerInfo.Templates)
                        templates.Add(template.Value.ToDictionary());

                    dictionary.Add("templates", templates);
                }
                #endregion

                #region Editor Tracking
                if (layerInfo.EditFieldsInfo != null)
                    dictionary.Add("editFieldsInfo", layerInfo.EditFieldsInfo.ToDictionary());
                #endregion

                #region OwnershipBasedAccess
                if (layerInfo.OwnershipBasedAccessControl != null)
                    dictionary.Add("ownershipBasedAccessControlForFeatures",
                        layerInfo.OwnershipBasedAccessControl.ToDictionary());
                #endregion

                #region Z & M data
                dictionary.Add("hasZ", layerInfo.HasZ);
                dictionary.Add("hasM", layerInfo.HasM);
                dictionary.Add("zDefault", layerInfo.ZDefault);
                dictionary.Add("enableZDefaults", layerInfo.EnableZDefaults);
                dictionary.Add("allowGeometryUpdates", layerInfo.AllowGeometryUpdates);
                #endregion

                return dictionary;
            }
            catch
            {
                return null;
            }
        }

        // Field - TODO
        internal static Dictionary<string, object> ToDictionary(this ESRI.ArcGIS.Client.Field field)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(field.Name))
                dictionary.Add("name", field.Name);

            // CHECK TOSTRING VALUES
            dictionary.Add("type", "esriFieldType" + field.Type.ToString());

            if (!string.IsNullOrEmpty(field.Alias))
                dictionary.Add("alias", field.Alias);
            if (field.Domain != null)
                dictionary.Add("domain", field.Domain.ToDictionary());

            dictionary.Add("length", field.Length);
            dictionary.Add("editable", field.Editable);
            dictionary.Add("nullable", field.Nullable);

            return dictionary as Dictionary<string, object>;
        }

        // Renderer
        internal static Dictionary<string, object> ToDictionary(this IRenderer renderer)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            if (renderer is SimpleRenderer)
            {
                dictionary.Add("type", "simple");
                SimpleRenderer simpleRenderer = (SimpleRenderer)renderer;
                if (!string.IsNullOrEmpty(simpleRenderer.Label))
                    dictionary.Add("label", simpleRenderer.Label);
                if (!string.IsNullOrEmpty(simpleRenderer.Description))
                    dictionary.Add("description", simpleRenderer.Description);
                if (simpleRenderer.Symbol is IJsonSerializable)
                    dictionary.Add("symbol", ((IJsonSerializable)simpleRenderer.Symbol).ToJson());
            }
            else if (renderer is UniqueValueRenderer)
            {
                dictionary.Add("type", "uniqueValue");

                UniqueValueRenderer uvRenderer = (UniqueValueRenderer)renderer;
                if (!string.IsNullOrEmpty(uvRenderer.Field))
                    dictionary.Add("field1", uvRenderer.Field);
                if (!string.IsNullOrEmpty(uvRenderer.DefaultLabel))
                    dictionary.Add("defaultLabel", uvRenderer.DefaultLabel);

                if (uvRenderer.DefaultSymbol is IJsonSerializable)
                    dictionary.Add("defaultSymbol", ((IJsonSerializable)uvRenderer.DefaultSymbol).ToJson());

                List<Dictionary<string, object>> uniqueValueInfos = new List<Dictionary<string, object>>();
                foreach (UniqueValueInfo info in uvRenderer.Infos)
                {
                    Dictionary<string, object> uniqueValueInfo = info.ToDictionary();
                    if (info.Value != null)
                        uniqueValueInfo.Add("value", info.Value);

                    uniqueValueInfos.Add(uniqueValueInfo);
                }
                dictionary.Add("uniqueValueInfos", uniqueValueInfos);

            }
            else if (renderer is UniqueValueMultipleFieldsRenderer)
            {
                dictionary.Add("type", "uniqueValue");

                UniqueValueMultipleFieldsRenderer multiValRenderer = (UniqueValueMultipleFieldsRenderer)renderer;
                if (multiValRenderer.Fields != null)
                {
                    if (multiValRenderer.Fields.Length > 0)
                        dictionary.Add("field1", multiValRenderer.Fields[0]);
                    if (multiValRenderer.Fields.Length > 1)
                        dictionary.Add("field2", multiValRenderer.Fields[1]);
                    if (multiValRenderer.Fields.Length > 2)
                        dictionary.Add("field3", multiValRenderer.Fields[2]);
                }
                if (!string.IsNullOrEmpty(multiValRenderer.FieldDelimiter))
                    dictionary.Add("fieldDelimiter", multiValRenderer.FieldDelimiter);
                if (!string.IsNullOrEmpty(multiValRenderer.DefaultLabel))
                    dictionary.Add("defaultLabel", multiValRenderer.DefaultLabel);

                if (multiValRenderer.DefaultSymbol is IJsonSerializable)
                    dictionary.Add("defaultSymbol", ((IJsonSerializable)multiValRenderer.DefaultSymbol).ToJson());

                List<Dictionary<string, object>> uniqueValueInfos = new List<Dictionary<string, object>>();
                foreach (UniqueValueMultipleFieldsInfo info in multiValRenderer.Infos)
                {
                    Dictionary<string, object> uniqueValueInfo = info.ToDictionary();
                    if (info.Values != null && !string.IsNullOrEmpty(multiValRenderer.FieldDelimiter))
                        uniqueValueInfo.Add("value", string.Join(multiValRenderer.FieldDelimiter, info.Values));

                    uniqueValueInfos.Add(uniqueValueInfo);
                }
                dictionary.Add("uniqueValueInfos", uniqueValueInfos);
            }
            else if (renderer is ClassBreaksRenderer)
            {
                dictionary.Add("type", "classBreaks");

                ClassBreaksRenderer cbRenderer = (ClassBreaksRenderer)renderer;
                if (!string.IsNullOrEmpty(cbRenderer.NormalizationField))
                    dictionary.Add("normalizationField", cbRenderer.NormalizationField);

                // CHECK TOSTRING VALUE
                dictionary.Add("normalizationType", "esriNormalizeBy" + cbRenderer.NormalizationType.ToString());

                dictionary.Add("normalizationTotal", cbRenderer.NormalizationTotal);

                if (!string.IsNullOrEmpty(cbRenderer.Field))
                    dictionary.Add("field", cbRenderer.Field);

                if (cbRenderer.DefaultSymbol is IJsonSerializable)
                    dictionary.Add("defaultSymbol", ((IJsonSerializable)cbRenderer.DefaultSymbol).ToJson());

                List<Dictionary<string, object>> classBreakInfos = new List<Dictionary<string, object>>();
                double overallMinValue = double.MaxValue;
                foreach (ClassBreakInfo info in cbRenderer.Classes)
                {
                    Dictionary<string, object> classBreakInfo = info.ToDictionary();
                    classBreakInfo.Add("classMinValue", info.MinimumValue);
                    classBreakInfo.Add("classMaxValue", info.MaximumValue);
                    if (info.MinimumValue < overallMinValue)
                        overallMinValue = info.MinimumValue;

                    classBreakInfos.Add(classBreakInfo);
                }
                dictionary.Add("minValue", overallMinValue);
                dictionary.Add("classBreakInfos", classBreakInfos);
            }

            return dictionary;
        }

        // RendererInfo
        internal static Dictionary<string, object> ToDictionary(this RendererInfo rendererInfo)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(rendererInfo.Label))
                dictionary.Add("label", rendererInfo.Label);
            if (!string.IsNullOrEmpty(rendererInfo.Description))
                dictionary.Add("description", rendererInfo.Description);
            if (rendererInfo.Symbol is IJsonSerializable)
                dictionary.Add("symbol", ((IJsonSerializable)rendererInfo.Symbol).ToJson());

            return dictionary;
        }

        // Relationship
        public static Dictionary<string, object> ToDictionary(this Relationship relationship)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            dictionary.Add("id", relationship.Id);
            if (!string.IsNullOrEmpty(relationship.Name))
                dictionary.Add("name", relationship.Name);

            dictionary.Add("relatedTableId", relationship.RelatedTableId);

            // CHECK TOSTRING VALUES
            dictionary.Add("role", "esriRelRole" + relationship.Role.ToString());
            dictionary.Add("cardinality", "esriRelCardinality" + relationship.Cardinality.ToString());

            if (!string.IsNullOrEmpty(relationship.KeyField))
                dictionary.Add("keyField", relationship.KeyField);
            dictionary.Add("isComposite", relationship.IsComposite);
            if (!string.IsNullOrEmpty(relationship.KeyFieldInRelationshipTable))
                dictionary.Add("keyFieldInRelationshipTable", relationship.KeyFieldInRelationshipTable);

            dictionary.Add("relationshipTableId", relationship.RelationshipTableId);
            return dictionary;
        }

        // FeatureType
        internal static Dictionary<string, object> ToDictionary(this FeatureType featureType)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            if (featureType.Id != null)
                dictionary.Add("id", featureType.Id);
            if (!string.IsNullOrEmpty(featureType.Name))
                dictionary.Add("name", featureType.Name);

            if (featureType.Domains != null)
            {
                Dictionary<string, object> domains = new Dictionary<string, object>();
                // Add type so domains get deserialized
                domains.Add("type", "");
                foreach (KeyValuePair<string, Domain> domain in featureType.Domains)
                {
                    if (domain.Value != null)
                        domains.Add(domain.Key, domain.Value);
                }

                dictionary.Add("domains", domains);
            }

            if (featureType.Templates != null)
            {
                List<Dictionary<string, object>> templates = new List<Dictionary<string, object>>();
                foreach (KeyValuePair<string, FeatureTemplate> template in featureType.Templates)
                    templates.Add(template.Value.ToDictionary());

                dictionary.Add("templates", templates);
            }

            return dictionary;
        }

        // Domain
        internal static Dictionary<string, object> ToDictionary(this Domain domain)
        {
            if (domain is RangeDomain<IComparable>)
                return ((RangeDomain<IComparable>)domain).ToDictionary();
            else if (domain is CodedValueDomain)
                return ((CodedValueDomain)domain).ToDictionary();
            else
                return null;
        }

        // RangeDomain
        internal static Dictionary<string, object> ToDictionary(this RangeDomain<IComparable> domain)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(domain.Name))
                dictionary.Add("name", domain.Name);

            List<object> range = new List<object>();
            range.Add(domain.MinimumValue);
            range.Add(domain.MaximumValue);
            dictionary.Add("range", range);

            return dictionary;
        }

        // CodedValueDomain
        public static Dictionary<string, object> ToDictionary(this CodedValueDomain domain)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(domain.Name))
                dictionary.Add("name", domain.Name);

            if (domain.CodedValues != null)
            {
                List<Dictionary<string, object>> codedValues = new List<Dictionary<string, object>>();
                foreach (KeyValuePair<object, string> value in domain.CodedValues)
                    codedValues.Add(value.ToDictionary());

                dictionary.Add("codedValues", codedValues);
            }

            return dictionary;
        }

        // CodedValue
        internal static Dictionary<string, object> ToDictionary(this KeyValuePair<object, string> value)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            dictionary.Add("code", value.Key);
            if (!string.IsNullOrEmpty(value.Value))
                dictionary.Add("name", value.Value);

            return dictionary;
        }

        // FeatureTemplate
        internal static Dictionary<string, object> ToDictionary(this FeatureTemplate template)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(template.Name))
                dictionary.Add("name", template.Name);
            if (!string.IsNullOrEmpty(template.Description)) 
                dictionary.Add("description", template.Description);

            // CHECK TOSTRING VALUE
            dictionary.Add("drawingTool", "esriFeatureEditTool" + template.DrawingTool.ToString());

            if (template.PrototypeGeometry != null || template.PrototypeAttributes != null)
            {
                Dictionary<string, object> prototype = new Dictionary<string, object>();

                if (template.PrototypeGeometry != null)
                    prototype.Add("geometry", JsonSerializer.Serialize<Geometry>(template.PrototypeGeometry));
                if (template.PrototypeAttributes != null)
                    prototype.Add("attributes", template.PrototypeAttributes);

                dictionary.Add("prototype", prototype);
            }

            return dictionary;
        }

        // EditFieldsInfo
        internal static Dictionary<string, object> ToDictionary(this EditFieldsInfo info)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(info.CreationDateField))
                dictionary.Add("creationDateField", info.CreationDateField);
            if (!string.IsNullOrEmpty(info.CreatorField))
                dictionary.Add("creatorField", info.CreatorField);
            if (!string.IsNullOrEmpty(info.EditDateField))
                dictionary.Add("editDateField", info.EditDateField);
            if (!string.IsNullOrEmpty(info.EditorField))
                dictionary.Add("editorField", info.EditorField);
            if (!string.IsNullOrEmpty(info.Realm))
                dictionary.Add("realm", info.Realm);

            return dictionary;
        }

        // OwnershipBasedAccessControl
        internal static Dictionary<string, object> ToDictionary(this OwnershipBasedAccessControl ctrl)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            dictionary.Add("allowOthersToUpdate", ctrl.AllowOthersToUpdate);
            dictionary.Add("allowOthersToDelete", ctrl.AllowOthersToDelete);

            return dictionary;
        }

        #endregion
    }
}
