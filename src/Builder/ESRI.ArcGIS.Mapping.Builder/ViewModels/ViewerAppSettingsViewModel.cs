/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using System.Windows.Data;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;
using ESRI.ArcGIS.Client.Portal;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class ViewerAppSettingsViewModel : DependencyObject
    {
        public ViewerAppSettingsViewModel()
        {
            ApplyCommand = new DelegateCommand(apply, canApplyCancel);
            CancelCommand = new DelegateCommand(cancel, canApplyCancel);
            Binding b = new Binding("ViewerApplication") { Source = ViewerApplicationControl.Instance };
            b.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(this, ViewerApplicationProperty, b);
        }

        #region Apply and Cancel commands
        private bool canApplyCancel(object commandParameter)
        {
            return ViewerApplication != null && (BingMapsAppId != ViewerApplication.BingMapsAppId ||
            PortalAppId != ViewerApplication.PortalAppId ||
            GeometryServiceUrl != ViewerApplication.GeometryService ||
            ArcGISOnlineSharing != ViewerApplication.ArcGISOnlineSharing ||
            ArcGISOnlineSecure != ViewerApplication.ArcGISOnlineSecure ||
            Proxy != ViewerApplication.Proxy);
        }
        public DelegateCommand ApplyCommand { get; set; }
        private void apply(object commandParameter)
        {
            if (ViewerApplication != null)
            {
                if (ViewerApplication.BingMapsAppId != BingMapsAppId)
                {
                    if (View.Instance != null && View.Instance.ConfigurationStore != null)
                        View.Instance.ConfigurationStore.BingMapsAppId = BingMapsAppId;
                    ViewerApplication.BingMapsAppId = BingMapsAppId;
                    if (View.Instance != null && View.Instance.Map != null)
                    {
                        foreach (Layer layer in View.Instance.Map.Layers)
                        {
                            Client.Bing.TileLayer bingLayer = layer as Client.Bing.TileLayer;
                            if (bingLayer != null && ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetUsesBingAppID(bingLayer))
                            {
                                bingLayer.Token = ConfigurationStore.Current.BingMapsAppId;
                                bingLayer.Refresh();
                            }
                        }
                    }
                }
                if (ViewerApplication.PortalAppId != PortalAppId)
                {
                    if (View.Instance != null && View.Instance.ConfigurationStore != null)
                        View.Instance.ConfigurationStore.PortalAppId = PortalAppId;
                    ViewerApplication.PortalAppId = PortalAppId;
                }
                if (ViewerApplication.GeometryService != GeometryServiceUrl)
                {
                    if (View.Instance != null)
                        View.Instance.SetGeometryServiceUrl(GeometryServiceUrl);
                    ViewerApplication.GeometryService = GeometryServiceUrl;
                }

                bool portalChanged = false;
                if (ViewerApplication.ArcGISOnlineSharing != ArcGISOnlineSharing)
                {
                    if (View.Instance != null)
                        View.Instance.ArcGISOnlineSharing = ArcGISOnlineSharing;
                    ViewerApplication.ArcGISOnlineSharing = ArcGISOnlineSharing;
                    portalChanged = true;
                }
                if (ViewerApplication.ArcGISOnlineSecure != ArcGISOnlineSecure)
                {
                    if (View.Instance != null)
                        View.Instance.ArcGISOnlineSecure = ArcGISOnlineSecure;
                    ViewerApplication.ArcGISOnlineSecure = ArcGISOnlineSecure;
                    portalChanged = true;
                }
                if (ViewerApplication.Proxy != Proxy)
                {
                    if (View.Instance != null)
                        View.Instance.ProxyUrl = Proxy;
                    ViewerApplication.Proxy = Proxy;
                }

                if (portalChanged)
                {
                    ArcGISPortal portal = new ArcGISPortal()
                    {
                        Url = View.Instance.BaseUrl.ToLower().StartsWith("https://") ? ArcGISOnlineSecure 
                            : ArcGISOnlineSharing,
                        ProxyUrl = ViewerApplication.ArcGISOnlineProxy
                    };

                    BuilderApplication.Instance.UpdatingSettings = true;
                    portal.InitializeAsync(portal.Url, (o, ex) =>
                    {
                        View.Instance.Portal = portal;
                        BuilderApplication.Instance.UpdatingSettings = false;
                    });
                }
            }
            raiseChange();
        }

        public DelegateCommand CancelCommand { get; set; }
        private void cancel(object commandParameter)
        {
            if (ViewerApplication != null)
            {
                BingMapsAppId = ViewerApplication.BingMapsAppId;
                PortalAppId = ViewerApplication.PortalAppId;
                GeometryServiceUrl = ViewerApplication.GeometryService;
                ArcGISOnlineSharing = ViewerApplication.ArcGISOnlineSharing;
                ArcGISOnlineSecure = ViewerApplication.ArcGISOnlineSecure;
                Proxy = ViewerApplication.Proxy;
            }
            raiseChange();
        }
        #endregion

        public static void OnChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            ViewerAppSettingsViewModel source = o as ViewerAppSettingsViewModel;
            if (source != null)
            {
                source.raiseChange();
            }
        }

        void raiseChange()
        {
            ApplyCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }

        public ViewerApplication ViewerApplication
        {
            get { return (ViewerApplication)GetValue(ViewerApplicationProperty); }
            set { SetValue(ViewerApplicationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewerApplication.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewerApplicationProperty =
            DependencyProperty.Register("ViewerApplication", typeof(ViewerApplication), typeof(ViewerAppSettingsViewModel), new PropertyMetadata(OnViewerAppChange));

        public static void OnViewerAppChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            ViewerAppSettingsViewModel source = o as ViewerAppSettingsViewModel;
            if (source != null)
            {
                Binding b = new Binding("BingMapsAppId") { Source = source.ViewerApplication };
                b.Mode = BindingMode.OneWay;
                BindingOperations.SetBinding(source, BingMapsAppIdProperty, b);
                b = new Binding("PortalAppId") { Source = source.ViewerApplication };
                b.Mode = BindingMode.OneWay;
                BindingOperations.SetBinding(source, PortalAppIdProperty, b);
                b = new Binding("GeometryService") { Source = source.ViewerApplication };
                b.Mode = BindingMode.OneWay;
                BindingOperations.SetBinding(source, GeometryServiceUrlProperty, b);
                b = new Binding("ArcGISOnlineSharing") { Source = source.ViewerApplication };
                b.Mode = BindingMode.OneWay;
                BindingOperations.SetBinding(source, ArcGISOnlineSharingProperty, b);
                b = new Binding("ArcGISOnlineSecure") { Source = source.ViewerApplication };
                b.Mode = BindingMode.OneWay;
                BindingOperations.SetBinding(source, ArcGISOnlineSecureProperty, b);
                b = new Binding("Proxy") { Source = source.ViewerApplication };
                b.Mode = BindingMode.OneWay;
                BindingOperations.SetBinding(source, ProxyProperty, b);
            }
        }

        public string BingMapsAppId
        {
            get { return (string)GetValue(BingMapsAppIdProperty); }
            set { SetValue(BingMapsAppIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BingMapsAppId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BingMapsAppIdProperty =
            DependencyProperty.Register("BingMapsAppId", typeof(string), typeof(ViewerAppSettingsViewModel), new PropertyMetadata(OnChange));

        public string PortalAppId
        {
            get { return (string)GetValue(PortalAppIdProperty); }
            set { SetValue(PortalAppIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PortalAppId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PortalAppIdProperty =
            DependencyProperty.Register("PortalAppId", typeof(string), typeof(ViewerAppSettingsViewModel), new PropertyMetadata(OnChange));

        public string GeometryServiceUrl
        {
            get { return (string)GetValue(GeometryServiceUrlProperty); }
            set { SetValue(GeometryServiceUrlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GeometryServiceUrl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GeometryServiceUrlProperty =
            DependencyProperty.Register("GeometryServiceUrl", typeof(string), typeof(ViewerAppSettingsViewModel), new PropertyMetadata(OnChange));

        public string ArcGISOnlineSharing
        {
            get { return (string)GetValue(ArcGISOnlineSharingProperty); }
            set { SetValue(ArcGISOnlineSharingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ArcGISOnlineSharing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ArcGISOnlineSharingProperty =
            DependencyProperty.Register("ArcGISOnlineSharing", typeof(string), typeof(ViewerAppSettingsViewModel), new PropertyMetadata(OnChange));

        public string ArcGISOnlineSecure
        {
            get { return (string)GetValue(ArcGISOnlineSecureProperty); }
            set { SetValue(ArcGISOnlineSecureProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ArcGISOnlineSecure.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ArcGISOnlineSecureProperty =
            DependencyProperty.Register("ArcGISOnlineSecure", typeof(string), typeof(ViewerAppSettingsViewModel), new PropertyMetadata(OnChange));

        #region Proxy

        /// <summary>
        /// Gets or sets the proxy URL used for web requests that require routing through a proxy server
        /// </summary>
        public string Proxy
        {
            get { return (string)GetValue(ProxyProperty); }
            set { SetValue(ProxyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Proxy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProxyProperty =
            DependencyProperty.Register("Proxy", typeof(string), typeof(ViewerAppSettingsViewModel),
            new PropertyMetadata(OnChange));

        #endregion Proxy

    }
}
