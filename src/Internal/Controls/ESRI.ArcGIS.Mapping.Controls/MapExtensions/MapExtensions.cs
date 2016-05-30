/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.WebMap;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using ESRI.ArcGIS.Mapping.DataSources;
using esriControlsResources = ESRI.ArcGIS.Mapping.Controls.Resources;
using System.Windows;
using ESRI.ArcGIS.Client.Toolkit.DataSources;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public static class MapExtensions
    {
        /// <summary>
        /// Identifies the WebMapDisplayName attached property
        /// </summary>
        private static readonly DependencyProperty WebMapDisplayNameProperty =
            DependencyProperty.RegisterAttached("WebMapDisplayName",
            typeof(string), typeof(Layer), null);

        /// <summary>
        /// Gets the display name of the layer as specified in the web map from which the layer originated
        /// </summary>
        private static string GetWebMapDisplayName(Layer obj)
        {
            return obj.GetValue(WebMapDisplayNameProperty) as string;
        }

        /// <summary>
        /// Sets the display name of the layer as specified in the web map from which the layer originated
        /// </summary>
        private static void SetWebMapDisplayName(Layer obj, string value)
        {
            obj.SetValue(WebMapDisplayNameProperty, value);
        }

        /// <summary>
        /// Determines the map units for the map
        /// </summary>
        /// <param name="map">The map to find map units for</param>
        /// <param name="callback">The method to invoke once map units have been retrieved</param>
        /// <param name="failedCallback">The method to invoke when map unit retrieval fails</param>
        public static void GetMapUnitsAsync(this Map Map, Action<MapUnit> callback, 
            Action<object, ExceptionEventArgs> failedCallback = null)
        {
            if (Map != null)
            {
                Layer layer = Map.Layers.FirstOrDefault();
                if (layer != null)
                {
                    ESRI.ArcGIS.Client.Bing.TileLayer tiledLayer = layer as ESRI.ArcGIS.Client.Bing.TileLayer;
                    if (tiledLayer != null)
                    {
                        // Bing is web mercator
                        callback(Core.DataSources.MapUnit.Meters);
                    }
                    else
                    {
                        ESRI.ArcGIS.Client.Toolkit.DataSources.OpenStreetMapLayer osmLayer = layer as ESRI.ArcGIS.Client.Toolkit.DataSources.OpenStreetMapLayer;
                        if (osmLayer != null)
                        {
                            // Open Streem map is web mercator
                            callback(Core.DataSources.MapUnit.Meters);
                        }
                        else
                        {
                            // ArcGIS Server Base Map                            
                            string layerUrl = null;
                            ArcGISTiledMapServiceLayer agsTiledlayer = layer as ArcGISTiledMapServiceLayer;
                            if (agsTiledlayer != null)
                                layerUrl = agsTiledlayer.Url;
                            else
                            {
                                ArcGISDynamicMapServiceLayer agsDynamicLayer = layer as ArcGISDynamicMapServiceLayer;
                                if (agsDynamicLayer != null)
                                    layerUrl = agsDynamicLayer.Url;
                                else
                                {
                                    ArcGISImageServiceLayer agsImageLayer = layer as ArcGISImageServiceLayer;
                                    if (agsImageLayer != null)
                                        layerUrl = agsImageLayer.Url;
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(layerUrl))
                            {
                                DataSourceProvider agsDataSource = new DataSourceProvider();
                                IBaseMapDataSource dataSource = agsDataSource.CreateDataSourceForBaseMapType(BaseMapType.ArcGISServer) as IBaseMapDataSource;
                                dataSource.GetMapServiceMetaDataCompleted += (o, args) =>
                                {
                                    callback(args.MapUnit);
                                };
                                dataSource.GetMapServiceMetaDataFailed += (o, args) =>
                                {
                                    if (failedCallback != null)
                                        failedCallback(Map, args);
                                };
                                dataSource.GetMapServiceMetaDataAsync(layerUrl, null);
                                return;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a <see cref="Map"/> object based on a web map
        /// </summary>
        /// <param name="map">The map to initialize</param>
        /// <param name="e">The <see cref="GetMapCompletedEventArgs"/> object containing the web map's information. 
        /// This is the event args type returned to the <see cref="Document.GetMapCompleted"/> event.</param>
        public static void InitializeFromWebMap(this Map map, GetMapCompletedEventArgs e, EventHandler<EventArgs> onLayerInitFailed = null)
        {
            map.Layers.Clear();

            if (e.Map.Extent != null && e.Map.Extent.SpatialReference != null
            && e.Map.Extent.SpatialReference.IsWebMercator()
            && double.IsNegativeInfinity(e.Map.Extent.YMin) 
            && double.IsPositiveInfinity(e.Map.Extent.YMax))
            {
                e.Map.Extent.YMin = double.NaN;
                e.Map.Extent.YMax = double.NaN;
            }

            map.Extent = e.Map.Extent;
            List<Layer> layers = new List<Layer>();
            List<Layer> basemapLayers = new List<Layer>();
            IEnumerable<Layer> allLayers = e.Map.Layers.FlattenLayers();
            List<string> featureCollectionLayerNames = new List<string>();

            // Create collection of layers to add to the map based on the layers in the web map
            foreach (Layer layer in allLayers)
            {
                // Set ShowLegend to true for each layer.  The framework handles layer visibility
                // in MapContents otherwise.
                layer.ShowLegend = true;

                if (layer is ESRI.ArcGIS.Client.Bing.TileLayer)
                   ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetUsesBingAppID((ESRI.ArcGIS.Client.Bing.TileLayer) layer, true);

                layer.ProcessWebMapProperties(e.DocumentValues);

                // Check whether any layers flagged for adding to the map have the same ID as the current one.  Layers
                // with the same ID represent feature collections that show up as part of the same layer in the online
                // viewers, but are actually serialized as separate layers.
                if (!(layer is GraphicsLayer) || !layers.Any(l => l.ID == layer.ID 
                || l.DisplayName == layer.DisplayName 
                || featureCollectionLayerNames.Contains(layer.DisplayName)))
                {
                    if ((bool)(layer.GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty)))
                        basemapLayers.Add(layer);

                    layers.Add(layer);
                }
                else // Layer belongs to a multi-layer feature collection.  Combine with layer already included in the list.
                {
                    GraphicsLayer currentLayer = layers.First(l => l.ID == layer.ID 
                        || l.DisplayName == layer.DisplayName 
                        || featureCollectionLayerNames.Contains(layer.DisplayName)) as GraphicsLayer;
                    GraphicsLayer newLayer = layer as GraphicsLayer;
                    if (newLayer != null && newLayer.Graphics.Count > 0)
                    {
                        if (currentLayer != null && LayerExtensions.GetGeometryType(currentLayer) == LayerExtensions.GetGeometryType(newLayer))
                        {
                            // Layers have the same geometry type - just copy the features from one to the other

                            newLayer.Graphics.MoveTo(currentLayer.Graphics);
                        }
                        else if (currentLayer != null && currentLayer.Graphics.Count == 0)
                        {
                            // Geometry types don't match, but the layer already added to the list doesn't have any 
                            // features graphics.  Override the renderer from the added layer with that from the current
                            // one and copy over the features in the currently layer.

                            currentLayer.Renderer = newLayer.Renderer;
                            newLayer.Graphics.MoveTo(currentLayer.Graphics);
                        }
                        else
                        {
                            // Geometry types don't match, but both layers have features.  We don't want to put the 
                            // features in the same layer because that eliminates the ability to configure the layer's
                            // renderer.  So create separate layers.

                            // The layers will have the same name by default.  To avoid having multiple layers with the
                            // same name, append a suffix that indicates the geometry type, i.e. points, lines, or areas.
                            if (currentLayer != null)
                            {
                                currentLayer.AppendGeometryToLayerName();
                                featureCollectionLayerNames.Add(layer.DisplayName);
                            }
                            newLayer.AppendGeometryToLayerName();

                            // The layers will have the same ID by default, which can cause unexpected behavior.  
                            // So give one of them a new ID.
                            newLayer.ID = Guid.NewGuid().ToString("N");
                            layers.Add(newLayer);

                            // Look in the web map's layers for other layers that have the same geometry type as the new
                            // layer.  Since the new layer has a new ID, and the logic here relies on ID to determine 
                            // whether to merge with another layer, we need to update the IDs of the layers yet to be
                            // processed to match the new ID.
                            IEnumerable<Layer> others = allLayers.Where(
                                l => (l.ID == layer.ID) && LayerExtensions.GetGeometryType((GraphicsLayer)l) ==
                                LayerExtensions.GetGeometryType((GraphicsLayer)layer));

                            foreach (GraphicsLayer gLayer in others)
                                gLayer.ID = newLayer.ID;
                        }
                    }
                } 
            }
            #region Get Basemap Title
            if (basemapLayers.Count > 0 && e.DocumentValues.ContainsKey("baseMap"))
            {
                IDictionary<string, object> dict = e.DocumentValues["baseMap"] as IDictionary<string, object>;
                if (dict != null)
                {
                    string baseMapTitle = "Basemap";
                    if (dict.ContainsKey("title"))
                    {
                        baseMapTitle = dict["title"] as string;
                        if (!string.IsNullOrWhiteSpace(baseMapTitle))
                            LayerExtensions.SetLayerName(basemapLayers[0], baseMapTitle);
                    }
                    //Mark reference layers
                    if (basemapLayers.Count > 1)
                    {
                        for (int i = 1; i < basemapLayers.Count; i++)
                        {
                            LayerExtensions.SetIsReferenceLayer(basemapLayers[i], true);
                            //Do not show in map contents
                            ESRI.ArcGIS.Client.Extensibility.LayerProperties.SetIsVisibleInMapContents(basemapLayers[i], false);
                        }
                    }
                }
            }
            #endregion

            e.Map.Layers.Clear();
            foreach (Layer layer in layers)
            {
                if (onLayerInitFailed != null)
                    layer.InitializationFailed += onLayerInitFailed;
                map.Layers.Add(layer);
            }

            #region Get map items and add any notes
            if (e.DocumentValues != null)
            {
                foreach (KeyValuePair<string, object> pair in e.DocumentValues)
                {
                    if (pair.Key == "MapItems")
                    {
                        List<GraphicsLayer> noteLayers = new List<GraphicsLayer>();
                        #region Get note layers
                        List<object> items = pair.Value as List<object>;
                        if (items == null)
                            continue;
                        foreach (var item in items)
                        {
                            IDictionary<string, object> dict = item as IDictionary<string, object>;
                            if (dict != null)
                            {
                                #region If note, add to notelayers
                                if (dict.ContainsKey("__type") && dict["__type"].ToString() == "Note:#ESRI.ArcGIS.Mapping.Controls.ArcGISOnline")
                                {
                                    if (dict.ContainsKey("Geometry") && dict.ContainsKey("Name"))
                                    {
                                        string name = dict["Name"] as string;
                                        IDictionary<string, object> gDict = dict["Geometry"] as IDictionary<string, object>;
                                        if (gDict == null) continue;
                                        ESRI.ArcGIS.Client.Geometry.Geometry geometry = null;
                                        if (gDict.ContainsKey("__type") && gDict["__type"] is string)
                                        {
                                            if (gDict["__type"].ToString() == "point:#ESRI.ArcGIS.Client.Geometry")
                                            {
                                                geometry = CreatePoint(gDict);
                                            }
                                            else if (gDict["__type"].ToString() == "Polyline:#ESRI.ArcGIS.Client.Geometry")
                                            {
                                                if (gDict.ContainsKey("paths") && gDict["paths"] is List<object>)
                                                {
                                                    List<object> paths = gDict["paths"] as List<object>;
                                                    Polyline line = new Polyline();
                                                    if (paths != null)
                                                    {
                                                        foreach (object path in paths)
                                                        {
                                                            List<object> points = path as List<object>;
                                                            ESRI.ArcGIS.Client.Geometry.PointCollection pts = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                                                            foreach (object point in points)
                                                            {
                                                                if (point is IDictionary<string, object>)
                                                                    pts.Add(CreatePoint(point as IDictionary<string, object>));
                                                            }
                                                            line.Paths.Add(pts);
                                                        }
                                                        geometry = line;
                                                    }
                                                }
                                            }
                                            else if (gDict["__type"].ToString() == "Polygon:#ESRI.ArcGIS.Client.Geometry")
                                            {
                                                if (gDict.ContainsKey("rings") && gDict["rings"] is List<object>)
                                                {
                                                    List<object> rings = gDict["rings"] as List<object>;
                                                    Polygon gon = new Polygon();
                                                    if (rings != null)
                                                    {
                                                        foreach (object ring in rings)
                                                        {
                                                            List<object> points = ring as List<object>;
                                                            ESRI.ArcGIS.Client.Geometry.PointCollection pts = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                                                            foreach (object point in points)
                                                            {
                                                                if (point is IDictionary<string, object>)
                                                                    pts.Add(CreatePoint(point as IDictionary<string, object>));
                                                            }
                                                            gon.Rings.Add(pts);
                                                        }
                                                        geometry = gon;
                                                    }
                                                }
                                            }
                                        }
                                        if (geometry != null && gDict.ContainsKey("spatialReference"))
                                        {
                                            IDictionary<string, object> srDict = gDict["spatialReference"] as IDictionary<string, object>;
                                            if (srDict != null)
                                            {
                                                if (srDict.ContainsKey("wkid"))
                                                    geometry.SpatialReference = new SpatialReference() { WKID = Int32.Parse(srDict["wkid"].ToString()) };
                                                else if (srDict.ContainsKey("wkt"))
                                                    geometry.SpatialReference = new SpatialReference() { WKT = srDict["wkt"].ToString() };
                                            }
                                        }
                                        if (geometry != null)
                                        {
                                            GraphicsLayer glayer = ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Note.CreateGraphicsLayer(name, new Graphic() { Geometry = geometry }, map);
                                            if (dict.ContainsKey("Visible"))
                                            {
                                                bool visible = true;
                                                try
                                                {
                                                    visible = (bool)(dict["Visible"]);
                                                }
                                                catch { }
                                                glayer.Visible = visible;
                                            }
                                            noteLayers.Add(glayer);
                                        }
                                    }

                                }
                                #endregion
                            }
                        }
                        if (noteLayers.Count > 0)
                        {
                            for (int i = noteLayers.Count - 1; i >= 0; i--)
                            {
                                if (noteLayers[i] != null)
                                    map.Layers.Add(noteLayers[i]);
                            }
                        }
                        #endregion
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Converts a layer hierarchy that may include group layers into a flat collection of layers.  
        /// </summary>
        private static IEnumerable<Layer> FlattenLayers(this IEnumerable<Layer> layers, GroupLayer parent = null)
        {
            LayerCollection flattenedLayers = new LayerCollection();
            foreach (Layer layer in layers)
            {
                if (layer is GroupLayer && !(layer is KmlLayer))
                {
                    // Flatten group layers
                    GroupLayer groupLayer = (GroupLayer)layer;
                    foreach (Layer child in groupLayer.ChildLayers.FlattenLayers(groupLayer))
                        flattenedLayers.Add(child);                    
                }
                else
                {
                    // If the layer was within a group layer, account for the group layer's visibility
                    // and opacity
                    if (parent != null)
                    {
                        layer.Visible = !parent.Visible ? false : layer.Visible;
                        layer.Opacity = parent.Opacity * layer.Opacity;
                        layer.DisplayName = parent.DisplayName;
                    }

                    flattenedLayers.Add(layer);
                }
            }

            return flattenedLayers;
        }

        /// <summary>
        /// Adds a suffix to a graphics layer indicating the type of geometry the layer contains
        /// </summary>
        /// <param name="layer">The layer to add the suffix to</param>
        /// <remarks>
        /// The method will not append the suffix of the layer name already ends with it, provided the 
        /// original name is stored in the WebMapDisplayName attached property.
        /// </remarks>
        internal static void AppendGeometryToLayerName(this GraphicsLayer layer)
        {
            string currentName = ESRI.ArcGIS.Client.Extensibility.MapApplication.GetLayerName(layer);
            string originalName = MapExtensions.GetWebMapDisplayName(layer);
            string nameAsResourceString = originalName != null ? currentName.Replace(originalName, "{0}") : currentName;
            if (nameAsResourceString != esriControlsResources.Strings.AreaNotesLayer
            && nameAsResourceString != esriControlsResources.Strings.LineNotesLayer
            && nameAsResourceString != esriControlsResources.Strings.PointNotesLayer)
            {
                string currentNameWithGeometry = currentName;
                GeometryType geometryType = LayerExtensions.GetGeometryType(layer);
                switch (geometryType)
                {
                    case GeometryType.Point:
                        currentNameWithGeometry = string.Format(esriControlsResources.Strings.PointNotesLayer, currentName);
                        break;
                    case GeometryType.Polyline:
                        currentNameWithGeometry = string.Format(esriControlsResources.Strings.LineNotesLayer, currentName);
                        break;
                    case GeometryType.Polygon:
                        currentNameWithGeometry = string.Format(esriControlsResources.Strings.AreaNotesLayer, currentName);
                        break;
                }

                MapExtensions.SetWebMapDisplayName(layer, currentName);
                ESRI.ArcGIS.Client.Extensibility.MapApplication.SetLayerName(layer, currentNameWithGeometry);
                layer.DisplayName = currentNameWithGeometry;
            }
        }

        /// <summary>
        /// Moves items from one list to another
        /// </summary>
        internal static void MoveTo(this IList source, IList target)
        {
            int count = source.Count;
            for (int i = 0; i < count; i++)
            {
                object item = source[0];
                source.RemoveAt(0);
                target.Add(item);
            }
        }

        private static ESRI.ArcGIS.Client.Geometry.MapPoint CreatePoint(IDictionary<string, object> gDict)
        {
            if (gDict.ContainsKey("x") && gDict.ContainsKey("y"))
            {
                bool parsedX, parsedY;
                double x, y;
                parsedX = double.TryParse(gDict["x"].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out x);
                parsedY = double.TryParse(gDict["y"].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out y);
                if (parsedX && parsedY)
                    return new MapPoint(x, y);
            }
            return null;
        }

        /// <summary>
        /// Gets whether a spatial reference instance is in a Web Mercator projection
        /// </summary>
        /// <param name="sref"></param>
        /// <returns></returns>
        private static bool IsWebMercator(this SpatialReference sref)
        {
            return sref != null && sref.WKID == 3857 || sref.WKID == 102113 || sref.WKID == 102100;
        }

    }
}
