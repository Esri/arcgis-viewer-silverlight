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
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using System.Collections.ObjectModel;
using System.Globalization;
using ESRI.ArcGIS.Mapping.Core.Symbols;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class GraphicsLayerXamlWriter : LayerXamlWriter
    {
        public GraphicsLayerXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces)
            : base(writer, namespaces)
        {

        }

        protected override void WriteAttributes(Layer layer)
        {
            base.WriteAttributes(layer);
            GraphicsLayer graphicsLayer = layer as GraphicsLayer;
            if (graphicsLayer != null)
                WriteAttribute("RendererTakesPrecedence", graphicsLayer.RendererTakesPrecedence);
        }

        protected override void WriteElementContents(Layer layer)
        {
            base.WriteElementContents(layer);

            GraphicsLayer graphicsLayer = layer as GraphicsLayer;
            if (graphicsLayer != null)
            {
                WriteGraphicsLayerElements(graphicsLayer);
            }
        }

        protected void WriteGraphicsLayerElements(GraphicsLayer layer)
        {
            GeometryType geomType = (GeometryType)layer.GetValue(LayerExtensions.GeometryTypeProperty);
            if (geomType != default(GeometryType))
            {
                writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.GeometryType", Constants.esriMappingNamespace, geomType.ToString());
            }

            LinearGradientBrush defaultBrush = LayerExtensions.GetGradientBrush(layer);
            if (defaultBrush != null)
            {
                writer.WriteStartElement(Constants.esriMappingPrefix, "LayerExtensions.GradientBrush", Constants.esriMappingNamespace);
                BrushXamlWriter brushWriter = new BrushXamlWriter(writer, Namespaces);
                brushWriter.WriteBrush(defaultBrush);
                writer.WriteEndElement();
            }

            string rendererAttributeDisplayName = LayerExtensions.GetRendererAttributeDisplayName(layer);
            if (!string.IsNullOrEmpty(rendererAttributeDisplayName))
            {
                writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.RendererAttributeDisplayName", Constants.esriMappingNamespace, rendererAttributeDisplayName);
            }

            //string geomServiceUrl = layer.GetValue(LayerExtensions.GeometryServiceUrlProperty) as string;
            //if (!string.IsNullOrEmpty(geomServiceUrl))
            //{
            //    writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.GeometryServiceUrl", Constants.esriMappingNamespace, geomServiceUrl);
            //}   

            //SpatialReference mapSpatialRef = (SpatialReference)layer.GetValue(LayerExtensions.MapSpatialReferenceProperty);
            //if (mapSpatialRef != null)
            //{
            //    writer.WriteStartElement(Constants.esriMappingPrefix, "LayerExtensions.MapSpatialReference", Constants.esriMappingNamespace);
            //    WriteSpatialReference(writer, Namespaces, mapSpatialRef);
            //    writer.WriteEndElement();
            //}

            //SpatialReference layerSpatialRef = (SpatialReference)layer.GetValue(LayerExtensions.LayerSpatialReferenceProperty);
            //if (layerSpatialRef != null)
            //{
            //    writer.WriteStartElement(Constants.esriMappingPrefix, "LayerExtensions.LayerSpatialReference", Constants.esriMappingNamespace);
            //    WriteSpatialReference(writer, Namespaces, layerSpatialRef);
            //    writer.WriteEndElement();
            //}

            Collection<FieldInfo> fields = (Collection<FieldInfo>)layer.GetValue(LayerExtensions.FieldsProperty);
            if (fields != null)
            {
                writer.WriteStartElement(Constants.esriMappingPrefix, "LayerExtensions.Fields", Constants.esriMappingNamespace);
                FieldInfo.WriteFieldInfos(fields, writer);
                writer.WriteEndElement();
            }
            string displayField = LayerExtensions.GetDisplayField(layer);
            if (!string.IsNullOrEmpty(displayField))
            {
                writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.DisplayField", Constants.esriMappingNamespace, displayField);
            }

            if (LayerExtensions.GetPopUpsOnClick(layer))
                writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.PopUpsOnClick", Constants.esriMappingNamespace, "True");
            else
                writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.PopUpsOnClick", Constants.esriMappingNamespace, "False");
            //string mapTipContainerXaml = layer.GetValue(LayerExtensions.MapTipContainerXamlProperty) as string;
            //if (!string.IsNullOrEmpty(mapTipContainerXaml))
            //{
            //    writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.MapTipContainerXaml", Constants.esriMappingNamespace, mapTipContainerXaml);

            //    writer.WriteStartElement("GraphicsLayer.MapTip", Constants.esriNamespace);

            //    writer.WriteStartElement("ContentControl");
            //    writer.WriteRaw(mapTipContainerXaml);
            //    writer.WriteEndElement();

            //    writer.WriteEndElement();
            //}

            if (layer.Clusterer != null)
            {
                WriteLayerClusterer(layer.Clusterer);
            }
            if (layer.Renderer != null)
            {
                WriteRenderer(layer.Renderer);
            }
            if (layer.Graphics.Count > 0)
            {
                WriteGraphicsDataset(layer);
            }

            //if (layer.MapTip != null)
            //{                
            //writer.WriteStartElement("GraphicsLayer.MapTip", Namespaces[Constants.esriPrefix]);
            //if (LayerMapTipFormatting != null)
            //{
            //    LayerMapTipFormattingEventArgs args = new LayerMapTipFormattingEventArgs
            //    {
            //        Layer = layer
            //    };
            //    LayerMapTipFormatting(this, args);
            //    string mapTipXaml = args.LayerMapTipXaml;
            //    if (!string.IsNullOrEmpty(mapTipXaml))
            //    {
            //        writer.WriteRaw(mapTipXaml);
            //        //Write(MapTipXaml);
            //    }
            //    else
            //    {
            //        // Must write a content template.
            //        writer.WriteRaw("<ContentControl />");
            //    }
            //}

            //writer.WriteEndElement();
            //}
        }

        private void WriteGraphicsDataset(GraphicsLayer graphicLayer)
        {
            if (graphicLayer == null 
            || graphicLayer is ICustomGraphicsLayer
            || (graphicLayer is FeatureLayer && !string.IsNullOrEmpty(((FeatureLayer)graphicLayer).Url))
            || graphicLayer.Graphics.Count < 1)
                return;
            
            List<PersistedGraphic> persistedGraphics = new List<PersistedGraphic>();
            foreach (Graphic graphic in graphicLayer.Graphics)
                persistedGraphics.Add(new PersistedGraphic(graphic));
            string json = null;
            try
            {
                json = JsonSerializer.Serialize<List<PersistedGraphic>>(persistedGraphics,
                    new Type[]{
                     typeof(ImageFillSymbol),typeof(MarkerSymbol),
                }
                    );
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            if (!string.IsNullOrEmpty(json))
            {
                writer.WriteStartElement(Constants.esriMappingPrefix, "LayerExtensions.Dataset", Constants.esriMappingNamespace);
                writer.WriteRaw(json);
                writer.WriteEndElement();
            }
        }

        protected virtual void WriteRenderer(IRenderer renderer)
        {
            if (IsSerializable(renderer))
            {
                ClassBreaksRenderer classBreaksRenderer = renderer as ClassBreaksRenderer;
                if (classBreaksRenderer != null)
                {
                    writer.WriteStartElement("GraphicsLayer.Renderer", Namespaces[Constants.esriPrefix]);
                    WriteClassBreaksRenderer(classBreaksRenderer);
                    writer.WriteEndElement();
                }
                else
                {
                    UniqueValueRenderer uniqueValueRenderer = renderer as UniqueValueRenderer;
                    if (uniqueValueRenderer != null)
                    {
                        writer.WriteStartElement("GraphicsLayer.Renderer", Namespaces[Constants.esriPrefix]);
                        WriteUniqueValueRenderer(uniqueValueRenderer);
                        writer.WriteEndElement();
                    }
                    else
                    {
                        SimpleRenderer simpleRenderer = renderer as SimpleRenderer;
                        if (simpleRenderer != null)
                        {
                            writer.WriteStartElement("GraphicsLayer.Renderer", Namespaces[Constants.esriPrefix]);
                            WriteSimpleValueRenderer(simpleRenderer);
                            writer.WriteEndElement();
                        }
                    }
                }
            }
        }

        private static bool IsSerializable(IRenderer renderer)
        {
            ClassBreaksRenderer classBreaksRenderer = renderer as ClassBreaksRenderer;
            if (classBreaksRenderer != null)
            {
                if (classBreaksRenderer.Classes != null)
                {
                    foreach (ClassBreakInfo info in classBreaksRenderer.Classes)
                    {
                        if (!SymbolXamlWriter.IsSerializable(info.Symbol))
                        {
                            return false;
                        }
                    }

                    if (!SymbolXamlWriter.IsSerializable(classBreaksRenderer.DefaultSymbol))
                        return false;
                }
            }
            else
            {
                UniqueValueRenderer uniqueValueRenderer = renderer as UniqueValueRenderer;
                if (uniqueValueRenderer != null)
                {
                    if (uniqueValueRenderer.Infos != null)
                    {
                        foreach (UniqueValueInfo info in uniqueValueRenderer.Infos)
                        {
                            if (!SymbolXamlWriter.IsSerializable(info.Symbol))
                            {
                                return false;
                            }
                        }

                        if (!SymbolXamlWriter.IsSerializable(uniqueValueRenderer.DefaultSymbol))
                            return false;
                    }
                }
                else
                {
                    SimpleRenderer simpleRenderer = renderer as SimpleRenderer;
                    if (simpleRenderer != null)
                    {
                        if (!SymbolXamlWriter.IsSerializable(simpleRenderer.Symbol))
                            return false;
                    }
                }
            }
            return true;
        }

        private void WriteUniqueValueRenderer(UniqueValueRenderer uniqueValueRenderer)
        {
            writer.WriteStartElement("UniqueValueRenderer", Namespaces[Constants.esriPrefix]);
			if (!string.IsNullOrEmpty(uniqueValueRenderer.Field))
                WriteAttribute("Field", uniqueValueRenderer.Field);
			WriteAttribute("DefaultLabel", uniqueValueRenderer.DefaultLabel);
            if (uniqueValueRenderer.DefaultSymbol != null)
            {
                writer.WriteStartElement("UniqueValueRenderer.DefaultSymbol", Namespaces[Constants.esriPrefix]);
                (new SymbolXamlWriter(writer, Namespaces)).WriteSymbol(uniqueValueRenderer.DefaultSymbol);
                writer.WriteEndElement();
            }
            if (uniqueValueRenderer.Infos != null && uniqueValueRenderer.Infos.Count > 0)
            {
                writer.WriteStartElement("UniqueValueRenderer.Infos", Namespaces[Constants.esriPrefix]);

                foreach (ESRI.ArcGIS.Client.UniqueValueInfo uniqueValueInfo in uniqueValueRenderer.Infos)
                {
                    WriteUniqueValueInfo(uniqueValueInfo);
                }

                writer.WriteEndElement();// UniqueValueRenderer.Infos
            }
            writer.WriteEndElement();
        }

        private void WriteSimpleValueRenderer(SimpleRenderer simpleRenderer)
        {
            writer.WriteStartElement("SimpleRenderer", Namespaces[Constants.esriPrefix]);
            if (simpleRenderer.Symbol != null)
            {
                writer.WriteStartElement("SimpleRenderer.Symbol", Namespaces[Constants.esriPrefix]);
                (new SymbolXamlWriter(writer, Namespaces)).WriteSymbol(simpleRenderer.Symbol);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void WriteUniqueValueInfo(UniqueValueInfo uniqueValueInfo)
        {
            if (uniqueValueInfo == null)
                return;

            UniqueValueInfoObj uniqueValueInfoObj = uniqueValueInfo as UniqueValueInfoObj;
            if (uniqueValueInfoObj != null)
            {
                writer.WriteStartElement("UniqueValueInfoObj", Namespaces[Constants.esriMappingPrefix]);
                if (uniqueValueInfoObj.Value != null)
                {
                    writer.WriteAttributeString("FieldType", uniqueValueInfoObj.FieldType.ToString());
                    WriteRendererInfoAttributes(uniqueValueInfo);
                    writeUniqueValueObj(uniqueValueInfoObj.SerializedValue);
                }
            }
            else
            {
                writer.WriteStartElement("UniqueValueInfo", Namespaces[Constants.esriPrefix]);
                WriteRendererInfoAttributes(uniqueValueInfo);
                if (uniqueValueInfo.Value != null)
                {
                    writeUniqueValue(uniqueValueInfo.Value);
                }
            }

            if (uniqueValueInfo.Symbol != null)
            {
                writer.WriteStartElement("UniqueValueInfo.Symbol", Namespaces[Constants.esriPrefix]);
                (new SymbolXamlWriter(writer, Namespaces)).WriteSymbol(uniqueValueInfo.Symbol);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void writeUniqueValueObj(object uniqueValueInfo)
        {
            if (uniqueValueInfo == null)
                return;

            NumericFieldValue numericFieldValue = uniqueValueInfo as NumericFieldValue;
            if (numericFieldValue != null)
            {
                writer.WriteStartElement("UniqueValueInfoObj.SerializedValue", Namespaces[Constants.esriMappingPrefix]);
                writeNumericFieldValue(numericFieldValue);
                writer.WriteEndElement();
                return;
            }

            AttachmentFieldValue attFieldValue = uniqueValueInfo as AttachmentFieldValue;
            if (attFieldValue != null)
            {
                writer.WriteStartElement("UniqueValueInfoObj.SerializedValue", Namespaces[Constants.esriMappingPrefix]);
                writeAttachmentFieldValue(attFieldValue);
                writer.WriteEndElement();
                return;
            }

            CurrencyFieldValue currFieldValue = uniqueValueInfo as CurrencyFieldValue;
            if (currFieldValue != null)
            {
                writer.WriteStartElement("UniqueValueInfoObj.SerializedValue", Namespaces[Constants.esriMappingPrefix]);
                writeCurrencyFieldValue(currFieldValue);
                writer.WriteEndElement();
                return;
            }

            EntityFieldValue entityFieldValue = uniqueValueInfo as EntityFieldValue;
            if (entityFieldValue != null)
            {
                writer.WriteStartElement("UniqueValueInfoObj.SerializedValue", Namespaces[Constants.esriMappingPrefix]);
                writeEntityFieldValue(entityFieldValue);
                writer.WriteEndElement();
                return;
            }

            HyperlinkFieldValue hyperlinkFieldValue = uniqueValueInfo as HyperlinkFieldValue;
            if (hyperlinkFieldValue != null)
            {
                writer.WriteStartElement("UniqueValueInfoObj.SerializedValue", Namespaces[Constants.esriMappingPrefix]);
                writeHyperlinkFieldValue(hyperlinkFieldValue);
                writer.WriteEndElement();
                return;
            }

            HyperlinkImageFieldValue hyperlinkImageFieldValue = uniqueValueInfo as HyperlinkImageFieldValue;
            if (hyperlinkImageFieldValue != null)
            {
                writer.WriteStartElement("UniqueValueInfoObj.SerializedValue", Namespaces[Constants.esriMappingPrefix]);
                writeHyperlinkImageFieldValue(hyperlinkImageFieldValue);
                writer.WriteEndElement();
                return;
            }

            LookupFieldValue lookupFieldValue = uniqueValueInfo as LookupFieldValue;
            if (lookupFieldValue != null)
            {
                writer.WriteStartElement("UniqueValueInfoObj.SerializedValue", Namespaces[Constants.esriMappingPrefix]);
                writeLookupFieldValue(lookupFieldValue);
                writer.WriteEndElement();
                return;
            }

            DateTimeFieldValue dateTimeFieldValue = uniqueValueInfo as DateTimeFieldValue;
            if (dateTimeFieldValue != null)
            {
                writer.WriteStartElement("UniqueValueInfoObj.SerializedValue", Namespaces[Constants.esriMappingPrefix]);
                writeDateTimeFieldValue(dateTimeFieldValue);
                writer.WriteEndElement();
                return;
            }

            if (uniqueValueInfo is double)
                writer.WriteAttributeString("SerializedValue", ((double)uniqueValueInfo).ToString(CultureInfo.InvariantCulture));
            else
                writer.WriteAttributeString("SerializedValue", uniqueValueInfo.ToString());
        }

        private void writeDateTimeFieldValue(DateTimeFieldValue dateTimeFieldValue)
        {
            if (dateTimeFieldValue == null)
                return;

            writer.WriteStartElement("DateTimeFieldValue", Namespaces[Constants.esriMappingPrefix]);
            if (!string.IsNullOrEmpty(dateTimeFieldValue.FormattedValue))
                writer.WriteAttributeString("FormattedValue", dateTimeFieldValue.FormattedValue);
            if (dateTimeFieldValue.Value != null)
                writer.WriteAttributeString("Value", dateTimeFieldValue.Value.ToString());
            writer.WriteEndElement();
        }

        private void writeNumericFieldValue(NumericFieldValue numericFieldValue)
        {
            if (numericFieldValue == null)
                return;

            writer.WriteStartElement("NumericFieldValue", Namespaces[Constants.esriMappingPrefix]);
            if (!string.IsNullOrEmpty(numericFieldValue.FormattedValue))
                writer.WriteAttributeString("FormattedValue", numericFieldValue.FormattedValue);
            if (!double.IsNaN(numericFieldValue.Value))
                writer.WriteAttributeString("Value", numericFieldValue.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }

        private void writeLookupFieldValue(LookupFieldValue lookupFieldValue)
        {
            if (lookupFieldValue == null)
                return;

            writer.WriteStartElement("LookupFieldValue", Namespaces[Constants.esriMappingPrefix]);
            if (!string.IsNullOrEmpty(lookupFieldValue.DisplayText))
                writer.WriteAttributeString("DisplayText", lookupFieldValue.DisplayText);
            if (!string.IsNullOrEmpty(lookupFieldValue.LinkUrl))
                writer.WriteAttributeString("LinkUrl", lookupFieldValue.LinkUrl);
            writer.WriteEndElement();
        }

        private void writeHyperlinkImageFieldValue(HyperlinkImageFieldValue hyperlinkImageFieldValue)
        {
            if (hyperlinkImageFieldValue == null)
                return;

            writer.WriteStartElement("HyperlinkImageFieldValue", Namespaces[Constants.esriMappingPrefix]);
            if (!string.IsNullOrEmpty(hyperlinkImageFieldValue.ImageTooltip))
                writer.WriteAttributeString("ImageTooltip", hyperlinkImageFieldValue.ImageTooltip);
            if (!string.IsNullOrEmpty(hyperlinkImageFieldValue.ImageUrl))
                writer.WriteAttributeString("ImageUrl", hyperlinkImageFieldValue.ImageUrl);
            writer.WriteEndElement();
        }

        private void writeHyperlinkFieldValue(HyperlinkFieldValue hyperlinkFieldValue)
        {
            if (hyperlinkFieldValue == null)
                return;

            writer.WriteStartElement("HyperlinkFieldValue", Namespaces[Constants.esriMappingPrefix]);
            if (!string.IsNullOrEmpty(hyperlinkFieldValue.DisplayText))
                writer.WriteAttributeString("DisplayText", hyperlinkFieldValue.DisplayText);
            if (!string.IsNullOrEmpty(hyperlinkFieldValue.LinkUrl))
                writer.WriteAttributeString("LinkUrl", hyperlinkFieldValue.LinkUrl);
            writer.WriteEndElement();
        }

        private void writeEntityFieldValue(EntityFieldValue entityFieldValue)
        {
            if (entityFieldValue == null)
                return;

            writer.WriteStartElement("EntityFieldValue", Namespaces[Constants.esriMappingPrefix]);
            if (!string.IsNullOrEmpty(entityFieldValue.DisplayText))
                writer.WriteAttributeString("DisplayText", entityFieldValue.DisplayText);
            if (!string.IsNullOrEmpty(entityFieldValue.LinkUrl))
                writer.WriteAttributeString("LinkUrl", entityFieldValue.LinkUrl);
            writer.WriteEndElement();
        }

        private void writeCurrencyFieldValue(CurrencyFieldValue currFieldValue)
        {
            if (currFieldValue == null)
                return;

            writer.WriteStartElement("CurrencyFieldValue", Namespaces[Constants.esriMappingPrefix]);
            if (!string.IsNullOrEmpty(currFieldValue.FormattedValue))
                writer.WriteAttributeString("FormattedValue", currFieldValue.FormattedValue);
            if (!double.IsNaN(currFieldValue.Value))
                writer.WriteAttributeString("Value", currFieldValue.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }

        private void writeAttachmentFieldValue(AttachmentFieldValue attFieldValue)
        {
            if (attFieldValue == null)
                return;

            writer.WriteStartElement("AttachmentFieldValue", Namespaces[Constants.esriMappingPrefix]);
            if (!string.IsNullOrEmpty(attFieldValue.DisplayText))
                writer.WriteAttributeString("DisplayText", attFieldValue.DisplayText);
            if (!string.IsNullOrEmpty(attFieldValue.LinkUrl))
                writer.WriteAttributeString("LinkUrl", attFieldValue.LinkUrl);
            writer.WriteEndElement();
        }

        private void writeUniqueValue(object uniqueValue)
        {
            if (uniqueValue == null)
                return;

            string str = uniqueValue as string;
            if (str != null)
            {
                writer.WriteAttributeString("Value", str);
                return;
            }
            else if (uniqueValue is double)
            {
                writer.WriteAttributeString("Value", uniqueValue.ToString());
                return;
            }

            if (uniqueValue is DateTime)
            {
                writer.WriteStartElement("UniqueValueInfo.Value", Namespaces[Constants.esriPrefix]);
                writer.WriteElementString("String", Namespaces[Constants.sysPrefix], ((DateTime)uniqueValue).ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
                return;
            }

            if (uniqueValue is int)
            {
                writer.WriteStartElement("UniqueValueInfo.Value", Namespaces[Constants.esriPrefix]);
                writer.WriteElementString("Int32", Namespaces[Constants.sysPrefix], ((int)uniqueValue).ToString());
                writer.WriteEndElement();
                return;
            }

            if (uniqueValue is Int32)
            {
                writer.WriteStartElement("UniqueValueInfo.Value", Namespaces[Constants.esriPrefix]);
                writer.WriteElementString("Int32", Namespaces[Constants.sysPrefix], ((Int32)uniqueValue).ToString());
                writer.WriteEndElement();
                return;
            }

            if (uniqueValue is Int64)
            {
                writer.WriteStartElement("UniqueValueInfo.Value", Namespaces[Constants.esriPrefix]);
                writer.WriteElementString("Int64", Namespaces[Constants.sysPrefix], ((Int64)uniqueValue).ToString());
                writer.WriteEndElement();
                return;
            }

            if (uniqueValue is bool)
            {
                writer.WriteStartElement("UniqueValueInfo.Value", Namespaces[Constants.esriPrefix]);
                writer.WriteElementString("Boolean", Namespaces[Constants.sysPrefix], ((bool)uniqueValue).ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
                return;
            }

            writer.WriteAttributeString("Value", uniqueValue.ToString());
        }

        protected void WriteClassBreaksRenderer(ClassBreaksRenderer cbRenderer)
        {
            writer.WriteStartElement("ClassBreaksRenderer", Namespaces[Constants.esriPrefix]);
			if (!string.IsNullOrEmpty(cbRenderer.Field))
				writer.WriteAttributeString("Field", cbRenderer.Field);
			if (cbRenderer.DefaultSymbol != null)
            {
                writer.WriteStartElement("ClassBreaksRenderer.DefaultSymbol", Namespaces[Constants.esriPrefix]);
                (new SymbolXamlWriter(writer, Namespaces)).WriteSymbol(cbRenderer.DefaultSymbol);
                writer.WriteEndElement();
            }
            if (cbRenderer.Classes != null && cbRenderer.Classes.Count > 0)
            {
                writer.WriteStartElement("ClassBreaksRenderer.Classes", Namespaces[Constants.esriPrefix]);

                foreach (ESRI.ArcGIS.Client.ClassBreakInfo classBreak in cbRenderer.Classes)
                {
                    WriteClassBreakInfo(classBreak);
                }

                writer.WriteEndElement();// ClassBreaksRenderer.Classes
            }
            writer.WriteEndElement();
        }

        #region clusterer
        protected void WriteLayerClusterer(Clusterer clusterer)
        {
            if (clusterer is FlareClusterer)
            {
                WriteFlareClusterer(clusterer as FlareClusterer);
            }
            else throw new NotSupportedException(Resources.Strings.ExceptionClusterer);
        }

        protected void WriteFlareClusterer(FlareClusterer flareClusterer)
        {
            writer.WriteStartElement("GraphicsLayer.Clusterer", Namespaces[Constants.esriPrefix]);
            writer.WriteStartElement("FlareClusterer", Namespaces[Constants.esriPrefix]);
            //Attributes...
            if (flareClusterer.Radius != 20)
                WriteAttribute("Radius", flareClusterer.Radius);
            if (flareClusterer.MaximumFlareCount != 10)
                WriteAttribute("MaximumFlareCount", flareClusterer.MaximumFlareCount);
            if (flareClusterer.FlareBackground is SolidColorBrush)
            {
                if ((flareClusterer.FlareBackground as SolidColorBrush).Color != Colors.Red)
                {
                    WriteAttribute("FlareBackground", (flareClusterer.FlareBackground as SolidColorBrush).Color);
                }
            }
            if (flareClusterer.FlareForeground is SolidColorBrush)
            {
                if ((flareClusterer.FlareForeground as SolidColorBrush).Color != Colors.White)
                {
                    WriteAttribute("FlareForeground", (flareClusterer.FlareForeground as SolidColorBrush).Color);
                }
            }
            //Elements...
            if (!(flareClusterer.FlareBackground is SolidColorBrush))
            {
                writer.WriteStartElement("FlareClusterer.FlareBackground", Namespaces[Constants.esriPrefix]);
                new BrushXamlWriter(writer, Namespaces).WriteBrush(flareClusterer.FlareBackground);
                writer.WriteEndElement();
            }
            if (!(flareClusterer.FlareForeground is SolidColorBrush))
            {
                writer.WriteStartElement("FlareClusterer.FlareForeground", Namespaces[Constants.esriPrefix]);
                new BrushXamlWriter(writer, Namespaces).WriteBrush(flareClusterer.FlareForeground);
                writer.WriteEndElement();
            }
            if (flareClusterer.Gradient != null)
            {
                writer.WriteStartElement("FlareClusterer.Gradient", Namespaces[Constants.esriPrefix]);
                new BrushXamlWriter(writer, Namespaces).WriteBrush(flareClusterer.Gradient);
                writer.WriteEndElement();
            }
            writer.WriteEndElement(); //FlareClusterer
            writer.WriteEndElement(); //GraphicsLayer.Clusterer
        }
        #endregion

        private void WriteRendererInfoAttributes(ESRI.ArcGIS.Client.RendererInfo rendererInfo)
        {
            if (!string.IsNullOrWhiteSpace(rendererInfo.Label))
                writer.WriteAttributeString("Label", rendererInfo.Label);
            if (!string.IsNullOrWhiteSpace(rendererInfo.Description))
                writer.WriteAttributeString("Description", rendererInfo.Description);
        }

        private void WriteClassBreakInfo(ESRI.ArcGIS.Client.ClassBreakInfo classBreak)
        {
            writer.WriteStartElement("ClassBreakInfo", Namespaces[Constants.esriPrefix]);
            WriteAttribute("MinimumValue", classBreak.MinimumValue);
            WriteAttribute("MaximumValue", classBreak.MaximumValue);
            WriteRendererInfoAttributes(classBreak);

            if (classBreak.Symbol != null)
            {
                writer.WriteStartElement("ClassBreakInfo.Symbol", Namespaces[Constants.esriPrefix]);
                (new SymbolXamlWriter(writer, Namespaces)).WriteSymbol(classBreak.Symbol);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        //protected void WriteSpatialReference(System.Xml.XmlWriter writer, Dictionary<string, string> Namespaces, SpatialReference sRef)
        //{
        //    if (sRef == null)
        //        return;

        //    writer.WriteStartElement("SpatialReference", Constants.esriNamespace);
        //    if (sRef.WKID != default(int))
        //        writer.WriteAttributeString("WKID", sRef.WKID.ToString());
        //    if (!string.IsNullOrEmpty(sRef.WKT))
        //        writer.WriteAttributeString("WKT", sRef.WKT);
        //    writer.WriteEndElement();            
        //}
    }
}
