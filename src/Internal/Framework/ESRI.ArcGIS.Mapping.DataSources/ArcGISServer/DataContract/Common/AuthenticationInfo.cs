/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    /// <summary>
    /// Provides information about how the server handles authentication
    /// </summary>
    [DataContract]
    public class AuthenticationInfo
    {
        /// <summary>
        /// Whether the server supports token authentication
        /// </summary>
        [DataMember(Name = "isTokenBasedSecurity")]
        public bool SupportsTokenAuthentication { get; set; }

        /// <summary>
        /// The URL to use for token retrieval 
        /// </summary>
        [DataMember(Name = "tokenServicesUrl")]
        public string TokenUrl { get; set; }

        /// <summary>
        /// The default lifespan of short-lived tokens, in minutes
        /// </summary>
        [DataMember(Name = "shortLivedTokenValidity")]
        public int ShortLivedTokenValidity { get; set; }
    }
}
