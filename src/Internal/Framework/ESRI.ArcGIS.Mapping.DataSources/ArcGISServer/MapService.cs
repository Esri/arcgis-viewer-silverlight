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
using ESRI.ArcGIS.Client;

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

        public void GetMapServiceDetails(object userState)
        {
            if (string.IsNullOrEmpty(Uri))
                throw new InvalidOperationException("Uri must not be null");

            UriBuilder builder = new UriBuilder(Uri);
            builder.Query = Utils.GetQueryParameters(Uri);
            Uri finalUrl = builder.Uri;

            webClient = new WebClient()
            {
#if SILVERLIGHT
                AllowReadStreamBuffering = false 
#endif
            };
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
                Exception ex = Utils.CheckJsonForException(json);
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

            List<Resource> childResources = new List<Resource>();
            if (mapServiceInfo.Layers != null)
            {                
                foreach (MapServiceLayerInfo layerInfo in mapServiceInfo.Layers)
                {
                    if (layerInfo.ParentLayerId != -1) // only had layers at the root of the map service node (no parent)
                        continue;
                    if (layerInfo.SubLayerIds == null)
                    {
                        childResources.Add(new Resource()
                        {
                            DisplayName = layerInfo.Name,
                            ResourceType = ResourceType.Layer,
                            Url = string.Format("{0}/{1}", Uri, layerInfo.ID),
                            Tag = layerInfo.ID,
                        });
                    }
                    else
                    {             
                        childResources.Add(new Resource()
                        {
                            DisplayName = layerInfo.Name,
                            ResourceType = ResourceType.GroupLayer,
                            Url = string.Format("{0}/{1}", Uri, layerInfo.ID),
                            Tag = layerInfo.ID,
                        });
                    }
                }
            }

            // Workaround for the fact that DataContractJsonSerializer doesn't call the default contructor
            // http://msdn.microsoft.com/en-us/library/system.runtime.serialization.datacontractserializer.aspx 
            if (mapServiceInfo.SpatialReference != null)
            {
                if (mapServiceInfo.SpatialReference.WKID == default(int))
                    mapServiceInfo.SpatialReference.WKID = -1;
            }

            OnGetMapServiceDetailsCompleted(new GetMapServiceDetailsCompletedEventArgs() { ChildResources = childResources, MapServiceInfo = mapServiceInfo, UserState = e.UserState });
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
