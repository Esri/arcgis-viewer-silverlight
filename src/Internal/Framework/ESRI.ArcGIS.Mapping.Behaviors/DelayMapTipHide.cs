/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Interactivity;
using System.Collections.Specialized;
using System.Collections;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Behaviors
{
    public class DelayMapTipHide : Behavior<Map>
    {
        public static readonly DependencyProperty HideDelayProperty = DependencyProperty.Register(
            "HideDelay", typeof(TimeSpan), typeof(DelayMapTipHide),
            new PropertyMetadata(TimeSpan.FromSeconds(0.5), OnHideDelayPropertyChanged));

        /// <summary>
        /// MapTip hide delay
        /// </summary>
        public TimeSpan HideDelay 
        {
            get { return (TimeSpan)GetValue(HideDelayProperty); }
            set { SetValue(HideDelayProperty, value); } 
        }

        private static void OnHideDelayPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            DelayMapTipHide delayBehavior = (DelayMapTipHide)d;
            if (delayBehavior == null)
                return;
            if (delayBehavior.AssociatedObject != null)
                setHideDelay(delayBehavior.AssociatedObject.Layers, delayBehavior.HideDelay);
        }


        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject == null || AssociatedObject.Layers == null)
                return;

            // Wire layer collection changed handler to monitor adding/removal of GraphicsLayers
            AssociatedObject.Layers.CollectionChanged += Layers_CollectionChanged;

            setHideDelay(AssociatedObject.Layers, HideDelay);
        }

        // Set the specified maptip hide delay on GraphicsLayers
        private static void setHideDelay(IList layers, TimeSpan hideDelay)
        {
            if (layers == null)
                return;
            foreach (Layer layer in layers)
            {
                GraphicsLayer graphicsLayer = layer as GraphicsLayer;
                if(graphicsLayer != null && graphicsLayer.MapTip != null)
                    graphicsLayer.MapTip.SetValue(GraphicsLayer.MapTipHideDelayProperty, hideDelay);
            }
        }

        // set hide delay for added GraphicsLayers
        void Layers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
                setHideDelay(e.NewItems, HideDelay);
        }
    }
}
