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
using System.Text;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class FeatureLayerXamlWriter : GraphicsLayerXamlWriter
    {
		public FeatureLayerXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces):
            base(writer, namespaces)
        {
            
        }

        protected override void WriteElementContents(Layer layer)
        {
            FeatureLayer featureLayer = layer as FeatureLayer;
            if (featureLayer != null)
            {
                WriteFeatureLayer(featureLayer);
            }            
        }

        private void WriteFeatureLayer(FeatureLayer layer)
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
            if (layer.Mode == FeatureLayer.QueryMode.OnDemand)
            {
                WriteAttribute("Mode", layer.Mode.ToString());
                if (layer.OnDemandCacheSize != 1000)
                    WriteAttribute("OnDemandCacheSize", layer.OnDemandCacheSize.ToString());
            }

            if (layer.OutFields != null && layer.OutFields.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string value in layer.OutFields)
                {
                    sb.AppendFormat("{0},", value);
                }
                string str = sb.ToString();
                if (str.Length > 0)
                {
                    str = str.Remove(str.Length - 1); // remove trailing ,
                    writer.WriteAttributeString("OutFields", str);
                }
            }
			if (layer.Geometry != null)
            {
                //TODO
            }

            if (layer.ObjectIDs != null && layer.ObjectIDs.Length > 0)
            {
                string oidString = "";
                foreach (int oid in layer.ObjectIDs)
                    oidString += oid.ToString() + ",";
                oidString = oidString.Substring(0, oidString.Length - 1);

                writer.WriteAttributeString("ObjectIDs", oidString);
            }

			// Serialize feature collection JSON for feature collection layers
            if (string.IsNullOrEmpty(layer.Url) && layer.LayerInfo != null) 
            {
                // feature collection layer.  Retrieve and store feature collection JSON

                string json = layer.GenerateFeatureCollectionJson();
                if (!string.IsNullOrEmpty(json))
                {
                    writer.WriteElementString(Constants.esriMappingPrefix,
                        "LayerExtensions.FeatureCollectionJson", Constants.esriMappingNamespace, json);
                }
            }

            WriteBaseElementContents(layer);
            WriteGraphicsLayerElements(layer);
            
        }
    }
}
