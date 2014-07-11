/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.FeatureService;

namespace ESRI.ArcGIS.Mapping.Core
{
    [DataContract]
    public class FieldInfo : FieldSettings
    {
        private string _alias;
        [DataMember(EmitDefaultValue = false)]
        public string AliasOnServer
        {
            get
            {
                return _alias ?? Name;
            }
            set
            {
                if (_alias != value)
                {
                    _alias = value;
                    OnPropertyChanged("AliasOnServer");
                }
            }
        }

		private DomainSubtypeLookup _lookup;
		[DataMember(EmitDefaultValue = false)]
		public DomainSubtypeLookup DomainSubtypeLookup
		{
			get
			{
				return _lookup;
			}
			set
			{
				if (_lookup != value)
				{
					_lookup = value;
					OnPropertyChanged("DomainSubtypeLookup");
				}
			}
		}
		
		public static bool SupportedInHeader(FieldType fieldType)
        {
            switch (fieldType)
            {
                case FieldType.Text:
                case FieldType.DecimalNumber:
                case FieldType.Integer:
                case FieldType.Currency:
                case FieldType.Image:
                case FieldType.Lookup:
                case FieldType.Boolean:
                case FieldType.DateTime:
                    return true;
                case FieldType.Attachment:
                case FieldType.Hyperlink:
                case FieldType.Entity:
                default:
                    return false;
            }
        }

        public static string GetDefaultDisplayField(IEnumerable<FieldInfo> fields)
        {
            string displayField = null;
            bool foundStringField = false;
            foreach (FieldInfo item in fields)
            {
                if (item.FieldType == FieldType.Text)
                {
                    if (displayField == null || !foundStringField) //2. Default to first string field
                    {
                        displayField = item.Name;
                        foundStringField = true;
                        if (item.Name.ToLower().Contains("name"))
                            break;
                    }
                    else if (item.Name.ToLower().Contains("name")) //3. Default to string field with 'name'
                    {
                        displayField = item.Name;
                        break;
                    }
                }
            }
            return displayField;
        }

        public static void WriteFieldInfos(IEnumerable<FieldInfo> fields, XmlWriter writer)
        {
            foreach (FieldInfo field in fields)
            {
                writer.WriteStartElement(Constants.esriMappingPrefix, "FieldInfo", Constants.esriMappingNamespace);
                if (!string.IsNullOrEmpty(field.Name))
                    writer.WriteAttributeString("Name", field.Name);
                if (!string.IsNullOrEmpty(field.DisplayName))
                    writer.WriteAttributeString("DisplayName", field.DisplayName);
                if (field.VisibleOnMapTip == false)
                    writer.WriteAttributeString("VisibleOnMapTip", "false");
                if (field.VisibleInAttributeDisplay == false)
                    writer.WriteAttributeString("VisibleInAttributeDisplay", "false");
                if (field.FieldType != FieldType.Text)
                    writer.WriteAttributeString("FieldType", field.FieldType.ToString());
                if (!string.IsNullOrEmpty(field.AliasOnServer))
                    writer.WriteAttributeString("AliasOnServer", field.AliasOnServer);
				writer.WriteAttributeString("DomainSubtypeLookup", field.DomainSubtypeLookup.ToString());
				writer.WriteEndElement();
            }
        }

		static ESRI.ArcGIS.Client.Field GetField(string name, List<Client.Field> fields)
		{
			foreach (Client.Field item in fields)
			{
				if (item.Name == name)
					return item;
			}
			return null;
		}
		
