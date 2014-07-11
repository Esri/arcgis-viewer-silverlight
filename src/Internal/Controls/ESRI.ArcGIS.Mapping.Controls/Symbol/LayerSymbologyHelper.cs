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
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.Symbols;
using System.Windows.Markup;
using FSS = ESRI.ArcGIS.Client.FeatureService.Symbols;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public static class LayerSymbologyHelper
    {
        /// <summary>
        /// Workaround for core bug
        /// </summary>
        /// <param name="layer"></param>
        public static void ReplaceDefaultFsSimpleMarkerSymbolWithClone(this GraphicsLayer layer)
        {
            SimpleRenderer simpleRenderer;
            if ((simpleRenderer = layer.Renderer as SimpleRenderer) != null)
            {
                if (simpleRenderer.Symbol is FSS.SimpleMarkerSymbol)
                    simpleRenderer.Symbol = simpleRenderer.Symbol.CloneSymbol();
                return;
            }

            ClassBreaksRenderer classBreakRenderer;
            if ((classBreakRenderer = layer.Renderer as ClassBreaksRenderer) != null)
            {
                if (classBreakRenderer.DefaultSymbol is FSS.SimpleMarkerSymbol)
                    classBreakRenderer.DefaultSymbol = classBreakRenderer.DefaultSymbol.CloneSymbol();
                return;
            }

            UniqueValueRenderer uniqueValueRenderer;
            if ((uniqueValueRenderer = layer.Renderer as UniqueValueRenderer) != null)
            {
                if (uniqueValueRenderer.DefaultSymbol is FSS.SimpleMarkerSymbol)
                    uniqueValueRenderer.DefaultSymbol = uniqueValueRenderer.DefaultSymbol.CloneSymbol();
                return;
            }
        }

        public static void IncreaseDefaultSymbolSize(this GraphicsLayer layer)
        {
            IncreaseDecreaseLayerSymbolSizeBy(layer, 1);
        }

        public static void IncreaseDecreaseLayerSymbolSizeBy(this GraphicsLayer layer, int increaseDecreaseBy)
        {
            Symbol symbol = layer.GetDefaultSymbol();
            IncreaseDecreaseSymbolSizeBy(symbol, increaseDecreaseBy);
        }

        public static void SetSymbolSizeTo(this Symbol symbol, int symbolSize)
        {
            ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol markerSymbol = symbol as ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol;
            if (markerSymbol != null)
            {
                markerSymbol.Size = symbolSize;
            }
            else
            {
                ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol ms = symbol as ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol;
                if (ms != null)
                {
                    ms.Size = symbolSize;
                }
                else
                {
                    FSS.SimpleMarkerSymbol sms = symbol as FSS.SimpleMarkerSymbol;
                    if (sms != null)
                    {
                        sms.Size = symbolSize;
                    }
                }
            }
        }

        public static void IncreaseDecreaseSymbolSizeBy(this Symbol symbol, int increaseDecreaseBy)
        {
            ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol markerSymbol = symbol as ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol;
            if (markerSymbol != null)
            {
                double newSize = markerSymbol.Size + increaseDecreaseBy;
                if (newSize >= 0)
                {
                    markerSymbol.Size = newSize;
                }
            }
            else
            {
                ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol ms = symbol as ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol;
                if (ms != null)
                {
                    double newSize = ms.Size + increaseDecreaseBy;
                    if (newSize >= 0)
                    {
                        ms.Size = newSize;
                    }
                }
                else
                {
                    FSS.SimpleMarkerSymbol sms = symbol as FSS.SimpleMarkerSymbol;
                    if (sms != null)
                    {
                      double newSize = sms.Size + increaseDecreaseBy;
                      if (newSize >= 0)
                      {
                          sms.Size = newSize;
                      }
                    }
                }
            }
        }

        public static void DecreaseDefaultSymbolSize(this GraphicsLayer layer)
        {
            IncreaseDecreaseLayerSymbolSizeBy(layer, -1);
        }

        public static void IncreaseDefaultSymbolBorderWidth(this GraphicsLayer layer)
        {
            Symbol symbol = layer.GetDefaultSymbol();

            FSS.SimpleMarkerSymbol fsSimpleMarkerSymbol;
            if ((fsSimpleMarkerSymbol = symbol as FSS.SimpleMarkerSymbol) != null)
                fsSimpleMarkerSymbol.OutlineThickness++;
            else
            {
                FillSymbol fillSymbol = symbol as FillSymbol;
                if (fillSymbol != null)
                {
                    double newSize = fillSymbol.BorderThickness + 1;
                    fillSymbol.BorderThickness = newSize;
                }
                else
                {
                    LineSymbol lineSymbol = symbol as LineSymbol;
                    if (lineSymbol != null)
                    {
                        double newSize = lineSymbol.Width + 1;
                        lineSymbol.Width = newSize;
                    }
                }
            }
        }

        public static void DecreaseDefaultSymbolBorderWidth(this GraphicsLayer layer)
        {
            Symbol symbol = layer.GetDefaultSymbol();

            FSS.SimpleMarkerSymbol fsSimpleMarkerSymbol;
            if ((fsSimpleMarkerSymbol = symbol as FSS.SimpleMarkerSymbol) != null && fsSimpleMarkerSymbol.OutlineThickness > 0)
                fsSimpleMarkerSymbol.OutlineThickness--;    
            else
            {
                FillSymbol fillSymbol = symbol as FillSymbol;
                if (fillSymbol != null)
                {
                    if (fillSymbol.BorderThickness > 0) // make sure it isn't -ve
                    {
                        double newSize = fillSymbol.BorderThickness - 1;
                        fillSymbol.BorderThickness = newSize;
                    }
                }
                else
                {
                    LineSymbol lineSymbol = symbol as LineSymbol;
                    if (lineSymbol != null)
                    {
                        if (lineSymbol.Width > 0)// make sure it isn't -ve
                        {
                            double newSize = lineSymbol.Width - 1;
                            lineSymbol.Width = newSize;
                        }
                    }
                }
            }
        }

        public static Color GetDefaultSymbolBorderColor(this GraphicsLayer layer)
        {
            Symbol symbol = layer.GetDefaultSymbol();

            SolidColorBrush solidColorBrush;

            FSS.SimpleMarkerSymbol fsSimpleMarkerSymbol;
            if ((fsSimpleMarkerSymbol = symbol as FSS.SimpleMarkerSymbol) != null)
            {
                if ((solidColorBrush = fsSimpleMarkerSymbol.OutlineColor as SolidColorBrush) != null)
                    return solidColorBrush.Color;
            }
            else
            {
                FillSymbol fillSymbol = symbol as FillSymbol;
                if (fillSymbol != null)
                {
                    solidColorBrush = fillSymbol.BorderBrush as SolidColorBrush;
                    if (solidColorBrush != null)
                        return solidColorBrush.Color;
                }
                else
                {
                    LineSymbol lineSymbol = symbol as LineSymbol;
                    if (lineSymbol != null)
                    {
                        solidColorBrush = lineSymbol.Color as SolidColorBrush;
                        if (solidColorBrush != null)
                            return solidColorBrush.Color;
                    }
                }
            }
            return Colors.Transparent;
        }

        public static void ChangeDefaultSymbolBorderColor(this GraphicsLayer layer, Color newColor)
        {
            ChangeDefaultSymbolBorderColor(layer, new SolidColorBrush(newColor));
        }

        public static void ChangeDefaultSymbolBorderColor(this GraphicsLayer layer, Brush borderBrush)
        {
            Symbol symbol = layer.GetDefaultSymbol();

            FSS.SimpleMarkerSymbol fsSimpleMarkerSymbol;
            if ((fsSimpleMarkerSymbol = symbol as FSS.SimpleMarkerSymbol) != null)
                fsSimpleMarkerSymbol.OutlineColor = borderBrush;
            else
            {
                FillSymbol fillSymbol = symbol as FillSymbol;
                if (fillSymbol != null)
                {
                    fillSymbol.BorderBrush = borderBrush;
                }
                else
                {
                    LineSymbol lineSymbol = symbol as LineSymbol;
                    if (lineSymbol != null)
                    {
                        lineSymbol.Color = borderBrush;
                    }
                }
            }
        }

        public static Color GetDefaultSymbolFillColor(this GraphicsLayer layer)
        {
            Symbol symbol = layer.GetDefaultSymbol();

            SolidColorBrush solidColorBrush;

            FSS.SimpleMarkerSymbol fsSimpleMarkerSymbol;
            if ((fsSimpleMarkerSymbol = symbol as FSS.SimpleMarkerSymbol) != null)
            {
                if ((solidColorBrush = fsSimpleMarkerSymbol.Color as SolidColorBrush) != null)
                    return solidColorBrush.Color;
            }
            else
            {
                FillSymbol fillSymbol = symbol as FillSymbol;
                if (fillSymbol != null)
                {
                    solidColorBrush = fillSymbol.Fill as SolidColorBrush;
                    if (solidColorBrush != null)
                        return solidColorBrush.Color;
                }
            }
            return Colors.Transparent;
        }

        public static void ChangeDefaultSymbolFillColor(this GraphicsLayer layer, Color newColor)
        {
            ChangeDefaultSymbolFillColor(layer, new SolidColorBrush(newColor));
        }

        public static void ChangeDefaultSymbolFillColor(this GraphicsLayer layer, Brush fillBrush)
        {
            Symbol symbol = layer.GetDefaultSymbol();
            #region FSS.SFS
            if (symbol is FSS.SimpleFillSymbol && fillBrush is SolidColorBrush)
            {
                ((FSS.SimpleFillSymbol)symbol).Color = (fillBrush as SolidColorBrush).Color;
            }
            #endregion
            #region FSS.PFS
            else if (symbol is FSS.PictureFillSymbol)
            {
                FSS.PictureFillSymbol pfs = symbol as FSS.PictureFillSymbol;
                pfs.Color = fillBrush;
            }
            #endregion
            #region Default Fill Symbol
            else if (symbol is FillSymbol)
            {
                (symbol as FillSymbol).Fill = fillBrush;
            }
            #endregion
            #region Marker Symbols
            else
            {
                #region Mapping Core Marker
                ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol markerSymbol = symbol as ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol;
                if (markerSymbol != null)
                {
                    markerSymbol.Color = fillBrush;
                }
                #endregion
                else
                {
                    #region Client SMS
                    ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol simpleMarkerSymbol = symbol as ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol;
                    if (simpleMarkerSymbol != null)
                    {
                        simpleMarkerSymbol.Color = fillBrush;
                    }
                    #endregion
                    #region FSS.SMS
                    else
                    {
                        FSS.SimpleMarkerSymbol sms = symbol as FSS.SimpleMarkerSymbol;
                        if (sms != null)
                        {
                            sms.Color = fillBrush;
                        }
                    }
                    #endregion
                }
            }
            #endregion

        }

        public static void ChangeSymbolOnDefaultRenderer(this GraphicsLayer layer, string symbolJson)
        {
            if (layer == null)
                return;

            if (!string.IsNullOrEmpty(symbolJson))
            {
                Symbol symbol = null;
                try
                {
                    symbol = SymbolJsonHelper.SymbolFromJson(symbolJson);
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogError(ex);
                }
                if (symbol != null)
                    layer.ChangeRenderer(symbol);
            }
        }
    }
}
