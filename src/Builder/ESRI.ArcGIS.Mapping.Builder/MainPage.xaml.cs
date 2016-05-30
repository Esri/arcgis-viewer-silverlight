/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Controls.MapContents;
using SearchTool;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Builder.Resources;
using ESRI.ArcGIS.Client.Portal;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Globalization;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name);
            ApplicationBuilderClient client = WCFProxyFactory.CreateApplicationBuilderProxy();
            client.GetConfigurationStoreXmlCompleted += new EventHandler<GetConfigurationStoreXmlCompletedEventArgs>(client_GetConfigurationStoreXmlCompleted);
            client.GetConfigurationStoreXmlAsync();

            DataContextChanged += MainPage_DataContextChanged;

            if (ViewerApplicationControl.Instance != null && BuilderApplication.Instance != null)
                ViewerApplicationControl.Instance.BuilderApplication = BuilderApplication.Instance;
        }

        void MainPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is BuilderApplication)
            {
                ((BuilderApplication)e.OldValue).PropertyChanged -= MainPage_PropertyChanged;
            }

            if (e.NewValue is BuilderApplication)
            {
                ((BuilderApplication)e.NewValue).PropertyChanged += MainPage_PropertyChanged;
            }
        }

        void MainPage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentSite")
            {
                BuilderApplication app = (BuilderApplication)sender;
                if (app.CurrentSite != null && !string.IsNullOrEmpty(app.CurrentSite.Name))
                {
                    Title.Blocks.Clear();
                    Run r = new Run()
                    {
                        Text = string.Format(Strings.ArcGISApplicationBuilder, app.CurrentSite.Name)
                    };
                    Paragraph p = new Paragraph();
                    p.Inlines.Add(r);
                    Title.Blocks.Add(p);

                    if (!string.IsNullOrEmpty(app.CurrentSite.Url))
                        Title.Hyperlink(app.CurrentSite.Name, app.CurrentSite.Url);
                }
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            BuilderApplication.Instance.TutorialDialogControl = TutorialControl;
            BuilderApplication.Instance.AddContentDialog = AddContentDialog;


            // Tooltips in RTL require the main page to be LTR, and LayoutRoot to be RTL
            FlowDirection = FlowDirection.LeftToRight;
        }

        void client_GetConfigurationStoreXmlCompleted(object sender, GetConfigurationStoreXmlCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;

            if (e.Error != null)
            {
                ESRI.ArcGIS.Mapping.Controls.Logger.Instance.LogError(e.Error);
                return;
            }

            string configurationStoreXml = e.ConfigurationStoreXml;
            if (!string.IsNullOrEmpty(configurationStoreXml))
            {
                BuilderApplication.Instance.ConfigurationStore = XmlSerializer.Deserialize<ConfigurationStore>(configurationStoreXml);
            }
        }

        void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TutorialControl.SetTabIndex(RibbonControl.TabControl.SelectedIndex);
        }   

        private void SitesCatalog_SiteOpened(object sender, SitesCatalog.SiteOpenedEventArgs e)
        {
            if (e.Site == null || string.IsNullOrWhiteSpace(e.Site.Url))
                return;

            BuilderApplication.Instance.CurrentSite = e.Site;
            BuilderApplication.Instance.BuilderScreenVisibility = System.Windows.Visibility.Visible;
            BuilderApplication.Instance.CatalogScreenVisibility = System.Windows.Visibility.Collapsed;
            BuilderApplication.Instance.NewappScreenVisibility = System.Windows.Visibility.Collapsed;

            SidePanelContainer.Visibility = System.Windows.Visibility.Collapsed;
            MapContentsHost.GoToMapContent(false);

            BuilderConfigurationProvider configProvider = ViewerApplicationControl.ConfigurationProvider as BuilderConfigurationProvider;
            if (configProvider != null)
                configProvider.MapXaml = null; // clear the map xaml

            string baseUrl = e.Site.Url;
            if (baseUrl.IndexOf("?", StringComparison.Ordinal) < 0 && !baseUrl.EndsWith("/", StringComparison.Ordinal)) // has no query parameter
                baseUrl += '/';
            ViewerApplicationControl.Instance.DefaultApplicationSettings = null;
            ViewerApplicationControl.BaseUri = new Uri(baseUrl, UriKind.Absolute);            
        }

        private void SitesCatalog_SiteCopy(object sender, SitesCatalog.SiteOpenedEventArgs e)
        {
            if (e.Site == null || string.IsNullOrWhiteSpace(e.Site.Url))
                return;

            // Create dialog control to prompt user for site information while passing in source site id that we
            // will eventually copy from.
            CopySiteDialogControl copySiteControl = new CopySiteDialogControl(e.Site.ID);

            // Display dialog control in modal mode
            BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.CopySite, copySiteControl, true);
        }

        private void ToggleSearch_Click(object sender, RoutedEventArgs e)
        {
            ToggleCommand("SearchTabItem");
        }

        private void ToggleAddContent_Click(object sender, RoutedEventArgs e)
        {
            ToggleCommand("BrowseTabItem");
        }

        private void ToggleMapContents_Click(object sender, RoutedEventArgs e)
        {
            ToggleCommand("MapContentsTabItem");
        }

        ToggleTableCommand toggleTableCommand;
        private void ToggleAttributeTable_Click(object sender, RoutedEventArgs e)
        {
            toggleTableCommand = toggleTableCommand ?? new ToggleTableCommand();
            toggleTableCommand.Execute(null);
        }

        private void ViewerApplicationControl_ToolbarsLoaded(object sender, EventArgs e)
        {
            initCommands();

            if (MapContentsHost != null)
            {
                if (MapContentsHost.MapContentControl != null)
                {
                    MapContentsHost.MapContentControl.Map = View.Instance.Map;
                    MapContentsHost.LayerConfiguration.View = View.Instance;
                    MapContentsHost.MapContentControl.Configuration = new MapContentsConfiguration { ContextMenuToolPanelName = "EditModeLayerConfigurationContextMenu", Mode = Mode.TopLevelLayersOnly };
                }
                if (MapContentsHost.LayerConfiguration != null)
                    MapContentsHost.LayerConfiguration.View = View.Instance;

                MapContentsHost.AddToolPanels();
            }

            if (AddContentDialog != null)
            {
                AddContentDialog.Map = View.Instance.Map;
                AddContentDialog.ConnectionsProvider = View.Instance.ConnectionsProvider;
                AddContentDialog.DataSourceProvider = View.Instance.DataSourceProvider;
            }
        }

        void LayoutColorPicker_LayoutChanged(object sender, EventArgs e)
        {
            initCommands();
        }

        private void initCommands()
        {
            toggleTableCommand = new ToggleTableCommand();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            BuilderApplication.Instance.CatalogScreenVisibility = Visibility.Collapsed;
            BuilderApplication.Instance.BuilderScreenVisibility = Visibility.Collapsed;
            BuilderApplication.Instance.NewappScreenVisibility = Visibility.Visible;
            SidePanelContainer.Visibility = System.Windows.Visibility.Collapsed;
            MapContentsHost.GoToMapContent(false);
        }

        private void Settings_ExtensionsCatalogChanged(object sender, EventArgs e)
        {
            // Refresh the commands which rely on the list of extensions            
            if(ManageToolbarCommand.Instance != null)
                ManageToolbarCommand.Instance.OnExtensionsCatalogChanged();

            if (AddMapBehaviorCommand.Instances != null)
            {
                foreach (AddMapBehaviorCommand cmd in AddMapBehaviorCommand.Instances)
                {
                    if (cmd != null)
                        cmd.OnExtensionsCatalogChanged();
                }
            }
        }

        private void ToggleCommand(string tabItemName)
        {
            int tabIndex = -1;

            for (int i = 0; i < SidePanelContainer.Items.Count; i++)
            {
                TabItem tab = SidePanelContainer.Items[i] as TabItem;
                if (tab.Name == tabItemName)
                {
                    tabIndex = i;
                    break;
                }
            }

            if (SidePanelContainer.SelectedIndex != tabIndex || SidePanelContainer.Visibility == Visibility.Collapsed)
            {
                SidePanelContainer.SelectedIndex = tabIndex;
                SidePanelContainer.Visibility = Visibility.Visible;
            }
            else
            {
                SidePanelContainer.Visibility = Visibility.Collapsed;
            }
        }

        private void AddContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            AddContentDialog.Map = View.Instance.Map;
            AddContentDialog.ConnectionsProvider = View.Instance.ConnectionsProvider;
            AddContentDialog.DataSourceProvider = View.Instance.DataSourceProvider;
        }

        private void AddContentDialog_ResourceSelected(object sender, ResourceSelectedEventArgs e)
        {
            ShowMapContentsTab();
        }

        private void AddContentDialog_LayerAdded(object sender, LayerAddedEventArgs e)
        {
            if (e.Layer is FeatureLayer)
            {
                FeatureLayer featureLayer = e.Layer as FeatureLayer;
                featureLayer.UpdateCompleted += applyAutomaticClustering;
            }

            View.Instance.AddLayerToMap(e.Layer, true, Core.LayerExtensions.GetTitle(e.Layer));
        }

        private void AddContentDialog_LayerAddFailed(object sender, ExceptionEventArgs e)
        {
            string paragraph = ESRI.ArcGIS.Mapping.Builder.Resources.Strings.AddLayerFailedExplanation;
            string errorMessage = String.IsNullOrEmpty(e.Exception.Message) ? ESRI.ArcGIS.Mapping.Builder.Resources.Strings.AddLayerFailedNoDetails : e.Exception.Message;
            string fullMessage = String.Format(paragraph, errorMessage);
            MessageBoxDialog.Show(fullMessage, ESRI.ArcGIS.Mapping.Builder.Resources.Strings.AddLayerFailed, MessageType.Error, MessageBoxButton.OK);
        }

        void applyAutomaticClustering(object sender, EventArgs e)
        {
            FeatureLayer featureLayer = sender as FeatureLayer;
            if (featureLayer != null && featureLayer.LayerInfo.GeometryType == Client.Tasks.GeometryType.Point)
            {
                GraphicsLayer gLayer = sender as GraphicsLayer;
                if (gLayer != null && gLayer.Graphics.Count >= Constants.AutoClusterFeaturesThresholdLimit)
                {
                    if (gLayer.Clusterer == null)
                        gLayer.Clusterer = new FlareClusterer();
                }
            }

            featureLayer.UpdateCompleted -= applyAutomaticClustering;
        }

        private void ShowMapContentsTab()
        {
            MapContentsHost.GoToMapContent(false);
            MapContentsHost.Visibility = Visibility.Visible;

            int mapContentsTabIndex = -1;
            for (int i = 0; i < SidePanelContainer.Items.Count; i++)
            {
                TabItem tab = SidePanelContainer.Items[i] as TabItem;
                if (tab.Name == "MapContentsTabItem")
                {
                    mapContentsTabIndex = i;
                    break;
                }
            }
            if (mapContentsTabIndex != -1)
                SidePanelContainer.SelectedIndex = mapContentsTabIndex;
        }

        private void TutorialDialogControl_Loaded(object sender, RoutedEventArgs e)
        {
            TutorialControl.sidePanel = SidePanelContainer;
            TutorialControl.syncRibbonControl = RibbonControl;
            DisplayGettingStartedMode(!TutorialControl.IsTutorialDisabled);
        }

        private void DisplayGettingStartedMode(bool showPanel)
        {
            BuilderApplication.Instance.GettingStartedVisibility = showPanel == true ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            PublishApplicationCommand cmd = new PublishApplicationCommand();
            if (cmd.CanExecute(null))
                cmd.Execute(true);
        }

        private void ViewerApplicationControl_ViewInitialized(object sender, ViewerApplicationControl.ViewEventArgs e)
        {
            e.View.WindowManager.DesignTimeWindowStyle = Application.Current.Resources["BuilderWindowStyle"] as Style;
        }

        private void ViewerApplicationControl_ViewLoaded(object sender, EventArgs e)
        {
            if (View.Instance != null && BuilderApplication.Instance != null)
                 BuilderApplication.Instance.LoadingOverlay = View.Instance.FindObjectInLayout("LoadingOverlay") as UIElement;

            SearchViewModel searchViewModel = BuilderSearchView.DataContext as SearchViewModel;
            if (searchViewModel.SearchProviders.Count > 0)
                return;

            ArcGISPortalServiceSearchProvider arcgisSearchProvider = new ArcGISPortalServiceSearchProvider()
                {
                    Portal = MapApplication.Current.Portal
                };

            ServiceSearchResultsView resultsView = arcgisSearchProvider.ResultsView as ServiceSearchResultsView;
            Style resultDetailsContainerStyle = Application.Current.Resources["PopupContentControl"] as Style;
            Style resultDetailsLeaderStyle = Application.Current.Resources["PopupLeader"] as Style;

            if (resultsView != null)
            {
                resultsView.ResultDetailsContainerStyle = resultDetailsContainerStyle;
                resultsView.ResultDetailsLeaderStyle = resultDetailsLeaderStyle;
            }
            searchViewModel.SearchProviders.Add(arcgisSearchProvider);

            GoogleServiceSearchProvider webSearchProvider = new GoogleServiceSearchProvider();
            resultsView = webSearchProvider.ResultsView as ServiceSearchResultsView;
            if (resultsView != null)
            {
                resultsView.ResultDetailsContainerStyle = resultDetailsContainerStyle;
                resultsView.ResultDetailsLeaderStyle = resultDetailsLeaderStyle;
            }
            searchViewModel.SearchProviders.Add(webSearchProvider);

            BuilderSearchView.DataContext = searchViewModel;
        }
    }
}
