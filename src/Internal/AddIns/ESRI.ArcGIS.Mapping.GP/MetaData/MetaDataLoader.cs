/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using ESRI.ArcGIS.Mapping.DataSources.ArcGISServer;
using ESRI.ArcGIS.Mapping.GP.Resources;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.GP.MetaData
{
    public class MetaDataLoader
    {
        public Uri ServiceEndpoint { get; set; }

        public event EventHandler LoadSucceeded;
        public event EventHandler LoadFailed;

        public GPMetaData ServiceInfo { get; private set; }
        public Exception Error { get; private set; }

        public void LoadMetadata(bool designTime, string proxyUrl = null)
        {
            ServiceInfo = null;
            Error = null;
            if (designTime)
            {
                //Designtime datasource
                ServiceInfo = new GPMetaData()
                {
                    DisplayName = "{TITLE}",
                    Parameters = new GPParameter[] {
								new GPParameter() { 
									DisplayName = "{DisplayName1}", DefaultValue = "{Default value}", Name="Param1",
									DataType = "GPString", Direction="esriGPParameterDirectionInput", ParameterType="esriGPParameterTypeRequired"
								},
								new GPParameter() { 
									DisplayName = "{DisplayName2}", DefaultValue = "{Default value}", Name="Param2",
									DataType = "GPString", Direction="esriGPParameterDirectionInput", ParameterType="esriGPParameterTypeRequired"
								}
						}
                };
                if (LoadSucceeded != null)
                    LoadSucceeded(this, null);
                return;
            }
            ArcGISWebClient wc = new ArcGISWebClient() { ProxyUrl = proxyUrl };
            wc.DownloadStringCompleted += wc_OpenReadCompleted;
            
            // Get service endpoint JSON URL
            string url = ServiceEndpoint.AbsoluteUri + "?f=json";

            Uri uri = new Uri(url, UriKind.Absolute);
            wc.DownloadStringAsync(uri, null, ArcGISWebClient.HttpMethods.Auto, proxyUrl);
        }

        private void GPServerInfoDownloaded(object sender, ArcGISWebClient.DownloadStringCompletedEventArgs e)
        {
            #region Error checking
            if (e.Error != null)
            {
                Error = e.Error;
                if (LoadFailed != null)
                    LoadFailed(this, null);
                return;
            }

            string json = e.Result;
            if (string.IsNullOrEmpty(json) || json.StartsWith("{\"error\":", StringComparison.Ordinal))
            {
                if (LoadFailed != null)
                    LoadFailed(this, null);
                return;
            }
            #endregion

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GeoprocessingServiceInfo));
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
            GeoprocessingServiceInfo gpServerInfo = (GeoprocessingServiceInfo)serializer.ReadObject(ms);

            ServiceInfo.ResultMapServerName = gpServerInfo.ResultMapServerName;
            ServiceInfo.CurrentVersion = gpServerInfo.CurrentVersion;
                
            if (LoadSucceeded != null)
                LoadSucceeded(this, null);
        }

        private void wc_OpenReadCompleted(object sender, ArcGISWebClient.DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Error = e.Error;
                if (LoadFailed != null)
                    LoadFailed(this, null);
                return;
            }

            // Make sure response is not empty
            string json = e.Result;
            if (string.IsNullOrEmpty(json))
            {
                Error = new Exception(Strings.EmptyResponse);
                if (LoadFailed != null)
                    LoadFailed(this, null);
                return;
            }

            // Check whether response contains error message
            if (json.StartsWith("{\"error\":", StringComparison.Ordinal))
            {
                try
                {
                    // Parse error message
                    ESRI.ArcGIS.Client.Utils.JavaScriptSerializer jss = new Client.Utils.JavaScriptSerializer();
                    Dictionary<string, object> dictionary = jss.DeserializeObject(json) as 
                        Dictionary<string, object>;

                    bool errorRetrieved = false;
                    if (dictionary != null && dictionary.ContainsKey("error"))
                    {
                        Dictionary<string, object> errorInfo = dictionary["error"] 
                            as Dictionary<string, object>;
                        if (errorInfo != null && errorInfo.ContainsKey("message")
                        && errorInfo["message"] is string)
                        {
                            Error = new Exception((string)errorInfo["message"]);
                            errorRetrieved = true;
                        }
                    }

                    if (!errorRetrieved)
                        Error = new Exception(Strings.UnexpectedServiceAccessError);
                }
                catch
                {
                    Error = new Exception(Strings.UnexpectedServiceAccessError);
                }

                if (LoadFailed != null)
                    LoadFailed(this, null);

                return;
            }

            //Inject __type information to help DataContractJsonSerializer determine which abstract class to
            //use when deserialing defaultValue property.
            int idx = json.IndexOf("\"dataType\"", 0, StringComparison.Ordinal);
            json = json.Replace("\"defaultValue\":{}", "\"defaultValue\":null");
            while (idx > -1)
            {
                string type = json.Substring(idx + 12,
                    json.Substring(idx + 13).IndexOf("\"", StringComparison.Ordinal) + 1);
                int start = json.IndexOf("\"defaultValue\":{", idx, StringComparison.Ordinal);
                int start2 = json.IndexOf("\"defaultValue\":[", idx, StringComparison.Ordinal);
                if (start2 > 0 && start2 < start)
                    start = start2;

                if (start > -1)
                {
                    string __type = null;
                    if (type == "GPFeatureRecordSetLayer")
                    {
                        __type = "\"__type\":\"GPFeatureRecordSetLayer:#ESRI.ArcGIS.Mapping.GP.MetaData\",";
                    }
                    else if (type == "GPLinearUnit")
                    {
                        __type = "\"__type\":\"GPLinearUnit:#ESRI.ArcGIS.Mapping.GP.MetaData\",";
                    }
                    else if (type == "GPDataFile")
                    {
                        __type = "\"__type\":\"GPDataFile:#ESRI.ArcGIS.Mapping.GP.MetaData\",";
                    }
                    if (__type != null)
                        json = json.Substring(0, start + 16) + __type + json.Substring(start + 16);
                }
                idx = json.IndexOf("\"dataType\"", idx + 10, StringComparison.Ordinal);
            }
            json = json.Replace("}\"Fields\"", "},\"Fields\""); //fix for bug in service
            Type[] types = {
						typeof(GPFeatureRecordSetLayer), 
						typeof(GPLinearUnit),
						typeof(GPDataFile)
					};
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GPMetaData), types);
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
            object graph = serializer.ReadObject(ms);
            ServiceInfo = (GPMetaData)graph;

            ArcGISWebClient wc = new ArcGISWebClient() { ProxyUrl = e.UserState as string };
            wc.DownloadStringCompleted += GPServerInfoDownloaded;
            string gp = "/GPServer/";

            string url = ServiceEndpoint.AbsoluteUri.Substring(0, 
                ServiceEndpoint.AbsoluteUri.LastIndexOf(gp) + gp.Length - 1) + "?f=json";

            Uri uri = new Uri(url, UriKind.Absolute);
            wc.DownloadStringAsync(uri);
        }
    }
}
