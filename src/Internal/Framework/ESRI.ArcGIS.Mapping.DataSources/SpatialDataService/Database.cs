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
using ESRI.ArcGIS.Mapping.Core.DataSources;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.DataSources.SpatialDataService 
{
    internal class Database
    {        
        private WebClient webClient;
        public string Uri { get; set; }
        public bool FilterForSpatialContent { get; set; }
        public Database(string uri)
        {
            Uri = uri;
        }

        Uri finalUrl;
        public void GetTables(object userState)
        {
            if (string.IsNullOrEmpty(Uri))
                throw new InvalidOperationException("Uri must not be null");

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
                    OnGetTablesInDatabaseFailed(new ExceptionEventArgs(e.Error, e.UserState));
                }
                return;
            }
            if (string.IsNullOrEmpty(e.Result))
            {
                OnGetTablesInDatabaseFailed(new ExceptionEventArgs(new Exception("Empty response"), e.UserState));
                return;
            }

            DatabaseTables databaseTables = null;
            try
            {
                string json = e.Result;
                if (Utils.IsSDSCatalogResponse(json))
                {
                    // We were expecting a response consisting of tables, instead we got a response of the catalog
                    // this must be because we formed/guessed our URL wrong
                    OnGetTablesInDatabaseFailed(new ExceptionEventArgs(new Exception("Invalid response recieved. Catalog response recieved when expecting database tables"), e.UserState));
                    return;
                }
                Exception exception = Utils.CheckJsonForException(json);
                if (exception != null)
                {
                    OnGetTablesInDatabaseFailed(new ExceptionEventArgs(exception, e.UserState));
                    return;
                }
                byte[] bytes = Encoding.Unicode.GetBytes(json);
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(DatabaseTables));
                    databaseTables = dataContractJsonSerializer.ReadObject(memoryStream) as DatabaseTables;
                    memoryStream.Close();
                }

                if (databaseTables == null)
                {
                    OnGetTablesInDatabaseFailed(new ExceptionEventArgs(new Exception("Unable to deserialize response"), e.UserState));
                    return;
                }

                List<Resource> childResources = new List<Resource>();
                int totalTableCount = databaseTables.Tables != null ? databaseTables.Tables.Count : 0;                
                if (databaseTables.Tables != null)
                {
                    int tableCount = 0;
                    foreach (string table in databaseTables.Tables)
                    {                        
                        Resource databaseTable = new Resource()
                        {
                            ResourceType = ResourceType.DatabaseTable,
                            DisplayName = table,
                            Url = string.Format("{0}/{1}", Uri, table),
                        };
                        if (FilterForSpatialContent)
                        {
                            FeatureService featureService = new FeatureService(databaseTable.Url);
                            featureService.GetFeatureServiceDetailsFailed += (o, args) => {
                                // Remove the table
                                childResources.Remove(args.UserState as Resource);

                                tableCount++;
                                if (tableCount >= totalTableCount)
                                {
                                    // all done raise the event
                                    OnGetTablesInDatabaseFailed(args);
                                }
                            };
                            featureService.GetFeatureServiceDetailsCompleted += (o, args) =>
                            {
                                tableCount++;
                                if (args.FeatureServiceInfo == null
                                    || !args.FeatureServiceInfo.DoesTableHasGeometryColumn())
                                {       
                                    // Remove the table
                                    childResources.Remove(args.UserState as Resource);
                                }

                                if (tableCount >= totalTableCount)
                                {
                                    // all done raise the event
                                    OnGetTablesInDatabaseCompleted(new GetTablesInDatabaseCompletedEventArgs() { ChildResources = childResources, UserState = e.UserState });
                                }
                            };

                            // Add table before validation to preserve catalog order.  Table will be removed if
                            // validation fails.
                            Resource child = new Resource()
                            {
                                ResourceType = ResourceType.DatabaseTable,
                                DisplayName = databaseTable.DisplayName,
                                Url = databaseTable.Url,
                            };
                            childResources.Add(child);

                            featureService.GetFeatureServiceDetails(child);
                        }
                        else
                        {
                            childResources.Add(databaseTable);
                        }
                    }
                }

                if (!FilterForSpatialContent || totalTableCount == 0)
                {
                    OnGetTablesInDatabaseCompleted(new GetTablesInDatabaseCompletedEventArgs() { ChildResources = childResources, UserState = e.UserState });
                }
            }
            catch (Exception ex)
            {
                OnGetTablesInDatabaseFailed(new ExceptionEventArgs(ex, e.UserState));
            }
        }
       

        protected void OnGetTablesInDatabaseCompleted(GetTablesInDatabaseCompletedEventArgs args)
        {
            if (GetTablesInDatabaseCompleted != null)
                GetTablesInDatabaseCompleted(this, args);
        }

        protected void OnGetTablesInDatabaseFailed(ExceptionEventArgs args)
        {
            if (GetTablesInDatabaseFailed != null)
                GetTablesInDatabaseFailed(this, args);
        }

        public event EventHandler<GetTablesInDatabaseCompletedEventArgs> GetTablesInDatabaseCompleted;
        public event EventHandler<ExceptionEventArgs> GetTablesInDatabaseFailed;
    }

    public class GetTablesInDatabaseCompletedEventArgs : EventArgs
    {
        public IEnumerable<Resource> ChildResources { get; set; }
        public object UserState { get; set; }
    }    
}
