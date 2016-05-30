/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Core.DataSources;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ScaleBar : Control
    {
        Rectangle PaddingLeftForScaleBarTextMeters;
        Rectangle PaddingLeftTopNotch;
        TextBlock ScaleBarTextForMeters;
        Rectangle PaddingLeftForScaleBarTextMiles;
        Rectangle PaddingLeftBottomNotch;
        TextBlock ScaleBarTextForMiles;
        Rectangle ScaleBarBlock;


        private const string PART_PADDING_LEFT_FOR_SCALEBAR_TEXT_METERS = "PaddingLeftForScaleBarTextMeters";
        private const string PART_PADDING_LEFT_TOPNOTCH = "PaddingLeftTopNotch";
        private const string PART_SCALEBAR_TEXT_FOR_METERS = "ScaleBarTextForMeters";
        private const string PART_PADDING_LEFT_FOR_SCALEBAR_TEXT_MILES = "PaddingLeftForScaleBarTextMiles";
        private const string PART_PADDING_LEFT_BOTTOM_NOTCH = "PaddingLeftBottomNotch";
        private const string PART_SCALEBAR_TEXT_FOR_MILES = "ScaleBarTextForMiles";
        private const string PART_SCALEBAR_BLOCK = "ScaleBarBlock";

        public ScaleBar()
        {
            DefaultStyleKey = typeof(ScaleBar);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PaddingLeftForScaleBarTextMeters = GetTemplateChild(PART_PADDING_LEFT_FOR_SCALEBAR_TEXT_METERS) as Rectangle;
            PaddingLeftTopNotch = GetTemplateChild(PART_PADDING_LEFT_TOPNOTCH) as Rectangle;
            ScaleBarTextForMeters = GetTemplateChild(PART_SCALEBAR_TEXT_FOR_METERS) as TextBlock;
            PaddingLeftForScaleBarTextMiles = GetTemplateChild(PART_PADDING_LEFT_FOR_SCALEBAR_TEXT_MILES) as Rectangle;
            PaddingLeftBottomNotch = GetTemplateChild(PART_PADDING_LEFT_BOTTOM_NOTCH) as Rectangle;
            ScaleBarTextForMiles = GetTemplateChild(PART_SCALEBAR_TEXT_FOR_MILES) as TextBlock;
            ScaleBarBlock = GetTemplateChild(PART_SCALEBAR_BLOCK) as Rectangle;
            refreshScalebar();
        }

        #region Helper Functions
        private void map_ExtentChanged(object sender, ESRI.ArcGIS.Client.ExtentEventArgs args)
        {
            refreshScalebar();
        }

        private double getMapResolution()
        {
            if (double.IsNaN(Map.Resolution) & Map.Width > 0 && Map.Extent != null)
            {
                return Map.Extent.Width / Map.Width;
            }
            return Map.Resolution;
        }

        private void refreshScalebar()
        {
            if (Map == null || double.IsNaN(getMapResolution()))
            {
                return;
            }

			ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit outUnit = ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Undefined;
            double outResolution;

            #region KiloMeters/Meters
			double roundedKiloMeters = getBestEstimateOfValue(getMapResolution(), ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Kilometers, out outUnit, out outResolution);
            double widthMeters = roundedKiloMeters / outResolution;
			bool inMeters = outUnit == ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Meters;

            if (PaddingLeftForScaleBarTextMeters != null &&
                PaddingLeftForScaleBarTextMeters != null &&
                ScaleBarTextForMeters != null)
            {
                PaddingLeftForScaleBarTextMeters.Width = widthMeters;
                PaddingLeftTopNotch.Width = widthMeters;
                ScaleBarTextForMeters.Text = string.Format("{0}{1}", roundedKiloMeters, (inMeters ? LocalizableStrings.ScaleBarUnit_meters : LocalizableStrings.ScaleBarUnit_kilometers));
                ScaleBarTextForMeters.Width = widthMeters;
            }
            #endregion

            #region Miles

			double roundedMiles = getBestEstimateOfValue(getMapResolution(), ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Miles, out outUnit, out outResolution);
            double widthMiles = roundedMiles / outResolution;
			bool inFeet = outUnit == ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Feet;
            if (PaddingLeftForScaleBarTextMiles != null &&
                PaddingLeftBottomNotch != null &&
                ScaleBarTextForMiles != null)
            {
                PaddingLeftForScaleBarTextMiles.Width = widthMiles;
                PaddingLeftBottomNotch.Width = widthMiles;
                ScaleBarTextForMiles.Text = string.Format("{0}{1}", roundedMiles, inFeet ? LocalizableStrings.ScaleBarUnit_feet : LocalizableStrings.ScaleBarUnit_miles);
                ScaleBarTextForMiles.Width = widthMiles;
            }
            #endregion

            if (roundedMiles == double.NaN || roundedKiloMeters == double.NaN && this.Visibility == Visibility.Visible)
                this.Visibility = Visibility.Collapsed;

            if (ScaleBarBlock != null)
            {
                double widthOfNotches = 4; // 2 for left notch, 2 for right notch
                double scaleBarBlockWidth = (widthMiles > widthMeters) ? widthMiles : widthMeters;
                scaleBarBlockWidth += widthOfNotches;
                if (!double.IsNaN(scaleBarBlockWidth))
                    ScaleBarBlock.Width = scaleBarBlockWidth;
            }
        }

		private double getBestEstimateOfValue(double resolution, ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit displayUnit, out ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit unit, out double outResolution)
        {
            unit = displayUnit;
            double rounded = 0;
            double originalRes = resolution;
            while (rounded < 0.5)
            {
                resolution = originalRes;
                if (MapUnit == MapUnit.DecimalDegrees)
                {
                    resolution = getResolutionForGeographic(Map.Extent.GetCenter(), resolution);
					resolution = resolution * (int)ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Meters / (int)unit;
                }
                else if (webMercSref.Equals(Map.SpatialReference))
                {
                    //WebMercator
                    MapPoint center = Map.Extent.GetCenter();
                    center.X = Math.Min(Math.Max(center.Y, -20037508.3427892), 20037508.3427892);
                    MapPoint center2 = merc.ToGeographic(new MapPoint(Math.Min(center.X + getMapResolution(), 20037508.3427892), center.Y)) as MapPoint;
                    center = merc.ToGeographic(center) as MapPoint;
                    resolution = getResolutionForGeographic(center, center2.X - center.X);
					resolution = resolution * (int)ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Meters / (int)unit;
                }
                else if (MapUnit != MapUnit.Undefined)
                {
                    resolution = resolution * (int)MapUnit / (int)unit;
                }

                double val = TargetWidth * resolution;
                val = roundToSignificant(val, resolution);
                double noFrac = Math.Round(val); // to get rid of the fraction
                if (val < 0.5)
                {
					ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit newUnit = ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Undefined;
                    // Automatically switch unit to a lower one
					if (unit == ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Kilometers)
						newUnit = ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Meters;
					else if (unit == ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Miles)
						newUnit = ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Feet;
					if (newUnit == ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Undefined) { break; } //no lower unit
                    unit = newUnit;
                }
                else if (noFrac > 1)
                {
                    rounded = noFrac;
                    var len = noFrac.ToString().Length;
                    if (len <= 2)
                    {
                        // single/double digits ... make it a multiple of 5 ..or 1,2,3,4
                        if (noFrac > 5)
                        {
                            rounded -= noFrac % 5;
                        }
                        while (rounded > 1 && (rounded / resolution) > TargetWidth)
                        {
                            // exceeded maxWidth .. decrement by 1 or by 5
                            double decr = noFrac > 5 ? 5 : 1;
                            rounded = rounded - decr;
                        }
                    }
                    else if (len > 2)
                    {
                        rounded = Math.Round(noFrac / Math.Pow(10, len - 1)) * Math.Pow(10, len - 1);
                        if ((rounded / resolution) > TargetWidth)
                        {
                            // exceeded maxWidth .. use the lower bound instead
                            rounded = Math.Floor(noFrac / Math.Pow(10, len - 1)) * Math.Pow(10, len - 1);
                        }
                    }
                }
                else
                { // anything between 0.5 and 1
                    rounded = Math.Floor(val);
                    if (rounded == 0)
                    {
                        //val >= 0.5 but < 1 so round up
                        rounded = (val == 0.5) ? 0.5 : 1;
                        if ((rounded / resolution) > TargetWidth)
                        {
                            // exceeded maxWidth .. re-try by switching to lower unit 
                            rounded = 0;
							ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit newUnit = ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Undefined;
                            // Automatically switch unit to a lower one
							if (unit == ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Kilometers)
								newUnit = ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Meters;
							else if (unit == ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Miles)
								newUnit = ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Feet;
							if (newUnit == ESRI.ArcGIS.Client.Toolkit.ScaleLine.ScaleLineUnit.Undefined) { break; } //no lower unit
                            unit = newUnit;
                        }
                    }
                }
            }
            outResolution = resolution;
            return rounded;
        }

        double roundToSignificant(double value, double resolution)
        {
            var round = Math.Floor(-Math.Log(resolution));
            if (round > 0)
            {
                round = Math.Pow(10, round);
                return Math.Round(value * round) / round;
            }
            else { return Math.Round(value); }
        }


        private static ESRI.ArcGIS.Client.Projection.WebMercator merc = new ESRI.ArcGIS.Client.Projection.WebMercator();
        private static SpatialReference webMercSref = new SpatialReference(102100);
        private const double toRadians = 0.017453292519943295769236907684886; //Conversion factor from degrees to radians
        private const double earthRadius = 6378137; //Earth radius in meters (defaults to WGS84 / GRS80)
        private const double degreeDist = 111319.49079327357264771338267052; //earthRadius * toRadians;

        /// <summary>
        /// Calculates horizontal scale at center of extent
        /// for geographic / Plate Carrée projection.
        /// Horizontal scale is 0 at the poles.
        /// </summary>
        private double getResolutionForGeographic(ESRI.ArcGIS.Client.Geometry.MapPoint center, double resolution)
        {
            double y = center.Y;
            if (Math.Abs(y) > 90) { return 0; }
            return Math.Cos(y * toRadians) * resolution * degreeDist;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Identifies the Map dependency property.
        /// </summary>
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register(
              "Map",
              typeof(Map),
              typeof(ScaleBar),
              new PropertyMetadata(OnMapPropertyChanged));


        private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Map oldMap = e.OldValue as Map;
            Map newMap = e.NewValue as Map;
            ScaleBar bar = d as ScaleBar;
            if (bar != null)
            {
                if (oldMap != null)
                {
                    oldMap.ExtentChanged -= bar.map_ExtentChanged;
                    oldMap.ExtentChanging -= bar.map_ExtentChanged;
                }
                if (newMap != null)
                {
                    newMap.ExtentChanged += bar.map_ExtentChanged;
                    newMap.ExtentChanging += bar.map_ExtentChanged;
                }
                bar.refreshScalebar();
            }
        }

        /// <summary>
        /// Gets or sets the map that the scale bar is buddied to.
        /// </summary>
        public ESRI.ArcGIS.Client.Map Map
        {
            get
            {
                return GetValue(MapProperty) as Map;
            }
            set
            {
                SetValue(MapProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="TargetWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetWidthProperty = DependencyProperty.Register("TargetWidth", typeof(double), typeof(ScaleBar), null);

        /// <summary>
        /// Gets or sets the target width of the scale bar.
        /// </summary>
        /// <remarks>The actual width of the scale bar changes when values are rounded.</remarks>
        public double TargetWidth
        {
            get { return (double)GetValue(TargetWidthProperty); }
            set { SetValue(TargetWidthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the map unit.
        /// </summary>
        public MapUnit MapUnit { get; set; }

        #endregion

    }
}
