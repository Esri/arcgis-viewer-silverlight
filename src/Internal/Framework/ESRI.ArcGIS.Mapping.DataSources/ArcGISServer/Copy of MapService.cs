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
    public class MapService 
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
                if (GetMapServiceDetailsFailed != null)
                    GetMapServiceDetailsFailed(this, new ExceptionEventArgs(e.Error, e.UserState));
                return;
            }

            if (string.IsNullOrEmpty(e.Result))
            {
                if (GetMapServiceDetailsFailed != null)
                    GetMapServiceDetailsFailed(this, new ExceptionEventArgs(new Exception("Empty response"), e.UserState));
                return;
            }

            MapServiceInfo mapServiceInfo = null;
            try
            {                
                string json = e.Result;
                byte[] bytes = Encoding.Unicode.GetBytes(json);
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(bytes))
                {
                    DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MapServiceInfo));
                    mapServiceInfo = dataContractJsonSerializer.ReadObject(memoryStream) as MapServiceInfo;
                    memoryStream.Close();
                }

                if (mapServiceInfo == null)
                {
                    if (GetMapServiceDetailsFailed != null)
                        GetMapServiceDetailsFailed(this, new ExceptionEventArgs(new Exception("Unable to deserialize response"), e.UserState));
                    return;
                }
            }
            catch(Exception ex)
            {
                if (GetMapServiceDetailsFailed != null)
                    GetMapServiceDetailsFailed(this, new ExceptionEventArgs(ex, e.UserState));
                return;
            }

            List<Resource> childResources = new List<Resource>();
            if (mapServiceInfo.Layers != null)
            {
                foreach (LayerInfo layerInfo in mapServiceInfo.Layers)
                {
                    if (layerInfo.SubLayerIds == null)
                    {
                        childResources.Add(new Resource()
                        {
                            DisplayName = layerInfo.Name,
                            ResourceType = ResourceType.Layer,
                            Url = string.Format("{0}/{1}", Uri, layerInfo.ID),
                        });
                    }
                    else
                    {
                        childResources.Add(new Resource()
                        {
                            DisplayName = layerInfo.Name,
                            ResourceType = ResourceType.GroupLayer,
                            Url = string.Format("{0}/{1}", Uri, layerInfo.ID),
                        });
                    }
                }
            }

            if (GetMapServiceDetailsCompleted != null)
                GetMapServiceDetailsCompleted(this, new GetMapServiceDetailsCompletedEventArgs() { ChildResources = childResources, MapServiceInfo = mapServiceInfo, UserState = e.UserState });
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
