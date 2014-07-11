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
    public class BackgroundColorPicker : Control
    {

        public BackgroundColorPicker()
        {
            DefaultStyleKey = typeof(BackgroundColorPicker);
        }

        ColorPalette Palette;
        Rectangle Start, End;
        public override void OnApplyTemplate()
        {
            initializeBackgroundStart();
            initializeBackgroundEnd();
            initializeThemeColors();

             if (Palette != null)
                Palette.ColorPicked -= Palette_ColorPicked;
             if (Start != null)
                 Start.MouseLeftButtonUp -= Start_MouseLeftButtonUp;
             if (End != null)
                 End.MouseLeftButtonUp -= End_MouseLeftButtonUp;

            base.OnApplyTemplate();
            Palette = GetTemplateChild("Palette") as ColorPalette;
            if (Palette != null)
            {
                setupPalette();
                Palette.ColorPicked += new EventHandler<ColorChosenEventArgs>(Palette_ColorPicked);
                ApplyThemeColors();
            }

            Start = GetTemplateChild("Start") as Rectangle;
            if (Start != null)
                Start.MouseLeftButtonUp += Start_MouseLeftButtonUp;
            End = GetTemplateChild("End") as Rectangle;
            if (End != null)
                End.MouseLeftButtonUp += End_MouseLeftButtonUp;
        }

        void Start_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsStartSelected = true;
        }

        void End_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsStartSelected = false;
        }

        void Palette_ColorPicked(object sender, ColorChosenEventArgs e)
        { 
            if (IsSolid)
            {
                BackgroundStart = new SolidColorBrush() { Color = e.Color };
                BackgroundEnd = new SolidColorBrush() { Color = e.Color };
            }
            else
            {
                if (IsStartSelected)
                    BackgroundStart = new SolidColorBrush() { Color = e.Color };
                else
                    BackgroundEnd = new SolidColorBrush() { Color = e.Color };
            }
        }

        void ApplyThemeColors()
        {
            if (ThemeColors != null && Palette != null)
            {
                foreach (Color color in ThemeColors)
                {
                    Palette.ThemeColors.Add(color);
                }
            }
        }

        private void initializeBackgroundStart()
        {
            if (BackgroundStart == null)
            {
                if (View.Instance != null)
                {
                    System.Windows.Data.Binding binding = new System.Windows.Data.Binding();
                    binding.Source = View.Instance.ApplicationColorSet;
                    binding.Path = new PropertyPath("BackgroundStartGradientColor");
                    binding.Mode = System.Windows.Data.BindingMode.TwoWay;
                    binding.Converter = new ColorToSolidColorBrushConverter();
                    SetBinding(BackgroundStartProperty, binding);
                }
                else
                    BackgroundStart = new SolidColorBrush(Colors.DarkGray);
            }
        }

        private void initializeBackgroundEnd()
        {
            if (BackgroundEnd == null)
            {
                if (View.Instance != null)
                {
                    System.Windows.Data.Binding binding = new System.Windows.Data.Binding();
                    binding.Source = View.Instance.ApplicationColorSet;
                    binding.Path = new PropertyPath("BackgroundEndGradientColor");
                    binding.Mode = System.Windows.Data.BindingMode.TwoWay;
                    binding.Converter = new ColorToSolidColorBrushConverter();
                    SetBinding(BackgroundEndProperty, binding);
                }
                else
                    BackgroundEnd = new SolidColorBrush(Colors.LightGray);
            }
        }

        private void initializeThemeColors()
        {
            if (View.Instance != null && ThemeColors == null)
            {
                System.Windows.Data.Binding binding = new System.Windows.Data.Binding();
                binding.Source = View.Instance.ThemeColors;
                SetBinding(ThemeColorsProperty, binding);
            }
        }

        public IEnumerable<Color> ThemeColors
        {
            get { return (IEnumerable<Color>)GetValue(ThemeColorsProperty); }
            set { SetValue(ThemeColorsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThemeColors.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThemeColorsProperty =
            DependencyProperty.Register("ThemeColors", typeof(IEnumerable<Color>), typeof(BackgroundColorPicker), new PropertyMetadata(null, OnThemeColorsPropertyChanged));

        private static void OnThemeColorsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackgroundColorPicker source = d as BackgroundColorPicker;
            source.ApplyThemeColors();
 
        }
        

        public bool IsSolid
        {
            get { return (bool)GetValue(IsSolidProperty); }
            set { SetValue(IsSolidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSolid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSolidProperty =
            DependencyProperty.Register("IsSolid", typeof(bool), typeof(BackgroundColorPicker), new PropertyMetadata(false, OnIsSolidPropertyChanged));

         private static void OnIsSolidPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackgroundColorPicker source = d as BackgroundColorPicker;
            if (source.IsSolid)
                source.BackgroundEnd = new SolidColorBrush() { Color = source.BackgroundStart.Color };
            else
            {
                if (source.BackgroundStart.Color.ToString() != Colors.White.ToString())
                    source.BackgroundEnd = new SolidColorBrush(Colors.White);
                else
                    source.BackgroundEnd = new SolidColorBrush(Colors.LightGray);
            }
            source.setupPalette();
        }

         void setupPalette()
         {
             if (Palette != null && BackgroundEnd != null && BackgroundStart != null)
             {
                 if (BackgroundEnd.Color.ToString() == BackgroundStart.Color.ToString())
                     IsSolid = true;
                 if (IsSolid)
                 {
                     Palette.SelectedColorBrush = BackgroundEnd;
                 }
                 else
                 {
                     if (IsStartSelected)
                         Palette.SelectedColorBrush = BackgroundStart;
                     else
                         Palette.SelectedColorBrush = BackgroundEnd;
                 }
             }
         }

         public bool IsStartSelected
         {
             get { return (bool)GetValue(IsStartSelectedProperty); }
             set { SetValue(IsStartSelectedProperty, value); }
         }

         // Using a DependencyProperty as the backing store for IsStartSelected.  This enables animation, styling, binding, etc...
         public static readonly DependencyProperty IsStartSelectedProperty =
             DependencyProperty.Register("IsStartSelected", typeof(bool), typeof(BackgroundColorPicker), new PropertyMetadata(true, IsStartChanged));

         private static void IsStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
         {
             BackgroundColorPicker source = d as BackgroundColorPicker;
             source.setupPalette();
         }

         public SolidColorBrush BackgroundStart
        {
            get { return (SolidColorBrush)GetValue(BackgroundStartProperty); }
            set { SetValue(BackgroundStartProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundStart.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundStartProperty =
            DependencyProperty.Register("BackgroundStart", typeof(SolidColorBrush), typeof(BackgroundColorPicker), new PropertyMetadata(colorChanged));

        private static void colorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackgroundColorPicker source = d as BackgroundColorPicker;
            source.setupPalette();
            if (source.ColorChanged != null)
                source.ColorChanged(source, null);
        }

        public SolidColorBrush BackgroundEnd
        {
            get { return (SolidColorBrush)GetValue(BackgroundEndProperty); }
            set { SetValue(BackgroundEndProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundEnd.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundEndProperty =
            DependencyProperty.Register("BackgroundEnd", typeof(SolidColorBrush), typeof(BackgroundColorPicker), new PropertyMetadata(colorChanged));

        public event EventHandler ColorChanged;
    }

}
