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
using System.Xml;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using System.Text;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class HeatMapFeatureLayerXamlWriter : LayerXamlWriter
    {
        public HeatMapFeatureLayerXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces) :
            base(writer, namespaces)
        {
            
        }

        protected override void WriteElementContents(Layer layer)
        {
            HeatMapFeatureLayer featureLayer = layer as HeatMapFeatureLayer;
            if (featureLayer != null)
            {
                WriteHeatMapFeatureLayer(featureLayer);
            }            
        }

        /*TODO: Serialize the following
         * 
        public bool DisableClientCaching { get; set; }
        public Geometry Geometry { get; set; }
         * */
        private void WriteHeatMapFeatureLayer(HeatMapFeatureLayer layer)
        {   
            WriteAttribute("Url", layer.Url);

            
            if (!string.IsNullOrEmpty(layer.Where))
            {
                WriteAttribute("Where", layer.Where);
            }
            if (!LayerExtensions.GetUsesProxy(layer))
            {
                if (!string.IsNullOrEmpty(layer.ProxyUrl))
                {
                    WriteAttribute("ProxyUrl", layer.ProxyUrl);
                }
                if (!string.IsNullOrEmpty(layer.Token))
                {
                    WriteAttribute("Token", layer.Token);
                }
            }
            if (!string.IsNullOrEmpty(layer.Text))
            {
                WriteAttribute("Text", layer.Text);
            }
            if (layer.MinimumResolution > double.Epsilon)
            {
                WriteAttribute("MinimumResolution", layer.MinimumResolution);
            }
            if (layer.MaximumResolution < double.MaxValue)
            {
                WriteAttribute("MaximumResolution", layer.MaximumResolution);
            }
            if (! double.IsNaN(layer.Intensity))
            {
                WriteAttribute("Intensity", layer.Intensity);
            }
            if (layer.MapSpatialReference != null)
                WriteSpatialReferenceAsAttribute("MapSpatialReference", layer.MapSpatialReference);

            if (layer.Gradient != null)
            {
                writer.WriteStartElement("HeatMapFeatureLayer.Gradient", Namespaces[Constants.esriMappingPrefix]);
                writer.WriteStartElement("GradientStopCollection");
                new BrushXamlWriter(writer, Namespaces).WriteGradientStops(layer.Gradient);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            WriteBaseElementContents(layer);
        }

   }
}
