/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Xml;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit.DataSources;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class GeoRssLayerXamlWriter : GraphicsLayerXamlWriter
    {
        public GeoRssLayerXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces) :
            base(writer, namespaces)
        {
           
        }

        protected override void WriteStartElement(string layerPrefix, string layerNamespace)
        {
            writer.WriteStartElement(Constants.esriPrefix, "GeoRssLayer", Constants.esriNamespace);
        }

        protected override void WriteAttributes(Layer layer)
        {
            base.WriteAttributes(layer);

            GeoRssLayer geoRssLayer = layer as GeoRssLayer;
            if (geoRssLayer != null)
            {
                if (geoRssLayer.Source != null)
                    writer.WriteAttributeString("Source", geoRssLayer.Source.AbsoluteUri);
            }
        }
    }
}
