/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder;
using ESRI.ArcGIS.Mapping.Builder.Common;
using ESRIControls = ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Builder.Resources;
using System.Linq;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class SitesCatalog : UserControl
    {
        private ObservableCollection<Site> CurrentSites;
       
        public SitesCatalog()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            "IsBusy", typeof(bool), typeof(SitesCatalog), null);

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly DependencyProperty StatusMessageProperty = DependencyProperty.Register(
            "StatusMessage", typeof(string), typeof(SitesCatalog), null);

        public string StatusMessage
        {
            get { return GetValue(StatusMessageProperty) as string; }
            set { SetValue(StatusMessageProperty, value); }
        }

        private void SitesCatalog_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationBuilderClient client = WCFProxyFactory.CreateApplicationBuilderProxy();
            client.GetSitesCompleted += new EventHandler<GetSitesCompletedEventArgs>(client_GetSitesCompleted);
            startProgressIndicator(Strings.LoadingSites);
            client.GetSitesAsync();
        }

        void client_GetSitesCompleted(object sender, GetSitesCompletedEventArgs e)
        {
            stopProgressIndicator();
            if(e.Cancelled || e.Error != null || e.Sites == null)
                return;

            BuilderApplication.Instance.Sites = e.Sites;
            BuilderApplication.Instance.Sites.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(BuilderApplication.Instance.Sites_CollectionChanged);
            SitesList.ItemsSource = CurrentSites = e.Sites;
            BuilderApplication.Instance.RefreshCatalogVisibility();
        }

        private void SiteEdit_Click(object sender, RoutedEventArgs e)
        {
            if (SiteOpened != null)
            {
                // Get global instance of the getting started control and display it if getting started mode is not disabled
                TutorialDialogControl tdc = BuilderApplication.Instance.TutorialDialogControl;
                BuilderApplication.Instance.GettingStartedVisibility = tdc.IsTutorialDisabled == false ? Visibility.Visible : Visibility.Collapsed;

                SiteOpened(this, new SiteOpenedEventArgs() { Site = ((FrameworkElement)sender).DataContext as Site });
            }
        }

        private void SiteCopy_Click(object sender, RoutedEventArgs e)
        {
            if (SiteCopy != null)
            {
                SiteCopy(this, new SiteOpenedEventArgs() { Site = ((FrameworkElement)sender).DataContext as Site });
            }
        }

        private void SiteDelete_Click(object sender, RoutedEventArgs e)
        {
            ESRIControls.MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.AreYouSureYouWantToDeleteWebSite, ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ConfirmDelete, MessageBoxButton.OKCancel,
                    new ESRIControls.MessageBoxClosedEventHandler(delegate(object obj, ESRIControls.MessageBoxClosedArgs args)
                    {
                        if (args.Result == MessageBoxResult.OK)
                        {
                            Site site = ((FrameworkElement)sender).DataContext as Site;
							startProgressIndicator(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.DeletingSite);
                            ApplicationBuilderClient client = WCFProxyFactory.CreateApplicationBuilderProxy();
                            client.DeleteWebSiteCompleted += new EventHandler<DeleteWebSiteCompletedEventArgs>(client_DeleteWebSiteCompleted);
                            client.DeleteWebSiteAsync(site.ID, site);
                        }
                    }));
        }

        // Upgrade site to the current version
        private void SiteUpgrade_Click(object sender, RoutedEventArgs e)
        {
            // Give the user feedback
            startProgressIndicator(Strings.UpgradingApplication);

            // Get the button that was clicked
            FrameworkElement element = (FrameworkElement)sender;

            // Get the site to be upgraded
            Site site = element.DataContext as Site;

            // Hook to the upgrade completed event
            ApplicationBuilderClient client = WCFProxyFactory.CreateApplicationBuilderProxy();
            client.UpgradeSiteCompleted += (o, args) => 
            {
                if (args.Error != null)
                {
                    ESRIControls.MessageBoxDialog.Show(args.Error.Message);
                }
                else
                {
                    // update the product version on the current site object
                    site.ProductVersion = args.Site.ProductVersion;

                    // Since the Site object is not a dependency object, 
                    Grid parent = element.FindAncestorOfType<Grid>();
                    if (parent != null)
                    {
                        parent.DataContext = null;
                        parent.DataContext = site;
                    }
                }

                // Hide progress indicator
                stopProgressIndicator();
            };

            // Get ID of current template
            string templateID = null;
            if (BuilderApplication.Instance != null && BuilderApplication.Instance.Templates != null)
                templateID = BuilderApplication.Instance.Templates.FirstOrDefault(t => t.IsDefault).ID;

            if (templateID == null)
                templateID = "Default";

            // Do upgrade
            client.UpgradeSiteAsync(site.ID, templateID);
        }

        void client_DeleteWebSiteCompleted(object sender, DeleteWebSiteCompletedEventArgs e)
        {
            stopProgressIndicator();
            if (CurrentSites != null)
                CurrentSites.Remove(e.UserState as Site);
        }

        private void startProgressIndicator(string progressText)
        {
            IsBusy = true;
            StatusMessage = progressText ?? ESRI.ArcGIS.Mapping.Builder.Resources.Strings.PleaseWait;
        }

        private void stopProgressIndicator()
        {
            IsBusy = false;
        }

        public class SiteOpenedEventArgs : EventArgs
        {            
            public Site Site { get; set; }
        }

        public event EventHandler<SiteOpenedEventArgs> SiteOpened;
        public event EventHandler<SiteOpenedEventArgs> SiteCopy;
    }
}
