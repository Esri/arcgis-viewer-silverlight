/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;
using System.Diagnostics;

namespace ESRI.ArcGIS.Mapping.Behaviors
{
    public class PositionMapTip : Behavior<Map>
    {
        private Point _mousePos; // Track the position of the mouse on the Map        

        /// <summary>
        /// Distance between the MapTip and the boundary of the map
        /// </summary>
        public double Margin { get; set; }

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject == null || this.AssociatedObject.Layers == null)
                return;

            // Wire layer collection changed handler to monitor adding/removal of GraphicsLayers
            this.AssociatedObject.Layers.CollectionChanged += Layers_CollectionChanged;

            foreach (Layer layer in this.AssociatedObject.Layers)
                wireHandlers(layer as GraphicsLayer);
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        /// <remarks>Override this to unhook functionality from the AssociatedObject.</remarks>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject == null || this.AssociatedObject.Layers == null)
                return;

            foreach (Layer layer in this.AssociatedObject.Layers)
                removeHandlers(layer as GraphicsLayer);

            this.AssociatedObject.Layers.CollectionChanged -= Layers_CollectionChanged;
        }

        // Add/remove MapTip positioning handlers for added/removed GraphicsLayers
        void Layers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

        // Add MapTip positioning handlers
        private void wireHandlers(GraphicsLayer graphicsLayer)
        {
            if (graphicsLayer == null) return;

            // Add handlers to get the position of the mouse on the map
            graphicsLayer.MouseEnter += GraphicsLayer_MouseEnterOrMove;
            graphicsLayer.MouseMove += GraphicsLayer_MouseEnterOrMove;
            //graphicsLayer.MouseLeave += graphicsLayer_MouseLeave;
        }

        //void graphicsLayer_MouseLeave(object sender, GraphicMouseEventArgs e)
        //{
        //   GraphicsLayer graphicsLayer = sender as GraphicsLayer;
        //    Debug.WriteLine(string.Format("@@MouseLeave, {0}", MapApplication.GetLayerName(graphicsLayer)));
        //}

        // Remove MapTip positioning handlers
        private void removeHandlers(GraphicsLayer graphicsLayer)
        {
            if (graphicsLayer == null) return;
            graphicsLayer.MouseEnter -= GraphicsLayer_MouseEnterOrMove;
            graphicsLayer.MouseMove -= GraphicsLayer_MouseEnterOrMove;

            if (graphicsLayer.MapTip is HoverResults)
                graphicsLayer.MapTip.SizeChanged -= MapTip_SizeChanged;
        }

        // Get mouse position
        void GraphicsLayer_MouseEnterOrMove(object sender, GraphicMouseEventArgs args)
        {
            _mousePos = args.GetPosition(AssociatedObject);
            GraphicsLayer graphicsLayer = sender as GraphicsLayer;
#if DEBUG
            if (args.Graphic != null)
            {
                if (args.Graphic.Symbol is ESRI.ArcGIS.Client.Clustering.FlareSymbol)
                    Debug.WriteLine(string.Format("@@MouseEnter/Move, flare, {0}", MapApplication.GetLayerName(graphicsLayer)));
                else if (args.Graphic.Attributes.ContainsKey("NAME"))
                    Debug.WriteLine(string.Format("@@MouseEnter/Move, {0}, {1}", args.Graphic.Attributes["NAME"], MapApplication.GetLayerName(graphicsLayer)));
                else
                    Debug.WriteLine(string.Format("@@MouseEnter/Move, no NAME field, {0}", MapApplication.GetLayerName(graphicsLayer)));
            }
#endif
            if (ESRI.ArcGIS.Client.Extensibility.LayerProperties.GetIsPopupEnabled(graphicsLayer))
            {
                if (!ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetPopUpsOnClick(graphicsLayer))
                {
                    HoverResults hoverResults = graphicsLayer.MapTip as HoverResults;
                    if (graphicsLayer.MapTip == null) //if not a custom map tip
                    {
                        //if first time, attach map tip to layer
                        attachApplicationMapTipToLayer(graphicsLayer, args.Graphic);
                        hoverResults = graphicsLayer.MapTip as HoverResults;
                    }

                    //if map tip dirty, rebuild map tip
                    if (hoverResults != null)
                    {
                        if (ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetIsMapTipDirty(graphicsLayer) &&
                                 args.Graphic == hoverResults.Graphic)
                            rebuildMapTipContentsBasedOnFieldVisibility(hoverResults);
                        //clear map tip dirty flag
                        ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetIsMapTipDirty(graphicsLayer, false);
                    }
                }
                else
                    graphicsLayer.MapTip = null;
            }
            else if (graphicsLayer.MapTip is HoverResults)
            {
                graphicsLayer.MapTip = null;
            }
        }

        // Position and size MapTip
        void MapTip_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (AssociatedObject == null)
            {
                return;
            }

            HoverResults mapTip = sender as HoverResults;
            if (mapTip == null)
            {
                return;
            }

            // Determine what quadrant of the Map the mouse is in
            bool upper = _mousePos.Y < AssociatedObject.ActualHeight / 2;
            bool right = _mousePos.X > AssociatedObject.ActualWidth / 2;

            if (mapTip.ActualHeight > 0)
            {
                double horizontalOffset;
                double verticalOffset;

                // Calculate max dimensions and offsets for MapTip
                verticalOffset = upper ? 0 : (int)(0 - mapTip.ActualHeight);
                horizontalOffset = right ? (int)(0 - mapTip.ActualWidth) : 0;
                
                // Apply dimensions and offsets
                double maxHeight;
                double maxWidth;
                maxHeight = upper ? AssociatedObject.ActualHeight - _mousePos.Y - Margin : _mousePos.Y - Margin;
                maxWidth = right ? _mousePos.X - Margin : AssociatedObject.ActualWidth - _mousePos.X - Margin;
                mapTip.MaxHeight = maxHeight;
                mapTip.MaxWidth = maxWidth;
                
                mapTip.SetValue(GraphicsLayer.MapTipHorizontalOffsetProperty, horizontalOffset);
                mapTip.SetValue(GraphicsLayer.MapTipVerticalOffsetProperty, verticalOffset);          
            }
        }

        internal static void rebuildMapTipContentsBasedOnFieldVisibility(HoverResults results)
        {
            HoverResults hr = results as HoverResults;
            if (hr == null)
                return;
            GraphicsLayer hrLayer = hr.Layer;
            if (hr.Graphic == null || hr.Graphic.Attributes == null || hrLayer == null)
            {
                hr.Visibility = Visibility.Collapsed;
                return;
            }
           Visibility hrVisibility = Visibility.Visible;
          
            IEnumerable<FieldInfo> fields = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetFields(hrLayer); // Store the fields on the Map Tips element
            string displayField = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetDisplayField(hrLayer);
            if (string.IsNullOrEmpty(displayField))
            {
                displayField = FieldInfo.GetDefaultDisplayField(fields);
                ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetDisplayField(hrLayer, displayField);
            }
            Graphic popupGraphic = hr.Graphic;
            PopupItem popupItem = new PopupItem()
            {
                DataTemplate = null,
                Graphic = popupGraphic,
                Layer = hrLayer,
                LayerName = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetLayerName(hrLayer),
                Title = null,
                TitleExpression = null,
            };
            popupItem.FieldInfos = MapTipsHelper.ToFieldSettings(ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetFields(hrLayer));
            MapTipsHelper.GetTitle(popupItem, null);

            bool hasContent = false;
            popupItem.DataTemplate = MapTipsHelper.BuildMapTipDataTemplate(popupItem, out hasContent);
            if (!hasContent)
                popupItem.DataTemplate = null;

            ESRI.ArcGIS.Client.Extensibility.PopupInfo popupInfo = new ESRI.ArcGIS.Client.Extensibility.PopupInfo()
            {
                AttributeContainer = hr.AttributeContainer,
                Container = hr,
                PopupItem = popupItem
            };
            hr.PopupInfo = popupInfo;

            bool hasTitle = !string.IsNullOrEmpty(popupInfo.PopupItem.Title);

            if (hasTitle || hasContent)
            {
                hrVisibility = Visibility.Visible;
                // re-establish delay
                TimeSpan delay = (TimeSpan)hr.Map.GetValue(DelayMapTipHide.HideDelayProperty);
                hrLayer.MapTip.SetValue(GraphicsLayer.MapTipHideDelayProperty, delay);
            }
            else
                hrVisibility = Visibility.Collapsed;
            hr.Visibility = hrVisibility;
        }

        bool hasVisibleFields(IEnumerable<FieldInfo> fields, IDictionary<string, object> graphicAttributes)
        {
            if (graphicAttributes == null || graphicAttributes.Count < 1)
                return false;
            foreach (FieldInfo f in fields)
            {
                if (f.VisibleOnMapTip && graphicAttributes.ContainsKey(f.Name) && graphicAttributes[f.Name] != null)
                {
                    if (graphicAttributes[f.Name] is string && string.IsNullOrEmpty(graphicAttributes[f.Name] as string))
                        return false;
                    return true;
                }
            }
            return false;
        }

        private void attachApplicationMapTipToLayer(GraphicsLayer layer, Graphic g)
        {
#if DEBUG
            Debug.WriteLine(string.Format("@@Attaching map tip to layer: {0}", MapApplication.GetLayerName(layer)));
#endif
            // Create a new control each time a popup is to be displayed. This is to reset the controls to their
            // initial state. If this is not done, and you have controls like ToggleButtons, they may retain their
            // expanded or collapsed state even though binding is supposed to reset this. This way, you have a
            // consistent starting point for the display of a popup.
            HoverResults hoverResults = new HoverResults()
            {
                Style = HoverLayoutStyleHelper.Instance.GetStyle("HoverPopupContainerStyle"),
                Layer = layer,
                Graphic = g,
                Map = AssociatedObject
            };

            // Associate map tip control with layer
            layer.MapTip = hoverResults;

            layer.MapTip.SizeChanged -= MapTip_SizeChanged;
            layer.MapTip.SizeChanged += MapTip_SizeChanged;

            // re-establish delay
            TimeSpan delay = (TimeSpan)AssociatedObject.GetValue(DelayMapTipHide.HideDelayProperty);
            layer.MapTip.SetValue(GraphicsLayer.MapTipHideDelayProperty, delay);
        }

        private T FindChildOfType<T>(DependencyObject obj, int? recursionLevels) where T : DependencyObject
        {
            if (recursionLevels == null)
                recursionLevels = 0;

            int childCount = VisualTreeHelper.GetChildrenCount(obj);
            DependencyObject depObj;
            for (int i = 0; i < childCount; i++)
            {
                depObj = VisualTreeHelper.GetChild(obj, i);
                var objAsT = depObj as T;
                if (objAsT != null)
                    return objAsT;

                if (VisualTreeHelper.GetChildrenCount(depObj) > 0 && recursionLevels > 0)
                {
                    objAsT = FindChildOfType<T>(depObj, recursionLevels--);
                    if (objAsT != null)
                        return objAsT;
                }
            }

            return null;
        }
    }
}
