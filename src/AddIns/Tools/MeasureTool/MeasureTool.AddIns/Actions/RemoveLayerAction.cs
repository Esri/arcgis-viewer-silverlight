/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Removes a layer from the target map
    /// </summary>
    public class RemoveLayerAction : TargetedTriggerAction<Map>
    {
        protected override void Invoke(object parameter)
        {
            if (Target != null && Layer != null)
            {
                if (Target.Layers.Contains(Layer))
                    Target.Layers.Remove(Layer);
            }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Layer"/> property
        /// </summary>
        public static DependencyProperty LayerProperty = DependencyProperty.Register(
            "Layer", typeof(Layer), typeof(RemoveLayerAction), null);

        /// <summary>
        /// Gets or sets the layer to remove
        /// </summary>
        public Layer Layer
        {
            get { return this.GetValue(LayerProperty) as Layer; }
            set { this.SetValue(LayerProperty, value); }
        }
    }
}
