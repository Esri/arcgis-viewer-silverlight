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
using System.Xml;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class ArcGISTiledMapServiceLayerXamlWriter: LayerXamlWriter
    {
        public ArcGISTiledMapServiceLayerXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces) :
            base(writer, namespaces)
        {
            
        }

        protected override void WriteAttributes(Layer layer)
        {
            base.WriteAttributes(layer);
            ArcGISTiledMapServiceLayer tiledLayer = layer as ArcGISTiledMapServiceLayer;
            if (tiledLayer != null)
            {
                WriteAttribute("Url", tiledLayer.Url);
                if (!LayerExtensions.GetUsesProxy(layer))
                {
                    if (!string.IsNullOrEmpty(tiledLayer.ProxyURL))
                    {
                        WriteAttribute("ProxyURL", tiledLayer.ProxyURL);
                    }
                    if (!string.IsNullOrEmpty(tiledLayer.Token))
                    {
                        WriteAttribute("Token", tiledLayer.Token);
                    }
                }
                if (!double.IsInfinity(tiledLayer.MaximumResolution) && !double.IsNaN(tiledLayer.MaximumResolution))
                    WriteAttribute("MaximumResolution", tiledLayer.MaximumResolution);
                if (!double.IsInfinity(tiledLayer.MinimumResolution) && !double.IsNaN(tiledLayer.MinimumResolution))
                    WriteAttribute("MinimumResolution", tiledLayer.MinimumResolution);
            }
        }          
    }
}
