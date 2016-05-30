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
using System.Linq;
using ESRI.ArcGIS.Client;
using System.Threading.Tasks;
using ESRI.ArcGIS.Mapping.DataSources.Resources;
using System.IO;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer 
{
    internal class Server
    {        
        private ArcGISWebClient webClient;
        public string Uri { get; set; }
        public Filter Filter { get; set; }
        public Server(string uri, string proxyUrl)
        {
            Uri = uri;
            this.ProxyUrl = proxyUrl;
        }

        string ProxyUrl;
        Uri finalUrl;
        public async void GetCatalog(object userState)
        {
            if (string.IsNullOrEmpty(Uri))
                throw new InvalidOperationException(Resources.Strings.ExceptionUriMustNotBeNull);

            // ensure that catalog requests always end with /services
            if (!Uri.EndsWith("/services", StringComparison.OrdinalIgnoreCase)
                && !Uri.EndsWith("/services/", StringComparison.OrdinalIgnoreCase))
            {
                if (!Uri.EndsWith("/", StringComparison.Ordinal))
                    Uri += "/";
                Uri += "services";
            }

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
                OnGetCatalogFailed(new ExceptionEventArgs(ex, userState));
            }
        }

        /// <summary>
        /// Retrieve instance-level meatadata for the server
        /// </summary>
        public async Task<ServerInfo> GetServerInfo()
        {
            if (string.IsNullOrEmpty(Uri))
                throw new InvalidOperationException(Resources.Strings.ExceptionUriMustNotBeNull);

            string serverInfoUrl = Uri;

            // Try to construct the server info URL

            // First, find "/services" in the URL and extract the part leading up to it
            if (serverInfoUrl.EndsWith("/services", StringComparison.OrdinalIgnoreCase)
            || serverInfoUrl.EndsWith("/services/", StringComparison.OrdinalIgnoreCase))
                serverInfoUrl = serverInfoUrl.Substring(0, serverInfoUrl.Length - 9);
            else if (serverInfoUrl.IndexOf("/services/", StringComparison.OrdinalIgnoreCase) > -1)
                serverInfoUrl = serverInfoUrl.Substring(0, serverInfoUrl.IndexOf("/services/", StringComparison.OrdinalIgnoreCase));

            // Second, append "/info" to the URL
            if (!serverInfoUrl.EndsWith("/info", StringComparison.OrdinalIgnoreCase)
                && !serverInfoUrl.EndsWith("/info/", StringComparison.OrdinalIgnoreCase))
            {
                if (!serverInfoUrl.EndsWith("/", StringComparison.Ordinal))
                    serverInfoUrl += "/";
                serverInfoUrl += "info";
            }

            // Add the query parameters to the URL and convert it to a URI
            UriBuilder builder = new UriBuilder(serverInfoUrl);
            builder.Query = Utils.GetQueryParameters(serverInfoUrl);
            finalUrl = builder.Uri;

            if (webClient == null)
                webClient = new ArcGISWebClient();
            webClient.ProxyUrl = ProxyUrl;

            string json = null;
            try
            {
                // Download the server metadata
                json = await webClient.DownloadStringTaskAsync(finalUrl);
            }
            catch (Exception exception)
            {
                throw exception;
            }

            // Check the result for cancellation or error

            if (string.IsNullOrEmpty(json))
                throw new Exception(Strings.ExceptionEmptyResponse);

            Exception ex = Utils.CheckJsonForException(json);

            if (ex != null)
                throw ex;

            // Deserialize the result
            ServerInfo info = null;
            try
            {
                byte[] bytes = Encoding.Unicode.GetBytes(json);
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    DataContractJsonSerializer dataContractJsonSerializer = 
                        new DataContractJsonSerializer(typeof(ServerInfo));
                    info = dataContractJsonSerializer.ReadObject(memoryStream) as ServerInfo;
                    memoryStream.Close();
                }
            }
            catch (Exception exception)
            {
                throw new Exception(Strings.ExceptionCouldNotReadServerInfo, exception);
            }

            // Apply the URL used to retrieve the metadata to the ServerInfo instance
            if (info != null)
            {
                info.Url = finalUrl.ToString();
                info.BaseUrl = serverInfoUrl;
                if (!string.IsNullOrEmpty(ProxyUrl))
                    info.ProxyUrl = ProxyUrl;
            }

            // Return the ServerInfo
            return info;
        }        

        public void Cancel()
        {            
            if (webClient != null && webClient.IsBusy)
                webClient.CancelAsync();                  
        }

        private object lockObj = new object();
        private void processResult(ArcGISWebClient.DownloadStringCompletedEventArgs e) 
        {
            if (e.Cancelled)
                return;

            if (e.Error != null)
            {
                OnGetCatalogFailed(new ExceptionEventArgs(e.Error, e.UserState));
                return;
            }
            if (string.IsNullOrEmpty(e.Result))
            {
                OnGetCatalogFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionEmptyResponse), e.UserState));                                        
                return;
            }

            string json = e.Result;
            Exception exception = Utils.CheckJsonForException(json);
            if (exception != null)
            {
                OnGetCatalogFailed(new ExceptionEventArgs(exception, e.UserState));
                return;
            }
            Catalog catalog = null;
            try
            {
                byte[] bytes = Encoding.Unicode.GetBytes(json);
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(bytes))
                {
                    DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(Catalog));
                    catalog = dataContractJsonSerializer.ReadObject(memoryStream) as Catalog;
                    memoryStream.Close();
                }
            }
            catch(Exception ex)
            {
                OnGetCatalogFailed(new ExceptionEventArgs(ex, e.UserState));   
                return; 
            }

            if (catalog == null) 
            { 
                OnGetCatalogFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnableToDeserializeCatalog), e.UserState));   
                return; 
            }

            if (catalog.Folders == null && catalog.Services == null)
            {
                OnGetCatalogFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionNoServicesFoundOnServer), e.UserState));
                return; 
            }

            List<Resource> childResources = new List<Resource>();
            List<Resource> resources = Utility.GetResources(catalog, Filter, Uri, ProxyUrl, lockObj);
            int childCount = 0;
            int totalChildCount = resources != null ? resources.Count : 0;

            // If the catalog has any folders, add them to the returned list first
            if (catalog.Folders != null)
            {
                totalChildCount += catalog.Folders.Count;
                foreach (string folderName in catalog.Folders)
                {
                    Folder folder = new Folder(string.Format("{0}/{1}", Uri, folderName), ProxyUrl)
                    {
                        Filter = Filter,
                    };
                    folder.GetServicesInFolderFailed += (o, args) =>
                    {
                        // Remove the folder
                        lock (lockObj)
                        {
                            childResources.Remove(args.UserState as Resource);
                        }

                        childCount++;
                        if (childCount >= totalChildCount)
                            OnGetCatalogCompleted(new GetCatalogCompletedEventArgs() { ChildResources = childResources, UserState = e.UserState });
                    };
                    folder.GetServicesInFolderCompleted += (o, args) =>
                    {
                        int nestedChildTotalCount = args.ChildResources.Count();
                        if (nestedChildTotalCount == 0)
                        {
                            // Remove the folder
                            lock (lockObj)
                            {
                                childResources.Remove(args.UserState as Resource);
                            }
                        }

                        childCount++;
                        if (childCount >= totalChildCount)
                            OnGetCatalogCompleted(new GetCatalogCompletedEventArgs() { ChildResources = childResources, UserState = e.UserState });
                    };

                    // Add the folder before validation so that the catalog order is preserved.  Folder will be
                    // removed if found to be invalid.
                    Resource folderResource = new Resource()
                    {
                        DisplayName = folderName,
                        Url = string.Format("{0}/{1}", Uri, folderName),
                        ProxyUrl = ProxyUrl,
                        ResourceType = ResourceType.Folder,
                    };
                    lock (lockObj)
                    {
                        childResources.Add(folderResource);
                    }
                    folder.GetServices(folderResource);
                }
            }

            // Remove any undesired services due to filtering
            foreach (Resource childRes in resources)
            {
                IService childService = ServiceFactory.CreateService(childRes.ResourceType, childRes.Url, childRes.ProxyUrl);
                if (childService != null)
                {
                    // Determine if filtering requires detailed information about the server and only make async call to
                    // obtain detailed info if true.
                    if (childService.IsServiceInfoNeededToApplyThisFilter(Filter))
                    {
                        childService.ServiceDetailsDownloadFailed += (o, args) =>
                        {
                            // Remove resource
                            lock (lockObj)
                            {
                                childResources.Remove(args.UserState as Resource);
                            }

                            childCount++;
                            if (childCount >= totalChildCount)
                                OnGetCatalogCompleted(new GetCatalogCompletedEventArgs() { ChildResources = childResources, UserState = e.UserState });
                        };
                        childService.ServiceDetailsDownloadCompleted += (o, args) =>
                        {
                            IService service = o as IService;
                            if (service == null || !service.IsFilteredIn(Filter)) // check if service is filtered
                            {
                                // Remove resource
                                lock (lockObj)
                                {
                                    childResources.Remove(args.UserState as Resource);
                                }
                            }
                            childCount++;
                            if (childCount >= totalChildCount)
                                OnGetCatalogCompleted(new GetCatalogCompletedEventArgs() { ChildResources = childResources, UserState = e.UserState });
                        };

                        // Add the service before validation so that the catalog order is preserved.  Service will be
                        // removed if found to be invalid.
                        Resource childResource = new Resource()
                        {
                            DisplayName = childRes.DisplayName,
                            Url = childService.Uri,
                            ProxyUrl = ProxyUrl,
                            ResourceType = childService.Type
                        };
                        lock (lockObj)
                        {
                            childResources.Add(childResource);
                        }
                        childService.GetServiceDetails(childResource);
                    }
                    else
                    {
                        // Apply filtering using basic information, not detailed information
                        if (childService != null && childService.IsFilteredIn(Filter))
                        {
                            lock (lockObj)
                            {
                                childResources.Add(new Resource()
                                {
                                    DisplayName = childRes.DisplayName,
                                    Url = childService.Uri,
                                    ProxyUrl = ProxyUrl,
                                    ResourceType = childService.Type
                                });
                            }
                        }
                        ++childCount;
                    }
                }
            }

            if (childCount >= totalChildCount)
                OnGetCatalogCompleted(new GetCatalogCompletedEventArgs() { ChildResources = childResources, UserState = e.UserState });
        }

        protected virtual void OnGetCatalogCompleted(GetCatalogCompletedEventArgs args)
        {
            if (GetCatalogCompleted != null)
                GetCatalogCompleted(this, args);
        }

        protected virtual void OnGetCatalogFailed(ExceptionEventArgs args)
        {
            if (GetCatalogFailed != null)
                GetCatalogFailed(this, args);
        }

        public event EventHandler<ExceptionEventArgs> GetCatalogFailed;
        public event EventHandler<GetCatalogCompletedEventArgs> GetCatalogCompleted;
    }    
}
