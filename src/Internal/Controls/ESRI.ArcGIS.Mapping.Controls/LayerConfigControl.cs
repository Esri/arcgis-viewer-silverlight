/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows.Controls;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public abstract class LayerConfigControl : Control
    {
        #region public Layer Layer
        /// <summary>
        /// 
        /// </summary>
        public Layer Layer
        {
            get { return GetValue(LayerProperty) as Layer; }
            set { SetValue(LayerProperty, value); }
        }

        /// <summary>
        /// Identifies the Layer dependency property.
        /// </summary>
        public static readonly System.Windows.DependencyProperty LayerProperty =
            System.Windows.DependencyProperty.Register(
                "Layer",
                typeof(Layer),
                typeof(LayerConfigControl),
                new System.Windows.PropertyMetadata(null, OnLayerPropertyChanged));

        /// <summary>
        /// LayerProperty property changed handler.
        /// </summary>
        /// <param name="d">LayerConfigControl that changed its Layer.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnLayerPropertyChanged(System.Windows.DependencyObject d, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            LayerConfigControl source = d as LayerConfigControl;
            source.OnLayerChanged(e);
        }

        protected virtual void OnLayerChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {

        }
        #endregion 
     
    }
}
