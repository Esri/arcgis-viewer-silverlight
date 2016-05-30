/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.DataSources;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer 
{
    internal class Layer 
    {
        private ArcGISWebClient webClient;
        public string Uri { get; set; }
        public string ProxyUrl { get; set; }
        public Layer(string uri, string proxyUrl) 
        {
            Uri = uri;
            ProxyUrl = proxyUrl;
        }

        Uri finalUrl;

        public async void GetLayerDetails(object userState)
        {
            if (string.IsNullOrEmpty(Uri))
                throw new InvalidOperationException(Resources.Strings.ExceptionUriMustNotBeNull);

            UriBuilder builder = new UriBuilder(Uri);
            builder.Query = Utils.GetQueryParameters(Uri);
            finalUrl = builder.Uri;

            if (webClient == null)
                webClient = new ArcGISWebClient();
            webClient.ProxyUrl = ProxyUrl;

            try
            {
                ArcGISWebClient.OpenReadCompletedEventArgs result =
                    await webClient.OpenReadTaskAsync(finalUrl, userState);
                processResult(result);
            }
            catch (Exception ex)
            {
                OnGetLayerDetailsFailed(new ExceptionEventArgs(ex, userState));
            }
        }

        private void processResult(ArcGISWebClient.OpenReadCompletedEventArgs e)
        {
            if (e == null || e.Cancelled)
                return;

            if (e.Error != null)
            {
                OnGetLayerDetailsFailed(new ExceptionEventArgs(e.Error, e.UserState));
                return;
            }

            if (e.Result == null)
            {
                OnGetLayerDetailsFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionEmptyResponse), e.UserState));
                return;
            }

            string json = (new StreamReader(e.Result)).ReadToEnd();
            if (string.IsNullOrEmpty(json))
            {
                OnGetLayerDetailsFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionEmptyResponse), e.UserState));
                return;
            }

            LayerDetails layerDetails;
            try
            {
                Exception exception = Utils.CheckJsonForException(json);
                if (exception != null)
                {
                    OnGetLayerDetailsFailed(new ExceptionEventArgs(exception, e.UserState));
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
                    OnGetLayerDetailsFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnableToDeserializeResponse), e.UserState));
                    return;
                }
            }
            catch (Exception ex)
            {
                OnGetLayerDetailsFailed(new ExceptionEventArgs(ex, e.UserState));
                return;
            }

            List<Resource> childResources = new List<Resource>();
            if (layerDetails.Fields != null)
            {
                foreach (ESRI.ArcGIS.Mapping.Core.Field field in layerDetails.Fields)
                {
                    childResources.Add(new Resource()
                    {
                        ResourceType = ResourceType.Field,
                        DisplayName = field.Alias,
                        ProxyUrl = ProxyUrl,
                        Url = field.Type,
                    });
                }
            }

            OnGetLayerDetailsCompleted(new GetLayerDetailsCompletedEventArgs() { ChildResources = childResources, LayerDetails = layerDetails, UserState = e.UserState });
        }

        public void Cancel()
        {           
            if (webClient != null && webClient.IsBusy)
                webClient.CancelAsync();
        }

        protected virtual void OnGetLayerDetailsFailed(ExceptionEventArgs args)
        {
            if (GetLayerDetailsFailed != null)
                GetLayerDetailsFailed(this, args);
        }

        protected virtual void OnGetLayerDetailsCompleted(GetLayerDetailsCompletedEventArgs args)
        {
            if (GetLayerDetailsCompleted != null)
                GetLayerDetailsCompleted(this, args);
        }

        public event EventHandler<GetLayerDetailsCompletedEventArgs> GetLayerDetailsCompleted;
        public event EventHandler<ExceptionEventArgs> GetLayerDetailsFailed;
    }

    public class GetLayerDetailsCompletedEventArgs : EventArgs
    {
        public IEnumerable<Resource> ChildResources { get; set; }
        public LayerDetails LayerDetails { get; set; }
        public object UserState { get; set; }
    }
}
