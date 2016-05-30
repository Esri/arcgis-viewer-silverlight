/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Interactivity;
using System.Collections;
using ESRI.ArcGIS.Mapping.Core.DataSources;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class MapXamlWriter : XamlWriterBase
    {        
        public Dictionary<string, ResourceDictionaryEntry> ResourceDictionary { get; private set; }
        public bool EncloseInUserControlTag { get; set; }
        public bool Indent { get; set; }
        public bool OmitXmlDeclaration { get; set; }
        
        public MapXamlWriter(bool indent): base(null, new Dictionary<string, string>())
        {            
            Namespaces.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");            
            Namespaces.Add(Constants.sysPrefix, Constants.sysNamespace);                                    
            Namespaces.Add(Constants.esriPrefix, Constants.esriNamespace);
            Namespaces.Add("esriBing", "clr-namespace:ESRI.ArcGIS.Client.Bing;assembly=ESRI.ArcGIS.Client.Bing");
            Namespaces.Add(Constants.esriMappingPrefix, Constants.esriMappingNamespace);
            Namespaces.Add(Constants.esriExtensibilityPrefix, Constants.esriExtensibilityNamespace);
            Namespaces.Add(Constants.esriFSSymbolsPrefix, Constants.esriFSSymbolsNamespace);
            //Namespaces.Add("i", "clr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity");
            //Namespaces.Add(Constants.esriBehaviorsPrefix, Constants.esriBehaviorsNamespace);

            Indent = indent;
            writerSettings = new XmlWriterSettings()
            {
                Indent = Indent,
                OmitXmlDeclaration = true,
                //NewLineOnAttributes = true,
                IndentChars = "\t"
            };
            ResourceDictionary = new Dictionary<string, ResourceDictionaryEntry>();
        }

        StringBuilder xaml;
        public string MapToXaml(Map map)
        {
            if (map == null)
                throw new ArgumentNullException("map");
            xaml = new StringBuilder();
            writer = XmlWriter.Create(xaml, writerSettings);

            if (EncloseInUserControlTag)
            {
                writer.WriteStartElement("UserControl");
                WriteNamespaces();

                if (ResourceDictionary.Count > 0)
                {
                    writer.WriteStartElement("UserControl.Resources");
                    WriteResources(ResourceDictionary);
                    writer.WriteEndElement();
                }
            }

            writer.WriteStartElement("Map", Constants.esriNamespace);
            if(!EncloseInUserControlTag)
                WriteNamespaces();

            if (map.Rotation != default(double))
                writer.WriteAttributeString("Rotation", map.Rotation.ToString(CultureInfo.InvariantCulture));

            if (map.SnapToLevels)
                writer.WriteAttributeString("SnapToLevels", map.SnapToLevels.ToString(CultureInfo.InvariantCulture));

            MapUnit scaleBarUnit = ScaleBarExtensions.GetScaleBarMapUnit(map);
            if (scaleBarUnit != MapUnit.Undefined)
            {
                writer.WriteElementString(Constants.esriMappingPrefix, "ScaleBarExtensions.ScaleBarMapUnit", Constants.esriMappingNamespace, scaleBarUnit.ToString());
            }

            writer.WriteElementString(Constants.esriPrefix, "Map.IsLogoVisible", Constants.esriNamespace, map.IsLogoVisible.ToString());

            if (map.Extent != null)
            {
                writer.WriteStartElement("Map.Extent", Constants.esriNamespace);

                writer.WriteStartElement("Envelope", Constants.esriNamespace);
                writer.WriteAttributeString("XMin", map.Extent.XMin.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("XMax", map.Extent.XMax.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("YMin", map.Extent.YMin.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("YMax", map.Extent.YMax.ToString(CultureInfo.InvariantCulture));                

                if (map.Extent.SpatialReference != null)
                {
                    writer.WriteStartElement("Envelope.SpatialReference", Constants.esriNamespace);

                    writer.WriteStartElement("SpatialReference", Constants.esriNamespace);
                    if (!string.IsNullOrEmpty(map.Extent.SpatialReference.WKT))
                        writer.WriteAttributeString("WKT", map.Extent.SpatialReference.WKT);
                    else
                        writer.WriteAttributeString("WKID", map.Extent.SpatialReference.WKID.ToString());
                    writer.WriteEndElement(); // SpatialReference

                    writer.WriteEndElement(); // Envelope.SpatialReference
                }

                writer.WriteEndElement(); // Envelope
                writer.WriteEndElement(); // Map.Extent                
            }
            //WriteXName("MyMap");

            if (!EncloseInUserControlTag)
            {
                if (ResourceDictionary.Count > 0)
                {
                    writer.WriteStartElement("Map.Resources", Constants.esriNamespace);
                    WriteResources(ResourceDictionary);
                    writer.WriteEndElement();
                }
            }
            
            foreach (Layer layer in map.Layers)
            {
                if ((bool)layer.GetValue(ESRI.ArcGIS.Mapping.Core.LayerExtensions.ExcludeSerializationProperty) == true)
                    continue;

                ICustomLayer customlayer = layer as ICustomLayer;
                if (customlayer != null)
                {
                    customlayer.Serialize(writer, Namespaces);
                    continue;
                }

                LayerXamlWriter layerXamlWriter = XamlWriterFactory.CreateLayerXamlWriter(layer, writer, Namespaces);
                if (layerXamlWriter != null)
                {
                    if (layer is HeatMapFeatureLayer)
                        layerXamlWriter.WriteLayer(layer, layer.GetType().Name, Constants.esriMappingNamespace);
                    else
                        layerXamlWriter.WriteLayer(layer, layer.GetType().Name, Constants.esriNamespace);
                }
                else
                {
                    //writer.WriteRaw(layer.ToString()); // write the ToString equivalent
                }
            }

            writer.WriteEndElement(); //esri:Map

            if(EncloseInUserControlTag)
                writer.WriteEndElement(); //UserControl

            writer.Flush();
            writer = null;
            string result = xaml.ToString();

            if (!EncloseInUserControlTag)
            {
                // Replace the generated XML for the outermost element with an esri:namespace
                // and ensure that default namespaces doesn't get overwritten by esri namespace
                result = result.Replace("<Map", "<esri:Map");
                result = result.Replace("</Map>", "</esri:Map>");
                result = result.Replace("xmlns=\"" + Constants.esriNamespace + "\"", "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            }
            else
            {
                // Inject default namespace
                result = result.Insert(13, "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" ");
            }

            if (writerSettings.Indent)
                result = result.Replace(" xmlns:", "\n\t xmlns:").Replace(" xmlns=\"", "\n\t xmlns=\"");
            return result;
        }
        //public string LayerToXaml(Layer layer)
        //{
        //    if (layer == null)
        //        return null;
        //    StringBuilder xaml = new StringBuilder();
        //    writer = XmlWriter.Create(xaml, writerSettings);
        //    writer.WriteStartElement("FeatureLayer");
        //    WriteNamespaces();
        //    WriteLayer(layer);
        //    writer.WriteEndElement();
        //    writer.Flush();
        //    writer = null;
        //    string result = xaml.ToString();
        //    int pos = result.IndexOf("<esri:FeatureLayer");
        //    if (pos > -1)
        //    {
        //        result = result.Substring(pos);
        //    }
        //    pos = result.LastIndexOf("</esri:FeatureLayer");
        //    if (pos > -1)
        //    {
        //        result = result.Substring(0, pos + 20);
        //    }
        //    return result;
        //}
        private void WriteResources(Dictionary<string, ResourceDictionaryEntry> dic)
        {
            foreach (string key in dic.Keys)
            {
                ResourceDictionaryEntry o = dic[key];
                if (o == null)
                    continue;
                IValueConverter valueConverter = o.Resource as IValueConverter;
                if (valueConverter != null)
                {                    
                    Type type = valueConverter.GetType();
                    writer.WriteStartElement(type.Name, Namespaces[o.Namespace]);
                    WriteXName(key);
                    writer.WriteEndElement();
                }
                else
                {
                    Write(o);
                    WriteXName(key);
                }
            }

            //foreach (KeyValuePair<object, object> pair in dic)
            //{

            //}
        }
        private void Write(object obj)
        {
            if (obj is Layer)
            {
                LayerXamlWriter layerXamlWriter = new LayerXamlWriter(writer, Namespaces);
                layerXamlWriter.WriteLayer(obj as Layer, obj.GetType().Name, Constants.esriNamespace);
            }
            else if (obj is Brush) 
            {
                new BrushXamlWriter(writer, Namespaces).WriteBrush(obj as Brush);
            }
            else if (obj is Symbol)
            {
                (new SymbolXamlWriter(writer, Namespaces)).WriteSymbol(obj as Symbol);
            }
            else if (obj is String) writer.WriteString(obj as string);
            else if (obj is double) writer.WriteString(((double)obj).ToString("0.0", CultureInfo.InvariantCulture));
            else throw new NotSupportedException(obj.GetType().ToString());
        }        
    }

    public class ResourceDictionaryEntry
    {
        public string Namespace;
        public object Resource;
    }    
}
