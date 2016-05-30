/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client.Geometry;

namespace SearchTool
{
    /// <summary>
    /// Represents an ArcGIS Server service such as a MapService or ImageService.
    /// </summary>
    [DataContract]
    public class ArcGISService
    {
        /// <summary>
        /// Gets or sets the title of the arcgis service.
        /// </summary>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the url of the arcgis service.
        /// </summary>
        [DataMember]
        public string Url { get; set; }

        [DataMember(Name = "spatialReference")]
        public ESRI.ArcGIS.Client.Geometry.SpatialReference SpatialReference { get; set; }

        [DataMember(Name = "units")]
        public string Units { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "serviceDescription")]
        public string ServiceDescription { get; set; }

        [DataMember(Name = "copyrightText")]
        public string CopyrightText { get; set; }

        [DataMember(Name = "currentVersion")]
        public string CurrentVersion { get; set; }

        public bool RequiresProxy { get; set; }

        /// <summary>
        /// Gets the service information for an ArcGIS Server service.
        /// </summary>
        /// <param name="url">The URL of the service</param>
        /// <param name="userState">Object to be passed to the callback</param>
        /// <param name="callback">The method to invoke when information retrieval has completed</param>
        /// <param name="proxyUrl">The URL of a proxy server to use when making the request</param>
        public static void GetServiceInfoAsync(string url, object userState, EventHandler<ServiceEventArgs> callback, string proxyUrl = null)
        {
            string jsonUrl = url.Contains("?") ? url + "&f=json" : url + "?f=json";
            WebUtil.OpenReadAsync(new Uri(jsonUrl), url, (sender, e) =>
            {
                if (e.Error != null)
                {
                    callback(null, new ServiceEventArgs());
                    return;
                }
                string requestUrl = (string)e.UserState;

                MapService mapService = WebUtil.ReadObject<MapService>(e.Result);
                if (mapService != null && mapService.Name != null && mapService.Units != null)
                {
                    mapService.Url = requestUrl;
                    mapService.RequiresProxy = e.UsedProxy;
                    mapService.InitTitle();
                    callback(null, new ServiceEventArgs() { Service = mapService, UserState = userState });
                    return;
                }

                FeatureService featureService = WebUtil.ReadObject<FeatureService>(e.Result);
                if (featureService != null && featureService.Layers != null && featureService.Layers.Length > 0)
                {
                    featureService.Url = requestUrl;
                    featureService.RequiresProxy = e.UsedProxy;
                    featureService.InitTitle();
                    callback(null, new ServiceEventArgs() { Service = featureService, UserState = userState });
                    return;
                }

                ImageService imageService = WebUtil.ReadObject<ImageService>(e.Result);
                if (imageService != null && imageService.PixelType != null)
                {
                    imageService.Url = requestUrl;
                    imageService.RequiresProxy = e.UsedProxy;
                    imageService.InitTitle();
                    callback(null, new ServiceEventArgs() { Service = imageService, UserState = userState });
                    return;
                }

                FeatureLayerService featureLayerService = WebUtil.ReadObject<FeatureLayerService>(e.Result);
                if (featureLayerService != null && featureLayerService.Type == "Feature Layer")
                {
                    featureLayerService.Url = requestUrl;
                    featureLayerService.RequiresProxy = e.UsedProxy;
                    featureLayerService.Title = featureLayerService.Name;
                    callback(null, new ServiceEventArgs() { Service = featureLayerService, UserState = userState });
                    return;
                }

                LocatorService locatorService = WebUtil.ReadObject<LocatorService>(e.Result);
                if (locatorService != null && locatorService.CandidateFields != null 
                && (locatorService.AddressFields != null || locatorService.SingleLineAddressField != null))
                {
                    locatorService.Url = requestUrl;
                    locatorService.RequiresProxy = e.UsedProxy;
                    locatorService.InitTitle();
                    callback(null, new ServiceEventArgs() { Service = locatorService, UserState = userState });
                    return;
                }

                callback(null, new ServiceEventArgs());
            }, proxyUrl);
        }
        void InitTitle()
        {
            // construct a title for the map service from the directory structure of the url
            //
            string[] tokens = Url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length > 2)
                Title = tokens[tokens.Length - 2];
            else
                Title = Url;
        }
    }

    /// <summary>
    /// Represents an ArcGIS Server MapService.
    /// </summary>
    [DataContract]
    public class MapService : ArcGISService
    {
        [DataMember(Name = "singleFusedMapCache")]
        public bool IsTiled { get; set; }

        [DataMember(Name = "mapName")]
        public string Name { get; set; }

        [DataMember(Name = "tileInfo")]
        public TileInfoInfo TileInfo { get; set; }

        [DataMember(Name = "layers")]
        public FeatureLayerDescription[] Layers { get; set; }
    }

    [DataContract]
    public class TileInfoInfo
    {
        // Properties
        [DataMember(Name = "cols")]
        public int Cols { get; set; }
        [DataMember(Name = "compressionQuality")]
        public string CompressionQuality { get; set; }
        [DataMember(Name = "dpi")]
        public int DPI { get; set; }
        [DataMember(Name = "format")]
        public string Format { get; set; }
        [DataMember(Name = "lods")]
        public LODInfo[] LODs { get; set; }
        [DataMember(Name = "origin")]
        public MapPoint Origin { get; set; }
        [DataMember(Name = "rows")]
        public int Rows { get; set; }
    }

    [DataContract]
    public class LODInfo
    {
        // Properties
        [DataMember(Name = "level")]
        public int Level { get; set; }
        [DataMember(Name = "resolution")]
        public double Resolution { get; set; }
        [DataMember(Name = "scale")]
        public double Scale { get; set; }
    }



    /// <summary>
    /// Represents an ArcGIS Server FeatureService.
    /// </summary>
    [DataContract]
    public class FeatureService : ArcGISService
    {
        [DataMember(Name = "layers")]
        public FeatureLayerDescription[] Layers { get; set; }
    }

    /// <summary>
    /// Represents an ArcGIS Server FeatureLayerDescription.
    /// </summary>
    [DataContract]
    public class FeatureLayerDescription
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }

    /// <summary>
    /// Represents an ArcGIS Server FeatureLayerService.
    /// </summary>
    [DataContract]
    public class FeatureLayerService : ArcGISService
    {
        Envelope _extent;

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "geometryType")]
        public string GeometryType { get; set; }

        [DataMember(Name = "extent")]
        public Envelope Extent
        {
            get { return _extent; }
            set
            {
                _extent = value;
                if (_extent != null)
                    SpatialReference = _extent.SpatialReference;
            }
        }

    }

    /// <summary>
    /// Represents an ArcGIS Server MapService.
    /// </summary>
    [DataContract]
    public class ImageService : ArcGISService
    {
        Envelope _extent;

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "extent")]
        public Envelope Extent
        {
            get { return _extent; }
            set
            {
                _extent = value;
                if (_extent != null)
                    SpatialReference = _extent.SpatialReference;
            }
        }

        [DataMember(Name = "serviceDataType")]
        public string ServiceDataType { get; set; }

        [DataMember(Name = "bandCount")]
        public int BandCount { get; set; }

        [DataMember(Name = "pixelSizeX")]
        public double PixelSizeX { get; set; }

        [DataMember(Name = "pixelSizeY")]
        public double PixelSizeY { get; set; }

        [DataMember(Name = "pixelType")]
        public string PixelType { get; set; }
    }

    /// <summary>
    /// Represents an ArcGIS Server locator service
    /// </summary>
    [DataContract]
    public class LocatorService : ArcGISService
    {
        [DataMember(Name = "addressFields")]
        public Field[] AddressFields { get; set; }

        [DataMember(Name = "candidateFields")]
        public Field[] CandidateFields { get; set; }

        [DataMember(Name = "intersectionCandidateFields")]
        public Field[] IntersectionCandidateFields { get; set; }

        [DataMember(Name = "singleLineAddressField")]
        public Field SingleLineAddressField { get; set; }

        [DataMember(Name = "locatorProperties")]
        public LocatorProperties LocatorProperties { get; set; }
    }

    /// <summary>
    /// Represents a field definition
    /// </summary>
    [DataContract]
    public class Field
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        [DataMember(Name = "required")]
        public bool Required { get; set; }

        [DataMember(Name = "length")]
        public int Length { get; set; }
    }

    /// <summary>
    /// Encapsulates the Locator Properties metadata of an ArcGIS Server locator service
    /// </summary>
    [DataContract]
    public class LocatorProperties
    {
        [DataMember(Name = "UICLSID")]
        public Guid ID { get; set; }

        [DataMember]
        public int SpellingSensitivity { get; set; }

        [DataMember]
        public bool MatchIfScoresTie { get; set; }

        [DataMember]
        public int SuggestedBatchSize { get; set; }

        [DataMember]
        public int MinimumCandidateScore { get; set; }

        [DataMember]
        public string SideOffsetUnits { get; set; }

        [DataMember]
        public string IntersectionConnectors { get; set; }

        [DataMember]
        public int SideOffset { get; set; }

        [DataMember]
        public int EndOffset { get; set; }

        [DataMember]
        public string WriteXYCoordFields { get; set; }

        [DataMember]
        public string WriteStandardizedAddressFields { get; set; }

        [DataMember]
        public string WriteReferenceIDField { get; set; }

        [DataMember]
        public string WritePercentAlongField { get; set; }
    }

    /// <summary>
    /// Provides data for asynchronous calls to get service information.
    /// </summary>
    public class ServiceEventArgs : EventArgs
    {
        public ArcGISService Service { get; set; }
        public object UserState { get; set; }
    }

    /// <summary>
    /// Represents an ArGIS Server Services Directory.
    /// </summary>
    [DataContract]
    public class ServicesDirectory
    {
        [DataMember(Name = "folders")]
        public string[] Folders { get; set; }

        [DataMember(Name = "services")]
        public ServiceDescription[] Services { get; set; }
    }

    /// <summary>
    /// Represents an ArGIS Service description.
    /// </summary>
    [DataContract]
    public class ServiceDescription
    {
        [DataMember(Name = "name")]
        public string Title { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        public string Url { get; set; }
    }

    /// <summary>
    /// Represents a collection of legends for MapService layers.
    /// </summary>
    [DataContract]
    public class MapServiceLayerLegends
    {
        [DataMember(Name = "layers")]
        public MapServiceLayerLegend[] LayerLegends { get; set; }
    }

    /// <summary>
    /// Represents the legend for a MapService sublayer.
    /// </summary>
    [DataContract]
    public class MapServiceLayerLegend
    {
        [DataMember(Name = "layerId")]
        public string LayerId { get; set; }
        [DataMember(Name = "layerName")]
        public string LayerName { get; set; }
        [DataMember(Name = "legend")]
        public LegendItem[] LegendItems { get; set; }

        /// <summary>
        /// Gets the layer legends of sublayers of the layer that is 
        /// represented by this MapServiceLayerLegend.
        /// </summary>
        public List<MapServiceLayerLegend> Children { get; set; }

        /// <summary>
        /// Gets the layer legends of sublayers that are visible.
        /// </summary>
        public ObservableCollection<MapServiceLayerLegend> VisibleChildren { get; set; }


        /// <summary>
        /// Searches for the MapServiceLayerLegend whose Children collection contains the 
        /// specified layerLegend.
        /// </summary>
        /// <param name="layerLegend">The MapServiceLayerLegend of a MapService sublayer.</param>
        /// <returns></returns>
        public MapServiceLayerLegend FindContainer(MapServiceLayerLegend layerLegend)
        {
            if (Children.Contains(layerLegend))
                return this;

            foreach (MapServiceLayerLegend childLayerLegend in Children)
            {
                MapServiceLayerLegend container = childLayerLegend.FindContainer(layerLegend);
                if (container != null)
                    return container;
            }
            return null;
        }
    }

    /// <summary>
    /// Represents an individual symbol description of a layer legend.
    /// </summary>
    [DataContract]
    public class LegendItem
    {
        [DataMember(Name = "label")]
        public string Label { get; set; }
        [DataMember(Name = "imgBytes")]
        public string SymbolBytes { get; set; } //legend swatches returned in byte format

        public ImageSource Symbol
        {
            get
            {
                if (!string.IsNullOrEmpty(SymbolBytes))
                {
                    byte[] bytes = System.Convert.FromBase64String(SymbolBytes);

                    BitmapImage image = new BitmapImage();

                    using (MemoryStream mS = new MemoryStream(bytes))
                        image.SetSource(mS);

                    return image;
                }

                return null;
            }
        }
    }
}
