/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Behaviors
{
    public class LayersInitializedBehavior : Behavior<Map>
    {
        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
        protected override void OnAttached()
        {
            base.OnAttached();
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.Loaded += new RoutedEventHandler(AssociatedObject_Loaded);
            }
        }

        void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.AssociatedObject == null || this.AssociatedObject.Layers == null)
                return;
            this.AssociatedObject.Layers.LayersInitialized += Layers_LayersInitialized;
        }

        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            if (ApplicationHelper.CurrentApplication != null)
                ApplicationHelper.CurrentApplication.PublishNotificationEvent(NotificationEvent.LayersInitialized, sender as Layer);
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        /// <remarks>Override this to unhook functionality from the AssociatedObject.</remarks>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.Layers.LayersInitialized -= Layers_LayersInitialized;
        }
    }
}
