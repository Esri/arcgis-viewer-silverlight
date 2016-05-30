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
using System.Windows.Data;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name="ColorRampListBox", Type=typeof(ListBox))]
    public class PreDefinedColorRampControl : Control
    {
        ListBox ColorRampListBox = null;

        private Rectangle _selectedRectangle;
        public PreDefinedColorRampControl()
        {
            DefaultStyleKey = typeof(PreDefinedColorRampControl);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ColorRampListBox = GetTemplateChild("ColorRampListBox") as ListBox;
            if(ColorRampListBox != null)
            {
                ColorRampListBox.SelectionChanged += new SelectionChangedEventHandler(onListBoxSelectionChanged);
                loadColorRampListBox();
            }
        }

        public event EventHandler<GradientBrushChangedEventArgs> GradientBrushChanged;
        protected void OnGradientBrushChanged(GradientBrushChangedEventArgs e)
        {
            if (GradientBrushChanged != null)
                GradientBrushChanged(this, e);
        }        
        
        private bool _fireEvent = false; // used to avoid the event firing when programatically setting the selectedindex
        private void onListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorRampListBox == null)
                return;
            ListBoxItem item = ColorRampListBox.SelectedItem as ListBoxItem;
            if (item == null)
                return;
            Rectangle rect = item.Content as Rectangle;
            if (rect == null)
                return;
            if (_selectedRectangle != null)
            {
                // un-select the previous one
                _selectedRectangle.StrokeThickness = 1;
            }
            // select the current one
            _selectedRectangle = rect;
            _selectedRectangle.StrokeThickness = 2;
            LinearGradientBrush brush = rect.Fill as LinearGradientBrush;
            if (brush == null)
                return;            
            if(_fireEvent)
                OnGradientBrushChanged(new GradientBrushChangedEventArgs { GradientBrush = brush });            
        }

        internal void Initialize()
        {
            _fireEvent = false;
            setLayerInfoColor();            
            _fireEvent = true;
        }        
        
        private void setLayerInfoColor()
        {
            if (ColorRampListBox == null)
                return;
            if (ColorRampListBox.Items.Count > 0)
            {
                GraphicsLayer layerInfo = DataContext as GraphicsLayer;
                if (layerInfo != null)
                {
                    ClassBreaksRenderer classBreaksRenderer = layerInfo.Renderer as ClassBreaksRenderer;
                    if(classBreaksRenderer != null)
                    {
                        // TODO:- evaluate whether this condition is valid and apply 
                        // && !layerInfo.ClassBreaks.IsUsingCustomFillSymbolColors
                        if(classBreaksRenderer.Classes.Count > 1)
                        {
                            // Compare the color of the boundary classbreaks
                            // with the start and end color gradients 
                            // and set the selected item
                            ClassBreakInfo first = classBreaksRenderer.Classes[0];
                            ClassBreakInfo last = classBreaksRenderer.Classes[classBreaksRenderer.Classes.Count - 1];
                            if (first.Symbol is SimpleFillSymbol && last.Symbol is SimpleFillSymbol)
                            {
                                SimpleFillSymbol firstSymbol = first.Symbol as SimpleFillSymbol;
                                SimpleFillSymbol lastSymbol = last.Symbol as SimpleFillSymbol;
                                Color firstColor = (firstSymbol.Fill as SolidColorBrush).Color;
                                Color lastColor = (lastSymbol.Fill as SolidColorBrush).Color;
                                setSelectedColorInRamp(firstColor, lastColor);
                            }
                            else if (first.Symbol is SimpleLineSymbol && last.Symbol is SimpleLineSymbol)
                            {
                                SimpleLineSymbol firstSymbol = first.Symbol as SimpleLineSymbol;
                                SimpleLineSymbol lastSymbol = last.Symbol as SimpleLineSymbol;
                                Color firstColor = (firstSymbol.Color as SolidColorBrush).Color;
                                Color lastColor = (lastSymbol.Color as SolidColorBrush).Color;
                                setSelectedColorInRamp(firstColor, lastColor);
                            }
                        }
                        else
                        {
                            // default to the first one
                            ListBoxItem item = ColorRampListBox.Items[0] as ListBoxItem;
                            if (_selectedRectangle != null)
                            {
                                // un-select the previous one
                                _selectedRectangle.StrokeThickness = 1;
                            }
                            Rectangle rect = item.Content as Rectangle;
                            if (rect != null)
                            {
                                // select the current one
                                _selectedRectangle = rect;
                                _selectedRectangle.StrokeThickness = 2;
                            }
                            ColorRampListBox.SelectedIndex = 0;
                        }
                    }
                }
            }            
        }

        private void setSelectedColorInRamp(Color firstColor, Color lastColor)
        {
            if (ColorRampListBox == null)
                return;
            foreach (ListBoxItem item in ColorRampListBox.Items)
            {
                if (item == null)
                    continue;
                Rectangle rect = item.Content as Rectangle;
                if (rect == null)
                    continue;
                LinearGradientBrush brush = rect.Fill as LinearGradientBrush;
                if (brush == null)
                    continue;
                if (brush.GradientStops == null || brush.GradientStops.Count < 2)
                    continue;
                Color startGradientColor = brush.GradientStops[0].Color;
                Color endGradientColor = brush.GradientStops[brush.GradientStops.Count - 1].Color;
                if (startGradientColor == firstColor
                    && endGradientColor == lastColor)
                {
                    item.IsSelected = true;
                    if (_selectedRectangle != null)
                    {
                        // un-select the previous one
                        _selectedRectangle.StrokeThickness = 1;
                    }
                    // select the current one
                    _selectedRectangle = rect;
                    _selectedRectangle.StrokeThickness = 2;
                    break;
                }
            }
        }

        private void PredefinedColorRampControl_Loaded(object sender, RoutedEventArgs e)
        {            
            Initialize();
        }        
        
        private void loadColorRampListBox()
        {            
            if (ColorRampListBox.Items.Count < 1)
            {
                foreach (LinearGradientBrush brush in PreDefinedColors.Brushes)
                {
                    ListBoxItem item = new ListBoxItem
                    {                        
                        Content = createColorRectangle(brush),
                        IsSelected = doesLayerColorRampBrushMatchBrush(brush)
                    };
                    ColorRampListBox.Items.Add(item);
                }
                setLayerInfoColor();
            }            
        }

        private bool doesLayerColorRampBrushMatchBrush(LinearGradientBrush brush)
        {
            if (brush == null || brush.GradientStops.Count < 2)
                return false;
            // nik:- TODO (Refactor)
            //LayerInfo layerInfo = ApplicationInstance.Instance.CurrentLayerBeingConfigured;
            //if (layerInfo != null)
            //{
            //    if (layerInfo.ClassBreaks != null
            //        && !layerInfo.ClassBreaks.IsUsingCustomFillSymbolColors
            //        && layerInfo.ClassBreaks.Count > 1)
            //    {
            //        // Compare the color of the boundary classbreaks
            //        // with the start and end color gradients 
            //        // and set the selected item
            //        ClassBreakInfo first = layerInfo.ClassBreaks[0];
            //        ClassBreakInfo last = layerInfo.ClassBreaks[layerInfo.ClassBreaks.Count - 1];
            //        if (first.SymbolInfo is SimpleFillSymbol && last.SymbolInfo is SimpleFillSymbol)
            //        {
            //            SimpleFillSymbol firstSymbol = first.SymbolInfo as SimpleFillSymbol;
            //            SimpleFillSymbol lastSymbol = last.SymbolInfo as SimpleFillSymbol;
            //            Color firstColor = ColorPickerUtils.FromHex(firstSymbol.FillColor);
            //            Color lastColor = ColorPickerUtils.FromHex(lastSymbol.FillColor);
            //            return firstColor == brush.GradientStops[0].Color && lastColor == brush.GradientStops[brush.GradientStops.Count - 1].Color;
            //        }
            //        else if (first.SymbolInfo is SimpleLineSymbol && last.SymbolInfo is SimpleLineSymbol)
            //        {
            //            SimpleLineSymbol firstSymbol = first.SymbolInfo as SimpleLineSymbol;
            //            SimpleLineSymbol lastSymbol = last.SymbolInfo as SimpleLineSymbol;
            //            Color firstColor = ColorPickerUtils.FromHex(firstSymbol.Color);
            //            Color lastColor = ColorPickerUtils.FromHex(lastSymbol.Color);
            //            return firstColor == brush.GradientStops[0].Color && lastColor == brush.GradientStops[brush.GradientStops.Count - 1].Color;                        
            //        }
            //    }
            //}
            return false;
        }

        private Rectangle createColorRectangle(ColorInfo cInfo)
        {
            ColorInfoToLinearGradientBrushConverter conv = new ColorInfoToLinearGradientBrushConverter();
            return createColorRectangle((LinearGradientBrush)conv.Convert(cInfo, null, null, null));
        }

        private Rectangle createColorRectangle(LinearGradientBrush fillBrush)
        {
            if (ColorRampListBox == null)
                return null;
            double maxWidth = ColorRampListBox.ActualWidth - 30;
            if (ColorRampListBox.Width < 30 || double.IsNaN(ColorRampListBox.Width)) // anomaly
            {
                ColorRampListBox.Width = 340;
                maxWidth = 310;
            }
            if (maxWidth <= 0)
            {
                maxWidth = 310;
            }
            Rectangle rect = new Rectangle
            {
                Height = 15,
                Width = maxWidth,
                VerticalAlignment = VerticalAlignment.Stretch,
                Stroke = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(3, 1, 3, 1),
                StrokeThickness = 1,
                Fill = fillBrush
            };
            return rect;
        }
    }

    public class GradientBrushChangedEventArgs : EventArgs
    {
        public LinearGradientBrush GradientBrush { get; set; }
    }    

    internal class ColorInfo
    {
        public Color Start { get; set; }
        public Color End { get; set; }
    }

    internal class ColorInfoToLinearGradientBrushConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ColorInfo cInfo = value as ColorInfo;
            if (cInfo == null)
                return null;
            LinearGradientBrush brush = new LinearGradientBrush { 
                 GradientStops = new GradientStopCollection(),
                 StartPoint = new Point(0,1),
                 EndPoint = new Point(1,0)
            };
            brush.GradientStops.Add(new GradientStop { Color = cInfo.Start, Offset = 0.1 });
            brush.GradientStops.Add(new GradientStop { Color = cInfo.End, Offset = 1.1 });
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Not supported");
        }

        #endregion
    }
}
