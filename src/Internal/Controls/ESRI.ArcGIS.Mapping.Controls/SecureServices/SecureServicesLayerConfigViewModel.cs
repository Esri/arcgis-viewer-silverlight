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
using ESRI.ArcGIS.Client;
using System.Reflection;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class SecureServicesLayerConfigViewModel : DependencyObject
    {
        Layer layer;
        public SecureServicesLayerConfigViewModel()
        {
            MapApplication.Current.SelectedLayerChanged += Current_SelectedLayerChanged;
            Current_SelectedLayerChanged(null, null);
        }

        void Current_SelectedLayerChanged(object sender, EventArgs e)
        {
            layer = MapApplication.Current.SelectedLayer;
            if (layer != null)
            {
                if (SecureServicesHelper.SupportsProxyUrl(layer))
                {
                    this.IsEnabled = true;
                    this.UsesProxy = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetUsesProxy(layer);
                }
                else
                {
                    this.IsEnabled = false;
                    this.UsesProxy = false;
                }
            }
        }

        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(SecureServicesLayerConfigViewModel), new PropertyMetadata(false));

        public bool UsesProxy
        {
            get { return (bool)GetValue(UsesProxyProperty); }
            set { SetValue(UsesProxyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UsesProxy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsesProxyProperty =
            DependencyProperty.Register("UsesProxy", typeof(bool), typeof(SecureServicesLayerConfigViewModel), new PropertyMetadata(OnProxyUseChange));

        static void OnProxyUseChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            SecureServicesLayerConfigViewModel vm = o as SecureServicesLayerConfigViewModel;
            ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetUsesProxy(vm.layer, vm.UsesProxy);
            if (vm.UsesProxy)
            {
                if (vm.layer != null && ViewerApplicationControl.Instance != null && ViewerApplicationControl.Instance.ViewerApplication != null)
                    SecureServicesHelper.SetProxyUrl(vm.layer, SecureServicesHelper.GetProxyUrl());
            }
            else
            {
                SecureServicesHelper.SetProxyUrl(vm.layer, null);
            }
            
        }


    }
}
