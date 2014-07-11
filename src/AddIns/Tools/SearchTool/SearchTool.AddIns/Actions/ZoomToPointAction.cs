/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

// ==============================================================
// PRESERVE TO DEMONSTRATE ISSUES WITH ZOOMTORESOLUTION METHOD
// ==============================================================

using System;
using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace SearchTool
{
    /// <summary>
    /// Zooms to a point
    /// </summary>
    public class ZoomToPointAction : TargetedTriggerAction<MapPoint>
    {
        protected override void Invoke(object parameter)
        {
            if (Target != null && Map != null && Map.Layers.Count > 0)
            {
                Layer baseLayer = Map.Layers[0];
                double zoomResolution = -1;
                if (baseLayer is ArcGISTiledMapServiceLayer)
                {
                    // Zoom to maximum level of detail
                    Lod[] lods = ((ArcGISTiledMapServiceLayer)baseLayer).TileInfo.Lods;
                    zoomResolution = lods[lods.Length - 3].Resolution;
                }
                else if (baseLayer.MaximumResolution <= Map.MaximumResolution)
                {
                    // Zoom to base layer's max resolution
                    zoomResolution = baseLayer.MaximumResolution;
                }
                else if (Map.MaximumResolution <= baseLayer.MaximumResolution 
                && !double.IsInfinity(Map.MaximumResolution))
                {
                    // Zoom to map's max resolution
                    zoomResolution = Map.MaximumResolution;
                }


                if (zoomResolution > 0 && Math.Round(Map.Resolution, 8) != Math.Round(zoomResolution, 8))
                {
                    TimeSpan panDuration = Map.PanDuration;
                    TimeSpan zoomDuration = Map.ZoomDuration;
                    Map.PanDuration = TimeSpan.FromTicks(0);
                    Map.ZoomDuration = TimeSpan.FromTicks(0);
                    //Map.ZoomToResolution(zoomResolution);
                    Map.PanTo(Target);
                    Map.PanDuration = panDuration;
                    Map.ZoomDuration = zoomDuration;
                }
                else
                {
                    // Map is already at the desired resolution or the base layer and map's 
                    // max resolution are both positive infinity, so just pan to the point.
                    Map.PanTo(Target);
                }
            }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Map" Property />
        /// </summary>
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register("Map",
            typeof(Map), typeof(ZoomToPointAction), null);

        /// <summary>
        /// Gets or sets the map to zoom
        /// </summary>
        public Map Map
        {
            get { return GetValue(MapProperty) as Map; }
            set { SetValue(MapProperty, value); }
        }

    }
}
