/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.DataSources.Resources;

namespace ESRI.ArcGIS.Mapping.DataSources.SpatialDataService
{
    internal class SpatialDataServiceDataSource : ESRI.ArcGIS.Mapping.Core.DataSources.DataSourceWithResources
    {
        public override string ID
        {
            get { return Constants.SpatialDataService; }
        }
        
        public override void CancelAllCurrentRequests()
        {
            if (featureService != null)
                featureService.Cancel();
            if (server != null)
                server.Cancel();
            if (database != null)
                database.Cancel();
        }
        FeatureService featureService;
        public override void CreateLayerAsync(Resource layerResource, SpatialReference mapSpatialReference, object userState)
        {
            if (layerResource.ResourceType == ResourceType.DatabaseTable)
            {
                featureService = new FeatureService(layerResource.Url);
                featureService.GetFeatureServiceDetailsFailed += (o, e) =>
                {
                    OnCreateLayerFailed(e);
                };
                featureService.GetFeatureServiceDetailsCompleted += (o, e) =>
                {
                    if (e.FeatureServiceInfo == null)
                    {
                        OnCreateLayerFailed(new ESRI.ArcGIS.Mapping.Core.ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnableToRetrieveLayerDetails), e.UserState));
                        return;
                    }
                    GeometryType GeometryType = GeometryType.Unknown;
                    switch (e.FeatureServiceInfo.GeometryType)
                    {
                        case "esriGeometryPoint":
                            GeometryType = GeometryType.Point;
                            break;
                        case "esriGeometryMultipoint":
                            GeometryType = GeometryType.MultiPoint;
                            break;
                        case "esriGeometryPolyline":
                            GeometryType = GeometryType.Polyline;
                            break;
                        case "esriGeometryPolygon":
                            GeometryType = GeometryType.Polygon;
                            break;
                    }
                    FeatureLayer newFeatureLayer = new FeatureLayer()
                    {
                        Url = featureService.Uri,
                        ID = Guid.NewGuid().ToString("N"),
                        Mode = FeatureLayer.QueryMode.OnDemand,
                        Renderer = new ESRI.ArcGIS.Mapping.Core.Symbols.HiddenRenderer()
                    };        
                    newFeatureLayer.SetValue(MapApplication.LayerNameProperty, layerResource.DisplayName);
                    newFeatureLayer.SetValue(Core.LayerExtensions.GeometryTypeProperty, GeometryType);
                    newFeatureLayer.SetValue(Core.LayerExtensions.DisplayUrlProperty, layerResource.Url);

                    if (e.FeatureServiceInfo.Fields != null)
                    {
                        Collection<FieldInfo> fields = new Collection<FieldInfo>();
                        foreach (Field field in e.FeatureServiceInfo.Fields)
                        {
                            if (field.DataType == "Microsoft.SqlServer.Types.SqlGeometry" || field.DataType == "esriFieldTypeGeometry")
                                continue;
                            fields.Add(new FieldInfo()
                            {
                                DisplayName = field.Name,
                                FieldType = mapFieldType(field.DataType),
                                Name = field.Name,
                                VisibleInAttributeDisplay = true,
                                VisibleOnMapTip = true,
                            });
                        }
                        newFeatureLayer.SetValue(Core.LayerExtensions.FieldsProperty, fields);
                    }
                    newFeatureLayer.OutFields.Add("*"); // Get all fields at configuration time
                    OnCreateLayerCompleted(new CreateLayerCompletedEventArgs() { Layer = newFeatureLayer, UserState = e.UserState, GeometryType = GeometryType });
                };
                featureService.GetFeatureServiceDetails(userState);
            }
            else
            {
                OnCreateLayerFailed(new ESRI.ArcGIS.Mapping.Core.ExceptionEventArgs(new Exception(string.Format(Resources.Strings.ExceptionCannotCreateLayerForResourceType, layerResource.ResourceType.ToString())), userState));
            }
        }

        public FieldType mapFieldType(string fieldType)
        {
            // The system values are for v1.x
            if (fieldType == "esriFieldTypeDouble"
                || fieldType == "esriFieldTypeSingle"
                || fieldType == "System.Double"                
                || fieldType == "System.Float"
                || fieldType == "System.Single")
            {
                return FieldType.DecimalNumber;
            }          
            else if(fieldType == "esriFieldTypeInteger"
                || fieldType == "esriFieldTypeSmallInteger"
                || fieldType == "esriFieldTypeOID"
                || fieldType == "System.Int32"
                || fieldType == "System.Int16"
                || fieldType == "System.Int64")
            {
                return FieldType.Integer;
            }
            else if (fieldType == "esriFieldTypeDate"
                || fieldType == "System.DateTime")
            {
                return FieldType.DateTime;
            }
            return FieldType.Text; // For now all other fields are treated as strings
        }

        public override void GetChildResourcesAsync(Resource parentResource, Filter filter, object userState)
        {
            if ((filter & Filter.None) == Filter.None ||
                (filter & Filter.SpatiallyEnabledResources) == Filter.SpatiallyEnabledResources ||
                (filter & Filter.FeatureServices) == Filter.FeatureServices)
            {
                switch (parentResource.ResourceType)
                {
                    case ResourceType.Server:
                    case ResourceType.FeatureServer:
                        getChildResourcesForServer(parentResource, filter, userState);
                        break;
                    case ResourceType.Database:
                        getTablesInDatabase(parentResource, userState);
                        break;
                    case ResourceType.DatabaseTable:
                        getFieldsInDatabaseTable(parentResource, userState);
                        break;
                }
            }
            else
            {
                OnGetChildResourcesCompleted(new GetChildResourcesCompletedEventArgs() { ChildResources = null, UserState = userState });
            }
        }

        FeatureService featureServer;
        private void getFieldsInDatabaseTable(Resource parentResource, object userState)
        {
            featureServer = new FeatureService(parentResource.Url);
            featureServer.GetFeatureServiceDetailsFailed += (o, e) =>
            {
                OnGetChildResourcesFailed(e);
            };
            featureServer.GetFeatureServiceDetailsCompleted += (o, e) =>
            {
                OnGetChildResourcesCompleted(new GetChildResourcesCompletedEventArgs() { ChildResources = e.ChildResources, UserState = e.UserState });
            };
            featureServer.GetFeatureServiceDetails(userState);
        }

        Database database;
        private void getTablesInDatabase(Resource parentResource, object userState)
        {
            database = new Database(parentResource.Url);
            database.GetTablesInDatabaseFailed += (o, e) =>
            {
                OnGetChildResourcesFailed(e);
            };
            database.GetTablesInDatabaseCompleted += (o, e) =>
            {
                OnGetChildResourcesCompleted(new GetChildResourcesCompletedEventArgs() { ChildResources = e.ChildResources, UserState = e.UserState });
            };
            database.GetTables(userState);
        }

        Server server;
        private void getChildResourcesForServer(Resource parentResource, Filter filter, object userState)
        {
            server = new Server(parentResource.Url) { FilterForSpatialContent = (filter & Filter.SpatiallyEnabledResources) == Filter.SpatiallyEnabledResources };
            server.GetCatalogFailed += (o, e) =>
            {
                OnGetChildResourcesFailed(e);
            };
            server.GetCatalogCompleted += (o, e) =>
            {
                OnGetChildResourcesCompleted(new GetChildResourcesCompletedEventArgs() { ChildResources = e.ChildResources, UserState = e.UserState });
            };
            server.GetCatalog(userState);
        }

        public override Resource GetResource(string connectionString, string proxyUrl)
        {
            if (!connectionString.StartsWith("http://") && !connectionString.StartsWith("https://"))
                connectionString = string.Format("http://{0}", connectionString);

            // Initialize to "Server" so that "catalog" will be called with this value unless this logic detects that the
            // connection string actually refers to another type of resource like a map service, layer, etc.
            Resource res = new Resource()
            {
                Url = connectionString,
                ResourceType = ResourceType.Server,
            };

            try
            {
                Uri myUri = new Uri(connectionString);
                if (myUri.IsAbsoluteUri)
                {
                    string path = myUri.AbsolutePath;
                    string[] pathComponents = path.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                    if (pathComponents.Length >= 2)
                    {
                        // This is for SDS 10.1 which mimics ArcGIS Server
                        if (String.Compare(pathComponents[pathComponents.Length - 1], "featureserver", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            res.ResourceType = ResourceType.FeatureServer;
                            res.DisplayName = pathComponents[pathComponents.Length - 2];
                            return res;
                        }
                        else if (String.Compare(pathComponents[pathComponents.Length - 2], "databases", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            res.ResourceType = ResourceType.Database;
                            res.DisplayName = pathComponents[pathComponents.Length - 1];
                            return res;
                        }
                    }

                    if (pathComponents.Length >= 3)
                    {
                        if (String.Compare(pathComponents[pathComponents.Length - 2], "featureserver", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            // It may seem wrong to call this a GP Server when the URL is clearly indicating a child tool, but
                            // we need to do this in order to get the parent service and all child tools since we want this
                            // context in the tree control.
                            res.ResourceType = ResourceType.FeatureServer;
                            res.DisplayName = pathComponents[pathComponents.Length - 3];

                            // Store the layer "id" in the tag. This will be extracted and used when populating the tree
                            // control in order to select the proper child item.
                            res.Tag = pathComponents[pathComponents.Length - 1];

                            // Create a URL that removes the trailing layer id
                            int i = res.Url.LastIndexOf("/");
                            if (i >= 0)
                                res.Url = res.Url.Remove(i);
                            return res;
                        }
                        else if (String.Compare(pathComponents[pathComponents.Length - 3], "databases", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            // We need to do this in order to get the parent database and all child tables since we want this
                            // context in the tree control.
                            res.ResourceType = ResourceType.Database;
                            res.DisplayName = pathComponents[pathComponents.Length - 2];

                            // Store the layer "id" in the tag. This will be extracted and used when populating the tree
                            // control in order to select the proper child item.
                            res.Tag = pathComponents[pathComponents.Length - 1];

                            // Create a URL that removes the trailing layer id
                            int i = res.Url.LastIndexOf("/");
                            if (i >= 0)
                                res.Url = res.Url.Remove(i);
                            return res;
                        }
                    }
                }
            }
            catch
            {
            }

            // Parse to determine if this
            return res;
        }

        public override void GetCatalog(string connectionString, string proxyUrl, Filter filter, object userState)
        {
            Queue<string> candidateUrls = new Queue<string>();
            AddUrlCandidate(candidateUrls, getSdsEndpointUrlCandidate(connectionString, true, false));
            AddUrlCandidate(candidateUrls, getSdsEndpointUrlCandidate(connectionString, true, true));
            AddUrlCandidate(candidateUrls, getSdsEndpointUrlCandidate(connectionString, false, false));
            AddUrlCandidate(candidateUrls, getSdsEndpointUrlCandidate(connectionString, false, true));
            tryGetResourceForSdsCandidateUrl(candidateUrls, filter, userState);  
        }

        private void AddUrlCandidate(Queue<string> urls, string candidateUrl)
        {
            // Enqueue candidate URL only if it is not already in the list
            if (!urls.Contains(candidateUrl))
                urls.Enqueue(candidateUrl);
        }

        private void tryGetResourceForSdsCandidateUrl(Queue<string> candidateUrls, Filter filter, object userState)
        {
            if (candidateUrls == null || candidateUrls.Count < 1)
            {
                OnGetCatalogFailed(new ExceptionEventArgs(new Exception(Strings.ExceptionDoneTryingallCandidateURLs), userState));
                return;
            }
            string agsRestUrl = candidateUrls.Dequeue();
            server = new Server(agsRestUrl) { FilterForSpatialContent = (filter & Filter.SpatiallyEnabledResources) == Filter.SpatiallyEnabledResources };
            server.GetCatalogCompleted += (o, e) =>
            {
                OnGetCatalogCompleted(new GetCatalogCompletedEventArgs() { ChildResources = e.ChildResources, UserState = e.UserState });
            };
            server.GetCatalogFailed += (o, e) =>
            {
                tryGetResourceForSdsCandidateUrl(candidateUrls, filter, userState);
            };
            server.GetCatalog(userState);
        }

        public override bool IsResourceSelectable(Resource resource, Filter filter)
        {
            return resource.ResourceType == ResourceType.DatabaseTable;
        }

        public override bool SupportsChildResources(Resource resource, Filter filter)
        {
            bool hasChildResource = resource.ResourceType == ResourceType.Server
                || resource.ResourceType == ResourceType.Database;
            return hasChildResource;
        }

        private string getSdsEndpointUrlCandidate(string url, bool useHttp, bool appendPath)
        {
            if (url == null)
                return null;
            string[] urlComponents = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

            int index = 0;
            string prefix = urlComponents[0];
            if ("http:".Equals(prefix, StringComparison.InvariantCultureIgnoreCase) || "https:".Equals(prefix, StringComparison.InvariantCultureIgnoreCase))
            {
                url = string.Format("{0}//", prefix);
                index++;
            }
            else
            {
                url = useHttp ? "http://" : "https://";
            }

            for (int i = index; i < urlComponents.Length; i++)
                url = string.Format("{0}{1}/", url, urlComponents[i]);

            string targetUrl = url;
            string lastComponent = urlComponents[urlComponents.Length - 1];
            if ("databases".Equals(lastComponent, StringComparison.InvariantCultureIgnoreCase))
                targetUrl = url;
            else if ("sds".Equals(lastComponent, StringComparison.InvariantCultureIgnoreCase))
                targetUrl = string.Format("{0}databases", url);
            else
            {
                if(appendPath)
                    targetUrl = string.Format("{0}sds/databases", url);
            }

            return targetUrl;
        }        
    }
}
