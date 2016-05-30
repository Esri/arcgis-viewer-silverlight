/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.Core
{
    public static class GraphicsLayerTypeFixer
    {
        private static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    
        private static object ConvertToExpectedType(object valueObject, FieldType expectedValueType)
        {
            object result = valueObject;
            switch (expectedValueType)
            {
                case FieldType.DateTime:
                    if (valueObject is string)
                        result = Convert.ToDateTime(valueObject, CultureInfo.InvariantCulture);
                    else
                        try
                        {
                            result = Epoch.AddMilliseconds(Convert.ToDouble(valueObject, CultureInfo.InvariantCulture));
                        }
                        catch { }
                    break;
                case FieldType.DecimalNumber: // Use double instead of decimal so values match renderer
                    if (valueObject.GetType() != typeof(double))
                    {
                        try { result = Convert.ToDouble(valueObject, CultureInfo.InvariantCulture); }
                        catch { }
                    }
                    break;
                case FieldType.Integer:
                    if (valueObject.GetType() != typeof(int))
                    {
                        try { result = Convert.ToInt32(valueObject, CultureInfo.InvariantCulture); }
                        catch { }
                    }
                    break;
                case FieldType.Text:
                    if (valueObject.GetType() != typeof(string))
                    {
                        try { result = Convert.ToString(valueObject, CultureInfo.InvariantCulture); }
                        catch { }
                    }
                    break;
                case FieldType.Attachment:
                case FieldType.Boolean:
                case FieldType.Currency:
                case FieldType.Entity:
                case FieldType.Hyperlink:
                case FieldType.Image:
                case FieldType.Lookup:
                    break;
            }
            return result;
        }

        private static void CorrectDataTypes(IDictionary<string, object> attributes, IEnumerable<FieldInfo> fields)
        {
            if (attributes == null || fields == null) return;
            foreach (FieldInfo field in fields)
            {
                try
                {
                    object val = attributes[field.Name];
                    if (val == null) continue;
                    attributes[field.Name] = ConvertToExpectedType(val, field.FieldType);
                }
                catch (KeyNotFoundException) { }
            }
        }

        public static void CorrectDataTypes(IEnumerable<Graphic> graphics, GraphicsLayer layer)
        {
            IEnumerable<FieldInfo> fields = GetFields(layer);
            foreach (Graphic graphic in graphics)
                CorrectDataTypes(graphic.Attributes, fields);
        }

        private static IEnumerable<FieldInfo> GetFields(GraphicsLayer layer)
        {
            Collection<FieldInfo> fields = Core.LayerExtensions.GetFields(layer);
            if (fields.Count() == 0 && layer.Graphics.Count > 0)
            {
                Dictionary<string, Dictionary<FieldType, int>> fieldTypesDict = layer.Graphics[0].Attributes.ToDictionary(k => k.Key, v => new Dictionary<FieldType, int> { { GetFieldType(v.Value), 1 } });
                IEnumerable<string> fieldNames = fieldTypesDict.Keys.ToArray();
                foreach (Graphic graphic in layer.Graphics)
                {
                    foreach (string fieldName in fieldNames)
                    {
                        object value = graphic.Attributes[fieldName];
                        FieldType valueType = GetFieldType(value);
                        if (fieldTypesDict[fieldName].ContainsKey(valueType))
                            fieldTypesDict[fieldName][valueType]++;
                        else
                            fieldTypesDict[fieldName] = new Dictionary<FieldType, int> { { valueType, 1 } };
                    }
                }

                foreach (var item in fieldTypesDict)
                    fields.Add(new FieldInfo
                               {
                                   Name = item.Key,
                                   FieldType = item.Value.OrderByDescending(s => s.Value).First().Key,
                                   VisibleInAttributeDisplay = true,
                                   DisplayName = item.Key,
                                   VisibleOnMapTip = false
                               });
            }
            return fields;
        }

        private static FieldType GetFieldType(object valueObject)
        {
            if (valueObject == null) return FieldType.Text;
            Type valueType = valueObject.GetType();
            if (valueType == typeof(DateTime))
                return FieldType.DateTime;
            if (valueType == typeof(double) || valueType == typeof(float))
                return FieldType.DecimalNumber;
            if (valueType == typeof(int) || valueType == typeof(short))
                return FieldType.Integer;
            if (valueType == typeof(string))
                return FieldType.Text;

            return FieldType.Text;
        }
    }
}
