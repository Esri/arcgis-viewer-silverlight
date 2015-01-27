/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using ESRI.ArcGIS.Mapping.DataSources;
using controls = ESRI.ArcGIS.Mapping.Controls;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Mapping.Behaviors;
using System.Text;
using System.Xml;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using ESRI.ArcGIS.Mapping.Core.Symbols;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public partial class View
    {
        private SymbologyInfo _symbology;
        private SymbologyInfo Symbology 
        {
            get
            {
                if(_symbology == null)
                    _symbology = new SymbologyInfo();

                return _symbology;
            }
        }

        private void SubscribeToSymbolConfigProviderEvents()
        {
            if (SymbolConfigProvider != null)
            {
                SymbolConfigProvider.GetDefaultLinearGradientBrushCompleted -= SymbolConfigProvider_GetDefaultLinearGradientBrushCompleted;
                SymbolConfigProvider.GetDefaultLinearGradientBrushFailed -= SymbolConfigProvider_GetDefaultLinearGradientBrushFailed;
                SymbolConfigProvider.GetDefaultSymbolCompleted -= SymbolConfigProvider_GetDefaultSymbolCompleted;
                SymbolConfigProvider.GetDefaultSymbolFailed -= SymbolConfigProvider_GetDefaultSymbolFailed;

                SymbolConfigProvider.GetDefaultLinearGradientBrushCompleted += SymbolConfigProvider_GetDefaultLinearGradientBrushCompleted;
                SymbolConfigProvider.GetDefaultLinearGradientBrushFailed += SymbolConfigProvider_GetDefaultLinearGradientBrushFailed;
                SymbolConfigProvider.GetDefaultSymbolCompleted += SymbolConfigProvider_GetDefaultSymbolCompleted;
                SymbolConfigProvider.GetDefaultSymbolFailed += SymbolConfigProvider_GetDefaultSymbolFailed;
            }
        }

        void SymbolConfigProvider_GetDefaultSymbolFailed(object sender, ExceptionEventArgs e)
        {
            GraphicsLayer layer = e.UserState as GraphicsLayer;
            if (layer == null)
                return;

            if (Core.LayerExtensions.GetRunLayerPostInitializationActions(layer))
            {
                PerformPostLayerInitializationActions(layer, true);
                Core.LayerExtensions.SetRunLayerPostInitializationActions(layer, false);
                return;
            }

            AddLayer(layer, true, null);
        }

        void SymbolConfigProvider_GetDefaultSymbolCompleted(object sender, GetDefaultSymbolCompletedEventArgs e)
        {
            Symbology.AddDefaultSymbolForGeometryType(e.GeometryType, e.DefaultSymbol);

            GraphicsLayer layer = e.UserState as GraphicsLayer;
            if (layer == null)
                return;

            if (Core.LayerExtensions.GetRunLayerPostInitializationActions(layer))
            {
                PerformPostLayerInitializationActions(layer, true);
                Core.LayerExtensions.SetRunLayerPostInitializationActions(layer, false);
                return;
            }

            AddLayer(layer, true, null);
        }

        void SymbolConfigProvider_GetDefaultLinearGradientBrushFailed(object sender, ExceptionEventArgs e)
        {
            GraphicsLayer layer = e.UserState as GraphicsLayer;
            if (layer == null)
                return;

            if (Core.LayerExtensions.GetRunLayerPostInitializationActions(layer))
            {
                PerformPostLayerInitializationActions(layer, true);
                Core.LayerExtensions.SetRunLayerPostInitializationActions(layer, false);
                return;
            }

            AddLayer(layer, true, null);
        }

        void SymbolConfigProvider_GetDefaultLinearGradientBrushCompleted(object sender, GetDefaultLinearGradientBrushEventArgs e)
        {
            Symbology.AddDefaultGradientBrush(e.ColorRampType, e.DefaultBrush);

            GraphicsLayer layer = e.UserState as GraphicsLayer;
            if (layer == null)
                return;

            GeometryType geometryType = Core.LayerExtensions.GetGeometryType(layer);
            SymbolConfigProvider.GetDefaultSymbol(geometryType, layer);
        }

        public void AddLayerToMap(Layer layer, bool isSelected, string layerDisplayName)
        {
            GraphicsLayer graphicsLayer = layer as GraphicsLayer;
            if (SymbolConfigProvider != null && graphicsLayer != null) // No symbology information available .. get symbology and add to map
            {
                GeometryType geometryType = Core.LayerExtensions.GetGeometryType(graphicsLayer);
                if (geometryType == GeometryType.Unknown)
                {
                    graphicsLayer.Renderer = new HiddenRenderer(); //ensure layer remains invisible and we will later ensure we get type from graphics
                    AddLayer(layer, isSelected, layerDisplayName);
                }
                else
                {
                    if (!Symbology.DefaultGradientBrushes.ContainsKey(ColorRampType.ClassBreaks) ||
                        !Symbology.DefaultSymbols.ContainsKey(geometryType))
                        SymbolConfigProvider.GetDefaultLinearGradientBrush(graphicsLayer, ColorRampType.ClassBreaks);
                    else
                        AddLayer(layer, isSelected, layerDisplayName);
                }
            }
            else
                AddLayer(layer, isSelected, layerDisplayName);
        }

        //private void ApplyLayerTransperancyBasedOfGeometryType(GraphicsLayer glayer, GeometryType geometryType)
        //{
        //    if (glayer != null)
        //    {
        //        switch (geometryType)
        //        {
        //            case GeometryType.Point:
        //                glayer.Opacity = 1.0;
        //                break;
        //            case GeometryType.Polyline:
        //                glayer.Opacity = 1.0;
        //                break;
        //            case GeometryType.Polygon:
        //                glayer.Opacity = 0.5; // for polygons, we make it semi-transparent
        //                break;
        //        }
        //    }
        //}
        private void ApplyDefaultRenderer(GraphicsLayer gLayer, GeometryType geometryType)
        {
            if (gLayer == null && Symbology == null)
                return;

            SymbolDescription desc = null;
            if (geometryType == GeometryType.MultiPoint) // Treat MultiPoint as point for looking up default symbol
                geometryType = GeometryType.Point;
            if (Symbology.DefaultSymbols.TryGetValue(geometryType, out desc))
            {                
                if (desc != null && desc.Symbol != null)
                {
                    Symbol symbol = desc.Symbol.CloneSymbol();
                    if (symbol != null)
                        gLayer.ChangeRenderer(symbol);
                }
            }
        }
        private void ApplyDefaultGradientBrush(GraphicsLayer gLayer)
        {
            if(Symbology.DefaultGradientBrushes.ContainsKey(ColorRampType.ClassBreaks))
            {
                System.Windows.Media.Brush brush = Symbology.DefaultGradientBrushes[ColorRampType.ClassBreaks].CloneBrush();
                if (brush != null)
                {
                    Core.LayerExtensions.SetGradientBrush(gLayer, brush as System.Windows.Media.LinearGradientBrush);
                }
            }
        }

        private void getLayerInfos(Layer layer)
        {
            if (layer == null)
                return;
            string layerUrl = IdentifySupport.GetLayerUrl(layer);
            if (!string.IsNullOrWhiteSpace(layerUrl))
            {
                MapServiceLayerInfoHelper helper = new MapServiceLayerInfoHelper(layerUrl, layer, IdentifySupport.GetLayerProxyUrl(layer));
                helper.GetLayerInfosCompleted += (s, e) =>
                {
                    Collection<LayerInformation> gpResultMapServerLayerInfos = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetGPResultMapServerLayerInfos(layer);
                    Collection<LayerInformation> layerInfos = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetLayerInfos(layer);
                    if (gpResultMapServerLayerInfos != null)
                    {
                        Collection<int> layerIds = new Collection<int>();
                        foreach (LayerInformation layerInfo in layerInfos)
                        {
                            LayerInformation gpResultMapServerLayerInfo = gpResultMapServerLayerInfos.FirstOrDefault(p => p.ID == layerInfo.ID);
                            if (gpResultMapServerLayerInfo != null)
                            {
                                layerInfo.PopUpsEnabled = gpResultMapServerLayerInfo.PopUpsEnabled;
                                if (!string.IsNullOrEmpty(gpResultMapServerLayerInfo.DisplayField)) layerInfo.DisplayField = gpResultMapServerLayerInfo.DisplayField;
                                if (layerInfo.PopUpsEnabled) layerIds.Add(layerInfo.ID);

                                foreach (FieldInfo field in layerInfo.Fields)
                                {
                                    FieldInfo gpField = gpResultMapServerLayerInfo.Fields.FirstOrDefault(p => p.Name == field.Name);
                                    if (gpField != null)
                                    {
                                        field.DisplayName = gpField.DisplayName;
                                        field.VisibleOnMapTip = gpField.VisibleOnMapTip;
                                    }
                                }
                            }
                        }
                        ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetIdentifyLayerIds(layer, layerIds);

                    }
                };
                helper.GetLayerInfos(null);
            }
        }

        private void PerformPostLayerInitializationActions(Layer layer, bool initializationSuccess)
        {
            GraphicsLayer gLayer = layer as GraphicsLayer;
            if (gLayer != null)
            {
                GeometryType geometryType = Core.LayerExtensions.GetGeometryType(gLayer);
                Collection<FieldInfo> layerFields = Core.LayerExtensions.GetFields(gLayer);
                FeatureLayer featureLayer = layer as FeatureLayer;
                if (layerFields.Count == 0 &&
                    featureLayer != null && featureLayer.LayerInfo != null && featureLayer.LayerInfo.Fields != null)
                {
                    foreach (ESRI.ArcGIS.Client.Field field in featureLayer.LayerInfo.Fields)
                    {
                        if (FieldHelper.IsFieldFilteredOut(field.Type))
                            continue;
                        ESRI.ArcGIS.Mapping.Core.FieldInfo fieldInfo = ESRI.ArcGIS.Mapping.Core.FieldInfo.FieldInfoFromField(featureLayer, field);
                        layerFields.Add(fieldInfo);
                    }
                }
                if (gLayer.Graphics != null)
                {
                    #region Get geometry type, start getting symbology
                    if (geometryType == GeometryType.Unknown && gLayer.Graphics.Count > 0)
                    {
                        geometryType = LayerUtils.GetGeometryTypeFromGraphic(gLayer.Graphics.ElementAtOrDefault(0));
                        Core.LayerExtensions.SetGeometryType(gLayer, geometryType);

                        if ((gLayer.Renderer == null || gLayer.Renderer is HiddenRenderer) && !Symbology.DefaultSymbols.ContainsKey(geometryType))
                        {
                            if (geometryType == GeometryType.Unknown)
                            {
                                gLayer.SetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.ErrorMessageProperty, "Layer has unspecified geometry type.");
                            }
                            else
                            {
                                Core.LayerExtensions.SetRunLayerPostInitializationActions(gLayer, true);
                                SymbolConfigProvider.GetDefaultLinearGradientBrush(gLayer, ColorRampType.ClassBreaks);
                            }
                            return;
                        }
                    }
                    #endregion

                    #region Project graphics if necessary
                    if (graphicsRequireReprojection(gLayer.Graphics))
                    {
                        GeometryServiceOperationHelper helper = new GeometryServiceOperationHelper(
                             new ConfigurationStoreHelper().GetGeometryServiceUrl(ConfigurationStore)
                            );
                        helper.ProjectGraphicsCompleted += (sender, args) =>
                        {
                            GraphicsLayer targetLayer = args.UserState as GraphicsLayer;
                            if (targetLayer != null)
                            {
                                targetLayer.Graphics.Clear();
                                foreach (Graphic graphic in args.Graphics)
                                    targetLayer.Graphics.Add(graphic);
                            }
                        };
                        helper.ProjectGraphics(gLayer.Graphics, Map.SpatialReference, gLayer);
                    }
                    #endregion

                    #region Get field information
                    if (layerFields.Count == 0) // fields not determined yet
                    {
                        determineFieldsFromGraphic(layerFields, gLayer.Graphics.ElementAtOrDefault(0));
                    }
                    #endregion
                }

                #region Get renderer from feature layer's layer info, if necessary
                if (gLayer.Renderer == null || gLayer.Renderer is HiddenRenderer)
                {
                    FeatureLayer lay = gLayer as FeatureLayer;
                    if (lay != null && lay.LayerInfo != null && lay.LayerInfo.Renderer != null)
                    {
                        lay.Renderer = lay.LayerInfo.Renderer;
                    }
                }
                #endregion

                #region Change PictureMarkerSymbol to ImageFillSymbol
                if (gLayer.Renderer != null && (geometryType == GeometryType.Point || geometryType == GeometryType.MultiPoint))
                {
                    SimpleRenderer sr = gLayer.Renderer as SimpleRenderer;
                    ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol pms = null;
                    if (sr != null)
                    {
                        pms = sr.Symbol as ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol;
                        if (pms != null)
                            sr.Symbol = SymbolJsonHelper.ToImageFillSymbol(pms);
                    }
                    else
                    {
                        ClassBreaksRenderer cbr = gLayer.Renderer as ClassBreaksRenderer;
                        if (cbr != null)
                        {
                            foreach (ClassBreakInfo info in cbr.Classes)
                            {
                                pms = info.Symbol as ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol;
                                if (pms != null)
                                    info.Symbol = SymbolJsonHelper.ToImageFillSymbol(pms);
                            }
                        }
                        else
                        {
                            UniqueValueRenderer uvr = gLayer.Renderer as UniqueValueRenderer;
                            if (uvr != null)
                            {
                                foreach (UniqueValueInfo info in uvr.Infos)
                                {
                                    pms = info.Symbol as ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol;
                                    if (pms != null)
                                        info.Symbol = SymbolJsonHelper.ToImageFillSymbol(pms);
                                }
                            }
                        }
                    }
                }
                #endregion

                if (gLayer.Renderer == null || gLayer.Renderer is HiddenRenderer)
                    ApplyDefaultRenderer(gLayer, geometryType);

                ApplyDefaultGradientBrush(gLayer);
            }
            else if ((layer is ArcGISDynamicMapServiceLayer || layer is ArcGISTiledMapServiceLayer)
                && !((bool)layer.GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty)))
            {
                //get layer infos - used later for figuring out domain/subtypes, etc
                if ((layer.GetValue(ESRI.ArcGIS.Mapping.Core.LayerExtensions.LayerInfosProperty) as Collection<LayerInformation>)
                    == null)
                    getLayerInfos(layer);
            }
            bool doSelect = false;
            layer.SetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.InitialUpdateCompletedProperty, true);
            if (!initializationSuccess)
                layer.SetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.InitialUpdateFailedProperty, true);
            else
            {
                bool hasId = !string.IsNullOrEmpty(layer.ID) || !string.IsNullOrEmpty(layer.GetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty) as  string);
                // Certain layers get added when the map draw mode is changed (An empty graphics layer is added)
                // We don't want to auto-select this layer
                if (hasId || !(layer is GraphicsLayer))
                    doSelect = true;
            }

            if (doSelect)
                SetSelectedLayer(new LayerEventArgs() { Layer = layer });

            SubscribeToLayerInitializationEvents(layer, false);
        }

        private bool graphicsRequireReprojection(GraphicCollection graphicCollection)
        {
            if (Map.SpatialReference == null)
                return false;

            bool requiresProjection = false;
            if (graphicCollection != null)
            {
                foreach (Graphic graphic in graphicCollection)
                {
                    if (graphic.Geometry == null || graphic.Geometry.SpatialReference == null)
                        continue;
                    if (!Map.SpatialReference.Equals(graphic.Geometry.SpatialReference))
                    {
                        requiresProjection = true;
                        break;
                    }
                }
            }
            return requiresProjection;
        }

        private void determineFieldsFromGraphic(Collection<FieldInfo> layerFields, Graphic graphic)
        {
            if (graphic == null || layerFields == null)
                return;

            foreach (KeyValuePair<string, object> pair in graphic.Attributes)
            {
                FieldInfo field = new FieldInfo() { Name = pair.Key, DisplayName = pair.Key, VisibleInAttributeDisplay = true, VisibleOnMapTip = true };                
                if (pair.Value is DateTime)
                    field.FieldType = FieldType.DateTime;
                else if (pair.Value is double || pair.Value is decimal)
                    field.FieldType = FieldType.DecimalNumber;
                else if (pair.Value is int || pair.Value is byte)
                    field.FieldType = FieldType.Integer;
                else
                    field.FieldType = FieldType.Text;
                layerFields.Add(field);
            }
        }

        private void layerUpdateFailed(object sender, EventArgs e)
        {
            Layer layer = sender as Layer;
            layer.SetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.ErrorMessageProperty, "Layer could not be updated.");
            PerformPostLayerInitializationActions(layer, false);
        }

        private void layerInitFailed(object sender, EventArgs e)
        {
            Layer layer = sender as Layer;
            layer.SetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.ErrorMessageProperty, "Layer could not be initialized.");
            PerformPostLayerInitializationActions(layer, false);
        }

        private void layerUpdateCompleted(object sender, EventArgs e)
        {
            PerformPostLayerInitializationActions(sender as Layer, true);
        }

        private void SubscribeToLayerInitializationEvents(Layer layer, bool subscribe)
        {
            if (layer == null)
                return;

            if (subscribe)
            {
                //unsubscribe from events
                CustomGraphicsLayer cgl = layer as CustomGraphicsLayer;
                if (cgl != null)
                {
                    cgl.UpdateFailed += layerUpdateFailed;
                    cgl.UpdateCompleted += layerUpdateCompleted;
                }
                else
                {
                    FeatureLayer fl = layer as FeatureLayer;
                    if (fl != null)
                    {
                        fl.UpdateFailed += layerUpdateFailed;
                        fl.UpdateCompleted += layerUpdateCompleted;
                    }
                    else
                    {
                        layer.InitializationFailed += layerInitFailed;
                        layer.Initialized += layerUpdateCompleted;
                    }
                }
            }
            else
            {
                //unsubscribe from events
                CustomGraphicsLayer cgl = layer as CustomGraphicsLayer;
                if (cgl != null)
                {
                    cgl.UpdateFailed -= layerUpdateFailed;
                    cgl.UpdateCompleted -= layerUpdateCompleted;
                }
                else
                {
                    FeatureLayer fl = layer as FeatureLayer;
                    if (fl != null)
                    {
                        fl.UpdateFailed -= layerUpdateFailed;
                        fl.UpdateCompleted -= layerUpdateCompleted;
                    }
                    else
                    {
                        layer.InitializationFailed -= layerInitFailed;
                        layer.Initialized -= layerUpdateCompleted;
                    }
                }
            }
        }

        private bool IsLayerInitialized(Layer layer)
        {
            CustomGeoRssLayer geoRssLayer = layer as CustomGeoRssLayer;
            if (geoRssLayer != null) // GeoRss layers use graphics layer member composition - hence we need a seperate init property
                return geoRssLayer.IsInitComplete;
            GraphicsLayer gLayer = layer as GraphicsLayer;
            if (gLayer != null)
                return gLayer.IsInitialized && gLayer.Graphics != null;
            else
                return layer.IsInitialized;
        }
    }
}
