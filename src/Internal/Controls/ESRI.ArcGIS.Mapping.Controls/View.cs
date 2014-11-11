/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Mapping.Controls.Utils;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using ESRI.ArcGIS.Mapping.DataSources;
using controls = ESRI.ArcGIS.Mapping.Controls;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Mapping.Behaviors;
using System.Text;
using System.Xml;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using ESRI.ArcGIS.Mapping.Core.Symbols;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;
using System.ComponentModel;
using ESRI.ArcGIS.Client.Application.Controls;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;
using ESRI.ArcGIS.Mapping.Controls.MapContents;
using System.Globalization;
using LayerExtensions = ESRI.ArcGIS.Client.Extensibility.LayerExtensions;
using ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;
using System.Windows.Data;
using System.Xml.Linq;
using ESRI.ArcGIS.Client.WebMap;
using System.Windows.Markup;
using ESRI.ArcGIS.Mapping.Controls.Resources;
using ESRI.ArcGIS.Client.Portal;
using ESRI.ArcGIS.Client.FeatureService;
using System.Windows.Browser;
using ESRI.ArcGIS.Mapping.Windowing;
using ESRI.ArcGIS.Mapping.DataSources.ArcGISServer;
using EsriMapContents = ESRI.ArcGIS.Mapping.Controls.MapContents;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows.Threading;
using ESRI.ArcGIS.Mapping.Controls.Editor;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public partial class View : ContentControl, IMapApplication
    {
        private bool m_layersInitialized;
        private bool m_initializationTimedOut;
        private int m_behaviorsPendingInitCount = 0;
        private int m_toolsPendingInitCount = 0;
        private DispatcherTimer m_initializeTimeoutTimer;

        static View instance;
        public static View Instance
        {
            get { return instance; }
        }

        private static void setInstance(View view)
        {
            // Stop previous instance from listening to web map settings
            if (instance != null)
                ViewerApplication.WebMapSettings.PropertyChanged -= instance.WebMapSettings_PropertyChanged;
            
            instance = view; 
            if (ViewChanged != null) ViewChanged(null, EventArgs.Empty); 
        }

        public static event EventHandler ViewChanged;
        #region Map
        /// <summary>
        /// 
        /// </summary>
        public Map Map
        {
            get { return GetValue(MapProperty) as Map; }
            set { SetValue(MapProperty, value); }
        }

        /// <summary>
        /// Identifies the Map dependency property.
        /// </summary>
        public static readonly DependencyProperty MapProperty =
                DependencyProperty.Register(
                        "Map",
                        typeof(Map),
                        typeof(View),
                        new PropertyMetadata(OnMapPropertyChanged));

        public static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            View view = d as View;
            ViewerApplication.WebMapSettings.PropertyChanged -= view.WebMapSettings_PropertyChanged;
            ViewerApplication.WebMapSettings.PropertyChanged += view.WebMapSettings_PropertyChanged;

            Map map = e.NewValue as Map;
            if (map != null)
                map.SetWebMapSettings(ViewerApplication.WebMapSettings);
        }
        #endregion

        #region SelectedLayer
        /// <summary>
        /// The currently seleceted layer in the MapContents
        /// </summary>
        public Layer SelectedLayer
        {
            get { return GetValue(SelectedLayerProperty) as Layer; }
            set { SetValue(SelectedLayerProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectedLayer dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedLayerProperty =
                DependencyProperty.Register(
                        "SelectedLayer",
                        typeof(Layer),
                        typeof(View),
                        new PropertyMetadata(null, OnSelectedLayerPropertyChanged));

        /// <summary>
        /// SelectedLayerProperty property changed handler.
        /// </summary>
        /// <param name="d">View that changed its SelectedLayer.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnSelectedLayerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            View source = d as View;
            if(source != null)  
                source.OnSelectedLayerChanged(e.NewValue as Layer);
        }

        private void OnSelectedLayerChanged(Layer newLayer)
        {
            if (AttributeDisplay != null && AttributeDisplay.GraphicsLayer != newLayer)
                AttributeDisplay.GraphicsLayer = newLayer as GraphicsLayer;
            OnLayerSelectionChanged(new LayerEventArgs() { Layer= newLayer });
        }
        #endregion

        #region ConfigurationStoreProvider
        /// <summary>
        /// 
        /// </summary>
        public ConfigurationStoreProvider ConfigurationStoreProvider
        {
            get { return GetValue(ConfigurationStoreProviderProperty) as ConfigurationStoreProvider; }
            set { SetValue(ConfigurationStoreProviderProperty, value); }
        }

        /// <summary>
        /// Identifies the ConfigurationStoreProvider dependency property.
        /// </summary>
        public static readonly DependencyProperty ConfigurationStoreProviderProperty =
                DependencyProperty.Register(
                        "ConfigurationStoreProvider",
                        typeof(ConfigurationStoreProvider),
                        typeof(View),
                        new PropertyMetadata(new ConfigurationStoreProvider()));
        #endregion

        #region ConfigurationProvider
        /// <summary>
        /// 
        /// </summary>
        public ConfigurationProvider ConfigurationProvider
        {
            get { return GetValue(ConfigurationProviderProperty) as ConfigurationProvider; }
            set { SetValue(ConfigurationProviderProperty, value); }
        }

        /// <summary>
        /// Identifies the ConfigurationProvider dependency property.
        /// </summary>
        public static readonly DependencyProperty ConfigurationProviderProperty =
                DependencyProperty.Register(
                        "ConfigurationProvider",
                        typeof(ConfigurationProvider),
                        typeof(View),
                        new PropertyMetadata(null));
        #endregion

        #region DataSourceProvider
        /// <summary>
        /// 
        /// </summary>
        public DataSourceProvider DataSourceProvider
        {
            get { return GetValue(DataSourceProviderProperty) as DataSourceProvider; }
            set { SetValue(DataSourceProviderProperty, value); }
        }

        /// <summary>
        /// Identifies the DataSourceProvider dependency property.
        /// </summary>
        public static readonly DependencyProperty DataSourceProviderProperty =
                DependencyProperty.Register(
                        "DataSourceProvider",
                        typeof(DataSourceProvider),
                        typeof(View),
                        new PropertyMetadata(new DataSourceProvider()));
        #endregion

        #region ConnectionsProvider
        /// <summary>
        /// 
        /// </summary>
        public ConnectionsProvider ConnectionsProvider
        {
            get { return GetValue(ConnectionsProviderProperty) as ConnectionsProvider; }
            set { SetValue(ConnectionsProviderProperty, value); }
        }

        /// <summary>
        /// Identifies the ConnectionsProvider dependency property.
        /// </summary>
        public static readonly DependencyProperty ConnectionsProviderProperty =
                DependencyProperty.Register(
                        "ConnectionsProvider",
                        typeof(ConnectionsProvider),
                        typeof(View),
                        new PropertyMetadata(new ConnectionsProvider()));
        #endregion

        #region ConfigurationStore
        /// <summary>
        /// 
        /// </summary>
        public ConfigurationStore ConfigurationStore
        {
            get { return GetValue(ConfigurationStoreProperty) as ConfigurationStore; }
            set { SetValue(ConfigurationStoreProperty, value); }
        }

        /// <summary>
        /// Identifies the ConfigurationStore dependency property.
        /// </summary>
        public static readonly DependencyProperty ConfigurationStoreProperty =
                DependencyProperty.Register(
                        "ConfigurationStore",
                        typeof(ConfigurationStore),
                        typeof(View),
                        new PropertyMetadata(OnConfigStoreChange));
        static void OnConfigStoreChange(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            View view = d as View;
            if (view != null && view.ConfigurationStoreChanged != null)
                view.ConfigurationStoreChanged(view, EventArgs.Empty);
        }
        public event EventHandler ConfigurationStoreChanged;
        #endregion

        #region SymbolConfigProvider
        /// <summary>
        /// 
        /// </summary>
        public SymbolConfigProvider SymbolConfigProvider
        {
            get { return GetValue(SymbolConfigProviderProperty) as SymbolConfigProvider; }
            set { SetValue(SymbolConfigProviderProperty, value); }
        }

        /// <summary>
        /// Identifies the SymbolConfigProvider dependency property.
        /// </summary>
        public static readonly DependencyProperty SymbolConfigProviderProperty =
                DependencyProperty.Register(
                        "SymbolConfigProvider",
                        typeof(SymbolConfigProvider),
                        typeof(View),
                        new PropertyMetadata(new SymbolConfigProvider()));
        #endregion

        #region ThemeProvider
        /// <summary>
        /// 
        /// </summary>
        public ThemeProvider ThemeProvider
        {
            get { return GetValue(ThemeProviderProperty) as ThemeProvider; }
            set { SetValue(ThemeProviderProperty, value); }
        }

        /// <summary>
        /// Identifies the ThemeProvider dependency property.
        /// </summary>
        public static readonly DependencyProperty ThemeProviderProperty =
            DependencyProperty.Register(
                "ThemeProvider",
                typeof(ThemeProvider),
                typeof(View),
                new PropertyMetadata(null));
        #endregion

        #region ThemeColorSet
        /// <summary>
        /// 
        /// </summary>
        public ThemeColorSet ThemeColorSet
        {
            get { return GetValue(ThemeColorSetProperty) as ThemeColorSet; }
            set { SetValue(ThemeColorSetProperty, value); }
        }

        /// <summary>
        /// Identifies the ThemeColorSet dependency property.
        /// </summary>
        public static readonly DependencyProperty ThemeColorSetProperty =
            DependencyProperty.Register(
                "ThemeColorSet",
                typeof(ThemeColorSet),
                typeof(View),
                new PropertyMetadata(null, OnThemeColorSetPropertyChanged));

        /// <summary>
        /// ThemeColorSetProperty property changed handler.
        /// </summary>
        /// <param name="d">View that changed its ThemeColorSet.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnThemeColorSetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            View source = d as View;
            source.OnThemeColorSetChanged();
        }

        private void OnThemeColorSetChanged()
        {
            ThemeColorSet colorSet = ThemeColorSet;
            if (colorSet == null)
                return;

            Collection<System.Windows.Media.Color> colors = new Collection<System.Windows.Media.Color>();
            colors.Add(colorSet.TextBackgroundDark1);
            colors.Add(colorSet.TextBackgroundLight1);
            colors.Add(colorSet.TextBackgroundDark2);
            colors.Add(colorSet.TextBackgroundLight2);
            colors.Add(colorSet.Accent1);
            colors.Add(colorSet.Accent2);
            colors.Add(colorSet.Accent3);
            colors.Add(colorSet.Accent4);
            colors.Add(colorSet.Accent5);
            colors.Add(colorSet.Accent6);
            colors.Add(colorSet.Hyperlink);
            colors.Add(colorSet.FollowedHyperlink);

            ThemeColors = colors;
        }
        #endregion

        #region ThemeColors
        /// <summary>
        /// 
        /// </summary>
        public Collection<System.Windows.Media.Color> ThemeColors
        {
            get { return GetValue(ThemeColorsProperty) as Collection<System.Windows.Media.Color>; }
            set { SetValue(ThemeColorsProperty, value); }
        }

        /// <summary>
        /// Identifies the ThemeColors dependency property.
        /// </summary>
        public static readonly DependencyProperty ThemeColorsProperty =
            DependencyProperty.Register(
                "ThemeColors",
                typeof(Collection<System.Windows.Media.Color>),
                typeof(View),
                new PropertyMetadata(null));
        #endregion

        #region ApplicationColorSet
        /// <summary>
        /// 
        /// </summary>
        public ApplicationColorSet ApplicationColorSet
        {
            get { return GetValue(ApplicationColorSetProperty) as ApplicationColorSet; }
            set { SetValue(ApplicationColorSetProperty, value); }
        }

        /// <summary>
        /// Identifies the ApplicationColorSet dependency property.
        /// </summary>
        public static readonly DependencyProperty ApplicationColorSetProperty =
            DependencyProperty.Register(
                "ApplicationColorSet",
                typeof(ApplicationColorSet),
                typeof(View),
                new PropertyMetadata(new ApplicationColorSet()));
        #endregion

        public AttributeDisplay AttributeDisplay { get; set; }
        private GenericConfigControl EditorConfigControl { get; set; }
        private ScaleBar ScaleBar { get; set; }
        public EsriMapContents.MapContents MapContents { get; set; }
        private ProgressGauge ProgressGauge { get; set; }
        private AttributionDisplayControl AttributionDisplayControl { get; set; }
        public CultureInfo Culture { get; set; }

        public ObservableCollection<ICommand> ExtensionCommands { get; set; }

        public ObservableCollection<Behavior<Map>> ExtensionMapBehaviors { get; set; }

        public ObservableCollection<ExtensionBehavior> ExtensionBehaviors { get; set; }


        public EditorWidget Editor
        {
            get { return _editor; }
            set
            {
                if (_editor != value)
                {
                    _editor = value;
                    if (_editor != null)
                        EditorCommandUtility.SetEditorWidget(_editor);
                }
            }
        }
        private EditorWidget _editor = null;

        
        #region AutoScrollRowOnMouseOver
        /// <summary>
        /// 
        /// </summary>
        public bool AutoScrollRowOnMouseOver
        {
            get { return (bool)GetValue(AutoScrollRowOnMouseOverProperty); }
            set { SetValue(AutoScrollRowOnMouseOverProperty, value); }
        }

        /// <summary>
        /// Identifies the AutoScrollRowOnMouseOver dependency property.
        /// </summary>
        public static readonly DependencyProperty AutoScrollRowOnMouseOverProperty =
            DependencyProperty.Register(
                "AutoScrollRowOnMouseOver",
                typeof(bool),
                typeof(View),
                new PropertyMetadata(true, OnAutoScrollRowOnMouseOverPropertyChanged));

        /// <summary>
        /// AutoScrollRowOnMouseOverProperty property changed handler.
        /// </summary>
        /// <param name="d">View that changed its AutoScrollRowOnMouseOver.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnAutoScrollRowOnMouseOverPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            View source = d as View;
            source.OnAutoScrollRowOnMouseOverChanged();
        }
        #endregion

        #region FilterFeaturesOnMapExtentChanged
        /// <summary>
        /// 
        /// </summary>
        public bool FilterFeaturesOnMapExtentChanged
        {
            get { return (bool)GetValue(FilterFeaturesOnMapExtentChangedProperty); }
            set { SetValue(FilterFeaturesOnMapExtentChangedProperty, value); }
        }

        /// <summary>
        /// Identifies the FilterFeaturesOnMapExtentChanged dependency property.
        /// </summary>
        public static readonly DependencyProperty FilterFeaturesOnMapExtentChangedProperty =
            DependencyProperty.Register(
                "FilterFeaturesOnMapExtentChanged",
                typeof(bool),
                typeof(View),
                new PropertyMetadata(false, OnFilterFeaturesOnMapExtentChangedPropertyChanged));

        /// <summary>
        /// FilterFeaturesOnMapExtentChangedProperty property changed handler.
        /// </summary>
        /// <param name="d">View that changed its FilterFeaturesOnMapExtentChanged.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnFilterFeaturesOnMapExtentChangedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            View source = d as View;
            source.OnFilterFeaturesOnMapExtentChanged();
        }
        #endregion

        public ExtensionsDataManager ExtensionsDataManager { get; set; }

        public WindowManager WindowManager { get; set; }

        public LayoutProvider LayoutProvider { get; set; }

        public void SaveExtensionsConfigData()
        {
            if (ExtensionsDataManager == null)
                return;

            if (ExtensionBehaviors != null)
            {
                foreach (ExtensionBehavior extensionBehavior in ExtensionBehaviors)
                {
                    if (extensionBehavior == null)
                        continue;
                    ISupportsConfiguration behavior = extensionBehavior.MapBehavior as ISupportsConfiguration;
                    if (behavior != null)
                    {
                        try
                        {
                            string savedData = behavior.SaveConfiguration();
                            ExtensionsDataManager.SetExtensionDataForExtension(extensionBehavior.BehaviorId, savedData);
                        }
                        catch (Exception ex)
                        {
                            NotificationPanel.Instance.AddNotification(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.BehaviorSaveFailed, ESRI.ArcGIS.Mapping.Controls.Resources.Strings.BehaviorSaveFailedMessage, string.Format(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ErrorSaveConfigurationOfExtension, behavior.GetType().FullName) + Environment.NewLine + ex.Message, MessageType.Warning);
                            Logger.Instance.LogError(ex);
                        }
                    }
                }
            }

            IApplicationAdmin appAdmin = MapApplication.Current as IApplicationAdmin;
            if (appAdmin != null && appAdmin.ConfigurableControls != null)
            {
                ExtensionsDataManager = ExtensionsDataManager ?? new ExtensionsDataManager();
                ExtensionsDataManager.ExtensionsConfigData = ExtensionsDataManager.ExtensionsConfigData ?? new ExtensionsConfigData();
                ExtensionsDataManager.ExtensionsConfigData.ExtensionsData = ExtensionsDataManager.ExtensionsConfigData.ExtensionsData ?? new Dictionary<string, ExtensionData>();

                foreach (FrameworkElement elem in appAdmin.ConfigurableControls)
                {
                    if (string.IsNullOrWhiteSpace(elem.Name))
                        continue;

                    ExtensionData extensionData = null;
                    if (!ExtensionsDataManager.ExtensionsConfigData.ExtensionsData.TryGetValue(elem.Name, out extensionData))
                    {
                        extensionData = new ExtensionData();
                        ExtensionsDataManager.ExtensionsConfigData.ExtensionsData.Add(elem.Name, extensionData);
                    }

                    ISupportsConfiguration supportsConfig = elem as ISupportsConfiguration;
                    if (supportsConfig != null)
                    {
                        try
                        {
                            extensionData.ConfigData = supportsConfig.SaveConfiguration();
                        }
                        catch (Exception ex)
                        {
                            MessageBoxDialog.Show(string.Format(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ErrorSaveConfigurationOfExtension, elem.GetType().FullName) + Environment.NewLine + ex.Message);
                            Logger.Instance.LogError(ex);
                        }
                    }
                }
            }
        }

        private bool ComponentsInitialized
        {
            get 
            { 
                return m_layersInitialized && m_behaviorsPendingInitCount == 0 && m_toolsPendingInitCount == 0; 
            }
        }

        private void OnFilterFeaturesOnMapExtentChanged()
        {
            if (AttributeDisplay != null)
                AttributeDisplay.FilterFeaturesByMapExtent = FilterFeaturesOnMapExtentChanged;
        }

        private void OnAutoScrollRowOnMouseOverChanged()
        {
            if (AttributeDisplay != null)
                AttributeDisplay.AutoScrollToGraphic = AutoScrollRowOnMouseOver;
        }

        private bool m_HasLayerThatUpdatesOnExtentChanged { get; set; }

        public View() : this(null, null)
        {
        }

        public View(IApplicationServices applicationServices=null, WindowManager manager = null)
        {
            urls = new ApplicationUrls();
            Urls.ProxyUrl = string.Empty;
            Culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            Content = null;
            setInstance(this);
            WindowManager = manager ?? new WindowManager();
            VerticalContentAlignment = System.Windows.VerticalAlignment.Stretch;
            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;

            MapApplication.SetApplication(this);
            IApplicationAdmin appAdmin = MapApplication.Current as IApplicationAdmin;
            if (appAdmin != null)
            {
                if (appAdmin.ConfigurableControls != null)
                {
                    appAdmin.ConfigurableControls.Clear();
                    appAdmin.ConfigurableControls.CollectionChanged -= ConfigurableControls_CollectionChanged;
                    appAdmin.ConfigurableControls.CollectionChanged += ConfigurableControls_CollectionChanged;
                }
            }

            SidePanelHelper.Reset();
        }

        internal void Dispose()
        {
            IApplicationAdmin appAdmin = MapApplication.Current as IApplicationAdmin;
            if (appAdmin != null)
            {
                if (appAdmin.ConfigurableControls != null)
                    appAdmin.ConfigurableControls.CollectionChanged -= ConfigurableControls_CollectionChanged;
            }
            WebMapSettings webMapSettings = ViewerApplication.WebMapSettings;
            if (webMapSettings != null && webMapSettings.Document != null)
            {
                webMapSettings.Document.GetItemCompleted -= LoadWebMap_GetItemCompleted;
                webMapSettings.Document.GetMapCompleted -= LoadWebMap_GetMapCompleted;
            }
        }

        void ConfigurableControls_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
            {
                foreach(FrameworkElement element in e.NewItems)
                {
                    LoadConfigurationForConfigurableControl(element);
                }
            }
        }

        // This was made public so that the Builder application could utilize this logic when layers are added or removed
        // from the map content control and thus remain in sync with the map layers.
        private void SetSelectedLayer(LayerEventArgs e)
        {
            SelectedLayer = e.Layer;
            OnSelectedLayerChanged(e.Layer);
        }

        internal void SetScaleBarMapUnit(MapUnit unit)
        {
            _getMapUnitsRetryCount = 0;

            if (ScaleBar != null)
            {
                ScaleBar.MapUnit = unit;
                // Scalebar updates on extent changed.  Force event without altering extent.
                if (Map.Extent != null)
                    Map.Extent = Map.Extent.Clone();
            }
                ScaleBarExtensions.SetScaleBarMapUnit(Map, unit);
            }

        private void AddLayer(Layer layer, bool isSelected, string layerDisplayName)
        {
            if (layer == null || Map == null)
                return;

            //make sure we mark the layer as selected before adding it to the collection
            Core.LayerExtensions.SetLayerProperties(layer, Map, layerDisplayName, false);

            if (string.IsNullOrEmpty(layer.ID) || (!string.IsNullOrEmpty(layer.ID) && Map.Layers[layer.ID] != null))
                layer.ID = Guid.NewGuid().ToString("N");

            Map.Layers.Add(layer);
        }

        private void DeleteLayerFromMap(Layer layer)
        {
            if (Map != null)
                Map.Layers.Remove(layer);
        }

        public IEnumerable<string> ExtensionUrls { get; set; }
        public override void OnApplyTemplate()
        {
            ExtensionsManager.LoadAllExtensions(ExtensionUrls, onExtensionsLoadCompleted, onExtensionLoadFailed);
        }

        private void onExtensionLoadFailed(object sender, ExceptionEventArgs args)
        {
            OnExtensionLoadFailed(args);
        }

        protected virtual void OnExtensionLoadFailed(ExceptionEventArgs args)
        {
            if (ExtensionLoadFailed != null)
                ExtensionLoadFailed(this, args);
        }
        public event EventHandler<ExceptionEventArgs> ExtensionLoadFailed;

        private void onExtensionsLoadCompleted(object sender, EventArgs args)
        {
            ExtensionCommands = new ObservableCollection<ICommand>();
            IEnumerable<Type> exportedCommands = AssemblyManager.GetExportsForType(typeof(ICommand));
            if (exportedCommands != null)
            {
                foreach (Type type in exportedCommands)
                {
                    ICommand command = Activator.CreateInstance(type) as ICommand;
                    if(command != null)
                        ExtensionCommands.Add(command);
                }
            }

            ExtensionMapBehaviors = new ObservableCollection<Behavior<Map>>();
            IEnumerable<Type> exportedBehaviors = AssemblyManager.GetExportsForType(typeof(Behavior<Map>));
            if (exportedBehaviors != null)
            {
                foreach (Type type in exportedBehaviors)
                {
                    Behavior<Map> behavior = Activator.CreateInstance(type) as Behavior<Map>;
                    if (behavior != null)
                        ExtensionMapBehaviors.Add(behavior);
                }
            }
            // At this point, we have all available (exported) commands, behaviors, UI elements etc

            if (LayoutProvider == null)
                throw new InvalidOperationException(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionMustSpecifyLayoutProvider);

            LayoutProvider.GetLayout(null, onLayoutProviderCompleted, onLayoutProviderCompleted);
        }

        public List<ResourceDictionary> LayoutResourceDictionaries { get; set; }
        private void onLayoutProviderCompleted(object sender, EventArgs args)
        {
            LayoutEventArgs layoutArgs = args as LayoutEventArgs;

            if (LayoutResourceDictionaries != null)
            {
                foreach (ResourceDictionary item in LayoutResourceDictionaries)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(item);
                    layoutArgs.Content.Resources.MergedDictionaries.Add(item);
                }
            }
            // if no layout resource dictionaries have been defined, move resources from root 
            // element of layout into application resources
            else if (layoutArgs != null && layoutArgs.Content != null && layoutArgs.Content.Resources != null)
            {
                foreach (object key in layoutArgs.Content.Resources.Keys)
                {
                    object resource = layoutArgs.Content.Resources[key];
                    layoutArgs.Content.Resources.Remove(key);
                    Application.Current.Resources.Add(key, resource);
                }
            }

            ApplyLayout(layoutArgs);

            if (ThemeProvider == null)
                throw new InvalidOperationException(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionMustSpecifyThemeProvider);

            ThemeProvider.GetApplicationTheme(null, onThemeProviderCompleted, onThemeProviderCompleted);
        }

        private void onThemeProviderCompleted(object sender, EventArgs args)
        {
            // Save the Themes
            ThemeColorSet = ThemeProvider.ThemeColorSet;

            if (ConfigurationStoreProvider == null)
                throw new InvalidOperationException(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionMustSpecifyConfigurationStoreProvider);

            if (ConfigurationStore == null)
            {
                ConfigurationStoreProvider.GetConfigurationStoreFailed -= ConfigurationStoreProvider_GetConfigurationStoreCompleted;
                ConfigurationStoreProvider.GetConfigurationStoreFailed += ConfigurationStoreProvider_GetConfigurationStoreCompleted;
                ConfigurationStoreProvider.GetConfigurationStoreCompleted -= ConfigurationStoreProvider_GetConfigurationStoreCompleted;
                ConfigurationStoreProvider.GetConfigurationStoreCompleted += ConfigurationStoreProvider_GetConfigurationStoreCompleted;
                ConfigurationStoreProvider.GetConfigurationStoreAsync(null);
            }
            else
            {
                loadMapConfiguration();
            }
        }

        void ConfigurationStoreProvider_GetConfigurationStoreCompleted(object sender, EventArgs args)
        {
            GetConfigurationStoreCompletedEventArgs e = args as GetConfigurationStoreCompletedEventArgs;
            if (e != null)
            {
                ConfigurationStore = e.ConfigurationStore;
                ConfigurationStore.Current = e.ConfigurationStore;
                Urls.GeometryServiceUrl = new ConfigurationStoreHelper().GetGeometryServiceUrl(ConfigurationStore);
            }
            loadMapConfiguration();
        }

        private void loadMapConfiguration()
        {
            if (ConfigurationProvider == null)
                throw new InvalidOperationException(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionMustSpecifyConfigurationProvider);

            ConfigurationProvider.GetConfigurationAsync(null, ConfigurationProvider_ConfigurationLoaded, ConfigurationProvider_ConfigurationLoaded);
        }

        void ConfigurationProvider_ConfigurationLoaded(object sender, EventArgs args)
        {
            GetConfigurationCompletedEventArgs e = args as GetConfigurationCompletedEventArgs;
            if (e == null || e.Map == null)
            {
                // If loading map from config file failed, try loading from web map
                if (ViewerApplication.WebMapSettings.Linked == true)
                    applyMap(null);
                else
                OnConfigurationLoadFailed(new ExceptionEventArgs(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionMapNotSet, null));
            }
            else
            {
            applyMap(e.Map);
        }
        }

        private void applyMap(Map map)
        {
            try
            {
                WebMapSettings webMapSettings = ViewerApplication.WebMapSettings;

                // If map has web map ID but IsLinked is uninitialized (i.e. null), default to loading the web map.
                // Otherwise, rely on the value of IsLinked
                if ((webMapSettings.Linked == null && !string.IsNullOrEmpty(webMapSettings.ID)) 
                || webMapSettings.Linked == true)
                    {
                    initializeFromWebMap(map);
                }
                else
                {
                    loadMap(map);

                    // Get the item info for the web map that the view is based on
                    if (!string.IsNullOrEmpty(webMapSettings.ID) && webMapSettings.Linked == false)
                    {
                        webMapSettings.Document.GetItemCompleted += (o, e) =>
                            {
                                if (e.ItemInfo != null)
                                ViewerApplication.WebMapSettings.ItemInfo = e.ItemInfo;
                                initializeBindings();
                                OnConfigurationLoaded(EventArgs.Empty);
                            };
                        webMapSettings.Document.GetItemAsync(webMapSettings.ID);
                    }
                    else
                    {
                        initializeBindings();
                        OnConfigurationLoaded(EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError(ex);
                OnConfigurationLoadFailed(new ExceptionEventArgs(ex, null));
                return;
            }
        }

        private void initializeAgolFromDocument(ESRI.ArcGIS.Client.WebMap.Document doc, 
            Action<ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnline> callback)
        {
            if (doc == null || callback == null)
                return;

            ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnline agol = new ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnline();
            string baseUrl = doc.ServerBaseUrl;
            string webMapSharingUrl = baseUrl.EndsWith("content", StringComparison.OrdinalIgnoreCase) ?
                baseUrl.Substring(0, baseUrl.Length - 7) : baseUrl;
            string webMapSharingUrlSecure = webMapSharingUrl;

            // Initalize SSL and non-SSL sharing Urls
            if (webMapSharingUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                webMapSharingUrl = webMapSharingUrl.ToLower().Replace("https://", "http://");
            else if (webMapSharingUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                webMapSharingUrlSecure = webMapSharingUrlSecure.ToLower().Replace("http://", "https://");

            agol.Initialized += (o, e) => { callback(agol); };
            agol.Initialize(webMapSharingUrl, webMapSharingUrlSecure);
        }

        private void initializeFromWebMap(Map map, ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnline agol = null)
        {
            WebMapSettings webMapSettings = ViewerApplication.WebMapSettings;
            LoadWebMap(webMapSettings.ID, webMapSettings.Document, (a) =>
            {
                if (a.Error == null)
                {
                    associateBehaviorsWithMap(Map);
                    initializeBindings();
                    OnConfigurationLoaded(EventArgs.Empty);
                }
                else if (webMapSettings.Document.Token != null)
                {
                    // try without token
                    webMapSettings.Document.Token = null;
                    applyMap(map);
                }
                else
                {
                    // Could not load the web map
                    OnConfigurationLoadFailed(new ExceptionEventArgs(Strings.WebMapLoadFailed, null));
                }
            });
        }

        private void WebMapSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Linked")
            {
                WebMapSettings webMapSettings = ViewerApplication.WebMapSettings;
                if (!string.IsNullOrEmpty(webMapSettings.ID)
                    && webMapSettings.Linked == true)
                {
                    if (View.Instance.LinkedToWebMap != null)
                        View.Instance.LinkedToWebMap(View.Instance, EventArgs.Empty);
                }
            }
        }

        private void loadMap(Map map)
        {
            if (map == null)
                return;

            if (Map != null)
            {
                Map.Layers.Clear();
                //Clear map behaviors
                BehaviorCollection behaviors = Interaction.GetBehaviors(Map);
                if (behaviors != null)
                    behaviors.Clear();
                behaviors = Interaction.GetBehaviors(map);
                if (behaviors != null)
                    behaviors.Clear();
                // Copy extent, layers, map tip
                if (map.Extent != null)
                    Map.Extent = map.Extent.Clone();
                int layerCount = map.Layers.Count;

                for (int i = 0; i < layerCount; i++)
                {
                    Layer l = map.Layers[0];
                    map.Layers.Remove(l);

                    // Handle FeatureLayers created from feature collections
                    //
                    // NOTE - this is a work around for the limitation of the API that feature 
                    // collection layers can only be instantiated using JSON.  Since the Viewer's Map 
                    // serialization format is XAML, the layer deserialized as part of the map cannot
                    // be used directly.
                    if (l is FeatureLayer && string.IsNullOrEmpty(((FeatureLayer)l).Url))
                    {
                        // Get the feature collection JSON
                        string json = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetFeatureCollectionJson((FeatureLayer)l);
                        FeatureLayer newLayer = null;
                        if (!string.IsNullOrEmpty(json))
                        {
                            // Create an new layer from the JSON.
                            newLayer = FeatureLayer.FromJson(json);

                            // Copy the properties from the layer deserialized along with the map to
                            // the layer that was instantiated from feature collection JSON
                            ((FeatureLayer)l).CopyProperties(newLayer);

                            // Replace the layer from the deserialized map
                            l = newLayer;
                        }
                    }

                    l.ProcessWebMapProperties();
                    l.InitializationFailed += l_InitializationFailed;
                    Map.Layers.Add(l);
                }
                Map.GetMapUnitsAsync(SetScaleBarMapUnit);

                associateBehaviorsWithMap(Map);
            }
        }

        private void initializeBindings()
        {
            checkMapLayersForUpdatesOnExtentChanged();
            if (AttributeDisplay != null && AttributeDisplay.Map == null)
                AttributeDisplay.Map = Map;

            if (MapContents != null)
                MapContents.Map = Map;

            if (Editor != null)
                Editor.Map = Map;

            if (ScaleBar != null)
            {
                ScaleBar.MapUnit = ScaleBarExtensions.GetScaleBarMapUnit(Map);
                ScaleBar.Map = Map;
            }

            if (ProgressGauge != null)
                ProgressGauge.Map = Map;

            if (AttributionDisplayControl != null)
                AttributionDisplayControl.Map = Map;
        }

        public void loadToolPanels(string toolPanelsXml)
        {
            FrameworkElement layoutFileElement = this.Content as FrameworkElement;
            if (layoutFileElement != null)
            {
                if (!string.IsNullOrWhiteSpace(toolPanelsXml))
                {
                    XDocument xDoc = XDocument.Parse(toolPanelsXml);
                    XElement rootElement = xDoc.FirstNode as XElement;

                    ToolPanels.Current.PopulateToolPanelsFromXml(rootElement);
                }

                if (ToolPanels.Current != null && ToolPanels.Current.Count > 0)
                {
                    foreach (ToolPanel toolPanel in ToolPanels.Current)
                    {
                        // Add to UI
                        ContentControl toolPanelContainer = layoutFileElement.FindName(toolPanel.ContainerName) as ContentControl;
                        if (toolPanelContainer != null)
                            toolPanelContainer.Content = toolPanel;

                        // Hook to state change method to refresh other toolPanelss when the state of one changes
                        toolPanel.ToolStateChanged += toolPanel_ToolStateChanged;

                        foreach (var item in toolPanel.ToolPanelItems)
                        {
                            if (item is ButtonBase && ((ButtonBase)item).Command is INotifyInitialized)
                            {
                                var notifyInit = (INotifyInitialized)((ButtonBase)item).Command;
                                if (!notifyInit.IsInitialzed && notifyInit.InitializationError == null)
                                {
                                    m_toolsPendingInitCount++;

                                    EventHandler initHandler = null;
                                    initHandler = (o, e) =>
                                    {
                                        var n = (INotifyInitialized)o;
                                        n.Initialized -= initHandler;
                                        n.InitializationFailed -= initHandler;
                                        m_toolsPendingInitCount--;

                                        if (n.InitializationError != null)
                                            Logger.Instance.LogError(n.InitializationError);

                                        OnInitialized();
                                    };

                                    notifyInit.Initialized += initHandler;
                                    notifyInit.InitializationFailed += initHandler;
                                }
                                else if (notifyInit.InitializationError != null)
                                {
                                    Logger.Instance.LogError(notifyInit.InitializationError);
                                }
                            }
                        }
                    }
                }
            }

            // Also add toolPanel containers which are configurable controls 
            // This code will eventually go away once we make the toolPanel control public
            if (IsEditMode)
            {
                IApplicationAdmin appAdmin = MapApplication.Current as IApplicationAdmin;
                if (appAdmin != null && appAdmin.ConfigurableControls != null)
                {
                    foreach (FrameworkElement control in appAdmin.ConfigurableControls)
                    {
                        ContentControl ctrl = control as ContentControl;
                        if (ctrl != null && ctrl.Content == null && !string.IsNullOrWhiteSpace(ctrl.Name) &&
                            !(ctrl is EsriMapContents.MapContents)) //do this to temporary ensure that MapContents won't show in the list of toolpanels
                        {
                            if (ToolPanels.Current[ctrl.Name] != null)
                                continue;

                            ToolPanel newToolPanel = new ToolPanel()
                            {
                                ContainerName = ctrl.Name,
                                Name = ctrl.Name + "ToolPanel",
                            };
                            ToolPanels.Current.Add(newToolPanel);
                            try
                            {
                                ctrl.Content = newToolPanel;
                                // Hook to state change method to refresh other toolPanels when the state of one changes
                                newToolPanel.ToolStateChanged += toolPanel_ToolStateChanged;
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        private void toolPanel_ToolStateChanged(object sender, CommandEventArgs e)
        {
            ToolPanel sourceToolPanel = sender as ToolPanel;
            foreach (ToolPanel toolPanel in ToolPanels.Current)
            {
                if (toolPanel != sourceToolPanel)
                    toolPanel.Refresh();
            }
        }

        void l_InitializationFailed(object sender, EventArgs e)
        {
            Layer lay = sender as Layer;
            if (lay != null)
            {
                if (lay.InitializationFailure != null)
                {
                    NotificationPanel.Instance.AddNotification(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.LayerInitFailure, lay.InitializationFailure.Message != null ? lay.InitializationFailure.Message : ESRI.ArcGIS.Mapping.Controls.Resources.Strings.LayerNotInitialized, lay.InitializationFailure.ToString(), MessageType.Error);
                    lay.SetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.ErrorMessageProperty, lay.InitializationFailure.Message);
                }
                else
                    lay.SetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.ErrorMessageProperty, ESRI.ArcGIS.Mapping.Controls.Resources.Strings.LayerNotInitialized);

                lay.InitializationFailed -= layerInitFailed;
            }
        }

        private void AttachToControlEvents()
        {
            if (Map != null)
            {
                Map.Layers.LayersInitialized += Layers_LayersInitialized;
                Map.Layers.CollectionChanged += Layers_CollectionChanged;
            }
        }

        void InitializeNewLayer(Layer layer)
        {
            bool layerInitialized = IsLayerInitialized(layer);

            if (!layerInitialized)
                SubscribeToLayerInitializationEvents(layer, true);

            if (layerInitialized)
                PerformPostLayerInitializationActions(layer, true);
        }

        private void checkMapLayersForUpdatesOnExtentChanged()
        {
            // Check if all of the other layers are off as well
            bool allDisabled = true;
            foreach (Layer layer in Map.Layers)
            {
                CustomGraphicsLayer graphicsLayer = layer as CustomGraphicsLayer;
                if (graphicsLayer == null)
                    continue;

                if (Core.LayerExtensions.GetAutoUpdateOnExtentChanged(graphicsLayer))
                {
                    allDisabled = false;
                    break;
                }
            }
            if (allDisabled) // none of the layers are checking for updates on extent changed
                m_HasLayerThatUpdatesOnExtentChanged = false;
            else
                m_HasLayerThatUpdatesOnExtentChanged = true;
        }

        void ConnectionStoreProvider_ConfigurationStoreLoadFailed(object sender, ExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                Dispatcher.BeginInvoke((Action)delegate() // switch to UI thread
                {
                    Logger.Instance.LogError(e.Exception);
                    OnConfigurationStoreLoadFailed(new ExceptionEventArgs(e.Exception, null));
                });
                return;
            }
        }

        void ConnectionStoreProvider_ConfigurationStoreLoaded(object sender, GetConfigurationStoreCompletedEventArgs e)
        {
            if (e.ConfigurationStore == null)
            {
                Dispatcher.BeginInvoke((Action)delegate() // switch to UI thread
                {
                    Logger.Instance.LogError(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.LogUnableToRetrieveConfigurationInformation);
                    OnConfigurationLoadFailed(new ExceptionEventArgs(new Exception(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionUnableToRetrieveConfigurationInformation), null));
                });
                return;
            }

            ConfigurationStore = e.ConfigurationStore;
            ConfigurationStore.Current = e.ConfigurationStore;
            Urls.GeometryServiceUrl = new ConfigurationStoreHelper().GetGeometryServiceUrl(ConfigurationStore);
            if (ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnlineEnvironment.ConfigurationUrls != null)
                ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnlineEnvironment.ConfigurationUrls.GeometryServer = Urls.GeometryServiceUrl;
        }

        public void ApplyLayout(LayoutEventArgs e)
        {
            if (e == null)
                return;

            this.Content = e.Content;
            Map = e.Map;

            if (e.AttributeTableContainer != null)
            {
                if (e.AttributeTableContainer.Content == null)
                {
                    AttributeDisplay = new AttributeDisplay()
                    {
                        FeatureDataGrid = new FeatureDataGrid(),
                        AutoScrollToGraphic = AutoScrollRowOnMouseOver,
                        FilterFeaturesByMapExtent = FilterFeaturesOnMapExtentChanged,
                    };
                    e.AttributeTableContainer.Content = AttributeDisplay;
                }
                else
                {
                    FeatureDataGrid featureDataGrid = e.AttributeTableContainer.Content as FeatureDataGrid;
                    if (featureDataGrid != null)
                    {
                        e.AttributeTableContainer.Content = null; // remove from parent
                        AttributeDisplay = new AttributeDisplay()
                        {
                            FeatureDataGrid = featureDataGrid,
                            AutoScrollToGraphic = AutoScrollRowOnMouseOver,
                            FilterFeaturesByMapExtent = FilterFeaturesOnMapExtentChanged,
                        };
                        e.AttributeTableContainer.Content = AttributeDisplay;
                    }
                }
                if (AttributeDisplay != null && AttributeDisplay.FeatureDataGrid != null)
                {
                    try
                    {
                        Binding binding = XamlReader.Load("<Binding xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:extensibility=\"http://schemas.esri.com/arcgis/client/extensibility/2010\" Path=\"GraphicsLayer.(extensibility:LayerProperties.IsEditable)\" />") as Binding;
                        binding.Converter = new ReverseBooleanConverter();
                        binding.Source = AttributeDisplay;
                        BindingOperations.SetBinding(AttributeDisplay.FeatureDataGrid, DataGrid.IsReadOnlyProperty, binding);
                    }
                    catch
                    {
                        //Swallow
                    }

                    // workaround to avoid issue where initial load of attribute table (DataGrid) crashes 
                    // Silverlight when table includes columns with numeric field names.  
                    GraphicsLayer layer = AttributeDisplay.FeatureDataGrid.GraphicsLayer;
                    AttributeDisplay.FeatureDataGrid.GraphicsLayer = null;

                    ToggleTableCommand cmd = new ToggleTableCommand();

                    RoutedEventHandler loaded = null;
                    loaded = (o, args) =>
                    { 
                        cmd.Execute(null);
                        AttributeDisplay.FeatureDataGrid.Loaded -= loaded;
                        AttributeDisplay.FeatureDataGrid.GraphicsLayer = layer;
                    };

                    AttributeDisplay.FeatureDataGrid.Loaded += loaded;
                    cmd.Execute(null);
                }
            }

            if (e.ScaleBarContainer != null && e.ScaleBarContainer.Content == null)
            {
                e.ScaleBarContainer.Content = ScaleBar = new ScaleBar()
                {
                    TargetWidth = 150,
                    MapUnit = MapUnit.Undefined,
                };
            }

            if (e.ProgressIndicatorContainer != null && e.ProgressIndicatorContainer.Content == null)
            {
                e.ProgressIndicatorContainer.Content = ProgressGauge = new ProgressGauge();
            }

            if (e.AttributionDisplayContainer != null && e.AttributionDisplayContainer.Content == null)
            {
                e.AttributionDisplayContainer.Content = AttributionDisplayControl = new AttributionDisplayControl() { 
                    Foreground = e.AttributionDisplayContainer.Foreground,
                };
                AttributionDisplayControl.SetBinding(FrameworkElement.VisibilityProperty, new System.Windows.Data.Binding("Visibility") { Source = e.AttributionDisplayContainer, Mode = System.Windows.Data.BindingMode.TwoWay });
            }

            Editor = MapApplication.Current.FindObjectInLayout(ControlNames.EDITORWIDGET) as EditorWidget;
            if (e.EditorConfigContainer != null && e.EditorConfigContainer.Content == null)
            {
                EditorConfigControl = new GenericConfigControl();
                EditorConfigControl.Name = "EditorConfiguration";
                EditorConfigControl.Title = Controls.Resources.Strings.ConfigureEditorWidget;
                EditorConfigControl.Command = new ConfigureEditorCommand() { Map = Map };
                ElementExtensions.SetIsConfigurable(EditorConfigControl, true);
                object[] attributes = typeof(ToggleEditCommand).GetCustomAttributes(typeof(DisplayNameAttribute), false);
                if (attributes.Length > 0 && !string.IsNullOrEmpty(((DisplayNameAttribute)attributes[0]).Name))
                    ElementExtensions.SetDisplayName(EditorConfigControl, ((DisplayNameAttribute)attributes[0]).Name);
                e.EditorConfigContainer.Content = EditorConfigControl;

                // When done in XAML the binding doesn't work properly until the editorwidget is made visible.
                if (Editor != null)
                {
                    BindingOperations.SetBinding(Editor, EditorWidget.AlwaysDisplayDefaultTemplatesProperty, new Binding("DataContext.AlwaysDisplayDefaultTemplates") { Source = e.EditorConfigContainer });
                    BindingOperations.SetBinding(Editor, EditorWidget.AutoCompleteProperty, new Binding("DataContext.AutoComplete") { Source = e.EditorConfigContainer, Mode = BindingMode.TwoWay });
                    BindingOperations.SetBinding(Editor, EditorWidget.AutoSelectProperty, new Binding("DataContext.AutoSelect") { Source = e.EditorConfigContainer });
                    BindingOperations.SetBinding(Editor, EditorWidget.ContinuousProperty, new Binding("DataContext.Continuous") { Source = e.EditorConfigContainer });
                    BindingOperations.SetBinding(Editor, EditorWidget.EditVerticesEnabledProperty, new Binding("DataContext.EditVerticesEnabled") { Source = e.EditorConfigContainer });
                    BindingOperations.SetBinding(Editor, EditorWidget.FreehandProperty, new Binding("DataContext.Freehand") { Source = e.EditorConfigContainer, Mode = BindingMode.TwoWay });
                    BindingOperations.SetBinding(Editor, EditorWidget.MaintainAspectRatioProperty, new Binding("DataContext.MaintainAspectRatio") { Source = e.EditorConfigContainer });
                    BindingOperations.SetBinding(Editor, EditorWidget.MoveEnabledProperty, new Binding("DataContext.MoveEnabled") { Source = e.EditorConfigContainer });
                    BindingOperations.SetBinding(Editor, EditorWidget.RotateEnabledProperty, new Binding("DataContext.RotateEnabled") { Source = e.EditorConfigContainer });
                    BindingOperations.SetBinding(Editor, EditorWidget.ScaleEnabledProperty, new Binding("DataContext.ScaleEnabled") { Source = e.EditorConfigContainer });
                    BindingOperations.SetBinding(Editor, EditorWidget.LayerIDsProperty, new Binding("DataContext.LayerIds") { Source = e.EditorConfigContainer });
                }
            }

            ContentControl container = MapApplication.Current.FindObjectInLayout(ControlNames.ADDCONTENTCONTROLCONTAINER) as ContentControl;
            if (container != null)
            {
                AddContentDialog addContentDialog = new AddContentDialog()
                {
                    DataSourceProvider = DataSourceProvider,
                    ConnectionsProvider = ConnectionsProvider,
                    BorderThickness = new Thickness(0),
                    Map = Map,
                };
                addContentDialog.LayerAdded += AddContentDialog_LayerAdded;
                addContentDialog.LayerAddFailed += AddContentDialog_LayerAddFailed;
                container.Content = addContentDialog;
            }

            container = MapApplication.Current.FindObjectInLayout(ControlNames.MAPCONTENTS_CONTROL_CONTAINER_NAME) as ContentControl;
            if (container != null)
            {
                MapContents = new EsriMapContents.MapContents();
                MapContents.Name = ControlNames.MAPCONTENTS_CONTROL_NAME;
                ElementExtensions.SetIsConfigurable(MapContents, true);
                object[] attributes = typeof(ToggleMapContentsCommand).GetCustomAttributes(typeof(DisplayNameAttribute), false);
                if (attributes.Length > 0 && !string.IsNullOrEmpty(((DisplayNameAttribute)attributes[0]).Name))
                    ElementExtensions.SetDisplayName(MapContents, ((DisplayNameAttribute)attributes[0]).Name);
                MapContents.Map = Map;
                container.Content = MapContents;

                // Push container foreground and background to Map Contents
                Binding b = new Binding("Foreground") { Source = container };
                MapContents.SetBinding(EsriMapContents.MapContents.ForegroundProperty, b);
                b = new Binding("Background") { Source = container };
                MapContents.SetBinding(EsriMapContents.MapContents.BackgroundProperty, b);
            }

            if (Editor != null)
                Editor.Map = Map;

            WindowManager.FloatingWindowStyle = LayoutStyleHelper.Instance.GetStyle(WindowManager.FloatingWindowStyleKey);

            // Set the logic to be executed when an authentication challenge occurs
            IdentityManager.Current.ChallengeMethod = OnChallenge;
            if (!listeningToCredentials)
            {
                // Listen for changes in the application environment's set of credentials, setting a flag to 
                // ensure that this is only done once.
                listeningToCredentials = true;
                UserManagement.Current.Credentials.CollectionChanged += Credentials_CollectionChanged;
            }

            AttachToControlEvents();
            SubscribeToSymbolConfigProviderEvents();
        }

        // Raised when the set of credentials retrieved for the application changes
        private void Credentials_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null || (e.Action == NotifyCollectionChangedAction.Reset))
            {
                if (signedOutTimer == null)
                    signedOutTimer = new ThrottleTimer(2000, () => signedOut = false);
                signedOut = true;
                signedOutTimer.Invoke();
            }
        }

        private bool listeningToCredentials; // Tracks whether credentials' collection changed event has been hooked to
        private ThrottleTimer signedOutTimer; // Timer for suppressing challenges immediately after sign out
        private bool signedOut; // Flag indicating whether a sign-out has just occurred

        private ThrottleTimer signInCancelledTimer; // Timer for suppressing challenges immediately after sign-in has been cancelled
        private bool signInCancelled; // Flag indicating whether a sign-in cancellation has just occurred
        private string signInCancelUrl; // URL for which sign-in was cancelled

        private ThrottleTimer credentialCacheTimer; // Timer for suppressing challenges immediately after sign-in has completed
        private IdentityManager.Credential cachedCredential; // Credential that was just retrieved from sign-in
        
        // Raised when an authentication challenge occurs in the application
        private async void OnChallenge(string url, Action<IdentityManager.Credential, Exception> callback,
            IdentityManager.GenerateTokenOptions options = null)
        {
            if (signedOut) // Sign-out occurred within the last two seconds.  Do not prompt for login.
            {
                callback(null, null);
                return;
            }

            var uri = new Uri(url);
            var tree = uri.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (tree[tree.Length - 2].ToLower() == "services" && tree[tree.Length - 3].ToLower() == "rest")
            {
                // Challenge is coming from a secure folder.  Do not prompt.
                callback(null, null);
                return;
            }

            // In some cases, multiple challenges are raised for resources at the same endpoint (e.g. tiles from 
            // hosted tile services in ArcGIS Online).  To keep the user from being prompted to login multiple times
            // in succession, each new credential is cached for a half-second.  If another challenge is raised
            // within a half second, and the domain of the current challenge matches that of the cached credential,
            // then the cached credential is used.

            // Initialize timer for clearing cached credential
            if (credentialCacheTimer == null)
            {
                credentialCacheTimer = new ThrottleTimer(2000, () => cachedCredential = null);
            }


            // If there is a credential cached, then sign-in has just occurred.  Check to see if the saved 
            // should also be used for this challenge
            if (cachedCredential != null)
            {
                try
                {
                    // check whether the domain of the currently requested URL matches that of the cached credential
                    if ((new Uri(url)).Domain().ToLower() == (new Uri(cachedCredential.Url).Domain().ToLower()))
                    {
                        // Domains match, so use the cached credential
                        callback(cachedCredential, null);
                        return;
                    }
                }
                catch { }
            }


            // Sometimes challenges are raised after sign-in is cancelled.  To avoid prompting the user after
            // cancellation, this timer will suppress challenges for two seconds.

            // Initialize timer for resetting sign-in cancellation tracking
            if (signInCancelledTimer == null)
            {
                signInCancelledTimer = new ThrottleTimer(2000, () =>
                {
                    // Two seconds has expired since sign-in was cancelled.  Reset tracking variables.
                    signInCancelled = false;
                    signInCancelUrl = null;
                });
            }

            // Check whether sign-in has been cancelled within two seconds
            if (signInCancelled && !string.IsNullOrEmpty(signInCancelUrl))
            {
                try
                {
                    // Check whether the domain of the requested URL matches that for the cancelled sign-in
                    Uri requestUri = new Uri(url);
                    Uri cancelUri = new Uri(signInCancelUrl);
                    if (requestUri.Domain().Equals(cancelUri.Domain(), StringComparison.OrdinalIgnoreCase)
                    && requestUri.AbsolutePath.Equals(cancelUri.AbsolutePath, StringComparison.OrdinalIgnoreCase))
                    {
                        // Domains match - do not prompt user
                        callback(null, null);
                        return;
                    }
                }
                catch { }
            }

            var proxyUrl = options != null ? options.ProxyUrl : null;
            // Sign in suppression checks passed.  Try to authenticate using existing credentials
            // Try existing credentials
            IdentityManager.Credential cred = await ApplicationHelper.TryExistingCredentials(url, proxyUrl);
            if (cred != null)
            {
                // The existing credentials were sufficient for authentication.

                // If the request was for a URL in the current ArcGIS Portal, sign into Portal
                if (url.IsPortalUrl())
                    await ApplicationHelper.SignInToPortal(cred);

                // If there is not already a credential in the app's credentials collection that has the
                // same URL, add this one to the collection
                if (!UserManagement.Current.Credentials.Any(c => c.Url != null
                && c.Url.Equals(cred.Url, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(c.Token)))
                    UserManagement.Current.Credentials.Add(cred);

                // Pass the credential to the callback
                callback(cred, null);
                return;
            }

            // Try just getting a credential for the URL without specifying login info.  This can work if 
            // the browser has authenticated the user (e.g. using PKI or IWA auth)
            try
            {
                cred = await IdentityManager.Current.GenerateCredentialTaskAsyncEx(url, options);
            }
            catch { } // Swallow authorization exception

            if (cred != null)
            {
                callback(cred, null);
                return;
            }

            // Existing credentials were insufficient.  Prompt user to sign in.

            SignInCommand signInCommand = null;
            if (url.IsPortalUrl()) // Sign into ArcGIS portal
                signInCommand = new SignInToAGSOLCommand() { ProxyUrl = proxyUrl };
            else // Sign into ArcGIS Server
                signInCommand = new SignInToServerCommand() { Url = url, ProxyUrl = proxyUrl };

            signInCommand.SignedIn += (o, e) =>
            {
                // Temporarily store the credential and start the timer.  This allows this credential
                // to be re-used if there is another challenge with the same domain within a couple 
                // seconds of sign-in.
                cachedCredential = e.Credential;
                credentialCacheTimer.Invoke();

                // Pass the retrieved credential to the callback
                callback(e.Credential, null);
            };
            signInCommand.Cancelled += (o, e) =>
            {
                // Set the flags indicating that sign-in was cancelled and start the timer.  If another
                // challenge for the same resouce is raised within the next couple seconds, it is assumed
                // that the user should not be prompted because cancellation of sign-in has just occurred.
                signInCancelled = true;
                signInCancelUrl = url;
                signInCancelledTimer.Invoke();

                // Pass the retrieved credential to the callback
                callback(null, null);
            };

            // Promt user to sign-in
            signInCommand.Execute(null);
        }

        /// <summary>
        /// This gracefully removes all the layers from the Map.
        /// </summary>
        public void Clear()
        {
            // Remove the visible layer
            SelectedLayer = null;

            // Remove the single onclickpopupcontrol
            PopupHelper.Reset();

            // Remove all layers
            if (Map != null && Map.Layers != null)
                Map.Layers.Clear();
        }

        private void LoadConfigurationForConfigurableControl(FrameworkElement configurableControl)
        {
            if (configurableControl == null || string.IsNullOrWhiteSpace(configurableControl.Name))
                return;

            string configData = null;
            ExtensionData extensionData = null;
            if (ExtensionsDataManager != null && ExtensionsDataManager.ExtensionsConfigData != null &&
                ExtensionsDataManager.ExtensionsConfigData.ExtensionsData != null && 
                ExtensionsDataManager.ExtensionsConfigData.ExtensionsData.TryGetValue(configurableControl.Name, out extensionData))
            {
                if (extensionData != null)
                    configData = extensionData.ConfigData;
            }
            configData = configData ?? string.Empty;
            ISupportsConfiguration supportsConfig = configurableControl as ISupportsConfiguration;
            if (supportsConfig != null)
            {
                try
                {
                    supportsConfig.LoadConfiguration(configData);
                }
                catch (Exception ex)
                {
                    MessageBoxDialog.Show(string.Format(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ErrorLoadConfigurationOfExtension, configurableControl.GetType().FullName) + Environment.NewLine + ex.Message);
                    Logger.Instance.LogError(ex);
                }
            }
        }

        public string GetConfigurationOfControls()
        {
            IApplicationAdmin appAdmin = MapApplication.Current as IApplicationAdmin;
            if (appAdmin == null || appAdmin.ConfigurableControls == null)
                return null;

            ExtensionsConfigData data = new ExtensionsConfigData();

            foreach (FrameworkElement elem in appAdmin.ConfigurableControls)
            {
                if (string.IsNullOrWhiteSpace(elem.Name))
                    continue;

                ExtensionData extensionData = null;
                if (ExtensionsDataManager.ExtensionsConfigData.ExtensionsData.TryGetValue(elem.Name, out extensionData))
                    {
                    if (data.ExtensionsData == null)
                        data.ExtensionsData = new Dictionary<string, ExtensionData>();
                    data.ExtensionsData.Add(elem.Name, extensionData);
                }
            }

            return data.ToXml();
        }

        internal void AddContentDialog_LayerAdded(object sender, LayerAddedEventArgs e)
        {
            if (e.Layer == null)
                return;
            if (View.Instance != null)
            {
                SidePanelHelper.ShowMapContents();
                if (e.Layer is FeatureLayer)
                {
                    FeatureLayer featureLayer = e.Layer as FeatureLayer;
                    featureLayer.UpdateCompleted += applyAutomaticClustering;
                }
                else if (e.Layer is CustomGraphicsLayer)
                {
                    CustomGraphicsLayer customGraphicsLayer = e.Layer as CustomGraphicsLayer;
                    customGraphicsLayer.UpdateCompleted += applyAutomaticClustering;
                }
                View.Instance.AddLayerToMap(e.Layer, true, null);
            }
        }

        void applyAutomaticClustering(object sender, EventArgs e)
        {
            GraphicsLayer gLayer = sender as GraphicsLayer;
            bool autoCluster = gLayer != null && 
                               gLayer.Graphics.Count >= Constants.AutoClusterFeaturesThresholdLimit &&
                               Core.LayerExtensions.GetGeometryType(gLayer) == GeometryType.Point && 
                               gLayer.Clusterer == null;

            if (Editor != null && Editor.LayerIDs == null)//only add clustering if "All Layers" are not selected for editing
                autoCluster = false;

            if(autoCluster)
                gLayer.Clusterer = new FlareClusterer();

            FeatureLayer featureLayer = gLayer as FeatureLayer;
            if (featureLayer != null)
                featureLayer.UpdateCompleted -= applyAutomaticClustering;
            CustomGraphicsLayer customGraphicsLayer = gLayer as CustomGraphicsLayer;
            if(customGraphicsLayer != null)
                customGraphicsLayer.UpdateCompleted -= applyAutomaticClustering;
        }

        internal void AddContentDialog_LayerAddFailed(object sender, ExceptionEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)
                    delegate
                    {

                        if (e.Exception.Message == ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionCachedMapServiceSpatialReferenceDoesNotMatch)
                            MessageBoxDialog.Show(LocalizableStrings.CachedMapServiceSpatialReferenceMisMatch, ESRI.ArcGIS.Mapping.Controls.Resources.Strings.CachedMapServiceSpatialReferenceMisMatchCaption, MessageBoxButton.OK);
                        else
                            MessageBoxDialog.Show(e.Exception.Message ?? ESRI.ArcGIS.Mapping.Controls.Resources.Strings.MsgUnknownError, ESRI.ArcGIS.Mapping.Controls.Resources.Strings.MsgUnableToCreateLayer, MessageBoxButton.OK);

                    });
        }

        void Layers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Layer newLayer in e.NewItems)
                {
                    // If graphics layer, hook event to detect when selection has changed
                    GraphicsLayer gLayer = newLayer as GraphicsLayer;
                    if (gLayer != null)
                    {
                        gLayer.PropertyChanged -= SelectionCountPropertyChanged;
                        gLayer.PropertyChanged += SelectionCountPropertyChanged;
                    }
                    InitializeNewLayer(newLayer);
                }
            }

            if (e.OldItems != null)
            {
                if (AttributeDisplay != null)
                {
                    foreach (Layer oldLayer in e.OldItems)
                    {
                        if (MapApplication.Current.SelectedLayer != oldLayer)
                            continue;

                        // if the attribute display is showing the current layer which was removed
                        // set the layer to null. When the next layer is selected, the attribute table will show it
                        if (AttributeDisplay.GraphicsLayer == oldLayer)
                        {
                            AttributeDisplay.GraphicsLayer = null;
                            break;
                        }

                        // Unhook monitoring of layer selection
                        GraphicsLayer gLayer = oldLayer as GraphicsLayer;
                        if (gLayer != null)
                            gLayer.PropertyChanged -= SelectionCountPropertyChanged;
                    }
                }
            }
        }

        private void SelectionCountPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectionCount")
                OnSelectionChanged(null);
        }

        #region Helper Functions
        bool mapInitComplete = false;
        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            if (Map.SpatialReference == null) // wait till spatial ref is not null
                return;
            mapInitComplete = true;

            LayerCollection layers = (LayerCollection)sender;
            foreach (Layer layer in layers)
            {
                if (SelectedLayer == null)
                    InitializeNewLayer(layer);

                loadPersistedGraphicsForGraphicLayers(layer);

            }
            OnMapLayersInitialized(args);
        }

        private void loadPersistedGraphicsForGraphicLayers(ESRI.ArcGIS.Client.Layer layer)
        {
            ESRI.ArcGIS.Client.GraphicsLayer graphicsLayer = layer as ESRI.ArcGIS.Client.GraphicsLayer;
            if (graphicsLayer == null
            || graphicsLayer is ICustomGraphicsLayer
            || (graphicsLayer is FeatureLayer && !string.IsNullOrEmpty(((FeatureLayer)graphicsLayer).Url))
            || graphicsLayer.Graphics.Count > 0)
                    return;

            string persistedDatasetJson = Core.LayerExtensions.GetDataset(graphicsLayer);
            if (string.IsNullOrEmpty(persistedDatasetJson))
                return;

            persistedDatasetJson = persistedDatasetJson.Replace("\"ControlTemplate\":null,", "");
            IEnumerable<PersistedGraphic> persistedGraphics = JsonSerializer.Deserialize<IEnumerable<PersistedGraphic>>(persistedDatasetJson,
                new Type[]{
                     typeof(ImageFillSymbol),typeof(ESRI.ArcGIS.Mapping.Core.Symbols.MarkerSymbol),
                        }
                );

            if (persistedGraphics != null)
            {
                List<Graphic> graphics = persistedGraphics.Where(p => p != null).Select(p => p.ToGraphic()).Where(p => p != null).ToList();

                GraphicsLayerTypeFixer.CorrectDataTypes(graphics, graphicsLayer);
                
                graphics.ForEach(p => graphicsLayer.Graphics.Add(p));
            }
            if (graphicsLayer.Renderer == null || graphicsLayer.Renderer is HiddenRenderer)
                PerformPostLayerInitializationActions(layer, true);
        }
        private void onBaseMapChanged(BaseMapChangedEventArgs e)
        {
            BaseMapInfo baseMapInfo = e.BaseMapInfo;
            ChangeBaseMap(baseMapInfo);
        }
        #endregion

        public void ZoomMapToExtent(Envelope env)
        {
            if (env == null || Map == null)
                return;

            if (!mapInitComplete)
                throw new InvalidOperationException(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionCannotCallZoomMapToExtentBeforeMapHasInitialized);

            if (Map.SpatialReference == null)
                throw new InvalidOperationException(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionCannotCallZoomMapToExtentBeforeMapHasInitializedMapReferenceNull);

            if (env.SpatialReference == null)
                throw new ArgumentException(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionMustSpecifySpatialReferenceOnEnvelope);

            if (Map.SpatialReference.Equals(env.SpatialReference))
            {
                Map.Extent = env;
            }
            else
            {
                GeometryServiceOperationHelper geomHelper = new GeometryServiceOperationHelper(
                                                                                                                new ConfigurationStoreHelper().GetGeometryServiceUrl(ConfigurationStore));
                geomHelper.GeometryServiceOperationFailed += (o, args) =>
                {
                    Logger.Instance.LogError(args.Exception);
                    throw args.Exception;
                };
                geomHelper.ProjectExtentCompleted += (o, args) =>
                {
                    Map.Extent = args.Extent;
                };
                geomHelper.ProjectExtent(env, Map.SpatialReference);
            }
        }

        public void ChangeBaseMap(BaseMapInfo baseMapInfo)
        {
            if (baseMapInfo.UseProxy)
                baseMapInfo.ProxyUrl = ProxyUrl;
            else
                baseMapInfo.ProxyUrl = null;

            if (baseMapInfo == null)
                throw new ArgumentNullException("baseMapInfo");
            IBaseMapDataSource dataSource = DataSourceProvider.CreateDataSourceForBaseMapType(baseMapInfo) as IBaseMapDataSource;
            if (dataSource == null)
                throw new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionUnableToInstantiateDatasourceForBaseMapType, baseMapInfo.BaseMapType.ToString()));
            if (Map == null)
                throw new Exception(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionMapHasNotBeenInitializedYet);
            if (Map.SpatialReference == null)
                throw new Exception(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionMapLayersHaveNotBeenInstantiated);
            dataSource.GetMapServiceMetaDataCompleted += new EventHandler<GetMapServiceMetaDataCompletedEventArgs>(dataSourceInstance_GetMapServiceMetaDataComplete);
            dataSource.GetMapServiceMetaDataFailed += (o, e) =>
            {
                MessageBoxDialog.Show(string.Format(Strings.ServiceConnectionError, baseMapInfo.Url), 
                    Strings.ServiceConnectionErrorCaption, MessageType.Error, MessageBoxButton.OK, null, true);
                OnBaseMapChangeFailed(e);
            };

            try
            {
                dataSource.GetMapServiceMetaDataAsync(baseMapInfo.Url, baseMapInfo);
            }
            catch (Exception ex) // One case where this will throw an exception is when a string that is not a URL is specified as the URL
            {
                MessageBoxDialog.Show(string.Format(Strings.ServiceConnectionError, baseMapInfo.Url),
                    Strings.ServiceConnectionErrorCaption, MessageType.Error, MessageBoxButton.OK, null, true);
                OnBaseMapChangeFailed(new ExceptionEventArgs(ex, null));
            }
        }

        void dataSourceInstance_GetMapServiceMetaDataComplete(object sender, GetMapServiceMetaDataCompletedEventArgs e)
        {
            BaseMapInfo baseMapInfo = e.UserState as BaseMapInfo;
            if (baseMapInfo == null)
                return;
            if (!e.IsCached)
            {
                MessageBoxDialog.Show(Strings.ExceptionOnlyCachedServicesCanBeUsedAsBaseMaps, Strings.UnsupportedBasemapType,
                    MessageType.Error, MessageBoxButton.OK, null, true);
                OnBaseMapChangeFailed(new ExceptionEventArgs(new Exception(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionOnlyCachedServicesCanBeUsedAsBaseMaps), e.UserState));
                return;
            }
            createNewMapBasedOnTargetSpatialReferenceForBaseMapLayer(baseMapInfo, e.SpatialReference, e.InitialExtent, e.FullExtent);
            SetScaleBarMapUnit(e.MapUnit);
        }

        #region Event Callbacks
        private void baseMapConfigWindow_BaseMapChanged(object sender, BaseMapChangedEventArgs e)
        {
            onBaseMapChanged(e);
        }

        private void createNewMapBasedOnTargetSpatialReferenceForBaseMapLayer(BaseMapInfo baseMapInfo, SpatialReference targetServiceSpatialReference,
                Envelope targetInitialExtent, Envelope targetFullExtent)
        {
            if (Map.SpatialReference.Equals(targetServiceSpatialReference))
            {
                // Spatial references are equal -> extents are valid to compare               
                Envelope targetExtent = Map.Extent;
                if (isFullyContainedWithin(targetExtent, targetFullExtent))
                    targetExtent = targetFullExtent; // if the full extent of the new service is fully within, automatically zoom to it                                
                reCreateBaseMapLayer(baseMapInfo, targetExtent);
            }
            else if (targetServiceSpatialReference != null)
            {
                // Spatial reference mismatch

                // Since cached services cannot re-project on the fly.
                // Check if the application has any cached map services for operational data. 
                // If so, Ask the user if they wish to delete them before changing basemap
                bool hasCachedMapServiceLayer = false;
                for (int i = 1; i < Map.Layers.Count; i++) // we only care about operational data .. not basemap layer (index = 0
                {
                    if (Map.Layers[i] is ArcGISTiledMapServiceLayer)
                    {
                        hasCachedMapServiceLayer = true;
                        break;
                    }
                }
                if (hasCachedMapServiceLayer)
                {
                    MessageBoxDialog.Show(LocalizableStrings.OperationMapServicesLayersWillBeRemoved, LocalizableStrings.OperationMapServicesLayersWillBeRemovedCaption, MessageBoxButton.OKCancel,
                                    new MessageBoxClosedEventHandler(delegate(object obj, MessageBoxClosedArgs args1)
                                    {
                                        if (args1.Result == MessageBoxResult.OK)
                                        {
                                            // Remove all cached map services
                                            for (int i = Map.Layers.Count - 1; i > 0; i--)
                                            {
                                                ArcGISTiledMapServiceLayer tiledMapServiceLayer = Map.Layers[i] as ArcGISTiledMapServiceLayer;
                                                if (tiledMapServiceLayer != null)
                                                    Map.Layers.Remove(tiledMapServiceLayer);
                                            }
                                        }
                                        else
                                        {
                                            OnBaseMapChangeFailed(new ExceptionEventArgs(new Exception(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionCacheMapServicesCannotReproject), null));
                                            return;
                                        }
                                        CreateNewBaseMapWithSpatialReference(targetInitialExtent, targetFullExtent, baseMapInfo, targetServiceSpatialReference);
                                    }));
                }
                else
                    CreateNewBaseMapWithSpatialReference(targetInitialExtent, targetFullExtent, baseMapInfo, targetServiceSpatialReference);
            }
            else
            {
                reCreateBaseMapLayer(baseMapInfo, null);
            }
        }

        private void CreateNewBaseMapWithSpatialReference(Envelope targetInitialExtent, Envelope targetFullExtent, BaseMapInfo baseMapInfo, SpatialReference targetServiceSpatialReference)
        {
            // Reproject oldMap's extent before comparison
            GeometryServiceOperationHelper geomHelper = new GeometryServiceOperationHelper(
                            new ConfigurationStoreHelper().GetGeometryServiceUrl(ConfigurationStore),
                            baseMapInfo.UseProxy ? baseMapInfo.ProxyUrl : null);
            geomHelper.GeometryServiceOperationFailed += (o, args) =>
            {
                Logger.Instance.LogError(args.Exception);
                MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.MsgUnableToAccessGeometryService + Environment.NewLine + args.Exception != null ? args.Exception.Message : ESRI.ArcGIS.Mapping.Controls.Resources.Strings.MsgUnknownError);
            };
            geomHelper.ProjectExtentCompleted += (o, args) =>
            {
                Envelope targetExtent = null;
                // If the extents (compared in same projection) interesect, set the extent of the new map to the projected extent
                if (targetInitialExtent != null)
                {
                    if (args.Extent.Intersects(targetInitialExtent))
                    {
                        if (isFullyContainedWithin(args.Extent, targetInitialExtent))
                            targetExtent = targetFullExtent; // if the full extent of the new service is fully within, automatically zoom to it
                        else
                            targetExtent = args.Extent;
                    }
                    else
                        targetExtent = targetInitialExtent;
                }
                else if (targetFullExtent != null)
                {
                    if (args.Extent.Intersects(targetFullExtent))
                    {
                        if (isFullyContainedWithin(args.Extent, targetFullExtent))
                            targetExtent = targetFullExtent; // if the full extent of the new service is fully within, automatically zoom to it
                        else
                            targetExtent = args.Extent;
                    }
                    else
                        targetExtent = targetFullExtent;
                }

                // else don't set an extent
                // the map will default to the full extent of the service

                // Since map will not be in a different projection, we have to re-create the map
                BaseMapInfo targetBaseMapInfo = baseMapInfo;
                IBaseMapDataSource dataSoure = DataSourceProvider.CreateDataSourceForBaseMapType(targetBaseMapInfo.BaseMapType) as IBaseMapDataSource;
                if (dataSoure == null)
                    throw new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionDatasourceNotLoadedForBaseMapType, targetBaseMapInfo.BaseMapType.ToString()));
                TiledMapServiceLayer layer = dataSoure.CreateBaseMapLayer(targetBaseMapInfo);
                layer.SetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty, true);
                layer.SetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty, targetBaseMapInfo.DisplayName);
                checkAndEnsureBingMapsTokenIfRequired(layer);

                // Save current selected layer
                Layer currentSelectedLayer = SelectedLayer;

                saveGraphicsInViewForSelectedLayerInAttributeDisplay();

                // Disable listening for layer changed events because we are re-adding layers to the collection. The initialization events are not fired nor is symbology changed
                Map.Layers.CollectionChanged -= Layers_CollectionChanged;

                List<Layer> oldBaseMapLayers = new List<Layer>();
                foreach (Layer l in Map.Layers)
                {
                    if ((bool)l.GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty))
                        oldBaseMapLayers.Add(l);
                }

                switchBaseMapLayer(layer, targetExtent, oldBaseMapLayers);

                // Re-Enable listening for layer changed events
                Map.Layers.CollectionChanged += Layers_CollectionChanged;

                // Restore current selected layer
                SelectedLayer = currentSelectedLayer;

                restoreGraphicsInViewForSelectedLayerInAttributeDisplay();

                OnNewMapCreated(new MapRecreatedEventArgs() { NewMap = Map });
                OnBaseMapChangeComplete(EventArgs.Empty);
            };
            geomHelper.ProjectExtent(Map.Extent, targetServiceSpatialReference);
        }

        #region Save and Restore graphics within view for selected layer
        // Workaround:- When we switch base maps layers, the layers do no re-initialize
        // It merely re-draws. This is problematic since we rely on hit-testing to find graphics within the view
        // This graphics would not have re-drawn with the code flow.
        // Hence we save the graphics within the view for the selected (graphics) layer
        // And restore after the drawing is complete.
        IEnumerable<Graphic> graphicsInViewForSelectedLayer;
        private void saveGraphicsInViewForSelectedLayerInAttributeDisplay()
        {
            if (AttributeDisplay != null && AttributeDisplay.FilterFeaturesByMapExtent && SelectedLayer is GraphicsLayer)
            {
                graphicsInViewForSelectedLayer = AttributeDisplay.GetGraphicsInViewForSelectedLayer();
            }
        }

        private void restoreGraphicsInViewForSelectedLayerInAttributeDisplay()
        {
            GraphicsLayer selectedGraphicsLayer = SelectedLayer as GraphicsLayer;
            if (selectedGraphicsLayer != null && AttributeDisplay != null && AttributeDisplay.FilterFeaturesByMapExtent && selectedGraphicsLayer != null)
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    AttributeDisplay.GraphicsLayer = selectedGraphicsLayer;
                    AttributeDisplay.SetGraphicsInViewForSelectedLayer(graphicsInViewForSelectedLayer);
                });
            }
        }
        #endregion

        private bool isFullyContainedWithin(Envelope outerExtent, Envelope innerExtent)
        {
            if (outerExtent == null || innerExtent == null)
                return false;
            return outerExtent.XMin <= innerExtent.XMin
                    && outerExtent.XMax >= innerExtent.XMax
                    && outerExtent.YMin <= innerExtent.YMin
                    && outerExtent.YMax >= innerExtent.YMax;
        }

        private void checkAndEnsureBingMapsTokenIfRequired(Layer layer)
        {
            TileLayer bingLayer = layer as TileLayer;
            if (bingLayer != null)
            {
                if (string.IsNullOrEmpty(ConfigurationStore.BingMapsAppId))
                    throw new Exception(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionNoBingMapsTokenAvailable);
                bingLayer.Token = ConfigurationStore.BingMapsAppId;
                ESRI.ArcGIS.Mapping.Core.LayerExtensions.SetUsesBingAppID(bingLayer, true);
            }
        }

        private void reCreateBaseMapLayer(BaseMapInfo targetBaseMapInfo, Envelope newMapExtent)
        {
            IBaseMapDataSource dataSource = DataSourceProvider.CreateDataSourceForBaseMapType(targetBaseMapInfo) as IBaseMapDataSource;
            if (dataSource == null)
                throw new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ExceptionUnableToRetrieveDatasource, targetBaseMapInfo.BaseMapType.ToString()));
            List<Layer> oldBaseMapLayers = new List<Layer>();
            foreach (Layer layer in Map.Layers)
            {
                if ((bool)layer.GetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty))
                    oldBaseMapLayers.Add(layer);
            }
            if (oldBaseMapLayers.Count == 1 && dataSource.CanSwitchBaseMapLayer(oldBaseMapLayers[0]))
            {
                dataSource.SwitchBaseMapLayer(oldBaseMapLayers[0], targetBaseMapInfo);

                OnBaseMapChangeComplete(EventArgs.Empty);
            }
            else
            {
                TiledMapServiceLayer layer = dataSource.CreateBaseMapLayer(targetBaseMapInfo);
                layer.SetValue(ESRI.ArcGIS.Client.WebMap.Document.IsBaseMapProperty, true);
                layer.SetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty, targetBaseMapInfo.DisplayName);
                checkAndEnsureBingMapsTokenIfRequired(layer);

                // Save current selected layer
                Layer currentSelectedLayer = SelectedLayer;

                saveGraphicsInViewForSelectedLayerInAttributeDisplay();

                // Disable listening for layer changed events because we are re-adding layers to the collection. The initialization events are not fired nor is symbology changed
                Map.Layers.CollectionChanged -= Layers_CollectionChanged;

                switchBaseMapLayer(layer, newMapExtent, oldBaseMapLayers);

                // Re-Enable listening for layer changed events
                Map.Layers.CollectionChanged += Layers_CollectionChanged;

                if (Map.Layers.Count > 1 && Map.Layers.Contains(currentSelectedLayer))
                {
                    // Restore current selected layer
                    SelectedLayer = currentSelectedLayer;
                }
                else
                {
                    SelectedLayer = layer;
                }

                restoreGraphicsInViewForSelectedLayerInAttributeDisplay();

                OnNewMapCreated(new MapRecreatedEventArgs() { NewMap = Map });
                OnBaseMapChangeComplete(EventArgs.Empty);
            }
        }

        private void switchBaseMapLayer(TiledMapServiceLayer baseMapLayer, Envelope newExtent, List<Layer> oldBasemapLayers)
        {
            if (Map == null)
                return;

            // 1. Save the operational layers (We assume a single base layer)
            System.Collections.Generic.Stack<Layer> layers = new System.Collections.Generic.Stack<Layer>();
            for (int i = Map.Layers.Count - 1; i >= 0; i--)
            {
                Layer l = Map.Layers[i];
                if (oldBasemapLayers.Contains(l))
                    continue;

                Map.Layers.RemoveAt(i);
                layers.Push(l);
            }

            // 2. Clear the layers collection
            Map.Layers.Clear();

            // 3. Set the extent
            bool spatialReferenceUnchanged = Map.SpatialReference.Equals(newExtent.SpatialReference);
            Map.Extent = newExtent;

            // 4a. Set layer id if this is not set
            if (string.IsNullOrEmpty(baseMapLayer.ID) || (!string.IsNullOrEmpty(baseMapLayer.ID) && Map.Layers[baseMapLayer.ID] != null))
                baseMapLayer.ID = Guid.NewGuid().ToString("N");

            // 4. Add the new base map
            Map.Layers.Add(baseMapLayer);

            // 5. Re-add the operational layers         
            while (layers.Count > 0)
            {
                Layer layer = layers.Pop();
                if (!spatialReferenceUnchanged)
                {
                    //reproject graphics layers that are not feature layers 
                    // Feature layers support reprojection
                    if (layer is GraphicsLayer && !(layer is FeatureLayer))
                    {
                        GraphicsLayer graphicsLayer = layer as GraphicsLayer;
                        if (graphicsLayer.Graphics.Count > 0)
                        {
                            GeometryServiceOperationHelper helper = new GeometryServiceOperationHelper(
                                                                                                        new ConfigurationStoreHelper().GetGeometryServiceUrl(ConfigurationStore));
                            helper.ProjectGraphicsCompleted += (o, e) =>
                            {
                                GraphicsLayer targetLayer = e.UserState as GraphicsLayer;
                                if (targetLayer != null)
                                {
                                    targetLayer.Graphics.Clear();
                                    foreach (Graphic graphic in e.Graphics)
                                        targetLayer.Graphics.Add(graphic);
                                }
                            };
                            helper.ProjectGraphics(graphicsLayer.Graphics, newExtent.SpatialReference, graphicsLayer);
                        }

                        // update the map spatial reference on custom layers
                        ICustomGraphicsLayer customGraphicsLayer = layer as ICustomGraphicsLayer;
                        if (customGraphicsLayer != null)
                            customGraphicsLayer.MapSpatialReference = Map.SpatialReference;
                        else
                        {
                            HeatMapLayerBase heatMapLayer = layer as HeatMapLayerBase;
                            if (heatMapLayer != null)
                                heatMapLayer.MapSpatialReference = Map.SpatialReference;
                        }
                    }
                    else
                    {
                        HeatMapLayerBase heatMapLayer = layer as HeatMapLayerBase;
                        if (heatMapLayer != null && heatMapLayer.HeatMapPoints.Count > 0)
                        {
                            GeometryServiceOperationHelper helper = new GeometryServiceOperationHelper(new ConfigurationStoreHelper().GetGeometryServiceUrl(ConfigurationStore));
                            helper.ProjectPointsCompleted += (o, e) =>
                            {
                                PointCollection points = new PointCollection();

                                foreach (MapPoint item in e.Points)
                                {
                                    if (item != null)
                                        points.Add(item);
                                }

                                heatMapLayer.HeatMapPoints = points;
                                heatMapLayer.MapSpatialReference = points[0].SpatialReference;
                                heatMapLayer.Refresh();
                            };
                            helper.ProjectPoints(heatMapLayer.HeatMapPoints, newExtent.SpatialReference);
                        }
                    }
                }
                Map.Layers.Add(layer);
            }
        }

        protected virtual void OnNewMapCreated(MapRecreatedEventArgs args)
        {
            //ensureMapCenterInit();
            OnMapRecreated(args);
        }

        private void associateBehaviorsWithMap(Map map)
        {
            if (map == null)
                return;
            BehaviorCollection behaviors = Interaction.GetBehaviors(map);
            if (behaviors == null)
                return;
            behaviors.Clear();
            behaviors.Add(new PositionMapTip() { Margin = 15 });
            behaviors.Add(new DelayMapTipHide() { HideDelay = TimeSpan.FromSeconds(0.5) });
            behaviors.Add(new LayerAutoUpdateBehavior());
            behaviors.Add(new ClearEditOnClickBehavior());
            if (ExtensionBehaviors != null)
            { 
                foreach (ExtensionBehavior extensionBehavior in ExtensionBehaviors)
                {
                    if (extensionBehavior == null) continue;

                    Behavior<Map> mapBehavior = null;
                    mapBehavior = extensionBehavior.MapBehavior;
                    if (mapBehavior == null)
                        mapBehavior = getMapBehaviorForExtensionBehavior(extensionBehavior);
                    if (mapBehavior == null)
                        continue;
                    extensionBehavior.MapBehavior = mapBehavior;

                    ISupportsConfiguration supportsConfiguration = mapBehavior as ISupportsConfiguration;
                    if (supportsConfiguration != null)
                    {
                        string configData = null;
                        if (ExtensionsDataManager != null)
                        {
                            string instanceConfigData = ExtensionsDataManager.GetExtensionDataForExtension(extensionBehavior.BehaviorId);
                            if (instanceConfigData != null)
                                configData = instanceConfigData;
                        }
                        try
                        {
                            supportsConfiguration.LoadConfiguration(configData);
                        }
                        catch (Exception ex)
                        {
                            MessageBoxDialog.Show(string.Format(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ErrorLoadConfigurationOfExtension, mapBehavior.GetType().FullName) + Environment.NewLine + ex.Message);
                            Logger.Instance.LogError(ex);
                        }
                    }
                    if (extensionBehavior.IsEnabled && !behaviors.Contains(mapBehavior))
                    {
                        try
                        {
                            if (mapBehavior is INotifyInitialized)
                            {
                                var notifyInit = (INotifyInitialized)mapBehavior;
                                if (!notifyInit.IsInitialzed && notifyInit.InitializationError == null)
                                {
                                    m_behaviorsPendingInitCount++;

                                    EventHandler initHandler = null;
                                    initHandler = (o, e) =>
                                    {
                                        var n = (INotifyInitialized)o;
                                        n.Initialized -= initHandler;
                                        n.InitializationFailed -= initHandler;
                                        m_behaviorsPendingInitCount--;

                                        if (n.InitializationError != null)
                                            Logger.Instance.LogError(n.InitializationError);

                                        OnInitialized();
                                    };

                                    notifyInit.Initialized += initHandler;
                                    notifyInit.InitializationFailed += initHandler;
                                }
                                else if (notifyInit.InitializationError != null)
                                {
                                    Logger.Instance.LogError(notifyInit.InitializationError);
                                }
                            }

                            behaviors.Add(mapBehavior);
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.LogError(ex);
                        }
                    }
                }
            }
        }

        public Behavior<Client.Map> getMapBehaviorForExtensionBehavior(ExtensionBehavior extensionBehavior)
        {
            if (ExtensionMapBehaviors == null || extensionBehavior == null || string.IsNullOrEmpty(extensionBehavior.CommandValueId))
                return null;

            if (extensionBehavior.MapBehavior != null)
                return extensionBehavior.MapBehavior;

            string[] splits = extensionBehavior.CommandValueId.Split(new char[] { ';' });
            if (splits == null || splits.Length < 2)
                return null;

            string typeName = splits[0].Replace("Type=", "");
            string assemblyName = splits[1].Replace("Assembly=", "");
            if (!assemblyName.Contains("Version="))
                assemblyName = this.GetType().Assembly.FullName.Replace("ESRI.ArcGIS.Mapping.Controls", assemblyName);

            foreach (Behavior<Map> mapBehavior in ExtensionMapBehaviors)
            {
                if (mapBehavior == null)
                    continue;
                Type type = mapBehavior.GetType();
                if (type.FullName == typeName && type.Assembly.FullName == assemblyName)
                {
                    return type.Assembly.CreateInstance(type.FullName) as Behavior<Map>;
                }
            }
            Type t = Type.GetType(string.Format("{0},{1}", typeName, assemblyName));
            if(t != null)
                return t.Assembly.CreateInstance(t.FullName) as Behavior<Map>;
            return null;
        }
        #endregion

        #region TODO: Make extensible for custom layers
        const string esriSharePointNamespace = "clr-namespace:ESRI.ArcGIS.Mapping.SharePoint.Client.Core;assembly=ESRI.ArcGIS.Mapping.SharePoint.Client.Core";
        const string esriSharePointNamespacePrefix = "esriSharePoint";
        public string GetMapConfiguration(bool useSharePoint)
        {
            // Only return map configuration if the web map is not linked
            if (ViewerApplication.WebMapSettings.Linked != true)
            {
            Dictionary<string, string> customNamespaces = new Dictionary<string, string>();
            customNamespaces.Add(esriSharePointNamespacePrefix, esriSharePointNamespace);
            return GetMapConfiguration(customNamespaces);
        }
            else
            {
                return string.Empty;
            }
        }
        #endregion

        public string GetMapConfiguration(Dictionary<string, string> customNamespaces = null)
        {
            // Only return map configuration if the web map is not linked
            if (ViewerApplication.WebMapSettings.Linked != true)
            {
            MapXamlWriter mapXamlWriter = new MapXamlWriter(true);
            if (customNamespaces != null)
            {
                foreach (KeyValuePair<string, string> customNamespace in customNamespaces)
                {
                    mapXamlWriter.Namespaces.Add(customNamespace.Key, customNamespace.Value);
                }
            }
                return mapXamlWriter.MapToXaml(Map);
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetSelectedLayerConfiguration(Dictionary<string, string> customNamespaces)
        {
            if (SelectedLayer == null)
                return null;
            Dictionary<string, string> Namespaces = new Dictionary<string, string>();
            Namespaces.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            Namespaces.Add(Constants.esriPrefix, Constants.esriNamespace);
            Namespaces.Add(Constants.sysPrefix, Constants.sysNamespace);
            Namespaces.Add("esriBing", "clr-namespace:ESRI.ArcGIS.Client.Bing;assembly=ESRI.ArcGIS.Client.Bing");
            Namespaces.Add(Constants.esriMappingPrefix, Constants.esriMappingNamespace);
            Namespaces.Add(Constants.esriFSSymbolsPrefix, Constants.esriFSSymbolsNamespace);
			Namespaces.Add(Constants.esriExtensibilityPrefix, Constants.esriExtensibilityNamespace);
            if (customNamespaces != null)
            {
                foreach (KeyValuePair<string, string> customNamespace in customNamespaces)
                {
                    Namespaces.Add(customNamespace.Key, customNamespace.Value);
                }
            }

            StringBuilder xaml = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(xaml, new XmlWriterSettings() { OmitXmlDeclaration = true });
            writer.WriteStartElement("ContentControl");

            // write namespaces
            foreach (string key in Namespaces.Keys)
            {
                string _namespace = "http://schemas.microsoft.com/winfx/2006/xaml"; // default
                if (Namespaces.ContainsKey(key))
                    _namespace = Namespaces[key];
                writer.WriteAttributeString("xmlns", key, null, _namespace);
            }

            ICustomLayer customlayer = SelectedLayer as ICustomLayer;
            if (customlayer != null)
            {
                customlayer.Serialize(writer, Namespaces);
            }
            else
            {
                LayerXamlWriter layerXamlWriter = XamlWriterFactory.CreateLayerXamlWriter(SelectedLayer, writer, Namespaces);
                if (layerXamlWriter == null)
                    throw new NotSupportedException(SelectedLayer.GetType().FullName);
                layerXamlWriter.WriteLayer(SelectedLayer, SelectedLayer.GetType().Name, Constants.esriNamespace);
            }

            writer.WriteEndElement();

            writer.Flush();
            writer = null;
            string config = xaml.ToString();
            // Inject default namespace
            config = config.Insert(16, "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" ");
            return config;
        }

        protected virtual void OnMapLayersInitialized(EventArgs args)
        {
            if (MapLayersInitialized != null)
                MapLayersInitialized(this, args);

            m_layersInitialized = true;

            // Fire MapApplication's Initialized event
            OnInitialized(); 
        }

        protected virtual void OnConfigurationLoaded(EventArgs args)
        {
            if (ConfigurationLoaded != null)
                ConfigurationLoaded(this, args);
        }

        protected virtual void OnCustomBaseMapChanged(BaseMapChangedEventArgs args)
        {
            if (CustomBaseMapChanged != null)
                CustomBaseMapChanged(this, args);
        }

        protected virtual void OnBaseMapChangeComplete(EventArgs args)
        {
            if (BaseMapChangeComplete != null)
                BaseMapChangeComplete(this, args);
        }

        protected virtual void OnBaseMapChangeFailed(ExceptionEventArgs args)
        {
            Logger.Instance.LogError(args.Exception);
            if (BaseMapChangeFailed != null)
                BaseMapChangeFailed(this, args);
        }

        protected virtual void OnConfigurationLoadFailed(ExceptionEventArgs args)
        {
            if (ConfigurationLoadFailed != null)
                ConfigurationLoadFailed(this, args);

            OnInitializationFailed(args);
        }

        protected virtual void OnConfigurationStoreLoadFailed(ExceptionEventArgs args)
        {
            if (ConfigurationStoreLoadFailed != null)
                ConfigurationStoreLoadFailed(this, args);

            OnInitializationFailed(args);
        }

        protected virtual void OnMapRecreated(MapRecreatedEventArgs args)
        {
            if (MapRecreated != null)
                MapRecreated(this, args);
        }

        protected virtual void OnLayerSelectionChanged(LayerEventArgs e)
        {
            if (SelectedLayerChanged != null)
                SelectedLayerChanged(this, EventArgs.Empty);
        }

        protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        public event EventHandler<ExceptionEventArgs> ConfigurationLoadFailed;
        public event EventHandler<ExceptionEventArgs> ConfigurationStoreLoadFailed;
        public event EventHandler ConfigurationLoaded;
        public event EventHandler<MapRecreatedEventArgs> MapRecreated;
        public event EventHandler<ExceptionEventArgs> BaseMapChangeFailed;
        public event EventHandler BaseMapChangeComplete;
        public event EventHandler MapLayersInitialized;
        public event EventHandler<BaseMapChangedEventArgs> CustomBaseMapChanged;
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
        public event EventHandler SelectedLayerChanged;
        public event EventHandler LinkedToWebMap;

        #region IMapApplication Members

        public object ShowWindow(string windowTitle, FrameworkElement windowContents, bool isModal, 
            EventHandler<System.ComponentModel.CancelEventArgs> onHidingHandler, 
            EventHandler onHideHandler, WindowType windowType = WindowType.Floating,
            double? top = null, double? left = null)
        {
            return WindowManager.ShowWindow(windowTitle, windowContents, isModal, onHidingHandler, onHideHandler, 
                windowType, top, left);
        }

        public void HideWindow(FrameworkElement windowContents)
        {
            WindowManager.HideWindow(windowContents);
        }

        public DependencyObject FindObjectInLayout(string controlName)
        {
            FrameworkElement elem = this.Content as FrameworkElement;
            if (elem == null)
                return null;
            return elem.FindName(controlName) as DependencyObject;
        }

        public Uri ResolveUrl(string urlToBeResolved)
        {
            return ImageUrlResolver.ResolveUrl(urlToBeResolved);
        }

        public void LoadMap(Map map, Action<LoadMapCompletedEventArgs> callback = null, object userToken = null)
        {
            try
            {
                loadMap(map);
            }
            catch (Exception ex)
            {
                if (callback != null)
                    callback(new LoadMapCompletedEventArgs() { Error = ex, UserState = userToken });
            }
            if (callback != null)
                callback(new LoadMapCompletedEventArgs() { UserState = userToken });
        }

        private static bool loadingWebMap = false;
        private Action<GetMapCompletedEventArgs> loadWebMapCallback = null;
        private string loadWebMapID = null;
        public void LoadWebMap(string id, ESRI.ArcGIS.Client.WebMap.Document document = null, 
            Action<GetMapCompletedEventArgs> callback = null, object userToken = null)
        {
            attemptingReload = false;
            loadWebMap(id, document, callback, userToken);
        }

        private void loadWebMap(string id, ESRI.ArcGIS.Client.WebMap.Document document,
            Action<GetMapCompletedEventArgs> callback, object userToken)
        {
            if (loadingWebMap)
                return;

            loadingWebMap = true;
            if (document == null)
                document = new Client.WebMap.Document() { GeometryServiceUrl = GeometryServiceUrl };
            else
                document.GetMapCompleted -= LoadWebMap_GetMapCompleted;

            loadWebMapCallback = callback;
            loadWebMapID = id;

            document.GetMapCompleted += LoadWebMap_GetMapCompleted;
            document.GetMapAsync(id, userToken);
        }

        private SignInToAGSOLCommand _signInCommand;
        bool attemptingReload;
        private void LoadWebMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            ESRI.ArcGIS.Client.WebMap.Document doc = (ESRI.ArcGIS.Client.WebMap.Document)sender;

            if (e.Error == null && e.Map != null)
            {
                attemptingReload = false;
                Map.InitializeFromWebMap(e);
                Map.GetMapUnitsAsync(SetScaleBarMapUnit, getMapUnitsFailed);

                doc.GetItemCompleted -= LoadWebMap_GetItemCompleted;
                doc.GetItemCompleted += LoadWebMap_GetItemCompleted;
                doc.GetItemAsync(loadWebMapID, e);
            }
            else
            {
                loadingWebMap = false;
                if (attemptingReload)
                {
                    attemptingReload = false;
                    loadWebMapCallback(e);
                    return;
                }
                initializeAgolFromDocument(doc, (agol) =>
                    {
                            // Attempt to sign in from isolated storage
                            AGOLUser user = new AGOLUser(agol);
                            user.SignInFromLocalStorage((o, a) =>
                            {
                                if (user.Token != null)
                                {
                                    // Sign in was successful - try loading web map again
                                    doc.Token = user.Token;
                                    attemptingReload = true;
                                    loadWebMap(loadWebMapID, doc, loadWebMapCallback, e.UserState);
                                }
                                else
                                {
                                    // Try signing into ArcGIS Online/Portal
                                    if (_signInCommand == null)
                                    {
                                        // Initialize sign-on command
                                        _signInCommand = new SignInToAGSOLCommand();
                                        _signInCommand.SignedIn += (o2, a2) =>
                                        {
                                            doc.Token = _signInCommand.ArcGISOnline.User.Token;
                                            attemptingReload = true;
                                            loadWebMap(loadWebMapID, doc, loadWebMapCallback, e.UserState);
                                        };

                                        _signInCommand.Cancelled += (o2, a2) =>
                                        {
                                            if (loadWebMapCallback != null)
                                                loadWebMapCallback(e);
                                        };
                                    }

                                    _signInCommand.ArcGISOnline = agol;
                                    _signInCommand.Execute(null);
                                }
                            });
                    });
            }
        }

        private int _getItemRetryCount = 0;
        private void LoadWebMap_GetItemCompleted(object sender, GetItemCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // Call to GetItemAsync seems to arbitrarily throw a WebRequest error even though the previous
                // GetMapAsync call completed successfully.  The issue appears to happen more frequently as
                // more web maps are opened within the same session.  
                if (_getItemRetryCount < 3)
                {
                    ESRI.ArcGIS.Client.WebMap.Document doc = (ESRI.ArcGIS.Client.WebMap.Document)sender;
                    _getItemRetryCount++;

                    Dispatcher.BeginInvoke(() =>
                    {
                        doc.GetItemAsync(loadWebMapID, e.UserState);
                    });
                }
                else
                {
                    // Reset retry counter
                    _getItemRetryCount = 0;

                    // Establish link to web map without ItemInfo
                    ViewerApplication.WebMapSettings.ID = loadWebMapID;
                    ViewerApplication.WebMapSettings.Document = (ESRI.ArcGIS.Client.WebMap.Document)sender;
                    ViewerApplication.WebMapSettings.Linked = true;

                    loadingWebMap = false;

                    if (loadWebMapCallback != null)
                        loadWebMapCallback(e.UserState as GetMapCompletedEventArgs);
                }
            }
            else
            {
                if (_getItemRetryCount > 0)
                    _getItemRetryCount = 0; // Reset counter

                // update web map settings
                ViewerApplication.WebMapSettings.ItemInfo = e.ItemInfo;
                ViewerApplication.WebMapSettings.ID = e.ItemInfo.ID;
                ViewerApplication.WebMapSettings.Document = (ESRI.ArcGIS.Client.WebMap.Document)sender;

                // Default the map to be linked to the web map
                ViewerApplication.WebMapSettings.Linked = true;

                loadingWebMap = false;

                if (loadWebMapCallback != null)
                    loadWebMapCallback(e.UserState as GetMapCompletedEventArgs);
            }
        }

        private int _getMapUnitsRetryCount = 0;
        private void getMapUnitsFailed(object sender, ExceptionEventArgs args)
        {
            if (_getMapUnitsRetryCount < 3)
            {
                _getMapUnitsRetryCount++;
                Map map = (Map)sender;
                map.GetMapUnitsAsync(SetScaleBarMapUnit, getMapUnitsFailed);
            }
            else
            {
                _getMapUnitsRetryCount = 0;
            }
        }

        public OnClickPopupInfo GetPopup(Graphic graphic, Layer layer, int? layerId = null)
        {
            return PopupHelper.GetPopup(new[] {graphic}, layer, layerId);
        }

        public void ShowPopup(Graphic graphic, Layer layer, int? layerId = null)
        {
            PopupHelper.ShowPopup(graphic, layer, layerId);
        }

        public event EventHandler Initialized;

        private async void OnInitialized()
        {
            if (m_initializeTimeoutTimer == null)
            {
                m_initializeTimeoutTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(30) };
                EventHandler onInitTimeout = null;
                onInitTimeout = (o, e) =>
                {
                    m_initializeTimeoutTimer.Stop();
                    m_initializeTimeoutTimer.Tick -= onInitTimeout;

                    if (ComponentsInitialized)
                    {
                        // Components already initialized.  Do nothing.
                        return;
                    }
                    else
                    {
                        // Initialization timeout has expired.  Force Initialized event to fire.
                        m_initializationTimedOut = true;
                        OnInitialized();
                    }
                };
                m_initializeTimeoutTimer.Tick += onInitTimeout;
                m_initializeTimeoutTimer.Start();
            }

            if (Initialized != null && (m_initializationTimedOut || ComponentsInitialized))
            {
                await TaskEx.Delay(500); // Insert a delay to allow a rendering pass
                Initialized(this as IMapApplication, EventArgs.Empty);
            }
        }

        public event EventHandler InitializationFailed;

        private void OnInitializationFailed(ExceptionEventArgs e)
        {
            if (InitializationFailed != null)
                InitializationFailed(this as IMapApplication, e);

            Logger.Instance.LogError(e.Exception);
        }

        #endregion

        #region Url Storage
        public string GeometryServiceUrl
        {
            get
            {
                return Urls.GeometryServiceUrl;
            }
        }

        public void SetGeometryServiceUrl(string value)
        {
            GeometryServiceInfo foundItem = null;
                if (ConfigurationStore.GeometryServices != null &&
                                           ConfigurationStore.GeometryServices.Count > 0)
                {
                    foreach (GeometryServiceInfo item in ConfigurationStore.GeometryServices)
                    {
                        if (item.Url == Urls.GeometryServiceUrl)
                        {
                            if (value != null)
                                item.Url = value;
                            foundItem = item;
                        }
                    }
                }
                if (foundItem == null && value != null)
                {
                    if (ConfigurationStore.GeometryServices == null)
                        ConfigurationStore.GeometryServices = new List<GeometryServiceInfo>();
                    ConfigurationStore.GeometryServices.Add(new GeometryServiceInfo() { Url = value });
                }
                else if (value == null)
                    ConfigurationStore.GeometryServices.Remove(foundItem);
                Urls.GeometryServiceUrl = value;
                string geometryService = value == null ? string.Empty : value;
                #region Update on ArcGISOnlineEnvironment
                if (ArcGISOnlineEnvironment.ConfigurationUrls != null)
                    ArcGISOnlineEnvironment.ConfigurationUrls.GeometryServer = geometryService;
                #endregion
                if (ViewerApplicationControl.Instance != null && ViewerApplicationControl.Instance.ViewerApplication != null)
                    ViewerApplicationControl.Instance.ViewerApplication.GeometryService = geometryService;
        }

        public string BaseUrl
        {
            get
            {
                return Urls.BaseUrl;
            }
            set
            {
                Urls.BaseUrl = value;
            }
        }

        public bool IsEditMode { get; set; }
        public string ArcGISOnlineConfigurationFileUrl { get; set; }
        public string PrintPreviewDocumentLibraryUrl { get; set; }
        public string DefaultProxyUrl { get; set; }

        public string ProxyUrl
        {
            get { return Urls.ProxyUrl; }
            set
            {
                if (value != Urls.ProxyUrl)
                {
                    Urls.ProxyUrl = value;
                    OnProxyUrlChange(this, EventArgs.Empty);
                    #region Update Proxy on layers
                    SecureServicesHelper.UpdateProxyUrl(Urls.ProxyUrl);
                    #endregion
                }
            }
        }

        public event EventHandler ProxyUrlChanged;

        protected void OnProxyUrlChange(object sender, EventArgs args)
        {
            if (ProxyUrlChanged != null)
                ProxyUrlChanged(sender, args);
        }

        ApplicationUrls urls;
        public ApplicationUrls Urls
        {
            get
            {
                return urls;
            }
        }

        /// <summary>
        /// Gets the ArcGIS portal endpoint used by the application
        /// </summary>
        public ArcGISPortal Portal
        {
            get { return GetValue(PortalProperty) as ArcGISPortal; }
            set { SetValue(PortalProperty, value); }
        }

        /// <summary>
        /// Identifies the Portal dependency property.
        /// </summary>
        public static readonly DependencyProperty PortalProperty =
                DependencyProperty.Register(
                        "Portal",
                        typeof(ArcGISPortal),
                        typeof(View),
                        new PropertyMetadata(OnPortalPropertyChanged));

        public static void OnPortalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ArcGISPortal portal = e.NewValue as ArcGISPortal;

            if (MapApplication.Current != null)
                MapApplication.Current.SetValue(MapApplication.PortalProperty, portal);

            if (portal != null && !portal.IsInitialized && !string.IsNullOrEmpty(portal.Url))
                portal.InitializeAsync(portal.Url, null);
        }

        public string ArcGISOnlineSharing
        {
            get { return (string)GetValue(ArcGISOnlineSharingProperty); }
            set { SetValue(ArcGISOnlineSharingProperty, value); }
        }

        public static readonly DependencyProperty ArcGISOnlineSharingProperty =
            DependencyProperty.Register("ArcGISOnlineSharing", typeof(string), typeof(View), new PropertyMetadata(OnAgsOlSharingChange));
        static void OnAgsOlSharingChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            View view = o as View;
            if (view != null)
            {
                if (ViewerApplicationControl.Instance != null && ViewerApplicationControl.Instance.ViewerApplication != null)
                    ViewerApplicationControl.Instance.ViewerApplication.ArcGISOnlineSharing = view.ArcGISOnlineSharing;
                if (ArcGISOnlineEnvironment.ConfigurationUrls != null)
                {
                    ArcGISOnlineEnvironment.ConfigurationUrls.Sharing = view.ArcGISOnlineSharing;
                    if (ArcGISOnlineEnvironment.ArcGISOnline != null)
                        ArcGISOnlineEnvironment.ArcGISOnline.Initialize(ArcGISOnlineEnvironment.ConfigurationUrls.Sharing, ArcGISOnlineEnvironment.ConfigurationUrls.Secure);
                }
            }
        }

        public string ArcGISOnlineSecure
        {
            get { return (string)GetValue(ArcGISOnlineSecureProperty); }
            set { SetValue(ArcGISOnlineSecureProperty, value); }
        }

        public static readonly DependencyProperty ArcGISOnlineSecureProperty =
            DependencyProperty.Register("ArcGISOnlineSecure", typeof(string), typeof(View), new PropertyMetadata(OnAgsOlSecureChange));
        static void OnAgsOlSecureChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            View view = o as View;
            if (view != null)
            {
                if (ViewerApplicationControl.Instance != null && ViewerApplicationControl.Instance.ViewerApplication != null)
                    ViewerApplicationControl.Instance.ViewerApplication.ArcGISOnlineSecure = view.ArcGISOnlineSecure;
                if (ArcGISOnlineEnvironment.ConfigurationUrls != null)
                {
                    ArcGISOnlineEnvironment.ConfigurationUrls.Secure = view.ArcGISOnlineSecure;
                    if (ArcGISOnlineEnvironment.ArcGISOnline != null)
                        ArcGISOnlineEnvironment.ArcGISOnline.Initialize(ArcGISOnlineEnvironment.ConfigurationUrls.Sharing, ArcGISOnlineEnvironment.ConfigurationUrls.Secure);
                }
            }
        }

        public string ArcGISOnlineProxy
        {
            get { return (string)GetValue(ArcGISOnlineProxyProperty); }
            set { SetValue(ArcGISOnlineProxyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ArcGISOnlineProxy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ArcGISOnlineProxyProperty =
            DependencyProperty.Register("ArcGISOnlineProxy", typeof(string), typeof(View), new PropertyMetadata(OnAgsOlProxyChange));
        static void OnAgsOlProxyChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            View view = o as View;
            if (view != null)
            {
                if (ViewerApplicationControl.Instance != null && ViewerApplicationControl.Instance.ViewerApplication != null)
                    ViewerApplicationControl.Instance.ViewerApplication.ArcGISOnlineProxy = view.ArcGISOnlineProxy;
                if (ArcGISOnlineEnvironment.ConfigurationUrls != null)
                {
                    ArcGISOnlineEnvironment.ConfigurationUrls.ProxyServer = view.ArcGISOnlineProxy;
                    ArcGISOnlineEnvironment.ConfigurationUrls.ProxyServerEncoded = view.ArcGISOnlineProxy;
                }
            }
        }
        #endregion
    }

    public class MapRecreatedEventArgs : EventArgs
    {
        public Map NewMap { get; set; }
    }

    public class LayerEventArgs : EventArgs
    {
        public Layer Layer { get; set; }
    }
}
