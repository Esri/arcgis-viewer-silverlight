/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class ProxyUrlHelper
    {
        public static string GetProxyUrl()
        {
            if (MapApplication.Current != null && MapApplication.Current.Urls != null)
                return MapApplication.Current.Urls.ProxyUrl;

            return null;
        }

        public static void SetProxyUrl(Layer layer)
        {
            string proxyUrl = GetProxyUrl();
            SetProxyUrl(layer, proxyUrl);
        }

        public static bool CanChangeProxyUrl(Layer layer)
        {
            if (layer is ArcGISDynamicMapServiceLayer ||
                layer is ArcGISImageServiceLayer ||
                layer is ArcGISTiledMapServiceLayer)
                return !layer.IsInitialized;
            return (layer is FeatureLayer || layer is HeatMapFeatureLayer);
        }

        public static void SetProxyUrl(Layer layer, string proxyUrl)
        {
            ArcGISDynamicMapServiceLayer dmsLayer = layer as ArcGISDynamicMapServiceLayer;
            string url = null;
            if (dmsLayer != null)
            {
                if (dmsLayer.ProxyURL != proxyUrl)
                {
                    url = dmsLayer.Url;
                    try
                    {
                        dmsLayer.Url = "";
                    }
                    catch { }
                    dmsLayer.ProxyURL = proxyUrl;
                    dmsLayer.Url = url;
                    dmsLayer.Refresh();
                }
                return;
            }
            ArcGISImageServiceLayer isLayer = layer as ArcGISImageServiceLayer;
            if (isLayer != null)
            {
                if (isLayer.ProxyURL != proxyUrl)
                {
                    url = isLayer.Url;
                    try
                    {
                        isLayer.Url = "";
                    }
                    catch { }
                    isLayer.ProxyURL = proxyUrl;
                    isLayer.Url = url;
                    isLayer.Refresh();
                }
                return;
            }
            ArcGISTiledMapServiceLayer tmsLayer = layer as ArcGISTiledMapServiceLayer;
            if (tmsLayer != null)
            {
                if (tmsLayer.ProxyURL != proxyUrl)
                {
                    url = tmsLayer.Url;
                    try
                    {
                        tmsLayer.Url = "";
                    }
                    catch { }
                    tmsLayer.ProxyURL = proxyUrl;
                    tmsLayer.Url = url;
                    tmsLayer.Refresh();
                }
                return;
            }
            FeatureLayer fLayer = layer as FeatureLayer;
            if (fLayer != null)
            {
                if (fLayer.ProxyUrl != proxyUrl)
                {
                    fLayer.ProxyUrl = proxyUrl;
                    fLayer.Update();
                }
                return;
            }
            HeatMapFeatureLayer hmfLayer = layer as HeatMapFeatureLayer;
            if (hmfLayer != null)
            {
                if (hmfLayer.ProxyUrl != proxyUrl)
                {
                    hmfLayer.ProxyUrl = proxyUrl;
                    hmfLayer.Update();
                }
                return;
            }
        }

        public static string GetProxyUrl(Layer layer)
        {
            if (layer is ArcGISDynamicMapServiceLayer)
                return (layer as ArcGISDynamicMapServiceLayer).ProxyURL;
            else if (layer is ArcGISImageServiceLayer)
                return (layer as ArcGISImageServiceLayer).ProxyURL;
            else if (layer is ArcGISTiledMapServiceLayer)
                return (layer as ArcGISTiledMapServiceLayer).ProxyURL;
            else if (layer is FeatureLayer)
                return (layer as FeatureLayer).ProxyUrl;
            else if (layer is HeatMapFeatureLayer)
                return (layer as HeatMapFeatureLayer).ProxyUrl;
            return null;
        }

        public static bool SupportsProxyUrl(Layer layer)
        {
            return ((layer is ArcGISDynamicMapServiceLayer ||
                layer is ArcGISImageServiceLayer ||
                layer is ArcGISTiledMapServiceLayer ||
                layer is FeatureLayer ||
                layer is HeatMapFeatureLayer) &&
                    !(ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetIsBaseMapLayer(layer)));
        }
    }
}
