/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace SearchTool
{
    public class AdjustColumnMinWidthToFitTabsBehavior : Behavior<Grid>
    {
        // Collection of tabs that events have been hooked to
        private List<TabItem> hookedItems = new List<TabItem>();

        // Collection that is being monitored for change notification 
        private INotifyCollectionChanged hookedCollection;

        #region Behavior Overrides

        protected override void OnAttached()
        {
            base.OnAttached();

            // Initialize event handlers and bindings
            if (TabControl != null)
            {
                hookToTabEvents(TabControl.Items);
                setItemsSourceBinding(TabControl);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            // Clear event handlers and bindings
            unHookFromTabEvents();
            clearItemsSourceBinding();
        }

        #endregion

        #region Column

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Column"/> property
        /// </summary>
        public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register(
            "Column", typeof(int), typeof(AdjustColumnMinWidthToFitTabsBehavior),
            new PropertyMetadata(-1, OnColumnChanged));

        /// <summary>
        /// Gets or sets the index of the column to adjust the minimum width of
        /// </summary>
        public int Column
        {
            get { return (int)GetValue(ColumnProperty); }
            set { SetValue(ColumnProperty, value); }
        }

        private static void OnColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // redo column width adjustment
            ((AdjustColumnMinWidthToFitTabsBehavior)d).adjustColumnMinWidth();
        }

        #endregion

        #region TabControl

        /// <summary>
        /// Backing DependencyProperty for the <see cref="TabControl"/> property
        /// </summary>
        public static readonly DependencyProperty TabControlProperty = DependencyProperty.Register(
            "TabControl", typeof(TabControl), typeof(AdjustColumnMinWidthToFitTabsBehavior),
            new PropertyMetadata(OnTabControlChanged));

        /// <summary>
        /// Gets or sets the <see cref="TabControl"/> on which to base auto-fitting
        /// </summary>
        public TabControl TabControl
        {
            get { return GetValue(TabControlProperty) as TabControl; }
            set { SetValue(TabControlProperty, value); }
        }

        private static void OnTabControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AdjustColumnMinWidthToFitTabsBehavior behavior = (AdjustColumnMinWidthToFitTabsBehavior)d;

            // Unhook from events on previous tab control's tabs
            behavior.unHookFromTabEvents();
            behavior.clearItemsSourceBinding();

            if (e.NewValue is TabControl)
            {
                TabControl tabControl = (TabControl)e.NewValue;

                // Wire up events
                behavior.hookToTabEvents(tabControl.Items);
                behavior.setItemsSourceBinding(tabControl);

                // Calculate and apply column width
                behavior.adjustColumnMinWidth();
            }
        }

        #endregion

        #region ItemsSource

        /// <summary>
        /// Backing DependencyProperty for the <see cref="ItemsSource"/> property
        /// </summary>
        private static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(AdjustColumnMinWidthToFitTabsBehavior),
            new PropertyMetadata(OnItemsSourceChanged));

        /// <summary>
        /// Gets or sets the source of the bound tab control's items
        /// </summary>
        private IEnumerable ItemsSource
        {
            get { return GetValue(ItemsSourceProperty) as IEnumerable; }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AdjustColumnMinWidthToFitTabsBehavior behavior = (AdjustColumnMinWidthToFitTabsBehavior)d;

            // Unhook event handlers
            behavior.unHookFromTabEvents();

            // Hook to events on new tabs
            if (behavior.TabControl != null)
                behavior.hookToTabEvents(behavior.TabControl.Items);

            // Calculate and apply column width
            behavior.adjustColumnMinWidth();
        }

        #endregion

        #region Event Handlers

        // Fires when the number of tabs changes
        private void Tabs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Unhook from events on old items and hook to events on new ones
            unHookFromTabEvents(e.OldItems);
            hookToTabEvents(e.NewItems);

            // Recalculate and apply column width
            adjustColumnMinWidth();
        }

        // Fires when a tab's size changes
        private void Tab_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Recalculate and apply column width
            adjustColumnMinWidth();
        }

        // Fires when a tab's layout changes
        private void Tab_LayoutUpdated(object sender, EventArgs e)
        {
            adjustColumnMinWidth();
        }

        #endregion

        // Calculates and applies the minimum width necessary to accommodate the tabs in the 
        // specified tab control
        private void adjustColumnMinWidth()
        {
            if (AssociatedObject == null || TabControl == null || Column < 0 
            || Column >= AssociatedObject.ColumnDefinitions.Count 
            || TabControl.Items.Count == 0)
                return;

            ColumnDefinition cDef = AssociatedObject.ColumnDefinitions[Column];

            // Difference between column width and tab control width. This difference needs to be
            // preserved when calculating the new column width.
            double tabControlMargin = cDef.ActualWidth - TabControl.ActualWidth;

            double aggregateTabWidths = 0;
            double tabPanelMargin = 0;

            // Calculate width for each tab
            for (int i = 0; i < TabControl.Items.Count; i++)
            {
                TabItem tab = TabControl.Items[i] as TabItem;
                if (tab == null)
                    return; // Unexpected object type - bail out

                double threshholdWidth = double.IsNaN(tab.MinWidth) ? 0 : tab.MinWidth;
                if (tab.ActualWidth > threshholdWidth)
                {
                    // Aggregate tab width and margin
                    aggregateTabWidths += tab.ActualWidth + tab.Margin.Left + tab.Margin.Right;

                    if (i == 0)
                    {
                        // Get the margin of the panel containing the tabs.  Note that this is the only
                        // dimension on an element external to the tabs that is taken into account, so
                        // if the TabControl's template has been modified to include other elements, this
                        // measurement will not be sufficient.

                        TabPanel tabPanel = tab.FindAncestorOfType<TabPanel>();
                        if (tabPanel != null)
                            tabPanelMargin += tabPanel.Margin.Left + tabPanel.Margin.Right;
                    }
                }
                else
                {
                    return; // nothing to measure - bail out
                }
            }

            // Apply calculated widths as column min width. The constant 2 is applied as this is necessary 
            // to prevent wrapping, but walking the visual tree and measuring could not account for these 
            // 2 pixels
            cDef.MinWidth = tabControlMargin + tabPanelMargin + aggregateTabWidths + 2;
        }

        // Wire up event handlers to tabs
        private void hookToTabEvents(IList tabs)
        {
            if (tabs == null) return;

            for (int i = 0; i < tabs.Count; i++)
            {
                TabItem tab = tabs[i] as TabItem;
                if (tab != null && !hookedItems.Contains(tab))
                {
                    tab.SizeChanged += Tab_SizeChanged;
                    tab.LayoutUpdated += Tab_LayoutUpdated;
                    hookedItems.Add(tab);
                }
            }

            if (tabs is INotifyCollectionChanged && !tabs.Equals(hookedCollection))
            {
                INotifyCollectionChanged collection = (INotifyCollectionChanged)tabs;
                collection.CollectionChanged += Tabs_CollectionChanged;
                hookedCollection = collection;
            }
        }

        // Unhook event handlers from tabs
        private void unHookFromTabEvents(IList tabs = null)
        {
            if (tabs == null)
                tabs = hookedItems;

            for (int i = 0; i < tabs.Count; i++)
            {
                TabItem tab = tabs[i] as TabItem;
                if (tab != null && hookedItems.Contains(tab))
                {
                    tab.SizeChanged -= Tab_SizeChanged;
                    tab.LayoutUpdated -= Tab_LayoutUpdated;
                    hookedItems.Remove(tab);
                }
            }

            if (tabs is INotifyCollectionChanged && tabs.Equals(hookedCollection))
            {
                INotifyCollectionChanged collection = (INotifyCollectionChanged)tabs;
                collection.CollectionChanged -= Tabs_CollectionChanged;
                hookedCollection = null;
            }
        }

        private void setItemsSourceBinding(TabControl tabControl)
        {
            Binding b = new Binding("ItemsSource") { Source = tabControl };
            BindingOperations.SetBinding(this, ItemsSourceProperty, b);
        }

        private void clearItemsSourceBinding()
        {
            BindingOperations.SetBinding(this, ItemsSourceProperty, new Binding());
        }
    }
}
