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
    public class ArcGISImageServiceLayerXamlWriter: LayerXamlWriter
    {
        public ArcGISImageServiceLayerXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces) :
            base(writer, namespaces)
        {
            
        }

        protected override void WriteAttributes(Layer layer)
        {
            base.WriteAttributes(layer);
            ArcGISImageServiceLayer imageServiceLayer = layer as ArcGISImageServiceLayer;
            if (imageServiceLayer != null)
            {
                WriteAttribute("Url", imageServiceLayer.Url);
                if (!LayerExtensions.GetUsesProxy(layer))
                {
                    if (!string.IsNullOrEmpty(imageServiceLayer.ProxyURL))
                    {
                        WriteAttribute("ProxyURL", imageServiceLayer.ProxyURL);
                    }
                    if (!string.IsNullOrEmpty(imageServiceLayer.Token))
                    {
                        WriteAttribute("Token", imageServiceLayer.Token);
                    }
                }

                WriteAttribute("ImageFormat", imageServiceLayer.ImageFormat.ToString());

                if (imageServiceLayer.BandIds != null && imageServiceLayer.BandIds.Length > 0)
                    WriteAttribute("BandIds", string.Join(",", imageServiceLayer.BandIds));
            }
        }  
    }
}
