/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Core
{
    public static class FieldHelper
    {
        public static FieldType MapFieldType(ESRI.ArcGIS.Client.Field.FieldType fieldType)
        {
            if (fieldType == ESRI.ArcGIS.Client.Field.FieldType.Double
                || fieldType == Client.Field.FieldType.Single)
            {
                return FieldType.DecimalNumber;
            }
            else if (fieldType == Client.Field.FieldType.OID
                || fieldType == ESRI.ArcGIS.Client.Field.FieldType.Integer
                || fieldType == ESRI.ArcGIS.Client.Field.FieldType.SmallInteger)
            {
                return FieldType.Integer;
            }
            else if (fieldType == Client.Field.FieldType.Date)
            {
                return FieldType.DateTime;
            }
            return FieldType.Text; // For now all other fields are treated as strings
        }

        public static bool IsFieldFilteredOut(string fieldTypeStr)
        {
            if (fieldTypeStr.Equals("esriFieldTypeOID") || 
                fieldTypeStr.Equals("esriFieldTypeBlob") || 
                fieldTypeStr.Equals("esriFieldTypeGeometry") ||
                fieldTypeStr.Equals("esriFieldTypeRaster"))
                return true;

            return false;
        }

        public static bool IsFieldFilteredOut(ESRI.ArcGIS.Client.Field.FieldType fieldType)
        {
            if (fieldType == ESRI.ArcGIS.Client.Field.FieldType.OID ||
                fieldType == ESRI.ArcGIS.Client.Field.FieldType.Blob ||
                fieldType == ESRI.ArcGIS.Client.Field.FieldType.Geometry ||
                fieldType == ESRI.ArcGIS.Client.Field.FieldType.Raster ||
                fieldType == ESRI.ArcGIS.Client.Field.FieldType.Unknown)
                return true;

            return false;
        }
    }
}
