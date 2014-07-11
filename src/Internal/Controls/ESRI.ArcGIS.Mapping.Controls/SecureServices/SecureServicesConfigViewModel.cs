/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using System.Windows.Input;
using System;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class SecureServicesConfigViewModel : DependencyObject, IDisposable
    {
        public SecureServicesConfigViewModel()
        {
            if (View.Instance != null)
                View.Instance.ProxyUrlChanged += Instance_ProxyUrlChanged;
            Proxy = SecureServicesHelper.GetProxyUrl();
            OKCommand = new DelegateCommand(okClicked);
        }

        void Instance_ProxyUrlChanged(object sender, System.EventArgs e)
        {
            Proxy = SecureServicesHelper.GetProxyUrl();
        }

        public string Proxy
        {
            get { return (string)GetValue(ProxyProperty); }
            set { SetValue(ProxyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Proxy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProxyProperty =
            DependencyProperty.Register("Proxy", typeof(string), typeof(SecureServicesConfigViewModel), new PropertyMetadata(null));

        void okClicked(object param)
        {
            string proxy = param as string;
            if (proxy != SecureServicesHelper.GetProxyUrl())
                SecureServicesHelper.SetProxyUrl(proxy);
        }

        public ICommand OKCommand { get; set; }

        public void Dispose()
        {
            if (View.Instance != null)
                View.Instance.ProxyUrlChanged -= Instance_ProxyUrlChanged;
        }
    }
}
