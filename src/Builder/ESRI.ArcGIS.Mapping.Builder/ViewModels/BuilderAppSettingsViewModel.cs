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
using MappingControls = ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;
using ESRI.ArcGIS.Mapping.Builder.Common;
using ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder;
using System.Linq;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class BuilderAppSettingsViewModel : DependencyObject
    {
        internal const string SETTINGSCONFIGPATH = "~/App_Data/Settings.xml";
        public BuilderAppSettingsViewModel()
        {
            ApplyCommand = new DelegateCommand(apply, canApplyCancel);
            CancelCommand = new DelegateCommand(cancel, canApplyCancel); 
            ViewerApplication = BuilderApplication.Instance;
            Binding b = new Binding("BingMapsAppId") { Source = BuilderApplication.Instance };
            b.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(this, BingMapsAppIdProperty, b);
            b = new Binding("PortalAppId") { Source = BuilderApplication.Instance };
            b.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(this, PortalAppIdProperty, b);
            b = new Binding("GeometryService") { Source = BuilderApplication.Instance };
            b.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(this, GeometryServiceUrlProperty, b);
            b = new Binding("ArcGISOnlineSharing") { Source = BuilderApplication.Instance };
            b.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(this, ArcGISOnlineSharingProperty, b);
            b = new Binding("ArcGISOnlineSecure") { Source = BuilderApplication.Instance };
            b.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(this, ArcGISOnlineSecureProperty, b);
            b = new Binding("Proxy") { Source = BuilderApplication.Instance };
            b.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(this, ProxyProperty, b);
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
                ViewerApplication.BingMapsAppId = BingMapsAppId;
                ViewerApplication.PortalAppId = PortalAppId;
                ViewerApplication.GeometryService = GeometryServiceUrl;
                ViewerApplication.Proxy = Proxy;
                ViewerApplication.ArcGISOnlineSharing = ArcGISOnlineSharing;
                ViewerApplication.ArcGISOnlineSecure = ArcGISOnlineSecure;
                if (ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnlineEnvironment.ConfigurationUrls.Sharing != BuilderApplication.Instance.ArcGISOnlineSharing ||
                ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnlineEnvironment.ConfigurationUrls.Secure != BuilderApplication.Instance.ArcGISOnlineSecure ||
                ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnlineEnvironment.ConfigurationUrls.ProxyServer != BuilderApplication.Instance.ArcGISOnlineProxy)
                {
                    BuilderApplication.Instance.UpdatingSettings = true;

                    EventHandler<EventArgs> onInitialized = null;
                    onInitialized = (o, e) =>
                        {
                            ArcGISOnlineEnvironment.ArcGISOnline.Initialized -= onInitialized;
                            BuilderApplication.Instance.UpdatingSettings = false;

                            if (ArcGISOnlineEnvironment.ArcGISOnline.InitializationError == null)
                            {
                                BuilderApplication.Instance.MapCenterRequiresRefresh = true;
                            }
                            else
                            {
                                string message = string.Format(
                                    MappingControls.LocalizableStrings.GetString("ServiceConnectionErrorDuringInit"),
                                    ArcGISOnlineSharing);
                                string caption = 
                                    MappingControls.LocalizableStrings.GetString("ServiceConnectionErrorCaption");
                                MessageBoxDialog.Show(message, caption, MessageBoxButton.OK);
                            }
                        };
                    ArcGISOnlineEnvironment.ArcGISOnline.Initialized += onInitialized;

                    ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnlineEnvironment.LoadConfig(BuilderApplication.Instance.ArcGISOnlineSharing,
                        BuilderApplication.Instance.ArcGISOnlineSecure, BuilderApplication.Instance.ArcGISOnlineProxy,
                        false, true);
                }
                WriteChanges();
                raiseChange();
            }
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
                raiseChange();
            }
        }
        #endregion

        #region ViewerApplication
        public ViewerApplication ViewerApplication
        {
            get { return (ViewerApplication)GetValue(ViewerApplicationProperty); }
            set { SetValue(ViewerApplicationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewerApplication.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewerApplicationProperty =
            DependencyProperty.Register("ViewerApplication", typeof(ViewerApplication), typeof(BuilderAppSettingsViewModel), null);
        #endregion

        void WriteChanges()
        {
            //Write builder settings
            string xml = BuilderApplication.Instance.ToXml();
            ApplicationBuilderClient client = WCFProxyFactory.CreateApplicationBuilderProxy();
            client.SaveSettingsAsync(SETTINGSCONFIGPATH, xml);
        }


        public static void OnChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            BuilderAppSettingsViewModel source = o as BuilderAppSettingsViewModel;
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
        
        #region BingMapsAppId
        public string BingMapsAppId
        {
            get { return (string)GetValue(BingMapsAppIdProperty); }
            set { SetValue(BingMapsAppIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BingMapsAppId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BingMapsAppIdProperty =
            DependencyProperty.Register("BingMapsAppId", typeof(string), typeof(BuilderAppSettingsViewModel), new PropertyMetadata(OnChange));
        #endregion BingMapsAppId

        #region PortalAppId
        public string PortalAppId
        {
            get { return (string)GetValue(PortalAppIdProperty); }
            set { SetValue(PortalAppIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PortalAppId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PortalAppIdProperty =
            DependencyProperty.Register("PortalAppId", typeof(string), typeof(BuilderAppSettingsViewModel), new PropertyMetadata(OnChange));
        #endregion PortalAppId

        #region GeometryServiceUrl
        public string GeometryServiceUrl
        {
            get { return (string)GetValue(GeometryServiceUrlProperty); }
            set { SetValue(GeometryServiceUrlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GeometryServiceUrl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GeometryServiceUrlProperty =
            DependencyProperty.Register("GeometryServiceUrl", typeof(string), typeof(BuilderAppSettingsViewModel), new PropertyMetadata(OnChange));
        #endregion GeometryServiceUrl

        #region ArcGISOnlineSharing
        public string ArcGISOnlineSharing
        {
            get { return (string)GetValue(ArcGISOnlineSharingProperty); }
            set { SetValue(ArcGISOnlineSharingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ArcGISOnlineSharing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ArcGISOnlineSharingProperty =
            DependencyProperty.Register("ArcGISOnlineSharing", typeof(string), typeof(BuilderAppSettingsViewModel), new PropertyMetadata(OnChange));
        #endregion

        #region ArcGISOnlineSecure
        public string ArcGISOnlineSecure
        {
            get { return (string)GetValue(ArcGISOnlineSecureProperty); }
            set { SetValue(ArcGISOnlineSecureProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ArcGISOnlineSecure.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ArcGISOnlineSecureProperty =
            DependencyProperty.Register("ArcGISOnlineSecure", typeof(string), typeof(BuilderAppSettingsViewModel), new PropertyMetadata(OnChange));
        #endregion ArcGISOnlineSecure

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
            DependencyProperty.Register("Proxy", typeof(string), typeof(BuilderAppSettingsViewModel), 
            new PropertyMetadata(OnChange));

        #endregion Proxy
    }
}
