/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "ColorPalette", Type = typeof(ColorPalette))]
    [TemplatePart(Name = "DropDownButton", Type = typeof(DropDownButton))]
    public class SolidColorBrushSelector : Control
    {
        DropDownButton DropDownButton;
        internal ColorPalette ColorPalette;

        public SolidColorBrushSelector()
        {
            DefaultStyleKey = typeof(SolidColorBrushSelector);
        }


        public string ColorBrushName
        {
            get;
            set;
        }

        #region SelectedColorBrush
        /// <summary>
        /// 
        /// </summary>
        public SolidColorBrush ColorBrush
        {
            get { return GetValue(ColorBrushProperty) as SolidColorBrush; }
            set { SetValue(ColorBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectedColorBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty ColorBrushProperty =
            DependencyProperty.Register(
                "SelectedColorBrush",
                typeof(SolidColorBrush),
                typeof(SolidColorBrushSelector),
                new PropertyMetadata(null, OnSelectedColorBrushPropertyChanged));

        /// <summary>
        /// SelectedColorBrushProperty property changed handler.
        /// </summary>
        /// <param name="d">ColorPalette that changed its SelectedColorBrush.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnSelectedColorBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SolidColorBrushSelector source = d as SolidColorBrushSelector;
            SolidColorBrush value = e.NewValue as SolidColorBrush;
            source.OnSelectedColorBrushPropertyChanged();
        }
        #endregion 

        #region Image
        /// <summary>
        /// 
        /// </summary>
        public BitmapImage Image
        {
            get { return GetValue(ImageProperty) as BitmapImage; }
            set { SetValue(ImageProperty, value); }
        }

        /// <summary>
        /// Identifies the SmallImage dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(
                "Image",
                typeof(BitmapImage),
                typeof(SolidColorBrushSelector),
                new PropertyMetadata(null));
        #endregion

        #region DisplayArrowBelowImage
        /// <summary>
        /// 
        /// </summary>
        public bool DisplayArrowBelowImage
        {
            get { return (bool)GetValue(DisplayArrowBelowImageProperty); }
            set { SetValue(DisplayArrowBelowImageProperty, value); }
        }

        /// <summary>
        /// Identifies the DisplayArrowBelowImage dependency property.
        /// </summary>
        public static readonly DependencyProperty DisplayArrowBelowImageProperty =
            DependencyProperty.Register(
                "DisplayArrowBelowImage",
                typeof(bool),
                typeof(SolidColorBrushSelector),
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
                typeof(SolidColorBrushSelector),
                new PropertyMetadata(null));
        #endregion

        public bool AutoClosePaletteOnColorChosen { get; set; }

        internal void OnSelectedColorBrushPropertyChanged()
        {
            if (ColorPalette != null)
                ColorPalette.SelectedColorBrush = ColorBrush;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            initializeColorBrush();
            initializeThemeColors();

            if (ColorPalette != null)
                ColorPalette.ColorPicked -= ColorPalette_ColorPicked;

            ColorPalette = GetTemplateChild("ColorPalette") as ColorPalette;
            if (ColorPalette != null)
            {
                ColorPalette.ColorPicked += ColorPalette_ColorPicked;
                if (ColorBrush != null)
                    ColorPalette.SelectedColorBrush = ColorBrush;
            }

            if (DropDownButton != null)
            {
                DropDownButton.Opening -= DropDownButton_Opening;
                DropDownButton.PopupContent = null;
            }

            DropDownButton = GetTemplateChild("DropDownButton") as DropDownButton;
            if (DropDownButton != null)
            {
                DropDownButton.Opening += DropDownButton_Opening;
            }

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
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
        private void initializeColorBrush()
        {
            if (View.Instance != null && ColorBrush == null && !string.IsNullOrEmpty(ColorBrushName))
            {
                System.Windows.Data.Binding binding = new System.Windows.Data.Binding();
                binding.Source = View.Instance.ApplicationColorSet;
                binding.Path = new PropertyPath(ColorBrushName);
                binding.Mode = System.Windows.Data.BindingMode.TwoWay;
                binding.Converter = new ColorToSolidColorBrushConverter();
                SetBinding(ColorBrushProperty, binding);
            }
        }

        internal event EventHandler InitCompleted;

        void ColorPalette_ColorPicked(object sender, ColorChosenEventArgs e)
        {
            if (ColorBrush == null)
                ColorBrush = new SolidColorBrush(e.Color);
            else
                ColorBrush.Color = e.Color;
            OnColorPicked(e);
            if(AutoClosePaletteOnColorChosen)
                ClosePopup();
        }

        void DropDownButton_Opening(object sender, EventArgs e)
        {
            OnOpening(e);
        }

        protected virtual void OnColorPicked(ColorChosenEventArgs e)
        {
            if (this.ColorPicked != null)
            {
                this.ColorPicked(this, e);
            }
        }

        protected virtual void OnOpening(EventArgs args)
        {
            if (Opening != null)
                Opening(this, args);
        }

        public event EventHandler<ColorChosenEventArgs> ColorPicked;
        public event EventHandler Opening;

        #region IsContentPopupOpen
        /// <summary>
        /// 
        /// </summary>
        public bool IsContentPopupOpen
        {
            get { return (bool)GetValue(IsContentPopupOpenProperty); }
            set { SetValue(IsContentPopupOpenProperty, value); }
        }

        /// <summary>
        /// Identifies the IsContentPopupOpen dependency property.
        /// </summary>
        public static readonly DependencyProperty IsContentPopupOpenProperty =
            DependencyProperty.Register(
                "IsContentPopupOpen",
                typeof(bool),
                typeof(SolidColorBrushSelector),
                new PropertyMetadata(false));
        #endregion

        internal void RepositionPopup()
        {

        }

        internal void ClosePopup()
        {
            if (DropDownButton != null)
                DropDownButton.IsContentPopupOpen = false;
        }
    }
}
