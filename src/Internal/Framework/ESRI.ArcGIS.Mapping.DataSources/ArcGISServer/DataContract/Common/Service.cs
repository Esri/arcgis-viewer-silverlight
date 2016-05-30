/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/


using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    [DataContract]
    public class Service 
    {
        /// <summary>
        /// Service name
        /// </summary>
        [DataMember(Name = "name")]       
        public string Name { get; set; }

        /// <summary>
        /// Service type
        /// </summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }
    }
}
