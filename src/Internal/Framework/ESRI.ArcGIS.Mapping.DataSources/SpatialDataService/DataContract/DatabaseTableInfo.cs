/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Runtime.Serialization;
using ESRI.ArcGIS.Client.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ESRI.ArcGIS.Mapping.DataSources.SpatialDataService 
{
    [DataContract]
    public class DatabaseTableInfo {
        // Properties
        [DataMember(Name = "tableName")]
        public string TableName { get; set; }
        [DataMember(Name = "fields")]
        public List<Field> Fields { get; set; }
        [DataMember(Name="primaryKeys")]
        public List<string> PrimaryKeys { get; set;}
        [DataMember(Name = "geometryType")]
        public string GeometryType { get; set; }

        internal bool DoesTableHasGeometryColumn()
        {   
            if (Fields != null)
            {
               Field geometryField = Fields.FirstOrDefault<Field>(f => f.DataType ==  "esriFieldTypeGeometry"
                                                                        || f.DataType == "Microsoft.SqlServer.Types.SqlGeometry"
                                                                        || f.DataType == "Microsoft.SqlServer.Types.SqlGeography");
               if (geometryField != null)
                   return true;
               bool hasShapeX = Fields.FirstOrDefault<Field>(f => "shapex".Equals(f.Name, StringComparison.InvariantCultureIgnoreCase)) != null;
               if (!hasShapeX)
                   return false;
               return Fields.FirstOrDefault<Field>(f => "shapey".Equals(f.Name, StringComparison.InvariantCultureIgnoreCase)) != null;               
            } 
            return false;
        }
    }

	[DataContract]
	public class Field
	{
		// Properties
		[DataMember(Name = "name")]
		public string Name { get; set; }
		[DataMember(Name = "type")]
		public string DataType { get; set; }
	}
}
