/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Core;
using System.Windows.Media;

namespace ESRI.ArcGIS.Mapping.Core.Symbols
{
    public class SymbologyInfo
    {
        private Dictionary<GeometryType, SymbolDescription> _defaultSymbols;
        public Dictionary<GeometryType, SymbolDescription> DefaultSymbols
        {
            get
            {
                if (_defaultSymbols == null)
                    _defaultSymbols = new Dictionary<GeometryType, SymbolDescription>();
                return _defaultSymbols;
            }
        }

        private Dictionary<ColorRampType, LinearGradientBrush> _defaultGradientBrushes;
        public Dictionary<ColorRampType, LinearGradientBrush> DefaultGradientBrushes
        {
            get
            {
                if (_defaultGradientBrushes == null)
                    _defaultGradientBrushes = new Dictionary<ColorRampType, LinearGradientBrush>();
                return _defaultGradientBrushes;
            }
        }

        public void AddDefaultSymbolForGeometryType(GeometryType geoType, SymbolDescription symbol)
        {
            if (!DefaultSymbols.ContainsKey(geoType))
                DefaultSymbols.Add(geoType, symbol);
        }
        public void AddDefaultGradientBrush(ColorRampType colorRamp, LinearGradientBrush brush)
        {
            if (!DefaultGradientBrushes.ContainsKey(colorRamp))
                DefaultGradientBrushes.Add(colorRamp, brush);
        }
    }
}
