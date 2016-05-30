/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Resources;
using System.Windows.Markup;
using System.Windows.Data;
using ESRI.ArcGIS.Mapping.GP.MetaData;
using System.Windows.Controls;
using ESRI.ArcGIS.Mapping.GP.ParameterSupport;
using System.Windows.Media;

namespace ESRI.ArcGIS.Mapping.GP
{
    [Export(typeof(ICommand))]
	[DisplayName("GeoprocessingDisplayName")]
	[Category("CategoryGeoprocessing")]
	[Description("GeoprocessingDescription")]
    public class GeoprocessingCommand : ICommand, ISupportsWizardConfiguration
    {
        // Wizard pages
        private WizardPage browsePage;
        private WizardPage titleAndLayerOrderPage;
        private WizardPage inputParamsPage;
        private WizardPage outputParamsPage;
        private WizardPage errorPage;

        private BrowseContentDialog browseDialog;
        private GPConfiguration configuration;
        private GPConfiguration _backupConfiguration;
        private bool _useProxy;

        // Object for loading GP service endpoint parameters
        MetaDataLoader metadataLoader = new MetaDataLoader();

        // Flag to indicated whether GP configuration was loaded from serialized data.  Used to
        // bypass re-initializing UI from service endpoint in the serialized case.
        private bool gpConfigSerialized = false;

        public GeoprocessingCommand()
        {
            if (MapApplication.Current.IsEditMode)
            {
                metadataLoader.LoadFailed += GPTaskEndpoint_LoadFailed;
                metadataLoader.LoadSucceeded += GPTaskEndpoint_LoadSucceeded;

                // initialize wizard pages
                initializeBrowsePage();
                inputParamsPage = new WizardPage()
                {
                    Heading = ESRI.ArcGIS.Mapping.GP.Resources.Strings.ConfigureInputHeading,
                    InputValid = true
                };
                outputParamsPage = new WizardPage()
                {
                    Heading = ESRI.ArcGIS.Mapping.GP.Resources.Strings.ConfigureOutputHeading,
                    InputValid = true
                };
                titleAndLayerOrderPage = new WizardPage()
                {
                    Heading = ESRI.ArcGIS.Mapping.GP.Resources.Strings.ConfigureTitleAndLayerOrder,
                    InputValid = true
                };
                errorPage = new WizardPage()
                {
                    Heading = new TextBlock()
                    {
                        Text = ESRI.ArcGIS.Mapping.GP.Resources.Strings.FailedToLoadTaskInformation,
                        FontWeight = FontWeights.Bold,
                    },
                    InputValid = false
                };
            }
        }

        public Map Map
        {
            get { return MapApplication.Current.Map; }
        }

        #region ICommand members

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        GPWidget gpWidget;
        public void Execute(object parameter)
        {
            if (configuration == null)
            {
				MessageBoxDialog.Show(Resources.Strings.GeoprocessingHasNotBeenconfigured, Resources.Strings.ConfigurationError, MessageBoxButton.OK);
                return;
            }
            if (gpWidget != null)
                MapApplication.Current.HideWindow(gpWidget);

            // Update proxy URL if proxy is used
            if (configuration.UseProxy && MapApplication.Current != null && MapApplication.Current.Urls != null)
                configuration.ProxyUrl = MapApplication.Current.Urls.ProxyUrl;

            gpWidget = new GPWidget();
            gpWidget.Configuration = configuration;
            gpWidget.Map = Map;
            gpWidget.ExecuteCompleted += new EventHandler<GPWidget.ExecuteCompleteEventArgs>(gpWidget_ExecuteCompleted);
            SingleWindow.Current.ShowWindow(MapApplication.Current, configuration.Title, gpWidget);
        }

