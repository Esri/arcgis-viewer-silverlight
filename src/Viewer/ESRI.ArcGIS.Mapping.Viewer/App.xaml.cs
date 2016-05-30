/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using ESRI.ArcGIS.Client.Application.Layout;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Core;
using System.Xml.Linq;
using System.Globalization;
using ESRI.ArcGIS.Mapping.Windowing;
using ESRI.ArcGIS.Client;
using System.Windows.Browser;

namespace ESRI.ArcGIS.Mapping.Viewer
{
    public partial class App : Application
    {

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

            if (appUrl.ToLower().Contains("index.htm"))
                appUrl = appUrl.Substring(0, appUrl.Length - 9);

            // Set referer
            IdentityManager.Current.TokenGenerationReferer = Uri.EscapeUriString(appUrl);
            #endregion

            var result = WebRequest.RegisterPrefix("http://", ArcGISTokenWebRequestProvider.Instance);
            result = WebRequest.RegisterPrefix("https://", ArcGISTokenWebRequestProvider.Instance);

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
            // update the RTLHelper instance
            RTLHelper helper = Application.Current.Resources["RTLHelper"] as RTLHelper;
            Debug.Assert(helper != null);
            if (helper != null)
                helper.UpdateFlowDirection();
        }

        private void StartupAfterGettingResources(object sender, StartupEventArgs e)
        {
            string baseUrl = getBaseUri(e);
            MainPage mainPage = null;

            ViewerApplicationControl viewerControl = new ViewerApplicationControl() {
                     BaseUri = new Uri(baseUrl, UriKind.Absolute) };
            this.RootVisual = mainPage = new MainPage() { Content = viewerControl };
            viewerControl.ViewInitialized += (o, args) => 
                { viewerControl.View.ApplicationColorSet.SyncDesignHostBrushes = true; };

            RTLHelper helper = Application.Current.Resources["RTLHelper"] as RTLHelper;
            Debug.Assert(helper != null);
            if (helper != null)
                mainPage.FlowDirection = helper.FlowDirection;

            WebClientFactory.Initialize();
            ImageUrlResolver.RegisterImageUrlResolver(new UrlResolver(baseUrl));
        }

        private static string getBaseUri(StartupEventArgs e)
        {
            string baseUrl = null;
            if (e.InitParams.ContainsKey("url"))
            {
                // If a url is passed in via init-params (multi-tenant viewer only)
                baseUrl = e.InitParams["url"];
            }
            else
            {
                // else use the url of the .xap file 
                string absUrl = Application.Current.Host.Source.AbsoluteUri;
                baseUrl = absUrl.Substring(0, absUrl.LastIndexOf('/')+1);
            }
            return baseUrl;
        }

        private void Application_Exit(object sender, EventArgs e)
        {

        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            ESRI.ArcGIS.Mapping.Controls.Logger.Instance.LogError(e.ExceptionObject);
            LayerErrorHandler.HandleIfLayerError(e.ExceptionObject);
            e.Handled = true;
        }

        ErrorDisplay errorDisplay;
        private void ReportErrorToUser(ApplicationUnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject != null)
            {
                errorDisplay = new ErrorDisplay();

                // Attempt to get the stack trace with IL offsets
                string stackTraceIL = e.ExceptionObject.StackTraceIL();

                ErrorData data = new ErrorData()
                {
                    Message = e.ExceptionObject.Message,
                    StackTrace = !string.IsNullOrEmpty(stackTraceIL) ? stackTraceIL : 
                        e.ExceptionObject.StackTrace
                };

                errorDisplay.DataContext = data;

                // Size the error UI
                double width = Application.Current.RootVisual.RenderSize.Width * 0.67;
                errorDisplay.Width = width > errorDisplay.MaxWidth ? errorDisplay.MaxWidth : width;
                errorDisplay.Completed += new EventHandler<EventArgs>(errorDisplay_Completed);

                // Show the error
                MapApplication.Current.ShowWindow(StringResourcesManager.Instance.Get("ErrorCaption"), 
                    errorDisplay);
            }
        }

        private void errorDisplay_Completed(object sender, EventArgs e)
        {
            MapApplication.Current.HideWindow(errorDisplay);
        }
    }

    public class UrlResolver : ESRI.ArcGIS.Mapping.Core.IUrlResolver
    {
        private string m_baseUrl;
        public UrlResolver(string baseUrl)
        {
            m_baseUrl = baseUrl;
            if(!string.IsNullOrEmpty(m_baseUrl))
            m_baseUrl = m_baseUrl.TrimEnd('/');
        }

        public string ResolveUrl(string url)
        {
            if (url == null)
                return null;
            return string.Format("{0}/{1}", m_baseUrl, url.TrimStart('/'));
        }
    }
}
