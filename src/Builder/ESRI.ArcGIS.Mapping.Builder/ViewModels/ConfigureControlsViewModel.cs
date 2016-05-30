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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class ConfigureControlDataItem
    {
        public string Label { get; set; }
        public FrameworkElement Element { get; set; }
    }

    public class ConfigureControlsViewModel : DependencyObject
    {
        public ConfigureControlsViewModel()
        {
            //Configure = new DelegateCommand(configure);
            Close = new DelegateCommand(close);
            ConfigurableItems = new ObservableCollection<ConfigureControlDataItem>();
        }

        #region Selected Index and Item
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(ConfigureControlsViewModel), new PropertyMetadata(-1, OnSelectedIndexChanged));

        private static void OnSelectedIndexChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            ConfigureControlsViewModel ccvm = o as ConfigureControlsViewModel;
            if (ccvm == null)
                return;

            if (ccvm.SelectedIndex < 0 || ccvm.ConfigurableItems.Count < 1)
            {
                ccvm.SelectedItem = null;
                //ccvm.ConfigureEnabled = false;
            }
            else
            {
                ccvm.SelectedItem = ccvm.ConfigurableItems[ccvm.SelectedIndex];
                //ccvm.ConfigureEnabled = true;
            }
        }

        public ConfigureControlDataItem SelectedItem
        {
            get { return (ConfigureControlDataItem)GetValue(SelectedItemProperty); }
            internal set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(ConfigureControlDataItem), typeof(ConfigureControlsViewModel), new PropertyMetadata(null));
        #endregion

        #region Configure Button Enable
        //public bool ConfigureEnabled
        //{
        //    get { return (bool)GetValue(ConfigureEnabledProperty); }
        //    set { SetValue(ConfigureEnabledProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for ConfigureEnabled.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ConfigureEnabledProperty =
        //    DependencyProperty.Register("ConfigureEnabled", typeof(bool), typeof(ConfigureControlsViewModel), new PropertyMetadata(false));
        #endregion

        #region Configurable Items
        public ObservableCollection<ConfigureControlDataItem> ConfigurableItems
        {
            get { return (ObservableCollection<ConfigureControlDataItem>)GetValue(ConfigurableItemsProperty); }
            set { SetValue(ConfigurableItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConfigurableItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConfigurableItemsProperty =
            DependencyProperty.Register("ConfigurableItems", typeof(ObservableCollection<ConfigureControlDataItem>), typeof(ConfigureControlsViewModel), new PropertyMetadata(null));
        #endregion

        #region Configure Property
        //public ICommand Configure
        //{
        //    get { return (ICommand)GetValue(ConfigureProperty); }
        //    set { SetValue(ConfigureProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Configure.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ConfigureProperty =
        //    DependencyProperty.Register("Configure", typeof(ICommand), typeof(ConfigureControlsViewModel), null);

        //private void configure(object commandParameter)
        //{
        //    ISupportsConfiguration supportsConfig = SelectedItem.Element as ISupportsConfiguration;
        //    if (supportsConfig != null)
        //    {
        //        supportsConfig.Configure();
        //    }
        //}
        #endregion

        #region Close Property
        public ICommand Close
        {
            get { return (ICommand)GetValue(CloseProperty); }
            set { SetValue(CloseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Close.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseProperty =
            DependencyProperty.Register("Close", typeof(ICommand), typeof(ConfigureControlsViewModel), null);

        private void close(object commandParameter)
        {
            if (Closed != null)
                Closed(this, EventArgs.Empty);
        }

        public event EventHandler Closed;
        #endregion
    }
}
