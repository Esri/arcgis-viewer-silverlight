/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.DataSources.SpatialDataService 
{
    internal class FeatureService
    {
        private WebClient webClient;
        public string Uri { get; set; }
        public FeatureService(string uri)
        {
            Uri = uri;
        }

        private Uri finalUrl;
        public void GetFeatureServiceDetails(object userState)
        {
            if (string.IsNullOrEmpty(Uri))
                throw new InvalidOperationException(Resources.Strings.ExceptionUriMustNotBeNull);

            UriBuilder builder = new UriBuilder(Uri);
            builder.Query = Utils.GetQueryParameters(Uri);
            finalUrl = builder.Uri;

            webClient = WebClientFactory.CreateWebClient();
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
            if (e.Cancelled)
                return;

            if (e.Error != null)
            {
                bool redownloadAttempted = WebClientFactory.RedownloadAttempted.Contains(webClient);
                if (Utils.IsMessageLimitExceededException(e.Error) && !redownloadAttempted)
                {
                    // Re-issue the request which should serve it out of cache      
                    // and helps us avoid the error which is caused by setting AllowReadStreamBuffering=false
                    // which was used to workaround the problem of SL4 and gzipped content
                    WebClientFactory.RedownloadStringAsync(webClient, finalUrl, e.UserState);                    
                }
                else
                {
                    if (redownloadAttempted) WebClientFactory.RedownloadAttempted.Remove(webClient);
                    OnGetFeatureServiceDetailsFailed(new ExceptionEventArgs(e.Error, e.UserState));
                }
                return;
            }

            if (string.IsNullOrEmpty(e.Result))
            {
                OnGetFeatureServiceDetailsFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionEmptyResponse), e.UserState));
                return;
            }

            DatabaseTableInfo featureServiceInfo = null;
            try
            {
                string json = e.Result;
                if (Utils.IsSDSCatalogResponse(json))
                {
                    // We were expecting a response consisting of table metadata, instead we got a response of the catalog
                    // this must be because we formed/guessed our URL wrong
                    OnGetFeatureServiceDetailsFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionInvalidResponseRecievedCatalogRecievedWhenExpectingTableMetadata), e.UserState));
                    return;
                }
                Exception exception = Utils.CheckJsonForException(json);
                if (exception != null)
                {
                    OnGetFeatureServiceDetailsFailed(new ExceptionEventArgs(exception, e.UserState));
                    return;
                }
                byte[] bytes = Encoding.Unicode.GetBytes(json);
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(DatabaseTableInfo));
                    featureServiceInfo = dataContractJsonSerializer.ReadObject(memoryStream) as DatabaseTableInfo;
                    memoryStream.Close();
                }

                if (featureServiceInfo == null)
                {
                    OnGetFeatureServiceDetailsFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnableToDeserializeResponse), e.UserState));
                    return;
                }
            }
            catch (Exception ex)
            {
                OnGetFeatureServiceDetailsFailed(new ExceptionEventArgs(ex, e.UserState));
                return;
            }

            List<Resource> childResources = new List<Resource>();
            if (featureServiceInfo.Fields != null)
            {
                foreach (Field field in featureServiceInfo.Fields)
                {
                    if (field.DataType == "Microsoft.SqlServer.Types.SqlGeometry" || field.DataType == "esriFieldTypeGeometry")
                        continue;
                    childResources.Add(new Resource()
                    {
                        DisplayName = field.Name,
                        ResourceType = ResourceType.Field,
                        Url = field.DataType,
                    });
                }
            }

            OnGetFeatureServiceDetailsCompleted(new GetFeatureServiceDetailsCompletedEventArgs() { ChildResources = childResources, FeatureServiceInfo = featureServiceInfo, UserState = e.UserState });
        }        

        protected void OnGetFeatureServiceDetailsCompleted(GetFeatureServiceDetailsCompletedEventArgs args)
        {
            if (GetFeatureServiceDetailsCompleted != null)
                GetFeatureServiceDetailsCompleted(this, args);
        }

        protected void OnGetFeatureServiceDetailsFailed(ExceptionEventArgs args)
        {
            if (GetFeatureServiceDetailsFailed != null)
                GetFeatureServiceDetailsFailed(this, args);
        }

        public event EventHandler<GetFeatureServiceDetailsCompletedEventArgs> GetFeatureServiceDetailsCompleted;
        public event EventHandler<ExceptionEventArgs> GetFeatureServiceDetailsFailed;
    }

    public class GetFeatureServiceDetailsCompletedEventArgs : EventArgs
    {
        public IEnumerable<Resource> ChildResources { get; set; }
        public DatabaseTableInfo FeatureServiceInfo { get; set; }
        public object UserState { get; set; }
    }   
}
