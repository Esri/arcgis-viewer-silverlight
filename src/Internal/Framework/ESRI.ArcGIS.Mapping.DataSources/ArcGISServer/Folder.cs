/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
    internal class Folder 
    {        
        private ArcGISWebClient webClient;
        public string Uri { get; set; }
        public string ProxyUrl { get; set; }
        public Filter Filter { get; set; }
        public Folder(string uri, string proxyUrl)
        {
            Uri = uri;
            ProxyUrl = proxyUrl;
        }

        Uri finalUrl;
        public async void GetServices(object userState)
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
                ArcGISWebClient.DownloadStringCompletedEventArgs result =
                    await webClient.DownloadStringTaskAsync(finalUrl, userState);
                processResult(result);
            }
            catch (Exception ex)
            {
                OnGetServicesInFolderFailed(new ExceptionEventArgs(ex, userState));
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
                OnGetServicesInFolderFailed(new ExceptionEventArgs(e.Error, e.UserState));
                return;
            }
            if (string.IsNullOrEmpty(e.Result))
            {
                OnGetServicesInFolderFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionEmptyResponse), e.UserState));
                return;
            }

            Catalog catalog = null;
            try
            {                
                string json = e.Result;
                byte[] bytes = Encoding.Unicode.GetBytes(json);
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(bytes))
                {
                    DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(Catalog));
                    catalog = dataContractJsonSerializer.ReadObject(memoryStream) as Catalog;
                    memoryStream.Close();
                }

                if (catalog == null)
                {
                    OnGetServicesInFolderFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnableToDeserializeResponse), e.UserState));
                    return;
                }

                bool retrieveChildServices = (Filter & Filter.CachedResources) == Filter.CachedResources;
                List<Resource> resources = Utility.GetResources(catalog, Filter, Uri, ProxyUrl);
                if (retrieveChildServices && resources.Count > 0)
                {
                    int childCount = 0;
                    List<Resource> childResources = new List<Resource>();
                    foreach (Resource childResource in resources)
                    {
                        IService service = ServiceFactory.CreateService(childResource.ResourceType, childResource.Url, childResource.ProxyUrl);
                        if (service != null)
                        {
                            service.ServiceDetailsDownloadFailed += (o, args) =>
                            {
                                // Remove service
                                childResources.Remove(args.UserState as Resource);

                                childCount++;
                                if (childCount >= resources.Count)
                                    OnGetServicesInFolderCompleted(new GetServicesInFolderCompletedEventArgs() { ChildResources = childResources, UserState = e.UserState });
                            };
                            service.ServiceDetailsDownloadCompleted += (o, args) =>
                            {
                                childCount++;
                                IService ser = o as IService;
                                if (ser == null || !ser.IsFilteredIn(Filter))
                                {
                                    // Remove service
                                    childResources.Remove(args.UserState as Resource);
                                }
                                if (childCount >= resources.Count)
                                    OnGetServicesInFolderCompleted(new GetServicesInFolderCompletedEventArgs() { ChildResources = childResources, UserState = e.UserState });
                            };

                            // Add the service before validation so that the catalog order is preserved.  Service will be
                            // removed if found to be invalid.
                            Resource child = new Resource()
                            {
                                DisplayName = childResource.DisplayName,
                                Url = service.Uri,
                                ProxyUrl = ProxyUrl,
                                ResourceType = service.Type,
                            };
                            childResources.Add(child);

                            service.GetServiceDetails(child);
                        }
                    }
                }
                else
                {
                    OnGetServicesInFolderCompleted(new GetServicesInFolderCompletedEventArgs() { ChildResources = resources, UserState = e.UserState });
                }
            }
            catch(Exception ex)
            {
                OnGetServicesInFolderFailed(new ExceptionEventArgs(ex, e.UserState));
            }
        }

        protected virtual void OnGetServicesInFolderCompleted(GetServicesInFolderCompletedEventArgs args)
        {
            if (GetServicesInFolderCompleted != null)
                GetServicesInFolderCompleted(this, args);
        }

        protected virtual void OnGetServicesInFolderFailed(ExceptionEventArgs args)
        {
            if (GetServicesInFolderFailed != null)
                GetServicesInFolderFailed(this, args);
        }

        public event EventHandler<GetServicesInFolderCompletedEventArgs> GetServicesInFolderCompleted;
        public event EventHandler<ExceptionEventArgs> GetServicesInFolderFailed;
    }

    public class GetServicesInFolderCompletedEventArgs : EventArgs
    {
        public IEnumerable<Resource> ChildResources { get; set; }
        public object UserState { get; set; }
    }
}
