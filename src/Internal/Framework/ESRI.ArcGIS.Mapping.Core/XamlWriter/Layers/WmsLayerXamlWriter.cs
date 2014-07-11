/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Xml;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit.DataSources;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class WmsLayerXamlWriter : LayerXamlWriter
    {
        public WmsLayerXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces) :
            base(writer, namespaces)
        {

        }

        protected override void WriteStartElement(string layerPrefix, string layerNamespace)
        {
            writer.WriteStartElement(Constants.esriPrefix, "WmsLayer", Constants.esriNamespace);
        }

        protected override void WriteAttributes(Layer layer)
        {
            base.WriteAttributes(layer);

            WmsLayer wmsLayer = layer as WmsLayer;
            if (wmsLayer != null)
            {
                if(!string.IsNullOrEmpty(wmsLayer.Url))
                    writer.WriteAttributeString("Url", wmsLayer.Url);
                if (!string.IsNullOrEmpty(wmsLayer.ProxyUrl))
                    writer.WriteAttributeString("ProxyUrl", wmsLayer.ProxyUrl);
                if (!string.IsNullOrEmpty(wmsLayer.Version))
                    writer.WriteAttributeString("Version", wmsLayer.Version);
                if(wmsLayer.SkipGetCapabilities)
                    writer.WriteAttributeString("SkipGetCapabilities", "True");

                if (wmsLayer.Layers != null)
                {
                    string layersStr = string.Empty;
                    if (wmsLayer.Layers.Length > 0)
                    {
                        foreach (string layerId in wmsLayer.Layers)
                            layersStr += layerId.ToString() + ',';

                        layersStr = layersStr.TrimEnd(',');
                    }
                    WriteAttribute("Layers", layersStr);
                }
            }
        }
    }
}
