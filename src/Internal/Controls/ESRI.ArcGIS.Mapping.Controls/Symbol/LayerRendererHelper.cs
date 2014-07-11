/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public static class LayerRendererHelper
    {
        public static FieldInfo ChangeRenderer(this GraphicsLayer graphicsLayer, string rendererType, FieldInfo attributeField)
        {
            IRenderer newRenderer = null;
            GeometryType geometryType = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetGeometryType(graphicsLayer);
            LinearGradientBrush gradientBrush = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetGradientBrush(graphicsLayer);
            FieldInfo changedRenderAttributeField = null;
            List<Symbol> existingSymbolSet = null;
            if (Constants.ClassBreaksRenderer.Equals(rendererType))
            {
                int classBreakCount = 5;
                ClassBreaksRenderer currentRenderer = graphicsLayer.Renderer as ClassBreaksRenderer;
                if (currentRenderer != null)
                {
                    classBreakCount = currentRenderer.Classes.Count;
                    existingSymbolSet = new List<Symbol>();
                    foreach (ClassBreakInfo classBreak in currentRenderer.Classes)
                        existingSymbolSet.Add(classBreak.Symbol);
                }
                newRenderer = createClassBreakRendererBasedOnSelectedAttribute(graphicsLayer, attributeField, gradientBrush, geometryType, classBreakCount, existingSymbolSet, out changedRenderAttributeField);
            }
            else if (Constants.UniqueValueRenderer.Equals(rendererType))
            {
                UniqueValueRenderer currentRenderer = graphicsLayer.Renderer as UniqueValueRenderer;
                if (currentRenderer != null)
                {
                    existingSymbolSet = new List<Symbol>();
                    foreach (UniqueValueInfo uniqueValue in currentRenderer.Infos)
                        existingSymbolSet.Add(uniqueValue.Symbol);
                }
                newRenderer = createUniqueValueRendererBasedOnSelectedAttribute(graphicsLayer, attributeField, gradientBrush, geometryType, existingSymbolSet, out changedRenderAttributeField);
            }
            else if (Constants.SimpleRenderer.Equals(rendererType))
            {
                newRenderer = createNewDefaultSimpleRenderer(graphicsLayer);
            }

            assignNewRendererToLayer(graphicsLayer, newRenderer);
            if (changedRenderAttributeField != null)
                ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetRendererAttributeDisplayName(graphicsLayer, changedRenderAttributeField.DisplayName);
            else if (attributeField != null)
                ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetRendererAttributeDisplayName(graphicsLayer, attributeField.DisplayName);
            else
                ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetRendererAttributeDisplayName(graphicsLayer, null);
            return changedRenderAttributeField;
        }

        public static IRenderer CloneRenderer(this IRenderer renderer)
        {
            if (renderer == null)
                return null;
            SimpleRenderer simpleRenderer = renderer as SimpleRenderer;
            if (simpleRenderer != null)
            {
                return new SimpleRenderer()
                {
                    Symbol = simpleRenderer.Symbol != null ? simpleRenderer.Symbol.CloneSymbol() : null,
                };
            }
            else
            {
                ClassBreaksRenderer classBreaksRenderer = renderer as ClassBreaksRenderer;
                if (classBreaksRenderer != null)
                {
                    ClassBreaksRenderer clone = new ClassBreaksRenderer() { 
                         Field = classBreaksRenderer.Field,
                         DefaultSymbol = classBreaksRenderer.DefaultSymbol != null ? classBreaksRenderer.DefaultSymbol.CloneSymbol() : null,
                    };
                    if (classBreaksRenderer.Classes != null)
                    {
                        foreach (ClassBreakInfo classBreak in classBreaksRenderer.Classes)
                        {
                            clone.Classes.Add(new ClassBreakInfo() { 
                                 Description = classBreak.Description,
                                 Label = classBreak.Label,
                                 MaximumValue = classBreak.MaximumValue,
                                 MinimumValue = classBreak.MinimumValue,
                                 Symbol = classBreak.Symbol != null ? classBreak.Symbol.CloneSymbol() : null,
                            });
                        }
                    }
                    return clone;
                }
                else
                {
                    UniqueValueRenderer uniqueValueRenderer = renderer as UniqueValueRenderer;
                    if (uniqueValueRenderer != null)
                    {
                        UniqueValueRenderer clone = new UniqueValueRenderer() { 
                             Field = uniqueValueRenderer.Field,
                             DefaultSymbol = uniqueValueRenderer.DefaultSymbol != null ? uniqueValueRenderer.DefaultSymbol.CloneSymbol() : null,
                        };
                        if (uniqueValueRenderer.Infos != null)
                        {
                            foreach (UniqueValueInfo uniqueValue in uniqueValueRenderer.Infos)
                            {
                                clone.Infos.Add(new UniqueValueInfo()
                                {
                                    Description = uniqueValue.Description,
                                    Label = uniqueValue.Label,
                                    Value = uniqueValue.Value,
                                    Symbol = uniqueValue.Symbol != null ? uniqueValue.Symbol.CloneSymbol() : null,
                                });
                            }
                        }
                        return clone;
                    }
                }
            }
            return null;
        }

        public static FieldInfo ChangeAttributeForRenderer(this GraphicsLayer graphicsLayer, FieldInfo newAttributeField)
        {
            IRenderer newRenderer = null;
            GeometryType geometryType = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetGeometryType(graphicsLayer);
            LinearGradientBrush gradientBrush = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetGradientBrush(graphicsLayer);
            FieldInfo changedAttribute = null;
            List<Symbol> existingSymbolSet;
            ClassBreaksRenderer classBreaksRenderer = graphicsLayer.Renderer as ClassBreaksRenderer;
            if (classBreaksRenderer != null)
            {
                existingSymbolSet = new List<Symbol>();
                foreach (ClassBreakInfo classBreak in classBreaksRenderer.Classes)
                    existingSymbolSet.Add(classBreak.Symbol);
                newRenderer = createClassBreakRendererBasedOnSelectedAttribute(graphicsLayer, newAttributeField, gradientBrush, geometryType, classBreaksRenderer.Classes.Count, existingSymbolSet, out changedAttribute);
            }
            else
            {
                UniqueValueRenderer uniqueValueRenderer = graphicsLayer.Renderer as UniqueValueRenderer;
                if (uniqueValueRenderer != null)
                {
                    existingSymbolSet = new List<Symbol>();
                    foreach (UniqueValueInfo uniqueValue in uniqueValueRenderer.Infos)
                        existingSymbolSet.Add(uniqueValue.Symbol);
                    newRenderer = createUniqueValueRendererBasedOnSelectedAttribute(graphicsLayer, newAttributeField, gradientBrush, geometryType, existingSymbolSet, out changedAttribute);
                }
            }

            assignNewRendererToLayer(graphicsLayer, newRenderer);
            graphicsLayer.Refresh();
            if (changedAttribute != null)
                ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetRendererAttributeDisplayName(graphicsLayer, changedAttribute.DisplayName);
            else if (newAttributeField != null)
                ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetRendererAttributeDisplayName(graphicsLayer, newAttributeField.DisplayName);
            else
                ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetRendererAttributeDisplayName(graphicsLayer, null);
            return changedAttribute;
        }

        public static void ApplyColorRampGradientBrushToRenderer(this GraphicsLayer graphicsLayer, LinearGradientBrush brush)
        {
            // must have atleast 2 stops
            if (brush == null || brush.GradientStops.Count < 2)
                return;

            if (graphicsLayer == null)
                return;

            ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetGradientBrush(graphicsLayer, brush);
            ClassBreaksRenderer classBreaskRenderer = graphicsLayer.Renderer as ClassBreaksRenderer;
            if (classBreaskRenderer != null)
            {
                if (classBreaskRenderer.Classes.Count < 2)
                    return;

                List<Symbol> symbols = new List<Symbol>();
                foreach (ClassBreakInfo classBreak in classBreaskRenderer.Classes)
                {
                    symbols.Add(classBreak.Symbol);
                }
                applyLinearGradientBrushToSymbolSet(symbols, brush, classBreaskRenderer.DefaultSymbol);

                // Some changes to ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleSymbols do not trigger the legendchanged event. Fix this by triggering the event by cloning the symbol.
                bool refresh = false;
                foreach (ClassBreakInfo classBreak in classBreaskRenderer.Classes)
                    if (classBreak.Symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol || classBreak.Symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleLineSymbol || classBreak.Symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleFillSymbol)
                    {
                        classBreak.Symbol = classBreak.Symbol.CloneSymbol();
                        refresh = true;
                    }
                if (refresh) graphicsLayer.Refresh(); //Only the first time a gradientbrush is applied the layer refreshes itself, so fix this by refreshing manually. 

            }
            else
            {
                UniqueValueRenderer uniqueValueRenderer = graphicsLayer.Renderer as UniqueValueRenderer;
                if (uniqueValueRenderer != null)
                {
                    List<Symbol> symbols = new List<Symbol>();
                    foreach (UniqueValueInfo uniqueValue in uniqueValueRenderer.Infos)
                    {
                        symbols.Add(uniqueValue.Symbol);
                    }
                    applyLinearGradientBrushToSymbolSet(symbols, brush, uniqueValueRenderer.DefaultSymbol);

                    // Some changes to ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleSymbols do not trigger the legendchanged event. Fix this by triggering the event by cloning the symbol.
                    bool refresh = false;
                    foreach (UniqueValueInfo uniqueValue in uniqueValueRenderer.Infos)
                        if (uniqueValue.Symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol || uniqueValue.Symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleLineSymbol || uniqueValue.Symbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleFillSymbol)
                        {
                            uniqueValue.Symbol = uniqueValue.Symbol.CloneSymbol();
                            refresh = true;
                        }
                    if (refresh) graphicsLayer.Refresh(); //Only the first time a gradientbrush is applied the layer refreshes itself, so fix this by refreshing manually. 

                }
            }
        }

        public static void RegenerateClassBreaksOnCountChanged(this GraphicsLayer graphicsLayer, int newClassBreaksCount)
        {
            if (newClassBreaksCount < 2) // don't allow a classbreaks renderer to have less than 2 class breaks
                return;

            ClassBreaksRenderer classBreaksRenderer = graphicsLayer.Renderer as ClassBreaksRenderer;
            if (classBreaksRenderer == null)
                return;

            LinearGradientBrush currentBrush = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetGradientBrush(graphicsLayer);
            string attributeName = classBreaksRenderer.Field;

            FieldInfo attributeField = null;
            IEnumerable<FieldInfo> fields = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetFields(graphicsLayer);
            if (fields != null)
                attributeField = fields.FirstOrDefault<FieldInfo>(f => f.Name == attributeName);

            // Rebuild the class breaks
            double min, max;
            getMinAndMaxForGraphicsLayer(graphicsLayer, attributeField, out min, out max);

            List<Symbol> existingSymbolSet = new List<Symbol>();
            foreach (ClassBreakInfo classBreak in classBreaksRenderer.Classes)
                existingSymbolSet.Add(classBreak.Symbol);
            IRenderer newRenderer = createNewDefaultClassBreaksRenderer(graphicsLayer, attributeField, min, max, newClassBreaksCount, currentBrush, existingSymbolSet);

            assignNewRendererToLayer(graphicsLayer, newRenderer);
        }

        private static IRenderer createClassBreakRendererBasedOnSelectedAttribute(GraphicsLayer graphicsLayer, FieldInfo attributeField, LinearGradientBrush defaultColorRampGradientBrush, GeometryType geometryType, int classBreakCount, IEnumerable<Symbol> existingSymbolSet, out FieldInfo changedAttributeField)
        {
            changedAttributeField = attributeField;
            ClassBreaksRenderer classBreaksRenderer = graphicsLayer.Renderer as ClassBreaksRenderer;
            if (classBreaksRenderer != null) // Already using classBreaksRenderer
            {
                if (attributeField != null && attributeField.Name != classBreaksRenderer.Field) // Using different attribute for the renderer
                {
                    classBreaksRenderer.Field = attributeField.Name;

                    // Rebuild the class breaks
                    double min, max;
                    getMinAndMaxForGraphicsLayer(graphicsLayer, attributeField, out min, out max);

                    return createNewDefaultClassBreaksRenderer(graphicsLayer, attributeField, min, max, classBreakCount, defaultColorRampGradientBrush, existingSymbolSet);
                }
            }
            else
            {
                string attributeFieldName = null;
                if (graphicsLayer.Renderer is UniqueValueRenderer)
                {
                    // 1. Check if the renderer has an attribute set as UniqueValue Renderer
                    UniqueValueRenderer uniqueValueRenderer = graphicsLayer.Renderer as UniqueValueRenderer;
                    if (uniqueValueRenderer != null)
                        attributeFieldName = uniqueValueRenderer.Field;
                }

                if (string.IsNullOrEmpty(attributeFieldName)) // No attribute known .. use the first numeric field
                {
                    FieldInfo field = null;
                    Collection<FieldInfo> layerFields = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetFields(graphicsLayer);
                    if (layerFields != null)
                        field = layerFields.FirstOrDefault<FieldInfo>(f => f.FieldType == FieldType.DecimalNumber || f.FieldType == FieldType.Integer || f.FieldType == FieldType.Currency);
                    if (field != null)
                    {
                        attributeField = field;
                        changedAttributeField = field;
                        attributeFieldName = field.Name;
                    }
                }
                else
                {
                    FieldInfo field = null;
                    Collection<FieldInfo> layerFields = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetFields(graphicsLayer);
                    if (layerFields != null)
                        field = layerFields.FirstOrDefault<FieldInfo>(f => f.Name == attributeFieldName); // find matching field
                    if (field == null || (field.FieldType != FieldType.Integer && field.FieldType != FieldType.DecimalNumber && field.FieldType != FieldType.Currency))
                    {
                        // if the previous field was not a number or currency, then automatically switch it to the first numeric one
                        field = layerFields.FirstOrDefault<FieldInfo>(f => f.FieldType == FieldType.Integer || f.FieldType == FieldType.DecimalNumber || f.FieldType == FieldType.Currency);
                        if (field != null)
                        {
                            attributeField = field;
                            changedAttributeField = field;
                            attributeFieldName = field.Name;
                        }
                    }
                }

                if (string.IsNullOrEmpty(attributeFieldName)) // An attribute name must be specified
                    return null;

                // Loop through graphics and calculate max and min
                double min, max;
                getMinAndMaxForGraphicsLayer(graphicsLayer, attributeField, out min, out max);

                return createNewDefaultClassBreaksRenderer(graphicsLayer, attributeField, min, max, classBreakCount, defaultColorRampGradientBrush, existingSymbolSet);
            }
            return null;
        }

        private static IRenderer createNewDefaultClassBreaksRenderer(GraphicsLayer graphicsLayer, FieldInfo attributeField, double min, double max, int numOfClassBreaks, LinearGradientBrush defaultColorRampGradientBrush, IEnumerable<Symbol> existingSymbolSet)
        {
            Symbol defaultSymbol = graphicsLayer.GetDefaultSymbolClone();
            ClassBreaksRenderer renderer = new ClassBreaksRenderer()
            {
                Field = attributeField != null ? attributeField.Name : null,
                DefaultSymbol = defaultSymbol,
            };
            List<Symbol> symbols = new List<Symbol>();
            if (numOfClassBreaks < 2) // classbreaks renderer must have atleast 2 classbreaks
                numOfClassBreaks = 2;
            double rangeSize = Math.Round((max - min) / numOfClassBreaks, 2);
            double rangeDelta = 1.0; // delta between 2 class ranges // we choose an integral size
            double lastRangeDeltaIncr = 1.0; // SL core api requires the last classbreak to be greater than max value of dataset
            bool fractionalIncrement = false;
            if (Math.Round(max, 0) != max)// we are dealing with a non-integeral values, so our delta's are in fractional increments
            {
                fractionalIncrement = true;
                rangeDelta = 0.01;
                lastRangeDeltaIncr = 0.01;
            }

            double startValue = min;
            for (int i = 0; i < numOfClassBreaks; i++)
            {
                Symbol symbol = null;
                if (existingSymbolSet != null)
                    symbol = existingSymbolSet.ElementAtOrDefault(i);
                if (symbol == null)
                    symbol = graphicsLayer.GetDefaultSymbolClone();
                double endValue = (startValue + rangeSize) - rangeDelta;
                ClassBreakInfo classBreak = new ClassBreakInfo()
                {
                    MinimumValue = fractionalIncrement ? startValue : Math.Floor(startValue),
                    MaximumValue = fractionalIncrement ? endValue : Math.Floor(endValue),
                    Symbol = symbol,
                };
                if (i == numOfClassBreaks - 1) // last class break
                {
                    classBreak.MaximumValue = max + lastRangeDeltaIncr;
                    if (max > 1000000) // SL has a limitation on values greater than a million http://msdn.microsoft.com/en-us/library/bb412393.aspx
                        classBreak.MaximumValue += 2.0;// the +2 is to workaround Silverlights limitation of single precision values
                }
                symbols.Add(symbol);
                renderer.Classes.Add(classBreak);
                startValue += rangeSize;
            }
            if (defaultColorRampGradientBrush != null)
            {
                if (existingSymbolSet == null) // apply the gradient brush, only if symbols have not been pre-defined
                {
                    applyLinearGradientBrushToSymbolSet(symbols, defaultColorRampGradientBrush, defaultSymbol);
                }
            }
            return renderer;
        }

        private static UniqueValueRenderer createNewDefaultUniqueValueRenderer(GraphicsLayer graphicsLayer, IEnumerable<object> uniqueValues, FieldInfo attributeField, LinearGradientBrush defaultColorRampGradientBrush, IEnumerable<Symbol> existingSymbolSet)
        {
            Symbol defaultSymbol = graphicsLayer.GetDefaultSymbol();
            UniqueValueRenderer uniqueValueRenderer = new UniqueValueRenderer()
            {
                Field = attributeField != null ? attributeField.Name : null,
                DefaultSymbol = defaultSymbol,
            };
            if (uniqueValues != null)
            {
                List<Symbol> symbols = new List<Symbol>();
                int i = 0;
                foreach (object uniqueValue in uniqueValues)
                {
                    Symbol symbol = null;
                    if (existingSymbolSet != null)
                        symbol = existingSymbolSet.ElementAtOrDefault(i);
                    if (symbol == null)
                        symbol = graphicsLayer.GetDefaultSymbolClone();
                    uniqueValueRenderer.Infos.Add(new UniqueValueInfoObj()
                    {
                        Symbol = symbol,
                        SerializedValue = uniqueValue,
                        FieldType = attributeField.FieldType,
                    });
                    symbols.Add(symbol);
                    i++;
                }
                if (defaultColorRampGradientBrush != null)
                {
                    if (existingSymbolSet == null) // apply the gradient brush, only if symbols have not been pre-defined
                    {
                        applyLinearGradientBrushToSymbolSet(symbols, defaultColorRampGradientBrush, defaultSymbol);
                    }
                }
            }
            return uniqueValueRenderer;
        }

        private static IEnumerable<object> getAllUniqueValues(GraphicsLayer graphicsLayer, FieldInfo attributeField)
        {
            if (graphicsLayer == null || attributeField == null || string.IsNullOrEmpty(attributeField.Name))
                return null;
            List<object> uniqValues = new List<object>();
            foreach (Graphic g in graphicsLayer.Graphics)
            {
                object o;
                if (g.Attributes.TryGetValue(attributeField.Name, out o))
                {
                    if (o == null)
                        continue; // ignore blank values
                    string objString = o.ToString();
                    if (string.IsNullOrEmpty(objString))
                        continue;
                    if (!uniqValues.Contains(o))
                        uniqValues.Add(o);
                }
            }
            uniqValues.Sort();
            return uniqValues;
        }

        private static void applyLinearGradientBrushToSymbolSet(List<Symbol> symbols, LinearGradientBrush brush, Symbol defaultSymbol)
        {
            int totalSymbols = symbols.Count;
            if (totalSymbols < 1)
                return;
            // Pick the boundary colors
            Color colorFirst = brush.GradientStops[0].Color;
            Color colorLast = brush.GradientStops[brush.GradientStops.Count - 1].Color;

            // The first symbol gets the start value            
            applyColorToSymbol(symbols[0], colorFirst);
            // also assign the same color to the default symbol
            applyColorToSymbol(defaultSymbol, colorFirst);

            // the last symbol gets the last value
            applyColorToSymbol(symbols[totalSymbols - 1], colorLast);

            if (totalSymbols < 2)
                return;

            // we assigned colors to the first and last .. the remaining ones will use interpolated colors
            // The interpolation will be based on number of symbols
            byte startR = colorFirst.R;
            byte endR = colorLast.R;
            byte startG = colorFirst.G;
            byte endG = colorLast.G;
            byte startB = colorFirst.B;
            byte endB = colorLast.B;
            double stepR = ((double)((int)endR - (int)startR)) / (totalSymbols - 1);
            double stepG = ((double)((int)endG - (int)startG)) / (totalSymbols - 1);
            double stepB = ((double)((int)endB - (int)startB)) / (totalSymbols - 1);

            double currentR = startR;
            double currentG = startG;
            double currentB = startB;
            for (int i = 1; i < totalSymbols - 1; i++)
            {
                currentR += stepR;
                currentG += stepG;
                currentB += stepB;
                Symbol symbol = symbols[i];
                if (symbol == null)
                    continue;

                applyColorToSymbol(symbol, Color.FromArgb(255, (byte)currentR, (byte)currentG, (byte)currentB));
            }
        }

        private static void applyColorToSymbol(Symbol symbol, Color color)
        {
            if (symbol == null)
                return;

            FillSymbol fillSymbol = symbol as FillSymbol;
            if (fillSymbol != null)
            {
                byte currentAlpha = (byte)255;
                if (fillSymbol.Fill != null)
                    currentAlpha = (byte)(255 * fillSymbol.Fill.Opacity); // Preserve current opacity of symbol color
                string colorHex = Color.FromArgb(currentAlpha, color.R, color.G, color.B).ToString();
                Color newColor = ColorPickerUtils.FromHex(colorHex);
                if (fillSymbol is ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleFillSymbol)
                    (fillSymbol as ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleFillSymbol).Color = newColor;
                else
                    fillSymbol.Fill = new SolidColorBrush(newColor);
            }
            else
            {
                LineSymbol lineSymbol = symbol as LineSymbol;
                if (lineSymbol != null)
                {
                    byte currentAlpha = (byte)255;
                    if (lineSymbol.Color != null)
                        currentAlpha = (byte)(255 * lineSymbol.Color.Opacity);// Preserve current opacity of symbol color
                    string colorHex = Color.FromArgb(currentAlpha, color.R, color.G, color.B).ToString();
                    Color newColor = ColorPickerUtils.FromHex(colorHex);
                    lineSymbol.Color = new SolidColorBrush(newColor);
                }
                else
                {
                    ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol markerSymbol = symbol as ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol;
                    if (markerSymbol != null)
                    {
                        byte currentAlpha = (byte)255;
                        if (markerSymbol.Color != null)
                            currentAlpha = (byte)(255 * markerSymbol.Color.Opacity);// Preserve current opacity of symbol color
                        string colorHex = Color.FromArgb(currentAlpha, color.R, color.G, color.B).ToString();
                        Color newColor = ColorPickerUtils.FromHex(colorHex);
                        markerSymbol.Color = new SolidColorBrush(newColor);
                    }
                    else
                    {
                        ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol simpleMarkerSymbol = symbol as ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol;
                        if (simpleMarkerSymbol != null)
                        {
                            byte currentAlpha = (byte)255;
                            if (simpleMarkerSymbol.Color != null)
                                currentAlpha = (byte)(255 * simpleMarkerSymbol.Color.Opacity);// Preserve current opacity of symbol color
                            string colorHex = Color.FromArgb(currentAlpha, color.R, color.G, color.B).ToString();
                            Color newColor = ColorPickerUtils.FromHex(colorHex);
                            simpleMarkerSymbol.Color = new SolidColorBrush(newColor);
                        }
                        else
                        {
                            ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol sms = symbol as ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol;
                            if (sms != null)
                            {
                                byte currentAlpha = (byte)255;
                                if (sms.Color is SolidColorBrush)
                                    currentAlpha = (sms.Color as SolidColorBrush).Color.A;
                                else if (sms.Color != null)
                                    currentAlpha = (byte)(255 * sms.Color.Opacity);// Preserve current opacity of symbol color
                                string colorHex = Color.FromArgb(currentAlpha, color.R, color.G, color.B).ToString();
                                Color newColor = ColorPickerUtils.FromHex(colorHex);
                                sms.Color = new SolidColorBrush(newColor);
                                if (sms.OutlineColor != null)
                                    sms.OutlineColor = new SolidColorBrush(newColor);
                            }
                        }
                    }
                }
            }
        }

        private static void getMinAndMaxForGraphicsLayer(GraphicsLayer graphicsLayer, FieldInfo attributeField, out double min, out double max)
        {
            min = double.NaN;
            max = double.NaN;
            if (graphicsLayer != null && attributeField != null && !string.IsNullOrEmpty(attributeField.Name))
            {
                foreach (Graphic g in graphicsLayer.Graphics)
                {
                    object o;
                    if (g.Attributes.TryGetValue(attributeField.Name, out o))
                    {
                        if (o == null)
                            continue;
                        double value;
                        NumericFieldValue numericFieldValue = o as NumericFieldValue;
                        if (numericFieldValue != null)
                        {
                            o = numericFieldValue.Value;
                        }
                        else
                        {
                            CurrencyFieldValue currencyFieldValue = o as CurrencyFieldValue;
                            if (currencyFieldValue != null)
                            {
                                o = currencyFieldValue.Value;
                            }
                        }

                        if (double.TryParse(
                            string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", o),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out value))
                        {
                            if (double.IsNaN(min)) // first value
                                min = value;
                            else
                                min = Math.Min(min, value);

                            if (double.IsNaN(max)) // first value
                                max = value;
                            else
                                max = Math.Max(max, value);
                        }
                    }
                }
            }
            if (double.IsNaN(min))
                min = 0d;
            if (double.IsNaN(max))
                max = 0d;
        }

        private static void assignNewRendererToLayer(GraphicsLayer graphicsLayer, IRenderer newRenderer)
        {
            if (graphicsLayer == null)
                return;

            if (newRenderer == null)
            {
                newRenderer = graphicsLayer.CreateDefaultRenderer();
            }

            // Assign the renderer to the layer (potentially need to clear out FeatureSymbol)
            graphicsLayer.ChangeRenderer(newRenderer);
        }

        private static UniqueValueRenderer createUniqueValueRendererBasedOnSelectedAttribute(GraphicsLayer graphicsLayer, FieldInfo attributeField, LinearGradientBrush defaultColorRampGradientBrush, GeometryType geometryType, IEnumerable<Symbol> existingSymbolSet, out FieldInfo changedAttributeField)
        {
            changedAttributeField = attributeField;
            UniqueValueRenderer uniqueValueRenderer = graphicsLayer.Renderer as UniqueValueRenderer;
            if (uniqueValueRenderer != null) // already using unique value renderer
            {
                if (attributeField != null && attributeField.Name != uniqueValueRenderer.Field) // check if attribute has been changed
                {
                    IEnumerable<object> uniqueValues = getAllUniqueValues(graphicsLayer, attributeField);
                    uniqueValueRenderer = createNewDefaultUniqueValueRenderer(graphicsLayer, uniqueValues, attributeField, defaultColorRampGradientBrush, existingSymbolSet);
                }
            }
            else
            {
                string attributeFieldName = null;
                if (graphicsLayer.Renderer is ClassBreaksRenderer)
                {
                    // Check if the class breaks renderer had an attribute set. If so, honor that attribute name
                    ClassBreaksRenderer classBreakRenderer = graphicsLayer.Renderer as ClassBreaksRenderer;
                    if (classBreakRenderer != null)
                        attributeFieldName = classBreakRenderer.Field;
                }

                if (string.IsNullOrEmpty(attributeFieldName)) // No attribute known .. use the first non-attachment field
                {
                    FieldInfo field = null;
                    Collection<FieldInfo> layerFields = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetFields(graphicsLayer);
                    if (layerFields != null)
                        field = layerFields.FirstOrDefault<FieldInfo>(f => f.FieldType != FieldType.Attachment);
                    if (field != null)
                    {
                        changedAttributeField = field;
                        attributeField = field;
                        attributeFieldName = field.Name;
                    }
                }

                if (string.IsNullOrEmpty(attributeFieldName)) // An attribute name must be specified
                    return null;

                IEnumerable<object> uniqueValues = getAllUniqueValues(graphicsLayer, attributeField);
                // Create a default unique value renderer
                uniqueValueRenderer = createNewDefaultUniqueValueRenderer(graphicsLayer, uniqueValues, attributeField, defaultColorRampGradientBrush, existingSymbolSet);
            }
            return uniqueValueRenderer;
        }

        private static SimpleRenderer createNewDefaultSimpleRenderer(GraphicsLayer graphicsLayer)
        {
            Symbol defaultSymbol = graphicsLayer.GetDefaultSymbolClone();
            SimpleRenderer rendererRenderer = new SimpleRenderer()
            {
                Symbol = defaultSymbol,
            };
            return rendererRenderer;
        }
    }

}