        protected virtual void OnCanExecuteChanged(EventArgs args)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, args);
        }

        #endregion

        #region ISupportsConfiguration members

        public void Configure() { }

        public void LoadConfiguration(string configData)
        {
            gpConfigSerialized = !string.IsNullOrEmpty(configData);
            configuration = gpConfigSerialized ? new GPConfiguration(configData) : null;
            if (browseDialog != null)
                browseDialog.ShowRestrictedServices = configuration.UseProxy;

            _backupConfiguration = gpConfigSerialized ? new GPConfiguration(configData) : null;

            if (MapApplication.Current.IsEditMode)
            {
                if (configuration != null && configuration.TaskEndPoint != null)
                    browseDialog.SelectedResourceUrl = configuration.TaskEndPoint.AbsoluteUri;

                // Update pages
                updateInputParamsPage();
                updateOutputParamsPage();
                updateTitleAndLayerOrderPage();
            }
        }

        public string SaveConfiguration()
        {
            if (configuration != null)
                return configuration.ToJson();
            return null;
        }

        #endregion

        #region ISupportsWizardConfiguration members

        public WizardPage CurrentPage { get; set; }

        // Size of the GP configuration UI
        private Size desiredSize = new Size(315, 330);
        public Size DesiredSize
        {
            get { return desiredSize; }
            set { desiredSize = value; }
        }

        public bool PageChanging()
        {
            return true;
        }

        private ObservableCollection<WizardPage> pages;
        public ObservableCollection<WizardPage> Pages
        {
            get
            {
                if (pages == null)
                    addConfigPages();
                return pages;
            }
            set { pages = value; }
        }

        public void OnCancelled()
        {
            browseDialog.Reset(); //as if it has not yet been configured

            if (_backupConfiguration != null)
            {
                browseDialog.ShowRestrictedServices = _backupConfiguration.UseProxy;

                if (_backupConfiguration.TaskEndPoint != null)
                {
                    // Check whether the proxy URL has changed since the last time tool configuration was saved
                    bool proxyChanged = MapApplication.Current != null && MapApplication.Current.Urls != null
                        && _backupConfiguration.ProxyUrl != MapApplication.Current.Urls.ProxyUrl;

                    // Only revert the URL on the service browsing UI if proxy is not being used or the proxy 
                    // URL is the same as the last time configuration was saved
                    if (!_backupConfiguration.UseProxy || !proxyChanged)
                        browseDialog.SelectedResourceUrl = _backupConfiguration.TaskEndPoint.AbsoluteUri;
                    else
                        browseDialog.SelectedResourceUrl = null;
                }

                configuration = _backupConfiguration.Clone();
            }
            else
            {
                browseDialog.SelectedResourceUrl = null;
                configuration = null;
            }

            //refresh the ui with previous settings
            updateInputParamsPage();
            updateOutputParamsPage();
            updateTitleAndLayerOrderPage();
        }

        public void OnCompleted()
        {
            _backupConfiguration = configuration != null ? configuration.Clone() : null;
        }

        #endregion

        private void gpWidget_ExecuteCompleted(object sender, GPWidget.ExecuteCompleteEventArgs e)
        {
            if (!((gpWidget.LatestErrors != null && gpWidget.LatestErrors.Count > 0) //errors need to be displayed
              || GPResultWidget.AreDisplayable(gpWidget.Configuration.OutputParameters))) //results need to be displayed in control
            {
                MapApplication.Current.HideWindow(gpWidget);
            }
        }

        #region GP Task Selection Event Handlers

        private void GPTaskEndpoint_Changed(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(browseDialog.SelectedResourceUrl))
                return;
            else if (gpConfigSerialized)
            {
                // Reset bypass flag
                gpConfigSerialized = false;
                return;
            }

            // Remove error page if present
            if (Pages.Contains(errorPage))
                Pages.Remove(errorPage);

            // Check whether proxy is being used
            _useProxy = browseDialog.ShowRestrictedServices;
            string proxyUrl = _useProxy && MapApplication.Current != null && MapApplication.Current.Urls != null ?
                MapApplication.Current.Urls.ProxyUrl : null;
    
            // Check whether the component is being shown at run-time or in a designer (e.g. Visual Studio)
            bool designMode = System.ComponentModel.DesignerProperties.GetIsInDesignMode(Application.Current.RootVisual);

            // Retrieve info from selected GP task endpoint
            metadataLoader.ServiceEndpoint = new Uri(browseDialog.SelectedResourceUrl);
            metadataLoader.LoadMetadata(designMode, proxyUrl);
        }

        private void GPTaskEndpoint_LoadSucceeded(object sender, EventArgs e)
        {
            if (configuration == null)
                configuration = new GPConfiguration();

            configuration.LoadConfiguration(metadataLoader.ServiceInfo, metadataLoader.ServiceEndpoint);
            configuration.UseProxy = _useProxy;

            // Update pages
            updateInputParamsPage();
            updateOutputParamsPage();
            updateTitleAndLayerOrderPage();
        }

        private void GPTaskEndpoint_LoadFailed(object sender, EventArgs e)
        {
            // Initialize ContentControl to hold error details
            if (errorPage.Content == null)
            {
                errorPage.Content = new ContentControl()
                {
                    Content = metadataLoader.Error.Message,
                    Style = ResourceUtility.LoadEmbeddedStyle("Themes/GPErrorPageStyle.xaml", "GPErrorPageStyle")
                };
            }
            else
                ((ContentControl)errorPage.Content).Content = metadataLoader.Error.Message;

            // Add error page
            if (!Pages.Contains(errorPage))
                Pages.Insert(1, errorPage);
        }

        #endregion

        #region Config UI Generation Methods

        private void addConfigPages()
        {
            // Add config pages to wizard
            pages = new ObservableCollection<WizardPage>();
            pages.Add(browsePage);
            pages.Add(inputParamsPage);
            pages.Add(outputParamsPage);
            pages.Add(titleAndLayerOrderPage);
        }

        private void initializeBrowsePage()
        {
            // Create browse control
            browseDialog = new BrowseContentDialog()
            {
                Filter = Core.DataSources.Filter.GeoprocessingServices,
                DataSourceProvider = ViewUtility.GetDataSourceProvider(),
                ConnectionsProvider = ViewUtility.GetConnectionsProvider(),
                Style = ResourceUtility.LoadEmbeddedStyle("Themes/GPBrowseStyle.xaml", "GPBrowseStyle"),
                SelectedResourceUrl = configuration != null && configuration.TaskEndPoint != null ?
                    configuration.TaskEndPoint.AbsoluteUri : null,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch
            };

            if (configuration != null)
                browseDialog.ShowRestrictedServices = configuration.UseProxy;

            // Encapsulate browse control in a wizard page
            browsePage = new WizardPage()
            {
                Content = browseDialog,
                Heading = ESRI.ArcGIS.Mapping.GP.Resources.Strings.BrowseToGP,                
            };

            // Bind the selected resource URL of the browse control to whether the input 
            // of the browse page is valid.  Validation state will be updated when the 
            // selected resource is updated.
            Binding b = new Binding("SelectedResourceUrl")
            {
                Source = browseDialog,
                Converter = new IsNotNullOrEmptyConverter()
            };
            BindingOperations.SetBinding(browsePage, WizardPage.InputValidProperty, b);

            // When a new GP task is selected, wire handler to retrieve info from service endoint
            browseDialog.SelectedResourceChanged += GPTaskEndpoint_Changed;
        }

        private void updateTitleAndLayerOrderPage()
        {
            ContentControl titleContent = new ContentControl()
            {
                Style = ResourceUtility.LoadEmbeddedStyle("Themes/GPTitlePageStyle.xaml", "GPTitlePageStyle"),
                DataContext = configuration
            };
            titleAndLayerOrderPage.Content = createPageContent(titleContent);
            // Add page to collection if not already present.  This is always the last page, so simply add it.
            if (!Pages.Contains(titleAndLayerOrderPage))
                Pages.Add(titleAndLayerOrderPage);
        }

        private void updateInputParamsPage()
        {
            // Update input parameters page
            if (configuration != null && configuration.InputParameters != null && configuration.InputParameters.Count > 0)
            {
                // Create input params page content and add page to collection if not already present
                inputParamsPage.Content = createPageContent(GPConfigUIBuilder.GenerateParameterUI(configuration.InputParameters));
                if (!Pages.Contains(inputParamsPage))
                    Pages.Insert(1, inputParamsPage);
            }
            // Remove input params page if current GP task has no input parameters
            else if (Pages.Contains(inputParamsPage))
                Pages.Remove(inputParamsPage);
        }

        private void updateOutputParamsPage()
        {
            // Update output parameters page
            if (configuration != null && configuration.OutputParameters != null && configuration.OutputParameters.Count > 0)
            {
                // Create output params page content
                outputParamsPage.Content = createPageContent(GPConfigUIBuilder.GenerateParameterUI(configuration.OutputParameters));
                // Add page to collection if not already present.  Note that insertion index depends on whether
                // input prams page is present.
                if (!Pages.Contains(outputParamsPage) && Pages.Contains(inputParamsPage))
                    Pages.Insert(2, outputParamsPage);
                else if (!Pages.Contains(outputParamsPage))
                    Pages.Insert(1, outputParamsPage);
            }
            // Remove output params page if current GP task has no output parameters
            else if (Pages.Contains(outputParamsPage))
                Pages.Remove(outputParamsPage);
        }

        private ContentControl createPageContent(object content)
        {
            ContentControl pageContent = new ContentControl()
            {
                Content = content,
                Style = ResourceUtility.LoadEmbeddedStyle("Themes/GPContentStyle.xaml", "GPContentStyle"),
                DataContext = configuration,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
            pageContent.EnforceForegroundOnChildTextBlocks();
            return pageContent;
        }

        #endregion
    }
}
