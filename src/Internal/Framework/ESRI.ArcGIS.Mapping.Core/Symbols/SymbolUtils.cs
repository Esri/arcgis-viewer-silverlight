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
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Core
{
    public static class SymbolUtils
    {
        public static Symbol CloneSymbol(this Symbol symbol)
        {
            ICustomSymbol customSymbol = symbol as ICustomSymbol;
            if (customSymbol != null)
                return customSymbol.Clone();
            if (symbol is IJsonSerializable && symbol.GetType().Namespace.StartsWith("ESRI.ArcGIS.Client.FeatureService.Symbols", StringComparison.Ordinal))
            {
                string json = GetJsonForSymbol(symbol as IJsonSerializable);
                if (!string.IsNullOrEmpty(json))
                {
                    if (symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol)
                        json = json.Replace(",\"outline\":null", string.Empty);//WORKAROUND for core bug
                    return Symbol.FromJson(json);
                }
            }
            FillSymbol fillSymbol = symbol as FillSymbol;
            if (fillSymbol != null)
            {
                return cloneFillSymbol(fillSymbol);
            }
            else
            {
                LineSymbol lineSymbol = symbol as LineSymbol;
                if (lineSymbol != null)
                {
                    return cloneLineSymbol(lineSymbol);
                }
                else
                {
                    ESRI.ArcGIS.Client.Symbols.MarkerSymbol markerSymbol = symbol as ESRI.ArcGIS.Client.Symbols.MarkerSymbol;
                    if (markerSymbol != null)
                    {
                        return cloneMarkerSymbol(markerSymbol);
                    }
                }
            }
            return symbol;
        }

        private static LineSymbol cloneLineSymbol(LineSymbol lineSymbol)
        {
            if (lineSymbol == null)
                return null;

            ESRI.ArcGIS.Mapping.Core.Symbols.SimpleLineSymbol mappingSimpleLineSymbol = lineSymbol as ESRI.ArcGIS.Mapping.Core.Symbols.SimpleLineSymbol;
            if (mappingSimpleLineSymbol != null)
            {
                ESRI.ArcGIS.Mapping.Core.Symbols.SimpleLineSymbol ls = new ESRI.ArcGIS.Mapping.Core.Symbols.SimpleLineSymbol()
                {
                    Color = CloneBrush(mappingSimpleLineSymbol.Color),
                    ControlTemplate = mappingSimpleLineSymbol.ControlTemplate,
                    SelectionColor = CloneBrush(mappingSimpleLineSymbol.SelectionColor),
                    Width = mappingSimpleLineSymbol.Width,
                };
                return ls;
            }

            SimpleLineSymbol simpleLineSymbol = lineSymbol as SimpleLineSymbol;
            if (simpleLineSymbol != null)
            {
                SimpleLineSymbol ls = new SimpleLineSymbol()
                {
                    Color = CloneBrush(simpleLineSymbol.Color),
                    ControlTemplate = simpleLineSymbol.ControlTemplate,
                    Style = simpleLineSymbol.Style,
                    Width = simpleLineSymbol.Width,
                };
                return ls;
            }

            CartographicLineSymbol cLineSymbol = lineSymbol as CartographicLineSymbol;
            if (cLineSymbol != null)
            {
                CartographicLineSymbol cs = new CartographicLineSymbol()
                {
                    Color = CloneBrush(cLineSymbol.Color),
                    ControlTemplate = cLineSymbol.ControlTemplate,
                    DashArray = cLineSymbol.DashArray,
                    DashCap = cLineSymbol.DashCap,
                    DashOffset = cLineSymbol.DashOffset,
                    EndLineCap = cLineSymbol.EndLineCap,
                    LineJoin = cLineSymbol.LineJoin,
                    MiterLimit = cLineSymbol.MiterLimit,
                    StartLineCap = cLineSymbol.StartLineCap,
                    Width = cLineSymbol.Width,
                };
                return cs;
            }

            LineSymbol l = new LineSymbol()
            {
                Color = CloneBrush(lineSymbol.Color),
                ControlTemplate = lineSymbol.ControlTemplate,
                Width = lineSymbol.Width,
            };
            return l;
        }

        private static FillSymbol cloneFillSymbol(FillSymbol fillSymbol)
        {
            if (fillSymbol == null)
                return null;

            ESRI.ArcGIS.Mapping.Core.Symbols.SimpleFillSymbol mappingFillSymbol = fillSymbol as ESRI.ArcGIS.Mapping.Core.Symbols.SimpleFillSymbol;
            if (mappingFillSymbol != null)
            {
                ESRI.ArcGIS.Mapping.Core.Symbols.SimpleFillSymbol sfs = new ESRI.ArcGIS.Mapping.Core.Symbols.SimpleFillSymbol()
                {
                    BorderBrush = CloneBrush(mappingFillSymbol.BorderBrush),
                    BorderThickness = mappingFillSymbol.BorderThickness,
                    ControlTemplate = mappingFillSymbol.ControlTemplate,
                    Fill = CloneBrush(mappingFillSymbol.Fill),
                    SelectionColor = CloneBrush(mappingFillSymbol.SelectionColor),
                };
                return sfs;
            }

            SimpleFillSymbol simpleFillSymbol = fillSymbol as SimpleFillSymbol;
            if (simpleFillSymbol != null)
            {
                SimpleFillSymbol sfs = new SimpleFillSymbol()
                {
                    BorderBrush = CloneBrush(simpleFillSymbol.BorderBrush),
                    BorderThickness = simpleFillSymbol.BorderThickness,
                    ControlTemplate = simpleFillSymbol.ControlTemplate,
                    Fill = CloneBrush(simpleFillSymbol.Fill),
                };
                return sfs;
            }

            PictureFillSymbol pictureFillSymbol = fillSymbol as PictureFillSymbol;
            if (pictureFillSymbol != null)
            {
                PictureFillSymbol pfs = new PictureFillSymbol()
                {
                    BorderBrush = CloneBrush(pictureFillSymbol.BorderBrush),
                    BorderThickness = pictureFillSymbol.BorderThickness,
                    ControlTemplate = pictureFillSymbol.ControlTemplate,
                    Fill = CloneBrush(pictureFillSymbol.Fill),
                    Source = pictureFillSymbol.Source,
                };
                return pfs;
            }

            FillSymbol fs = new FillSymbol()
            {
                BorderBrush = CloneBrush(fillSymbol.BorderBrush),
                BorderThickness = fillSymbol.BorderThickness,
                ControlTemplate = fillSymbol.ControlTemplate,
                Fill = CloneBrush(fillSymbol.Fill),
            };
            return fs;
        }

        public static Brush CloneBrush(this Brush brush)
        {
            SolidColorBrush sb = brush as SolidColorBrush;
            if (sb != null)
            {
                return new SolidColorBrush()
                {
                    Color = sb.Color,
                    Opacity = sb.Opacity,
                    Transform = sb.Transform,
                    RelativeTransform = sb.RelativeTransform,
                };
            }
            else
            {
                LinearGradientBrush gb = brush as LinearGradientBrush;
                if (gb != null)
                {
                    return new LinearGradientBrush()
                    {
                        ColorInterpolationMode = gb.ColorInterpolationMode,
                        EndPoint = gb.EndPoint,
                        GradientStops = CloneGradientStops(gb.GradientStops),
                        MappingMode = gb.MappingMode,
                        Opacity = gb.Opacity,
                        RelativeTransform = gb.RelativeTransform,
                        SpreadMethod = gb.SpreadMethod,
                        StartPoint = gb.StartPoint,
                        Transform = gb.Transform,
                    };
                }
                else
                {
                    ImageBrush imageBrush = brush as ImageBrush;
                    if (imageBrush != null)
                    {
                        return new ImageBrush()
                        {
                            AlignmentX = imageBrush.AlignmentX,
                            AlignmentY = imageBrush.AlignmentY,
                            ImageSource = imageBrush.ImageSource,
                            Opacity = imageBrush.Opacity,
                            RelativeTransform = imageBrush.RelativeTransform,
                            Stretch = imageBrush.Stretch,
                            Transform = imageBrush.Transform,
                        };
                    }
                }
            }
            return brush;
        }

        private static GradientStopCollection CloneGradientStops(GradientStopCollection gradientStopCollection)
        {
            if (gradientStopCollection == null)
                return null;

            GradientStopCollection gc = new GradientStopCollection();
            foreach (GradientStop g in gradientStopCollection)
            {
                gc.Add(new GradientStop()
                {
                    Color = g.Color,
                    Offset = g.Offset,
                });
            }
            return gc;
        }

        private static MarkerSymbol cloneMarkerSymbol(MarkerSymbol symbol)
        {
            if (symbol == null)
                return null;

            ESRI.ArcGIS.Mapping.Core.Symbols.ImageFillSymbol imageFillSymbol = symbol as ESRI.ArcGIS.Mapping.Core.Symbols.ImageFillSymbol;
            if (imageFillSymbol != null)
            {
                ESRI.ArcGIS.Mapping.Core.Symbols.ImageFillSymbol ifs = new ESRI.ArcGIS.Mapping.Core.Symbols.ImageFillSymbol()
                {
                    Color = CloneBrush(imageFillSymbol.Color),
                    ControlTemplate = imageFillSymbol.ControlTemplate,
                    OffsetX = imageFillSymbol.OffsetX,
                    OffsetY = imageFillSymbol.OffsetY,
                    OriginX = imageFillSymbol.OriginX,
                    OriginY = imageFillSymbol.OriginY,
                    Opacity = imageFillSymbol.Opacity,
                    Size = imageFillSymbol.Size,
                    ImageData = imageFillSymbol.ImageData,
                    Source = imageFillSymbol.Source,
                    Fill = CloneBrush(imageFillSymbol.Fill),
                    CursorName = imageFillSymbol.CursorName,
                };
                return ifs;
            }

            ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol ms = symbol as ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol;
            if (ms != null)
            {
                ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol marSymb = new ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol()
                {
                    DisplayName = ms.DisplayName,
                    Offset = new Point(ms.Offset.X, ms.Offset.Y),
                    Color = CloneBrush(ms.Color),
                    ControlTemplate = ms.ControlTemplate,
                    OffsetX = ms.OffsetX,
                    OffsetY = ms.OffsetY,
                    OriginX = ms.OriginX,
                    OriginY = ms.OriginY,
                    Opacity = ms.Opacity,
                    Size = ms.Size,
                };
                return marSymb;
            }

            ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol simpleMarkerSymbol = symbol as ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol;
            if (simpleMarkerSymbol != null)
            {
                ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol sms = new ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol()
                {
                    Color = CloneBrush(simpleMarkerSymbol.Color),
                    ControlTemplate = simpleMarkerSymbol.ControlTemplate,
                    Size = simpleMarkerSymbol.Size,
                    // Simple marker symbol doesn't allow setting OffsetX, OffsetY
                };
                return sms;
            }

            TextSymbol textSymbol = symbol as TextSymbol;
            if (textSymbol != null)
            {
                TextSymbol ts = new TextSymbol()
                {
                    ControlTemplate = textSymbol.ControlTemplate,
                    FontFamily = textSymbol.FontFamily,
                    FontSize = textSymbol.FontSize,
                    Foreground = CloneBrush(textSymbol.Foreground),
                    OffsetX = textSymbol.OffsetX,
                    OffsetY = textSymbol.OffsetY,
                    Text = textSymbol.Text,
                };
                return ts;
            }

            PictureMarkerSymbol pictureMarkerSymbol = symbol as PictureMarkerSymbol;
            if (pictureMarkerSymbol != null)
            {
                PictureMarkerSymbol pictMs = new PictureMarkerSymbol()
                {
                    ControlTemplate = pictureMarkerSymbol.ControlTemplate,
                    Height = pictureMarkerSymbol.Height,
                    OffsetX = pictureMarkerSymbol.OffsetX,
                    OffsetY = pictureMarkerSymbol.OffsetY,
                    Opacity = pictureMarkerSymbol.Opacity,
                    Source = pictureMarkerSymbol.Source,
                    Width = pictureMarkerSymbol.Width,
                };
                return pictMs;
            }

            MarkerSymbol markerS = new MarkerSymbol()
            {
                ControlTemplate = symbol.ControlTemplate,
                OffsetX = symbol.OffsetX,
                OffsetY = symbol.OffsetY,
            };
            return markerS;
        }

        public static string GetJsonForSymbol(IJsonSerializable serializable)
        {
            if (serializable == null)
                return null;
            string json = serializable.ToJson();
            return json;
        }

        public static void SetOpacity(this Brush brush, double opacity)
        {
            SolidColorBrush solidColorBrush = brush as SolidColorBrush;
            if (solidColorBrush != null)
                solidColorBrush.Color = Color.FromArgb(Convert.ToByte(255 * opacity), solidColorBrush.Color.R, solidColorBrush.Color.G, solidColorBrush.Color.B);
            else
                brush.Opacity = opacity;
        }

        public static double GetOpacity(this Brush brush)
        {
            SolidColorBrush solidColorBrush = brush as SolidColorBrush;
            if (solidColorBrush != null)
                return solidColorBrush.Color.A / 255d;
            else
                return brush.Opacity;
        }
    }
}
