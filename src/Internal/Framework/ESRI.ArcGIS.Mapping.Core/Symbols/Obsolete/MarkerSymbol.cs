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
using System.Windows.Markup;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Core.Symbols
{
    public class MarkerSymbol : ESRI.ArcGIS.Client.Symbols.MarkerSymbol
    {
        public MarkerSymbol()
        {
            Offset = new Point(0, 0);
            ControlTemplate = XamlReader.
            Load(
@"<ControlTemplate
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
		<Ellipse x:Name=""ellipse""
			RenderTransformOrigin=""0.5,0.5"" 
			Fill=""{Binding Symbol.Color}""
			Width=""{Binding Symbol.Size}""
			Height=""{Binding Symbol.Size}"" >
			<VisualStateManager.VisualStateGroups>
				<VisualStateGroup x:Name=""CommonStates"">
					<VisualState x:Name=""Normal"">
						<Storyboard>
							<DoubleAnimation BeginTime=""00:00:00"" Storyboard.TargetName=""ellipse"" Storyboard.TargetProperty=""(UIElement.RenderTransform).(ScaleTransform.ScaleX)"" To=""1"" Duration=""0:0:0.1"" />
							<DoubleAnimation BeginTime=""00:00:00"" Storyboard.TargetName=""ellipse"" Storyboard.TargetProperty=""(UIElement.RenderTransform).(ScaleTransform.ScaleY)"" To=""1"" Duration=""0:0:0.1"" />
						</Storyboard>
					</VisualState>
					<VisualState x:Name=""MouseOver"">
						<Storyboard>
							<DoubleAnimation BeginTime=""00:00:00"" Storyboard.TargetName=""ellipse"" Storyboard.TargetProperty=""(UIElement.RenderTransform).(ScaleTransform.ScaleX)"" To=""1.25"" Duration=""0:0:0.1"" />
							<DoubleAnimation BeginTime=""00:00:00"" Storyboard.TargetName=""ellipse"" Storyboard.TargetProperty=""(UIElement.RenderTransform).(ScaleTransform.ScaleY)"" To=""1.25"" Duration=""0:0:0.1"" />
						</Storyboard>
					</VisualState>
				</VisualStateGroup>
			</VisualStateManager.VisualStateGroups>
			<Ellipse.RenderTransform>
				<ScaleTransform ScaleX=""1"" ScaleY=""1"" />
			</Ellipse.RenderTransform>
		</Ellipse>
	</ControlTemplate>") as ControlTemplate;
        }

        #region Color
        /// <summary>
        /// 
        /// </summary>
        public Brush Color
        {
            get { return GetValue(SymbolColorProperty) as Brush; }
            set { SetValue(SymbolColorProperty, value); }
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MarkerSymbol dp = d as MarkerSymbol;
            if (dp != null)
                dp.OnPropertyChanged("Color");
        }

        /// <summary>
        /// Identifies the SymbolColor dependency property.
        /// </summary>
        public static readonly DependencyProperty SymbolColorProperty =
            DependencyProperty.Register(
                "Color",
                typeof(Brush),
                typeof(MarkerSymbol),
                new PropertyMetadata(null, OnColorChanged));
        #endregion 

        #region Size
        /// <summary>
        /// Size of the symbol
        /// </summary>
        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        /// <summary>
        /// Identifies the Size dependency property.
        /// </summary>
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(
                "Size",
                typeof(double),
                typeof(MarkerSymbol),
                new PropertyMetadata(10.0, OnSizeChanged));

        private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MarkerSymbol symbol = d
                as MarkerSymbol;
            if (d == null)
                return;

            symbol.setOffset();

            symbol.OnPropertyChanged("Size");
        }
        #endregion

        #region Opacity
        /// <summary>
        /// Identifies the <see cref="Opacity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OpacityProperty =
                        DependencyProperty.Register("Opacity", typeof(double), typeof(MarkerSymbol),
                        new PropertyMetadata(1.0, OnOpacityChanged));
        /// <summary>
        /// Gets or sets Opacity.
        /// </summary>
        public double Opacity
        {
            get { return (double)GetValue(OpacityProperty); }
            set { SetValue(OpacityProperty, value); }
        }

        private static void OnOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MarkerSymbol dp = d as MarkerSymbol;
            if (dp != null)
                dp.OnPropertyChanged("Opacity");
        }
        #endregion

        #region OriginX
        /// <summary>
        /// OriginX of the symbol
        /// </summary>
        public double OriginX
        {
            get { return (double)GetValue(OriginXProperty); }
            set { SetValue(OriginXProperty, value); }
        }

        /// <summary>
        /// Identifies the OriginX dependency property.
        /// </summary>
        public static readonly DependencyProperty OriginXProperty =
            DependencyProperty.Register(
                "OriginX",
                typeof(double),
                typeof(MarkerSymbol),
                new PropertyMetadata(0d, OnOriginXChanged));

        private static void OnOriginXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MarkerSymbol symbol = d as MarkerSymbol;
            if (d == null)
                return;

            symbol.setOffset();
        }

        protected virtual void setOffset()
        {
            Offset = new Point(Size * -OriginX, Size * -OriginY);
        }


        #endregion

        #region OriginY
        /// <summary>
        /// OriginY of the symbol
        /// </summary>
        public double OriginY
        {
            get { return (double)GetValue(OriginYProperty); }
            set { SetValue(OriginYProperty, value); }
        }

        /// <summary>
        /// Identifies the OriginY dependency property.
        /// </summary>
        public static readonly DependencyProperty OriginYProperty =
            DependencyProperty.Register(
                "OriginY",
                typeof(double),
                typeof(MarkerSymbol),
                new PropertyMetadata(0d, OnOriginYChanged));

        private static void OnOriginYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MarkerSymbol symbol = d as MarkerSymbol;
            if (d == null)
                return;

            symbol.setOffset();
        }
        #endregion

        #region Offset
        public Point Offset
        {
            get { return (Point)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Offset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(Point), typeof(MarkerSymbol), null);
        #endregion

        #region DisplayName
        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register("DisplayName", typeof(string), typeof(MarkerSymbol), null);
        #endregion

    }
}
