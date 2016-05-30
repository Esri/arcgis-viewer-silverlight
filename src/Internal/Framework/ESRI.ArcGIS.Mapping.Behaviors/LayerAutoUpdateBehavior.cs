/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Behaviors
{
    public class LayerAutoUpdateBehavior : Behavior<Map>
    {
        /// <summary>
        /// Dictionary to find AutoUpdate helper objects associated to each layer
        /// </summary>
        private readonly Dictionary<Layer, LayerAutoUpdateHelper> _autoUpdateHelpersDict =
            new Dictionary<Layer, LayerAutoUpdateHelper>();

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject == null || AssociatedObject.Layers == null)
                return;

            // Listen to when Layers are added/removed
            AssociatedObject.Layers.CollectionChanged += LayersCollectionChanged;

            // Listen to when the Map extent changes
            AssociatedObject.ExtentChanged += OnMapExtentChanged;

            List<Layer> filteredList = GetMapFilteredLayers();
            SetLayerAutoUpdate(filteredList);
        }

        void OnMapExtentChanged(object sender, ExtentEventArgs e)
        {
            // Run thru all the layers being tracked, and if any have autoUpdate turned
            // on and the update when map extent changes event turned on, update the
            // layer immediately.
            foreach (var helper in _autoUpdateHelpersDict.Values)
            {
                Layer layer = helper.FilteredLayer;
                if (layer != null && LayerExtensions.GetAutoUpdateOnExtentChanged(layer))
                {
                    helper.UpdateNow = true;
                    // The helper filters the events with a timer so that we only have one 
                    // update for a series of extent changed events within a short time.
                    helper.SettingsHaveChanged();
                }
            }
        }

        /// <summary>
        /// This method should be called whenever the map layers collection changes.
        /// </summary>
        /// <param name="layers">List of existing FeatureLayers/DynamicLayers in Map.  It is normal to not have any.</param>
        private void SetLayerAutoUpdate(List<Layer> layers)
        {
            // Each time this method is called, the layers collection represents
            // the entire set of FeatureLayers/DynamicLayers being used in the Map.  Since it
            // may be different from the last time, we may have cleanup to do.
            if (layers == null || layers.Count <= 0)
            {
                _autoUpdateHelpersDict.Clear();
                return;
            }

            // find any helpers that have layers not in the layers collection
            List<LayerAutoUpdateHelper> toRemove =
                _autoUpdateHelpersDict.Values.Where(item => !layers.Contains(item.FilteredLayer)).ToList();
            foreach (LayerAutoUpdateHelper helper in toRemove)
            {
                _autoUpdateHelpersDict.Remove(helper.ResetRefresh());
            }
            // update helpers for existing layers
            foreach (Layer layer in layers)
            {
                UpdateLayerSettings(layer);
            }
        }

        Dictionary<Layer, DependencyProperty> autoUpdateIntervalProps = new Dictionary<Layer,DependencyProperty>();
        Dictionary<Layer, DependencyProperty> visibleProps = new Dictionary<Layer, DependencyProperty>();
        private void UpdateLayerSettings(Layer layer)
        {
            if (layer == null || !IsLayerAutoUpdateCapable(layer))
                return;

            // find the helper that manages the auto-update for this layer
            LayerAutoUpdateHelper helper = null;
            _autoUpdateHelpersDict.TryGetValue(layer, out helper);

            if (helper == null)
            {
                // All FeatureLayers need a helper created for them in case the user
                // chooses to enable the layer's auto-update at a later time
                helper = new LayerAutoUpdateHelper { FilteredLayer = layer, ParentBehavior = this, IsEnabled = layer.Visible };
                // Add to lookup dictionary
                _autoUpdateHelpersDict.Add(layer, helper);

                //clear all listeners and their event subscriptions
                ClearPropertyListeners(layer);
                // Set the layer's property changed delegate to point to our method
                DependencyProperty listener =  Utility.NotifyOnAttachedPropertyChanged("AutoUpdateInterval", layer, OnLayerPropertyChanged, LayerExtensions.GetAutoUpdateInterval(layer));
                if (!autoUpdateIntervalProps.ContainsKey(layer))
                    autoUpdateIntervalProps.Add(layer, listener);
                else
                    autoUpdateIntervalProps[layer] = listener;
                
                // We also want to know when the layer visibility is changed so that we can
                // stop the refresh timers
                listener = Utility.NotifyOnAttachedPropertyChanged("Visible", layer, OnLayerVisibilityChanged, layer.Visible);
                if (!visibleProps.ContainsKey(layer))
                    visibleProps.Add(layer, listener);
                else
                    visibleProps[layer] = listener;
            }
            helper.SettingsHaveChanged();
        }

        /// <summary>
        /// Callback method for when the layer visibility is changed (ex: when the user unchecks the layer
        /// checkbox in Map Contents.
        /// </summary>
        public void OnLayerVisibilityChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var layer = o as Layer;
            if (layer == null) return;

            LayerAutoUpdateHelper helper = null;
            _autoUpdateHelpersDict.TryGetValue(layer, out helper);
            if (helper != null)
            {
                helper.IsEnabled = layer.Visible;
            }
        }

        /// <summary>
        /// Callback method for the LayerExtensions Auto-Update properties so that they can
        /// notify us when the properties changes for a given layer.
        /// </summary>
        public void OnLayerPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            UpdateLayerSettings(o as Layer);
        }

        // set hide delay for added GraphicsLayers
        private void LayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                // stop all auto-updates for layers that have been removed from the View's collection
                foreach (var layer in e.OldItems)
                {
                    if (layer is GraphicsLayer && LayerExtensions.GetAutoUpdateInterval((GraphicsLayer) layer) > 0)
                        LayerExtensions.SetAutoUpdateInterval((GraphicsLayer) layer, 0);
                }
            }

            if (AssociatedObject == null || AssociatedObject.Layers == null || AssociatedObject.Layers.Count <= 0)
                return;

            // we don't care what type of change happened to the layers collection, we only want 
            // the current existing FeatureLayers/DynamicLayers in the Map
            List<Layer> filteredList = GetMapFilteredLayers();

            SetLayerAutoUpdate(filteredList);
        }

        internal bool MapContainsLayer(Layer layer)
        {
            return (layer != null && AssociatedObject != null && AssociatedObject.Layers != null && AssociatedObject.Layers.Contains(layer));
        }

        private List<Layer> GetMapFilteredLayers()
        {
            var filteredList = new List<Layer>();

            // FeatureLayer
            List<FeatureLayer> featureLayers = AssociatedObject.Layers.OfType<FeatureLayer>().ToList();
            if (featureLayers.Count > 0)
                filteredList.AddRange(featureLayers.Select(f => f as Layer));

            // ArcGISDynamicMapServiceLayer
            List<ArcGISDynamicMapServiceLayer> dynamicLayers = AssociatedObject.Layers.OfType<ArcGISDynamicMapServiceLayer>().ToList();
            if (dynamicLayers.Count > 0)
                filteredList.AddRange(dynamicLayers.Select(f => f as Layer));

            // ICustomGraphicsLayer
            List<ICustomGraphicsLayer> customLayers = AssociatedObject.Layers.OfType<ICustomGraphicsLayer>().ToList();
            if (customLayers.Count > 0)
                filteredList.AddRange(customLayers.Select(f => f as Layer));

            return filteredList;
        }

        public static bool IsLayerAutoUpdateCapable(Layer layer)
        {
            return (layer != null && (layer is FeatureLayer && 
                (!string.IsNullOrEmpty(((FeatureLayer)layer).Url)))
                || layer is ArcGISDynamicMapServiceLayer || layer is ICustomGraphicsLayer);
        }

        void ClearPropertyListeners(Layer layer)
        {
            if (autoUpdateIntervalProps.ContainsKey(layer))
            {
                Utility.ClearAttachedPropertyListener(autoUpdateIntervalProps[layer], layer);
                autoUpdateIntervalProps.Remove(layer);
            }
            if (visibleProps.ContainsKey(layer))
            {
                Utility.ClearAttachedPropertyListener(visibleProps[layer], layer);
                visibleProps.Remove(layer);
            }
        }
    }
}
