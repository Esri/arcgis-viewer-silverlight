/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Toolkit.Primitives;
using System.Linq;
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    public class ShowLayersVisibleAtScaleControl : FrameworkElement
    {
        public ShowLayersVisibleAtScaleControl() { }

        #region ShowLayersVisibleAtScale
        /// <summary>
        /// 
        /// </summary>
        public bool ShowLayersVisibleAtScale
        {
            get { return (bool)GetValue(ShowLayersVisibleAtScaleProperty); }
            set { SetValue(ShowLayersVisibleAtScaleProperty, value); }
        }

        /// <summary>
        /// Identifies the Map dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowLayersVisibleAtScaleProperty =
            DependencyProperty.Register(
                "ShowLayersVisibleAtScale",
                typeof(bool),
                typeof(ShowLayersVisibleAtScaleControl),
                new PropertyMetadata(false));
        #endregion

        #region IsInScaleRange
        /// <summary>
        /// 
        /// </summary>
        public bool? IsInScaleRange
        {
            get { return (bool?)GetValue(IsInScaleRangeProperty); }
            set { SetValue(IsInScaleRangeProperty, value); }
        }

        /// <summary>
        /// Identifies the Map dependency property.
        /// </summary>
        public static readonly DependencyProperty IsInScaleRangeProperty =
            DependencyProperty.Register(
                "IsInScaleRange",
                typeof(bool?),
                typeof(ShowLayersVisibleAtScaleControl),
                new PropertyMetadata(null, OnIsInScaleRangePropertyChanged));

        public static void OnIsInScaleRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ShowLayersVisibleAtScaleControl conv = d as ShowLayersVisibleAtScaleControl;
            if (conv != null)
            {
                conv.SetTreeViewItemVisibility();
            }
        }

        #endregion

        #region LayerItems
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<LayerItemViewModel> LayerItems
        {
            get { return GetValue(LayerItemsProperty) as ObservableCollection<LayerItemViewModel>; }
            set { SetValue(LayerItemsProperty, value); }
        }

        /// <summary>
        /// Identifies the Map dependency property.
        /// </summary>
        public static readonly DependencyProperty LayerItemsProperty =
            DependencyProperty.Register(
                "LayerItems",
                typeof(ObservableCollection<LayerItemViewModel>),
                typeof(ShowLayersVisibleAtScaleControl),
                new PropertyMetadata(null, OnLayerItemsChanged));

        public static void OnLayerItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ShowLayersVisibleAtScaleControl conv = d as ShowLayersVisibleAtScaleControl;
            if (conv != null && conv.ShowLayersVisibleAtScale)
            {
                conv.SubscribeToPropertyChangedEvent(e.OldValue as ObservableCollection<LayerItemViewModel>, false);
                conv.SetTreeViewItemVisibility();
                conv.SubscribeToPropertyChangedEvent(e.NewValue as ObservableCollection<LayerItemViewModel>, true);
            }
        }
        private void SubscribeToPropertyChangedEvent(ObservableCollection<LayerItemViewModel> coll, bool subscribe)
        {
            if (coll == null)
                return;

            foreach (LayerItemViewModel mod in coll)
            {
                if (subscribe)
                    mod.PropertyChanged += LayerItemViewModel_PropertyChanged;
                else
                    mod.PropertyChanged -= LayerItemViewModel_PropertyChanged;
            }
        }
        private void LayerItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e == null)
                return;

            if (e.PropertyName == "IsInScaleRange")
                SetTreeViewItemVisibility();
        }
        #endregion

        private void SetTreeViewItemVisibility()
        {
            if (ShowLayersVisibleAtScale)
            {
                bool isNodeVisible = IsInScaleRange == true;
                if (isNodeVisible)
                {
                    LayerItemViewModel mod = this.DataContext as LayerItemViewModel;
                    if (mod != null && mod.LayerItems != null)
                    {
                        LayerItemViewModel child = mod.LayerItems.FirstOrDefault(lm => lm.IsInScaleRange);
                        if (child == null)
                            isNodeVisible = false;
                    }
                }
                //hide the node if not visible at current extent
                TreeViewItem item = ControlTreeHelper.FindAncestorOfType<TreeViewItem>(this);
                if (item != null)
                {
                    item.Visibility = isNodeVisible ? Visibility.Visible : Visibility.Collapsed;
                    if (item.Visibility == Visibility.Visible)
                        EnsureAncestorsVisible(item);
                }
            }
        }

        private void EnsureAncestorsVisible(TreeViewItem item)
        {
            TreeViewItem parent = ControlTreeHelper.FindAncestorOfType<TreeViewItem>(item);
            if (parent != null)
            {
                if (parent.Visibility != Visibility.Visible)
                    parent.Visibility = Visibility.Visible;
                EnsureAncestorsVisible(parent);
            }
        }
    }
}
