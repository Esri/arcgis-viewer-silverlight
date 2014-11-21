/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Mapping.Core.DataSources;
using System;
using System.Linq;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client.Extensibility;
using System.ComponentModel;
using System.Threading.Tasks;
using ESRI.ArcGIS.Mapping.DataSources.Resources;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ArcGISServerDataSource : ESRI.ArcGIS.Mapping.Core.DataSources.BaseMapDataSourceWithResources, IBaseMapDataSource
    {
        public ArcGISServerDataSource()
        {
            
        }
        public override string ID
        {
            get
            {
                return Constants.ArcGISServer;
            }
        }

        public string ProxyUrl { get; set; }

        public override Client.TiledMapServiceLayer CreateBaseMapLayer(BaseMapInfo baseMapInfo)
        {
            ArcGISTiledMapServiceLayer agsLayer = new ArcGISTiledMapServiceLayer() { Url = baseMapInfo.Url };
            
            // Apply proxy if necessary
            if (baseMapInfo.UseProxy)
            {
                agsLayer.ProxyURL = ProxyUrl;
                LayerExtensions.SetUsesProxy(agsLayer, true);
            }

            return agsLayer;
        }

        public override bool CanSwitchBaseMapLayer(Client.Layer layer)
        {
            return layer is ArcGISTiledMapServiceLayer && ((ArcGISTiledMapServiceLayer)layer).ProxyURL == ProxyUrl;
        }

        public override void SwitchBaseMapLayer(Client.Layer oldLayer, BaseMapInfo newBaseMapInfo)
        {
            Client.ArcGISTiledMapServiceLayer tiledLayer = oldLayer as Client.ArcGISTiledMapServiceLayer;
            if (tiledLayer != null)
            {
                // Apply proxy if one is in use or clear it if not
                if (newBaseMapInfo.UseProxy)
                {
                    tiledLayer.ProxyURL = ProxyUrl;
                    LayerExtensions.SetUsesProxy(tiledLayer, true);
                }
                else
                {
                    tiledLayer.ProxyURL = null;
                    LayerExtensions.SetUsesProxy(tiledLayer, false);
                }

                tiledLayer.Url = newBaseMapInfo.Url;

                tiledLayer.SetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty, newBaseMapInfo.DisplayName);
            }
        }

        public override void GetMapServiceMetaDataAsync(string mapServiceUrl, object userState)
        {
            if (string.IsNullOrEmpty(mapServiceUrl))
                throw new ArgumentNullException("mapServiceUrl");
            BaseMapServiceMetaDataLoader metaDataLoader = new BaseMapServiceMetaDataLoader(mapServiceUrl) 
                { ProxyUrl = this.ProxyUrl };
            metaDataLoader.GetBaseMapServiceMetaDataCompleted += (o, e) => {
                OnGetMapServiceMetaDataCompleted(e);
            };
            metaDataLoader.GetBaseMapServiceMetaDataFailed += (o, e) => {
                OnGetMapServiceMetaDataFailed(e);
            };
            metaDataLoader.GetBaseMapServiceMetaData(userState);
        }

        public override bool IsResourceSelectable(Resource resource, Filter filter)
        {
            if (resource != null)
            {
                switch (resource.ResourceType)
                {
                    case ResourceType.Layer:
                    case ResourceType.EditableLayer:
                    case ResourceType.MapServer:
                    case ResourceType.ImageServer:
                    case ResourceType.FeatureServer:
                    case ResourceType.GPTool:
                        return true;
                }
            }
            return false;
        }

        public override bool SupportsChildResources(Resource resource, Filter filter)
        {
            bool hasChildResource = resource.ResourceType == ResourceType.Server
                || resource.ResourceType == ResourceType.Folder
                || resource.ResourceType == ResourceType.MapServer
                || resource.ResourceType == ResourceType.FeatureServer
                || resource.ResourceType == ResourceType.GroupLayer
                || resource.ResourceType == ResourceType.GPServer;
            return hasChildResource;
        }

        public override void CancelAllCurrentRequests()
        {
            if (server != null)
                server.Cancel();
            if (folder != null)
                folder.Cancel();
            if (_service != null)
                _service.Cancel();
            if (layer != null)
                layer.Cancel();
            if (groupLayer != null)
                groupLayer.Cancel();
            if (featureLayer != null)
                featureLayer.Cancel();
        }
        string getCleanProxyUrl(string proxy)
        {
            if (!string.IsNullOrEmpty(proxy))
            {
                if (proxy.EndsWith("?", StringComparison.Ordinal))
                    return proxy.Substring(0, proxy.Length - 1);
            }
            return null;
        }
        Layer featureLayer;
        public override void CreateLayerAsync(Resource resource, SpatialReference mapSpatialReference, object userState)
        {
            if(resource == null)
                return;

            ESRI.ArcGIS.Client.Layer layer = null;

            switch (resource.ResourceType)
            {
                case ResourceType.MapServer:
                case ResourceType.FeatureServer:
                    {
                        string url = resource.Url;
                        if (resource.ResourceType == ResourceType.FeatureServer)
                        {
                            int i = url.LastIndexOf(string.Format("/{0}", ResourceType.FeatureServer.ToString()));
                            if (i >= 0)
                            {
                                url = url.Remove(i);
                                url = string.Format("{0}/{1}", url, ResourceType.MapServer.ToString());
                            }
                        }

                        _service = ServiceFactory.CreateService(ResourceType.MapServer, url, resource.ProxyUrl);
                        if (_service != null)
                        {
                            _service.ServiceDetailsDownloadFailed += (o, e) =>
                            {
                                OnCreateLayerFailed(e);
                            };
                            _service.ServiceDetailsDownloadCompleted += (o, e) =>
                            {
                                MapService mapService = o as MapService;
                                if (mapService.ServiceInfo == null)
                                {
                                    OnCreateLayerFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnableToRetrieveMapServiceDetails), e.UserState));
                                    return;
                                }

                                if (mapService.ServiceInfo.TileInfo != null && mapService.ServiceInfo.SingleFusedMapCache)
                                {
                                    #region Create tiled layer
                                    if (mapSpatialReference != null && !mapSpatialReference.Equals(mapService.ServiceInfo.SpatialReference))
                                    {
                                        OnCreateLayerFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionCachedMapServiceSpatialReferenceDoesNotMatch), e.UserState));
                                        return;
                                    }
                                    layer = new ArcGISTiledMapServiceLayer()
                                    {
                                        Url = url,
                                        ProxyURL = getCleanProxyUrl(mapService.ProxyUrl),
                                    };

                                    if (mapService.ServiceInfo.TileInfo.LODs != null)
                                    {
                                        double maxResolution = 0;
                                        double minResolution = 0;
                                        foreach (LODInfo lod in mapService.ServiceInfo.TileInfo.LODs)
                                        {
                                            if (lod.Resolution > maxResolution)
                                                maxResolution = lod.Resolution;
                                            if (minResolution <= 0 || minResolution > lod.Resolution)
                                                minResolution = lod.Resolution;
                                        }
                                        if (maxResolution > 0 )
                                            layer.MaximumResolution = maxResolution * 4;
                                        if (minResolution > 0)
                                            layer.MinimumResolution = minResolution / 4;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region create dynamic layer
                                    layer = new ArcGISDynamicMapServiceLayer()
                                    {
                                        Url = url,
                                        ProxyURL = getCleanProxyUrl(mapService.ProxyUrl),
                                    };
                                    #endregion
                                }

                                //Set layer's attached properties
                                if (layer != null)
                                {
                                    layer.SetValue(MapApplication.LayerNameProperty, resource.DisplayName);
                                    layer.SetValue(Core.LayerExtensions.DisplayUrlProperty, url);
                                    if (!string.IsNullOrEmpty(resource.ProxyUrl))
                                        Core.LayerExtensions.SetUsesProxy(layer, true);
                                    layer.ID = Guid.NewGuid().ToString("N");
                                }

                                OnCreateLayerCompleted(new CreateLayerCompletedEventArgs() { Layer = layer, UserState = e.UserState });

                            };

                            _service.GetServiceDetails(null);
                        }

                    } break;

                case ResourceType.ImageServer:
                    {
                        layer = new ArcGISImageServiceLayer()
                        {
                            Url = resource.Url,
                            ProxyURL = getCleanProxyUrl(resource.ProxyUrl),
                            ID = Guid.NewGuid().ToString("N"),
                        };

                        //Set layer's attached properties
                        layer.SetValue(MapApplication.LayerNameProperty, resource.DisplayName);
                        layer.SetValue(Core.LayerExtensions.DisplayUrlProperty, resource.Url);
                        if (!string.IsNullOrEmpty(resource.ProxyUrl))
                            Core.LayerExtensions.SetUsesProxy(layer, true);

                        // Need to declare handler separate from lambda expression to avoid erroneous
                        // "use of unassigned variable" build error
                        EventHandler<EventArgs> initialized = null;

                        // Need to populate the layer's metadata to handle initialization of image format
                        // and band IDs
                        initialized = (o, e) =>
                        {
                            layer.Initialized -= initialized;
                            ArcGISImageServiceLayer imageLayer = (ArcGISImageServiceLayer)layer;
                            if (imageLayer.Version < 10)
                            {
                                // Pre v10, band IDs must be specified explicitly.  But no more than 
                                // 3 can be used.  Just take up to the first 3 by default.
                                List<int> bandIDs = new List<int>();
                                for (int i = 0; i < imageLayer.BandCount; i++)
                                {
                                    bandIDs.Add(i);
                                    if (i == 2)
                                        break;
                                }

                                imageLayer.BandIds = bandIDs.ToArray();

                                // Use png format to support transparency
                                imageLayer.ImageFormat = ArcGISImageServiceLayer.ImageServiceImageFormat.PNG8;
                            }
                            else
                            {
                                // At v10 and later, band IDs do not need to be specified, and jpg/png
                                // format introduces some intelligence about which format is best
                                imageLayer.ImageFormat = 
                                    ArcGISImageServiceLayer.ImageServiceImageFormat.JPGPNG;
                            }

                            OnCreateLayerCompleted(new CreateLayerCompletedEventArgs() { Layer = layer, UserState = userState });
                        };

                        EventHandler<EventArgs> initFailed = null;
                        initFailed = (o, e) =>
                        {
                            layer.InitializationFailed -= initFailed;
                            OnCreateLayerCompleted(new CreateLayerCompletedEventArgs() { Layer = layer, UserState = userState });
                        };

                        layer.Initialized += initialized;
                        layer.InitializationFailed += initFailed;
                        layer.Initialize();
                    }
                    break;
                case ResourceType.Layer:
                case ResourceType.EditableLayer:
                    {
                        featureLayer = new Layer(resource.Url, resource.ProxyUrl);
                        featureLayer.GetLayerDetailsFailed += (o, e) =>
                        {
                            OnCreateLayerFailed(e);
                        };
                        featureLayer.GetLayerDetailsCompleted += (o, e) =>
                        {
                            if (e.LayerDetails == null)
                            {
                                OnCreateLayerFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionUnableToRetrieveLayerDetails), e.UserState));
                                return;
                            }
                            if (Utility.RasterLayer.Equals(e.LayerDetails.Type))
                            {
                                OnCreateLayerFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionRasterLayersNotSupported), e.UserState));
                                return;
                            }
                            else if (Utility.ImageServerLayer.Equals(e.LayerDetails.Type))
                            {
                                OnCreateLayerFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionImageServerLayersNotSupported), e.UserState));
                                return;
                            }

                            if (Utility.CapabilitiesSupportsQuery(e.LayerDetails.Capabilities) == false)
                            {
                                OnCreateLayerFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionLayerDoesNotSupportQuery), e.UserState));
                                return;
                            }

                            GeometryType GeometryType = GeometryType.Unknown;
                            switch (e.LayerDetails.GeometryType)
                            {
                                case "esriGeometryPoint":
                                    GeometryType = GeometryType.Point;
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
                                Url = featureLayer.Uri,
                                ProxyUrl = getCleanProxyUrl(featureLayer.ProxyUrl),
                                ID = Guid.NewGuid().ToString("N"),
                                Mode = FeatureLayer.QueryMode.OnDemand,
                                Renderer = new ESRI.ArcGIS.Mapping.Core.Symbols.HiddenRenderer()
                            };
                            newFeatureLayer.SetValue(MapApplication.LayerNameProperty, resource.DisplayName);
                            newFeatureLayer.SetValue(Core.LayerExtensions.DisplayUrlProperty, resource.Url);
                            newFeatureLayer.SetValue(Core.LayerExtensions.GeometryTypeProperty, GeometryType);
                            if (!string.IsNullOrEmpty(resource.ProxyUrl))
                                Core.LayerExtensions.SetUsesProxy(newFeatureLayer, true);
                            if (e.LayerDetails.Fields != null)
                            {
                                Collection<ESRI.ArcGIS.Mapping.Core.FieldInfo> fields = FieldInfosFromFields(e.LayerDetails.Fields);
                                newFeatureLayer.SetValue(Core.LayerExtensions.FieldsProperty, fields);
                                Core.LayerExtensions.SetDisplayField(newFeatureLayer, e.LayerDetails.DisplayField);
                            }
                            newFeatureLayer.OutFields.Add("*"); // Get all fields at configuration time
                            OnCreateLayerCompleted(new CreateLayerCompletedEventArgs() { Layer = newFeatureLayer, UserState = e.UserState, GeometryType = GeometryType });
                        };
                        featureLayer.GetLayerDetails(userState);
                    } break;

                default:
                    throw new Exception(string.Format(Resources.Strings.ExceptionCannotCreateLayerForResourceType, resource.ResourceType.ToString()));
            }
            
        }

        static Collection<ESRI.ArcGIS.Mapping.Core.FieldInfo> FieldInfosFromFields(List<Core.Field> fields)
        {
            Collection<ESRI.ArcGIS.Mapping.Core.FieldInfo> fieldInfos = new Collection<ESRI.ArcGIS.Mapping.Core.FieldInfo>();
            foreach (Core.Field field in fields)
            {
                if (FieldHelper.IsFieldFilteredOut(field.Type))
                    continue;

                fieldInfos.Add(Core.Field.FieldInfoFromField(field));
            }
            return fieldInfos;
        }

        public override void GetChildResourcesAsync(Resource parentResource, Filter filter, object userState)
        {
            switch (parentResource.ResourceType)
            {
                case ResourceType.Server:
                    getChildResourcesForServer(parentResource, filter, userState);
                    break;
                case ResourceType.Folder:
                    getServicesInFolder(parentResource, filter, userState);
                    break;
                case ResourceType.MapServer:
                case ResourceType.FeatureServer:
                case ResourceType.GPServer:
                    getResourcesInService(parentResource, userState);
                    break;
                case ResourceType.GroupLayer:
                    getLayersInGroupLayer(parentResource, userState);
                    break;
                case ResourceType.Layer:
                case ResourceType.EditableLayer:
                    getFieldsInLayer(parentResource, userState);
                    break;
            }
        }

        Layer layer;
        private void getFieldsInLayer(Resource parentResource, object userState)
        {
            layer = new Layer(parentResource.Url, parentResource.ProxyUrl);
            layer.GetLayerDetailsFailed += (o, e) =>
            {
                OnGetChildResourcesFailed(e);
            };
            layer.GetLayerDetailsCompleted += (o, e) =>
            {
                OnGetChildResourcesCompleted(new GetChildResourcesCompletedEventArgs() { ChildResources = e.ChildResources, UserState = e.UserState });
            };
            layer.GetLayerDetails(userState);
        }

        GroupLayer groupLayer;
        private void getLayersInGroupLayer(Resource parentResource, object userState)
        {
            groupLayer = new GroupLayer(parentResource.Url, parentResource.ProxyUrl);
            groupLayer.GetGroupLayerDetailsFailed += (o, e) =>
            {
                OnGetChildResourcesFailed(e);
            };
            groupLayer.GetGroupLayerDetailsCompleted += (o, e) =>
            {
                OnGetChildResourcesCompleted(new GetChildResourcesCompletedEventArgs() { ChildResources = e.ChildResources, UserState = e.UserState });
            };
            groupLayer.GetGroupLayerDetails(userState);
        }

        IService _service;
        private void getResourcesInService(Resource parentResource, object userState)
        {
            _service = ServiceFactory.CreateService(parentResource.ResourceType, parentResource.Url, parentResource.ProxyUrl);
            if (_service != null)
            {
                _service.ServiceDetailsDownloadFailed += (o, e) =>
                {
                    OnGetChildResourcesFailed(e);
                };
                _service.ServiceDetailsDownloadCompleted += (o, e) =>
                {
                    OnGetChildResourcesCompleted(new GetChildResourcesCompletedEventArgs() { ChildResources = (o as IService).ChildResources, UserState = e.UserState });
                };
                _service.GetServiceDetails(userState);
            }
        }

        Folder folder;
        private void getServicesInFolder(Resource parentResource, Filter filter, object userState)
        {
            folder = new Folder(parentResource.Url, parentResource.ProxyUrl) { Filter = filter };
            folder.GetServicesInFolderCompleted += (o, e) =>
            {
                OnGetChildResourcesCompleted(new GetChildResourcesCompletedEventArgs() { ChildResources = e.ChildResources, UserState = e.UserState });
            };
            folder.GetServicesInFolderFailed += (o, e) =>
            {
                OnGetChildResourcesFailed(e);
            };
            folder.GetServices(userState);
        }        

        Server server;
        private void getChildResourcesForServer(Resource parentResource, Filter filter, object userState)
        {
            server = new Server(parentResource.Url, parentResource.ProxyUrl) { Filter = filter };
            server.GetCatalogCompleted += (o, e) =>
            {
                OnGetChildResourcesCompleted(new GetChildResourcesCompletedEventArgs() { ChildResources = e.ChildResources, UserState = e.UserState });
            };
            server.GetCatalogFailed += (o, e) =>
            {
                OnGetChildResourcesFailed(e);
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
                ProxyUrl = proxyUrl
            };

            // Create a URI from the connection string and parse it looking for special token elements that can be used to
            // determine what kind of entity is located at the specified endpoint.
            try
            {
                Uri myUri = new Uri(connectionString);
                if (myUri.IsAbsoluteUri)
                {
                    string path = myUri.AbsolutePath;
                    string[] pathComponents = path.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                    if (pathComponents.Length >= 2)
                    {
                        if (String.Compare(pathComponents[pathComponents.Length - 1], "mapserver", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            res.ResourceType = ResourceType.MapServer;
                            res.DisplayName = pathComponents[pathComponents.Length - 2];
                            return res;
                        }
                        else if (String.Compare(pathComponents[pathComponents.Length - 1], "gpserver", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            res.ResourceType = ResourceType.GPServer;
                            res.DisplayName = pathComponents[pathComponents.Length - 2];
                            return res;
                        }
                        else if (String.Compare(pathComponents[pathComponents.Length - 1], "featureserver", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            res.ResourceType = ResourceType.FeatureServer;
                            res.DisplayName = pathComponents[pathComponents.Length - 2];
                            return res;
                        }
                    }

                    if (pathComponents.Length >= 3)
                    {
                        if (String.Compare(pathComponents[pathComponents.Length - 2], "mapserver", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            // It may seem wrong to call this a Map Server when the URL is clearly indicating a child layer, but
                            // we need to do this in order to get the parent service and all child layers since we want this
                            // context in the tree control.
                            res.ResourceType = ResourceType.MapServer;
                            res.DisplayName = pathComponents[pathComponents.Length - 3];

                            // Store the layer "id" in the tag. This will be extracted and used when populating the tree
                            // control in order to select the proper child item.
                            res.Tag = pathComponents[pathComponents.Length - 1];

                            // Create a URL that removes the trailing id
                            int i = res.Url.LastIndexOf("/");
                            if (i >= 0)
                                res.Url = res.Url.Remove(i);
                            return res;
                        }
                        else if (String.Compare(pathComponents[pathComponents.Length - 2], "gpserver", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            // It may seem wrong to call this a GP Server when the URL is clearly indicating a child tool, but
                            // we need to do this in order to get the parent service and all child tools since we want this
                            // context in the tree control.
                            res.ResourceType = ResourceType.GPServer;
                            res.DisplayName = pathComponents[pathComponents.Length - 3];

                            // Store the tool "id" in the tag. This will be extracted and used when populating the tree
                            // control in order to select the proper child item.
                            res.Tag = pathComponents[pathComponents.Length - 1];

                            // Create a URL that removes the trailing id
                            int i = res.Url.LastIndexOf("/");
                            if (i >= 0)
                                res.Url = res.Url.Remove(i);
                            return res;
                        }
                        else if (String.Compare(pathComponents[pathComponents.Length - 2], "featureserver", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            // It may seem wrong to call this a Feature Server when the URL is clearly indicating a child layer, but
                            // we need to do this in order to get the parent service and all child layers since we want this
                            // context in the tree control.
                            res.ResourceType = ResourceType.FeatureServer;
                            res.DisplayName = pathComponents[pathComponents.Length - 3];

                            // Store the layer "id" in the tag. This will be extracted and used when populating the tree
                            // control in order to select the proper child item.
                            res.Tag = pathComponents[pathComponents.Length - 1];

                            // Create a URL that removes the trailing id
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

            return res;
        }

        /// <summary>
        /// Retrieve the list of resources provided by the server
        /// </summary>
        public override void GetCatalog(string connectionString, string proxyUrl, Filter filter, object userState)
        {
            // Based on the input URL, get the list of URLs that might be the server endpoint
            Queue<string> candidateUrls = getCandidateUrls(connectionString);

            if (server == null || !candidateUrls.Contains(server.Uri))
            {
                // Try the URLs
                tryGetResourceForAgsCandidateUrl(candidateUrls, proxyUrl, filter, userState);
            }
            else
            {
                // The server URL is already one of the candidates, so assume this is the correct one
                server.Filter = filter;
                server.GetCatalogCompleted += (o, e) =>
                {
                    OnGetCatalogCompleted(new GetCatalogCompletedEventArgs() { ChildResources = e.ChildResources, UserState = e.UserState });
                };
                server.GetCatalogFailed += (o, e) =>
                {
                    OnGetCatalogFailed(new ExceptionEventArgs(new Exception(Strings.ExceptionDoneTryingallCandidateURLs), userState));
                };
                server.GetCatalog(userState);
            }
        }

        /// <summary>
        /// Get the instance-level metadata for the server
        /// </summary>
        private async Task<ServerInfo> getServerInfo(string connectionString, string proxyUrl)
        {
            // Based on the input URL, get the different possible URLs that could point to the server
            Queue<string> candidateUrls = getCandidateUrls(connectionString);
            if (server == null || !candidateUrls.Contains(server.Uri))
            {
                // Try the different URLs
                int count = candidateUrls.Count;
                for (int i = 0; i < count; i++)
                {
                    server = new Server(candidateUrls.Dequeue(), proxyUrl);

                    try
                    {
                        ServerInfo info = await server.GetServerInfo();
                        if (info != null)
                        {
                            m_cachedServerInfos.Add(info);
                            return info;
                        }
                    }
                    catch { } // swallow exception because we're intentionally trying different URL permutations
                }

                return null;
            }
            else
            {
                // Since the server was already assigned one of the candidate URLs, assume it is the correct
                // one and retrieve server metadata directly
                return await server.GetServerInfo();
            }
        }

        /// <summary>
        /// Get the instance-level metadata for the ArcGIS Server instance specified by the connection string
        /// </summary>
        public async static Task<ServerInfo> GetServerInfo(string connectionString, string proxyUrl)
        {
            var candidateUrls = getCandidateUrls(connectionString).ToArray();
            ServerInfo serverInfo = null;
            foreach (var url in candidateUrls)
            {
                foreach (var info in m_cachedServerInfos)
                {
                    if (url == info.Url)
                    {
                        serverInfo = info;
                        break;
                    }
                }
            }

            if (serverInfo == null)
            {
                var dataSource = new ArcGISServerDataSource();
                serverInfo = await dataSource.getServerInfo(connectionString, proxyUrl);
            }

            return serverInfo;
        }

        private static List<ServerInfo> m_cachedServerInfos = new List<ServerInfo>();

        /// <summary>
        /// Retrieves the services directory URL for the ArcGIS Server indicated by the passed-in URL
        /// </summary>
        public static async Task<string> GetServicesDirectoryURL(string url, string proxyUrl)
        {
            string restUrl = null;

            ServerInfo info = await ArcGISServerDataSource.GetServerInfo(url, proxyUrl);
            if (info != null && info.AuthenticationInfo != null && info.AuthenticationInfo.SupportsTokenAuthentication)
            {
                // Construct services directory URL from info URL.  Services directory URL is required for
                // IdentityManager to resolve token retrieval endpoint properly.
                restUrl = info.BaseUrl;
                if (restUrl.Contains("?"))
                    restUrl = restUrl.Split('?')[0];

                restUrl = restUrl.TrimEnd('/');
                if (restUrl.EndsWith("/info", StringComparison.OrdinalIgnoreCase))
                    restUrl = restUrl.Substring(0, restUrl.Length - 4) + "services";
            }

            return restUrl;
        }

        private static Queue<string> getCandidateUrls(string connectionString)
        {
            Queue<string> candidateUrls = new Queue<string>();
            AddUrlCandidate(candidateUrls, getAgsRestUrlCandidate(connectionString, true, true, false));
            AddUrlCandidate(candidateUrls, getAgsRestUrlCandidate(connectionString, true, true, true));
            AddUrlCandidate(candidateUrls, getAgsRestUrlCandidate(connectionString, false, true, false));
            AddUrlCandidate(candidateUrls, getAgsRestUrlCandidate(connectionString, false, true, true));
            AddUrlCandidate(candidateUrls, getAgsRestUrlCandidate(connectionString, true, false, false));
            AddUrlCandidate(candidateUrls, getAgsRestUrlCandidate(connectionString, true, false, true));
            AddUrlCandidate(candidateUrls, getAgsRestUrlCandidate(connectionString, false, false, false));
            AddUrlCandidate(candidateUrls, getAgsRestUrlCandidate(connectionString, false, false, true));
            return candidateUrls;
        }

        private static void AddUrlCandidate(Queue<string> urls, string candidateUrl)
        {
            // Enqueue candidate URL only if it is not already in the list
            if (!urls.Contains(candidateUrl))
                urls.Enqueue(candidateUrl);
        }

        private void tryGetResourceForAgsCandidateUrl(Queue<string> candidateUrls, string proxyUrl, Filter filter, object userState)
        {
            if (candidateUrls == null || candidateUrls.Count < 1)
            {
                OnGetCatalogFailed(new ExceptionEventArgs(new Exception(Strings.ExceptionDoneTryingallCandidateURLs), userState));
                return;
            }
            string agsRestUrl = candidateUrls.Dequeue();
            server = new Server(agsRestUrl, proxyUrl) { Filter = filter };
            server.GetCatalogCompleted += (o, e) =>
            {
                OnGetCatalogCompleted(new GetCatalogCompletedEventArgs() { ChildResources = e.ChildResources, UserState = e.UserState });
            };
            server.GetCatalogFailed += (o, e) =>
            {
                tryGetResourceForAgsCandidateUrl(candidateUrls, proxyUrl, filter, userState);
            };
            server.GetCatalog(userState);
        }

        private static string getAgsRestUrlCandidate(string url, bool appendOnlyInstancePath, bool useHttp, bool appendPath)
        {
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
            if ("services".Equals(lastComponent, StringComparison.InvariantCultureIgnoreCase))
                targetUrl = url;
            else if ("rest".Equals(lastComponent, StringComparison.InvariantCultureIgnoreCase))
                targetUrl = string.Format("{0}services", url);
            else if ("arcgis".Equals(lastComponent, StringComparison.InvariantCultureIgnoreCase))
                targetUrl = string.Format("{0}rest/services", url);
            else
            {
                if (appendOnlyInstancePath)
                {
                    if (appendPath)
                        targetUrl = string.Format("{0}rest/services", url);
                }
                else
                {
                    if (appendPath)
                        targetUrl = string.Format("{0}arcgis/rest/services", url);
                }
            }

            return targetUrl;
        }        
    }
}
