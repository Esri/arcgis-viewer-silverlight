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
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class IdentifySupport
    {
        public static string GetLayerUrl(Layer layer)
        {
            string layerUrl = null;
            if (layer is ArcGISDynamicMapServiceLayer)
                layerUrl = (layer as ArcGISDynamicMapServiceLayer).Url;
            else if (layer is ArcGISTiledMapServiceLayer)
                layerUrl = (layer as ArcGISTiledMapServiceLayer).Url;
            return layerUrl;
        }

        public static string GetLayerProxyUrl(Layer layer)
        {
            string layerProxyUrl = null;
            if (layer is ArcGISDynamicMapServiceLayer)
                layerProxyUrl = (layer as ArcGISDynamicMapServiceLayer).ProxyURL;
            else if (layer is ArcGISTiledMapServiceLayer)
                layerProxyUrl = (layer as ArcGISTiledMapServiceLayer).ProxyURL;
            return layerProxyUrl;
        }

        public static bool LayerSupportsPopUps(Layer layer)
        {
            return layer is GraphicsLayer || layer is ArcGISDynamicMapServiceLayer || layer is ArcGISTiledMapServiceLayer
                && !ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetIsBaseMapLayer(layer);
        }
    }
}
