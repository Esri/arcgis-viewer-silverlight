/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Mapping.Core;
using System.Xml.Linq;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class SymbolXamlWriter : XamlWriterBase
    {
        public SymbolXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces)
            : base(writer, namespaces)
        {
        }

        #region Symbols

        void WriteJsonAttribute(IJsonSerializable serializable)
        {
            if (serializable != null)
            {
                string json = SymbolUtils.GetJsonForSymbol(serializable);
                if (!string.IsNullOrEmpty(json))
                    writer.WriteElementString(Constants.esriMappingPrefix, "SymbolExtensions.Json",
                       Constants.esriMappingNamespace, json);
            }
        }

        public void WriteSymbol(Symbol symbol)
        {
            if (IsSerializable(symbol))
            {
                FillSymbol fillSymbol = symbol as FillSymbol;
                if (fillSymbol != null)
                {
                    WriteFillSymbol(fillSymbol);
                }
                else
                {
                    LineSymbol lineSymbol = symbol as LineSymbol;
                    if (lineSymbol != null)
                    {
                        WriteLineSymbol(lineSymbol);
                    }
                    else
                    {
                        MarkerSymbol markerSymbol = symbol as MarkerSymbol;
                        if (markerSymbol != null)
                        {
                            WriteMarkerSymbol(markerSymbol);
                        }
                        else
                            throw new NotSupportedException(symbol.GetType().FullName);
                    }
                }
            }
        }

        private void WriteMarkerSymbol(MarkerSymbol markerSymbol)
        {
            ICustomSymbol symbol = markerSymbol as ICustomSymbol;
            if (symbol != null)
            {
                symbol.Serialize(writer, Namespaces);
                return;
            }

            string prefix = Constants.esriPrefix;
            if (markerSymbol is ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol)
                prefix = Constants.esriMappingPrefix;
            else if (markerSymbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol ||
                    markerSymbol is ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol)
                prefix = Constants.esriFSSymbolsPrefix;
            StartType(markerSymbol, prefix);

            if (markerSymbol is SimpleMarkerSymbol)
            {
                #region Size,Style,Color
                SimpleMarkerSymbol sms = markerSymbol as SimpleMarkerSymbol;
                WriteAttribute("Size", sms.Size);
                if (sms.Style != SimpleMarkerSymbol.SimpleMarkerStyle.Circle)
                {
                    WriteAttribute("Style", sms.Style.ToString());
                }
                if (sms.Color is SolidColorBrush && sms.Color.Opacity == 1)
                {
                    WriteAttribute("Color", (sms.Color as SolidColorBrush).Color);
                }
                else
                {                    
                    writer.WriteStartElement("SimpleMarkerSymbol.Color", Namespaces[Constants.esriPrefix]);
                    new BrushXamlWriter(writer, Namespaces).WriteBrush(sms.Color);
                    writer.WriteEndElement();
                }
                #endregion
            }
            else if (markerSymbol is ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol || markerSymbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol)
            {
                WriteJsonAttribute(markerSymbol as IJsonSerializable);
            }
            else
            {
                if (markerSymbol is ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol)
                {
                    #region Size,OriginX, OriginY, Opacity
                    ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol marker = markerSymbol as ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol;
                    if (marker != null)
                    {
                        WriteAttribute("Size", marker.Size);
                        WriteAttribute("OriginX", marker.OriginX);
                        WriteAttribute("OriginY", marker.OriginY);
                        if (marker.Opacity < 1)
                            WriteAttribute("Opacity", marker.Opacity);
                    }
                    #endregion
                }
                else if (!(markerSymbol is ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol || markerSymbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol))
                {
                    #region OffsetX, OffsetY
                    if (markerSymbol.OffsetX != 0)
                        WriteAttribute("OffsetX", markerSymbol.OffsetX);
                    if (markerSymbol.OffsetY != 0)
                        WriteAttribute("OffsetY", markerSymbol.OffsetY);
                    #endregion
                }

                PictureMarkerSymbol pms = markerSymbol as PictureMarkerSymbol;
                if (pms != null)
                {
                    #region PMS
                    WriteAttribute("Width", pms.Width);
                    WriteAttribute("Height", pms.Height);
                    if (pms.Opacity < 1)
                        WriteAttribute("Opacity", pms.Opacity);
                    string imageSource = SymbolExtensions.GetOriginalSource(pms);
                    if (!string.IsNullOrEmpty(imageSource))
                        writer.WriteElementString(Constants.esriMappingPrefix, "SymbolExtensions.OriginalSource", Constants.esriMappingNamespace, imageSource);
                    else
                    {
                        BitmapImage bmp = pms.Source as BitmapImage;
                        if (bmp != null && bmp.UriSource != null)
                        {
                            WriteAttribute("Source", bmp.UriSource.OriginalString);
                        }
                    }
                    #endregion
                }
                else if (markerSymbol is TextSymbol)
                {
                    #region TS
                    TextSymbol ts = markerSymbol as TextSymbol;
                    WriteAttribute("FontSize", ts.FontSize);
                    WriteAttribute("FontFamily", ts.FontFamily.Source);
                    WriteAttribute("Text", ts.Text);
                    if (ts.Foreground != null)
                    {
                        writer.WriteStartElement("TextSymbol.Foreground", Namespaces[Constants.esriPrefix]);
                        new BrushXamlWriter(writer, Namespaces).WriteBrush(ts.Foreground);
                        writer.WriteEndElement();
                    }
                    #endregion
                }
                else if (markerSymbol is ESRI.ArcGIS.Mapping.Core.Symbols.ImageFillSymbol)
                {
                    #region IFS: Source
                    ESRI.ArcGIS.Mapping.Core.Symbols.ImageFillSymbol fillSymbol = markerSymbol as ESRI.ArcGIS.Mapping.Core.Symbols.ImageFillSymbol;
                    if (!string.IsNullOrEmpty(fillSymbol.ImageData))
                        writer.WriteAttributeString("ImageData", fillSymbol.ImageData); 
                    else if (!string.IsNullOrEmpty(fillSymbol.Source))
                        writer.WriteAttributeString("Source", fillSymbol.Source);
                    #endregion
                }
                else if (markerSymbol is ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol)
                {
                    #region Color
                    ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol marker = markerSymbol as ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol;
                    if (marker != null)
                    {
                        if (marker.Color != null)
                        {
                            writer.WriteStartElement("MarkerSymbol.Color", Namespaces[Constants.esriMappingPrefix]);
                            new BrushXamlWriter(writer, Namespaces).WriteBrush(marker.Color);
                            writer.WriteEndElement();
                        }
                    }
                    #endregion
                }
                #region Feature Service Symbols - Not used
                //else if (markerSymbol is ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol)
                //{
                //    #region pms
                //    ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol pmsfs = markerSymbol as ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol;
                //    WriteAttribute("Width", pmsfs.Width);
                //    WriteAttribute("Height", pmsfs.Height);
                //    WriteAttribute("Opacity", pmsfs.Opacity);
                //    WriteFeatureServiceMarkerSymbolProperties("PictureMarkerSymbol", pmsfs.Angle, pmsfs.XOffsetFromCenter,
                //         pmsfs.YOffsetFromCenter, pmsfs.Color, pmsfs.SelectionColor, pmsfs.RenderTransformPoint);
                //    #region Set ContentType,Url and ImageData to set Source
                //    SetImageSourceRelatedProperties(pmsfs, pmsfs.ToJson());
                //    #endregion
                //    #endregion
                //}
                //else if (markerSymbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol)
                //{
                //    #region smsfs
                //    ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol smsfs = markerSymbol as ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol;
                //    WriteAttribute("OutlineThickness", smsfs.OutlineThickness);
                //    WriteAttribute("Size", smsfs.Size);
                //    WriteAttribute("Style", smsfs.Style.ToString());
                //    WriteAttribute("OutlineStyle", smsfs.OutlineStyle.ToString());
                //    WriteFeatureServiceMarkerSymbolProperties("SimpleMarkerSymbol", smsfs.Angle, smsfs.XOffsetFromCenter,
                //       smsfs.YOffsetFromCenter, smsfs.Color, smsfs.SelectionColor, smsfs.RenderTransformPoint);
                //    if (smsfs.OutlineColor != null)
                //    {
                //        writer.WriteStartElement("SimpleMarkerSymbol.OutlineColor", Namespaces[Constants.esriFSSymbolsPrefix]);
                //        new BrushXamlWriter(writer, Namespaces).WriteBrush(smsfs.OutlineColor);
                //        writer.WriteEndElement();
                //    }
                //    #endregion
                //}
                #endregion
                else
                    throw new NotSupportedException(markerSymbol.GetType().FullName);
            }
            writer.WriteEndElement();
        }

        //private void WriteFeatureServiceMarkerSymbolProperties(string symbolType, double angle, 
        //    double xOffsetFromCenter, double yOffsetFromCenter, Brush color, Brush selectionColor, Point renderTransformPoint)
        //{
        //    WriteAttribute("Angle", angle);
        //    WriteAttribute("XOffsetFromCenter", xOffsetFromCenter);
        //    WriteAttribute("YOffsetFromCenter", yOffsetFromCenter);
        //    if (color != null)
        //    {
        //        writer.WriteStartElement(symbolType + ".Color", Namespaces[Constants.esriFSSymbolsPrefix]);
        //        new BrushXamlWriter(writer, Namespaces).WriteBrush(color);
        //        writer.WriteEndElement();
        //    }
        //    if (selectionColor != null)
        //    {
        //        writer.WriteStartElement(symbolType + ".SelectionColor", Namespaces[Constants.esriFSSymbolsPrefix]);
        //        new BrushXamlWriter(writer, Namespaces).WriteBrush(selectionColor);
        //        writer.WriteEndElement();
        //    }
        //}
        private void WriteLineSymbol(LineSymbol lineSymbol)
        {
            if (lineSymbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleLineSymbol)
            {
                StartType(lineSymbol, Constants.esriFSSymbolsPrefix);
                WriteJsonAttribute(lineSymbol as IJsonSerializable);
            }
            else
            {
                StartType(lineSymbol, Constants.esriPrefix);
                if (lineSymbol.Width != 1.0)
                    WriteAttribute("Width", lineSymbol.Width);
                if (lineSymbol is SimpleLineSymbol)
                {
                    SimpleLineSymbol sls = lineSymbol as SimpleLineSymbol;
                    if (sls.Style != SimpleLineSymbol.LineStyle.Solid)
                        WriteAttribute("Style", (lineSymbol as SimpleLineSymbol).Style.ToString());
                }
                //else if (lineSymbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleLineSymbol)
                //{
                //    ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleLineSymbol slsfs = lineSymbol as ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleLineSymbol;
                //    WriteAttribute("Style", slsfs.Style.ToString());
                //    if (slsfs.SelectionColor != null)
                //    {
                //        writer.WriteStartElement("SimpleLineSymbol.SelectionColor", Namespaces[Constants.esriFSSymbolsPrefix]);
                //        new BrushXamlWriter(writer, Namespaces).WriteBrush(slsfs.SelectionColor);
                //        writer.WriteEndElement();
                //    }
                //}
                SolidColorBrush sb = lineSymbol.Color as SolidColorBrush;
                if (sb != null)
                {
                    WriteAttribute("Color", sb.Color);
                }
                else
                {
                    writer.WriteStartElement(lineSymbol.GetType().Name + ".Color", Namespaces[Constants.esriPrefix]);
                    new BrushXamlWriter(writer, Namespaces).WriteBrush(lineSymbol.Color);
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }

        private void WriteFillSymbol(FillSymbol fillSymbol)
        {
            if (fillSymbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleFillSymbol || fillSymbol is ESRI.ArcGIS.Client.FeatureService.Symbols.PictureFillSymbol)
            {
                StartType(fillSymbol, Constants.esriFSSymbolsPrefix);
                WriteJsonAttribute(fillSymbol as IJsonSerializable);
            }
            else
            {
                StartType(fillSymbol, Constants.esriPrefix);

                if (fillSymbol.BorderThickness != 1.0)
                    WriteAttribute("BorderThickness", fillSymbol.BorderThickness);

                SolidColorBrush sb = fillSymbol.BorderBrush as SolidColorBrush;
                if (sb != null)
                {
                    WriteAttribute("BorderBrush", sb.Color);
                }
                else
                {
                    writer.WriteStartElement(fillSymbol.GetType().Name + ".BorderBrush", Namespaces[Constants.esriPrefix]);
                    new BrushXamlWriter(writer, Namespaces).WriteBrush(fillSymbol.BorderBrush);
                    writer.WriteEndElement();
                }

                sb = fillSymbol.Fill as SolidColorBrush;
                if (sb != null)
                {
                    WriteAttribute("Fill", sb.Color);
                }
                else
                {
                    writer.WriteStartElement(fillSymbol.GetType().Name + ".Fill", Namespaces[Constants.esriPrefix]);
                    new BrushXamlWriter(writer, Namespaces).WriteBrush(fillSymbol.Fill);
                    writer.WriteEndElement();
                }

                //if (fillSymbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleFillSymbol)
                //{
                //    ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleFillSymbol sfs = fillSymbol as ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleFillSymbol;
                //    WriteAttribute("Style", sfs.Style.ToString());
                //    WriteAttribute("BorderStyle", sfs.BorderStyle.ToString());
                //    WriteAttribute("Color", sfs.Color);
                //    if (sfs.SelectionColor != null)
                //    {
                //        writer.WriteStartElement("SimpleFillSymbol.SelectionColor", Namespaces[Constants.esriFSSymbolsPrefix]);
                //        new BrushXamlWriter(writer, Namespaces).WriteBrush(sfs.SelectionColor);
                //        writer.WriteEndElement();
                //    }
                //}
                //else if (fillSymbol is ESRI.ArcGIS.Client.FeatureService.Symbols.PictureFillSymbol)
                //{
                //    ESRI.ArcGIS.Client.FeatureService.Symbols.PictureFillSymbol pfs = fillSymbol as ESRI.ArcGIS.Client.FeatureService.Symbols.PictureFillSymbol;
                //    WriteAttribute("Height", pfs.Height);
                //    WriteAttribute("Width", pfs.Width);
                //    WriteAttribute("Opacity", pfs.Opacity);
                //    WriteAttribute("BorderStyle", pfs.BorderStyle.ToString());
                //    if (pfs.Color != null)
                //    {
                //        writer.WriteStartElement("PictureFillSymbol.OutlineColor", Namespaces[Constants.esriFSSymbolsPrefix]);
                //        new BrushXamlWriter(writer, Namespaces).WriteBrush(pfs.Color);
                //        writer.WriteEndElement();
                //    }
                //    if (pfs.SelectionColor != null)
                //    {
                //        writer.WriteStartElement("PictureFillSymbol.SelectionColor", Namespaces[Constants.esriFSSymbolsPrefix]);
                //        new BrushXamlWriter(writer, Namespaces).WriteBrush(pfs.SelectionColor);
                //        writer.WriteEndElement();
                //    }
                //    #region Set ContentType,Url and ImageData to set Source
                //    string json = pfs.ToJson();
                //    SetImageSourceRelatedProperties(pfs, json);
                //    #endregion
                //}
            }
            writer.WriteEndElement();
        }

        //private void SetImageSourceRelatedProperties(Symbol symbol, string json)
        //{
        //    ESRI.ArcGIS.Client.Utils.JavaScriptSerializer jss = new ESRI.ArcGIS.Client.Utils.JavaScriptSerializer();
        //    Dictionary<string, object> dictionary = jss.DeserializeObject(json) as Dictionary<string, object>;
        //    if (dictionary.ContainsKey("contentType"))
        //    {
        //        string value = dictionary["contentType"] as string;
        //        if (!string.IsNullOrEmpty(value))
        //        {
        //            SymbolExtensions.SetImageContentType(symbol, value);
        //            writer.WriteElementString(Constants.esriMappingPrefix, "SymbolExtensions.ImageContentType",
        //                Constants.esriMappingNamespace, value);
        //        }
        //    }
        //    if (dictionary.ContainsKey("url"))
        //    {
        //        string value = dictionary["url"] as string;
        //        if (!string.IsNullOrEmpty(value))
        //        {
        //            SymbolExtensions.SetImageUrl(symbol, value);
        //            writer.WriteElementString(Constants.esriMappingPrefix, "SymbolExtensions.ImageUrl",
        //                Constants.esriMappingNamespace, value);
        //        }
        //    }
        //    if (dictionary.ContainsKey("imageData"))
        //    {
        //        string value = dictionary["imageData"] as string;
        //        if (!string.IsNullOrEmpty(value))
        //        {
        //            SymbolExtensions.SetImageData(symbol, value);
        //            writer.WriteElementString(Constants.esriMappingPrefix, "SymbolExtensions.ImageData",
        //                Constants.esriMappingNamespace, value);
        //        }
        //    }
        //}

        internal static bool IsSerializable(Symbol symbol)
        {
            if (symbol == null)
                return true;

            Type type = symbol.GetType();
            if (type != null && type.IsVisible)
                return true;

            return false;
        }

        #endregion
    }
}