		public static DomainSubtypeLookup GetDomainSubTypeLookup(Layer layer, FieldInfo field)
		{
			if (field.DomainSubtypeLookup != DomainSubtypeLookup.NotDefined)
				return field.DomainSubtypeLookup;
			ESRI.ArcGIS.Client.FeatureLayer featureLayer = layer as ESRI.ArcGIS.Client.FeatureLayer;
			if (featureLayer == null)
			{
				field.DomainSubtypeLookup = DomainSubtypeLookup.None;
				return field.DomainSubtypeLookup;
			}
			ESRI.ArcGIS.Client.FeatureService.FeatureLayerInfo featureLayerInfo = featureLayer.LayerInfo;
			if (featureLayerInfo != null)
			{
				Client.Field f = GetField(field.Name, featureLayerInfo.Fields);
				field.DomainSubtypeLookup = GetDomainSubTypeLookup(featureLayerInfo, f);
			}
			return field.DomainSubtypeLookup;
		}
		public static DomainSubtypeLookup GetDomainSubTypeLookup(FeatureLayerInfo featureLayerInfo, Client.Field field)
		{
			DomainSubtypeLookup lookup = DomainSubtypeLookup.None;
			if (featureLayerInfo != null)
			{

				Client.Field f = GetField(field.Name, featureLayerInfo.Fields);
				//Field has domain
				if (f != null && f.Domain is CodedValueDomain)
				{
					lookup = DomainSubtypeLookup.FieldDomain;
				}
				//field is type id field
				else if (featureLayerInfo.TypeIdField == field.Name)
				{
					lookup = DomainSubtypeLookup.TypeIdField;
				}
				//feature type defines domain for field
				else if (featureLayerInfo.FeatureTypes != null)
				{
					foreach (KeyValuePair<object, FeatureType> fTypePair in featureLayerInfo.FeatureTypes)
					{
						if (fTypePair.Value != null && fTypePair.Value.Domains != null)
						{
							foreach (KeyValuePair<string, Domain> pair in fTypePair.Value.Domains)
							{
								if (pair.Key.Equals(field.Name) && pair.Value is CodedValueDomain)
								{
									lookup = DomainSubtypeLookup.FeatureTypeDomain;
									break;
								}

							}
						}
					}
				}
			}

			return lookup;
		}
		
		public static Core.FieldInfo FieldInfoFromField(Layer layer, Client.Field field)
		{
			ESRI.ArcGIS.Mapping.Core.FieldInfo fieldInfo = new ESRI.ArcGIS.Mapping.Core.FieldInfo()
			{
				DisplayName = field.Alias,
				AliasOnServer = field.Alias,
				FieldType = mapFieldType(field.Type),
				Name = field.Name,
				VisibleInAttributeDisplay = true,
				VisibleOnMapTip = true,
			};
			ESRI.ArcGIS.Client.FeatureLayer featureLayer = layer as ESRI.ArcGIS.Client.FeatureLayer;
			if (featureLayer != null)
			{
				ESRI.ArcGIS.Client.FeatureService.FeatureLayerInfo featureLayerInfo = featureLayer.LayerInfo;
				if (featureLayerInfo != null)
				{
					fieldInfo.DomainSubtypeLookup = GetDomainSubTypeLookup(featureLayerInfo, field);
				}
			}
			return fieldInfo;
		}

		static FieldType mapFieldType(Client.Field.FieldType fieldType)
		{
			switch (fieldType)
			{
				case ESRI.ArcGIS.Client.Field.FieldType.Double:
				case ESRI.ArcGIS.Client.Field.FieldType.Single:
					return FieldType.DecimalNumber;
				case ESRI.ArcGIS.Client.Field.FieldType.Integer:
				case ESRI.ArcGIS.Client.Field.FieldType.SmallInteger:
				case ESRI.ArcGIS.Client.Field.FieldType.OID:
				case ESRI.ArcGIS.Client.Field.FieldType.Blob:
					return FieldType.Integer;
				case ESRI.ArcGIS.Client.Field.FieldType.Date:
					return FieldType.DateTime;
				case ESRI.ArcGIS.Client.Field.FieldType.GUID:
				case ESRI.ArcGIS.Client.Field.FieldType.Geometry:
				case ESRI.ArcGIS.Client.Field.FieldType.GlobalID:
				case ESRI.ArcGIS.Client.Field.FieldType.Raster:
				case ESRI.ArcGIS.Client.Field.FieldType.String:
				case ESRI.ArcGIS.Client.Field.FieldType.Unknown:
				case ESRI.ArcGIS.Client.Field.FieldType.XML:
				default:
					return FieldType.Text;
			}
		}
	}

	public enum DomainSubtypeLookup
	{
 		NotDefined,
		None,
		FieldDomain,
		TypeIdField,
		FeatureTypeDomain
	}
}
