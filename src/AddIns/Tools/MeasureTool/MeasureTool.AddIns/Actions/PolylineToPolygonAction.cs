/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client.Geometry;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Converts a <see cref="ESRI.ArcGIS.Client.Geometry.Polyline"/> to a <see cref="ESRI.ArcGIS.Client.Geometry.Polygon"/>
    /// </summary>
    public class PolylineToPolygonAction : TriggerAction<DependencyObject>
    {
        protected override void Invoke(object parameter)
        {
            if (Polyline != null)
                Polygon = Polyline.ToPolygon();
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Polyline"/> property
        /// </summary>
        public static DependencyProperty PolylineProperty = DependencyProperty.Register(
            "Polyline", typeof(Polyline), typeof(PolylineToPolygonAction), null);

        /// <summary>
        /// Gets or sets the polyline to be converted
        /// </summary>
        public Polyline Polyline
        {
            get { return this.GetValue(PolylineProperty) as Polyline; }
            set { this.SetValue(PolylineProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Polygon"/> property
        /// </summary>
        public static DependencyProperty PolygonProperty = DependencyProperty.Register(
            "Polygon", typeof(Polygon), typeof(PolylineToPolygonAction), null);

        /// <summary>
        /// Gets the converted polygon
        /// </summary>
        public Polygon Polygon
        {
            get { return this.GetValue(PolygonProperty) as Polygon; }
            private set { this.SetValue(PolygonProperty, value); }
        }
    }
}
