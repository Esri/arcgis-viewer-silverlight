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
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Threading;
using System.Globalization;
using System.Windows.Data;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "FillRadioButton", Type = typeof(RadioButton))]
    [TemplatePart(Name = "BorderRadioButton", Type = typeof(RadioButton))]
    [TemplatePart(Name = "ColorPickerSection", Type = typeof(ContentControl))]
    [TemplatePart(Name = "FillColorPicker", Type = typeof(ColorPicker))]
    [TemplatePart(Name = "PreDefinedColorRampControl", Type = typeof(PreDefinedColorRampControl))]
    [TemplatePart(Name = "BorderColorPicker", Type = typeof(ColorPicker))]
    [TemplatePart(Name = "FillOpacityPanel", Type = typeof(ContentControl))]
    [TemplatePart(Name = "FillOpacitySlider", Type = typeof(Slider))]
    [TemplatePart(Name = "BorderOpacityPanel", Type = typeof(ContentControl))]
    [TemplatePart(Name = "BorderOpacitySlider", Type = typeof(Slider))]
    [TemplatePart(Name = "BorderWidthPanel", Type = typeof(ContentControl))]
    [TemplatePart(Name = "BorderWidthTextBox", Type = typeof(TextBox))]
    public class FillSymbolConfigControl : Control
    {
        RadioButton FillRadioButton = null;
        RadioButton BorderRadioButton = null;
        ContentControl ColorPickerSection = null;
        ColorPicker FillColorPicker = null;
        PreDefinedColorRampControl PreDefinedColorRampControl = null;
        ColorPicker BorderColorPicker = null;
        ContentControl FillOpacityPanel = null;
        Slider FillOpacitySlider = null;
        ContentControl BorderOpacityPanel = null;
        Slider BorderOpacitySlider = null;
        ContentControl BorderWidthPanel = null;
        TextBox BorderWidthTextBox = null;

        public bool IsConfiguringClassBreaks { get; set; }
        public bool AllowCustomColors { get; set; }

        public FillSymbolConfigControl()
        {
            DefaultStyleKey = typeof(FillSymbolConfigControl);

            _borderChangedTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 500) };
            _borderChangedTimer.Tick += BorderChangedTimer_Tick;
            _fillChangedTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 500) };
            _fillChangedTimer.Tick += FillChangedTimer_Tick;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            FillRadioButton = GetTemplateChild("FillRadioButton") as RadioButton;
            if (FillRadioButton != null)
                FillRadioButton.Checked += new RoutedEventHandler(RadioButton_Checked);

            BorderRadioButton = GetTemplateChild("BorderRadioButton") as RadioButton;
            if(BorderRadioButton != null)
                BorderRadioButton.Checked += new RoutedEventHandler(RadioButton_Checked);

            ColorPickerSection = GetTemplateChild("ColorPickerSection") as ContentControl;

            FillColorPicker = GetTemplateChild("FillColorPicker") as ColorPicker;
            if(FillColorPicker != null)
                FillColorPicker.ColorChanged += new EventHandler(FillColorPicker_ColorChanged);

            PreDefinedColorRampControl = GetTemplateChild("PreDefinedColorRampControl") as PreDefinedColorRampControl;
            if (PreDefinedColorRampControl != null)
                PreDefinedColorRampControl.GradientBrushChanged += new EventHandler<GradientBrushChangedEventArgs>(onGradientBrushChanged);

            BorderColorPicker = GetTemplateChild("BorderColorPicker") as ColorPicker;
            if(BorderColorPicker != null)
                BorderColorPicker.ColorChanged += new EventHandler(BorderColorPicker_ColorChanged);

            FillOpacityPanel = GetTemplateChild("FillOpacityPanel") as ContentControl;

            FillOpacitySlider = GetTemplateChild("FillOpacitySlider") as Slider;
            if (FillOpacitySlider != null)
                FillOpacitySlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(FillSlider_ValueChanged);                

            BorderOpacityPanel = GetTemplateChild("BorderOpacityPanel") as ContentControl;

            BorderOpacitySlider = GetTemplateChild("BorderOpacitySlider") as Slider;
            if(BorderOpacitySlider != null)
                BorderOpacitySlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(BorderSlider_ValueChanged);

            BorderWidthPanel = GetTemplateChild("BorderWidthPanel") as ContentControl;
            BorderWidthTextBox = GetTemplateChild("BorderWidthTextBox") as TextBox;
            if (BorderWidthTextBox != null)
            {
                BorderWidthTextBox.SetBinding(TextBox.TextProperty, new Binding() { 
                    Source = DataContext,
                    Mode = BindingMode.TwoWay,
                    Path = new PropertyPath("BorderThickness"),
                });
            }

            Initialize();
            if (PreDefinedColorRampControl != null)
                PreDefinedColorRampControl.Initialize();
        }        

        internal void initializeFillPicker()
        {
            SimpleFillSymbol simpleFillSymbol = FillColorPicker.DataContext as SimpleFillSymbol;
            if (simpleFillSymbol != null)
            {
                if (FillColorPicker != null)
                {
                    FillColorPicker.Color = (simpleFillSymbol.Fill as SolidColorBrush).Color;
                    FillColorPicker.Hsv = FillColorPicker.Color.ToHsv();
                }
            }

            //FillColorPicker.Size = Math.Min(LayoutRoot.ColumnDefinitions[1].ActualWidth, LayoutRoot.ActualHeight);
        }

        private void BorderColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == Visibility.Collapsed || this.DataContext == null)
                return;

            initializeBorderPicker();
        }

        internal void initializeBorderPicker()
        {
            SimpleFillSymbol simpleFillSymbol = BorderColorPicker.DataContext as SimpleFillSymbol;
            if (simpleFillSymbol != null)
            {
                if (BorderColorPicker != null)
                {
                    BorderColorPicker.Color = (simpleFillSymbol.BorderBrush as SolidColorBrush).Color;
                    BorderColorPicker.Hsv = BorderColorPicker.Color.ToHsv();
                }
            }

            //BorderColorPicker.Size = Math.Min(LayoutRoot.ColumnDefinitions[1].ActualWidth, LayoutRoot.ActualHeight);
        }

        internal void intializeTransparencySliders()
        {            
            SimpleFillSymbol simpleFillSymbol = FillOpacitySlider.DataContext as SimpleFillSymbol;
            if (simpleFillSymbol != null)
            {                
                if (FillOpacitySlider != null)
                    FillOpacitySlider.Value = simpleFillSymbol.Fill.Opacity;
                if (BorderOpacitySlider != null)
                    BorderOpacitySlider.Value = simpleFillSymbol.BorderBrush.Opacity;
            }
        }

        public void Initialize()
        {
            initializeFillPicker();
            initializeBorderPicker();
            if(FillRadioButton != null)
                FillRadioButton.IsChecked = true;

            SimpleFillSymbol simpleFillSymbol = DataContext as SimpleFillSymbol;
            if (simpleFillSymbol != null)
            {
                if(FillOpacitySlider != null)
                    FillOpacitySlider.Value = simpleFillSymbol.Fill.Opacity;
                if(BorderOpacitySlider != null)
                    BorderOpacitySlider.Value = simpleFillSymbol.BorderBrush.Opacity;
                if(BorderWidthTextBox != null)
                    BorderWidthTextBox.Text = simpleFillSymbol.BorderThickness.ToString(CultureInfo.InvariantCulture);
            }

            if (IsConfiguringClassBreaks)
            {
                // TODO:= change to VSM states
                if (PreDefinedColorRampControl != null)
                    PreDefinedColorRampControl.Visibility = AllowCustomColors ? Visibility.Collapsed : Visibility.Visible;
                if (FillColorPicker != null)
                {
                    FillColorPicker.Visibility = AllowCustomColors ? Visibility.Visible : Visibility.Collapsed;
                    FillColorPicker.Margin = new Thickness(20, 0, 0, 0);
                }
                if(BorderColorPicker != null)
                    BorderColorPicker.Margin = new Thickness(20, 0, 0, 0);
            }
            else
            {
                if (PreDefinedColorRampControl != null)
                    PreDefinedColorRampControl.Visibility = Visibility.Collapsed;
                if (FillColorPicker != null)
                {
                    FillColorPicker.Visibility = Visibility.Visible;
                    FillColorPicker.Margin = new Thickness(10, 0, 0, 0);
                }
                if (BorderColorPicker != null)
                    BorderColorPicker.Margin = new Thickness(10, 0, 0, 0);
            }
            if(FillColorPicker != null)
                FillColorPicker.Refresh();
            if(BorderColorPicker != null)
                BorderColorPicker.Refresh();
        }

        internal void Refresh()
        {
            Initialize();
        }

        private void FillColorPicker_ColorChanged(object sender, EventArgs e)
        {
            ColorPicker picker = (ColorPicker)sender;
            SimpleFillSymbol Symbol = (SimpleFillSymbol)picker.DataContext;
            //Symbol.FillHsv = picker.Hsv;

            Color color = picker.Color;
            if(FillOpacitySlider != null)
                color.A = Convert.ToByte(255 * FillOpacitySlider.Value);
            Symbol.Fill = new SolidColorBrush(color);
        }

        private void BorderColorPicker_ColorChanged(object sender, EventArgs e)
        {
            ColorPicker picker = (ColorPicker)sender;
            SimpleFillSymbol Symbol = (SimpleFillSymbol)picker.DataContext;
            //Symbol.BorderHsv = picker.Hsv;

            Color color = picker.Color;
            if(BorderOpacitySlider != null)
                color.A = Convert.ToByte(255 * BorderOpacitySlider.Value);
            Symbol.BorderBrush = new SolidColorBrush(color);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {            
            bool showFill = sender == FillRadioButton;
            if (FillColorPicker != null)
            {
                if(ColorPickerSection != null)
                    ColorPickerSection.Visibility = showFill ? Visibility.Visible : Visibility.Collapsed;
                if(FillOpacityPanel != null)
                    FillOpacityPanel.Visibility = showFill ? Visibility.Visible : Visibility.Collapsed;
                if(BorderColorPicker != null)
                    BorderColorPicker.Visibility = !showFill ? Visibility.Visible : Visibility.Collapsed;
                if (BorderOpacityPanel != null)
                    BorderOpacityPanel.Visibility = !showFill ? Visibility.Visible : Visibility.Collapsed;
                if(BorderWidthPanel != null)
                    BorderWidthPanel.Opacity = !showFill ? 1 : 0;
                if (!showFill)
                    initializeBorderPicker();
            }
        }

        private DispatcherTimer _borderChangedTimer;
        private void BorderSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DataContext != null)
            {
                if (_borderChangedTimer.IsEnabled)
                    _borderChangedTimer.Stop();

                _borderChangedTimer.Start();
            }
        }

        void BorderChangedTimer_Tick(object sender, EventArgs e)
        {
            _borderChangedTimer.Stop();
            if (FillOpacitySlider != null)
            {
                SimpleFillSymbol simpleFillSymbol = DataContext as SimpleFillSymbol;
                if(simpleFillSymbol != null)
                    simpleFillSymbol.BorderBrush.Opacity = BorderOpacitySlider.Value;
            }
        }


        private DispatcherTimer _fillChangedTimer;
        private void FillSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DataContext != null)
            {
                if (_fillChangedTimer.IsEnabled)
                    _fillChangedTimer.Stop();

                _fillChangedTimer.Start();
            }
        }

        void FillChangedTimer_Tick(object sender, EventArgs e)
        {
            _fillChangedTimer.Stop();
            if (FillOpacitySlider != null)
            {
                SimpleFillSymbol simpleFillSymbol = DataContext as SimpleFillSymbol;
                if(simpleFillSymbol != null)
                    simpleFillSymbol.Fill.Opacity = FillOpacitySlider.Value;
            }
        }        

        internal void SetAllowCustomFillSymbolColors(bool isCustom)
        {
            AllowCustomColors = isCustom;
            if (PreDefinedColorRampControl != null)
                PreDefinedColorRampControl.Visibility = isCustom ? Visibility.Collapsed : Visibility.Visible;
            if (FillColorPicker != null)
                FillColorPicker.Visibility = isCustom ? Visibility.Visible : Visibility.Collapsed;
        }

        private void onGradientBrushChanged(object sender, GradientBrushChangedEventArgs e)
        {
            OnGradientBrushChanged(e);
        }

        public event EventHandler<GradientBrushChangedEventArgs> GradientBrushChanged;
        protected void OnGradientBrushChanged(GradientBrushChangedEventArgs e)
        {
            if (GradientBrushChanged != null)
                GradientBrushChanged(this, e);
        }

        //private bool _init = false;
        //private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (_init)
        //        return;

        //    LayoutRoot.Width = this.Width;
        //    LayoutRoot.Height = this.Height;

        //    // To give a unique groupname across the 2 instances of the UserControl, we use the IsConfiguringClassBreaks property
        //    // which has a different value for each
        //    cbxBorder.GroupName = cbxFill.GroupName = this.IsConfiguringClassBreaks ? "_1" : "1";

        //    double width = LayoutRoot.ColumnDefinitions[0].ActualWidth;
        //    if (!double.IsNaN(width) && width > 0)
        //    {
        //        FillOpacitySlider.Width = width;
        //        BorderOpacitySlider.Width = width;
        //        _init = true;
        //    }
        //}
    }
}
