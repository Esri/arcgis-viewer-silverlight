/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.Symbols;
using FSS = ESRI.ArcGIS.Client.FeatureService.Symbols;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class SymbolConfigControl : Control
    {
        private const string PART_SYMBOLPICKER = "SymbolPicker";
        private const string PART_POINTTHICKNESS = "PointThickness";
        private const string PART_SYMBOLBORDERTHICKNESS = "SymbolBorderThickness";
        private const string PART_OUTLINECOLORSELECTOR = "OutlineColorSelector";
        private const string PART_FILLCOLORSELECTOR = "FillColorSelector";
        private const string PART_OPACITYSLIDER = "OpacitySlider";
        private const string PART_BORDEROPACITYSLIDER = "BorderOpacitySlider";

        internal SymbolPicker SymbolPicker;
        internal ThicknessUpDownControl PointThickness;
        internal ThicknessUpDownControl SymbolBorderThickness;
        internal SolidColorBrushSelector OutlineColorSelector;
        internal SolidColorBrushSelector FillColorSelector;
        internal Slider OpacitySlider;
        internal Slider BorderOpacitySlider;

        public SymbolConfigControl()
        {
            DefaultStyleKey = typeof(SymbolConfigControl);
        }

        public override void OnApplyTemplate()
        {   
            if (PointThickness != null)
                PointThickness.ThicknessValueChanged -= PointThickness_ThicknessValueChanged;

            if (SymbolBorderThickness != null)
                SymbolBorderThickness.ThicknessValueChanged -= SymbolBorderThickness_ThicknessValueChanged;

            if (OutlineColorSelector != null)
                OutlineColorSelector.ColorPicked -= OutlineColorSelector_ColorPicked;

            if (SymbolPicker != null)
            {
                SymbolPicker.CollapseDropDown();
                SymbolPicker.SymbolSelected -= SymbolPicker_SymbolSelected;
            }

            if(OpacitySlider != null)
                OpacitySlider.ValueChanged -= OpacitySlider_ValueChanged;

            if (BorderOpacitySlider != null)
                BorderOpacitySlider.ValueChanged -= BorderOpacitySlider_ValueChanged;

            base.OnApplyTemplate();

            SymbolPicker = GetTemplateChild(PART_SYMBOLPICKER) as SymbolPicker;            
            if(SymbolPicker != null)
                SymbolPicker.SymbolSelected += SymbolPicker_SymbolSelected;

            PointThickness = GetTemplateChild(PART_POINTTHICKNESS) as ThicknessUpDownControl;
            if(PointThickness != null)
                PointThickness.ThicknessValueChanged += PointThickness_ThicknessValueChanged;

            SymbolBorderThickness = GetTemplateChild(PART_SYMBOLBORDERTHICKNESS) as ThicknessUpDownControl;
            if (SymbolBorderThickness != null)
                SymbolBorderThickness.ThicknessValueChanged += SymbolBorderThickness_ThicknessValueChanged;

            OutlineColorSelector = GetTemplateChild(PART_OUTLINECOLORSELECTOR) as SolidColorBrushSelector;
            if(OutlineColorSelector != null)
                OutlineColorSelector.ColorPicked += OutlineColorSelector_ColorPicked;

            FillColorSelector = GetTemplateChild(PART_FILLCOLORSELECTOR) as SolidColorBrushSelector;
            if(FillColorSelector != null)
                FillColorSelector.ColorPicked += FillColorSelector_ColorPicked;

            OpacitySlider = GetTemplateChild(PART_OPACITYSLIDER) as Slider;
            if (OpacitySlider != null)
                OpacitySlider.ValueChanged += OpacitySlider_ValueChanged;

            BorderOpacitySlider = GetTemplateChild(PART_BORDEROPACITYSLIDER) as Slider;
            if(BorderOpacitySlider != null)
                BorderOpacitySlider.ValueChanged += BorderOpacitySlider_ValueChanged;

            bindUIToSymbol();

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }

        internal event EventHandler InitCompleted;

        void BorderOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol markerSymbol = Symbol as ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol;
            ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol simpleMarkerSymbol = Symbol as ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol;
            FSS.SimpleMarkerSymbol sms = Symbol as FSS.SimpleMarkerSymbol;
            FillSymbol fillSymbol = Symbol as FillSymbol;            
            if (fillSymbol != null)
            {
                if (fillSymbol.BorderBrush != null)
                {
                    fillSymbol.BorderBrush.SetOpacity(e.NewValue);
                    onCurrentSymbolChanged();
                }
            }
            else if (sms != null)
            {
                if (sms.OutlineColor != null)
                {
                    sms.OutlineColor.SetOpacity(e.NewValue);
                    onCurrentSymbolChanged();
                }
            }
        }

        void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Symbol is ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol)
            {
                ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol symbol = (ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol) Symbol;
                symbol.Opacity = e.NewValue;
                onCurrentSymbolChanged();
            }
            else if (Symbol is FSS.SimpleMarkerSymbol)
            {
                FSS.SimpleMarkerSymbol symbol = (FSS.SimpleMarkerSymbol) Symbol;
                if (symbol.Color != null)
                {
                    symbol.Color.SetOpacity(e.NewValue);
                    onCurrentSymbolChanged();
                }
            }
            else if (Symbol is FSS.SimpleLineSymbol)
            {
                FSS.SimpleLineSymbol symbol = (FSS.SimpleLineSymbol)Symbol;
                if (symbol.Color != null)
                {
                    symbol.Color.SetOpacity(e.NewValue);
                    onCurrentSymbolChanged();
                }
            }
            else if (Symbol is FSS.SimpleFillSymbol)
            {
                FSS.SimpleFillSymbol symbol = (FSS.SimpleFillSymbol)Symbol;
                if (symbol.Color != null)
                {
                    symbol.Color = Color.FromArgb(Convert.ToByte(255 * e.NewValue), symbol.Color.R, symbol.Color.G, symbol.Color.B);
                    onCurrentSymbolChanged();
                }
            }
            else if (Symbol is SimpleMarkerSymbol)
            {
                SimpleMarkerSymbol symbol = (SimpleMarkerSymbol)Symbol;
                if (symbol.Color != null)
                {
                    symbol.Color.SetOpacity(e.NewValue);
                    onCurrentSymbolChanged();
                }
            }
            else if (Symbol is LineSymbol)
            {
                LineSymbol symbol = (LineSymbol)Symbol;
                if (symbol.Color != null)
                {
                    symbol.Color.SetOpacity(e.NewValue);
                    onCurrentSymbolChanged();
                }
            }
            else if (Symbol is FillSymbol)
            {
                FillSymbol symbol = (FillSymbol)Symbol;
                if (symbol.Fill != null)
                {
                    symbol.Fill.SetOpacity(e.NewValue);
                    onCurrentSymbolChanged();
                }
            }
        }

        private void onCurrentSymbolChanged(bool layerSymbolUpdated = false)
        {
            OnSymolChanged(new SymbolSelectedEventArgs() { Symbol = Symbol, LayerSymbolUpdated = layerSymbolUpdated });
        }

        public void CollapseSymbolPickerDropDown()
        {
            if (SymbolPicker != null)
                SymbolPicker.CollapseDropDown();
        }

        void PointThickness_ThicknessValueChanged(object sender, EventArgs e)
        {
            ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol markerSymbol = Symbol as ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol;
            if (markerSymbol != null)
            {
                markerSymbol.Size = PointThickness.TargetThickness.Bottom;
                onCurrentSymbolChanged(true);
            }
            else
            {
                ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol simpleMarkerSymbol = Symbol as ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol;
                if (simpleMarkerSymbol != null)
                {
                    simpleMarkerSymbol.Size = PointThickness.TargetThickness.Bottom;
                    onCurrentSymbolChanged(true);
                }
                else
                {
                    ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol sms = Symbol as ESRI.ArcGIS.Client.FeatureService.Symbols.SimpleMarkerSymbol;
                    if (sms != null)
                    {
                        sms.Size = PointThickness.TargetThickness.Bottom;
                        onCurrentSymbolChanged(true);
                    }
                }
            }
        }

        void SymbolPicker_SymbolSelected(object sender, SymbolSelectedEventArgs e)
        {
            Symbol = e.Symbol;
            OnSymolChanged(e);
            bindUIToSymbol();
        }

        void FillColorSelector_ColorPicked(object sender, ColorChosenEventArgs e)
        {            
            FillSymbol fillSymbol = Symbol as FillSymbol;
            if (fillSymbol != null)
            {
                #region FSS.SFS
                if (fillSymbol is FSS.SimpleFillSymbol)
                {
                    ((FSS.SimpleFillSymbol)fillSymbol).Color = e.Color;
                     onCurrentSymbolChanged();
                }
                #endregion
                #region FSS.PFS
                else if (fillSymbol is FSS.PictureFillSymbol)
                {
                    FSS.PictureFillSymbol pfs = fillSymbol as FSS.PictureFillSymbol;
                     SolidColorBrush brush = pfs.Color as SolidColorBrush;
                     if (brush != null)
                     {
                         brush.Color = e.Color;
                         onCurrentSymbolChanged();
                     }
                }
                #endregion
                #region Default
                else
                {
                    SolidColorBrush brush = fillSymbol.Fill as SolidColorBrush;
                    if (brush != null)
                    {
                        brush.Color = e.Color;
                        onCurrentSymbolChanged();
                    }
                }
                #endregion
            }
            else
            {
                #region Mapping Core Marker
                ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol markerSymbol = Symbol as ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol;
                if (markerSymbol != null)
                {
                    SolidColorBrush sb = markerSymbol.Color as SolidColorBrush;
                    if (sb != null)
                    {
                        sb.Color = e.Color;
                        onCurrentSymbolChanged();
                    }
                }
                #endregion 
                else
                {
                    #region Client SMS
                    ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol simpleMarkerSymbol = Symbol as ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol;
                    if (simpleMarkerSymbol != null)
                    {
                        SolidColorBrush sb = simpleMarkerSymbol.Color as SolidColorBrush;
                        if (sb != null)
                        {
                            sb.Color = e.Color;
                            onCurrentSymbolChanged();
                        }
                    }
                    #endregion
                    #region FSS.SMS
                    else
                    {
                        FSS.SimpleMarkerSymbol sms = Symbol as FSS.SimpleMarkerSymbol;
                        if (sms != null)
                        {
                            SolidColorBrush sb = sms.Color as SolidColorBrush;
                            if (sb != null)
                            {
                                sb.Color = e.Color;
                                onCurrentSymbolChanged();
                            }
                        }
                    }
                    #endregion
                }
            }
        }

        void OutlineColorSelector_ColorPicked(object sender, ColorChosenEventArgs e)
        {
            FillSymbol fillSymbol = Symbol as FillSymbol;
            LineSymbol lineSymbol = Symbol as LineSymbol;
            FSS.SimpleMarkerSymbol sms = Symbol as FSS.SimpleMarkerSymbol;
            if (sms != null)
            {
                SolidColorBrush brush = sms.OutlineColor as SolidColorBrush;
                if (brush != null)
                {
                    brush.Color = e.Color;
                    onCurrentSymbolChanged();
                }
            }
            else if (fillSymbol != null)
            {
                SolidColorBrush brush = fillSymbol.BorderBrush as SolidColorBrush;
                if (brush != null)
                {
                    brush.Color = e.Color;
                    onCurrentSymbolChanged();
                }
            }
            else if (lineSymbol != null)
            {
                SolidColorBrush brush = lineSymbol.Color as SolidColorBrush;
                if (brush != null)
                {
                    brush.Color = e.Color;
                    onCurrentSymbolChanged();
                }
            }
        }

        void SymbolBorderThickness_ThicknessValueChanged(object sender, EventArgs e)
        {
            if (SymbolBorderThickness == null)
                return;

            double newSize = SymbolBorderThickness.TargetThickness.Bottom;
            if (newSize < 0)
                return;

            FillSymbol fillSymbol = Symbol as FillSymbol;
            FSS.SimpleMarkerSymbol sms = Symbol as FSS.SimpleMarkerSymbol;
            if (sms != null)
            {
                sms.OutlineThickness = newSize;
                onCurrentSymbolChanged();
            }
            else if (fillSymbol != null)
            {
                fillSymbol.BorderThickness = newSize;
                onCurrentSymbolChanged();
            }
            else
            {
                LineSymbol lineSymbol = Symbol as LineSymbol;
                if (lineSymbol != null)
                {
                    lineSymbol.Width = newSize;
                    onCurrentSymbolChanged();
                }
            }
        }

        #region Symbol
        /// <summary>
        /// 
        /// </summary>
        public Symbol Symbol
        {
            get { return GetValue(SymbolProperty) as Symbol; }
            set { SetValue(SymbolProperty, value); }
        }

        /// <summary>
        /// Identifies the Symbol dependency property.
        /// </summary>
        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register(
                "Symbol",
                typeof(Symbol),
                typeof(SymbolConfigControl),
                new PropertyMetadata(null, OnSymbolPropertyChanged));

        /// <summary>
        /// SymbolProperty property changed handler.
        /// </summary>
        /// <param name="d">SymbolConfigRibbonControl that changed its Symbol.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnSymbolPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SymbolConfigControl source = d as SymbolConfigControl;
            source.bindUIToSymbol();
        }
        #endregion

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
                typeof(SymbolConfigControl),
                new PropertyMetadata(GeometryType.Point));
        #endregion 

        #region SymbolPickerVisibility
        /// <summary>
        /// 
        /// </summary>
        public Visibility SymbolPickerVisibility
        {
            get { return (Visibility)GetValue(SymbolPickerVisibilityProperty); }
            set { SetValue(SymbolPickerVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the SymbolPickerVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty SymbolPickerVisibilityProperty =
            DependencyProperty.Register(
                "SymbolPickerVisibility",
                typeof(Visibility),
                typeof(SymbolConfigControl),
                new PropertyMetadata(Visibility.Visible));
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
                typeof(SymbolConfigControl),
                new PropertyMetadata(null));
        #endregion 
        
        #region ThemeColors
        /// <summary>
        /// 
        /// </summary>
        public Collection<Color> ThemeColors
        {
            get { return GetValue(ThemeColorsProperty) as Collection<Color>; }
            set { SetValue(ThemeColorsProperty, value); }
        }

        /// <summary>
        /// Identifies the ThemeColors dependency property.
        /// </summary>
        public static readonly DependencyProperty ThemeColorsProperty =
            DependencyProperty.Register(
                "ThemeColors",
                typeof(Collection<Color>),
                typeof(SymbolConfigControl),
                new PropertyMetadata(null));
        #endregion

        void bindUIToSymbol()
        {
            if (SymbolPicker != null)
                SymbolPicker.Symbol = Symbol;                

            ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol markerSymbol = Symbol as ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol;

            FSS.SimpleMarkerSymbol fsSimpleMarkerSymbol = Symbol as FSS.SimpleMarkerSymbol;
            FSS.SimpleLineSymbol fsSimpleLineSymbol = Symbol as FSS.SimpleLineSymbol;
            FSS.SimpleFillSymbol fsSimpleFillSymbol = Symbol as FSS.SimpleFillSymbol;
            
            SimpleMarkerSymbol simpleMarkerSymbol = Symbol as SimpleMarkerSymbol; 
            
            FillSymbol fillSymbol = Symbol as FillSymbol;
            LineSymbol lineSymbol = Symbol as LineSymbol;

            if (Symbol != null)
            {
                #region Size
                if (PointThickness != null)
                {   
                    // we support Size property on SimpleMarkerSymbol and ImageFillSymbol
                    ImageFillSymbol imagefillSymbol = Symbol as ImageFillSymbol;
                    if (imagefillSymbol != null)
                    {
                        PointThickness.IsEnabled = true;
                        PointThickness.TargetThickness = new Thickness(imagefillSymbol.Size);
                    }
                    else
                    {
                        if (markerSymbol != null)
                        {
                            PointThickness.IsEnabled = true;
                            PointThickness.TargetThickness = new Thickness(markerSymbol.Size);
                        }
                        else
                        {
                            if (simpleMarkerSymbol != null)
                            {
                                PointThickness.IsEnabled = true;
                                PointThickness.TargetThickness = new Thickness(simpleMarkerSymbol.Size);
                            }
                            else if (fsSimpleMarkerSymbol != null)
                            {
                                PointThickness.IsEnabled = true;
                                PointThickness.TargetThickness = new Thickness(fsSimpleMarkerSymbol.Size);
                            }
                            else
                            {
                                PointThickness.IsEnabled = false;
                            }
                        }
                    }
                }
                #endregion

                #region Border Thickness
                if (SymbolBorderThickness != null)
                {
                    if (fillSymbol != null)
                    {
                        SymbolBorderThickness.IsEnabled = true;
                        SymbolBorderThickness.TargetThickness = new Thickness(fillSymbol.BorderThickness);
                    }
                    else if (lineSymbol != null)
                    {
                        SymbolBorderThickness.IsEnabled = true;
                        SymbolBorderThickness.TargetThickness = new Thickness(lineSymbol.Width);
                    }
                    else if (fsSimpleMarkerSymbol != null)
                    {
                        if (fsSimpleMarkerSymbol.OutlineColor != null)
                        {
                            SymbolBorderThickness.IsEnabled = true;
                            SymbolBorderThickness.TargetThickness = new Thickness(
                                double.IsNaN(fsSimpleMarkerSymbol.OutlineThickness) ? 0 : fsSimpleMarkerSymbol.OutlineThickness);
                        }
                        else
                            SymbolBorderThickness.IsEnabled = false;
                    }
                    else
                    {
                        SymbolBorderThickness.IsEnabled = false;
                    }
                    
                }
                #endregion

                #region BorderColor
                if (OutlineColorSelector != null)
                {
                    if (fsSimpleMarkerSymbol != null)
                    {
                        SolidColorBrush sb = fsSimpleMarkerSymbol.OutlineColor as SolidColorBrush;
                        if (sb != null)
                        {
                            OutlineColorSelector.IsEnabled = true;
                            OutlineColorSelector.ColorBrush = sb;
                        }
                        else
                            OutlineColorSelector.IsEnabled = false;
                    }
                    else if (fillSymbol != null)
                    {
                        SolidColorBrush sb = fillSymbol.BorderBrush as SolidColorBrush;
                        if (sb != null)
                        {
                            OutlineColorSelector.IsEnabled = true;
                            OutlineColorSelector.ColorBrush = sb;
                        }
                        else
                            OutlineColorSelector.IsEnabled = false;
                    }
                    else if (lineSymbol != null)
                    {
                        SolidColorBrush sb = lineSymbol.Color as SolidColorBrush;
                        if (sb != null)
                        {
                            OutlineColorSelector.IsEnabled = true;
                            OutlineColorSelector.ColorBrush = sb;
                        }
                        else
                            OutlineColorSelector.IsEnabled = false;
                    }
                    else
                    {
                        OutlineColorSelector.IsEnabled = false;
                    }
                    
                }
                #endregion

                #region Fill Color
                if (FillColorSelector != null)
                {
                    if (fillSymbol != null)
                    {
                        SolidColorBrush sb = fillSymbol.Fill as SolidColorBrush;
                        if (sb != null)
                        {
                            FillColorSelector.IsEnabled = true;
                            FillColorSelector.ColorBrush = sb;
                        }
                        else
                            FillColorSelector.IsEnabled = false;
                    }
                    else
                    {
                        if (markerSymbol != null)
                        {
                            SolidColorBrush sb = markerSymbol.Color as SolidColorBrush;
                            if (sb != null)
                            {
                                FillColorSelector.IsEnabled = true;
                                FillColorSelector.ColorBrush = sb;
                            }
                            else
                                FillColorSelector.IsEnabled = false;
                        }
                        else if (fsSimpleMarkerSymbol != null)
                        {
                            SolidColorBrush sb = fsSimpleMarkerSymbol.Color as SolidColorBrush;
                            if (sb != null)
                            {
                                FillColorSelector.IsEnabled = true;
                                FillColorSelector.ColorBrush = sb;
                            }
                            else
                                FillColorSelector.IsEnabled = false;
                        }
                        else if (simpleMarkerSymbol != null)
                        {
                            SolidColorBrush sb = simpleMarkerSymbol.Color as SolidColorBrush;
                            if (sb != null)
                            {
                                FillColorSelector.IsEnabled = true;
                                FillColorSelector.ColorBrush = sb;
                            }
                            else
                                FillColorSelector.IsEnabled = false;
                        }
                        else
                        {
                            FillColorSelector.IsEnabled = false;
                        }
                    }
                }
                #endregion
                
                #region Opacity
                if (OpacitySlider != null)
                {
                    if (markerSymbol != null)
                    {
                        OpacitySlider.Value = markerSymbol.Opacity;
                    }
                    else if (simpleMarkerSymbol != null)
                    {
                        if (simpleMarkerSymbol.Color != null)
                            OpacitySlider.Value = simpleMarkerSymbol.Color.GetOpacity();
                    }
                    else if (fsSimpleMarkerSymbol != null)
                    {
                        if (fsSimpleMarkerSymbol.Color != null)
                            OpacitySlider.Value = fsSimpleMarkerSymbol.Color.GetOpacity();
                    }
                    else if (fsSimpleLineSymbol != null)
                    {
                        if (fsSimpleLineSymbol.Color != null)
                            OpacitySlider.Value = fsSimpleLineSymbol.Color.GetOpacity();
                    }
                    else if (fsSimpleFillSymbol != null)
                    {
                        if (fsSimpleFillSymbol.Color != null)
                            OpacitySlider.Value = fsSimpleFillSymbol.Color.A / 255d;
                    }
                    else if (fillSymbol != null)
                    {
                        if (fillSymbol.Fill != null)
                            OpacitySlider.Value = fillSymbol.Fill.GetOpacity();
                    }
                    else if (lineSymbol != null)
                    {
                        if (lineSymbol.Color != null)
                            OpacitySlider.Value = lineSymbol.Color.GetOpacity();
                    }
                }
                #endregion

                #region Border Opacity
                if (BorderOpacitySlider != null)
                {
                    if (fillSymbol != null)
                    {
                        if (fillSymbol.BorderBrush != null)
                        {
                            BorderOpacitySlider.Visibility = System.Windows.Visibility.Visible;
                            BorderOpacitySlider.Value = fillSymbol.BorderBrush.GetOpacity();
                        }
                        else
                            BorderOpacitySlider.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else if (fsSimpleMarkerSymbol != null && fsSimpleMarkerSymbol.OutlineColor != null)
                    {
                        BorderOpacitySlider.Visibility = System.Windows.Visibility.Visible;
                        BorderOpacitySlider.Value = fsSimpleMarkerSymbol.OutlineColor.GetOpacity();
                    }
                    else
                        BorderOpacitySlider.Visibility = System.Windows.Visibility.Collapsed;
                }
                #endregion
            }
        }

        public void RepositionPopups()
        {
            if (OutlineColorSelector != null)
                OutlineColorSelector.RepositionPopup();

            if (FillColorSelector != null)
                FillColorSelector.RepositionPopup();
        }

        public void CloseAllPopups()
        {
            if (OutlineColorSelector != null)
                OutlineColorSelector.IsContentPopupOpen = false;

            if (FillColorSelector != null)
                FillColorSelector.IsContentPopupOpen = false;
        }

        protected virtual void OnSymolChanged(SymbolSelectedEventArgs args)
        {
            if (SymbolModified != null)
                SymbolModified(this, args);
        }

        public event EventHandler<SymbolSelectedEventArgs> SymbolModified;
    }
}
