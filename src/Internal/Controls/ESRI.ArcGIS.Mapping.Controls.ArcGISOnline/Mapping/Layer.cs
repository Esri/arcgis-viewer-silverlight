/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Runtime.Serialization;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client.Geometry;
using System.Collections.Generic;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Text;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using ESRI.ArcGIS.Mapping.Core.Symbols;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    /// <summary>
    /// Represents a Layer in the map. This could be a layer within a Basemap or
    /// a layer that exists on its own.
    /// </summary>
    /// <remarks>
    /// Layers are created from LayerDescription. At runtime, Layer
    /// wraps an ArcGIS Layer which gets initialized on-demand.
    /// </remarks>  
    internal class Layer
    {
        ESRI.ArcGIS.Client.Layer _internalLayer;
        SpatialReference _mapSpatialReference;  // the spatial reference of the map that the layer is being added to

        CallState _initializationCallState;  // call state for the InitializeAsync method


        /// <summary>
        /// Creates a new Layer from the specified LayerDescription.
        /// </summary>
        public Layer(LayerDescription layerDesc)
        {
            LayerDescription = layerDesc;
            ConnectionStatus = LayerConnectionStatus.NotConnected;
        }


        /// <summary>
        /// The information about the layer map service, the name, url, etc.
        /// </summary>
        public LayerDescription LayerDescription { get; set; }


        /// <summary>
        /// Returns the Url of the service drawn by the layer.
        /// </summary>
        private string Url
        {
            get { return LayerDescription.Url; }
        }

        /// <summary>
        /// The type of layer.
        /// </summary>
        private LayerType Type { get; set; }

        /// <summary>
        /// Initializes the layer by creating the appropriate ESRI.ArcGIS.Client.Layer and 
        /// holding onto it (_internalLayer).
        /// </summary>
        public void InitializeAsync(Map map, object userState, EventHandler<LayerInitializedEventArgs> callback)
        {
            if (_internalLayer != null)
                return;


            ConnectionStatus = LayerConnectionStatus.NotConnected;

            // determine the layer type - TODO - change this when the web map format supports Bing and OSM
            //
            TileLayer.LayerType bingLayerType = TileLayer.LayerType.Aerial;
            switch (LayerDescription.LayerType)
            {
                case "BingMapsAerial":
                    bingLayerType = TileLayer.LayerType.Aerial;
                    Type = LayerType.BingLayer;
                    break;
                case "BingMapsRoad":
                    bingLayerType = TileLayer.LayerType.Road;
                    Type = LayerType.BingLayer;
                    break;
                case "BingMapsHybrid":
                    bingLayerType = TileLayer.LayerType.AerialWithLabels;
                    Type = LayerType.BingLayer;
                    break;
                case "OpenStreetMap":
                    Type = LayerType.OpenStreetMapLayer;
                    break;
                default:
                    if (LayerDescription.Service is ImageService)
                        Type = LayerType.ArcGISImageServiceLayer;
                    else if (LayerDescription.Service is FeatureLayerService)
                        Type = LayerType.ArcGISFeatureServiceLayer;
                    else
                    {
                        bool tiled = LayerDescription.Service is MapService && ((MapService)LayerDescription.Service).IsTiled &&
                          (map.SpatialReference == null || map.SpatialReference.Equals(((MapService)LayerDescription.Service).SpatialReference));
                        Type = tiled ? LayerType.ArcGISTiledMapServiceLayer : LayerType.ArcGISDynamicMapServiceLayer;

                        // if the service's spatial reference is different from the map's spatial reference, always set the layer type to be
                        // ArcGISDynamicMapServiceLayer
                        if (map != null && LayerDescription.Service != null)
                            if (!SpatialReference.AreEqual(LayerDescription.Service.SpatialReference, map.SpatialReference, true))
                                Type = LayerType.ArcGISDynamicMapServiceLayer;
                    }
                    break;
            }

            // now create the appropriate ESRI.ArcGIS.Client.Layer
            //
            string proxy = (LayerDescription.Service != null && LayerDescription.Service.RequiresProxy) ? ArcGISOnlineEnvironment.ConfigurationUrls.ProxyServerEncoded : null;

            if (Type == LayerType.ArcGISTiledMapServiceLayer)
            {
                ArcGISTiledMapServiceLayer tiledLayer = new ArcGISTiledMapServiceLayer() { Url = this.Url, ProxyURL = proxy };
                _internalLayer = tiledLayer;
                MapService mapService = LayerDescription.Service as MapService;
                if (mapService != null && mapService.TileInfo != null && mapService.TileInfo.LODs != null)
                {
                    double maxResolution = 0;
                    double minResolution = 0;
                    foreach (LODInfo lod in mapService.TileInfo.LODs)
                    {
                        if (lod.Resolution > maxResolution)
                            maxResolution = lod.Resolution;
                        if (minResolution <= 0 || minResolution > lod.Resolution)
                            minResolution = lod.Resolution;
                    }
                    if (maxResolution > 0)
                        tiledLayer.MaximumResolution = maxResolution * 4;
                    if (minResolution > 0)
                        tiledLayer.MinimumResolution = minResolution / 4;
                }
            }
            else if (Type == LayerType.ArcGISDynamicMapServiceLayer)
                _internalLayer = new ArcGISDynamicMapServiceLayer() { Url = this.Url, ProxyURL = proxy };
            else if (Type == LayerType.ArcGISImageServiceLayer)
            {
                int[] bandIds = ((ImageService)LayerDescription.Service).BandCount < 4 ? null : new int[] { 0, 1, 2 };
                _internalLayer = new ArcGISImageServiceLayer() { Url = this.Url, ProxyURL = proxy, ImageFormat = ArcGISImageServiceLayer.ImageServiceImageFormat.PNG8, BandIds = bandIds };
            }
            else if (Type == LayerType.ArcGISFeatureServiceLayer)
            {
                _internalLayer = new FeatureLayer() { 
                    Url = this.Url, 
                    ProxyUrl = proxy, 
                    Mode = LayerDescription.QueryMode, 
                    OutFields = new ESRI.ArcGIS.Client.Tasks.OutFields() { "*" },
                    Renderer = new ESRI.ArcGIS.Mapping.Core.Symbols.HiddenRenderer()
               };
            }
            else if (Type == LayerType.OpenStreetMapLayer)
                _internalLayer = new OpenStreetMapLayer();
            else if (Type == LayerType.BingLayer)
            {
                TileLayer tileLayer = new TileLayer() { LayerStyle = bingLayerType };
                tileLayer.Token = ArcGISOnlineEnvironment.BingToken;
                tileLayer.ServerType = ServerType.Production;

                _internalLayer = tileLayer;
            }

            _internalLayer.Visible = LayerDescription.Visible;
            _internalLayer.Opacity = LayerDescription.Opacity;

            if (!string.IsNullOrEmpty(LayerDescription.Title))
                _internalLayer.SetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty, LayerDescription.Title);

            if (LayerDescription.IsReference)
                ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetIsReferenceLayer(_internalLayer, true);

            // remember the map's spatial reference in order to determine after initialization
            // if the layer will work properly
            //
            if (map != null)
                _mapSpatialReference = map.SpatialReference;

            _internalLayer.Initialized += _internalLayer_Initialized;
            _internalLayer.InitializationFailed += _internalLayer_InitializationFailed;
            _initializationCallState = new CallState() { Callback = callback, UserState = userState };
            _internalLayer.Initialize();
        }

        /// <summary>
        /// Occurs when a layer fails to initialize. We actually treat that case in the Initialized
        /// handler which also gets fired but this handler needs to be in place or an exception
        /// gets thrown and bubbles up to the browser.
        /// </summary>
        void _internalLayer_InitializationFailed(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Raised by the ArcGIS layer when initialization completes successfully.
        /// </summary>
        /// <remarks>
        /// Updates the layer's ConnectionStatus. Raises the Initialized event.
        /// </remarks>
        void _internalLayer_Initialized(object sender, EventArgs e)
        {
            SetInternalLayer(_internalLayer);

            EventHandler<LayerInitializedEventArgs> callback = (EventHandler<LayerInitializedEventArgs>)_initializationCallState.Callback;
            if (callback != null)
                callback(this, new LayerInitializedEventArgs() { UserState = _initializationCallState.UserState,
                                                                 Error = ConnectionStatus != LayerConnectionStatus.Connected,
                                                                  Layer = _internalLayer
                });

            if (Initialized != null)
                Initialized(this, EventArgs.Empty);
        }
        private void SetFeatureLayerPropsFromLayerInfo(FeatureLayer fLayer)
        {
            if (fLayer == null || fLayer.LayerInfo == null)
                return;

            if (string.IsNullOrWhiteSpace(LayerExtensions.GetDisplayUrl(fLayer)))
                fLayer.SetValue(LayerExtensions.DisplayUrlProperty, fLayer.Url);

            if (!string.IsNullOrWhiteSpace(fLayer.LayerInfo.DisplayField) &&
                string.IsNullOrWhiteSpace(LayerExtensions.GetDisplayField(fLayer)))
                fLayer.SetValue(LayerExtensions.DisplayFieldProperty, fLayer.LayerInfo.DisplayField);

            if (LayerExtensions.GetGeometryType(fLayer) == GeometryType.Unknown)
            {
                if (fLayer.LayerInfo.GeometryType == ESRI.ArcGIS.Client.Tasks.GeometryType.Point)
                    fLayer.SetValue(LayerExtensions.GeometryTypeProperty, GeometryType.Point);
                if (fLayer.LayerInfo.GeometryType == ESRI.ArcGIS.Client.Tasks.GeometryType.MultiPoint)
                    fLayer.SetValue(LayerExtensions.GeometryTypeProperty, GeometryType.MultiPoint);
                else if (fLayer.LayerInfo.GeometryType == ESRI.ArcGIS.Client.Tasks.GeometryType.Polyline)
                    fLayer.SetValue(LayerExtensions.GeometryTypeProperty, GeometryType.Polyline);
                else if (fLayer.LayerInfo.GeometryType == ESRI.ArcGIS.Client.Tasks.GeometryType.Polygon)
                    fLayer.SetValue(LayerExtensions.GeometryTypeProperty, GeometryType.Polygon);
            }

            if (fLayer.LayerInfo.Fields != null)
            {
                Collection<FieldInfo> layerFields = LayerExtensions.GetFields(fLayer);
                if (layerFields.Count == 0) // fields not determined yet
                {
                    Collection<FieldInfo> fieldInfos = new Collection<FieldInfo>();
                    foreach (ESRI.ArcGIS.Client.Field field in fLayer.LayerInfo.Fields)
                    {
                        if (FieldHelper.IsFieldFilteredOut(field.Type))
                            continue;

                        FieldInfo f = ESRI.ArcGIS.Mapping.Core.FieldInfo.FieldInfoFromField(fLayer, field);

                        fieldInfos.Add(f);
                    }
                    fLayer.SetValue(LayerExtensions.FieldsProperty, fieldInfos);
                }
            }
        }

        private void SetInternalLayer(ESRI.ArcGIS.Client.Layer layer)
        {
            _internalLayer = layer;

            SetFeatureLayerPropsFromLayerInfo(_internalLayer as FeatureLayer);

            if (_internalLayer.InitializationFailure != null)
            {
                ConnectionStatus = LayerConnectionStatus.InitializationFailed;
            }
            else if (_mapSpatialReference == null) // initialized independent of the map
            {
                ConnectionStatus = LayerConnectionStatus.Connected;
            }
            else
            {
                if (SpatialReference.AreEqual(_mapSpatialReference, _internalLayer.SpatialReference, false))
                {
                    ConnectionStatus = LayerConnectionStatus.Connected;

                }
                else if (Type == LayerType.ArcGISDynamicMapServiceLayer ||
                         Type == LayerType.ArcGISImageServiceLayer ||
                         Type == LayerType.ArcGISFeatureServiceLayer)
                {
                    ConnectionStatus = LayerConnectionStatus.Connected;
                }
            }

        }

        /// <summary>
        /// Raised when the Layer has been initialized.
        /// </summary>
        public event EventHandler Initialized;

        private LayerConnectionStatus ConnectionStatus { get; set; }

        private enum LayerConnectionStatus
        {
            NotConnected,
            Connected,
            InitializationFailed
        }

         /// <summary>
        /// Specifies the type of ArcGIS layer to which a layer corresponds.
        /// </summary>
        private enum LayerType
        {
            ArcGISTiledMapServiceLayer,
            ArcGISDynamicMapServiceLayer,
            ArcGISImageServiceLayer,
            ArcGISFeatureServiceLayer,
            BingLayer,
            OpenStreetMapLayer
        }
    }


    /// <summary>
    /// Provides data for asynchronous initialization.
    /// </summary>
    public class LayerInitializedEventArgs : EventArgs
    {
        public object UserState { get; set; }
        public ESRI.ArcGIS.Client.Layer Layer { get; set; }
        public bool Error { get; set; }
    }

}
