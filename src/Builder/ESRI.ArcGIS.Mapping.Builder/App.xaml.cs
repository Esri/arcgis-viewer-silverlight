/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using ESRI.ArcGIS.Client.Application.Layout;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder;
using ESRI.ArcGIS.Mapping.Builder.Common;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Windowing;
using ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;
using System.Windows.Threading;
using ESRI.ArcGIS.Mapping.Builder.Resources;
using ESRI.ArcGIS.Client;
using System.Windows.Browser;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class App : Application
    {
        ErrorDisplay errorDisplay;

        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;

            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            #region Set IdentityManager Referer
            // Get URL
            string appUrl = HtmlPage.Document.DocumentUri.ToString();

            // Remove query string
            Uri appUri = new Uri(appUrl);
            if (!string.IsNullOrEmpty(appUri.Query))
                appUrl = appUrl.Replace(appUri.Query, "");

            if (appUrl.ToLower().Contains("default.aspx"))
                appUrl = appUrl.Substring(0, appUrl.Length - 12);

            // Set referer
            IdentityManager.Current.TokenGenerationReferer = Uri.EscapeUriString(appUrl);
            #endregion

            // Check if Language Culture is supported
            string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.ToString();
            ESRI.ArcGIS.Mapping.Core.SatelliteResources.Xap resourceXap = new ESRI.ArcGIS.Mapping.Core.SatelliteResources.Xap();
            if (!resourceXap.IsSupportedLanguage(cultureName))
            {
                cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
                if (!resourceXap.IsSupportedLanguage(cultureName)) // Region only lang.
                {
                    StartupAfterGettingResources(sender, e);	//Not supported, so call StartupAfterGettingResources
                    return;
                }
            }
            // Load satellite resource file if it exist.
            string resourceUri = string.Format("/Culture/Xaps/{0}{1}", cultureName, Application.Current.Host.Source.AbsolutePath);// insert language path
            int indexXap = Application.Current.Host.Source.AbsolutePath.LastIndexOf('/');
            if (indexXap > 0)
            { // resources is a sub-folder of site.
                string sitePath = Application.Current.Host.Source.AbsolutePath.Substring(0, indexXap);
                string resXap = Application.Current.Host.Source.AbsolutePath.Substring(indexXap + 1);
                resourceUri = string.Format("{0}/Culture/Xaps/{1}/{2}", sitePath, cultureName, resXap);
            }
            resourceUri = resourceUri.Replace(".xap", ".resources.xap");//update filename
            if (resourceUri.Contains("?")) resourceUri = resourceUri.Substring(0, resourceUri.IndexOf("?", StringComparison.Ordinal)); //remove parameters. 
            resourceXap.Load(resourceUri, delegate { StartupAfterGettingResources(sender, e); });

            RTLHelper helper = Application.Current.Resources["RTLHelper"] as RTLHelper;
            Debug.Assert(helper != null);
            if (helper != null)
                helper.UpdateFlowDirection();
        }

		private void StartupAfterGettingResources(object sender, StartupEventArgs e)
        {
            if (e.InitParams.ContainsKey("userId"))
            {
                BuilderApplication.Instance.UserId = WCFProxyFactory.UserId = e.InitParams["userId"];
            }

            ApplicationBuilderClient client = WCFProxyFactory.CreateApplicationBuilderProxy();
            client.GetTemplatesCompleted += (s, e1) =>
            {
                BuilderApplication.Instance.Templates = e1.Templates;

                ApplicationBuilderClient builderClient = WCFProxyFactory.CreateApplicationBuilderProxy();
                builderClient.GetSettingsXmlCompleted += onSettingsGet;
                builderClient.GetSettingsXmlAsync();
            };
            client.GetTemplatesAsync();
        }

        void onSettingsGet(object sender, GetSettingsXmlCompletedEventArgs args)
        {
            if (args.Error != null)
            {
                // Create a default viewer application
                BuilderApplication.Instance.TitleText = ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ArcGISViewerForMicrosoftSilverlight;
                loadAllExtensions();
                return;
            }
            BuilderApplication.InitFromXml(args.SettingsXml);

            EventHandler<EventArgs> onAgolInitialized = null;
            onAgolInitialized = (o, e) =>
                {
                    ArcGISOnlineEnvironment.ArcGISOnline.Initialized -= onAgolInitialized;

                    // Mark Builder as initialized.  Delay for half a second to allow bindings to update.
                    DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };
                    timer.Tick += (s, a) =>
                    {
                        timer.Stop();
                        BuilderApplication.Instance.IsInitialized = true;
                    };
                    timer.Start();
                };
            ArcGISOnlineEnvironment.ArcGISOnline.Initialized += onAgolInitialized;
            ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnlineEnvironment.LoadConfig(BuilderApplication.Instance.ArcGISOnlineSharing, BuilderApplication.Instance.ArcGISOnlineSecure, BuilderApplication.Instance.ArcGISOnlineProxy);

            loadAllExtensions();
        }

        private void loadAllExtensions()
        {
            ApplicationBuilderClient client = WCFProxyFactory.CreateApplicationBuilderProxy();
            client.GetExtensionLibrariesCompleted += new EventHandler<GetExtensionLibrariesCompletedEventArgs>(client_GetExtensionLibrariesCompleted);
            client.GetExtensionLibrariesAsync();
        }

        void client_GetExtensionLibrariesCompleted(object sender, GetExtensionLibrariesCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
            {
                if(e.Error != null)
                    Logger.Instance.LogError(e.Error);
                loadUI();
                return;
            }

            BuilderApplication.Instance.ExtensionsRepositoryBaseUrl = e.ExtensionsRepositoryBaseUrl;
            List<string> extensionUrls = new List<string>();
            foreach (Extension extension in e.Extensions)
            {
                string url = extension.Url;
                if (!string.IsNullOrEmpty(url) && ! extensionUrls.Contains(url))
                      extensionUrls.Add(extension.Url);
                BuilderApplication.Instance.AllExtensions.Add(extension);
            }
            ESRI.ArcGIS.Mapping.Core.ExtensionsManager.LoadAllExtensions(extensionUrls, extensionsProvider_ExtensionsLoadComplete, extensionsProvider_ExtensionLoadFailed);            
        }

        bool hasStartupExtensionLoadFailedEvent;
       void extensionsProvider_ExtensionLoadFailed(object sender, ExceptionEventArgs args)
       {
           hasStartupExtensionLoadFailedEvent = true;
           NotificationPanel.Instance.RaiseExtensionLoadFailed(sender, args);
        }

        void extensionsProvider_ExtensionsLoadComplete(object sender, EventArgs args)
        {
            loadUI();
        }

        MainPage mainPage;
        private void loadUI()
        {
            MapApplication.SetApplication(BuilderApplication.Instance);
            AppCoreHelper.SetService(new ApplicationServices());
 
            this.RootVisual = mainPage = new MainPage()
            {
                DataContext = BuilderApplication.Instance,
            };
            ESRI.ArcGIS.Mapping.Core.Utility.SetRTL(mainPage);
            ImageUrlResolver.RegisterImageUrlResolver(new BuilderImageUrlResolver());
            WebClientFactory.Initialize();

            NotificationPanel.Instance.Initialize();

            if (!NotificationPanel.Instance.OptedOutOfNotification)
            {
                EventHandler handler = null;
                handler = (o, e) =>
                {
                    if (NotificationPanel.Instance.OptedOutOfNotification)
                    {
                        NotificationPanel.Instance.NotificationsUpdated -= handler;
                        return;
                    }

                    showExtensionLoadFailedMessageOnStartup();
                };
                NotificationPanel.Instance.NotificationsUpdated += handler;
                if (hasStartupExtensionLoadFailedEvent)
                {
                    showExtensionLoadFailedMessageOnStartup();
                }
            }
        }

        private static void showExtensionLoadFailedMessageOnStartup()
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
				BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ExtensionNotifications, NotificationPanel.Instance, false, null, null);
            }
            );
        }
        
        private void Application_Exit(object sender, EventArgs e)
        {
            if (View.Instance != null)
                    View.Instance.Clear();
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (e.Handled || ESRI.ArcGIS.Mapping.Core.AsyncExtensions.DownloadStringTaskInProgress)
                return; 

            Logger.Instance.LogError(e.ExceptionObject);
            if (e.ExceptionObject != null && e.ExceptionObject.StackTrace != null &&
            e.ExceptionObject.StackTrace.Contains("ESRI.ArcGIS.Client.WebMap.MapConfiguration"))
            {
                // if loading a webmap caused the problem kill the loading indicator
                if (BuilderApplication.Instance.MapCenter != null && BuilderApplication.Instance.MapCenter.LoadingMapIndicator != null)
                    BuilderApplication.Instance.MapCenter.LoadingMapIndicator.Visibility = Visibility.Collapsed;
            }
            e.Handled = true;
            ReportErrorToUser(e, LayerErrorHandler.HandleIfLayerError(e.ExceptionObject));
        }

        private void ReportErrorToUser(ApplicationUnhandledExceptionEventArgs e, string errorMessage)
        {
            if (e.ExceptionObject != null)
            {
                errorDisplay = new ErrorDisplay();

                // Attempt to get the stack trace with IL offsets
                string stackTraceIL = e.ExceptionObject.StackTraceIL();

                ErrorData data = new ErrorData()
                {
                    Message = errorMessage ?? e.ExceptionObject.Message,
                    StackTrace = !string.IsNullOrEmpty(stackTraceIL) ? stackTraceIL :
                        e.ExceptionObject.StackTrace
                };

                errorDisplay.DataContext = data;

                if (Application.Current.RootVisual != null)
                {
                    // Size the error UI
                    double width = Application.Current.RootVisual.RenderSize.Width * 0.67;
                    errorDisplay.Width = width > errorDisplay.MaxWidth ? errorDisplay.MaxWidth : width;
                    errorDisplay.Completed += new EventHandler<EventArgs>(errorDisplay_Completed);

                    // Show the error
                    BuilderApplication.Instance.ShowWindow(Strings.ErrorOccured, errorDisplay, false, null, null);
                }
                else
                {
                    MessageBox.Show(data.Message + "\n" + data.StackTrace, Strings.ErrorOccured, MessageBoxButton.OK);
                }
            }
        }

        void errorDisplay_Completed(object sender, EventArgs e)
        {
            BuilderApplication.Instance.HideWindow(errorDisplay);
        }

    }
}
