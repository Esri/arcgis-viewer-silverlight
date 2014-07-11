/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace SearchTool
{
    /// <summary>
    /// Automatically selects the first item in a <see cref="Selector"/> control.  Requires that
    /// the Selector's ItemsSource be defined and implement <see cref="INotifyCollectionChanged"/>
    /// </summary>
    public class AutoSelectFirstItem : Behavior<Selector>
    {
        private bool hasZeroItems; // Tracks whether the targeted Selector has no items

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject != null)
            {
                // Bind to the targeted Selector's ItemsSource
                Binding b = new Binding("ItemsSource") { Source = AssociatedObject };
                BindingOperations.SetBinding(this, ItemsSourceProperty, b);

                // Wire collection changed events
                hookToItemsChanged();
            }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="ItemsSource"/> property
        /// </summary>
        public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(object), typeof(AutoSelectFirstItem), new PropertyMetadata(OnItemsSourcePropertyChanged));

        /// <summary>
        /// Used to bind to the ItemsSource property on the targeted Selector control
        /// </summary>
        private object ItemsSource
        {
            get { return GetValue(ItemsSourceProperty) as object; }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Fires when the ItemsSource property is first bound or the ItemsSource changes on the targeted Selector control
        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoSelectFirstItem a = (AutoSelectFirstItem)d;

            // Unhook from the previous source's collection changed event
            if (e.OldValue is INotifyCollectionChanged)
                ((INotifyCollectionChanged)e.OldValue).CollectionChanged -= a.Items_CollectionChanged;

            // Hook to the new source's events
            a.hookToItemsChanged();

            // If there are items in the new source, auto-select the first one
            if (a != null && a.AssociatedObject != null && a.AssociatedObject.Items.Count > 0 
            && a.AssociatedObject.SelectedItem == null)
                a.AssociatedObject.SelectedIndex = 0;
        }

        // Fires when targeted Selector's collection changes
        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (hasZeroItems && AssociatedObject.Items.Count > 0)
            {
                // object has gone from having zero items to at least one.  Select it.

                hasZeroItems = false;
                Dispatcher.BeginInvoke(() => // Wrap item selection in BeginInvoke to allow item
                {                        // rendering/updating to complete.
                    if (AssociatedObject.Items.Count > 0)
                        AssociatedObject.SelectedIndex = 0;
                });
            }
            else if (!hasZeroItems && AssociatedObject.Items.Count == 0)
            {
                // Items have been cleared.  Reset flag so subsequent collection changed events
                // will attempt to reselect the first item.
                hasZeroItems = true;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject == null)
                return;

            // Unhook events
            if (AssociatedObject.ItemsSource is INotifyCollectionChanged)
                ((INotifyCollectionChanged)AssociatedObject.ItemsSource).CollectionChanged -= Items_CollectionChanged;
        }

        // Hooks to the bound collection's changed events
        private void hookToItemsChanged()
        {
            if (AssociatedObject.ItemsSource is INotifyCollectionChanged)
            {
                hasZeroItems = AssociatedObject.Items.Count == 0;
                ((INotifyCollectionChanged)AssociatedObject.ItemsSource).CollectionChanged += Items_CollectionChanged;
            }
        }
    }
}

