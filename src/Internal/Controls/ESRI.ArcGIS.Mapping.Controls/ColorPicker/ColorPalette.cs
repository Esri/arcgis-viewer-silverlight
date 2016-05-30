/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [ContentProperty("Colors")]
    public class ColorPalette : Control
    {        
        private Grid ColorsGrid;        
        private Grid GraduationsGrid;        
        private const int _maxRows = 5;
        private Size _buttonSize = new Size(13.0, 13.0);        
        internal List<ColorPaletteButton> _colorButtons;
        private ColorPaletteButton _lastSelectedColorButton;
        private ToggleButton ToggleViewButton;
        private ColorSelector ColorSelector;
        private Grid StandardColorsGrid;

        #region SelectedColorBrush
        /// <summary>
        /// 
        /// </summary>
        public SolidColorBrush SelectedColorBrush
        {
            get { return GetValue(SelectedColorBrushProperty) as SolidColorBrush; }
            set { SetValue(SelectedColorBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectedColorBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedColorBrushProperty =
            DependencyProperty.Register(
                "SelectedColorBrush",
                typeof(SolidColorBrush),
                typeof(ColorPalette),
                new PropertyMetadata(new SolidColorBrush(System.Windows.Media.Colors.White), OnSelectedColorBrushPropertyChanged));

        /// <summary>
        /// SelectedColorBrushProperty property changed handler.
        /// </summary>
        /// <param name="d">ColorPalette that changed its SelectedColorBrush.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnSelectedColorBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPalette source = d as ColorPalette;            
            source.OnSelectedColorBrushPropertyChanged();
        }
        #endregion

        private void OnSelectedColorBrushPropertyChanged()
        {
            if (this._colorButtons != null)
            {
                foreach (ColorPaletteButton btn in this._colorButtons)
                {
                    btn.IsSelected = false; // IsCurrentSelectedColor(btn.Fill.Color);
                    if (btn.IsSelected)
                        _lastSelectedColorButton = btn;
                }
            }

            if (ColorSelector != null)
            {
                if(SelectedColorBrush != null)
                    ColorSelector.Color = SelectedColorBrush.Color;
            }
        }

        private bool IsCurrentSelectedColor(Color c)
        {            
            if (SelectedColorBrush != null)
            {
                return SelectedColorBrush.Color.Equals(c);
            }
            return false;
        }

        public event EventHandler<ColorChosenEventArgs> ColorPicked;

        public ColorPalette()
        {
            DefaultStyleKey = typeof(ColorPalette);                          
            SetValue(ColorsProperty, new Collection<Color>());
            SetValue(ThemeColorsProperty, new Collection<Color>());
            _colorButtons = new List<ColorPaletteButton>();
        }

        public override void OnApplyTemplate()
        {
            if (ColorSelector != null)
                ColorSelector.ColorSelected -= ColorSelector_ColorSelected;

            base.OnApplyTemplate();
            if (ColorsGrid != null)
                ColorsGrid.Children.Clear();
            if (GraduationsGrid != null)
                GraduationsGrid.Children.Clear();
            if (StandardColorsGrid != null)
                StandardColorsGrid.Children.Clear();

            ColorsGrid = GetTemplateChild("ColorsGrid") as Grid;
            GraduationsGrid = GetTemplateChild("GraduationsGrid") as Grid;
            ToggleViewButton = GetTemplateChild("ToggleViewButton") as ToggleButton;
            StandardColorsGrid = GetTemplateChild("StandardColorsGrid") as Grid;

            ColorSelector = GetTemplateChild("ColorSelector") as ColorSelector;
            if (ColorSelector != null)
            {
                ColorSelector.ColorSelected += ColorSelector_ColorSelected;
                if (SelectedColorBrush != null)
                    ColorSelector.InitialColor = SelectedColorBrush.Color;
            }
            generateColorButtons();
            buildColorGrids();
            buildStandardColorsGrid();
        }

        private void buildStandardColorsGrid()
        {
            if (StandardColorsGrid == null || Colors == null)
                return;

            StandardColorsGrid.Children.Clear();

            int rowNum = 0;
            int columnNum = 0;
            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(0.0, 0);
            StandardColorsGrid.RowDefinitions.Add(rowDefinition);
            
            int themeColorsCount = ThemeColors != null ? ThemeColors.Count : 7;
            foreach (Color standardColor in Colors)
            {
                ColumnDefinition definition = new ColumnDefinition();
                definition.Width = new GridLength(0.0, 0);
                StandardColorsGrid.ColumnDefinitions.Add(definition);

                ColorPaletteButton standardColorButton = generateColorButton(columnNum, rowNum, standardColor,
                    ColorPaletteButton.StackPosition.None);
                standardColorButton.Margin = new Thickness(2.0);
                StandardColorsGrid.Children.Add(standardColorButton);
                columnNum++;

                // Keep the number of columns equal to the number of theme colors
                if ((columnNum >= themeColorsCount) && themeColorsCount > 0 && (columnNum != Colors.Count - 1))
                {
                    // move to next row
                    rowNum++;
                    columnNum = 0;
                    rowDefinition = new RowDefinition();
                    rowDefinition.Height = new GridLength(0.0, 0);
                    StandardColorsGrid.RowDefinitions.Add(rowDefinition);
                }
            }
        }
    
        internal void ColorSelector_ColorSelected(Color c)
        {            
            OnColorPicked(new ColorChosenEventArgs(c));
        }

        protected virtual void OnColorPicked(ColorChosenEventArgs e)
        {
            if (this.ColorPicked != null)
                this.ColorPicked(this, e);
        }

        private void onColorButtonColorSelected(object sender, ColorChosenEventArgs args)
        {
            //if (_lastSelectedColorButton != null) // unselect the previous button
              //  _lastSelectedColorButton.IsSelected = false;                

            SelectedColorBrush = new SolidColorBrush(args.Color);

            OnColorPicked(args);
           
            _lastSelectedColorButton = sender as ColorPaletteButton;
            //if (_lastSelectedColorButton != null) // select this button
              //  _lastSelectedColorButton.IsSelected = true;

            if (ColorSelector != null)
                ColorSelector.InitialColor = ColorSelector.Color = args.Color;
        }

        private void buildColorGrids()
        {
            if (this.ColorsGrid != null)
            {
                this.ColorsGrid.Children.Clear();
                this.createColumnDefinitions(this.ColorsGrid, ThemeColors != null ? ThemeColors.Count : 7);
            }

            if (this.GraduationsGrid != null)
            {
                this.GraduationsGrid.Children.Clear();
                this.createColumnDefinitions(this.GraduationsGrid, ThemeColors != null ? ThemeColors.Count : 7);
                this.createRowDefinitions(this.GraduationsGrid, this.MaxRows);
            }

            for (int i = 0; i < this._colorButtons.Count; i++)
            {
                if (i % (MaxRows + 1) == 0)
                {
                    if (this.ColorsGrid != null)
                    {
                        this.ColorsGrid.Children.Add(this._colorButtons[i]);
                    }
                }
                else
                {
                    if (this.GraduationsGrid != null)
                    {
                        this.GraduationsGrid.Children.Add(this._colorButtons[i]);
                    }
                }
            }
        }        
    
        internal void generateColorButtons()
        {
             Color colorWhite = System.Windows.Media.Colors.White;
             Color colorBlack = System.Windows.Media.Colors.Black;
            // detach all click handlers
             foreach (ColorPaletteButton btn in _colorButtons)
             {
                 btn.ColorSelected -= this.onColorButtonColorSelected;
             }
             
            this._colorButtons.Clear();
            int colNum = 0;
            Collection<Color> themeColors = ThemeColors ?? Colors;
            ColorPaletteButton.StackPosition position = ColorPaletteButton.StackPosition.None;
            if (themeColors != null)
            {
                foreach (Color currentColor in themeColors)
                {
                    Color color3;
                    int rowNum = 0;
                    int num5;

                    colNum++;
                    color3 = currentColor;
                    this._colorButtons.Add(this.generateColorButton(colNum - 1, color3, 
                        ColorPaletteButton.StackPosition.None));
                    rowNum = 0;
                    if (ColorToDouble(color3) <= 0.699999988079071)
                    {
                        num5 = (int)Math.Floor((double)(((double)this.MaxRows) / 2.0));
                        int num6 = (int)Math.Ceiling((double)(((double)this.MaxRows) / 2.0));
                        for (int i = 0; i < num6; i++)
                        {
                            float num8 = ((float)(i + 1)) / (num6 + 1f);
                            position = i == 0 ? ColorPaletteButton.StackPosition.Top : ColorPaletteButton.StackPosition.Middle;
                            this._colorButtons.Add(this.generateColorButton(colNum - 1, rowNum++, generateColor(colorWhite, color3, num8),
                                position));
                        }
                        int num9 = 0;

                        while (num9 < num5)
                        {
                            float num10 = ((float)(num9 + 1)) / ((float)num5);
                            num10 *= 0.7f;
                            position = num9 < num5 - 1 ? ColorPaletteButton.StackPosition.Middle : ColorPaletteButton.StackPosition.Bottom;
                            this._colorButtons.Add(this.generateColorButton(colNum - 1, rowNum++, generateColor(color3, colorBlack, num10),
                                position));
                            num9++;
                        }
                        continue;
                    }
                    int num3 = 0;
                    while (num3 < this.MaxRows)
                    {
                        float num4 = ((float)(num3 + 1)) / (this.MaxRows + 1f);
                        num4 *= 0.7f;

                        if (num3 == 0)
                            position = ColorPaletteButton.StackPosition.Top;
                        else if (num3 == this.MaxRows - 1)
                            position = ColorPaletteButton.StackPosition.Bottom;
                        else
                            position = ColorPaletteButton.StackPosition.Middle;

                        this._colorButtons.Add(this.generateColorButton(colNum - 1, rowNum++, generateColor(color3, colorBlack, num4), 
                            position));
                        num3++;
                    }
                }
            }
        }
    
        private static double ColorToDouble(Color color)
        {
          double red = ((double) color.R) / 255.0;
          double green = ((double) color.G) / 255.0;
          double blue = ((double) color.B) / 255.0;
          return (((0.30000001192092896 * red) + (0.5899999737739563 * green)) + (0.10999999940395355 * blue));
        }
        
        private void createColumnDefinitions(Grid grid, int colMax)
        {
          grid.ColumnDefinitions.Clear();
          for (int i = 0; i < colMax; i++)
          {
            ColumnDefinition definition = new ColumnDefinition();
            definition.Width = new GridLength(0.0, 0);
            grid.ColumnDefinitions.Add(definition);
          }
        }
        
        private void createRowDefinitions(Grid grid, int rowMax)
        {
          grid.RowDefinitions.Clear();
          for (int i = 0; i < rowMax; i++)
          {
            RowDefinition definition = new RowDefinition();
            definition.Height = new GridLength(0.0, 0);
            grid.RowDefinitions.Add(definition);
          }
        }
        
        internal static Color generateColor(Color colorA, Color colorB, float factor)
        {
          double num = ((double) colorA.A) / 255.0;
          double num2 = ((double) colorA.R) / 255.0;
          double num3 = ((double) colorA.G) / 255.0;
          double num4 = ((double) colorA.B) / 255.0;
          double num5 = ((double) colorB.A) / 255.0;
          double num6 = ((double) colorB.R) / 255.0;
          double num7 = ((double) colorB.G) / 255.0;
          double num8 = ((double) colorB.B) / 255.0;
          byte num9 = Convert.ToByte((double) ((num + ((num5 - num) * factor)) * 255.0));
          byte num10 = Convert.ToByte((double) ((num2 + ((num6 - num2) * factor)) * 255.0));
          byte num11 = Convert.ToByte((double) ((num3 + ((num7 - num3) * factor)) * 255.0));
          byte num12 = Convert.ToByte((double) ((num4 + ((num8 - num4) * factor)) * 255.0));
          return Color.FromArgb(num9, num10, num11, num12);
        }

        private ColorPaletteButton generateColorButton(int columnNumber, Color fillColor, 
            ColorPaletteButton.StackPosition position)
        {
          ColorPaletteButton button = new ColorPaletteButton(fillColor, position);
          button.ColorSelected += this.onColorButtonColorSelected;
          button.Width = this._buttonSize.Width;
          button.Height= this._buttonSize.Height;
          button.Margin = new Thickness(2.0);
          Grid.SetColumn(button, columnNumber);
          return button;
        }

        private ColorPaletteButton generateColorButton(int columnNumber, int rowNum, Color fillColor, 
            ColorPaletteButton.StackPosition position)
        {
            ColorPaletteButton button = new ColorPaletteButton(fillColor, position);
            button.IsSelected = false; // IsCurrentSelectedColor(fillColor);
            button.ColorSelected += this.onColorButtonColorSelected;
            button.Width = this._buttonSize.Width;
            button.Height = this._buttonSize.Height;
            Grid.SetColumn(button, columnNumber);
            Grid.SetRow(button, rowNum);
            if (rowNum == 0)
            {
                button.Margin = new Thickness(2.0, 2.0, 2.0, 0.0);
                return button;
            }
            if (rowNum < (this.MaxRows - 1))
            {
                button.Margin = new Thickness(2.0, 0.0, 2.0, 0.0);
                return button;
            }
            button.Margin = new Thickness(2.0, 0.0, 2.0, 2.0);
            return button;
        }

        #region Colors
        /// <summary>
        /// 
        /// </summary>
        public Collection<Color> Colors
        {
            get { return GetValue(ColorsProperty) as Collection<Color>; }
            set { SetValue(ColorsProperty, value); }
        }

        /// <summary>
        /// Identifies the Colors dependency property.
        /// </summary>
        public static readonly DependencyProperty ColorsProperty =
            DependencyProperty.Register(
                "Colors",
                typeof(Collection<Color>),
                typeof(ColorPalette),
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
                typeof(ColorPalette),
                new PropertyMetadata(null));        
        #endregion

        private int MaxRows
        {
          get
          {
              return _maxRows;
          }
        }
    }

    public class ColorPaletteButton : Control
    {
        private Border _border;
        private Border _outerBorder;
        private Border _contentBorder;
        private SolidColorBrush OuterBorderBrush = new SolidColorBrush(Color.FromArgb(255, 226, 228, 231));
        private SolidColorBrush SelectedBorderBrush = new SolidColorBrush(Color.FromArgb(255, 239, 72, 16));
        private SolidColorBrush InnerBorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 226, 148));
        private SolidColorBrush MouseOverBorderBrush = new SolidColorBrush(Color.FromArgb(255, 242, 148, 54));
        private SolidColorBrush TransparentBorderBrush = new SolidColorBrush(Colors.Transparent);
        private StackPosition _position = StackPosition.None;

        #region IsSelected
        /// <summary>
        /// 
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        /// Identifies the IsSelected dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                "IsSelected",
                typeof(bool),
                typeof(ColorPaletteButton),
                new PropertyMetadata(false, OnIsSelectedPropertyChanged));

        /// <summary>
        /// IsSelectedProperty property changed handler.
        /// </summary>
        /// <param name="d">ColorPaletteButton that changed its IsSelected.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPaletteButton source = d as ColorPaletteButton;
            source.UpdateBorderOnIsSelectedChanged();
        }
        #endregion 
        
        #region Fill
        /// <summary>
        /// 
        /// </summary>
        public SolidColorBrush Fill
        {
            get { return GetValue(FillBrushProperty) as SolidColorBrush; }
            set { SetValue(FillBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the FillBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty FillBrushProperty =
            DependencyProperty.Register(
                "Fill",
                typeof(SolidColorBrush),
                typeof(ColorPaletteButton),
                new PropertyMetadata(null));
        #endregion 

        public ColorPaletteButton()
        {
            this.DefaultStyleKey = typeof(ColorPaletteButton);
        }


        public ColorPaletteButton(Color color, StackPosition position): this()
        {
            Fill = new SolidColorBrush(color);
            _position = position;
        }

        public override void OnApplyTemplate()
        {   
            if (_contentBorder != null)
            {
                _contentBorder.MouseLeftButtonUp -= _contentPresenter_MouseLeftButtonUp;
                _contentBorder.MouseEnter -= rectangle_MouseEnter;
                _contentBorder.MouseLeave -= rectangle_MouseLeave;
            }

            base.OnApplyTemplate();

            _border = GetTemplateChild("Border") as Border;
            _outerBorder = GetTemplateChild("OuterBorder") as Border;

            _contentBorder = GetTemplateChild("Content") as Border;
            if (_contentBorder != null)
            {
                _contentBorder.MouseLeftButtonUp += _contentPresenter_MouseLeftButtonUp;
                _contentBorder.MouseEnter += rectangle_MouseEnter;
                _contentBorder.MouseLeave += rectangle_MouseLeave;
            }

            switch (_position)
            {
                case (StackPosition.None):
                    _outerBorder.BorderThickness = new Thickness(1.0);
                    _border.BorderThickness = new Thickness(0.0);
                    break;
                case (StackPosition.Top):
                    _outerBorder.BorderThickness = new Thickness(1.0, 1.0, 1.0, 0.0);
                    _border.BorderThickness = new Thickness(0.0, 0.0, 0.0, 1.0);
                    break;
                case (StackPosition.Middle):
                    _outerBorder.BorderThickness = new Thickness(1.0, 0.0, 1.0, 0.0);
                    _border.BorderThickness = new Thickness(0.0, 1.0, 0.0, 1.0);
                    break;
                case (StackPosition.Bottom):
                    _outerBorder.BorderThickness = new Thickness(1.0, 0.0, 1.0, 1.0);
                    _border.BorderThickness = new Thickness(0.0, 1.0, 0.0, 0.0);
                    break;
            }

            if (IsSelected)
            {
                if (_border != null)
                    _border.BorderBrush = SelectedBorderBrush;
                if (_outerBorder != null)
                    _outerBorder.BorderBrush = SelectedBorderBrush;
                if (_contentBorder != null)
                    _contentBorder.BorderBrush = InnerBorderBrush;
            }
            else
            {
                if (_border != null)
                    _border.BorderBrush = Fill;
                if (_outerBorder != null)
                    _outerBorder.BorderBrush = OuterBorderBrush;
                if (_contentBorder != null)
                    _contentBorder.BorderBrush = Fill;
            }
        }

        private void UpdateBorderOnIsSelectedChanged()
        {
            if (_border != null)
            {
                _border.BorderBrush = IsSelected ? SelectedBorderBrush : Fill;
                _outerBorder.BorderBrush = IsSelected ? SelectedBorderBrush : OuterBorderBrush;
                _contentBorder.BorderBrush = IsSelected ? InnerBorderBrush : Fill;
            }
        }

        void rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_border != null && !IsSelected)
            {
                _border.BorderBrush = Fill;
                if (_outerBorder != null)
                    _outerBorder.BorderBrush = OuterBorderBrush;
                if (_contentBorder != null)
                    _contentBorder.BorderBrush = Fill;
            }
        }

        void rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_border != null)
                _border.BorderBrush = MouseOverBorderBrush;
            if (_outerBorder != null)
                _outerBorder.BorderBrush = MouseOverBorderBrush;
            if (_contentBorder != null)
                _contentBorder.BorderBrush = InnerBorderBrush;
        }

        void _contentPresenter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OnColorSelected(new ColorChosenEventArgs(Fill.Color));
        }

        protected virtual void OnColorSelected(ColorChosenEventArgs args)
        {
            if(ColorSelected != null)
                ColorSelected(this, args);
        }

        public event EventHandler<ColorChosenEventArgs> ColorSelected;

        public enum StackPosition { None, Top, Middle, Bottom }
    }

    public class ColorChosenEventArgs  : EventArgs
    {
        public Color Color { get; set; }
        public ColorChosenEventArgs(Color color)
        {
            Color = color;
        }
    }
}

