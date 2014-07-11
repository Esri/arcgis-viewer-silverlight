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
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;

namespace ESRI.ArcGIS.Mapping.Core
{
    internal class LayerCloneHelper
    {
        public Layer CloneLayer(Layer layer)
        {
            if (layer == null)
                return null;

            ICustomLayer customLayer = layer as ICustomLayer;
            if (customLayer != null)
                return customLayer.CloneLayer();

            ArcGISDynamicMapServiceLayer dynamicMapServiceLayer = layer as ArcGISDynamicMapServiceLayer;
            if (dynamicMapServiceLayer != null)
            {
                ArcGISDynamicMapServiceLayer newLayer = new ArcGISDynamicMapServiceLayer()
                {
                    ImageFormat = dynamicMapServiceLayer.ImageFormat,
                    ProxyURL = dynamicMapServiceLayer.ProxyURL,
                    Token = dynamicMapServiceLayer.Token,
                    Url = dynamicMapServiceLayer.Url,
                    DisableClientCaching = dynamicMapServiceLayer.DisableClientCaching,
                    LayerDefinitions = dynamicMapServiceLayer.LayerDefinitions,
                    VisibleLayers = dynamicMapServiceLayer.VisibleLayers,
                };
                copyBaseLayerAttributes(layer, newLayer);
                return newLayer;
            }

            ArcGISTiledMapServiceLayer tiledMapServiceLayer = layer as ArcGISTiledMapServiceLayer;
            if (tiledMapServiceLayer != null)
            {
                ArcGISTiledMapServiceLayer newLayer = new ArcGISTiledMapServiceLayer()
                {
                    ProxyURL = tiledMapServiceLayer.ProxyURL,
                    Token = tiledMapServiceLayer.Token,
                    Url = tiledMapServiceLayer.Url,
                };
                copyBaseLayerAttributes(layer, newLayer);
                return newLayer;
            }

            TileLayer tileLayer = layer as TileLayer;
            if (tileLayer != null)
            {
                TileLayer newLayer = new TileLayer()
                {
                    Culture = tileLayer.Culture,
                    LayerStyle = tileLayer.LayerStyle,
                    ServerType = tileLayer.ServerType,
                    Token = tileLayer.Token,
                };
                copyBaseLayerAttributes(layer, newLayer);
                return newLayer;
            }

            FeatureLayer featureLayer = layer as FeatureLayer;
            if (featureLayer != null)
            {
                FeatureLayer newLayer = new FeatureLayer()
                {
                    AutoSave = featureLayer.AutoSave,
                    Clusterer = featureLayer.Clusterer,
                    Color = featureLayer.Color,
                    DisableClientCaching = featureLayer.DisableClientCaching,
                    FeatureSymbol = featureLayer.FeatureSymbol,
                    Geometry = featureLayer.Geometry,
                    MapTip = featureLayer.MapTip,
                    Mode = featureLayer.Mode,
                    ObjectIDs = featureLayer.ObjectIDs,
                    OnDemandCacheSize = featureLayer.OnDemandCacheSize,
                    ProxyUrl = featureLayer.ProxyUrl,
                    Renderer = featureLayer.Renderer,
                    SelectionColor = featureLayer.SelectionColor,
                    Text = featureLayer.Text,
                    Token = featureLayer.Token,
                    Url = featureLayer.Url,
                    Where = featureLayer.Where,
                };
                foreach (string outfields in featureLayer.OutFields)
                    newLayer.OutFields.Add(outfields);
                copyBaseLayerAttributes(layer, newLayer);
                return newLayer;
            }

            GraphicsLayer graphicsLayer = layer as GraphicsLayer;
            if (graphicsLayer != null)
            {
                GraphicsLayer newLayer = new GraphicsLayer()
                {
                    Clusterer = graphicsLayer.Clusterer,
                    MapTip = graphicsLayer.MapTip,
                    Renderer = graphicsLayer.Renderer,
                    IsHitTestVisible = graphicsLayer.IsHitTestVisible,
                };
                copyBaseLayerAttributes(layer, tiledMapServiceLayer);
                return newLayer;
            }

            return null;
        }

        private void copyBaseLayerAttributes(Layer source, Layer destination)
        {
            destination.ID = source.ID;
            destination.Effect = source.Effect;
            destination.Opacity = source.Opacity;
            destination.Visible = source.Visible;
            destination.MaximumResolution = source.MaximumResolution;
            destination.MinimumResolution = source.MinimumResolution;
            destination.SetValue(LayerExtensions.DisplayInLayerListProperty, source.GetValue(LayerExtensions.DisplayInLayerListProperty));
            destination.SetValue(LayerExtensions.DisplayNameProperty, source.GetValue(LayerExtensions.DisplayNameProperty));
        }
    }
}
