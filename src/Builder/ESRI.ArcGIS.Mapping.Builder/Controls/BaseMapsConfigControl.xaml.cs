/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class BaseMapsConfigControl : UserControl
    {
        public BaseMapsConfigControl()
        {
            InitializeComponent();
            Binding b = new Binding("BingMapsAppId") { Source = BuilderApplication.Instance };
            b.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(this, BingKeyProperty, b);
            this.DataContext = this;
        }

        private void BaseMapsConfigControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (BuilderApplication.Instance.ConfigurationStore != null)
                BaseMapsConfigurationControl.BaseMaps = BuilderApplication.Instance.ConfigurationStore.BaseMaps;
        }

        public string BingKey
        {
            get { return (string)GetValue(BingKeyProperty); }
            set { SetValue(BingKeyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BingKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BingKeyProperty =
            DependencyProperty.Register("BingKey", typeof(string), typeof(ESRI.ArcGIS.Mapping.Builder.BaseMapsConfigControl), new PropertyMetadata(null));

        

        
    }
}
