/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client;
using System.Windows;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class SecureServicesLayerConfig : LayerConfigControl
    {
        public SecureServicesLayerConfig()
        {
            DefaultStyleKey = typeof(SecureServicesLayerConfig);
            SecureServicesLayerConfigViewModel viewModel = new SecureServicesLayerConfigViewModel();
            this.DataContext = viewModel;
            System.Windows.Data.Binding binding = new System.Windows.Data.Binding();
            binding.Source = viewModel;
            binding.Path = new PropertyPath("IsEnabled");
            binding.Mode = System.Windows.Data.BindingMode.TwoWay;
            SetBinding(IsEnabledProperty, binding);
        }
    }
}
