/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer 
{
    internal class GroupLayer 
    {
        private ArcGISWebClient webClient;
        public string Uri { get; set; }
        public string ProxyUrl { get; set; }
        public GroupLayer(string uri, string proxyUrl) 
        {
            Uri = uri;
            ProxyUrl = proxyUrl;
        }

        Uri finalUrl;
        public async void GetGroupLayerDetails(object userState)
        {
            if (string.IsNullOrEmpty(Uri))
                throw new InvalidOperationException(Resources.Strings.ExceptionUriMustNotBeNull);

            UriBuilder builder = new UriBuilder(Uri);
            builder.Query = Utils.GetQueryParameters(Uri);
            finalUrl = builder.Uri;

            finalUrl = ESRI.ArcGIS.Mapping.Core.Utility.CreateUriWithProxy(ProxyUrl, finalUrl);

            if (webClient == null)
                webClient = new ArcGISWebClient();

            try
            {
                ArcGISWebClient.DownloadStringCompletedEventArgs result =
                    await webClient.DownloadStringTaskAsync(finalUrl, userState);
                processResult(result);
            }
            catch (Exception ex)
            {
                OnGetGroupLayerDetailsFailed(new ExceptionEventArgs(ex, userState));
            }
        }

        public void Cancel()
        {           
            if (webClient != null && webClient.IsBusy)
                webClient.CancelAsync();
        }        
        
        private void processResult(ArcGISWebClient.DownloadStringCompletedEventArgs e) 
        {
            if (e.Cancelled)
                return;

            if (e.Error != null)
            {
                OnGetGroupLayerDetailsFailed(new ExceptionEventArgs(e.Error, e.UserState));
                return;
            }

            if (string.IsNullOrEmpty(e.Result))
            {
                OnGetGroupLayerDetailsFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionEmptyResponse), e.UserState));
                return;
            }

            LayerDetails layerDetails;
            try
            {
                string json = e.Result;
                Exception exception = Utils.CheckJsonForException(json);
                if (exception != null)
                {
                    OnGetGroupLayerDetailsFailed(new ExceptionEventArgs(exception, e.UserState));
                    return;
                }
                byte[] bytes = Encoding.Unicode.GetBytes(json);
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(bytes))
                {
                    DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(LayerDetails));
                    layerDetails = dataContractJsonSerializer.ReadObject(memoryStream) as LayerDetails;
                    memoryStream.Close();
                }

                if (layerDetails == null)
                {
                    OnGetGroupLayerDetailsFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnableToDeserializeResponse), e.UserState));
                    return;
                }
            }
            catch (Exception ex)
            {
                OnGetGroupLayerDetailsFailed(new ExceptionEventArgs(ex, e.UserState));
                return;
            }

            List<Resource> childResources = new List<Resource>();
            if (layerDetails.SubLayers != null)
            {
                int totalSubLayerCount = layerDetails.SubLayers.Count;
                int subLayerCount = 0;                
                string parentMapServerUrl = Uri.Substring(0, Uri.IndexOf("MapServer", StringComparison.OrdinalIgnoreCase)+9);
                foreach (SubLayer subLayer in layerDetails.SubLayers)
                {       
                    // In order to determine whether a sub layer is a group layer or not, we need to make more requests
                    string subLayerUrl = string.Format("{0}/{1}", parentMapServerUrl, subLayer.ID);
                    Layer lyr = new Layer(subLayerUrl, ProxyUrl);
                    lyr.GetLayerDetailsFailed += (o, args) =>{

                        // Remove layer
                        childResources.Remove(args.UserState as Resource);

                        subLayerCount++;
                        if(subLayerCount >= totalSubLayerCount)
                            OnGetGroupLayerDetailsCompleted(new GetLayerDetailsCompletedEventArgs() { ChildResources = childResources, LayerDetails = layerDetails, UserState = e.UserState });
                    };
                    lyr.GetLayerDetailsCompleted += (o,args) =>{
                        subLayerCount++;
                        if (args.LayerDetails == null)
                        {
                            childResources.Remove(args.UserState as Resource);
                            return;
                        }

                        Resource childResource = args.UserState as Resource;
                        childResource.ResourceType = args.LayerDetails.Type == "Group Layer" ? ResourceType.GroupLayer : ResourceType.Layer;
                        childResource.DisplayName = args.LayerDetails.Name;
                        childResource.Url = string.Format("{0}/{1}", parentMapServerUrl, args.LayerDetails.ID);
                        childResource.ProxyUrl = ProxyUrl;
                        childResource.Tag = args.LayerDetails.ID;

                        if(subLayerCount >= totalSubLayerCount)
                            OnGetGroupLayerDetailsCompleted(new GetLayerDetailsCompletedEventArgs() { ChildResources = childResources, LayerDetails = layerDetails, UserState = e.UserState });
                    };

                    // Add layer before validating to preserve catalog order.  Layer will be removed if validation
                    // fails.
                    Resource child = new Resource();
                    childResources.Add(child);

                    lyr.GetLayerDetails(child);
                }
            }
            else
            {
                OnGetGroupLayerDetailsCompleted(new GetLayerDetailsCompletedEventArgs() { ChildResources = childResources, LayerDetails = layerDetails, UserState = e.UserState });
            }
        }

        protected virtual void OnGetGroupLayerDetailsFailed(ExceptionEventArgs args)
        {
            if (GetGroupLayerDetailsFailed != null)
                GetGroupLayerDetailsFailed(this, args);
        }

        protected virtual void OnGetGroupLayerDetailsCompleted(GetLayerDetailsCompletedEventArgs args)
        {
            if (GetGroupLayerDetailsCompleted != null)
                GetGroupLayerDetailsCompleted(this, args);
        }

        public event EventHandler<GetLayerDetailsCompletedEventArgs> GetGroupLayerDetailsCompleted;
        public event EventHandler<ExceptionEventArgs> GetGroupLayerDetailsFailed;
    }
}
