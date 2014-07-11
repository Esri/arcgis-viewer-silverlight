/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.GP.MetaData
{
	[DataContract]
	public class GPMetaData
	{
		[DataMember(Name="name")]
		public string Name { get; set; }
		[DataMember(Name = "displayName")]
		public string DisplayName { get; set; }
		[DataMember(Name = "category")]
		public string Category { get; set; }
		[DataMember(Name = "helpUrl")]
		public Uri HelpUrl { get; set; }
		[DataMember(Name = "executionType")]
		public string ExecutionType { get; set; }
		[DataMember(Name = "parameters")]
		public GPParameter[] Parameters { get; set; }
        [DataMember(Name = "resultMapServerName")]
        public string ResultMapServerName { get; set; }
        [DataMember(Name = "currentVersion")]
        public string CurrentVersion { get; set; }
    }

	[DataContract]
	public class GPParameter
	{
		[DataMember(Name = "name")]
		public string Name { get; set; }
		[DataMember(Name = "dataType")]
		public string DataType { get; set; }
		[DataMember(Name = "displayName")]
		public string DisplayName { get; set; }
		[DataMember(Name = "direction")]
		public string Direction { get; set; }
		[DataMember(Name = "defaultValue")]
		public object DefaultValue { get; set; }
        [DataMember(Name = "parameterType")]
		public string ParameterType { get; set; }
        [DataMember(Name = "category")]
		public string Category { get; set; }
        [DataMember(Name = "choiceList")]
        public string[] ChoiceList { get; set; }
    }

	[DataContract]
	public class GPLinearUnit
	{
		[DataMember(Name = "distance")]
		public double Distance { get; set; }
		[DataMember(Name = "units")]
		public string Units { get; set; }
	}

	[DataContract]
	public class GPDataFile
	{
		[DataMember(Name = "url")]
		public string Url { get; set; }
	}

	[DataContract]
	public class GPFeatureRecordSetLayer
	{
		[DataMember(Name = "geometryType")]
		public string GeometryType { get; set; }
		[DataMember(Name = "spatialReference")]
		public ESRI.ArcGIS.Client.Geometry.SpatialReference SpatialReference { get; set; }
		[DataMember(Name = "fields")]
		public Field[] Fields { get; set; }
	}

	[DataContract]
	public class Field
	{
		[DataMember(Name = "name")]
		public string Name { get; set; }
		[DataMember(Name = "type")]
		public string Type { get; set; }
		[DataMember(Name = "alias")]
		public string Alias { get; set; }
	}
}
