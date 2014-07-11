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
    public static class XamlWriterFactory
    {
        public static LayerXamlWriter CreateLayerXamlWriter(Layer layer, XmlWriter writer, Dictionary<string, string> Namespaces)
        {   
            if (layer is FeatureLayer)
                return new FeatureLayerXamlWriter(writer, Namespaces);
            
            if (layer is ArcGISTiledMapServiceLayer)
                return new ArcGISTiledMapServiceLayerXamlWriter(writer, Namespaces);
            
            if (layer is ArcGISDynamicMapServiceLayer)
                return new ArcGISDynamicMapServiceLayerXamlWriter(writer, Namespaces);
            
            if (layer is ArcGISImageServiceLayer)
                return new ArcGISImageServiceLayerXamlWriter(writer, Namespaces);
            
            if (layer is Client.Bing.TileLayer)
                return new BingMapsTiledLayerXamlWriter(writer, Namespaces);
            
            if (layer is HeatMapFeatureLayer)
                return new HeatMapFeatureLayerXamlWriter(writer, Namespaces);
            
            if (layer is HeatMapLayer)
                return new HeatMapLayerXamlWriter(writer, Namespaces);
            
            if (layer is GeoRssLayer)
                return new GeoRssLayerXamlWriter(writer, Namespaces);
            
            if (layer is OpenStreetMapLayer)
                return new OpenStreetMapLayerXamlWriter(writer, Namespaces);
            
            if (layer is WmsLayer)
                return new WmsLayerXamlWriter(writer, Namespaces);
            
            if (layer is GraphicsLayer)
                return new GraphicsLayerXamlWriter(writer, Namespaces);

            if (layer is KmlLayer)
                return new KmlLayerXamlWriter(writer, Namespaces);

            return null;
        }
    }
}
