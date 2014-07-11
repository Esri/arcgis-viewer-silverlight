/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Projection;
using ESRI.ArcGIS.Client.Extensibility;
using MeasureTool.Addins.Resources;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Provides extension methods relevant to the Measure add-in
    /// </summary>
    internal static class Extensions
    {
        private static WebMercator _webMercator = new WebMercator();

        /// <summary>
        /// Returns the description of the enumerated value, if one is defined, or a string representation of the value
        /// </summary>
        internal static string GetDescription(this Enum e)
        {
            FieldInfo fi = e.GetType().GetField(e.ToString());
            return Utils.GetEnumDescription(fi);
        }

        /// <summary>
        /// Gets the index of an object in an array
        /// </summary>
        /// <param name="array">The array containing the object</param>
        /// <param name="obj">The object to get the index of</param>
        /// <returns>The index of the object if it exists in the array, -1 otherwise</returns>
        internal static int IndexOf(this object[] array, object obj)
        {
            int index = -1;
            if (obj == null)
                return index;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(obj))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        /// <summary>
        /// Checkes whether a sub-layer (LayerInfo) is in a measurable state
        /// </summary>
        /// <param name="subLayer">The sub-layer to check</param>
        /// <param name="parentLayer">The map service layer containing the sub-layer</param>
        /// <param name="map">The map containing the map service layer</param>
        /// <returns>True if the sub-layer is measurable, false if not</returns>
        internal static bool IsMeasurable(this LayerInfo subLayer, ArcGISDynamicMapServiceLayer parentLayer, Map map)
        {
            if (parentLayer.VisibleLayers == null)
                return subLayer.DefaultVisibility && subLayer.IsInVisibleRange(map);
            else
                return parentLayer.VisibleLayers.Contains(subLayer.ID) && subLayer.IsInVisibleRange(map);
        }

        /// <summary>
        /// Checks whether a layer is in a measurable state
        /// </summary>
        /// <param name="layer">The layer to check</param>
        /// <param name="map">The map containing the layer</param>
        /// <returns></returns>
        internal static bool IsMeasurable(this Layer layer, Map map)
        {
            // Check whether the layer is visible, of a measurable type, has a defined layer name, and is not
            // hidden due to scale dependency
            bool measurable = layer.Visible && ((layer is ArcGISDynamicMapServiceLayer) || (layer is GraphicsLayer) 
                || (layer is FeatureLayer)) && !string.IsNullOrEmpty(MapApplication.GetLayerName(layer)) 
                && layer.IsInVisibleRange(map);

            // For map service layers, make sure at least one sub-layer is visible
            if (measurable && layer is ArcGISDynamicMapServiceLayer) 
            {
                ArcGISDynamicMapServiceLayer dynLayer = (ArcGISDynamicMapServiceLayer)layer;
                // Check sub-layer visibility directly and as defined by the sub-layer's scale dependency
                measurable = dynLayer.Layers.Any(subLayer => dynLayer.GetLayerVisibility(subLayer.ID)
                    && subLayer.IsInVisibleRange(map));
            }
            
            return measurable;
        }

        /// <summary>
        /// Gets whether a sub-layer is visible in the map based on the sub-layer's scale dependency
        /// and the map's current scale
        /// </summary>
        internal static bool IsInVisibleRange(this LayerInfo subLayer, Map map)
        {
            if (map == null)
                return true; // No map, so scale dependency is N/A

            bool isBeyondMaxScale = !double.IsInfinity(subLayer.MaxScale) && subLayer.MaxScale > map.Scale;
            bool isBeyondMinScale = subLayer.MinScale > 0 && subLayer.MinScale < map.Scale;
            return !isBeyondMaxScale && !isBeyondMinScale;
        }

        /// <summary>
        /// Gets whether a layer is visible in the map based on the layer's scale dependency
        /// and the map's current scale
        /// </summary>
        internal static bool IsInVisibleRange(this Layer layer, Map map)
        {
            if (map == null)
                return true; // No map, so scale dependency is N/A

            bool isBeyondMaxScale = layer.MaximumResolution > 0 && layer.MaximumResolution < map.Resolution;
            bool isBeyondMinScale = layer.MinimumResolution > 0 && layer.MinimumResolution > map.Resolution;
            if (!isBeyondMaxScale && !isBeyondMinScale && layer is FeatureLayer)
            {
                FeatureLayer fLayer = (FeatureLayer)layer;
                isBeyondMaxScale = !double.IsInfinity(fLayer.LayerInfo.MaximumScale) && fLayer.LayerInfo.MaximumScale > map.Scale;
                isBeyondMinScale = fLayer.LayerInfo.MinimumScale > 0 && fLayer.LayerInfo.MinimumScale < map.Scale;
            }
            return !isBeyondMaxScale && !isBeyondMinScale;
        }

        /// <summary>
        /// Gets the measurable sub-layers within a map service layer
        /// </summary>
        /// <param name="dynLayer">The <see cref="ArcGISDynamicMapServiceLayer"/> containing the sub-layers</param>
        /// <param name="map">The <see cref="Map"/> containing the map service layer</param>
        internal static ObservableCollection<LayerInfo> GetMeasurableSubLayers(
            this ArcGISDynamicMapServiceLayer dynLayer, Map map)
        {
            ObservableCollection<LayerInfo> subLayers = new ObservableCollection<LayerInfo>();
            if (dynLayer.VisibleLayers == null) // sub-layer visibility has not been altered from its default state
            {
                // Add layer to measurable layers if it is visible by default and is not hidden due to 
                // scale dependency
                foreach (LayerInfo info in dynLayer.Layers)
                {
                    if (info.DefaultVisibility && info.IsInVisibleRange(map))
                        subLayers.Add(info);
                }
            }
            else // sub-layer visibility is not default
            {
                // Add layer to measurable layers if it is currently visible is not hidden due to scale dependency
                foreach (int visibleLayerIndex in dynLayer.VisibleLayers)
                {
                    if (dynLayer.Layers[visibleLayerIndex].IsInVisibleRange(map))
                        subLayers.Add(dynLayer.Layers[visibleLayerIndex]);
                }
            }

            return subLayers;
        }

        /// <summary>
        /// Applies a function to transform each point in a geometry
        /// </summary>
        /// <param name="g">The geometry containing the points to transform</param>
        /// <param name="pointTransform">The function to apply to each point</param>
        internal static Geometry TransformPoints(this Geometry g, Func<MapPoint, MapPoint> pointTransform)
        {
            Geometry outputGeometry = null;
            if (g is MapPoint)
            {
                outputGeometry = pointTransform((MapPoint)g);
            }
            else if (g is Polyline)
            {
                Polyline transformedLine = new Polyline();
                Polyline inputLine = (Polyline)g;
                foreach (PointCollection pc in inputLine.Paths)
                {
                    PointCollection transformedPoints = new PointCollection();
                    foreach (MapPoint p in pc)
                        transformedPoints.Add(pointTransform(p));

                    transformedLine.Paths.Add(transformedPoints);
                }

                outputGeometry = transformedLine;
            }
            else if (g is Polygon)
            {
                Polygon transformedPoly = new Polygon();
                Polygon inputPoly = (Polygon)g;
                foreach (PointCollection pc in inputPoly.Rings)
                {
                    PointCollection transformedPoints = new PointCollection();
                    foreach (MapPoint p in pc)
                        transformedPoints.Add(pointTransform(p));

                    transformedPoly.Rings.Add(transformedPoints);
                }

                outputGeometry = transformedPoly;
            }

            return outputGeometry;
        }

        /// <summary>
        /// Gets whether a spatial reference instance is in a Web Mercator projection
        /// </summary>
        /// <param name="sref"></param>
        /// <returns></returns>
        internal static bool IsWebMercator(this SpatialReference sref)
        {
            return sref != null && sref.WKID == 3857 || sref.WKID == 102113 || sref.WKID == 102100;
        }

        /// <summary>
        /// Creates a clone of a <see cref="ESRI.ArcGIS.Client.Geometry.PointCollection"/>
        /// </summary>
        /// <param name="points">The PointCollection to be cloned</param>
        /// <returns>A copy of the PointCollection</returns>
        internal static PointCollection Clone(this PointCollection points)
        {
            PointCollection clone = new PointCollection();
            foreach (MapPoint p in points)
                clone.Add(p.Clone());
            return clone;
        }

        /// <summary>
        /// Converts an Envelope to a Polygon
        /// </summary>
        internal static Polygon ToPolygon(this Envelope env)
        {
            PointCollection points = new PointCollection();
            points.Add(new MapPoint(env.XMin, env.YMax) { SpatialReference = env.SpatialReference });
            points.Add(new MapPoint(env.XMax, env.YMax) { SpatialReference = env.SpatialReference });
            points.Add(new MapPoint(env.XMax, env.YMin) { SpatialReference = env.SpatialReference });
            points.Add(new MapPoint(env.XMin, env.YMin) { SpatialReference = env.SpatialReference });
            points.Add(new MapPoint(env.XMin, env.YMax) { SpatialReference = env.SpatialReference });

            Polygon polygon = new Polygon() { SpatialReference = env.SpatialReference };
            polygon.Rings.Add(points);
            return polygon;
        }

        /// <summary>
        /// Converts a <see cref="ESRI.ArcGIS.Client.Geometry.Polyline"/> to a 
        /// <see cref="ESRI.ArcGIS.Client.Geometry.Polygon"/>
        /// </summary>
        internal static Polygon ToPolygon(this Polyline polyline)
        {
            Polygon polygon = new Polygon() { SpatialReference = polyline.SpatialReference };
            foreach (PointCollection path in polyline.Paths)
            {
                // Clone the path to be a ring of the polygon
                PointCollection ring = path.Clone();

                // Close the ring if it is not already
                if (ring[0].X != ring[ring.Count - 1].X ||
                ring[0].Y != ring[ring.Count - 1].Y)
                    ring.Add(ring[0].Clone());

                // Add the ring to the polygon
                polygon.Rings.Add(ring);
            }
            return polygon;
        }

        /// <summary>
        /// Retrieves a point on a map corresponding to a mouse event
        /// </summary>
        /// <param name="e">The mouse event</param>
        /// <param name="map">The map to determine the point for</param>
        internal static MapPoint GetMapPoint(this MouseEventArgs e, Map map)
        {
            Point screenPoint = e.GetPosition(map);
            MapPoint mapPoint = map.ScreenToMap(screenPoint);
            mapPoint.SpatialReference = map.SpatialReference;
            return mapPoint;
        }

        /// <summary>
        /// Calculates the area of a <see cref="ESRI.ArcGIS.Client.Geometry.Envelope"/>
        /// </summary>
        internal static double Area(this Envelope env)
        {
            return env.ToPolygon().Area();
        }

        /// <summary>
        /// Calculates the area of a <see cref="ESRI.ArcGIS.Client.Geometry.Polygon"/>
        /// </summary>
        internal static double Area(this Polygon polygon)
        {
            polygon = polygon.PrepareForClientOps();

            // Due to bug with calling Geodesic.Area on multi-ring polygons, create a polygon
            // from each ring in the original polygon and sum up the area of each ring
            double area = 0;
            Polygon subPoly = new Polygon() { SpatialReference = polygon.SpatialReference };
            foreach (PointCollection ring in polygon.Rings)
            {                
                subPoly.Rings.Add(ring.Clone());
                area += Math.Abs(Geodesic.Area(subPoly));
                subPoly.Rings.Clear();
            }

            return area;
        }

        /// <summary>
        /// Calculates the perimeter of a <see cref="ESRI.ArcGIS.Client.Geometry.Envelope"/>
        /// </summary>
        internal static double Perimeter(this Envelope env)
        {
            return env.ToPolygon().Perimeter();
        }

        /// <summary>
        /// Calculates the perimeter of a <see cref="ESRI.ArcGIS.Client.Geometry.Polygon"/>
        /// </summary>
        internal static double Perimeter(this Polygon polygon)
        {
            polygon = polygon.PrepareForClientOps();
            return Geodesic.Length(polygon);
        }

        /// <summary>
        /// Calculates the length of a <see cref="ESRI.ArcGIS.Client.Geometry.Polyline"/>
        /// </summary>
        internal static double Length(this Polyline polyline)
        {
            polyline = polyline.PrepareForClientOps();
            return Geodesic.Length(polyline);
        }

        /// <summary>
        /// Checks whether geometric operations can be performed client-side for a 
        /// <see cref="ESRI.ArcGIS.Client.Geometry.Geometry"/>
        /// </summary>
        internal static bool SupportsClientGeometricOps(this Geometry geometry)
        {
            return geometry.SpatialReference != null &&
                (geometry.SpatialReference.IsWebMercator() || 
                geometry.SpatialReference.WKID == 4326);
        }

        /// <summary>
        /// Prepares a <see cref="ESRI.ArcGIS.Client.Geometry.Polygon"/> instance for 
        /// client-side geometric operations
        /// </summary>
        internal static Polygon PrepareForClientOps(this Polygon polygon)
        {
            validateProjectionForClientOps(polygon);

            Euclidian.Densify(polygon, 10000);

            if (polygon.SpatialReference.IsWebMercator())
                polygon = (Polygon)_webMercator.ToGeographic(polygon);

            polygon = (Polygon)Geometry.NormalizeCentralMeridian(polygon);

            return polygon;
        }

        /// <summary>
        /// Prepares a <see cref="ESRI.ArcGIS.Client.Geometry.Polyline"/> instance for 
        /// client-side geometric operations
        /// </summary>
        internal static Polyline PrepareForClientOps(this Polyline polyline)
        {
            validateProjectionForClientOps(polyline);

            Euclidian.Densify(polyline, 10000);

            if (polyline.SpatialReference.IsWebMercator())
                polyline = (Polyline)_webMercator.ToGeographic(polyline);

            polyline = (Polyline)Geometry.NormalizeCentralMeridian(polyline);

            return polyline;
        }

        /// <summary>
        /// Creates a deep clone of a <see cref="ESRI.ArcGIS.Client.Geometry.Geometry"/> instance
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        internal static Geometry Clone(this Geometry geometry)
        {
            Geometry clone = null;
            if (geometry is MapPoint)
                clone = ((MapPoint)geometry).Clone();
            else if (geometry is MultiPoint)
                clone = ((MultiPoint)geometry).Clone();
            else if (geometry is Polyline)
                clone = ((Polyline)geometry).Clone();
            else if (geometry is Polygon)
                clone = ((Polygon)geometry).Clone();
            else if (geometry is Envelope)
                clone = ((Envelope)geometry).Clone();
            return clone;
        }

        private static void validateProjectionForClientOps(Geometry geometry)
        {
            if (!geometry.SupportsClientGeometricOps())
                throw new NotSupportedException(Strings.NonClientSpatialRef);
        }
    }
}
