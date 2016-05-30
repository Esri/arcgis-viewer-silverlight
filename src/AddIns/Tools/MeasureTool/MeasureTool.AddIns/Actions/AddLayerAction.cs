/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Adds a layer to the targeted map
    /// </summary>
    public class AddLayerAction : TargetedTriggerAction<Map>
    {
        protected override void Invoke(object parameter)
        {
            if (Target != null && Layer != null && !Target.Layers.Contains(Layer))
                Target.Layers.Add(Layer);
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Layer"/> property
        /// </summary>
        public static DependencyProperty LayerProperty = DependencyProperty.Register(
            "Layer", typeof(Layer), typeof(AddLayerAction), null);

        /// <summary>
        /// Gets or sets the layer to add
        /// </summary>
        public Layer Layer
        {
            get { return this.GetValue(LayerProperty) as Layer; }
            set { this.SetValue(LayerProperty, value); }
        }
    }
}
