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
using System.Xml;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class ArcGISDynamicMapServiceLayerXamlWriter : LayerXamlWriter
    {
        public ArcGISDynamicMapServiceLayerXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces) :
            base(writer, namespaces)
        {
            
        }

        protected override void WriteAttributes(Layer layer)
        {
            base.WriteAttributes(layer);
            ArcGISDynamicMapServiceLayer dynamicLayer = layer as ArcGISDynamicMapServiceLayer;
            if (dynamicLayer != null)
            {
                WriteAttribute("Url", dynamicLayer.Url);

                if (!LayerExtensions.GetUsesProxy(layer))
                {
                    if (!string.IsNullOrEmpty(dynamicLayer.ProxyURL))
                    {
                        WriteAttribute("ProxyURL", dynamicLayer.ProxyURL);
                    }
                    if (!string.IsNullOrEmpty(dynamicLayer.Token))
                    {
                        WriteAttribute("Token", dynamicLayer.Token);
                    }
                }
                if (dynamicLayer.VisibleLayers != null)
                {
                    string visibleLayersStr = string.Empty;
                    if (dynamicLayer.VisibleLayers.Length > 0)
                    {
                        foreach (int layerId in dynamicLayer.VisibleLayers)
                        {
                            visibleLayersStr += layerId.ToString() + ',';
                        }

                        visibleLayersStr = visibleLayersStr.TrimEnd(',');
                    }
                    WriteAttribute("VisibleLayers", visibleLayersStr);
                }
            }
        }           
    }
}
