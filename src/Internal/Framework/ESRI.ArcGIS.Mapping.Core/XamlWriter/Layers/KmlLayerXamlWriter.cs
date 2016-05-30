/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit.DataSources;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class KmlLayerExtensions
    {
        public static readonly DependencyProperty VisibleLayerIDsProperty = DependencyProperty.Register("VisibleLayerIDs", typeof(string), typeof(KmlLayer), null);
        public static string GetVisibleLayerIDs(KmlLayer layer)
        {
            return (string) layer.GetValue(VisibleLayerIDsProperty);
        }

        public static void SetVisibleLayerIDs(KmlLayer layer, string value)
        {
            layer.SetValue(VisibleLayerIDsProperty, value);
            if (!string.IsNullOrEmpty(value))
            {
                layer.SetVisibilityByIDs(value.Split(',').Select(p => Convert.ToInt32(p)));
            }
        }
    }

    public class KmlLayerXamlWriter : GraphicsLayerXamlWriter
    {
        public KmlLayerXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces) :
            base(writer, namespaces)
        {
        }

        protected override void WriteStartElement(string layerPrefix, string layerNamespace)
        {
            writer.WriteStartElement(Constants.esriPrefix, "KmlLayer", Constants.esriNamespace);
        }

        protected override void WriteAttributes(Layer layer)
        {
            base.WriteAttributes(layer);

            KmlLayer kmlLayer = layer as KmlLayer;
            if (kmlLayer != null)
            {
                if (kmlLayer.Url != null)
                    writer.WriteAttributeString("Url", kmlLayer.Url.OriginalString);

                if (!LayerExtensions.GetUsesProxy(layer) && !string.IsNullOrEmpty(kmlLayer.ProxyUrl))
                    WriteAttribute("ProxyUrl", kmlLayer.ProxyUrl);

                writer.WriteAttributeString("DisableClientCaching", kmlLayer.DisableClientCaching.ToString());

                if (kmlLayer.RefreshInterval != null)
                    writer.WriteAttributeString("RefreshInterval", kmlLayer.RefreshInterval.ToString());

                IEnumerable<int> visibleLayerIDs = kmlLayer.GenerateVisibilityIDs();
                if (visibleLayerIDs.Count() > 0)
                    writer.WriteElementString(Constants.esriMappingPrefix, "KmlLayerExtensions.VisibleLayerIDs", Constants.esriMappingNamespace, visibleLayerIDs.Select(p => p.ToString()).Aggregate((id1, id2) => id1 + "," + id2));
            }
        }
    }
}
