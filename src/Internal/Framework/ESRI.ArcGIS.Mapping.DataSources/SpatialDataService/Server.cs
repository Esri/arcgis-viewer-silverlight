/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using System.Linq;

namespace ESRI.ArcGIS.Mapping.DataSources.SpatialDataService 
{
    internal class Server
    {
        private WebClient webClient;
        public string Uri { get; set; }
        public bool FilterForSpatialContent { get; set; }
        public Server(string uri)
        {
            Uri = uri;
        }

        Uri finalUrl;
        public void GetCatalog(object userState)
        {
            if (string.IsNullOrEmpty(Uri))
                throw new InvalidOperationException(Resources.Strings.ExceptionUriMustNotBeNull);            

            // ensure that catalog requests always end with /databases
            if (!Uri.EndsWith("/databases", StringComparison.OrdinalIgnoreCase)
                && !Uri.EndsWith("/databases/", StringComparison.OrdinalIgnoreCase))
            {
                if (!Uri.EndsWith("/", StringComparison.Ordinal))
                    Uri += "/";
                Uri += "databases";
            }
            UriBuilder builder = new UriBuilder(Uri);
            builder.Query = Utils.GetQueryParameters(Uri);
            finalUrl = builder.Uri;

            webClient = WebClientFactory.CreateWebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(this.DownloadStringCompleted);
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
                    OnGetCatalogFailed(new ExceptionEventArgs(e.Error, e.UserState));
                }
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
            DatabaseCatalog catalog = null;
            try
            {
                byte[] bytes = Encoding.Unicode.GetBytes(json);
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(bytes))
                {
                    DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(DatabaseCatalog));
                    catalog = dataContractJsonSerializer.ReadObject(memoryStream) as DatabaseCatalog;
                    memoryStream.Close();
                }
            }
            catch (Exception ex)
            {
                OnGetCatalogFailed(new ExceptionEventArgs(ex, e.UserState));
                return;
            }

            if (catalog == null)
            {
                OnGetCatalogFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnableToDeserializeCatalog), e.UserState));
                return;
            }

            List<Resource> childResources = new List<Resource>();            
            int totalDatabasesCount = catalog.Databases == null ? 0 : catalog.Databases.Count;
            if (catalog.Databases != null)
            {                
                int databaseCount = 0;
                foreach (string databaseName in catalog.Databases)
                {                    
                    Resource databaseResource = new Resource()
                    {
                        DisplayName = databaseName,
                        Url = string.Format("{0}/{1}", Uri, databaseName),
                        ResourceType = ResourceType.Database,
                    };
                    if (!FilterForSpatialContent)
                    {
                        childResources.Add(databaseResource);
                    }
                    else
                    {
                        Database db = new Database(databaseResource.Url) { FilterForSpatialContent = true };
                        db.GetTablesInDatabaseFailed += (o, args) => {
                            // remove the database
                            childResources.Remove(args.UserState as Resource);

                            databaseCount++;                            
                            if (databaseCount >= totalDatabasesCount)
                            {
                                // all done, raise the event
                                OnGetCatalogFailed(args);
                            }
                        };
                        db.GetTablesInDatabaseCompleted += (o, args) =>
                        {
                            databaseCount++;
                            bool hasAtleastOneSpatialTable = args.ChildResources.Count() > 0;
                            if (!hasAtleastOneSpatialTable)
                            {
                                // remove the database
                                childResources.Remove(args.UserState as Resource);
                            }
                            if (databaseCount >= totalDatabasesCount)
                            {
                                // all done, raise the event
                                OnGetCatalogRequestCompleted(new GetCatalogCompletedEventArgs() { ChildResources = childResources, UserState = e.UserState });
                            }
                        };

                        // Add database prior to validation to preserve catalog order.  Database will be removed
                        // if validation fails.
                        Resource child = new Resource()
                        {
                            DisplayName = databaseName,
                            Url = string.Format("{0}/{1}", Uri, databaseName),
                            ResourceType = ResourceType.Database,
                        };
                        childResources.Add(child);

                        db.GetTables(child);
                    }
                }                
            }

            if(!FilterForSpatialContent || totalDatabasesCount == 0)
            {
                OnGetCatalogRequestCompleted(new GetCatalogCompletedEventArgs() { ChildResources = childResources, UserState = e.UserState });
            }
        }

        protected void OnGetCatalogFailed(ExceptionEventArgs args)
        {
            if (GetCatalogFailed != null)
                GetCatalogFailed(this, args);
        }

        protected void OnGetCatalogRequestCompleted(GetCatalogCompletedEventArgs args)
        {
            if (GetCatalogCompleted != null)
                GetCatalogCompleted(this, args);
        }       

        public event EventHandler<ExceptionEventArgs> GetCatalogFailed;
        public event EventHandler<GetCatalogCompletedEventArgs> GetCatalogCompleted;
    }    
}
