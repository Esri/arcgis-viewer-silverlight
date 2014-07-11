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

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "ColorPicker", Type = typeof(ColorPicker))]
    [TemplatePart(Name = "PreDefinedColorRampControl", Type = typeof(PreDefinedColorRampControl))]
    [TemplatePart(Name = "OpacitySlider", Type = typeof(Slider))]
    [TemplatePart(Name = "WidthTextBox", Type = typeof(TextBox))]
    public class LineSymbolConfigControl : Control
    {
        private ColorPicker ColorPicker = null;
        private PreDefinedColorRampControl PreDefinedColorRampControl = null;
        private Slider OpacitySlider = null;
        private TextBox WidthTextBox = null;

        public bool IsConfiguringClassBreaks { get; set; }
        public bool AllowCustomColors { get; set; }

        public LineSymbolConfigControl()
        {
            DefaultStyleKey = typeof(LineSymbolConfigControl);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ColorPicker = GetTemplateChild("ColorPicker") as ColorPicker;
            PreDefinedColorRampControl = GetTemplateChild("PreDefinedColorRampControl") as PreDefinedColorRampControl;
            OpacitySlider = GetTemplateChild("OpacitySlider") as Slider;
            WidthTextBox = GetTemplateChild("WidthTextBox") as TextBox;

            attachEventsAndBindings();

            Initialize();
        }

        private void attachEventsAndBindings()
        {
            if (WidthTextBox != null)
            {
                WidthTextBox.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding() { 
                     Source = DataContext,
                     Mode = System.Windows.Data.BindingMode.TwoWay,
                     Path = new PropertyPath("Width"),
                });
            }

            if (OpacitySlider != null)
            {
                OpacitySlider.SetBinding(Slider.ValueProperty, new System.Windows.Data.Binding() { 
                    Source = DataContext,                    
                    Path = new PropertyPath("Opacity"),
                });
                OpacitySlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(OpacitySlider_ValueChanged);
            }

            if (ColorPicker != null)
                ColorPicker.ColorChanged += new EventHandler(ColorPicker_ColorChanged);

            if(PreDefinedColorRampControl != null)
                PreDefinedColorRampControl.GradientBrushChanged += new EventHandler<GradientBrushChangedEventArgs>(PreDefinedColorRampControl_GradientBrushChanged);
        }

        void PreDefinedColorRampControl_GradientBrushChanged(object sender, GradientBrushChangedEventArgs e)
        {
            OnGradientBrushChanged(e);
        }                       

        //private void ColorPicker_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (this.Visibility == Visibility.Collapsed || this.DataContext == null)
        //        return;

        //    initializePicker();
        //}

        internal void Initialize()
        {
            initializePicker();
            intializeTransparencySlider();

            if (PreDefinedColorRampControl != null)
            {
                if (IsConfiguringClassBreaks)
                {
                    PreDefinedColorRampControl.Visibility = AllowCustomColors ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    PreDefinedColorRampControl.Visibility = Visibility.Collapsed;
                }
            }

            if (ColorPicker != null)
            {
                if (IsConfiguringClassBreaks)
                {
                    ColorPicker.Visibility = AllowCustomColors ? Visibility.Visible : Visibility.Collapsed;
                    ColorPicker.Margin = new Thickness(20, 0, 0, 0);
                }
                else
                {
                    ColorPicker.Visibility = Visibility.Visible;
                    ColorPicker.Margin = new Thickness(10, 0, 0, 0);
                }
                ColorPicker.Refresh();
            }            
        }

        internal void SetAllowCustomFillSymbolColors(bool isCustom)
        {
            AllowCustomColors = isCustom;
            if (PreDefinedColorRampControl != null)
                PreDefinedColorRampControl.Visibility = isCustom ? Visibility.Collapsed : Visibility.Visible;
            if (ColorPicker != null)
                ColorPicker.Visibility = isCustom ? Visibility.Visible : Visibility.Collapsed;
        }

        internal void initializePicker()
        {
            if (ColorPicker != null)
            {
                SimpleLineSymbol simpleLineSymbol = ColorPicker.DataContext as SimpleLineSymbol;
                if (simpleLineSymbol != null)
                {
                    ColorPicker.Color = (simpleLineSymbol.Color as SolidColorBrush).Color;
                    ColorPicker.Hsv = ColorPicker.Color.ToHsv();
                }
            }

            //ColorPicker.Size = Math.Min(LayoutRoot.ColumnDefinitions[1].ActualWidth, LayoutRoot.ActualHeight);
        }

        private void intializeTransparencySlider()
        {
            if (OpacitySlider != null)
            {
                SimpleLineSymbol simpleLineSymbol = OpacitySlider.DataContext as SimpleLineSymbol;
                if (simpleLineSymbol != null)
                    OpacitySlider.Value = simpleLineSymbol.Color.Opacity;
            }
        }

        private void ColorPicker_ColorChanged(object sender, EventArgs e)
        {
            ColorPicker picker = (ColorPicker)sender;
            SimpleLineSymbol simpleLineSymbol = picker.DataContext as SimpleLineSymbol;
            if (simpleLineSymbol != null)
            {
                //Symbol.Hsv = picker.Hsv;

                Color color = picker.Color;
                color.A = Convert.ToByte(255 * OpacitySlider.Value);
                simpleLineSymbol.Color = new SolidColorBrush(color);
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SimpleLineSymbol simpleLineSymbol = DataContext as SimpleLineSymbol;
            if (simpleLineSymbol != null)
                simpleLineSymbol.Color.Opacity = OpacitySlider.Value;
        }

        public event EventHandler<GradientBrushChangedEventArgs> GradientBrushChanged;
        protected void OnGradientBrushChanged(GradientBrushChangedEventArgs e)
        {
            if (GradientBrushChanged != null)
                GradientBrushChanged(this, e);
        }
    }
}
