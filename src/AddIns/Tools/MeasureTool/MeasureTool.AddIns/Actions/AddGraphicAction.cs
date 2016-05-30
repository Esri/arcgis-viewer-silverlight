/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using System.Collections.Generic;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Adds a <see cref="ESRI.ArcGIS.Client.Graphic"/> to the target <see cref="ESRI.ArcGIS.Client.GraphicsLayer"/>
    /// </summary>
    public class AddGraphicAction : TargetedTriggerAction<GraphicsLayer>
    {
        protected override void Invoke(object parameter)
        {
            if ((Target != null) && (Graphic != null))
                Target.Graphics.Add(this.Graphic);
        }

        #region Dependency Properties

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Graphic"/> property
        /// </summary>
        public static DependencyProperty GraphicProperty = DependencyProperty.Register(
            "Graphic", typeof(Graphic), typeof(AddGraphicAction), null);

        /// <summary>
        /// Gets or sets the Graphic to add
        /// </summary>
        public Graphic Graphic
        {
            get { return GetValue(GraphicProperty) as Graphic; }
            set { SetValue(GraphicProperty, value); }
        }

        #endregion Dependency Properties
    }
}
