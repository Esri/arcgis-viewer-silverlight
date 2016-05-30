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
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Utils;
using Tasks = ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Geometry;
using Extensibility = ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Core
{
    public static class LayerExtensions
    {
        public static readonly DependencyProperty ExcludeSerializationProperty = DependencyProperty.RegisterAttached("ExcludeSerializationProperty", typeof(bool), typeof(Layer), null);

        /// <summary>
        /// Default Auto-Update interval in milliseconds
        /// </summary>
        public static readonly double DefaultAutoUpdateInterval = 30000d;
        public static void SetLayerName(ESRI.ArcGIS.Client.Layer layer, string layerName)
        {
            MapApplication.SetLayerName(layer, layerName);
        }
        public static string GetLayerName(ESRI.ArcGIS.Client.Layer layer)
        {
            return MapApplication.GetLayerName(layer);
        }

        #region DisplayUrl
        public static readonly DependencyProperty DisplayUrlProperty = DependencyProperty.RegisterAttached("DisplayUrl", typeof(string), typeof(Layer), null);
        public static void SetDisplayUrl(Layer layer, string value)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            layer.SetValue(DisplayUrlProperty, value);
        }
        public static string GetDisplayUrl(Layer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            return (string)layer.GetValue(DisplayUrlProperty);
        }
        #endregion

        #region Geometry Type
        public static readonly DependencyProperty GeometryTypeProperty = DependencyProperty.RegisterAttached("GeometryType", typeof(GeometryType), typeof(GraphicsLayer), null);
        public static void SetGeometryType(GraphicsLayer graphicsLayer, GeometryType value)
        {
            if (graphicsLayer == null)
            {
                throw new ArgumentNullException("graphicsLayer");
            }
            graphicsLayer.SetValue(GeometryTypeProperty, value);
        }
        public static GeometryType GetGeometryType(GraphicsLayer graphicsLayer)
        {
            if (graphicsLayer == null)
            {
                throw new ArgumentNullException("graphicsLayer");
            }
            return (GeometryType)graphicsLayer.GetValue(GeometryTypeProperty);
        }
        #endregion

        #region IsMapTipDirty
        public static bool GetIsMapTipDirty(GraphicsLayer graphicsLayer)
        {
            return (bool)graphicsLayer.GetValue(IsMapTipDirtyProperty);
        }

        public static void SetIsMapTipDirty(GraphicsLayer graphicsLayer, bool value)
        {
            graphicsLayer.SetValue(IsMapTipDirtyProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsMapTipDirty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMapTipDirtyProperty =
            DependencyProperty.RegisterAttached("IsMapTipDirty", typeof(bool), typeof(GraphicsLayer), new PropertyMetadata(false));
        #endregion

        #region GPResultMapServer LayerInfos
        public static Collection<LayerInformation> GetGPResultMapServerLayerInfos(DependencyObject obj)
        {
            return (Collection<LayerInformation>)obj.GetValue(GPResultMapServerLayerInfosProperty);
        }

        public static void SetGPResultMapServerLayerInfos(DependencyObject obj, Collection<LayerInformation> value)
        {
            obj.SetValue(GPResultMapServerLayerInfosProperty, value);
        }

        public static readonly DependencyProperty GPResultMapServerLayerInfosProperty =
            DependencyProperty.RegisterAttached("GPResultMapServerLayerInfos", typeof(Collection<LayerInformation>), typeof(Layer), null);

        #endregion

        #region LayerInfos
        public static Collection<LayerInformation> GetLayerInfos(Layer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }

            Collection<LayerInformation> layerInfos = layer.GetValue(LayerInfosProperty) as Collection<LayerInformation>;
            if (layerInfos == null)
            {
                // Initialize on demand
                layerInfos = new Collection<LayerInformation>();
                SetLayerInfos(layer, layerInfos);
            }

            return layerInfos;
        }

        public static void SetLayerInfos(Layer layer, Collection<LayerInformation> value)
        {
            layer.SetValue(LayerInfosProperty, value);
        }

        // Using a DependencyProperty as the backing store for LayerInfos.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayerInfosProperty =
            DependencyProperty.RegisterAttached("LayerInfos", typeof(Collection<LayerInformation>), typeof(Layer), new PropertyMetadata(null));
        #endregion

        #region IdentifyLayerIds
        public static Collection<int> GetIdentifyLayerIds(Layer layer)
        {

            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }

            Collection<int> layerIds = layer.GetValue(IdentifyLayerIdsProperty) as Collection<int>;
            if (layerIds == null)
            {
                // Initialize on demand
                layerIds = new Collection<int>();
                SetIdentifyLayerIds(layer, layerIds);
            }

            return layerIds;
        }

        public static void SetIdentifyLayerIds(Layer layer, Collection<int> value)
        {
            layer.SetValue(IdentifyLayerIdsProperty, value);
        }

        // Using a DependencyProperty as the backing store for IdentifyLayerIds.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdentifyLayerIdsProperty =
            DependencyProperty.RegisterAttached("IdentifyLayerIds", typeof(Collection<int>), typeof(Layer), new PropertyMetadata(null));
        #endregion

        #region PopUpsOnClick
        public static readonly DependencyProperty PopUpsOnClickProperty = DependencyProperty.RegisterAttached("PopUpsOnClick", typeof(bool), typeof(GraphicsLayer), new PropertyMetadata(true));
        public static void SetPopUpsOnClick(GraphicsLayer layer, bool value)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            layer.SetValue(PopUpsOnClickProperty, value);
        }
        public static bool GetPopUpsOnClick(GraphicsLayer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            return (bool)layer.GetValue(PopUpsOnClickProperty);
        }
        #endregion

        #region PopupInfo - for backwards compatibility 
        public static string GetPopupInfo(Layer obj)
        {
            return (string)obj.GetValue(PopupInfoProperty);
        }

        public static void SetPopupInfo(Layer obj, string value)
        {
            obj.SetValue(PopupInfoProperty, value);
        }

        // Using a DependencyProperty as the backing store for PopupInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupInfoProperty =
            DependencyProperty.RegisterAttached("PopupInfo", typeof(string), typeof(Layer), new PropertyMetadata(onPopupInfoChange));

        static void onPopupInfoChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            Layer layer = o as Layer;
            if (layer != null)
            {
                string popupInfoJSON = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetPopupInfo(layer);
                if (!string.IsNullOrWhiteSpace(popupInfoJSON))
                {
                    Dictionary<int, string> popupTemplates = new Dictionary<int, string>();
                    Dictionary<int, string> titleExpressions = new Dictionary<int, string>();
                    if (layer is GraphicsLayer)
                    {
                        #region get popup info for the layer
                        var popupInfo = PopupInfo.FromJson(popupInfoJSON);
                        if (popupInfo != null)
                        {
                            popupTemplates.Add(-1, popupInfo.PopupInfoTemplateXaml);
                            titleExpressions.Add(-1, popupInfo.Title);
                        }
                        #endregion
                    }
                    else
                    {
                        #region Get popup info for sublayers
                        var popupInfos = PopupInfo.DictionaryFromJson(popupInfoJSON);
                        if (popupInfos != null && popupInfos.Count > 0)
                        {
                            foreach (var item in popupInfos)
                            {
                                if (item.Value != null)
                                {
                                    int id;
                                    if (int.TryParse(item.Key, out id))
                                    {
                                        string title = item.Value.Title;
                                        string template = item.Value.PopupInfoTemplateXaml;
                                        if (template != null)
                                            popupTemplates.Add(id, template);
                                        titleExpressions.Add(id, title);
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    SetWebMapPopupDataTemplates(layer, popupTemplates);
                    SetWebMapPopupTitleExpressions(layer, titleExpressions);
                }
                else
                {
                    SetWebMapPopupDataTemplates(layer, null);
                    SetWebMapPopupTitleExpressions(layer, null);
                }
            }
        }
        #endregion

        #region UsePopupFromWebMap
        public static bool GetUsePopupFromWebMap(Layer obj)
        {
            return (bool)obj.GetValue(UsePopupFromWebMapProperty);
        }

        public static void SetUsePopupFromWebMap(Layer obj, bool value)
        {
            obj.SetValue(UsePopupFromWebMapProperty, value);
        }

        // Using a DependencyProperty as the backing store for UsePopupFromWebMap.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsePopupFromWebMapProperty =
            DependencyProperty.RegisterAttached("UsePopupFromWebMap", typeof(bool), typeof(Layer), new PropertyMetadata(false));
        #endregion

        #region Fields
        public static readonly DependencyProperty FieldsProperty = DependencyProperty.RegisterAttached("Fields", typeof(Collection<FieldInfo>), typeof(GraphicsLayer), new PropertyMetadata(null));
        public static void SetFields(GraphicsLayer graphicsLayer, Collection<FieldInfo> value)
        {
            graphicsLayer.SetValue(FieldsProperty, value);
            SetIsMapTipDirty(graphicsLayer, true);
        }
        public static Collection<FieldInfo> GetFields(GraphicsLayer graphicsLayer)
        {
            if (graphicsLayer == null)
            {
                throw new ArgumentNullException("graphicsLayer");
            }

            Collection<FieldInfo> fields = graphicsLayer.GetValue(FieldsProperty) as Collection<FieldInfo>;
            if (fields == null)
            {
                // Initialize on demand
                fields = new Collection<FieldInfo>();
                SetFields(graphicsLayer, fields);
            }

            return fields;
        }
        #endregion

        #region DisplayField
        public static string GetDisplayField(GraphicsLayer graphicsLayer)
        {
            return (string)graphicsLayer.GetValue(DisplayFieldProperty);
        }

        public static void SetDisplayField(GraphicsLayer graphicsLayer, string value)
        {
            graphicsLayer.SetValue(DisplayFieldProperty, value);
            SetIsMapTipDirty(graphicsLayer, true);
        }

        // Using a DependencyProperty as the backing store for DisplayField.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayFieldProperty =
            DependencyProperty.RegisterAttached("DisplayField", typeof(string), typeof(GraphicsLayer),null);
        #endregion

        #region GradientBrush
        /// <summary>
        /// Gets the value of the GradientBrush attached property for a specified GraphicsLayer.
        /// </summary>
        /// <param name="graphicsLayer">The GraphicsLayer from which the property value is read.</param>
        /// <returns>The GradientBrush property value for the GraphicsLayer.</returns>
        public static LinearGradientBrush GetGradientBrush(GraphicsLayer graphicsLayer)
        {
            if (graphicsLayer == null)
            {
                throw new ArgumentNullException("graphicsLayer");
            }
            return graphicsLayer.GetValue(GradientBrushProperty) as LinearGradientBrush;
        }

        /// <summary>
        /// Sets the value of the GradientBrush attached property to a specified GraphicsLayer.
        /// </summary>
        /// <param name="graphicsLayer">The GraphicsLayer to which the attached property is written.</param>
        /// <param name="value">The needed GradientBrush value.</param>
        public static void SetGradientBrush(GraphicsLayer graphicsLayer, LinearGradientBrush value)
        {
            if (graphicsLayer == null)
            {
                throw new ArgumentNullException("graphicsLayer");
            }
            graphicsLayer.SetValue(GradientBrushProperty, value);
        }

        /// <summary>
        /// Identifies the GradientBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty GradientBrushProperty =
            DependencyProperty.RegisterAttached(
                "GradientBrush",
                typeof(LinearGradientBrush),
                typeof(LayerExtensions),
                new PropertyMetadata(null));
        #endregion

        #region IsConfigurable
        //TODO - currently configurable defaults to true, as we have no infrastructure of setting this
        public static readonly DependencyProperty IsConfigurableProperty = DependencyProperty.RegisterAttached("IsConfigurable", typeof(bool), typeof(Layer), new PropertyMetadata(true));
        public static void SetIsConfigurable(Layer layer, bool value)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            layer.SetValue(IsConfigurableProperty, value);
        }
        public static bool GetIsConfigurable(Layer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            return (bool)layer.GetValue(IsConfigurableProperty);
        }
        #endregion

        #region RendererAttributeDisplayName
        /// <summary>
        /// Gets the value of the RendererAttributeDisplayName attached property for a specified GraphicsLayer.
        /// </summary>
        /// <param name="element">The GraphicsLayer from which the property value is read.</param>
        /// <returns>The RendererAttributeDisplayName property value for the GraphicsLayer.</returns>
        public static string GetRendererAttributeDisplayName(GraphicsLayer element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return element.GetValue(RendererAttributeDisplayNameProperty) as string;
        }

        /// <summary>
        /// Sets the value of the RendererAttributeDisplayName attached property to a specified GraphicsLayer.
        /// </summary>
        /// <param name="element">The GraphicsLayer to which the attached property is written.</param>
        /// <param name="value">The needed RendererAttributeDisplayName value.</param>
        public static void SetRendererAttributeDisplayName(GraphicsLayer element, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(RendererAttributeDisplayNameProperty, value);
        }

        /// <summary>
        /// Identifies the RendererAttributeDisplayName dependency property.
        /// </summary>
        public static readonly DependencyProperty RendererAttributeDisplayNameProperty =
            DependencyProperty.RegisterAttached(
                "RendererAttributeDisplayName",
                typeof(string),
                typeof(LayerExtensions),
                new PropertyMetadata(null));
        #endregion

        #region RunLayerPostInitializationActions
        public static readonly DependencyProperty RunLayerPostInitializationActionsProperty = DependencyProperty.RegisterAttached("RunLayerPostInitializationActions", typeof(bool), typeof(GraphicsLayer), null);
        public static void SetRunLayerPostInitializationActions(GraphicsLayer layer, bool value)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            if (value)
                layer.SetValue(RunLayerPostInitializationActionsProperty, value);
            else
                layer.ClearValue(RunLayerPostInitializationActionsProperty);
        }
        public static bool GetRunLayerPostInitializationActions(Layer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            return (bool)layer.GetValue(RunLayerPostInitializationActionsProperty);
        }
        #endregion

        #region AutoUpdateInterval

        /// <summary>   
        /// Auto-Update refresh interval always in milliseconds
        /// </summary>
        public static readonly DependencyProperty AutoUpdateIntervalProperty =
            DependencyProperty.RegisterAttached("AutoUpdateInterval", typeof(double), typeof(Layer),
                                                new PropertyMetadata(0.0d, null));

        /// <summary>
        /// Gets the AutoUpdateInterval property. This dependency property 
        /// indicates the wait time between auto-refreshes (in milliseconds).
        /// </summary>
        public static double GetAutoUpdateInterval(DependencyObject d)
        {
            return (double)d.GetValue(AutoUpdateIntervalProperty);
        }

        /// <summary>
        /// Sets the AutoUpdateInterval property. This dependency property 
        /// indicates the wait time between auto-refreshes (in milliseconds).
        /// </summary>
        public static void SetAutoUpdateInterval(DependencyObject d, double value)
        {
            double existingValue = (double)d.GetValue(AutoUpdateIntervalProperty);
            // prevent setting the value when equal to existing value
            if (Math.Abs(existingValue - value) > Double.Epsilon)
            {
            	d.SetValue(AutoUpdateIntervalProperty, value);
            }
        }

        #endregion

        #region AutoUpdateOnExtentChanged

        /// <summary>   
        /// Auto-Update refresh if the map extent changes
        /// </summary>
        public static readonly DependencyProperty AutoUpdateOnExtentChangedProperty =
            DependencyProperty.RegisterAttached("AutoUpdateOnExtentChanged", typeof(bool), typeof(Layer),
                                                new PropertyMetadata(false, null));

        /// <summary>
        /// Gets the AutoUpdateOnExtentChanged property. This dependency property 
        /// indicates if update should occur when map extent changes.
        /// </summary>
        public static bool GetAutoUpdateOnExtentChanged(DependencyObject d)
        {
            return (bool)d.GetValue(AutoUpdateOnExtentChangedProperty);
        }

        /// <summary>
        /// Sets the AutoUpdateOnExtentChanged property. This dependency property 
        /// indicates if update should occur when map extent changes.
        /// </summary>
        public static void SetAutoUpdateOnExtentChanged(DependencyObject d, bool value)
        {
            bool existingValue = (bool)d.GetValue(AutoUpdateOnExtentChangedProperty);
            // prevent setting the value when equal to existing value
            if (existingValue != value)
            {
                d.SetValue(AutoUpdateOnExtentChangedProperty, value);
            }
        }

        #endregion

        public static void SetLayerProperties(Layer layer, Map map, string layerDisplayName, bool initialUpdateCompleted)
        {
            if (!string.IsNullOrEmpty(layerDisplayName))
            {
                layer.SetValue(MapApplication.LayerNameProperty, layerDisplayName);
            }
            layer.SetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.InitialUpdateCompletedProperty, initialUpdateCompleted);
        }

        #region Dataset
        /// <summary>
        /// Gets the value of the Dataset attached property for a specified GraphicsLayer.
        /// </summary>
        /// <param name="layer">The GraphicsLayer from which the property value is read.</param>
        /// <returns>The Dataset property value for the GraphicsLayer.</returns>
        public static string GetDataset(GraphicsLayer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("element");
            }
            return layer.GetValue(DatasetProperty) as string;
        }

        /// <summary>
        /// Sets the value of the Dataset attached property to a specified GraphicsLayer.
        /// </summary>
        /// <param name="layer">The GraphicsLayer to which the attached property is written.</param>
        /// <param name="value">The needed Dataset value.</param>
        public static void SetDataset(GraphicsLayer layer, string value)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("element");
            }
            layer.SetValue(DatasetProperty, value);
        }

        /// <summary>
        /// Identifies the Dataset dependency property.
        /// </summary>
        public static readonly DependencyProperty DatasetProperty =
            DependencyProperty.RegisterAttached(
                "Dataset",
                typeof(string),
                typeof(LayerExtensions),
                new PropertyMetadata(null));
        #endregion

        #region IsReferenceLayer
        public static readonly DependencyProperty IsReferenceProperty =
            DependencyProperty.RegisterAttached("IsReferenceLayer", typeof(bool), typeof(ESRI.ArcGIS.Client.Layer), new PropertyMetadata(false));
        public static void SetIsReferenceLayer(ESRI.ArcGIS.Client.Layer layer, bool value)
        {
            layer.SetValue(IsReferenceProperty, value);
        }
        public static bool GetIsReferenceLayer(ESRI.ArcGIS.Client.Layer o)
        {
            return (bool)o.GetValue(IsReferenceProperty);
        }
        #endregion


        //OBSOLETE - left for backwards compatibility for Sharepoint
        #region Title
        public static void SetTitle(ESRI.ArcGIS.Client.Layer layer, string layerName)
        {
            MapApplication.SetLayerName(layer, layerName);
        }
        public static string GetTitle(ESRI.ArcGIS.Client.Layer layer)
        {
            return  MapApplication.GetLayerName(layer);
        }
        #endregion

        public static void SetIsBaseMapLayer(ESRI.ArcGIS.Client.Layer layer, bool value)
        {
            layer.SetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty, value);
        }
        public static bool GetIsBaseMapLayer(ESRI.ArcGIS.Client.Layer o)
        {
            return (bool)o.GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty);
        }

        #region UsesProxy
        public static bool GetUsesProxy(Layer obj)
        {
            return (bool)obj.GetValue(UsesProxyProperty);
        }

        public static void SetUsesProxy(Layer obj, bool value)
        {
            obj.SetValue(UsesProxyProperty, value);
        }

        // Using a DependencyProperty as the backing store for UsesProxy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsesProxyProperty =
            DependencyProperty.RegisterAttached("UsesProxy", typeof(bool), typeof(Layer), new PropertyMetadata(false, onUsesProxyChanged));

        static void onUsesProxyChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            Layer layer = o as Layer;
            if (layer != null)
            {
                if (GetUsesProxy(layer))
                    ProxyUrlHelper.SetProxyUrl(layer);
                else
                    ProxyUrlHelper.SetProxyUrl(layer, null);
            }
        }
        #endregion




        public static bool GetUsesBingAppID(Client.Bing.TileLayer obj)
        {
            return (bool)obj.GetValue(UsesBingAppIDProperty);
        }

        public static void SetUsesBingAppID(Client.Bing.TileLayer obj, bool value)
        {
            obj.SetValue(UsesBingAppIDProperty, value);
        }

        // Using a DependencyProperty as the backing store for UsesBingAppID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsesBingAppIDProperty =
            DependencyProperty.RegisterAttached("UsesBingAppID", typeof(bool), typeof(Client.Bing.TileLayer), new PropertyMetadata(false, onUsesBingChanged));

        static void onUsesBingChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            Client.Bing.TileLayer layer = o as Client.Bing.TileLayer;
            if (layer != null)
            {
                if (GetUsesBingAppID(layer) && ConfigurationStore.Current != null)
                {
                    //Leave old token in place for backwards compatibility
                    if (!(string.IsNullOrEmpty(ConfigurationStore.Current.BingMapsAppId) &&
                        layer.Token == "AhSZtOGb51X9fKt5KT8Cxi4CkcMIvPYei7QmT0plKbUuZLQjgCU3CUz-7eCaoR7y"))
                             layer.Token = ConfigurationStore.Current.BingMapsAppId;
                }
            }
        }
		#region PopupDataTemplates

        public static string SerializeDictionary(IDictionary<int, string> dictionary)
        {
            if (dictionary == null)
                return null;
            StringBuilder sb = new StringBuilder();
            ESRI.ArcGIS.Mapping.Core.JsonWriter jw = new ESRI.ArcGIS.Mapping.Core.JsonWriter(new StringWriter(sb));
            bool wroteFirst = false;
            jw.StartObject();
            foreach (var item in dictionary)
            {
                if (item.Value != null)
                {
                    if (wroteFirst)
                        jw.Writer.Write(",");
                    jw.WriteProperty(item.Key.ToString(), item.Value);
                    wroteFirst = true;
                }
            }
            jw.EndObject();
            return sb.ToString();
        }

        public static IDictionary<int, string> DeserializeDictionary(string serialized)
        {
            if (string.IsNullOrEmpty(serialized))
                return null;
            JavaScriptSerializer jss = new JavaScriptSerializer();
            IDictionary<string, object> dictionary = jss.DeserializeObject(serialized) as IDictionary<string, object>;
            Dictionary<int, string> dict = new Dictionary<int, string>();
            foreach (var item in dictionary)
            {
                int id;
                if (int.TryParse(item.Key, out id))
                {
                    dict.Add(id, item.Value as string);
                }
            }
            return dict;
        }

		/// <summary>
		/// Layer popup template
		/// </summary>
		/// <param name="obj">Layer</param>
		/// <returns>DataTemplate</returns>
		public static string GetSerializedPopupDataTemplates(Layer obj)
		{
			if (obj == null) return null;
			IDictionary<int, string> dataTemplates = LayerProperties.GetPopupDataTemplates(obj);
			if (dataTemplates == null || dataTemplates.Count < 1) return null;
            return SerializeDictionary(dataTemplates);
		}

		/// <summary>
		/// Layer popup template
		/// </summary>
		/// <param name="obj">Layer</param>
		/// <param name="value">DataTemplate</param>
        public static void SetSerializedPopupDataTemplates(Layer obj, string value)
		{
            if (obj == null)
                return;
            obj.SetValue(SerializedPopupDataTemplatesProperty, value);
		}

		// Using a DependencyProperty as the backing store for PopupDataTemplateXaml. This enables programmatically changing the contents of a layer's Xaml.
		/// <summary>
		/// Attached property
		/// </summary>
		public static readonly DependencyProperty SerializedPopupDataTemplatesProperty =
            DependencyProperty.RegisterAttached("SerializedPopupDataTemplates", typeof(string), typeof(Layer), new PropertyMetadata(onSerializedPopupDataTemplatesChange));

        static void onSerializedPopupDataTemplatesChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            Layer layer = o as Layer;
            if (layer != null)
            {
                string value = args.NewValue as string;
                IDictionary<int, string> dataTemplates = DeserializeDictionary(value);
                LayerProperties.SetPopupDataTemplates(layer, dataTemplates);
            }
        }
		#endregion

		#region PopupTitleExpressions
		/// <summary>
		/// Layer popup expression
		/// </summary>
		/// <param name="obj">Layer</param>
		/// <returns>DataExpression</returns>
		public static string GetSerializedPopupTitleExpressions(Layer obj)
		{
            if (obj == null) return null;
            IDictionary<int, string> titles = LayerProperties.GetPopupTitleExpressions(obj);
            if (titles == null || titles.Count < 1) return null;
            return SerializeDictionary(titles);
		}

		/// <summary>
		/// Layer popup expression
		/// </summary>
		/// <param name="obj">Layer</param>
		/// <param name="value">DataExpression</param>
		public static void SetSerializedPopupTitleExpressions(Layer obj, string value)
		{
            if (obj == null)
                return;
            obj.SetValue(SerializedPopupTitleExpressionsProperty, value);
		}

		// Using a DependencyProperty as the backing store for PopupDataExpressionsXaml. This enables programmatically changing the contents of a layer's Xaml.
		/// <summary>
		/// Attached property
		/// </summary>
		public static readonly DependencyProperty SerializedPopupTitleExpressionsProperty =
            DependencyProperty.RegisterAttached("SerializedPopupTitleExpressions", typeof(string), typeof(Layer), new PropertyMetadata(onSerializedPopupTitleExpressionsChange));

        static void onSerializedPopupTitleExpressionsChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            Layer layer = o as Layer;
            if (layer != null)
            {
                string value = args.NewValue as string;
                IDictionary<int, string> dataTemplates = DeserializeDictionary(value);
                LayerProperties.SetPopupTitleExpressions(layer, dataTemplates);
            }
        }
		#endregion

        #region WebMap Popup Spec
        #region PopupDataTemplates
        #region Dictionary Props
        public static IDictionary<int, string> GetWebMapPopupDataTemplates(Layer obj)
        {
            return (IDictionary<int, string>)obj.GetValue(WebMapPopupDataTemplatesProperty);
        }

        public static void SetWebMapPopupDataTemplates(Layer obj, IDictionary<int, string> value)
        {
            obj.SetValue(WebMapPopupDataTemplatesProperty, value);
        }

        // Using a DependencyProperty as the backing store for WebMapPopupDataTemplates.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WebMapPopupDataTemplatesProperty =
            DependencyProperty.RegisterAttached("WebMapPopupDataTemplates", typeof(IDictionary<int, string>), typeof(Layer), null);
