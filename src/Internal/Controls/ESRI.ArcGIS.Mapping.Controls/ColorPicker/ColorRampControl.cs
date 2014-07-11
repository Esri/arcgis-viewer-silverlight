/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.Symbols;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ColorRampControl : Control
    {
        internal ListBox colorRampsListBox;
        private Rectangle _selectedRectangle;

        public ColorRampControl()
        {
            DefaultStyleKey = typeof(ColorRampControl);
        }

        public override void OnApplyTemplate()
        {
            if (colorRampsListBox != null)
                colorRampsListBox.SelectionChanged -= colorRampsListBox_SelectionChanged;

            base.OnApplyTemplate();

            colorRampsListBox = GetTemplateChild("ColorRampsListBox") as ListBox;
            if(colorRampsListBox != null)
                colorRampsListBox.SelectionChanged += colorRampsListBox_SelectionChanged;

            buildUI();

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }

        internal event EventHandler InitCompleted;

        void buildUI()
        {
            if (ColorBrushes != null)
            {
                bindColorsToListBox();
            }
            else
            {
                if (SymbolConfigProvider != null)
                {
                    SymbolConfigProvider.GetColorGradientBrushesCompleted -= SymbolConfigProvider_GetColorGradientBrushesCompleted;
                    SymbolConfigProvider.GetColorGradientBrushesCompleted += SymbolConfigProvider_GetColorGradientBrushesCompleted;
                    SymbolConfigProvider.GetColorGradientBrushesAsync(null, ColorRampType);
                }
            } 
        }

        private void bindColorsToListBox()
        {
            if (colorRampsListBox == null)
                return;

            colorRampsListBox.Items.Clear();            
            IEnumerable<LinearGradientBrush> brushes = (ColorBrushes != null) ? ColorBrushes : defaultColorBrushes;
            foreach (LinearGradientBrush brush in brushes)
            {
                ListBoxItem item = new ListBoxItem
                {
                    Content = createColorRectangle(brush),
                    IsSelected = doesLayerColorRampBrushMatchBrush(brush)
                };
                colorRampsListBox.Items.Add(item);
            }
        }

        IEnumerable<LinearGradientBrush> defaultColorBrushes;
        void SymbolConfigProvider_GetColorGradientBrushesCompleted(object sender, GetColorGradientBrushesCompletedEventArgs e)
        {
            defaultColorBrushes = e.ColorBrushes;
            bindColorsToListBox();

            //unsubscribe as otherwise each time another instance of color ramp control makes a request via SymbolConfigProvier, each exiting instance will catch the event too
            if(SymbolConfigProvider != null)
                SymbolConfigProvider.GetColorGradientBrushesCompleted -= SymbolConfigProvider_GetColorGradientBrushesCompleted;
        }

        void colorRampsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (colorRampsListBox == null)
                return;
            ListBoxItem item = colorRampsListBox.SelectedItem as ListBoxItem;
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

            SelectedColorBrushStops = brush.GradientStops;

            OnGradientBrushChanged(new GradientBrushChangedEventArgs { GradientBrush = brush });      
        }

        #region GeometryType
        /// <summary>
        /// 
        /// </summary>
        public GeometryType GeometryType
        {
            get { return (GeometryType)GetValue(GeometryTypeProperty); }
            set { SetValue(GeometryTypeProperty, value); }
        }

        /// <summary>
        /// Identifies the GeometryType dependency property.
        /// </summary>
        public static readonly DependencyProperty GeometryTypeProperty =
            DependencyProperty.Register(
                "GeometryType",
                typeof(GeometryType),
                typeof(ColorRampControl),
                new PropertyMetadata(GeometryType.Point));
        #endregion 

        #region ColorBrushes
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<LinearGradientBrush> ColorBrushes
        {
            get { return GetValue(ColorBrushesProperty) as IEnumerable<LinearGradientBrush>; }
            set { SetValue(ColorBrushesProperty, value); }
        }

        /// <summary>
        /// Identifies the ColorBrushes dependency property.
        /// </summary>
        public static readonly DependencyProperty ColorBrushesProperty =
            DependencyProperty.Register(
                "ColorBrushes",
                typeof(IEnumerable<LinearGradientBrush>),
                typeof(ColorRampControl),
                new PropertyMetadata(null));
        #endregion 

        
        #region SelectedColorBrush
        /// <summary>
        /// 
        /// </summary>
        public GradientStopCollection SelectedColorBrushStops
        {
            get { return GetValue(SelectedColorBrushStopsProperty) as GradientStopCollection; }
            set { SetValue(SelectedColorBrushStopsProperty, value); }
        }

        /// <summary>
        /// Identifies the ColorBrushes dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedColorBrushStopsProperty =
            DependencyProperty.Register(
                "SelectedColorBrushStops",
                typeof(GradientStopCollection),
                typeof(ColorRampControl),
                new PropertyMetadata(null));
        #endregion 


        #region ColorRampType


        public ColorRampType ColorRampType
        {
            get { return (ColorRampType)GetValue(ColorRampTypeProperty); }
            set { SetValue(ColorRampTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorRampType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorRampTypeProperty =
            DependencyProperty.Register("ColorRampType", typeof(ColorRampType), typeof(ColorRampControl), 
            new PropertyMetadata(ColorRampType.ClassBreaks, OnColorRampTypePropertyChanged));

        private static void OnColorRampTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorRampControl source = d as ColorRampControl;
            source.buildUI();
        }
        #endregion

        #region SymbolConfigProvider
        /// <summary>
        /// 
        /// </summary>
        public SymbolConfigProvider SymbolConfigProvider
        {
            get { return GetValue(SymbolConfigProviderProperty) as SymbolConfigProvider; }
            set { SetValue(SymbolConfigProviderProperty, value); }
        }

        /// <summary>
        /// Identifies the SymbolConfigProvider dependency property.
        /// </summary>
        public static readonly DependencyProperty SymbolConfigProviderProperty =
            DependencyProperty.Register(
                "SymbolConfigProvider",
                typeof(SymbolConfigProvider),
                typeof(ColorRampControl),
                new PropertyMetadata(null));
        #endregion 

        private bool doesLayerColorRampBrushMatchBrush(LinearGradientBrush brush)
        {
            if (brush == null || brush.GradientStops.Count < 2)
                return false;
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
            //        if (first.SymbolInfo is SimpleFillSymbolInfo && last.SymbolInfo is SimpleFillSymbolInfo)
            //        {
            //            SimpleFillSymbolInfo firstSymbol = first.SymbolInfo as SimpleFillSymbolInfo;
            //            SimpleFillSymbolInfo lastSymbol = last.SymbolInfo as SimpleFillSymbolInfo;
            //            Color firstColor = ColorPickerUtils.FromHex(firstSymbol.FillColor);
            //            Color lastColor = ColorPickerUtils.FromHex(lastSymbol.FillColor);
            //            return firstColor == brush.GradientStops[0].Color && lastColor == brush.GradientStops[brush.GradientStops.Count - 1].Color;
            //        }
            //        else if (first.SymbolInfo is SimpleLineSymbolInfo && last.SymbolInfo is SimpleLineSymbolInfo)
            //        {
            //            SimpleLineSymbolInfo firstSymbol = first.SymbolInfo as SimpleLineSymbolInfo;
            //            SimpleLineSymbolInfo lastSymbol = last.SymbolInfo as SimpleLineSymbolInfo;
            //            Color firstColor = ColorPickerUtils.FromHex(firstSymbol.Color);
            //            Color lastColor = ColorPickerUtils.FromHex(lastSymbol.Color);
            //            return firstColor == brush.GradientStops[0].Color && lastColor == brush.GradientStops[brush.GradientStops.Count - 1].Color;
            //        }
            //    }
            //}
            return false;
        }

        private void setSelectedColorInRamp(Color firstColor, Color lastColor)
        {
            if (colorRampsListBox == null)
                return;

            foreach (ListBoxItem item in colorRampsListBox.Items)
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

        private Rectangle createColorRectangle(LinearGradientBrush fillBrush)
        {            
            Rectangle rect = new Rectangle
            {
                Height = 15,
                Width = double.IsNaN(this.Width) ? 310 : this.Width - 30,
                VerticalAlignment = VerticalAlignment.Stretch,
                Stroke = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(3, 1, 3, 1),
                StrokeThickness = 1,
                Fill = fillBrush
            };
            return rect;
        }

        protected void OnGradientBrushChanged(GradientBrushChangedEventArgs e)
        {
            if (ColorGradientChosen != null)
                ColorGradientChosen(this, e);
        }  

        public event EventHandler<GradientBrushChangedEventArgs> ColorGradientChosen;       
    }

    public class GradientBrushChangedEventArgs : EventArgs
    {
        public LinearGradientBrush GradientBrush { get; set; }
    }    
}
