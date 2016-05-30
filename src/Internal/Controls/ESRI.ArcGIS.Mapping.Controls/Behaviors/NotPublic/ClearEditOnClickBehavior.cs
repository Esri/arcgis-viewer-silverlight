/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using ESRI.ArcGIS.Client;
using System.Windows.Threading;
using System.Windows.Interactivity;
using System.Collections.Specialized;
using ESRI.ArcGIS.Mapping.Controls.Utils;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ClearEditOnClickBehavior : Behavior<Map>
    {
        private DispatcherTimer _doubleClickTimer;
        private bool _wasDoubleClick;

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject == null || AssociatedObject.Layers == null)
                return;

            // map events
            AssociatedObject.MouseClick += map_MouseClick;
            AssociatedObject.ExtentChanging += map_ExtentChanging;

            // Wire layer collection changed handler to monitor adding/removal of GraphicsLayers
            AssociatedObject.Layers.CollectionChanged += Layers_CollectionChanged;

            foreach (Layer layer in AssociatedObject.Layers)
                wireHandlers(layer as GraphicsLayer);
            _doubleClickTimer = new DispatcherTimer();
            _doubleClickTimer.Interval = new TimeSpan(0, 0, 0, 0, 510);
            _doubleClickTimer.Tick += doubleClickTimer_Tick;
        }

        private void map_MouseClick(object sender, Map.MouseEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
                    {
                        if (e.Handled) return;
                        _wasDoubleClick = false;
                        _doubleClickTimer.Start();
                    }
                );
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject == null || AssociatedObject.Layers == null)
                return;

            foreach (Layer layer in AssociatedObject.Layers)
                removeHandlers(layer as GraphicsLayer);

            AssociatedObject.Layers.CollectionChanged -= Layers_CollectionChanged;
            if (_doubleClickTimer != null)
            {
                _doubleClickTimer.Stop();
                _doubleClickTimer.Tick -= doubleClickTimer_Tick;
            }
        }

        // Add/remove MapTip positioning handlers for added/removed GraphicsLayers
        private void Layers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Layer layer in e.NewItems)
                    wireHandlers(layer as GraphicsLayer);
            }

            if (e.OldItems != null)
            {
                foreach (Layer layer in e.OldItems)
                    removeHandlers(layer as GraphicsLayer);
            }
        }

        private void doubleClickTimer_Tick(object sender, EventArgs e)
        {
            _doubleClickTimer.Stop();
            if (!_wasDoubleClick)
                stopEditingAndSave();
        }

        private void stopEditingAndSave()
        {
            // stop editing a shape if it is in process
            EditorCommandUtility.StopEditing();
        }

        private void map_ExtentChanging(object sender, ExtentEventArgs e)
        {
            _wasDoubleClick = true;
        }

        private void graphicsLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
                                       {
                                           if (e.Handled) return;
                                           _wasDoubleClick = false;
                                           _doubleClickTimer.Start();
                                       }
                );
        }

        private void wireHandlers(GraphicsLayer graphicsLayer)
        {
            if (graphicsLayer == null) return;

            // Add handlers to get the position of the mouse on the map
            graphicsLayer.MouseLeftButtonDown += graphicsLayer_MouseLeftButtonDown;
        }

        private void removeHandlers(GraphicsLayer graphicsLayer)
        {
            if (graphicsLayer == null) return;
            graphicsLayer.MouseLeftButtonDown += graphicsLayer_MouseLeftButtonDown;
        }
    }
}
