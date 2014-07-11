/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Mapping.Core;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using System.Windows.Data;
using System.Linq;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class MapTipsConfig : LayerConfigControl, INotifyPropertyChanged
    {
        public MapTipsConfig()
        {
            this.DefaultStyleKey = typeof(MapTipsConfig);
        }
        MapTipsLayerConfig mapTipsLayerConfig;
        public override void OnApplyTemplate()
        {
            if (mapTipsLayerConfig != null)
                mapTipsLayerConfig.DisplayFieldChanged -= mapTipsLayerConfig_DisplayFieldChanged;
            base.OnApplyTemplate();
            mapTipsLayerConfig = GetTemplateChild("MapTipsLayerConfig") as MapTipsLayerConfig;
            if (mapTipsLayerConfig != null)
                mapTipsLayerConfig.DisplayFieldChanged += mapTipsLayerConfig_DisplayFieldChanged;
            bindUIToLayer();
        }

        void mapTipsLayerConfig_DisplayFieldChanged(object sender, EventArgs e)
        {
            onPropertyChanged("DisplayField");
        }

        internal event EventHandler InitCompleted;

        protected virtual void OnInitCompleted()
        {
            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }

        private void bindUIToLayer()
        {
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer != null)
            {
                bindToGraphicsLayer(graphicsLayer);
            }
            else if ((Layer is ArcGISDynamicMapServiceLayer || Layer is ArcGISTiledMapServiceLayer)
                && !ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetIsBaseMapLayer(Layer))
            {
                bindToMapServiceLayer(Layer);
            }
            else
                DataContext = null;
        }

        private void bindToMapServiceLayer(Layer layer)
        {
            if (layer == null)
                return;
            Collection<LayerInformation> layerInfos = LayerExtensions.GetLayerInfos(layer);
            if (layerInfos == null || layerInfos.Count < 1)
            {
                string layerUrl = IdentifySupport.GetLayerUrl(layer);
                if (!string.IsNullOrWhiteSpace(layerUrl))
                {
                    MapServiceLayerInfoHelper helper = new MapServiceLayerInfoHelper(layerUrl, Layer, IdentifySupport.GetLayerProxyUrl(layer));
                    helper.GetLayerInfosCompleted += helper_GetLayerInfosCompleted;
                    helper.GetLayerInfos(null);
                }
            }
            else
                setMapServiceLayerDataContext(layerInfos);
        }

        void helper_GetLayerInfosCompleted(object sender, MapServiceLayerInfoHelper.LayerInfosEventArgs e)
        {
            MapServiceLayerInfoHelper helper = sender as MapServiceLayerInfoHelper;
            Collection<int> layerIds = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetIdentifyLayerIds(helper.Layer);
            if ((layerIds != null && layerIds.Count > 0))
            {
                if (e.LayerInfos != null)
                {
                    foreach (LayerInformation item in e.LayerInfos)
                    {
                        if (layerIds.Contains(item.ID))
                            item.PopUpsEnabled = true;
                    }
                }
            }
            if (Layer == helper.Layer)//if still the selected layer, update the config layer
            {
                if (e.LayerInfos == null || e.LayerInfos.Count < 1)
                    this.DataContext = null;
                else
                    setMapServiceLayerDataContext(e.LayerInfos);
            }
        }

        void setMapServiceLayerDataContext(Collection<LayerInformation> layerInfos)
        {
            if (layerInfos.Count > 0)
            {
                #region Set data context
                MapTipsConfigInfo info = new MapTipsConfigInfo()
                {
                    LayerSelectionVisibility = true,
                    SupportsOnClick = true,
                    Layer = Layer,
                };
                setFromWebMap(info);
				info.IsPopupEnabled = ESRI.ArcGIS.Client.Extensibility.LayerProperties.GetIsPopupEnabled(Layer);
                info.Layers = layerInfos;
                info.SelectedItem = layerInfos[0];
                this.DataContext = info;
                info.PropertyChanged += info_PropertyChanged;
                #endregion
            }
        }

        private void setFromWebMap(MapTipsConfigInfo info)
        {
            IDictionary<int, string> webMapTemplates = LayerExtensions.GetWebMapPopupDataTemplates(Layer);
            if (webMapTemplates != null && webMapTemplates.Count > 0)
            {
                info.FromWebMap = LayerExtensions.GetUsePopupFromWebMap(Layer);
                info.WebMapPopupVisibility = System.Windows.Visibility.Visible;
            }
            else
            {
                info.FromWebMap = false;
                info.WebMapPopupVisibility = System.Windows.Visibility.Collapsed;
            }
        }

        void info_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PopUpsEnabled")
            {
                MapTipsConfigInfo info = sender as MapTipsConfigInfo;
                Collection<int> layerIds = new Collection<int>();
                if (info != null && info.Layers != null)
                {
                    foreach (LayerInformation layer in info.Layers)
                    {
                        if (layer.PopUpsEnabled)
                            layerIds.Add(layer.ID);
                    }
					if (Layer != null)
					{
						if (layerIds.Count < 1)
							Layer.ClearValue(LayerExtensions.IdentifyLayerIdsProperty);
						else
							LayerExtensions.SetIdentifyLayerIds(Layer, layerIds);
					}
					onPropertyChanged("IdentifyLayerIds");
                }
                mapTipsLayerConfig.BindUI();
            }
            else if (e.PropertyName == "PopUpsOnClick")
            {
                MapTipsConfigInfo info = sender as MapTipsConfigInfo;
                GraphicsLayer layer = Layer as GraphicsLayer;
                if (info != null && layer != null)
                {
                    if (!(info.PopUpsOnClick.Value))
                        LayerExtensions.SetPopUpsOnClick(layer, false);
                    else
                    {
                        LayerExtensions.SetPopUpsOnClick(layer, true);
                        #region if no display field, set to default
                        if (info.Layers != null && info.Layers.Count == 1 && info.Layers[0] != null)
                        {
                            if (string.IsNullOrEmpty(info.Layers[0].DisplayField) ||
                                info.Layers[0].DisplayField == ESRI.ArcGIS.Mapping.Core.LocalizableStrings.NoneInAngleBraces)
                            {
                                info.Layers[0].DisplayField = FieldInfo.GetDefaultDisplayField(info.Layers[0].Fields as IEnumerable<FieldInfo>);
                                if (!string.IsNullOrEmpty(info.Layers[0].DisplayField))
                                    LayerExtensions.SetDisplayField(layer, info.Layers[0].DisplayField);
                            }
                        }
                        #endregion
                    }
                    LayerExtensions.SetIsMapTipDirty(layer, true);
                    mapTipsLayerConfig.BindUI();
                    onPropertyChanged("PopUpsOnClick");
                }
            }
            else if (e.PropertyName == "FromWebMap")
            {
                MapTipsConfigInfo info = sender as MapTipsConfigInfo;
                if (info != null)
                {
                    LayerExtensions.SetUsePopupFromWebMap(Layer, info.FromWebMap);
                    if (Layer is GraphicsLayer)
                        LayerExtensions.SetIsMapTipDirty(Layer as GraphicsLayer, true);
					onPropertyChanged("FromWebMap");
                }
            }
			else if (e.PropertyName == "IsPopupEnabled")
			{
				MapTipsConfigInfo info = sender as MapTipsConfigInfo;
				ESRI.ArcGIS.Client.Extensibility.LayerProperties.SetIsPopupEnabled(Layer, info.IsPopupEnabled);
				if (Layer is GraphicsLayer)
					LayerExtensions.SetIsMapTipDirty(Layer as GraphicsLayer, true);
				onPropertyChanged("IsPopupEnabled");
			}
			if (!(Layer is GraphicsLayer))
                onPropertyChanged("LayerInfos");
        }

        private void bindToGraphicsLayer(GraphicsLayer graphicsLayer)
        {
            object fields = graphicsLayer.GetValue(LayerExtensions.FieldsProperty);

            if (fields is IEnumerable<FieldInfo>)
            {
                string displayField = LayerExtensions.GetDisplayField(graphicsLayer);
                if (string.IsNullOrEmpty(displayField))
                {
                    displayField = FieldInfo.GetDefaultDisplayField(fields as IEnumerable<FieldInfo>);
                    if (!string.IsNullOrEmpty(displayField))
                        LayerExtensions.SetDisplayField(graphicsLayer, displayField);
                }

                #region Set data context
                MapTipsConfigInfo info = new MapTipsConfigInfo()
                {
                    LayerSelectionVisibility = false,
                    PopUpsOnClick = LayerExtensions.GetPopUpsOnClick(graphicsLayer),
					IsPopupEnabled = ESRI.ArcGIS.Client.Extensibility.LayerProperties.GetIsPopupEnabled(graphicsLayer),
                    Layer = graphicsLayer,
                };
                setFromWebMap(info);
                    info.SupportsOnClick = true;
                info.Layers = new Collection<LayerInformation>();

                LayerInformation item = new LayerInformation();
                item.PopUpsEnabled = true;
                item.ID = 0;
                item.Name = LayerExtensions.GetTitle(Layer);
                item.DisplayField = displayField;
                item.Fields = fields as Collection<FieldInfo>;
				if (graphicsLayer is FeatureLayer)
				{
					foreach (FieldInfo field in item.Fields)
					{
						if (field.DomainSubtypeLookup == DomainSubtypeLookup.NotDefined)
							field.DomainSubtypeLookup = FieldInfo.GetDomainSubTypeLookup(graphicsLayer, field);
					}
				}
                info.Layers.Add(item);
                info.SelectedItem = item;
                info.PropertyChanged += info_PropertyChanged;
                this.DataContext = info;
                #endregion
            }
        }

        protected override void OnLayerChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {            
            bindUIToLayer();        
        }

        void onPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;
}    

}
