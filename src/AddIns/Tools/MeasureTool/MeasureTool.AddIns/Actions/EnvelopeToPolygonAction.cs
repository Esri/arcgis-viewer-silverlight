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
    /// Converts an <see cref="ESRI.ArcGIS.Client.Geometry.Envelope"/> to a <see cref="ESRI.ArcGIS.Client.Geometry.Polygon"/>
    /// </summary>
    public class EnvelopeToPolygonAction : TriggerAction<DependencyObject>
    {
        protected override void Invoke(object parameter)
        {
            if (Envelope != null)
                Polygon = Envelope.ToPolygon();
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Envelope"/> property
        /// </summary>
        public static DependencyProperty EnvelopeProperty = DependencyProperty.Register(
            "Envelope", typeof(Envelope), typeof(EnvelopeToPolygonAction), null);

        /// <summary>
        /// Gets or sets the Envelope to Convert
        /// </summary>
        public Envelope Envelope
        {
            get { return this.GetValue(EnvelopeProperty) as Envelope; }
            set { this.SetValue(EnvelopeProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Polygon"/> property
        /// </summary>
        public static DependencyProperty PolygonProperty = DependencyProperty.Register(
            "Polygon", typeof(Polygon), typeof(EnvelopeToPolygonAction), null);

        /// <summary>
        /// Gets the converted Polygon
        /// </summary>
        public Polygon Polygon
        {
            get { return this.GetValue(PolygonProperty) as Polygon; }
            private set { this.SetValue(PolygonProperty, value); }
        }
    }
}
