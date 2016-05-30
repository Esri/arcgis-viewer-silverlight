/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    /// <summary>
    /// Catalog
    /// </summary>
    [DataContract]
    public class Catalog
    {
        /// <summary>
        /// The name of the folder
        /// </summary>
        [DataMember(Name = "folders")]
        public List<string> Folders { get; set; }

        /// <summary>
        /// The name of the service
        /// </summary>
        [DataMember(Name = "services")]
        public List<Service> Services { get; set; }
    }
}
