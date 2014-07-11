/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Creates a <see cref="ESRI.ArcGIS.Client.Graphic"/> containing the specified <see cref="ESRI.ArcGIS.Client.Geometry.Geometry"/>
    /// </summary>
    public class GeometryToGraphicAction : TriggerAction<DependencyObject>
    {
        protected override void Invoke(object parameter)
        {
            if (this.Geometry != null)
                this.Graphic = new Graphic() { Geometry = this.Geometry };
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Geometry"/> property
        /// </summary>
        public static DependencyProperty GeometryProperty = DependencyProperty.Register(
            "Geometry", typeof(Geometry), typeof(GeometryToGraphicAction), null);

        /// <summary>
        /// Gets or sets the geometry to use when creating a graphic
        /// </summary>
        public Geometry Geometry
        {
            get { return this.GetValue(GeometryProperty) as Geometry; }
            set { this.SetValue(GeometryProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Graphic"/> property
        /// </summary>
        public static DependencyProperty GraphicProperty = DependencyProperty.Register(
            "Graphic", typeof(Graphic), typeof(GeometryToGraphicAction), null);

        /// <summary>
        /// Gets the created graphic
        /// </summary>
        public Graphic Graphic
        {
            get { return this.GetValue(GraphicProperty) as Graphic; }
            private set { this.SetValue(GraphicProperty, value); }
        }
    }
}
