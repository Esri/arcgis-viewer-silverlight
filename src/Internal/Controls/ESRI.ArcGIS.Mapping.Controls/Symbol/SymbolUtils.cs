/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Globalization;
using System.Windows.Media;
using System.Xml.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public static class SymbolUtils
    {
        public static Symbol GetDefaultSymbol(this GraphicsLayer layer)
        {
            if (layer != null)
            {
                return GetDefaultSymbol(layer.Renderer);
            }
            return null;
        }
        public static Symbol GetDefaultSymbol(this IRenderer renderer)
        {
            if (renderer != null)
            {
                SimpleRenderer simpleRenderer = renderer as SimpleRenderer;
                if (simpleRenderer != null)
                {
                    return simpleRenderer.Symbol;
                }
                else
                {
                    ClassBreaksRenderer classBreaksRenderer = renderer as ClassBreaksRenderer;
                    if(classBreaksRenderer != null)
                        return classBreaksRenderer.DefaultSymbol;
                    else
                    {
                        UniqueValueRenderer uniqueValueRenderer = renderer as UniqueValueRenderer;
                        if(uniqueValueRenderer != null)
                            return uniqueValueRenderer.DefaultSymbol;
                    }
                }
            }
            return null;
        }
        public static Symbol GetDefaultSymbolClone(this GraphicsLayer layer)
        {
            if (layer != null)
            {
                GeometryType type = LayerExtensions.GetGeometryType(layer);
                return GetDefaultSymbolClone(layer.Renderer, type);
            }
            return null;
        }
        public static Symbol GetDefaultSymbolClone(this IRenderer renderer, GeometryType type)
        {
            if (renderer != null)
            {
                Symbol defaultSymbol = renderer.GetDefaultSymbol();
                if (defaultSymbol != null) //if a default symbol already exists, clone it
                    return CloneSymbol(defaultSymbol);
            }

            return CreateDefaultSymbol(type);
        }

        internal static void ChangeRenderer(this GraphicsLayer layer, Symbol symbol)
        {
            if (layer != null)
                layer.ChangeRenderer(new SimpleRenderer() { Symbol = symbol });
        }
        internal static void ChangeRenderer(this GraphicsLayer layer, IRenderer renderer)
        {
            if (layer != null && renderer != null)
            {
                layer.Renderer = null;
                layer.Renderer = renderer;
            }
        }

        public static IRenderer CreateDefaultRenderer(this GraphicsLayer graphicsLayer)
        {
            GeometryType geomType = LayerExtensions.GetGeometryType(graphicsLayer);
            return CreateDefaultRenderer(geomType);
        }
        private static IRenderer CreateDefaultRenderer(GeometryType geomType)
        {
            SimpleRenderer renderer = new SimpleRenderer();
            renderer.Symbol = CreateDefaultSymbol(geomType);
            return renderer;
        }
        private static Symbol CreateDefaultSymbol(GeometryType geomType)
        {
            Symbol symbol = null;
            switch (geomType)
            {
                case GeometryType.Point:
                    {
                        symbol = new SimpleMarkerSymbol() { Color = new SolidColorBrush(Colors.Red) };
                    } break;
                case GeometryType.Polygon:
                    {
                        symbol = new FillSymbol() { BorderBrush = new SolidColorBrush(Colors.Black), Fill = new SolidColorBrush(Colors.Red) };
                    } break;
                case GeometryType.Polyline:
                    {
                        symbol = new LineSymbol() { Color = new SolidColorBrush(Colors.Red) };
                    } break;
            }
            return symbol;
        }

        internal static Symbol CloneSymbol(this Symbol symbol)
        {
            return ESRI.ArcGIS.Mapping.Core.SymbolUtils.CloneSymbol(symbol);
        }
 
    }
}
