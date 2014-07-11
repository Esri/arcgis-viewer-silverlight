/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Client.Geometry;
using System.Globalization;
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class LayerXamlWriter : XamlWriterBase
    {
        public LayerXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces) : base(writer, namespaces) 
        { 
        }

        #region Layer              

        protected virtual void WriteStartElement(string layerName, string layerNamespace)
        {
            writer.WriteStartElement(layerName, layerNamespace);
        }

        protected virtual void WriteEndElement()
        {
            writer.WriteEndElement();
        }

        protected virtual void WriteAttributes(Layer layer)
        {
            WriteBaseLayerAttributes(layer);
        }

        protected virtual void WriteElementContents(Layer layer)
        {
            WriteBaseElementContents(layer);
        }

        protected void WriteSpatialReferenceAsAttribute(string propertyName, SpatialReference sRef)
        {
            if (sRef == null)
                return;
            if (!string.IsNullOrEmpty(sRef.WKT))
                writer.WriteAttributeString(propertyName, sRef.WKT);
            else if (sRef.WKID != default(int))
                writer.WriteAttributeString(propertyName, sRef.WKID.ToString(CultureInfo.InvariantCulture));
        }

        protected virtual void WriteBaseElementContents(Layer layer)
        {
            // Write extended layer properties
            bool isBaseMapLayer = (bool)layer.GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty);
            if (isBaseMapLayer)
            {
                writer.WriteElementString(Constants.esriExtensibilityPrefix, "Document.IsBaseMap", Constants.esriExtensibilityNamespace, "true");
            }
            if (LayerExtensions.GetIsReferenceLayer(layer))
            {
                writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.IsReferenceLayer", Constants.esriMappingNamespace, "true");
            }

            if (LayerExtensions.GetUsesProxy(layer))
            {
                writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.UsesProxy", Constants.esriMappingNamespace, "true");
            }

            string displayName = layer.GetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty) as string;
            if (!string.IsNullOrEmpty(displayName))
            {
                writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.LayerName", Constants.esriMappingNamespace, displayName);
            }

            string displayUrl = (string)layer.GetValue(LayerExtensions.DisplayUrlProperty);
            if (!string.IsNullOrEmpty(displayUrl))
            {
                writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.DisplayUrl", Constants.esriMappingNamespace, displayUrl);
            }
            // Auto-Update is only available for certain layer types
            if (layer is FeatureLayer || layer is ArcGISDynamicMapServiceLayer || layer is CustomGraphicsLayer)
            {
                double autoUpdateInterval = LayerExtensions.GetAutoUpdateInterval(layer);
                // We only store the Interval value if it is > 0.  The Interval indicates
                // if auto update is being used.  A value of <=0 means we are not using 
                // auto update.
                if (autoUpdateInterval > 0.0d)
                {
                    writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.AutoUpdateInterval",
                                              Constants.esriMappingNamespace, autoUpdateInterval.ToString());

                }
                if (LayerExtensions.GetAutoUpdateOnExtentChanged(layer))
                    writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.AutoUpdateOnExtentChanged", Constants.esriMappingNamespace, "True");

            }
            Collection<int> identifyLayerIds = LayerExtensions.GetIdentifyLayerIds(layer);
            if (identifyLayerIds != null && identifyLayerIds.Count > 0)
            {
                writer.WriteStartElement(Constants.esriMappingPrefix, "LayerExtensions.IdentifyLayerIds", Constants.esriMappingNamespace);
                foreach (int id in identifyLayerIds)
                {
                    writer.WriteStartElement(Constants.sysPrefix, "Int32", Constants.sysNamespace);
                    writer.WriteValue(id);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            Collection<LayerInformation> layerInfos = LayerExtensions.GetLayerInfos(layer);
            if (layerInfos != null)
            {
                writer.WriteStartElement(Constants.esriMappingPrefix, "LayerExtensions.LayerInfos", Constants.esriMappingNamespace);
                LayerInformation.WriteLayerInfos(layerInfos, writer);
                writer.WriteEndElement();
            }

            bool usePopupFromWebMap = LayerExtensions.GetUsePopupFromWebMap(layer);
            if (usePopupFromWebMap)
            {
                writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.UsePopupFromWebMap", Constants.esriMappingNamespace, "true");
            }

			string popupDataTemplates = LayerExtensions.GetSerializedPopupDataTemplates(layer);
            if (!string.IsNullOrEmpty(popupDataTemplates))
			{
                writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.SerializedPopupDataTemplates", Constants.esriMappingNamespace, popupDataTemplates);
			}

			string popupTitles = LayerExtensions.GetSerializedPopupTitleExpressions(layer);
			if (!string.IsNullOrEmpty(popupTitles))
			{
                writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.SerializedPopupTitleExpressions", Constants.esriMappingNamespace, popupTitles);
			}

            popupDataTemplates = LayerExtensions.GetSerializedWebMapPopupDataTemplates(layer);
            if (!string.IsNullOrEmpty(popupDataTemplates))
            {
                writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.SerializedWebMapPopupDataTemplates", Constants.esriMappingNamespace, popupDataTemplates);
            }

            popupTitles = LayerExtensions.GetSerializedWebMapPopupTitleExpressions(layer);
            if (!string.IsNullOrEmpty(popupTitles))
            {
                writer.WriteElementString(Constants.esriMappingPrefix, "LayerExtensions.SerializedWebMapPopupTitleExpressions", Constants.esriMappingNamespace, popupTitles);
            }

			bool isPopupEnabled = ESRI.ArcGIS.Client.Extensibility.LayerProperties.GetIsPopupEnabled(layer);
			if (isPopupEnabled)
				writer.WriteElementString(Constants.esriExtensibilityPrefix, "LayerProperties.IsPopupEnabled", Constants.esriExtensibilityNamespace, "true");
			else
				writer.WriteElementString(Constants.esriExtensibilityPrefix, "LayerProperties.IsPopupEnabled", Constants.esriExtensibilityNamespace, "false");
            
            if (ESRI.ArcGIS.Client.Extensibility.LayerProperties.GetIsVisibleInMapContents(layer))
                writer.WriteElementString(Constants.esriExtensibilityPrefix, "LayerProperties.IsVisibleInMapContents", Constants.esriExtensibilityNamespace, "true");
            else
                writer.WriteElementString(Constants.esriExtensibilityPrefix, "LayerProperties.IsVisibleInMapContents", Constants.esriExtensibilityNamespace, "false");
		}

        public virtual void WriteLayer(Layer layer, string layerName, string layerNamespace)
        {
            WriteStartElement(layerName, layerNamespace);
            WriteAttributes(layer);
            WriteElementContents(layer);
            WriteEndElement();
        }

        protected void WriteBaseLayerAttributes(Layer layer)
        {
            if (!string.IsNullOrEmpty(layer.ID))
            {
                WriteAttribute("ID", layer.ID);
            }
            if (!layer.Visible)
            {
                WriteAttribute("Visible", "False");
            }
            if (layer.Opacity < 1)
            {
                WriteAttribute("Opacity", layer.Opacity);
            }

        }      

        //protected void WriteFrameworkElement(FrameworkElement element)
        //{
        //    if (element is Panel) //Has Children
        //    {
        //        if (element is Grid)
        //        {
        //            writer.WriteStartElement("Grid");
        //        }
        //        else if (element is StackPanel)
        //        {
        //            writer.WriteStartElement("StackPanel");
        //        }
        //        else if (element is Canvas)
        //        {
        //            writer.WriteStartElement("Canvas");
        //        }
        //        else
        //            throw new NotSupportedException(element.GetType().FullName);

        //        foreach (UIElement e in (element as Panel).Children)
        //        {
        //            if (e is FrameworkElement)
        //            {
        //                WriteFrameworkElement(e as FrameworkElement);
        //            }
        //            else
        //                throw new NotSupportedException(e.GetType().FullName);
        //        }
        //        writer.WriteEndElement();
        //    }
        //    else if (element is TextBlock)
        //    {
        //        TextBlock t = element as TextBlock;
        //        writer.WriteStartElement("TextBlock");
        //        if (!String.IsNullOrEmpty(t.Text))
        //            WriteAttribute("Text", t.Text);
        //        writer.WriteEndElement();
        //    }
        //    else if (element is Border)
        //    {
        //        Border border = element as Border;
        //        writer.WriteStartElement("Border");
        //        WriteAttribute("BorderThickness",
        //            string.Format("{0},{1},{2},{3}", border.BorderThickness.Left, border.BorderThickness.Top,
        //            border.BorderThickness.Right, border.BorderThickness.Bottom));
        //        writer.WriteStartElement("Border.Brush");
        //        new BrushXamlWriter(writer, Namespaces).WriteBrush(border.BorderBrush);
        //        writer.WriteEndElement();
        //        writer.WriteEndElement();
        //    }
        //    else
        //        throw new NotSupportedException(element.GetType().FullName);
        //}
             
        
        #endregion        

    }

}
