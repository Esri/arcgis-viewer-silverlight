/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping;
using ESRI.ArcGIS.Mapping.Core;
using System.Windows.Markup;
using System.Windows.Browser;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Json;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class SymbolJsonHelper
    {
        /// <summary>
        /// Creates a symbol compatible with Application Framework from JSON object
        /// </summary>
        /// <param name="jsonObject">JSON object defining a symbol</param>
        /// <returns>Symbol</returns>
        public static Symbol SymbolFromJson(JsonObject jsonObject)
        {
            Symbol symb = null;
            if (jsonObject != null)
            {
                string symbType = jsonObject["type"];
                if (!string.IsNullOrEmpty(symbType))
                {
                    switch (symbType)
                    {
                        case "esriPMS":// REST defined PictureMarkerSymbol --> output: ImageFillSymbol 
                            #region
                            ESRI.ArcGIS.Mapping.Core.Symbols.ImageFillSymbol imgsymb = new ESRI.ArcGIS.Mapping.Core.Symbols.ImageFillSymbol();
                            double size = 64; // standard size of images used for Viewer symbols
                            if (jsonObject.ContainsKey("width"))
                                size = jsonObject["width"];
                            imgsymb.Size = size;
                            if (jsonObject.ContainsKey("xoffset"))
                                imgsymb.OriginX = (size /2 + jsonObject["xoffset"]) / size;
                            if (jsonObject.ContainsKey("yoffset"))
                                imgsymb.OriginY = (size / 2 + jsonObject["yoffset"]) / size;
                            if (jsonObject.ContainsKey("imageData"))
                                imgsymb.ImageData = jsonObject["imageData"];
                            else if (jsonObject.ContainsKey("url"))
                                imgsymb.Source = jsonObject["url"];

                            symb = imgsymb;
                            break;
                            #endregion
                        default:
                            // all other REST defined cases
                            symb = Symbol.FromJson(jsonObject.ToString());
                            break;
                    }
                }
            }
            return symb;
        }


        /// <summary>
        /// Creates a symbol compatible with Application Framework from JSON string
        /// </summary>
        /// <param name="json">String containing JSON object defining a symbol</param>
        /// <returns>Symbol</returns>
        public static Symbol SymbolFromJson(string json)
        {
            Symbol symb = null;
            JsonObject jobj = JsonObject.Parse(json) as JsonObject;
            if (jobj != null)
                symb = SymbolFromJson(jobj);
            return symb;
        }

        /// <summary>
        /// Converts symbol to string containing JSON object defining a symbol
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <returns>String</returns>
        public static String SymbolToJsonString(Symbol symbol)
        {
            if (symbol is IJsonSerializable)
                return (symbol as IJsonSerializable).ToJson();
            string jstr = string.Empty;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            #region Mapping.Core Symbols
            // ImageFillSymbol to esriPMS type
            #region
            ESRI.ArcGIS.Mapping.Core.Symbols.ImageFillSymbol imgsymb = symbol as ESRI.ArcGIS.Mapping.Core.Symbols.ImageFillSymbol;
            if (imgsymb != null)
            {
                double size = imgsymb.Size;
                double xoffset = (size * imgsymb.OriginX) - size / 2;
                double yoffset = (size * imgsymb.OriginY) - size / 2;
                sb.AppendFormat("{0} \"type\" : \"esriPMS\", \"url\" : \"{2}\", ", "{", "}", imgsymb.Source);
                sb.AppendFormat("\"width\" : {0}, \"height\" : {0}, \"xoffset\" : {1}, \"yoffset\" : {2}", size, xoffset, yoffset);
                //sb.Append(", \"contentType\" : \"image/png\", \"angle\" : 0, \"color\" : null");
                sb.Append(" }");
                return sb.ToString();
            }
            #endregion
            // SimpleLineSymbol to esriSLS type
            #region
            ESRI.ArcGIS.Mapping.Core.Symbols.SimpleLineSymbol lsymb = symbol as ESRI.ArcGIS.Mapping.Core.Symbols.SimpleLineSymbol;
            if (lsymb != null)
            {
                SolidColorBrush lbrush = lsymb.Color as SolidColorBrush;
                if (lbrush != null)
                {
                    Color lcolor = lbrush.Color;
                    string lcolorstr = string.Format("[{0},{1},{2},{3}]", lcolor.R, lcolor.G, lcolor.B, lcolor.A);
                    sb.AppendFormat("{0} \"type\" : \"esriSLS\", \"style\" : \"esriSLSSolid\",", "{", "}");
                    sb.AppendFormat(" \"color\" : {0}, \"width\": {1}", lcolorstr, lsymb.Width);
                    sb.Append(" }");
                    return sb.ToString();
                }
            }
            #endregion
            // CartographicLineSymbol to esriSLS type
            #region
            ESRI.ArcGIS.Mapping.Core.Symbols.CartographicLineSymbol clsymb = symbol as ESRI.ArcGIS.Mapping.Core.Symbols.CartographicLineSymbol;
            if (clsymb != null)
            {
                SolidColorBrush clbrush = clsymb.Color as SolidColorBrush;
                if (clbrush != null)
                {
                    Color clcolor = clbrush.Color;
                    string clcolorstr = string.Format("[{0},{1},{2},{3}]", clcolor.R, clcolor.G, clcolor.B, clcolor.A);
                    if (clsymb.DashArray.Count > 0)
                    {
                        System.Text.StringBuilder sb2 = new System.Text.StringBuilder();
                        string cltype = "esriSLSDot";
                        foreach (double dashdouble in clsymb.DashArray)
                        {
                            if (sb2.Length > 0)
                                sb2.Append(" ");
                            sb2.AppendFormat("{0}", dashdouble);
                        }
                        switch (sb2.ToString())
                        {
                            case "1 1": // Dot

                                break;
                            case "2 1": // Dash
                                cltype = "esriSLSDash";
                                break;
                            case "2 1 1 1": // DashDot
                                cltype = "esriSLSDashDot";
                                break;
                            case "2 1 1 1 1 1": // DashDotDot
                                cltype = "esriSLSDashDotDot";
                                break;
                        }

                        sb.AppendFormat("{0} \"type\" : \"esriSLS\", \"style\" : \"{2}\",", "{", "}", cltype);
                        sb.AppendFormat(" \"color\" : {0}, \"width\": {1}", clcolorstr, clsymb.Width);
                        sb.Append(" }");
                    }
                }
                return sb.ToString();
            }
            #endregion
            // SimpleFillSymbol to esriSFS type
            #region
            ESRI.ArcGIS.Mapping.Core.Symbols.SimpleFillSymbol fsymb = symbol as ESRI.ArcGIS.Mapping.Core.Symbols.SimpleFillSymbol;
            if (fsymb != null)
            {
                SolidColorBrush brush = fsymb.Fill as SolidColorBrush;
                if (brush != null)
                {
                    Color color = brush.Color;
                    string colorstr = string.Format("[{0},{1},{2},{3}]", color.R, color.G, color.B, color.A);
                    sb.AppendFormat("{0} \"type\" : \"esriSFS\", \"style\" : \"esriSFSSolid\",", "{");
                    sb.AppendFormat(" \"color\" : {0}", colorstr);
                    if (fsymb.BorderBrush != null)
                    {
                        SolidColorBrush olbrush = fsymb.BorderBrush as SolidColorBrush;
                        Color olcolor = olbrush.Color;
                        string olcolorstr = string.Format("[{0},{1},{2},{3}]", olcolor.R, olcolor.G, olcolor.B, olcolor.A);
                        sb.AppendFormat(", \"outline\" : {0} \"type\" : \"esriSLS\", \"style\" : \"esriSLSSolid\", \"color\" : {2}, \"width\" : {3} {1}", "{", "}", olcolorstr, FromPixelsToPoints(fsymb.BorderThickness));
                    }
                    sb.Append(" }");
                }
                return sb.ToString();
            }
            #endregion
            #endregion
            return jstr;
        }

        /// <summary>
        /// Converts pixel units to points.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <returns></returns>
        public static double FromPixelsToPoints(double pixels)
        {
            return (pixels / 96) * 72;
        }

        public static ESRI.ArcGIS.Mapping.Core.Symbols.ImageFillSymbol ToImageFillSymbol(
            ESRI.ArcGIS.Client.FeatureService.Symbols.PictureMarkerSymbol pms)
        {
            if (pms != null)
            {
                Symbols.ImageFillSymbol symbol = SymbolFromJson(pms.ToJson()) as Symbols.ImageFillSymbol;

                if (string.IsNullOrEmpty(symbol.ImageData) && string.IsNullOrEmpty(symbol.Source))
                {
                    string imageData = SymbolExtensions.GetImageData(pms);
                    string source;
                    if (string.IsNullOrEmpty(symbol.ImageData) && !string.IsNullOrEmpty(imageData))
                        symbol.ImageData = imageData;
                    else if (string.IsNullOrEmpty(symbol.Source) && !string.IsNullOrEmpty(source=SymbolExtensions.GetImageUrl(pms)))
                        symbol.Source = source;
                }

                return symbol;
            }
            return null;
        }
    }

}
