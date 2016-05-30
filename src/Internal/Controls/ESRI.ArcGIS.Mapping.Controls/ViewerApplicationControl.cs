/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;
using ESRI.ArcGIS.Client.Application.Layout;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Controls;
using System.IO;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Mapping.Controls.MapContents;
using ESRI.ArcGIS.Client.Portal;
using ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;
using System.Linq;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public partial class ViewerApplicationControl : ContentControl
    {
        private static ViewerApplicationControl _instance;
        public static ViewerApplicationControl Instance { get { return _instance; } }
        static void SetInstance(ViewerApplicationControl instance)
        {
            _instance = instance;
        }

        public ViewerApplicationControl()
        {
            VerticalContentAlignment = System.Windows.VerticalAlignment.Stretch;
            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
            ToolPanels = new ToolPanels();
            BehaviorsConfiguration = new Controls.BehaviorsConfiguration();
            SetInstance(this);
        }

        #region BaseUri
        /// <summary>
        /// 
        /// </summary>
        public Uri BaseUri
        {
            get { return GetValue(BaseUriProperty) as Uri; }
            set { SetValue(BaseUriProperty, value); }
        }

        /// <summary>
        /// Identifies the BaseUri dependency property.
        /// </summary>
        public static readonly DependencyProperty BaseUriProperty =
            DependencyProperty.Register(
                "BaseUri",
                typeof(Uri),
                typeof(ViewerApplicationControl),
                new PropertyMetadata(null, OnBaseUriPropertyChanged));

        /// <summary>
        /// BaseUriProperty property changed handler.
        /// </summary>
        /// <param name="d">ViewerApplicationControl that changed its BaseUri.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnBaseUriPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            ViewerApplicationControl source = d as ViewerApplicationControl;
            source.disposeExtensibilityObjects();
            source.DisposeView();
            SidePanelHelper.Reset();
            source.initializeViewerApplication(true);
        }
        #endregion 
        
        private string ApplicationSettingsFilePath { get; set; }
        private string LayoutFilePath { get; set; }
        private string ConfigurationStoreFilePath { get; set; }
        private string ConnectionsFileFilePath { get; set; }
        private string MapConfigurationFilePath { get; set; }
        private string SymbolConfigurationFilePath { get; set; }
        private string ToolsConfigurationFilePath { get; set; }
        private string BehaviorsConfigurationFilePath { get; set; }
        private string ControlsConfigurationFilePath { get; set; }
        private string ClassBreaksColorGradientsConfigFileUrl { get; set; }
        private string HeatMapColorGradientsConfigFileUrl { get; set; }
        private string SymbolFolderParentUrl { get; set; }
        private string UniqueValueColorGradientsConfigFileUrl { get; set; }
        private string ThemeFilePath { get; set; }
        private string ApplicationColorsFilePath { get; set; }
        
        private string layoutFileContents = null;
        private string applicationSettingsFileContents = null;
        private string toolsXmlFileContents = null;
        private string behaviorsXmlFileContents = null;
        private string controlsXmlFileContents = null;
        private string applicationColorsFileContents = null;
        private List<ResourceDictionary> layoutResourceDictionaries = null;
        private bool m_IsDownloadingApplicationSettingsFile = false;        

        #region View
        /// <summary>
        /// 
        /// </summary>
        public View View
        {
            get { return GetValue(ViewProperty) as View; }
            set { SetValue(ViewProperty, value); }
        }

        /// <summary>
        /// Identifies the View dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewProperty =
            DependencyProperty.Register(
                "View",
                typeof(View),
                typeof(ViewerApplicationControl),
                new PropertyMetadata(null, OnViewPropertyChanged));

        private static void OnViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            View view = e.NewValue as View;
            ViewerApplicationControl viewer = d as ViewerApplicationControl;
            view.SelectionChanged -= viewer.view_SelectionChanged;
            view.SelectionChanged += viewer.view_SelectionChanged;
        }

        private void view_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (ToolPanel toolPanel in ToolPanels)
                toolPanel.Refresh();
        }
        #endregion        

        #region Toolbars
        /// <summary>
        /// 
        /// </summary>
        public ToolPanels ToolPanels
        {
            get { return GetValue(ToolPanelsProperty) as ToolPanels; }
            set { SetValue(ToolPanelsProperty, value); }
        }

        /// <summary>
        /// Identifies the ToolPanel dependency property.
        /// </summary>
        public static readonly DependencyProperty ToolPanelsProperty =
            DependencyProperty.Register(
                "ToolPanels",
                typeof(ToolPanels),
                typeof(ViewerApplicationControl),
                new PropertyMetadata(null));
        #endregion

        #region Behaviors
        /// <summary>
        /// 
        /// </summary>
        internal BehaviorsConfiguration BehaviorsConfiguration
        {
            get;
            set;
        }
		#endregion

        #region ViewerApplication
        /// <summary>
        /// 
        /// </summary>
        public ViewerApplication ViewerApplication
        {
            get { return GetValue(ViewerApplicationProperty) as ViewerApplication; }
            set { SetValue(ViewerApplicationProperty, value); }
        }

        /// <summary>
        /// Identifies the ViewerApplication dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewerApplicationProperty =
            DependencyProperty.Register(
                "ViewerApplication",
                typeof(ViewerApplication),
                typeof(ViewerApplicationControl),
                new PropertyMetadata(OnViewerApplicationPropertyChanged));

        private static void OnViewerApplicationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ViewerApplicationControl ctrl = (ViewerApplicationControl)d;

            if (e.OldValue != null)
                ((ViewerApplication)e.OldValue).PropertyChanged -= ctrl.ViewerApplication_PropertyChanged;

            if (e.NewValue != null)
                ((ViewerApplication)e.NewValue).PropertyChanged += ctrl.ViewerApplication_PropertyChanged;
        }

        private void ViewerApplication_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Proxy" && View != null)
                View.ProxyUrl = ViewerApplication.Proxy;
        }

        public ViewerApplication DefaultApplicationSettings
        {
            get { return (ViewerApplication)GetValue(DefaultApplicationSettingsProperty); }
            set { SetValue(DefaultApplicationSettingsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultApplicationSettings.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultApplicationSettingsProperty =
            DependencyProperty.Register("DefaultApplicationSettings", typeof(ViewerApplication), typeof(ViewerApplicationControl), null);
        #endregion

        public ViewerApplication BuilderApplication { get; set; }

        public FileConfigurationProvider ConfigurationProvider { get; set; }
        public bool IsEditMode { get; set; }
        private ApplicationColorSet ApplicationColorSet { get; set; }
        public ESRI.ArcGIS.Mapping.Core.IApplicationServices ApplicationServices { get; set; }

 	    #region LayoutElement
        internal FrameworkElement LayoutElement
        {
            get
            {
                return View != null ? View.Content as FrameworkElement : null;
            }
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.Content == null)
                initializeViewerApplication(false);

            // Tooltips in RTL require the main page to be LTR, and ViewerApplicationControl to be RTL
            RTLHelper helper = Application.Current.Resources["RTLHelper"] as RTLHelper;
            if (helper != null)
                FlowDirection = helper.FlowDirection;
        }

        void initializeViewerApplication(bool force)
        {
            if (BaseUri != null)
            {
                // Reset toolPanel and behaviors xml
                toolsXmlFileContents = null;
                behaviorsXmlFileContents = null;                
                controlsXmlFileContents = null;
                layoutResourceDictionaries = null;
                ToolPanels.Current.Clear();

                ApplicationSettingsFilePath = getUrl("/Config/Application.xml");
                downloadApplicationSettingsFile(force);
            }
        }

        private string getUrl(string relativeUrl, bool ignoreRandomizing=false)
        {
            string randomizeStr = string.Empty;
            if(!ignoreRandomizing)
            {
                string queryStr = BaseUri.Query.IndexOf("?", StringComparison.Ordinal) < 0 ? "?_r=" : "&_r=";
                if (ViewerApplication != null && !string.IsNullOrWhiteSpace(ViewerApplication.Version))
                {
                    randomizeStr = queryStr + ViewerApplication.Version;
                }
                else
                    randomizeStr = queryStr + Guid.NewGuid().ToString("N");
            }

            if (string.IsNullOrEmpty(relativeUrl))
                return relativeUrl;            
            UriBuilder resolvedUrl = new UriBuilder(new Uri(BaseUri, relativeUrl.TrimStart('/')));
            resolvedUrl.Query = BaseUri.Query.TrimStart('?') + randomizeStr;
            return resolvedUrl.Uri.AbsoluteUri; 
        }

        void downloadApplicationSettingsFile(bool force)
        {
            if (m_IsDownloadingApplicationSettingsFile)
                return;

            if (!force)
            {
                if (!string.IsNullOrWhiteSpace(applicationSettingsFileContents))
                {
                    initializeDataContext(applicationSettingsFileContents);
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(ApplicationSettingsFilePath))
            {
                WebClient applicationSettingsFileRequestClient = new WebClient();
                applicationSettingsFileRequestClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(ApplicationSettingsFileDownloadComplete);
                m_IsDownloadingApplicationSettingsFile = true;
                applicationSettingsFileRequestClient.DownloadStringAsync(new Uri(BaseUri, ApplicationSettingsFilePath));
            }
        }

        void ApplicationSettingsFileDownloadComplete(object sender, DownloadStringCompletedEventArgs e)
        {
            m_IsDownloadingApplicationSettingsFile = false;

            if (e.Cancelled)
                return;

            if (e.Error != null)
            {
                if (e.Error is System.Security.SecurityException)
                {
                    // Use MessageBoxDialog instead of MessageBox.  There is a bug with
                    // Firefox 3.6 that crashes Silverlight when using MessageBox.Show.
                    MessageBoxDialog.Show("A clientaccesspolicy.xml or crossdomain.xml might be missing at the web root.",
                        "Error accessing application files", MessageBoxButton.OK);
                }
                Logger.Instance.LogError(e.Error);
                return;
            }

            applicationSettingsFileContents = e.Result;

            initializeDataContext(applicationSettingsFileContents);
        }

        void initializeDataContext(string appSettingsFileContents)
        {
            ViewerApplication = new ViewerApplication(appSettingsFileContents);
            if (DefaultApplicationSettings != null)
            {
                ViewerApplication.ArcGISOnlineSecure = DefaultApplicationSettings.ArcGISOnlineSecure;
                ViewerApplication.ArcGISOnlineSharing = DefaultApplicationSettings.ArcGISOnlineSharing;
                ViewerApplication.BingMapsAppId = DefaultApplicationSettings.BingMapsAppId;
                ViewerApplication.PortalAppId = DefaultApplicationSettings.PortalAppId;
                ViewerApplication.GeometryService = DefaultApplicationSettings.GeometryService;
                ViewerApplication.ArcGISOnlineProxy = DefaultApplicationSettings.ArcGISOnlineProxy;
                ViewerApplication.Proxy = DefaultApplicationSettings.Proxy;
            }
            this.DataContext = ViewerApplication;
            ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnlineEnvironment.LoadConfig(ViewerApplication.ArcGISOnlineSharing, ViewerApplication.ArcGISOnlineSecure, ViewerApplication.ArcGISOnlineProxy,
                true, false, false);
            ConfigurationStoreFilePath = getUrl("/Config/Admin/ConfigurationStore.xml");
            ConnectionsFileFilePath = getUrl("/Config/Admin/Connections.xml");
            SymbolConfigurationFilePath = getUrl("/Config/Admin/Symbols.xml");
            SymbolFolderParentUrl = getUrl("/Config/Symbols", true);
            MapConfigurationFilePath = getUrl("/Config/Map.xml");
            ToolsConfigurationFilePath = getUrl("/Config/Tools.xml");
            BehaviorsConfigurationFilePath = getUrl("/Config/Behaviors.xml");
            ControlsConfigurationFilePath = getUrl("/Config/Controls.xml");
            HeatMapColorGradientsConfigFileUrl = getUrl("/Config/ResourceDictionaries/HeatMapBrushes.xaml");
            ClassBreaksColorGradientsConfigFileUrl = getUrl("/Config/ResourceDictionaries/ThematicMapBrushes.xaml");
            ThemeFilePath = getUrl("/Themes/Theme.thmx");
            ApplicationColorsFilePath = getUrl("/Config/Layouts/ResourceDictionaries/Common/Colors.xaml");
            LayoutFilePath = getUrl(ViewerApplication.LayoutFilePath);
            downloadApplicationColorsFile();
        }

        private void downloadApplicationColorsFile()
        {
            WebClient layoutFileRequestClient = new WebClient();
            layoutFileRequestClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(ApplicationColorsFileDownloadComplete);
            layoutFileRequestClient.DownloadStringAsync(new Uri(BaseUri, ApplicationColorsFilePath));
        }

        void ApplicationColorsFileDownloadComplete(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;

            if (e.Error != null)
            {
                Logger.Instance.LogError(e.Error);
                return;
            }

            applicationColorsFileContents = e.Result;

            if (ApplicationColorSet != null)
            {
                // Restore previous step
                ApplicationColorSet resetColors = new Core.ApplicationColorSet();
                resetColors.RestoreDefaultsColorsToApplication();
            }
            ApplicationColorSet = fromResourceDictionary(XamlReader.Load(e.Result) as ResourceDictionary);

            downloadLayoutFile();
        }
        
        void downloadLayoutFile()
        {
            if (!string.IsNullOrWhiteSpace(LayoutFilePath))
            {
                WebClient layoutFileRequestClient = new WebClient();
                layoutFileRequestClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(LayoutFileDownloadComplete);
                layoutFileRequestClient.DownloadStringAsync(new Uri(BaseUri, LayoutFilePath));
            }
        }

        void LayoutFileDownloadComplete(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;

            if (e.Error != null)
            {
                Logger.Instance.LogError(e.Error);
                return;
            }

            layoutFileContents = e.Result;

            downloadToolsFile();
        }

        void downloadToolsFile()
        {
            if (string.IsNullOrWhiteSpace(toolsXmlFileContents))
            {
                if (!string.IsNullOrWhiteSpace(ToolsConfigurationFilePath))
                {
                    WebClient toolsFileRequestWebClient = new WebClient();
                    toolsFileRequestWebClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(toolsFileRequestWebClient_DownloadStringCompleted);
                    toolsFileRequestWebClient.DownloadStringAsync(new Uri(BaseUri, ToolsConfigurationFilePath));
                }
            }
            else
                downloadBehaviorsFile();
        }

        void toolsFileRequestWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
                return;

            toolsXmlFileContents = e.Result;

            downloadBehaviorsFile();
        }

        void downloadBehaviorsFile()
        {
            if (string.IsNullOrWhiteSpace(behaviorsXmlFileContents))
            {
                if (!string.IsNullOrWhiteSpace(BehaviorsConfigurationFilePath))
                {
                    WebClient behaviorsFileRequestWebClient = new WebClient();
                    behaviorsFileRequestWebClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(behaviorsFileRequestWebClient_DownloadStringCompleted);
                    behaviorsFileRequestWebClient.DownloadStringAsync(new Uri(BaseUri, BehaviorsConfigurationFilePath));
                }
            }
            else
                downloadControlsFile();
        }

        void behaviorsFileRequestWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
                return;

            behaviorsXmlFileContents = e.Result;

            downloadControlsFile();
        }

        void downloadControlsFile()
        {
            if (string.IsNullOrWhiteSpace(controlsXmlFileContents))
            {
                if (!string.IsNullOrWhiteSpace(ControlsConfigurationFilePath))
                {
                    WebClient controlsFileRequestWebClient = new WebClient();
                    controlsFileRequestWebClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(controlsFileRequestWebClient_DownloadStringCompleted);
                    controlsFileRequestWebClient.DownloadStringAsync(new Uri(BaseUri, ControlsConfigurationFilePath));
                }
            }
            else
                downloadLayoutResources();
        }

        void controlsFileRequestWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
                return;

            controlsXmlFileContents = e.Result;            

            downloadLayoutResources();
        }

        void downloadLayoutResources()
        {
            List<string> resourceFileNames = null;
            layoutFileContents = XamlProcessor.ExtractResourceDictionaries(layoutFileContents, out resourceFileNames);
            List<string> resourceFilePaths = new List<string>();
            foreach (string file in resourceFileNames)
                resourceFilePaths.Add(getUrl(string.Format("/Config/Layouts/{0}", file)));
            DownloadManager.DownloadUrls(resourceFilePaths, OnLayoutResourcesDownloadComplete, OnLayoutResourcesDownloadError, resourceFilePaths);
        }

        private List<string> layoutResourceDictionaryFileContents;
        private void OnLayoutResourcesDownloadComplete(object sender, DownloadManager.DownloadCompleteEventArgs e)
        {            
            List<string> resourceFilePaths = e.UserState as List<string>;           
            layoutResourceDictionaryFileContents = new List<string>();
            if (resourceFilePaths != null)
            {
                // Order the resource dictionaries in the order in which they were declared in the layout file
                foreach (string resourceFilePath in resourceFilePaths)
                {
                    string xaml = null;
                    if (e.FileContents.TryGetValue(resourceFilePath, out xaml))
                        layoutResourceDictionaryFileContents.Add(xaml);        
                }
            }
            loadLayout();
        }

        private void OnLayoutResourcesDownloadError(object sender, ExceptionEventArgs e)
        {
            Logger.Instance.LogError(StringResourcesManager.Instance.Get("LayoutResourceDownloadError"));
            return;
        }

        void loadLayout()
        {
            ExtensionsManager.LoadAllExtensions(ViewerApplication.Extensions, onExtensionsLoadComplete, onExtensionsLoadFail);
        }

        private void onExtensionsLoadComplete(object sender, EventArgs args)
        {
            initializeView();
        }

        private void onExtensionsLoadFail(object sender, EventArgs args)
        {
            ExceptionEventArgs ex = args as ExceptionEventArgs;
            if (ex != null && ex.Exception != null)
                Logger.Instance.LogError(ex.Exception);
        }

        void initializeView()
        {
            if (!ArcGISOnlineEnvironment.ArcGISOnline.IsInitialized) // Wait for AGOL environment initialization
            {
                EventHandler<EventArgs> onInitialized = null;
                onInitialized = (o, e) =>
                {
                    ArcGISOnlineEnvironment.ArcGISOnline.Initialized -= onInitialized;
                    initializeView();
                };
                ArcGISOnlineEnvironment.ArcGISOnline.Initialized += onInitialized;
            }
            else
            {

                WindowManager manager = View != null && View.WindowManager != null ? View.WindowManager : null;

                string portalUrl = BaseUri.Scheme == "http" ? ViewerApplication.ArcGISOnlineSharing :
                    ViewerApplication.ArcGISOnlineSecure;

                // TODO: Investigate difference between ViewerApplication.ArcGISOnlineProxy and ViewerApplication.Proxy
                ArcGISPortal portal = new ArcGISPortal()
                {
                    Url = portalUrl,
                    Token = ArcGISOnlineEnvironment.ArcGISOnline.User != null
                        ? ArcGISOnlineEnvironment.ArcGISOnline.User.Token : null,
                    ProxyUrl = ViewerApplication.ArcGISOnlineProxy
                };

                View = new View(ApplicationServices, manager)
                {
                    IsEditMode = IsEditMode,
                    BaseUrl = BaseUri.ToString(),
                    ApplicationColorSet = ApplicationColorSet,
                    LayoutProvider = new LayoutFileProvider()
                    {
                        LayoutFileContents = layoutFileContents,
                    },
                    Portal = portal
                };
                if (!string.IsNullOrWhiteSpace(ConfigurationStoreFilePath))
                {
                    View.ConfigurationStoreProvider = new ViewerConfigurationStoreProvider(ViewerApplication.GeometryService, ViewerApplication.BingMapsAppId, ViewerApplication.PortalAppId);
                }
                View.ProxyUrl = View.DefaultProxyUrl = ViewerApplication.Proxy;
                layoutResourceDictionaries = new List<ResourceDictionary>();
                foreach (string layoutResourceDictionaryFileContent in layoutResourceDictionaryFileContents)
                {
                    if (!string.IsNullOrWhiteSpace(layoutResourceDictionaryFileContent))
                    {
                        ResourceDictionary resourceDict = XamlReader.Load(layoutResourceDictionaryFileContent) as ResourceDictionary;
                        if (resourceDict != null)
                        {
                            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                            layoutResourceDictionaries.Add(resourceDict);
                        }
                    }
                }
                View.LayoutResourceDictionaries = layoutResourceDictionaries;

                if (!string.IsNullOrWhiteSpace(ThemeFilePath))
                {
                    View.ThemeProvider = new ThemeProvider()
                    {
                        ThemeFileUrl = ThemeFilePath,
                    };
                }

                if (!string.IsNullOrWhiteSpace(ConnectionsFileFilePath))
                {
                    View.ConnectionsProvider = new FileConnectionsProvider()
                    {
                        ConfigurationFile = new DataFile() { IsUrl = true, Path = ConnectionsFileFilePath }
                    };
                }

                if (!string.IsNullOrWhiteSpace(MapConfigurationFilePath))
                {
                    FileConfigurationProvider fileConfigurationProvider = ConfigurationProvider ?? new FileConfigurationProvider();
                    fileConfigurationProvider.ConfigurationFile = new DataFile() { IsUrl = true, Path = MapConfigurationFilePath };
                    View.ConfigurationProvider = fileConfigurationProvider;
                }

                if (!string.IsNullOrWhiteSpace(SymbolConfigurationFilePath))
                {
                    View.SymbolConfigProvider = new FileSymbolConfigProvider()
                    {
                        ConfigFileUrl = SymbolConfigurationFilePath,
                        ClassBreaksColorGradientsConfigFileUrl = ClassBreaksColorGradientsConfigFileUrl,
                        HeatMapColorGradientsConfigFileUrl = HeatMapColorGradientsConfigFileUrl,
                        SymbolFolderParentUrl = SymbolFolderParentUrl,
                        UniqueValueColorGradientsConfigFileUrl = UniqueValueColorGradientsConfigFileUrl,
                    };
                }

                View.ExtensionsDataManager = new ExtensionsDataManager() { ExtensionsConfigData = new ExtensionsConfigData(controlsXmlFileContents) };
                View.ExtensionBehaviors = getBehaviors(behaviorsXmlFileContents);

                View.ExtensionLoadFailed += new EventHandler<ExceptionEventArgs>(View_ExtensionLoadFailed);

                View.ConfigurationLoaded += new EventHandler(View_ConfigurationLoaded);

                this.Content = View;

                OnViewInitialized(new ViewEventArgs() { View = View });
            }
        }

        private const string ACCENT_COLOR = "AccentColor";
        private const string ACCENT_TEXT_COLOR = "AccentTextColor";
        private const string BACKGROUND_END_GRADIENT_STOP_COLOR = "BackgroundEndGradientStopColor";
        private const string BACKGROUND_START_GRADIENT_STOP_COLOR = "BackgroundStartGradientStopColor";
        private const string BACKGROUND_TEXT_COLOR = "BackgroundTextColor";
        private const string SELECTION_COLOR = "SelectionColor";
        private const string SELECTION_OUTLINE_COLOR = "SelectionOutlineColor";

        private static ApplicationColorSet fromResourceDictionary(ResourceDictionary dict)
        {
            if (dict == null)
                return null;
            ApplicationColorSet colors = new ApplicationColorSet();
            colors.AccentColor = (Color)dict[ACCENT_COLOR];
            colors.AccentTextColor = (Color)dict[ACCENT_TEXT_COLOR];
            colors.BackgroundEndGradientColor = (Color)dict[BACKGROUND_END_GRADIENT_STOP_COLOR];
            colors.BackgroundStartGradientColor = (Color)dict[BACKGROUND_START_GRADIENT_STOP_COLOR];
            colors.BackgroundTextColor = (Color)dict[BACKGROUND_TEXT_COLOR];
            colors.SelectionColor = (Color)dict[SELECTION_COLOR];
            colors.SelectionOutlineColor = (Color)dict[SELECTION_OUTLINE_COLOR];
            return colors;
        }

        public string GetColorXaml()
        {
            if (string.IsNullOrEmpty(applicationColorsFileContents))
                return null;

            XDocument xDoc = XDocument.Parse(applicationColorsFileContents);
            ApplicationColorSet applicationColorSet = ApplicationColorSet;
            if (applicationColorSet != null)
            {
                var firstNode = xDoc.Nodes().FirstOrDefault(n => n is XElement);
                foreach (XElement resource in (firstNode as XElement).Elements())
                {
                    string resourceKey = null;
                    foreach (XAttribute attrib in resource.Attributes())
                    {
                        if (attrib.Name != null && attrib.Name.LocalName == "Key")
                        {
                            resourceKey = attrib.Value;
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(resourceKey))
                        continue;
                    switch (resourceKey)
                    {
                        case ACCENT_COLOR:
                            resource.SetValue(applicationColorSet.AccentColor.ToString());
                            break;
                        case ACCENT_TEXT_COLOR:
                            resource.SetValue(applicationColorSet.AccentTextColor.ToString());
                            break;
                        case BACKGROUND_END_GRADIENT_STOP_COLOR:
                            resource.SetValue(applicationColorSet.BackgroundEndGradientColor.ToString());
                            break;
                        case BACKGROUND_START_GRADIENT_STOP_COLOR:
                            resource.SetValue(applicationColorSet.BackgroundStartGradientColor.ToString());
                            break;
                        case BACKGROUND_TEXT_COLOR:
                            resource.SetValue(applicationColorSet.BackgroundTextColor.ToString());
                            break;
                        case SELECTION_COLOR:
                            resource.SetValue(applicationColorSet.SelectionColor.ToString());
                            break;
                        case SELECTION_OUTLINE_COLOR:
                            resource.SetValue(applicationColorSet.SelectionOutlineColor.ToString());
                            break;
                    }
                }
            }

            return xDoc.ToString(SaveOptions.OmitDuplicateNamespaces);
        }

        public XDocument GetBehaviorsXml()
        {
            if (View.Instance != null)
            {
                BehaviorsConfiguration configuration = BehaviorsConfiguration.GetFromExtensionBehaviors(View.Instance.ExtensionBehaviors, View.Instance.ExtensionsDataManager, BehaviorsConfiguration);
                if (configuration != null)
                {
                    return configuration.GetBehaviorsXml();
                }
            }
            return null;
        }

        public IEnumerable<string> GetAllAssembliesNamesInUse(out string behaviorsXml)
        {            
            List<string> assemblies = getUniqueAssemblyNamesForTypes(ToolPanels.GetToolItemObjects());
            IEnumerable<string> namespaces = getAllNamespacesInUse(out behaviorsXml);
            if (namespaces != null)
            {
                foreach (string namespaceMapping in namespaces)
                {
                    string assembly = AssemblyManager.GetAssemblyNameForNamespaceDeclaration(namespaceMapping);
                    if (!string.IsNullOrWhiteSpace(assembly))
                    {
                        if (!assemblies.Contains(assembly) && !AssemblyManager.IsBuiltInAssembly(assembly))
                            assemblies.Add(assembly);
                    }
                }
            }
            return assemblies;
        }

        private List<string> getUniqueAssemblyNamesForTypes(IEnumerable<object> toolItems)
        {
            if (toolItems == null)
                return null;
            List<string> assemNames = new List<string>();
            foreach (object obj in toolItems)
            {
                string assemblyName = obj.GetType().Assembly.FullName.Split(',')[0];
                if (!assemNames.Contains(assemblyName) && !AssemblyManager.IsBuiltInAssembly(assemblyName))
                    assemNames.Add(assemblyName);
            }
            return assemNames;
        }

        private IEnumerable<string> getAllNamespacesInUse(out string behaviorsXml)
        {
            List<string> namespaces = new List<string>();
            var behaviorsXmlDoc = GetBehaviorsXml();
            behaviorsXml = behaviorsXmlDoc.ToString(SaveOptions.OmitDuplicateNamespaces);

            addNamespaceDeclarationsFromXmlElement(namespaces, behaviorsXml, behaviorsXmlDoc.Root.Name.LocalName);
            addNamespaceDeclaratonsFromChildNodes(namespaces, behaviorsXmlDoc.Root);
            
            addNamespaceDeclarationsFromXmlElement(namespaces, layoutFileContents, "UserControl");
            return namespaces;
        }

        private void addNamespaceDeclaratonsFromChildNodes(IList<string> namespaces, XElement node)
        {
            var nodes = node.Elements();
            foreach (var child in nodes)
            {
                addNamespaceDeclarationsFromXmlElement(namespaces, child.ToString(), child.Name.LocalName);
                if (child.HasElements)
                    addNamespaceDeclaratonsFromChildNodes(namespaces, child);
            }
        }

        private void addNamespaceDeclarationsFromXmlElement(IList<string> namespaceDeclaration, string element, string rootElementName)
        {
            if (string.IsNullOrWhiteSpace(element) || namespaceDeclaration == null)
                return;

            // Given input of
            //  <RootElementName 
            //      xmlns:a="Something"
            //      xmlns:b="Something Else"
            //      xmlns:c="Something More">
            //   ...
            //  </RootElementName>

            // We are interested in getting out the strings
            // Something
            // Something Else
            // Something Mode

            int start = element.IndexOf("<" + rootElementName, StringComparison.Ordinal);
            if (start < 0)
                return;

            // Move past <RootElementName
            start += rootElementName.Length + 1;

            // Find end of start tag 
            int end = element.IndexOf(">", start, StringComparison.Ordinal);
            if (end < 0)
                return;

            // Get the attributes section of the RootElement
            string rootTag = element.Substring(start, end - start).Trim();
            string[] attributes = rootTag.Split(new string[] { " ", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (attributes != null)
            {
                foreach (string attribute in attributes)
                {
                    string attrib = attribute.Trim(); // remove all whitespaces
                    string[] split = attrib.Split(new string[] { "=\"" }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length < 2)
                        continue;

                    string xmlnsDecl = split[0];
                    if (!xmlnsDecl.StartsWith("xmlns:", StringComparison.OrdinalIgnoreCase)
                    && !(xmlnsDecl.ToLower() == "xmlns")) // we are only interested in namespace declarations
                        continue;

                    string namespaceMapping = split[1].TrimEnd('"');
                    if (!namespaceDeclaration.Contains(namespaceMapping))
                        namespaceDeclaration.Add(namespaceMapping);
                }
            }
        }

        void View_ExtensionLoadFailed(object sender, ExceptionEventArgs e)
        {
            OnExtensionLoadFailed(e);
        }

        protected virtual void OnExtensionLoadFailed(ExceptionEventArgs args)
        {
            if (ExtensionLoadFailed != null)
                ExtensionLoadFailed(this, args);
        }
        public event EventHandler<ExceptionEventArgs> ExtensionLoadFailed;

        void View_ConfigurationLoaded(object sender, EventArgs e)
        {
            OnViewLoaded(EventArgs.Empty);
            if (!string.IsNullOrWhiteSpace(toolsXmlFileContents))
            {
                this.View.loadToolPanels(toolsXmlFileContents);
                this.ToolPanels = ToolPanels.Current;
                OnToolPanelLoaded(EventArgs.Empty);
            }
            ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnlineEnvironment.ConfigurationUrls.GeometryServer = View.GeometryServiceUrl;
        }

       
        private ObservableCollection<ExtensionBehavior> getBehaviors(string behaviorsXml)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(behaviorsXml))
                {
                    BehaviorsConfiguration.PopulateBehaviorsFromXml(behaviorsXml);

                    List<ExtensionBehavior> extensionBehaviors = BehaviorsConfiguration.GetAsExtensionBehaviors(View.ExtensionsDataManager);
                    if (extensionBehaviors != null)
                        return new ObservableCollection<ExtensionBehavior>(extensionBehaviors);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError(ex);
            }
            return null;
        }

        private void DisposeView()
        {
            if(View != null)
                View.Dispose();
            OnViewDisposed(new ViewEventArgs() { View = View });
        }

        public void ApplyNewLayout(string newLayoutRelativePath)
        {
            DisposeView();

            layoutResourceDictionaries = null;
            LayoutFilePath = getUrl(newLayoutRelativePath);
            View.Instance.SaveExtensionsConfigData();
            if (ToolPanels != null)
            {
                // save the XML for the toolPanels
                toolsXmlFileContents = ToolPanels.GetToolPanelsXml();
            }
            ToolPanels.Current.Clear();
            behaviorsXmlFileContents = GetBehaviorsXml().ToString(SaveOptions.OmitDuplicateNamespaces);
            controlsXmlFileContents = View.GetConfigurationOfControls();

            disposeExtensibilityObjects();
            if (ViewerApplication != null)
                ViewerApplication.LayoutFilePath = newLayoutRelativePath;            
            downloadLayoutFile();

            // Reset the Identify Popup control and content
            PopupHelper.Reset();
        }

        

        private void disposeExtensibilityObjects()
        {
            if (ToolPanels != null)
            {   
                // Dipose attached event handlers
                ToolPanels.Dispose();
            }

            if (BehaviorsConfiguration != null)
                BehaviorsConfiguration.Dispose();
        }

        #region Events
        protected virtual void OnViewLoaded(EventArgs args)
        {
            if (ViewLoaded != null)
                ViewLoaded(this, args);
        }
        protected virtual void OnViewInitialized(ViewEventArgs e)
        {
            if (ViewInitialized != null)
                ViewInitialized(this, e);
        }
        protected virtual void OnViewDisposed(ViewEventArgs e)
        {
            if (ViewDisposed != null)
                ViewDisposed(this, e);
        }

        protected virtual void OnToolPanelLoaded(EventArgs args)
        {
            if (ToolPanelLoaded != null)
                ToolPanelLoaded(this, args);
        }       

        public event EventHandler ViewLoaded;       
        public event EventHandler<ViewEventArgs> ViewInitialized;
        public event EventHandler<ViewEventArgs> ViewDisposed;       
        public event EventHandler ToolPanelLoaded;

        public class ViewEventArgs : EventArgs
        {
            public View View { get; set; }
        }
        #endregion
    }

    class ViewerConfigurationStoreProvider : ConfigurationStoreProvider
    {
        private string _geometryServiceUrl;
        private string _bingMapsAppId;
        private string _portalAppId;
        public ViewerConfigurationStoreProvider(string geometryServiceUrl, string bingMapsAppId, string portalAppId)
        {
            _geometryServiceUrl = geometryServiceUrl;
            _bingMapsAppId = bingMapsAppId;
            _portalAppId = portalAppId;
        }

        public override void GetConfigurationStoreAsync(object userState)
        {
            ConfigurationStore store = new ConfigurationStore() { 
                GeometryServices = new List<GeometryServiceInfo>(),
                BingMapsAppId = _bingMapsAppId,
                PortalAppId = _portalAppId
            };
            if (!string.IsNullOrWhiteSpace(_geometryServiceUrl))
                store.GeometryServices.Add(new GeometryServiceInfo() { Url = _geometryServiceUrl });
            OnGetConfigurationStoreCompleted(new GetConfigurationStoreCompletedEventArgs() {  ConfigurationStore = store, UserState = userState });
        }
    }   

    public class RuntimeUrlResolver : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string urlToBeResolved = value as string;
            if (!string.IsNullOrWhiteSpace(urlToBeResolved))
            {
                return ImageUrlResolver.ResolveUrlForImage(urlToBeResolved);
            }
            else
            {
                urlToBeResolved = parameter as string;
                if (!string.IsNullOrWhiteSpace(urlToBeResolved))
                {
                    return ImageUrlResolver.ResolveUrlForImage(urlToBeResolved);
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
