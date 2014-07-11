/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.DataSources.SpatialDataService 
{
    /// <summary>
    /// Catalog
    /// </summary>
    [DataContract]
	public class DatabaseTables
	{
        /// <summary>
        /// The name of the service
        /// </summary>
        [DataMember(Name = "tables")]
        public List<string> Tables { get; set; }
    }
}
