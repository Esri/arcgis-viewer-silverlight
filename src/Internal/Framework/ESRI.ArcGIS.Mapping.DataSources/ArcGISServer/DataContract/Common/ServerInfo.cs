/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    /// <summary>
    /// Provides metadata about the server
    /// </summary>
    [DataContract]
    public class ServerInfo
    {
        /// <summary>
        /// The version of the software providing the service endpoint as a number
        /// </summary>
        [DataMember(Name = "currentVersion")]
        public double CurrentVersion { get; set; }

        /// <summary>
        /// The version of the software providing the service endpoint as a string
        /// </summary>
        [DataMember(Name = "fullVersion")]
        public string FullVersion { get; set; }

        /// <summary>
        /// The URL to the server's SOAP service endpoint
        /// </summary>
        [DataMember(Name = "soapUrl")]
        public string SoapUrl { get; set; }

        /// <summary>
        /// The URL to the server's SSL-secured SOAP service endpoint
        /// </summary>
        [DataMember(Name = "secureSoapUrl")]
        public string SecureSoapUrl { get; set; }

        /// <summary>
        /// URL to the main host of the federated system
        /// </summary>
        [DataMember(Name = "owningSystemUrl")]
        public string OwningSystemUrl { get; set; }

        /// <summary>
        /// Information about how the server supports authentication
        /// </summary>
        [DataMember(Name = "authInfo")]
        public AuthenticationInfo AuthenticationInfo { get; set; }

        /// <summary>
        /// The URL used to retrieve server info, including any necessary query string parameters
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The URL used to retrieve server info, excluding query string parameters
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// The URL of a proxy service used to retrieve server info
        /// </summary>
        public string ProxyUrl { get; set; }
    }
}
