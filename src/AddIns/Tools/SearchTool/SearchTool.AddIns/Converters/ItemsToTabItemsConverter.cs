/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SearchTool.Resources;

namespace SearchTool
{
    /// <summary>
    /// Converts a collection of items into tab items, applying the specified header and content templates.  The
    /// DataContext of each tab item, including header and content, is set to the item in the collection.  Allows
    /// specification of the ItemsSource property of a <see cref="TabControl"/>
    /// </summary>
    /// <remarks>
    /// The converter does not respond to changes to the collection.  So if items are added to or removed from
    /// the bound collection after the initial binding, the tab items will be out of sync with the collection.
    /// </remarks>
    public class ItemsToTabItemsConverter : IValueConverter
    {
        // Create the collection of tabs.  Use an observable collection so it's change-aware.
        ObservableCollection<TabItem> tabs;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            tabs = new ObservableCollection<TabItem>();

            // Get the collection passed to the converter.  Use IEnumerable<dynamic> because any IEnumerable<T> can be
            // cast to this, but IEnumerable<T> does not cast to IEnumerable
            IEnumerable<dynamic> items = value as IEnumerable<dynamic>;

            // Listen for changes in the included tabs
            if (items is INotifyCollectionChanged)
                ((INotifyCollectionChanged)items).CollectionChanged += Item_CollectionChanged;

            // Generate tabs from the current set of items
            addTabs(items);

            return tabs;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException(Strings.CannotConvertBack);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the template to apply to tab header content
        /// </summary>
        public DataTemplate HeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the style of generated tab headers
        /// </summary>
        public Style HeaderStyle { get; set; }

        /// <summary>
        /// Gets or sets the template to apply to tab content
        /// </summary>
        public DataTemplate ContentTemplate { get; set; }

        #endregion

        #region Private Methods

        // Raised when the items collection used to generate tabs changes
        private void Item_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (tabs == null)
                return;

            // Add and remove tabs based on the new and removed items
            addTabs(e.NewItems);
            removeTabs(e.OldItems);
        }

        // Generates tabs from the passed-in items
        private void addTabs(IEnumerable items)
        {
            if (items != null && HeaderTemplate != null && ContentTemplate != null)
            {
                foreach (object item in items)
                {
                    // Create elements from header and content templates.  Necessary because applying the templates directly to 
                    // the tab item's HeaderTemplate and ContentTemplate properties fails to establish the proper data context
                    FrameworkElement header = HeaderTemplate.LoadContent() as FrameworkElement;
                    FrameworkElement content = ContentTemplate.LoadContent() as FrameworkElement;

                    if (header != null && content != null)
                    {
                        // Set the data context of tab header and content
                        header.DataContext = item;
                        content.DataContext = item;

                        // Create the tab item and add to the collection of tabs
                        TabItem tab = new TabItem()
                        {
                            Header = header,
                            Content = content,
                            DataContext = item
                        };

                        if (HeaderStyle != null)
                            tab.Style = HeaderStyle;

                        tabs.Add(tab);
                    }
                }
            }
        }

        // Removes tabs corresponding to the passed in items
        private void removeTabs(IEnumerable items)
        {
            if (items != null)
            {
                foreach (object item in items)
                {
                    TabItem tab = tabs.FirstOrDefault(t => t.DataContext.Equals(item));
                    if (tab != null)
                        tabs.Remove(tab);
                }
            }
        }

        #endregion
    }
}
