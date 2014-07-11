/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows.Data;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Application.Layout.EditorSupport;
using ESRI.ArcGIS.Client.FeatureService;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Toolkit;
using FSSymbols = ESRI.ArcGIS.Client.FeatureService.Symbols;
using System.ComponentModel;

namespace ESRI.ArcGIS.Client.Application.Layout.Converters
{
    /// <summary>
    /// Converts a <see cref="ESRI.ArcGIS.Client.Toolkit.SymbolTemplate"/> to a editing template symbol.
    /// Swatch accounts for the template's symbology and drawing mode.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EditorSwatchGenerator : IValueConverter
    {
        /// <summary>
        /// Perform the conversion to a symbol
        /// </summary>
        /// <param name="value">The source SymbolTemplate being passed to the target</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic</param>
        /// <param name="culture">The culture of the conversion</param>
        /// <returns>
        /// The symbol to be passed to the target dependency property
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is SymbolTemplate))
                return value;

            SymbolTemplate symbolTemplate = value as SymbolTemplate;

            if (symbolTemplate.Symbol == null)
                return null;
            Symbol convertedSymbol = symbolTemplate.Symbol;

            if (symbolTemplate.FeatureTemplate == null)
                return convertedSymbol;

            FeatureTemplate featureTemplate = symbolTemplate.FeatureTemplate;

            if (symbolTemplate.Symbol is FSSymbols.SimpleFillSymbol ||
                symbolTemplate.Symbol is FSSymbols.PictureFillSymbol ||
                symbolTemplate.Symbol is FSSymbols.SimpleLineSymbol)
            {

                switch (featureTemplate.DrawingTool)
                {
                    case FeatureEditTool.Polygon:
                        convertedSymbol = new PolygonSymbol();
                        break;
                    case FeatureEditTool.AutoCompletePolygon:
                        convertedSymbol = new AutoCompletePolygonSymbol();
                        break;
                    case FeatureEditTool.Circle:
                        convertedSymbol = new CircleSymbol();
                        break;
                    case FeatureEditTool.Ellipse:
                        convertedSymbol = new EllipseSymbol();
                        break;
                    case FeatureEditTool.Rectangle:
                        convertedSymbol = new RectangleSymbol();
                        break;
                    case FeatureEditTool.Freehand:
                        if (symbolTemplate.Symbol is FSSymbols.SimpleLineSymbol)
                            convertedSymbol = new FreehandLineSymbol();
                        else
                            convertedSymbol = new FreehandPolygonSymbol();
                        break;
                    case FeatureEditTool.Line:
                        convertedSymbol = new PolylineSymbol();
                        break;
                }

                if (convertedSymbol is ShapeMarkerSymbol)
                {
                    ShapeMarkerSymbol shapeMarkerSymbol = convertedSymbol as ShapeMarkerSymbol;
                    FillMarkerSymbol fillMarkerSymbol = convertedSymbol as FillMarkerSymbol;
                    if (symbolTemplate.Symbol is FSSymbols.SimpleFillSymbol)
                    {
                        ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleFillSymbol simpleFillSymbol =
                            symbolTemplate.Symbol as FSSymbols.SimpleFillSymbol;
                        fillMarkerSymbol.Stroke = simpleFillSymbol.BorderBrush;
                        fillMarkerSymbol.Fill = simpleFillSymbol.Fill;
                        fillMarkerSymbol.StrokeStyle = simpleFillSymbol.BorderStyle;
                        fillMarkerSymbol.StrokeThickness = simpleFillSymbol.BorderThickness;
                        shapeMarkerSymbol.SelectionColor = simpleFillSymbol.SelectionColor;
                    }
                    else if (symbolTemplate.Symbol is FSSymbols.PictureFillSymbol)
                    {
                        ESRI.ArcGIS.Client.FeatureService.Symbols.PictureFillSymbol pictureFillSymbol =
                            symbolTemplate.Symbol as FSSymbols.PictureFillSymbol;
                        fillMarkerSymbol.Stroke = pictureFillSymbol.BorderBrush;
                        fillMarkerSymbol.StrokeStyle = pictureFillSymbol.BorderStyle;
                        fillMarkerSymbol.StrokeThickness = pictureFillSymbol.BorderThickness;
                        shapeMarkerSymbol.SelectionColor = pictureFillSymbol.SelectionColor;

                        // Create new image brush for fill in order to set stretch to Uniform
                        ImageBrush brush = new ImageBrush()
                        {
                            Stretch = Stretch.Uniform,
                            ImageSource = pictureFillSymbol.Source
                        };
                        fillMarkerSymbol.Fill = brush;
                    }
                    else if (symbolTemplate.Symbol is FSSymbols.SimpleLineSymbol)
                    {
                        ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleLineSymbol simpleLineSymbol =
                            symbolTemplate.Symbol as FSSymbols.SimpleLineSymbol;
                        shapeMarkerSymbol.Stroke = simpleLineSymbol.Color;
                        shapeMarkerSymbol.StrokeStyle = simpleLineSymbol.Style;
                        shapeMarkerSymbol.StrokeThickness = simpleLineSymbol.Width;
                        shapeMarkerSymbol.SelectionColor = simpleLineSymbol.SelectionColor;

                        if (fillMarkerSymbol != null)
                            fillMarkerSymbol.Fill = new SolidColorBrush(Colors.Transparent);
                    }
                }
            }
            
            return convertedSymbol;
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
