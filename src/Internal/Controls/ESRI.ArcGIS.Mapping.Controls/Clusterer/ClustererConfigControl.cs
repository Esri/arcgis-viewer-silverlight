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
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Mapping.Controls.Resources;
using System.Windows.Data;

namespace ESRI.ArcGIS.Mapping.Controls
{
	public class ClustererConfigControl : LayerConfigControl
	{
		private const string PART_UseClusteringCheckBox = "UseClusteringCheckBox";
		private const string PART_MaxPointsUpDown = "MaxPointsUpDown";
		private const string PART_BackgroundColorSelector = "BackgroundColorSelector";
		private const string PART_RadiusUpDown = "RadiusUpDown";
		private const string PART_ForegroundColorSelector = "ForegroundColorSelector";

		internal NumericUpDown MaxPointsUpDown;
		internal SolidColorBrushSelector BackgroundColorSelector;

		internal NumericUpDown RadiusUpDown;
		internal SolidColorBrushSelector ForegroundColorSelector;

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
                typeof(ClustererConfigControl),
                new PropertyMetadata(null));
        #endregion

		public ClustererConfigControl()
		{
			DefaultStyleKey = typeof(ClustererConfigControl);
		}

		public override void OnApplyTemplate()
		{
			if (MaxPointsUpDown != null)
				MaxPointsUpDown.ValueChanged -= MaxPointsUpDown_ValueChanged;
			if (BackgroundColorSelector != null)
				BackgroundColorSelector.ColorPicked -= BackgroundColorSelector_ColorPicked;
			if (RadiusUpDown != null)
				RadiusUpDown.ValueChanged -= RadiusUpDown_ValueChanged;
			if (ForegroundColorSelector != null)
				ForegroundColorSelector.ColorPicked -= ForegroundColorSelector_ColorPicked;

			base.OnApplyTemplate();

			MaxPointsUpDown = GetTemplateChild(PART_MaxPointsUpDown) as NumericUpDown;
			if (MaxPointsUpDown != null)
				MaxPointsUpDown.ValueChanged += MaxPointsUpDown_ValueChanged;

			BackgroundColorSelector = GetTemplateChild(PART_BackgroundColorSelector) as SolidColorBrushSelector;
			if (BackgroundColorSelector != null)
				BackgroundColorSelector.ColorPicked += BackgroundColorSelector_ColorPicked;

			RadiusUpDown = GetTemplateChild(PART_RadiusUpDown) as NumericUpDown;
			if (RadiusUpDown != null)
				RadiusUpDown.ValueChanged += RadiusUpDown_ValueChanged;

			ForegroundColorSelector = GetTemplateChild(PART_ForegroundColorSelector) as SolidColorBrushSelector;
			if (ForegroundColorSelector != null)
				ForegroundColorSelector.ColorPicked += ForegroundColorSelector_ColorPicked;

			bindUI();

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
		}

        internal event EventHandler InitCompleted;

        protected override void OnLayerChanged(System.Windows.DependencyPropertyChangedEventArgs e)
		{
			base.OnLayerChanged(e);
			bindUI();
		}

		GraphicsLayer graphicsLayer
		{
			get { return Layer as GraphicsLayer; }
		}

        private void bindUI()
        {
            if (graphicsLayer == null)
            {
                this.IsEnabled = false;
                return;
            }

            GeometryType goemType = LayerExtensions.GetGeometryType(graphicsLayer);
            //Point layers only
            if (goemType != GeometryType.Point)
            {
                this.IsEnabled = false;
                return;
            }

            this.IsEnabled = true;

            FlareClusterer clusterer = graphicsLayer.Clusterer as FlareClusterer;
            if (clusterer == null)
            {
                bindClustererToUI(new FlareClusterer());//bind to defaults from new clusterer
            }
            else
            {
                bindClustererToUI(clusterer);
            }
        }

		private void bindClustererToUI(FlareClusterer clusterer)
		{
			if (MaxPointsUpDown != null) MaxPointsUpDown.Value = Convert.ToDouble(clusterer.MaximumFlareCount);
			if (BackgroundColorSelector != null) BackgroundColorSelector.ColorBrush = clusterer.FlareBackground as SolidColorBrush;
			if (RadiusUpDown != null) RadiusUpDown.Value = Convert.ToDouble(clusterer.Radius);
			if (ForegroundColorSelector != null) ForegroundColorSelector.ColorBrush = clusterer.FlareForeground as SolidColorBrush;
		}

		void ForegroundColorSelector_ColorPicked(object sender, ColorChosenEventArgs e)
		{
			if (graphicsLayer != null) graphicsLayer.ChangeClusterFlareForeground(e.Color);
		}

		void RadiusUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (graphicsLayer == null) return;
			int newValue = Convert.ToInt32(RadiusUpDown.Value);
			if (newValue > 0)
			{
				graphicsLayer.ChangeClusterRadius(newValue);
			}
			else
			{
				FlareClusterer clusterer = graphicsLayer.Clusterer as FlareClusterer;
				if (clusterer != null)
					RadiusUpDown.Value = Convert.ToDouble(clusterer.Radius);
			}
		}

		void BackgroundColorSelector_ColorPicked(object sender, ColorChosenEventArgs e)
		{
			if (graphicsLayer != null) graphicsLayer.ChangeClusterFlareBackground(e.Color);
		}

		void MaxPointsUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (graphicsLayer == null) return;
			int newValue = Convert.ToInt32(MaxPointsUpDown.Value);
			if (newValue > 0)
			{
				graphicsLayer.ChangeClusterMaxPoints(newValue);
			}
			else
			{
				FlareClusterer clusterer = graphicsLayer.Clusterer as FlareClusterer;
				if (clusterer != null)
					MaxPointsUpDown.Value = Convert.ToDouble(clusterer.MaximumFlareCount);
			}
		}
	}

    public class IsClusteredConverter : DependencyObject, IValueConverter
    {
        #region Layer
        public Layer Layer
        {
            get { return GetValue(LayerProperty) as Layer; }
            set { SetValue(LayerProperty, value); }
        }

        /// <summary>
        /// Identifies the IsChecked dependency property.
        /// </summary>
        public static readonly DependencyProperty LayerProperty =
            DependencyProperty.Register(
                "Layer",
                typeof(Layer),
                typeof(IsClusteredConverter),
                new PropertyMetadata(null));
        #endregion

        #region CheckBox
        public CheckBox CheckBox
        {
            get { return GetValue(CheckBoxProperty) as CheckBox; }
            set { SetValue(CheckBoxProperty, value); }
        }

        /// <summary>
        /// Identifies the IsChecked dependency property.
        /// </summary>
        public static readonly DependencyProperty CheckBoxProperty =
            DependencyProperty.Register(
                "CheckBox",
                typeof(CheckBox),
                typeof(IsClusteredConverter),
                new PropertyMetadata(null));
        #endregion

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return true;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && (bool)value)
            {
                if (!ESRI.ArcGIS.Client.Extensibility.LayerProperties.GetIsEditable(Layer))
                    return new FlareClusterer();
                else
                {
                    MessageBoxDialog.Show(Strings.ClusteringDisallowedMessage, Strings.ClusteringDisallowedCaption, MessageBoxButton.OK);
                    if(CheckBox != null)
                        CheckBox.IsChecked = false;
                }
            }
            return null;
        }
    }
}
