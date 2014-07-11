/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Json;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.DataSources;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    public abstract class ServiceBase<T> : IService where T: class
    {
        private ArcGISWebClient webClient;
        public string Uri { get; set; }
        public string ProxyUrl { get; set; }
        public ResourceType Type { get; set; }
        public T ServiceInfo { get; set; }
        public abstract List<Resource> ChildResources { get; set; }

        public ServiceBase(string uri, string proxyUrl, ResourceType type) 
        {
            Uri = uri;
            ProxyUrl = proxyUrl;
            Type = type;
        }

        /// <summary>
        /// Each service will need to examine its "IsFilteredIn" method to see if, for the passed in filter here, whether
        /// or not service info must be retrieved in order to properly apply filtration. If it can be determined here that
        /// no service info is needed, then return false and the service information will not be retrieved otherwise it
        /// will be in order to properly filter.
        /// </summary>
        /// <param name="filter">The filter to be applied when loading services.</param>
        /// <returns>True if service info must be available to properly filter, False otherwise.</returns>
        public virtual bool IsServiceInfoNeededToApplyThisFilter(Filter filter)
        {
            return true;
        }

        public virtual bool IsFilteredIn(Filter filter)
        {
            return false;
        }


        private Uri finalUrl;
        public async void GetServiceDetails(object userState)
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
                ArcGISWebClient.DownloadStringCompletedEventArgs result = await webClient.DownloadStringTaskAsync(finalUrl, userState);
                processResult(result);
            }
            catch (Exception ex)
            {
                OnServiceDetailsDownloadFailed(new ExceptionEventArgs(ex, userState));
            }
        }

        public virtual void Cancel()
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
                OnServiceDetailsDownloadFailed(new ExceptionEventArgs(e.Error, e.UserState));
                return;
            }

            if (string.IsNullOrEmpty(e.Result))
            {
                OnServiceDetailsDownloadFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionEmptyResponse), e.UserState));
                return;
            }

            try
            {
                string json = e.Result;

                // Check if there is a JSON error
                JsonObject o = (JsonObject)JsonObject.Parse(json);
                if (o.ContainsKey("error"))
                {
                    string errorMessage = "";
                    int statusCode = -1;

                    o = (JsonObject)o["error"];
                    
                    // Extract the error message
                    if (o.ContainsKey("message"))
                        errorMessage = o["message"];

                    // Extract the HTTP status code
                    if (o.ContainsKey("code"))
                        statusCode = o["code"];

                    // Fire the failed event
                    OnServiceDetailsDownloadFailed(new ExceptionEventArgs(new Exception(errorMessage), e.UserState, statusCode));
                    return;
                }
                byte[] bytes = Encoding.Unicode.GetBytes(json);
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(bytes))
                {
                    DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T));
                    ServiceInfo = dataContractJsonSerializer.ReadObject(memoryStream) as T;
                    memoryStream.Close();
                }

                if (ServiceInfo == null)
                {
                    OnServiceDetailsDownloadFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnableToDeserializeResponse), e.UserState));
                    return;
                }
            }
            catch (Exception ex)
            {
                OnServiceDetailsDownloadFailed(new ExceptionEventArgs(ex, e.UserState));
                return;
            }

            OnServiceDetailsDownloadCompleted(new ServiceDetailsDownloadCompletedEventArgs() { UserState = e.UserState });      
        }

        protected virtual void OnServiceDetailsDownloadCompleted(ServiceDetailsDownloadCompletedEventArgs args)
        {
            if (ServiceDetailsDownloadCompleted != null)
                ServiceDetailsDownloadCompleted(this, args);
        }
        
        protected virtual void OnServiceDetailsDownloadFailed(ExceptionEventArgs args)
        {
            if (ServiceDetailsDownloadFailed != null)
                ServiceDetailsDownloadFailed(this, args);
        }

        public event EventHandler<ServiceDetailsDownloadCompletedEventArgs> ServiceDetailsDownloadCompleted;
        public event EventHandler<ExceptionEventArgs> ServiceDetailsDownloadFailed;
    }
}