#endregion
        /// <summary>
        /// Layer popup template
        /// </summary>
        /// <param name="obj">Layer</param>
        /// <returns>DataTemplate</returns>
        public static string GetSerializedWebMapPopupDataTemplates(Layer obj)
        {
            if (obj == null) return null;
            IDictionary<int, string> dataTemplates = GetWebMapPopupDataTemplates(obj);
            if (dataTemplates == null || dataTemplates.Count < 1) return null;
            return SerializeDictionary(dataTemplates);
        }

        /// <summary>
        /// Layer popup template
        /// </summary>
        /// <param name="obj">Layer</param>
        /// <param name="value">DataTemplate</param>
        public static void SetSerializedWebMapPopupDataTemplates(Layer obj, string value)
        {
            if (obj == null)
                return;
            obj.SetValue(SerializedWebMapPopupDataTemplatesProperty, value);
        }
        // Using a DependencyProperty as the backing store for PopupDataTemplateXaml. This enables programmatically changing the contents of a layer's Xaml.
        /// <summary>
        /// Attached property
        /// </summary>
        public static readonly DependencyProperty SerializedWebMapPopupDataTemplatesProperty =
            DependencyProperty.RegisterAttached("SerializedWebMapPopupDataTemplates", typeof(string), typeof(Layer), new PropertyMetadata(onSerializedWebMapPopupDataTemplatesChange));

        static void onSerializedWebMapPopupDataTemplatesChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            Layer layer = o as Layer;
            if (layer != null)
            {
                string value = args.NewValue as string;
                IDictionary<int, string> dataTemplates = DeserializeDictionary(value);
                SetWebMapPopupDataTemplates(layer, dataTemplates);
            }
        }
        #endregion

        #region PopupTitleExpressions
        #region Dictionary Props
        public static IDictionary<int, string> GetWebMapPopupTitleExpressions(Layer obj)
        {
            return (IDictionary<int, string>)obj.GetValue(WebMapPopupTitleExpressionsProperty);
        }

        public static void SetWebMapPopupTitleExpressions(Layer obj, IDictionary<int, string> value)
        {
            obj.SetValue(WebMapPopupTitleExpressionsProperty, value);
        }

        // Using a DependencyProperty as the backing store for WebMapPopupTitleExpressions.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WebMapPopupTitleExpressionsProperty =
            DependencyProperty.RegisterAttached("WebMapPopupTitleExpressions", typeof(IDictionary<int, string>), typeof(Layer), null);
        #endregion
        /// <summary>
        /// Layer popup expression
        /// </summary>
        /// <param name="obj">Layer</param>
        /// <returns>DataExpression</returns>
        public static string GetSerializedWebMapPopupTitleExpressions(Layer obj)
        {
            if (obj == null) return null;
            IDictionary<int, string> titles = GetWebMapPopupTitleExpressions(obj);
            if (titles == null || titles.Count < 1) return null;
            return SerializeDictionary(titles);
        }

        /// <summary>
        /// Layer popup expression
        /// </summary>
        /// <param name="obj">Layer</param>
        /// <param name="value">DataExpression</param>
        public static void SetSerializedWebMapPopupTitleExpressions(Layer obj, string value)
        {
            if (obj == null)
                return;
            obj.SetValue(SerializedWebMapPopupTitleExpressionsProperty, value);
        }

        // Using a DependencyProperty as the backing store for PopupDataExpressionsXaml. This enables programmatically changing the contents of a layer's Xaml.
        /// <summary>
        /// Attached property
        /// </summary>
        public static readonly DependencyProperty SerializedWebMapPopupTitleExpressionsProperty =
            DependencyProperty.RegisterAttached("SerializedWebMapPopupTitleExpressions", typeof(string), typeof(Layer), new PropertyMetadata(onSerializedWebMapPopupTitleExpressionsChange));

        static void onSerializedWebMapPopupTitleExpressionsChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            Layer layer = o as Layer;
            if (layer != null)
            {
                string value = args.NewValue as string;
                IDictionary<int, string> titles = DeserializeDictionary(value);
                SetWebMapPopupTitleExpressions(layer, titles);
            }
        }
        #endregion

        #endregion

        public static void ProcessWebMapProperties(this Layer layer, IDictionary<string, object> webMapProperties = null)
        {
            Func<string, bool> isNullEmptyOrWhiteSpace = null;
            isNullEmptyOrWhiteSpace = delegate(string s) { return string.IsNullOrWhiteSpace(s); };
            // Assign unique layer ID
            if (isNullEmptyOrWhiteSpace(layer.ID))
                layer.ID = Guid.NewGuid().ToString("N");

            IDictionary<string, object> webMapData = layer.GetValue(ESRI.ArcGIS.Client.WebMap.Document.WebMapDataProperty) as IDictionary<string, object>;
            if (webMapData != null)
            {
                ArcGISDynamicMapServiceLayer dynamicMapServiceLayer = layer as ArcGISDynamicMapServiceLayer;
                if (dynamicMapServiceLayer != null && webMapData.ContainsKey("visibleLayers"))
                {
                    dynamicMapServiceLayer.Initialized -= dynamicMapServiceLayer_Initialized;
                    dynamicMapServiceLayer.Initialized += dynamicMapServiceLayer_Initialized;
                }
            }
            
            // If layer name attached property has already been set, do not override set values with
            // web map properties
            if (isNullEmptyOrWhiteSpace(MapApplication.GetLayerName(layer)))
            {
                #region Set layer name
                string title = null;
                if (webMapData != null)
                {
                    if (!string.IsNullOrEmpty(layer.DisplayName))
                        title = layer.DisplayName;
                    if (isNullEmptyOrWhiteSpace(title) && webMapData.ContainsKey("title"))
                        title = webMapData["title"] as string;
                    if (isNullEmptyOrWhiteSpace(title) && webMapData.ContainsKey("name"))
                        title = webMapData["name"] as string;
                    if (isNullEmptyOrWhiteSpace(title) && webMapData.ContainsKey("id"))
                        title = webMapData["id"] as string;

                    if (!isNullEmptyOrWhiteSpace(title))
                        layer.SetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty, title);
                }
                #endregion

                #region Set LayerName of basemap
                if ((bool)(layer.GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty)))
                {
                    layer.SetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty, true);
                    if (isNullEmptyOrWhiteSpace(layer.GetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty) as string))
                        layer.SetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty, "Basemap");
                }
                #endregion

                if (layer is GraphicsLayer)
                {
                    GraphicsLayer graphicsLayer = (GraphicsLayer)layer;
                    FeatureLayer featureLayer = layer as FeatureLayer;

                    if (webMapData != null) webMapData["layerID"] = layer.ID; // Used to find this layer later on when setting the popuptemplates

                    #region Popup Support
                    if (webMapData != null && webMapData.ContainsKey("popupInfo"))
                    {
                        var popupInfoDictionary = webMapData["popupInfo"] as IDictionary<string, object>;
                        SetPopupInfoTemplate(graphicsLayer, popupInfoDictionary);
                    }
                    #endregion

                    // Initialize geometry type for feature collection layers
                    if (featureLayer != null && isNullEmptyOrWhiteSpace(featureLayer.Url))
                    {
                        if (LayerExtensions.GetGeometryType(graphicsLayer) == GeometryType.Unknown)
                        {
                            if (featureLayer != null && featureLayer.LayerInfo != null)
                            {
                                if (featureLayer.LayerInfo.GeometryType == Tasks.GeometryType.Point)
                                    graphicsLayer.SetValue(LayerExtensions.GeometryTypeProperty, GeometryType.Point);
                                else if (featureLayer.LayerInfo.GeometryType == Tasks.GeometryType.MultiPoint)
                                    graphicsLayer.SetValue(LayerExtensions.GeometryTypeProperty, GeometryType.MultiPoint);
                                else if (featureLayer.LayerInfo.GeometryType == Tasks.GeometryType.Polyline)
                                    graphicsLayer.SetValue(LayerExtensions.GeometryTypeProperty, GeometryType.Polyline);
                                else if (featureLayer.LayerInfo.GeometryType == Tasks.GeometryType.Polygon)
                                    graphicsLayer.SetValue(LayerExtensions.GeometryTypeProperty, GeometryType.Polygon);
                            }
                            else if (graphicsLayer.Graphics != null && graphicsLayer.Graphics.Count > 0)
                            {
                                graphicsLayer.SetValue(LayerExtensions.GeometryTypeProperty,
                                    graphicsLayer.Graphics[0].ShapeType());
                            }
                        }

                        // Initialize fields
                        Collection<FieldInfo> fields = new Collection<FieldInfo>();
                        if (featureLayer.LayerInfo != null && featureLayer.LayerInfo.Fields != null)
                        {
                            foreach (Client.Field field in featureLayer.LayerInfo.Fields)
                            {
                                //TODO:don't send feature layer as domains/subtypes not supported for feature collection layers.
                                //Need to figure out a way to get layer Json or alternative to find domain/subtype information later
                                fields.Add(FieldInfo.FieldInfoFromField(null, field));
                            }
                        }
                        LayerExtensions.SetFields(graphicsLayer, fields);

                        // Fix data types to support objects that expect a consistent type between records 
                        // (e.g. DataGrid)
                        GraphicsLayerTypeFixer.CorrectDataTypes(graphicsLayer.Graphics, graphicsLayer);

                        if (graphicsLayer.Renderer == null)
                            graphicsLayer.Renderer = new ESRI.ArcGIS.Mapping.Core.Symbols.HiddenRenderer();

                        if (webMapProperties != null)
                        {
                            IDictionary<int, string> popupDataTemplates = LayerExtensions.GetWebMapPopupDataTemplates(graphicsLayer);
                            if (popupDataTemplates == null || popupDataTemplates.Count == 0)
                                LayerExtensions.SetPopupDataTemplate(webMapProperties, graphicsLayer);
                        }
                    }
                }
                else
                {
                    if (webMapData != null)
                    {
                        if (webMapData.ContainsKey("layers") && (layer is ArcGISDynamicMapServiceLayer || layer is ArcGISTiledMapServiceLayer))
                        {
                            IList<object> item = webMapData["layers"] as IList<object>;
                            if (item != null)
                            {
                                #region Popup Support
                                try
                                {
                                    Dictionary<int, string> popupTemplates = new Dictionary<int, string>();
                                    Dictionary<int, string> popupTitles = new Dictionary<int, string>();
                                    foreach (IDictionary<string, object> dict in item)
                                    {
                                        //Parse popup information section
                                        if (dict.ContainsKey("id") && dict.ContainsKey("popupInfo"))
                                        {
                                            var popupInfoDictionary = dict["popupInfo"] as IDictionary<string, object>;
                                            if (popupInfoDictionary != null)
                                            {
                                                int id;
                                                if (int.TryParse(dict["id"].ToString(), out id))
                                                {
                                                    var popupInfo = PopupInfo.FromDictionary(popupInfoDictionary);
                                                    string popupTemplate = popupInfo.GeneratePopupInfoTemplateString();
                                                    if (!string.IsNullOrWhiteSpace(popupTemplate))
                                                        popupTemplates.Add(id, popupTemplate);
                                                    string popupTitle = popupInfo.Title;
                                                    if (!string.IsNullOrWhiteSpace(popupTitle))
                                                        popupTitles.Add(id, popupTitle);
                                                }
                                            }
                                        }
                                    }

                                    if (popupTemplates.Count > 0 || popupTitles.Count > 0)
                                    {
                                        LayerExtensions.SetWebMapPopupDataTemplates(layer, popupTemplates);
                                        LayerExtensions.SetWebMapPopupTitleExpressions(layer, popupTitles);
                                        LayerExtensions.SetUsePopupFromWebMap(layer, true);
                                    }
                                }
                                catch (Exception)
                                {
                                    //Logger.Instance.LogError("Error getting popup info for layer");
                                }
                                #endregion
                            }
                        }
                    }
                }
            }
        }

        static void dynamicMapServiceLayer_Initialized(object sender, EventArgs e)
        {
            ArcGISDynamicMapServiceLayer dynamicMapServiceLayer = (ArcGISDynamicMapServiceLayer)sender;
            IDictionary<string, object> webMapData = dynamicMapServiceLayer.GetValue(ESRI.ArcGIS.Client.WebMap.Document.WebMapDataProperty) as IDictionary<string, object>;

            bool visible = dynamicMapServiceLayer.Visible;
            dynamicMapServiceLayer.Visible = false;
            List<object> visibleLayers = webMapData["visibleLayers"] as List<object>;
            if (visibleLayers != null)
            {
                foreach (LayerInfo layerInfo in dynamicMapServiceLayer.Layers)
                {
                    if (!visibleLayers.Contains(layerInfo.ID))
                        dynamicMapServiceLayer.SetLayerVisibility(layerInfo.ID, false);
                }
            }
            dynamicMapServiceLayer.Visible = visible;
        }

        public static void SetPopupDataTemplate(IDictionary<string, object> dict, GraphicsLayer featureCollectionLayer)
        {
            var operationalLayersList = dict.ContainsKey("operationalLayers") ? dict["operationalLayers"] as List<object> : null;
            if (operationalLayersList != null)
            {
                foreach (var operationalLayer in operationalLayersList)
                {
                    var operationalLayerDict = operationalLayer as IDictionary<string, object>;
                    if (operationalLayerDict != null)
                    {
                        var featureCollectionDict = operationalLayerDict.ContainsKey("featureCollection") ? operationalLayerDict["featureCollection"] as IDictionary<string, object> : null;
                        if (featureCollectionDict != null)
                        {
                            var layersList = featureCollectionDict.ContainsKey("layers") ? featureCollectionDict["layers"] as List<object> : null;
                            if (layersList != null)
                            {
                                foreach (var lyr in layersList)
                                {
                                    var layerDict = lyr as IDictionary<string, object>;
                                    if (layerDict != null)
                                    {
                                        var popupInfoDict = layerDict.ContainsKey("popupInfo") ? layerDict["popupInfo"] as IDictionary<string, object> : null;
                                        var layerDefinitionDict = layerDict.ContainsKey("layerDefinition") ? layerDict["layerDefinition"] as IDictionary<string, object> : null;
                                        if (popupInfoDict != null && layerDefinitionDict != null)
                                        {
                                            string layerID = layerDefinitionDict.ContainsKey("layerID") ? layerDefinitionDict["layerID"] as string : null;
                                            if (featureCollectionLayer.ID == layerID)
                                            {
                                                LayerExtensions.SetPopupInfoTemplate(featureCollectionLayer, popupInfoDict);
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void SetPopupInfoTemplate(GraphicsLayer fLayer, IDictionary<string, object> popupInfoDictionary)
        {
            try
            {
                var popupInfo = PopupInfo.FromDictionary(popupInfoDictionary);

                string popupTemplate = popupInfo.GeneratePopupInfoTemplateString();
                if (!string.IsNullOrWhiteSpace(popupTemplate))
                {
                    Dictionary<int, string> popupTemplates = new Dictionary<int, string>();
                    popupTemplates.Add(-1, popupTemplate);
                    LayerExtensions.SetWebMapPopupDataTemplates(fLayer, popupTemplates);
                }
                string popupTitle = popupInfo.Title;
                if (!string.IsNullOrWhiteSpace(popupTitle))
                {
                    Dictionary<int, string> popupTitles = new Dictionary<int, string>();
                    popupTitles.Add(-1, popupTitle);
                    LayerExtensions.SetWebMapPopupTitleExpressions(fLayer, popupTitles);
                }
                if (!string.IsNullOrWhiteSpace(popupTemplate) || !string.IsNullOrWhiteSpace(popupTitle))
                {
                    LayerExtensions.SetPopUpsOnClick(fLayer, true);
                    LayerExtensions.SetUsePopupFromWebMap(fLayer, true);
                }
            }
            catch
            {
            }
        }

        // Identifies the FeatureCollectionJson attached property
        public static readonly DependencyProperty FeatureCollectionJsonProperty =
            DependencyProperty.RegisterAttached("FeatureCollectionJson",
            typeof(string), typeof(FeatureLayer), null);

        /// <summary>
        /// Gets the stored feature collection JSON for the layer.
        /// </summary>
        /// <remarks>
        /// Note that this property DOES NOT listen to the bound layer for updates.  It simply returns
        /// whatever value has been set on the FeatureCollectionJson attached property.
        /// </remarks>
        public static string GetFeatureCollectionJson(FeatureLayer obj)
        {
            return obj.GetValue(FeatureCollectionJsonProperty) as string;
        }

        /// <summary>
        /// Sets the stored feature collection JSON for the layer
        /// </summary>
        /// <remarks>
        /// Note that this method has no effect on the bound layer, other than to set the string
        /// value of this property.
        /// </remarks>
        public static void SetFeatureCollectionJson(FeatureLayer obj, string value)
        {
            obj.SetValue(FeatureCollectionJsonProperty, value);
        }

        /// <summary>
        /// Gets the geometry type of the graphic
        /// </summary>
        public static GeometryType ShapeType(this Graphic graphic)
        {
            // Note that this method is not named GetGeometryType to avoid naming collision with 
            // GeometryType attached property

            if (graphic != null)
            {
                if (graphic.Geometry is MapPoint)
                    return GeometryType.Point;
                else if (graphic.Geometry is MultiPoint)
                    return GeometryType.MultiPoint;
                else if (graphic.Geometry is Polyline)
                    return GeometryType.Polyline;
                else if (graphic.Geometry is Polygon || graphic.Geometry is Envelope)
                    return GeometryType.Polygon;
            }
            return GeometryType.Unknown;
        }

        /// <summary>
        /// Retrieves feature collection JSON for a feature layer.
        /// </summary>
        /// <remarks>
        /// The JSON returned by this function can be used to instantiate a feature collection layer
        /// via FeatureLayer.FromJson.  Note that this method DOES NOT serialize features as this 
        /// capability is not needed at this time.
        /// </remarks>
        public static string GenerateFeatureCollectionJson(this FeatureLayer layer, 
            bool humanReadable = false)
        {
            if (layer.LayerInfo == null)
            {
                return null;
            }
            else
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                dictionary.Add("layerDefinition", layer.GetLayerInfoDictionary());
                string json = dictionary.ToJson();

                return json;
            }
        }

        /// <summary>
        /// Copies all copyable properties from the source layer to the target layer
        /// </summary>
        public static void CopyProperties(this FeatureLayer sourceLayer, FeatureLayer targetLayer)
        {
            targetLayer.AutoSave = sourceLayer.AutoSave;
            targetLayer.Clusterer = sourceLayer.Clusterer;
            targetLayer.DisableClientCaching = sourceLayer.DisableClientCaching;
            targetLayer.DisplayName = sourceLayer.DisplayName;
            targetLayer.EditUserName = sourceLayer.EditUserName;
            targetLayer.GdbVersion = sourceLayer.GdbVersion;
            targetLayer.GraphicsSource = sourceLayer.GraphicsSource;
            targetLayer.ID = sourceLayer.ID;
            targetLayer.IgnoreServiceScaleRange = sourceLayer.IgnoreServiceScaleRange;
            targetLayer.MapTip = sourceLayer.MapTip;
            targetLayer.MaxAllowableOffset = sourceLayer.MaxAllowableOffset;
            targetLayer.MaximumResolution = sourceLayer.MaximumResolution;
            targetLayer.Mode = sourceLayer.Mode;
            targetLayer.ObjectIDs = sourceLayer.ObjectIDs;
            targetLayer.OnDemandCacheSize = sourceLayer.OnDemandCacheSize;
            targetLayer.Opacity = sourceLayer.Opacity;
            targetLayer.OutFields = sourceLayer.OutFields;
            targetLayer.ProjectionService = sourceLayer.ProjectionService;
            targetLayer.ProxyUrl = sourceLayer.ProxyUrl;
            targetLayer.Renderer = sourceLayer.Renderer;
            targetLayer.RendererTakesPrecedence = sourceLayer.RendererTakesPrecedence;
            targetLayer.SelectionColor = sourceLayer.SelectionColor;
            targetLayer.ShowLegend = sourceLayer.ShowLegend;
            targetLayer.Source = sourceLayer.Source;
            targetLayer.TimeOption = sourceLayer.TimeOption;
            targetLayer.Token = sourceLayer.Token;
            targetLayer.Url = sourceLayer.Url;
            targetLayer.ValidateEdits = sourceLayer.ValidateEdits;
            targetLayer.Visible = sourceLayer.Visible;
            targetLayer.VisibleTimeExtent = sourceLayer.VisibleTimeExtent;

            if (!string.IsNullOrEmpty(targetLayer.Url))
            {
                targetLayer.Geometry = sourceLayer.Geometry;
                targetLayer.Text = sourceLayer.Text;
                targetLayer.Where = sourceLayer.Where;
                targetLayer.ReturnZ = sourceLayer.ReturnZ;
                targetLayer.ReturnM = sourceLayer.ReturnM;
            }

            LayerExtensions.SetAutoUpdateInterval(targetLayer, 
                LayerExtensions.GetAutoUpdateInterval(sourceLayer));
            LayerExtensions.SetAutoUpdateOnExtentChanged(targetLayer,
                LayerExtensions.GetAutoUpdateOnExtentChanged(sourceLayer));
            LayerExtensions.SetDataset(targetLayer,
                LayerExtensions.GetDataset(sourceLayer));
            LayerExtensions.SetDisplayField(targetLayer,
                LayerExtensions.GetDisplayField(sourceLayer));
            LayerExtensions.SetDisplayUrl(targetLayer,
                LayerExtensions.GetDisplayUrl(sourceLayer));
            LayerExtensions.SetFeatureCollectionJson(targetLayer,
                LayerExtensions.GetFeatureCollectionJson(sourceLayer));
            LayerExtensions.SetFields(targetLayer,
                LayerExtensions.GetFields(sourceLayer));
            LayerExtensions.SetGeometryType(targetLayer,
                LayerExtensions.GetGeometryType(sourceLayer));
            LayerExtensions.SetGPResultMapServerLayerInfos(targetLayer,
                LayerExtensions.GetGPResultMapServerLayerInfos(sourceLayer));
            LayerExtensions.SetGradientBrush(targetLayer,
                LayerExtensions.GetGradientBrush(sourceLayer));
            LayerExtensions.SetIdentifyLayerIds(targetLayer,
                LayerExtensions.GetIdentifyLayerIds(sourceLayer));
            LayerExtensions.SetIsBaseMapLayer(targetLayer,
                LayerExtensions.GetIsBaseMapLayer(sourceLayer));
            LayerExtensions.SetIsConfigurable(targetLayer,
                LayerExtensions.GetIsConfigurable(sourceLayer));
            LayerExtensions.SetIsMapTipDirty(targetLayer,
                LayerExtensions.GetIsMapTipDirty(sourceLayer));
            LayerExtensions.SetIsReferenceLayer(targetLayer,
                LayerExtensions.GetIsReferenceLayer(sourceLayer));
            LayerExtensions.SetLayerInfos(targetLayer,
                LayerExtensions.GetLayerInfos(sourceLayer));
            LayerExtensions.SetLayerName(targetLayer,
                LayerExtensions.GetLayerName(sourceLayer));
            LayerExtensions.SetPopupInfo(targetLayer,
                LayerExtensions.GetPopupInfo(sourceLayer));
            LayerExtensions.SetPopUpsOnClick(targetLayer,
                LayerExtensions.GetPopUpsOnClick(sourceLayer));
            LayerExtensions.SetRendererAttributeDisplayName(targetLayer,
                LayerExtensions.GetRendererAttributeDisplayName(sourceLayer));
            LayerExtensions.SetRunLayerPostInitializationActions(targetLayer,
                LayerExtensions.GetRunLayerPostInitializationActions(sourceLayer));
            LayerExtensions.SetSerializedPopupDataTemplates(targetLayer,
                LayerExtensions.GetSerializedPopupDataTemplates(sourceLayer));
            LayerExtensions.SetSerializedPopupTitleExpressions(targetLayer,
                LayerExtensions.GetSerializedPopupTitleExpressions(sourceLayer));
            LayerExtensions.SetSerializedWebMapPopupDataTemplates(targetLayer,
                LayerExtensions.GetSerializedWebMapPopupDataTemplates(sourceLayer));
            LayerExtensions.SetSerializedWebMapPopupTitleExpressions(targetLayer,
                LayerExtensions.GetSerializedWebMapPopupTitleExpressions(sourceLayer));
            LayerExtensions.SetTitle(targetLayer,
                LayerExtensions.GetTitle(sourceLayer));
            LayerExtensions.SetUsePopupFromWebMap(targetLayer,
                LayerExtensions.GetUsePopupFromWebMap(sourceLayer));
            LayerExtensions.SetUsesProxy(targetLayer,
                LayerExtensions.GetUsesProxy(sourceLayer));
            LayerExtensions.SetWebMapPopupDataTemplates(targetLayer,
                LayerExtensions.GetWebMapPopupDataTemplates(sourceLayer));
            LayerExtensions.SetWebMapPopupTitleExpressions(targetLayer,
                LayerExtensions.GetWebMapPopupTitleExpressions(sourceLayer));

            Extensibility.LayerExtensions.SetErrorMessage(targetLayer,
                Extensibility.LayerExtensions.GetErrorMessage(sourceLayer));
            Extensibility.LayerExtensions.SetInitialUpdateCompleted(targetLayer,
                Extensibility.LayerExtensions.GetInitialUpdateCompleted(sourceLayer));
            Extensibility.LayerExtensions.SetInitialUpdateFailed(targetLayer,
                Extensibility.LayerExtensions.GetInitialUpdateFailed(sourceLayer));

            LayerProperties.SetIsPopupEnabled(targetLayer,
                LayerProperties.GetIsPopupEnabled(sourceLayer));
            LayerProperties.SetIsVisibleInMapContents(targetLayer,
                LayerProperties.GetIsVisibleInMapContents(sourceLayer));
            LayerProperties.SetPopupDataTemplates(targetLayer,
                LayerProperties.GetPopupDataTemplates(sourceLayer));
            LayerProperties.SetPopupTitleExpressions(targetLayer,
                LayerProperties.GetPopupTitleExpressions(sourceLayer));

            MapApplication.SetLayerName(targetLayer,
                MapApplication.GetLayerName(sourceLayer));
        }

        /// <summary>
        /// Converts a FeatureLayer's LayerInfo to a dictionary with property naming and tree
        /// structure consistent with ArcGIS JSON
        /// </summary>
        public static Dictionary<string, object> GetLayerInfoDictionary(this FeatureLayer layer)
        {
            if (layer.LayerInfo == null)
                return null;

            var layerDefinition = layer.LayerInfo.ToDictionary();

            // add version here because it's not public on layer info
            layerDefinition.Add("currentVersion", layer.Version);

            return layerDefinition;
        }
    }
}
