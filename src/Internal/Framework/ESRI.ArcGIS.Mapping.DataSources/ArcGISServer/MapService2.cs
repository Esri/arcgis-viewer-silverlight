/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer 
{
    internal class MapService 
    {
        private WebClient webClient;
        public string Uri { get; set; }
        public MapService(string uri) 
        {
            Uri = uri;
        }

        public void GetMapServiceDetails()
        {
            GetMapServiceDetails(null);
        }

        public void GetMapServiceDetails(object userState)
        {
            if (string.IsNullOrEmpty(Uri))
                throw new InvalidOperationException("Uri must not be null");

            UriBuilder builder = new UriBuilder(Uri);
            builder.Query = "f=json";
            Uri finalUrl = builder.Uri;

            webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadStringCompleted);
            webClient.DownloadStringAsync(finalUrl, userState);  
        }

        public void Cancel()
        {            
            if (webClient != null && webClient.IsBusy)
                webClient.CancelAsync();                    
        }

        private void DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e) 
        {
            if (e.Error != null)
            {
                OnGetMapServiceDetailsFailed(new ExceptionEventArgs(e.Error, e.UserState));
                return;
            }

            if (string.IsNullOrEmpty(e.Result))
            {
                OnGetMapServiceDetailsFailed(new ExceptionEventArgs(new Exception("Empty response"), e.UserState));
                return;
            }

            MapServiceInfo mapServiceInfo = null;
            try
            {                
                string json = e.Result;
                Exception ex = createFromJSON(json);
                if (ex != null)
                {
                    OnGetMapServiceDetailsFailed(new ExceptionEventArgs(ex, e.UserState));
                    return;
                }
                byte[] bytes = Encoding.Unicode.GetBytes(json);
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(bytes))
                {
                    DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MapServiceInfo));
                    mapServiceInfo = dataContractJsonSerializer.ReadObject(memoryStream) as MapServiceInfo;
                    memoryStream.Close();
                }

                if (mapServiceInfo == null)
                {
                    OnGetMapServiceDetailsFailed(new ExceptionEventArgs(new Exception("Unable to deserialize response"), e.UserState));
                    return;
                }
            }
            catch(Exception ex)
            {
                OnGetMapServiceDetailsFailed(new ExceptionEventArgs(ex, e.UserState));
                return;
            }

            // Workaround for the fact that DataContractJsonSerializer doesn't call the default contructor
            // http://msdn.microsoft.com/en-us/library/system.runtime.serialization.datacontractserializer.aspx 
            if (mapServiceInfo.SpatialReference != null)
            {
                if (mapServiceInfo.SpatialReference.WKID == default(int))
                    mapServiceInfo.SpatialReference.WKID = -1;
            }

            OnGetMapServiceDetailsCompleted(new GetMapServiceDetailsCompletedEventArgs() { MapServiceInfo = mapServiceInfo, UserState = e.UserState });
        }

        private Exception createFromJSON(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            if (json.StartsWith("{\"error"))
            {                
                int pos = json.IndexOf("\"message\":\"");
                if (pos > -1)
                {
                    int pos2 = json.IndexOf("\"", pos + 1);
                    if (pos2 > -1)
                    {
                        return new Exception(json.Substring(pos + 1, pos2 - pos));
                    }
                }
                return new Exception(json);
            }
            return null;
        }

        protected void OnGetMapServiceDetailsCompleted(GetMapServiceDetailsCompletedEventArgs args)
        {
            if (GetMapServiceDetailsCompleted != null)
                GetMapServiceDetailsCompleted(this, args);
        }

        protected void OnGetMapServiceDetailsFailed(ExceptionEventArgs args)
        {
            if (GetMapServiceDetailsFailed != null)
                GetMapServiceDetailsFailed(this, args);
        }

        public event EventHandler<GetMapServiceDetailsCompletedEventArgs> GetMapServiceDetailsCompleted;
        public event EventHandler<ExceptionEventArgs> GetMapServiceDetailsFailed;
    }    

    public class GetMapServiceDetailsCompletedEventArgs : EventArgs
    {
        public IEnumerable<Resource> ChildResources { get; set; }
        public MapServiceInfo MapServiceInfo { get; set; }
        public object UserState { get; set; }
    }
}
