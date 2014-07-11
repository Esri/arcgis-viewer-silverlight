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
    public class BingMapsTiledLayerXamlWriter : LayerXamlWriter
    {
        public BingMapsTiledLayerXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces) :
            base(writer, namespaces)
        {
           
        }

        protected override void WriteStartElement(string layerPrefix, string layerNamespace)
        {
            writer.WriteStartElement("TileLayer", Namespaces["esriBing"]);
        }

        protected override void WriteAttributes(Layer layer)
        {
            base.WriteAttributes(layer);

            Client.Bing.TileLayer tileLayer = layer as Client.Bing.TileLayer;
            if (tileLayer != null)
            {
                if (tileLayer.ServerType != default(ESRI.ArcGIS.Client.Bing.ServerType))
                    WriteAttribute("ServerType", tileLayer.ServerType.ToString());
                if (tileLayer.LayerStyle != default(ESRI.ArcGIS.Client.Bing.TileLayer.LayerType))
                    WriteAttribute("LayerStyle", tileLayer.LayerStyle.ToString());
                 if (!string.IsNullOrEmpty(tileLayer.Token))
                     WriteAttribute("Token", tileLayer.Token);

            }
       }
        protected override void WriteBaseElementContents(Layer layer)
        {
            base.WriteBaseElementContents(layer);
            writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.UsesBingAppID", Constants.esriMappingNamespace, "true");
        }
    }
}
