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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.DataSources;
using ESRI.ArcGIS.Mapping.DataSources;
using core = ESRI.ArcGIS.Mapping.Core;
using System.Linq;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Mapping.DataSources.ArcGISServer;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Collections;
using ESRI.ArcGIS.Mapping.Windowing;
using ESRI.ArcGIS.Mapping.Controls.Resources;
using ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "btnGo", Type = typeof(Button))]
    [TemplatePart(Name = "btnOk", Type = typeof(Button))]
    [TemplatePart(Name = "treeResources", Type = typeof(TreeView))]
    [TemplatePart(Name = "txtUrl", Type = typeof(WatermarkedTextBox))]
    [TemplatePart(Name = "DropDownToggle", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "ProgressIndicatorSection", Type = typeof(UIElement))]
    [TemplatePart(Name = "BusyProgressIndicator", Type = typeof(ActivityIndicator))]
    [TemplatePart(Name = "hlnkCancelButton", Type = typeof(HyperlinkButton))]
    [TemplatePart(Name = "ConnectionsPopupContent", Type = typeof(ConnectionsDropDownPopupControl))]
    [TemplatePart(Name = "ConnectionsPopup", Type = typeof(Popup))]
    public class BrowseContentDialog : Control
    {
        private string _currentInputUrl; // Stores the URL as it was input into the UI
        private string _currentRestUrl; // Stores the REST URL that corresponds to the input URL

        internal TreeView treeResources { get; private set; }
        internal Button btnOk { get; private set; }
        internal Button btnGo { get; private set; }
        internal FrameworkElement LayoutRoot { get; private set; }
        internal WatermarkedTextBox txtUrl { get; private set; }
        internal ScrollViewer scrlContent { get; private set; }
        internal ToggleButton DropDownToggle { get; private set; }
        internal ConnectionsDropDownPopupControl ConnectionsDropDownPopupControl { get; private set; }
        internal FloatingWindow ParentWindow { get; set; }
        internal UIElement ProgressIndicatorSection { get; set; }
        internal ActivityIndicator BusyProgressIndicator { get; set; }
        internal HyperlinkButton hlnkCancelButton { get; set; }
        internal Popup ConnectionsDropDownPopup { get; set; }

        internal ObservableCollection<Connection> Connections = null;

        public BrowseContentDialog()
        {
            DefaultStyleKey = typeof(BrowseContentDialog);

            Connections = new ObservableCollection<Connection>();

            Filter = core.DataSources.Filter.SpatiallyEnabledResources |
                     core.DataSources.Filter.ImageServices |
                     core.DataSources.Filter.FeatureServices;
            OKButtonText = ESRI.ArcGIS.Mapping.Controls.Resources.Strings.OK;
        }

        #region Properties

        #region DataSourceProvider
        /// <summary>
        /// 
        /// </summary>
        public ESRI.ArcGIS.Mapping.DataSources.DataSourceProvider DataSourceProvider
        {
            get { return GetValue(DataSourceProviderProperty) as ESRI.ArcGIS.Mapping.DataSources.DataSourceProvider; }
            set { SetValue(DataSourceProviderProperty, value); }
        }

        /// <summary>
        /// Identifies the DataSourceProvider dependency property.
        /// </summary>
        public static readonly DependencyProperty DataSourceProviderProperty =
            DependencyProperty.Register(
                "DataSourceProvider",
                typeof(ESRI.ArcGIS.Mapping.DataSources.DataSourceProvider),
                typeof(BrowseContentDialog),
                new PropertyMetadata(new ESRI.ArcGIS.Mapping.DataSources.DataSourceProvider()));
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
                typeof(BrowseContentDialog),
                new PropertyMetadata(new ConnectionsProvider(), OnConnectionsProviderPropertyChanged));

        /// <summary>
        /// ConnectionsProviderProperty property changed handler.
        /// </summary>
        /// <param name="d">BrowseContentDialog that changed its ConnectionsProvider.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnConnectionsProviderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BrowseContentDialog source = d as BrowseContentDialog;
            source.onConnectionsProviderChanged();
        }

        private void onConnectionsProviderChanged()
        {
            retrieveConnections();
        }

        private void retrieveConnections()
        {
            if (ConnectionsProvider != null)
            {
                if (Connections == null || Connections.Count < 1)
                {
                    ConnectionsProvider.GetConnectionsCompleted += new EventHandler<GetConnectionsCompletedEventArgs>(ConnectionsProvider_ConnectionsLoaded);
                    ConnectionsProvider.GetConnectionsFailed += (o, args) =>
                    {
                        if (args != null && args.Exception != null)
                        {
                            Dispatcher.BeginInvoke((Action)delegate
                            {
                                //MessageBox.Show("There was an error retrieving connections.Please try again." + Environment.NewLine + args.Exception.Message);
                                Logger.Instance.LogError(args.Exception);
                            });
                        }
                    };
                    ConnectionsProvider.GetConnectionsAsync(null);
                }
            }
        }

        void ConnectionsProvider_ConnectionsLoaded(object sender, GetConnectionsCompletedEventArgs e)
        {
            Connections = new ObservableCollection<Connection>();
            if (e.Connections != null)
            {
                foreach (Connection conn in e.Connections)
                {
                    if (DataSourceProvider.ConnectionTypeSupportsFilter(conn.ConnectionType, Filter))
                        Connections.Add(conn);
                }
            }
            if (ConnectionsDropDownPopupControl != null)
            {
                ConnectionsDropDownPopupControl.Connections = Connections;
                if (ConnectionsDropDownPopup != null && ConnectionsDropDownPopup.IsOpen)
                    positionDropDownRelativeToTextBox();
            }
        }
        #endregion

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
                typeof(BrowseContentDialog),
                new PropertyMetadata(null, OnMapPropertyChanged));

        /// <summary>
        /// MapProperty property changed handler.
        /// </summary>
        /// <param name="d">BrowseContentDialog that changed its Map.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BrowseContentDialog source = d as BrowseContentDialog;
            Map value = e.NewValue as Map;
        }
        #endregion

        #region Filter
        private Filter filter;//for access from background threads to avoid cross thread access exceptions
        public Filter Filter
        {
            get { return (Filter)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Filter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(Filter), typeof(BrowseContentDialog), new PropertyMetadata(OnFilterPropertyChanged));

        private static void OnFilterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BrowseContentDialog source = d as BrowseContentDialog;
            source.filter = source.Filter;
        }

        #endregion

        #region OKButtonText
        public string OKButtonText
        {
            get { return (string)GetValue(OKButtonTextProperty); }
            set { SetValue(OKButtonTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OKButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OKButtonTextProperty =
            DependencyProperty.Register("OKButtonText", typeof(string), typeof(BrowseContentDialog), null);
        #endregion

        #region SelectedResourceUrl
        /// <summary>
        /// Backing dependency property for SelectedResourceUrl
        /// </summary>
        public static readonly DependencyProperty SelectedResourceUrlProperty = DependencyProperty.Register(
            "SelectedResourceUrlProperty", typeof(string), typeof(BrowseContentDialog), 
            new PropertyMetadata(null, OnSelectedResourceUrlPropertyChanged));

        /// <summary>
        /// Gets the URL of the currently selected resource
        /// </summary>
        public string SelectedResourceUrl
        {
            get { return GetValue(SelectedResourceUrlProperty) as string; }
            set { SetValue(SelectedResourceUrlProperty, value); }
        }

        private static void OnSelectedResourceUrlPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BrowseContentDialog b = d as BrowseContentDialog;
            if (b.treeResources != null && b.treeResources.Items.Count == 0 && b.txtUrl != null && 
            e.NewValue != null)
                b.txtUrl.Text = e.NewValue as string;

            if (b.SelectedResourceChanged != null)
                b.SelectedResourceChanged(b, EventArgs.Empty);
        }
        #endregion

        #region Restricted Services Properties
        public bool ShowRestrictedServices
        {
            get { return (bool)GetValue(ShowRestrictedServicesProperty); }
            set { SetValue(ShowRestrictedServicesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowRestrictedServices.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowRestrictedServicesProperty =
            DependencyProperty.Register("ShowRestrictedServices", typeof(bool), typeof(BrowseContentDialog), new PropertyMetadata(false, OnShowRestrictedChange));

        static void OnShowRestrictedChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            BrowseContentDialog dialog = o as BrowseContentDialog;
            if (dialog != null)
            {
                dialog.Clear();
            }
        }

        public bool SecureServicesEnabled
        {
            get { return (bool)GetValue(SecureServicesEnabledProperty); }
            set { SetValue(SecureServicesEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecureServicesEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecureServicesEnabledProperty =
            DependencyProperty.Register("SecureServicesEnabled", typeof(bool), typeof(BrowseContentDialog), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets whether credentials are needed to access services provided 
        /// by the current ArcGIS Server endpoint
        /// </summary>
        public bool NeedsServerCredentials
        {
            get { return (bool)GetValue(NeedsServerCredentialsProperty); }
            set { SetValue(NeedsServerCredentialsProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="NeedsServerCredentials"/> property
        /// </summary>
        public static readonly DependencyProperty NeedsServerCredentialsProperty =
            DependencyProperty.Register("NeedsServerCredentials", typeof(bool),
            typeof(BrowseContentDialog), new PropertyMetadata(false, OnNeedsCredentialsChanged));

        /// <summary>
        /// Gets or sets whether credentials are needed to access services provided 
        /// by the current ArcGIS Online or Portal endpoint
        /// </summary>
        public bool NeedsPortalCredentials
        {
            get { return (bool)GetValue(NeedsPortalCredentialsProperty); }
            set { SetValue(NeedsPortalCredentialsProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="NeedsPortalCredentials"/> property
        /// </summary>
        public static readonly DependencyProperty NeedsPortalCredentialsProperty =
            DependencyProperty.Register("NeedsPortalCredentials", typeof(bool),
            typeof(BrowseContentDialog), new PropertyMetadata(false, OnNeedsCredentialsChanged));

        private static void OnNeedsCredentialsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BrowseContentDialog)d).calculateScrollViewerSize();            
        }

        #endregion

        private SignInToServerCommand signInToServerCommand;
        /// <summary>
        /// Gets the command for prompting users to sign into the currently 
        /// connected ArcGIS Server endpoint 
        /// </summary>
        public SignInToServerCommand SignInToServerCommand
        {
            get
            {
                // Initialize the command if it isn't already
                if (signInToServerCommand == null)
                {
                    signInToServerCommand = new SignInToServerCommand();
                    
                    // When a user signs in, refresh the services in the UI
                    signInToServerCommand.SignedIn += (o, e) => refresh();
                }
                return signInToServerCommand;
            }
        }

        private SignInToAGSOLCommand signInToPortalCommand;
        /// <summary>
        /// Gets the command for prompting users to sign into the currently 
        /// connected ArcGIS Online or Portal endpoint 
        /// </summary>
        public SignInToAGSOLCommand SignInToPortalCommand
        {
            get
            {
                // Initialize the command if it isn't already;
                if (signInToPortalCommand == null)
                {
                    signInToPortalCommand = new SignInToAGSOLCommand();

                    // When a user signs in, refresh the services in the UI
                    signInToPortalCommand.SignedIn += (o, e) => refresh();
                }
                return signInToPortalCommand;
            }
        }

        #endregion

        private bool hookedToCredentialsChanged = false;
        public override void OnApplyTemplate()
        {
            if (txtUrl != null)
            {
                txtUrl.KeyDown -= txtUrl_KeyDown;
                //txtUrl.GotFocus += txtUrl_GotFocus;
                //txtUrl.LostFocus += txtUrl_LostFocus;                
            }

            if (DropDownToggle != null)
            {
                DropDownToggle.Checked -= DropDownToggle_Checked;
                DropDownToggle.Unchecked -= DropDownToggle_Unchecked;
            }

            if (treeResources != null)
                treeResources.SelectedItemChanged -= treeConnections_SelectedItemChanged;

            if (hlnkCancelButton != null)
                hlnkCancelButton.Click -= hlnkCancelButton_Click;

            if (btnOk != null)
                btnOk.Click -= btnOk_Click;

            if (btnGo != null)
                btnGo.Click -= btnGo_Click;

            if (View.Instance != null)
                View.Instance.ProxyUrlChanged -= View_ProxyUrlChanged;
            if (ViewerApplicationControl.Instance != null)
            {
                ViewerApplicationControl.Instance.ViewInitialized -= ViewerApplicationControl_ViewInitialized;
                ViewerApplicationControl.Instance.ViewDisposed -= ViewerApplicationControl_ViewDisposed;
            }
            base.OnApplyTemplate();

            DataContext = this;
            txtUrl = GetTemplateChild("txtUrl") as WatermarkedTextBox;
            if (txtUrl != null)
            {
                txtUrl.KeyDown += txtUrl_KeyDown;
                txtUrl.Text = SelectedResourceUrl ?? "";
            }

            DropDownToggle = GetTemplateChild("DropDownToggle") as ToggleButton;
            if (DropDownToggle != null)
            {
                DropDownToggle.Checked += DropDownToggle_Checked;
                DropDownToggle.Unchecked += DropDownToggle_Unchecked;
                DropDownToggle.LostFocus += new RoutedEventHandler(DropDownToggle_LostFocus);
            }

            LayoutRoot = GetTemplateChild("LayoutRoot") as FrameworkElement;

            treeResources = GetTemplateChild("treeResources") as TreeView;
            if (treeResources != null)
                treeResources.SelectedItemChanged += treeConnections_SelectedItemChanged;

            btnOk = GetTemplateChild("btnOk") as Button;
            if (btnOk != null)
                btnOk.Click += btnOk_Click;

            btnGo = GetTemplateChild("btnGo") as Button;
            if (btnGo != null)
                btnGo.Click += btnGo_Click;

            scrlContent = GetTemplateChild("scrlContent") as ScrollViewer;

            ProgressIndicatorSection = GetTemplateChild("ProgressIndicatorSection") as UIElement;

            BusyProgressIndicator = GetTemplateChild("BusyProgressIndicator") as ActivityIndicator;

            hlnkCancelButton = GetTemplateChild("hlnkCancelButton") as HyperlinkButton;
            if (hlnkCancelButton != null)
                hlnkCancelButton.Click += hlnkCancelButton_Click;

            ConnectionsDropDownPopup = GetTemplateChild("ConnectionsPopup") as Popup;
            this.ConnectionsDropDownPopupControl = GetTemplateChild("ConnectionsPopupContent") as ConnectionsDropDownPopupControl;
            if (ConnectionsDropDownPopupControl != null)
            {
                ConnectionsDropDownPopupControl.Loaded += (o2, e2) =>
                {
                    OnDropDownExpanded(EventArgs.Empty);
                };
                ConnectionsDropDownPopupControl.ConnectionSelected += (o2, e2) =>
                {
                    // Store the URL of the selected connection
                    if (e2.Connection != null)
                        _currentInputUrl = e2.Connection.Url;

                    // Close the popup (by toggling the button)
                    if (DropDownToggle != null)
                        DropDownToggle.IsChecked = false;

                    // clear all resources
                    if (treeResources != null)
                        treeResources.Items.Clear();

                    // Display the URL
                    if (txtUrl != null && e2.Connection != null)
                        txtUrl.Text = e2.Connection.Url;

                    if (e2.Connection != null)
                    {
                        enableDisableUrlEntryUI(true);  //only disable url entry and show progress if there is a connection
                        if (ShowRestrictedServices)
                            e2.Connection.ProxyUrl = getProxyUrl();
                        else
                            e2.Connection.ProxyUrl = null;

                        getDataFromConnection(e2.Connection, false);
                    }
                };
                ConnectionsDropDownPopupControl.ConnectionDeleted += (o2, e2) =>
                {
                    deleteConnection(e2.Connection);
                };
            }

            retrieveConnections();

            if (ViewerApplicationControl.Instance != null)
            {
                ViewerApplicationControl.Instance.ViewInitialized += ViewerApplicationControl_ViewInitialized;
                ViewerApplicationControl.Instance.ViewDisposed += ViewerApplicationControl_ViewDisposed;
            }

            if (View.Instance != null)
                View.Instance.ProxyUrlChanged += View_ProxyUrlChanged;

            if (!string.IsNullOrEmpty(SecureServicesHelper.GetProxyUrl()))
                SecureServicesEnabled = true;
            else
                SecureServicesEnabled = false;

            if (!hookedToCredentialsChanged)
            {
                // Start listening for changes on the application environment's set of credentials
                UserManagement.Current.Credentials.CollectionChanged += Credentials_CollectionChanged;
                hookedToCredentialsChanged = true;
            }
            
            OnInitCompleted(EventArgs.Empty);
        }

        void DropDownToggle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DropDownToggle != null)
                DropDownToggle.IsChecked = false;
        }

        void ViewerApplicationControl_ViewDisposed(object sender, ViewerApplicationControl.ViewEventArgs e)
        {
            if (e.View != null)
                e.View.ProxyUrlChanged -= View_ProxyUrlChanged;
        } 

        void ViewerApplicationControl_ViewInitialized(object sender, ViewerApplicationControl.ViewEventArgs e)
        {
            if (e.View != null)
            {
                e.View.ProxyUrlChanged += View_ProxyUrlChanged;
                View_ProxyUrlChanged(null, EventArgs.Empty);
            }
        }

        void View_ProxyUrlChanged(object sender, EventArgs e)
        {
            if (ShowRestrictedServices)
                Clear();
            if (!string.IsNullOrEmpty(SecureServicesHelper.GetProxyUrl()))
                SecureServicesEnabled = true;
            else
            {
                ShowRestrictedServices = false;
                SecureServicesEnabled = false;
            }
        }

        protected virtual void OnInitCompleted(EventArgs e)
        {
            if (InitCompleted != null)
                InitCompleted(this, e);
        }

        //Clear previous values
        public void Reset()
        {
            ShowRestrictedServices = false;
            Clear(); 
        }

        void Clear()
        {
            SelectedResourceUrl = null;
            cancelConnectionRequest();
            if (treeResources != null)
            {
                treeResources.Items.Clear();
                treeResources.Visibility = Visibility.Collapsed;
            }
            if(ConnectionsDropDownPopupControl != null)
                ConnectionsDropDownPopupControl.ClearSelection();
            if(txtUrl != null)
                txtUrl.Text = string.Empty;
        }

        internal event EventHandler InitCompleted;

        void hlnkCancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancelConnectionRequest();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (scrlContent != null && !double.IsInfinity(availableSize.Height))
            {
                ParentWindow = ControlTreeHelper.FindAncestorOfType<FloatingWindow>(this);
                scrlContent.Height = availableSize.Height - (ParentWindow != null ? 90 : 70);
                signInLinkHeightDeducted = false;
                calculateScrollViewerSize();
            }
            return base.MeasureOverride(availableSize);
        }

        private bool signInLinkHeightDeducted;
        int signInLinkHeight = 30;
        private void calculateScrollViewerSize()
        {
            if (scrlContent == null)
                return;

            if ((NeedsPortalCredentials || NeedsServerCredentials) && !signInLinkHeightDeducted)
            {
                scrlContent.Height -= signInLinkHeight;
                signInLinkHeightDeducted = true;
            }
            else if (!NeedsPortalCredentials && !NeedsServerCredentials && signInLinkHeightDeducted)
            {
                scrlContent.Height += signInLinkHeight;
                signInLinkHeightDeducted = false;
            }
        }

        void DropDownToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ConnectionsDropDownPopup != null)
                ConnectionsDropDownPopup.IsOpen = false;
            OnDropDownCollapsed(EventArgs.Empty);
        }

        void DropDownToggle_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.RootVisual.MouseLeftButtonDown -= RootVisual_MouseLeftButtonUp;
            Application.Current.RootVisual.MouseLeftButtonDown += RootVisual_MouseLeftButtonUp;
            onDropDownToggle_Checked();
        }

        void RootVisual_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseChildPopups(); // clicked else where
        }

        private void onDropDownToggle_Checked()
        {
            ConnectionsDropDownPopupControl.Connections = Connections;
            if (txtUrl != null)
            {
                ConnectionsDropDownPopupControl.Width = txtUrl.RenderSize.Width;
                if (ConnectionsDropDownPopupControl.Width < 300) // ensure that it is atleast 300
                    ConnectionsDropDownPopupControl.Width = 300;
            }
            positionDropDownRelativeToTextBox();
            ConnectionsDropDownPopup.IsOpen = true;
        }

        private void positionDropDownRelativeToTextBox()
        {
            if (ConnectionsDropDownPopup == null)
                return;
            if (txtUrl != null)
            {
                GeneralTransform gt = (txtUrl).TransformToVisual(
Application.Current.RootVisual as UIElement
);

                Application.Current.RootVisual.MouseLeftButtonDown -= RootVisual_MouseLeftButtonDown;
                Application.Current.RootVisual.MouseLeftButtonDown += RootVisual_MouseLeftButtonDown;
            }

            if (Connections != null)
            {
                if(Connections.Count > 0)
                    ConnectionsDropDownPopup.Height = ConnectionsDropDownPopupControl.Height = Math.Min(Connections.Count * 30 + 20, 250);
            }
        }

        void RootVisual_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ConnectionsDropDownPopup != null)
                ConnectionsDropDownPopup.IsOpen = false;

            Application.Current.RootVisual.MouseLeftButtonDown -= RootVisual_MouseLeftButtonDown;
        }

        internal void OnParentContainerWindowMoved()
        {
            if (ConnectionsDropDownPopup == null || !ConnectionsDropDownPopup.IsOpen)
                return;
            positionDropDownRelativeToTextBox();
        }

        IDataSourceWithResources currentActiveConnectionDataSource;
        private async void getDataFromConnection(Connection connection, bool isNewConnection)
        {
            if (connection == null)
                return;

            if (DataSourceProvider == null)
                throw new InvalidOperationException("Must specify DataSourceProvider");

            // Reset whether endpoint needs credentials to access sevices
            NeedsServerCredentials = false;

            if (connection.ConnectionType == ConnectionType.Unknown)
            {
                // Try datasources 1, by 1
                Queue<IDataSourceWithResources> dataSources = new Queue<IDataSourceWithResources>();
                foreach (IDataSourceWithResources dataSource in DataSourceProvider.GetAllDataSourcesWhichSupportResources())
                {
                    dataSources.Enqueue(dataSource);
                }
                tryGetResourceFromNextDataSourceInQueue(dataSources, connection, isNewConnection);
            }
            else
            {
                ESRI.ArcGIS.Mapping.Core.DataSources.DataSource dataSource = DataSourceProvider.CreateNewDataSourceForConnectionType(connection.ConnectionType);
                if (dataSource == null)
                    throw new Exception("Unable to retrieve datasource for connection type " + connection.ConnectionType);

                IDataSourceWithResources dataSourceWithResource = dataSource as IDataSourceWithResources;
                if (dataSourceWithResource == null)
                    throw new Exception("The connection type " + connection.ConnectionType + " does not correspond to a datasource (" + dataSource.ID + ") which has resources");

                currentActiveConnectionDataSource = dataSourceWithResource;

                // Analyze the connection string and try to determine if this is a server or a more specific endpoint such as an
                // individual map service, layer, gp service, gp tool, etc.
                Resource res = dataSourceWithResource.GetResource(connection.Url, connection.ProxyUrl);
                if (res.ResourceType == ResourceType.Server)
                {
                    // Get the URL to use for token authentication
                    _currentRestUrl = 
                        await ArcGISServerDataSource.GetServicesDirectoryURL(connection.Url, connection.ProxyUrl);
                    // Initialize the command allowing users to sign-in
                    await initializeSignInCommand(_currentRestUrl, connection.ProxyUrl);

                    dataSourceWithResource.GetCatalogCompleted += (o, e) =>
                    {
                        dataSource_GetCatalogCompleted(o, e);
                    };
                    dataSourceWithResource.GetCatalogFailed += (o, e) =>
                    {
                        enableDisableUrlEntryUI(false);
                        Logger.Instance.LogError(e.Exception);

                        bool displayErrorMessage = e is GetCatalogFailedEventArgs && !((GetCatalogFailedEventArgs)e).DisplayErrorMessage;
                        if (!displayErrorMessage)
                        {
                            string message = string.Format(LocalizableStrings.ServiceConnectionErrorDuringInit, connection.Url);
                            MessageBoxDialog.Show(message, LocalizableStrings.ServiceConnectionErrorCaption, MessageBoxButton.OK);
                        }
                        OnConnectionRemoved(new ConnectionRemovedEventArgs() { Connection = connection });
                    };
                    dataSourceWithResource.GetCatalog(connection.Url, connection.ProxyUrl, filter, new object[] { connection, isNewConnection, Dispatcher });
                }
                else
                {
                    // If the filter permits this kind of resource, then obtain child information
                    if (ApplyFilterToResourceType(res, filter))
                    {
                        dataSourceWithResource.GetChildResourcesCompleted += (o, e) =>
                        {
                            resource_GetChildResourcesCompleted(o, e);
                        };
                        dataSourceWithResource.GetChildResourcesFailed += (o, e) =>
                        {
                            Logger.Instance.LogError(e.Exception);
                            MessageBoxDialog.Show("Unable to retrieve resources." + Environment.NewLine + e.Exception != null ? e.Exception.Message : "");
                        };
                        
                        // Before connecting to the server to get its list of services, try authenticating
                        // with the existing set of credentials in the application.  This makes it so that
                        // if a user has signed in to another server with a username and password that is
                        // valid for this server, it will authenticate using those credentials automatically
                        IdentityManager.Credential newCred = 
                            await ApplicationHelper.TryExistingCredentials(connection.Url, connection.ProxyUrl);
                        if (newCred != null)
                        {
                            // If authentication is successful and the URL belong to the current ArcGIS Portal,
                            // use the credentials to also sign into the application environment's current 
                            // Portal instance
                            if (await connection.Url.IsFederatedWithPortal())
                                await ApplicationHelper.SignInToPortal(newCred);

                            // If a credential for the URL has not already be saved into the application 
                            // environment, do so now
                            if (!!UserManagement.Current.Credentials.Any(c => c.Url != null 
                            && c.Url.Equals(newCred.Url, StringComparison.OrdinalIgnoreCase) 
                            && !string.IsNullOrEmpty(c.Token)))
                                UserManagement.Current.Credentials.Add(newCred);
                        }
                        dataSourceWithResource.GetChildResourcesAsync(res, filter, new object[] { treeResources, dataSourceWithResource, res, connection });
                    }
                    else
                    {
                        // If filtering prevents this kind of item in this context then create a tree node to indicate the error.
                        showHideProgressIndicator(true);
                        enableDisableUrlEntryUI(false);

                        TreeViewItem tvi = CreateTreeNodeWhenNoItems();
                        treeResources.Items.Add(tvi);
                    }
                }
            }
        }

        private void tryGetResourceFromNextDataSourceInQueue(Queue<IDataSourceWithResources> dataSources, Connection connection, bool isNewConnection)
        {
            if (dataSources == null || dataSources.Count < 1)
            {
                connectionFailed(connection);
                return;
            }
            GetResourceFromNextDataSourceInQueue(dataSources, connection, isNewConnection);
        }

        private void connectionFailed(Connection connection, bool showErrorMessage = true)
        {
            enableDisableUrlEntryUI(false);
            string message = string.Format(LocalizableStrings.ServiceConnectionErrorDuringInit, connection.Url);
            if (showErrorMessage)
                MessageBoxDialog.Show(message, LocalizableStrings.ServiceConnectionErrorCaption, MessageBoxButton.OK);
            OnConnectionRemoved(new ConnectionRemovedEventArgs() { Connection = connection });
            OnGetCatalogFailed(new core.ExceptionEventArgs(new Exception(message), null));
        }

        private async void GetResourceFromNextDataSourceInQueue(Queue<IDataSourceWithResources> dataSources, Connection connection, bool isNewConnection)
        {
            IDataSourceWithResources dataSource = dataSources.Dequeue();
            connection.ConnectionType = DataSourceProvider.MapDataSourceToConnectionType(dataSource);
            currentActiveConnectionDataSource = dataSource;

            // Analyze the connection string and try to determine if this is a server or a more specific endpoint such as an
            // individual map service, layer, gp service, gp tool, etc.
            Resource res = dataSource.GetResource(connection.Url, connection.ProxyUrl);
            if (res.ResourceType == ResourceType.Server)
            {
                // Get the URL to use for token authentication
                _currentRestUrl = 
                    await ArcGISServerDataSource.GetServicesDirectoryURL(connection.Url, connection.ProxyUrl);
                // Initialize the command allowing users to sign-in
                await initializeSignInCommand(_currentRestUrl, connection.ProxyUrl);

                dataSource.GetCatalogCompleted += (o, e) =>
                {
                    dataSource_GetCatalogCompleted(o, e);
                };
                dataSource.GetCatalogFailed += (o, e) =>
                {
                    // Try the next datasource                
                    tryGetResourceFromNextDataSourceInQueue(dataSources, connection, isNewConnection);
                };
                dataSource.GetCatalog(connection.Url, connection.ProxyUrl, filter, new object[] { connection, isNewConnection });
            }
            else
            {
                // If the filter permits this kind of resource, then obtain child information
                if (ApplyFilterToResourceType(res, filter))
                {
                    dataSource.GetChildResourcesCompleted += (o, e) =>
                    {
                        resource_GetChildResourcesCompleted(o, e);
                    };
                    dataSource.GetChildResourcesFailed += (o, e) =>
                    {
                        // Code 499 == token authentication failed.  So the endpoint exists, but the 
                        // token was not retrieved.  In this case, don't try other data source types.
                        if (e.StatusCode == 499) 
                            connectionFailed(connection, false);
                        else // Try the next datasource                
                            tryGetResourceFromNextDataSourceInQueue(dataSources, connection, isNewConnection);
                    };

                    // Before connecting to the server to get its list of services, try authenticating
                    // with the existing set of credentials in the application.  This makes it so that
                    // if a user has signed in to another server with a username and password that is
                    // valid for this server, it will authenticate using those credentials automatically
                    IdentityManager.Credential newCred = await ApplicationHelper.TryExistingCredentials(connection.Url, connection.ProxyUrl);
                    if (newCred != null)
                    {
                        // If authentication is successful and the URL belong to the current ArcGIS Portal,
                        // use the credentials to also sign into the application environment's current 
                        // Portal instance
                        if (await connection.Url.IsFederatedWithPortal())
                            await ApplicationHelper.SignInToPortal(newCred);

                        // If a credential for the URL has not already be saved into the application 
                        // environment, do so now
                        if (!UserManagement.Current.Credentials.Any(c => c.Url != null 
                        && c.Url.Equals(newCred.Url, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(c.Token)))
                            UserManagement.Current.Credentials.Add(newCred);
                    }
                    dataSource.GetChildResourcesAsync(res, filter, new object[] { treeResources, dataSource, res, connection });
                }
                else
                {
                    // If filtering prevents this kind of item in this context then create a tree node to indicate the error.
                    showHideProgressIndicator(true);
                    enableDisableUrlEntryUI(false);

                    TreeViewItem tvi = CreateTreeNodeWhenNoItems();
                    treeResources.Items.Add(tvi);
                }
            }
        }

        // Initializes the command used to prompt the user to sign in to the current 
        // ArcGIS Server or Portal instance
        private async Task initializeSignInCommand(string servicesDirectoryUrl, string proxyUrl)
        {
            if (servicesDirectoryUrl != null)
            {
                // Before intializing the command, attempt to authenticate using existing credentials.  If
                // successful, add the credential to the app environment's collection
                IdentityManager.Credential newCred = await ApplicationHelper.TryExistingCredentials(servicesDirectoryUrl, proxyUrl);
                if (newCred != null && !UserManagement.Current.Credentials.Any(c => c.Url != null 
                && c.Url.Equals(newCred.Url, StringComparison.OrdinalIgnoreCase) 
                && !string.IsNullOrEmpty(c.Token)))
                    UserManagement.Current.Credentials.Add(newCred);

                if (await servicesDirectoryUrl.IsFederatedWithPortal()) // Check whether the AGS instance referred to is a federated server
                {
                    // Server is federated, so sign-in will be with Portal, not AGS

                    SignInToPortalCommand.ProxyUrl = proxyUrl;
                    NeedsServerCredentials = false; // don't need server credentials since we'll be signing into Portal
                    NeedsPortalCredentials = newCred == null; // whether we need credentials for Portal depends on whether TryExistingCredentials found any

                    if (!NeedsPortalCredentials 
                    && ArcGISOnlineEnvironment.ArcGISOnline != null
                    && (ArcGISOnlineEnvironment.ArcGISOnline.User == null 
                        || ArcGISOnlineEnvironment.ArcGISOnline.User.Current == null)) 
                    {
                        // We already have credentials for Portal, so complete the "sign-in" experience for portal
                        IdentityManager.Credential portalCred = null;
                        if (!newCred.Url.IsPortalUrl()) // Check whether the URL originates from the Portal server
                        { 
                            try
                            {
                                // The credential we have is for a federated server that's not the portal server itself.  So try getting
                                // one for the portal server as we'll need that to get secure Portal metadata.
                                var portalUrl = MapApplication.Current != null && MapApplication.Current.Portal != null 
                                    ? MapApplication.Current.Portal.Url : null;
                                portalCred = await IdentityManager.Current.GetCredentialTaskAsync(portalUrl, false); 
                            }
                            catch {}
                        }
                        // Complete "sign-in."  This performs actions like getting the current user's metadata.
                        await ApplicationHelper.SignInToPortal(portalCred ?? newCred);
                    }
                }
                else
                {
                    SignInToServerCommand.Url = servicesDirectoryUrl;
                    SignInToServerCommand.ProxyUrl = proxyUrl;
                    NeedsServerCredentials = newCred == null;
                    NeedsPortalCredentials = false;
                }
            }
        }

        // Monitors the application environment's set of credentials for changes
        private void Credentials_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Checks whether the passed-in list contains a credential for the current service endpoint
            Func<IList, bool> containsCurrentCredential = (l) =>
                {
                    if (l == null || _currentRestUrl == null)
                        return false;

                    foreach (IdentityManager.Credential cred in l)
                    {
                        if (cred.Url.TrimEnd('/').Equals(_currentRestUrl.TrimEnd('/'), 
                        StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    return false;
                };

            // Refresh the list of services if the old or new credentials contain the current service 
            // endpoint (e.g. a sign in or sign out has occurred to the endpoint) or all the
            // credentials have been cleared
            if (containsCurrentCredential(e.OldItems) || containsCurrentCredential(e.NewItems)
            || e.Action == NotifyCollectionChangedAction.Reset && e.NewItems == null)
                refresh();
        }

        private string getProxyUrl()
        {
            string proxyUrl = SecureServicesHelper.GetProxyUrl();
            if (!string.IsNullOrEmpty(proxyUrl))
            {
                proxyUrl = proxyUrl.Trim();
                if (!proxyUrl.EndsWith("?", StringComparison.Ordinal))
                    proxyUrl = proxyUrl + "?";
            }
            return proxyUrl;
        }

        private bool ApplyFilterToResourceType(Resource res, Filter filter)
        {
            if (res.ResourceType == ResourceType.MapServer || res.ResourceType == ResourceType.Layer)
            {
                if ((filter & Filter.None) == Filter.None ||
                    (filter & Filter.SpatiallyEnabledResources) == Filter.SpatiallyEnabledResources)
                    return true;
                else
                    return false;
            }
            else if (res.ResourceType == ResourceType.GPServer || res.ResourceType == ResourceType.GPTool)
            {
                if ((filter & Filter.None) == Filter.None || 
                    (filter & Filter.GeoprocessingServices) == Filter.GeoprocessingServices)
                    return true;
                else
                    return false;
            }
            else if (res.ResourceType == ResourceType.FeatureServer || res.ResourceType == ResourceType.EditableLayer)
            {
                if ((filter & Filter.None) == Filter.None ||
                    (filter & Filter.FeatureServices) == Filter.FeatureServices)
                    return true;
                else
                    return false;
            }
            else if (res.ResourceType == ResourceType.Database || res.ResourceType == ResourceType.DatabaseTable)
            {
                if ((filter & Filter.None) == Filter.None ||
                    (filter & Filter.SpatiallyEnabledResources) == Filter.SpatiallyEnabledResources)
                    return true;
                else
                    return false;
            }

            return false;
        }

        void dataSource_GetCatalogCompleted(object sender, GetCatalogCompletedEventArgs e)
        {
            enableDisableUrlEntryUI(false);

            object[] userState = e.UserState as object[];
            if (userState == null || userState.Length < 2)
                return;

            Connection connection = userState[0] as Connection;
            if (connection == null)
                return;

            if (e.ChildResources == null)
                return;

            if (treeResources == null)
                return;

            ESRI.ArcGIS.Mapping.Core.DataSources.DataSource ds = DataSourceProvider.CreateNewDataSourceForConnectionType(connection.ConnectionType);
            if (ds == null)
                return;

            IDataSourceWithResources dsWithResource = ds as IDataSourceWithResources;

            treeResources.Items.Clear();

            foreach (Resource childResource in e.ChildResources)
            {
                TreeViewItem treeViewItem = new TreeViewItem
                {
                    HeaderTemplate = LayoutRoot != null ? LayoutRoot.Resources["ResourceNodeDataTemplate"] as DataTemplate : null,
                    Header = childResource,
                    DataContext = childResource,
                    IsExpanded = false,
                    Style = LayoutRoot != null ? LayoutRoot.Resources["TreeViewItemStyle"] as Style : null,
                };
                if(!string.IsNullOrWhiteSpace(childResource.DisplayName))
                    treeViewItem.SetValue(System.Windows.Automation.AutomationProperties.AutomationIdProperty, childResource.DisplayName);
                
                treeViewItem.SetValue(TreeViewItemExtensions.ConnectionProperty, connection);
                if (dsWithResource != null && dsWithResource.SupportsChildResources(childResource, filter))
                {
                    treeViewItem.Items.Add(createRetrievingNode());
                    treeViewItem.Expanded += new RoutedEventHandler(resourceNode_Expanded);
                }
                treeResources.Items.Add(treeViewItem);
            }

            showHideProgressIndicator(true);
            bool addConnection = true;
            if (treeResources.Items.Count == 0)
            {
                if(!string.IsNullOrEmpty(e.Error))
                    MessageBoxDialog.Show(e.Error, ESRI.ArcGIS.Mapping.Controls.Resources.Strings.BrowseDialogGeographicDataNotFound, MessageBoxButton.OK);
                else if ((Filter & Filter.GeoprocessingServices) == Filter.GeoprocessingServices)
                    MessageBoxDialog.Show(string.Format(LocalizableStrings.NoGPServicesFound, connection.Name));
                else
                    MessageBoxDialog.Show(string.Format(LocalizableStrings.NoMapServicesFound, connection.Name));
                addConnection = false;
            }

            OnGetCatalogCompleted(e);
            bool isNewConnection = (bool)userState[1];
            if (isNewConnection)
            {
                if (addConnection)
                {
                    Connections.Add(connection);
                    if (ConnectionsProvider != null)
                        ConnectionsProvider.AddConnection(connection); // inform the provider about the new connection
                    OnConnectionAdded(new ConnectionAddedEventArgs() { Connection = connection });
                }
            }
        }

        void resourceNode_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = sender as TreeViewItem;
            if (tvi == null)
            {
                tvi.Items.Clear();
                return;
            }
            if (hadAlreadyRetrievedChildNodes(tvi))
                return;
            Resource resource = tvi.DataContext as Resource;
            if (resource == null)
            {
                tvi.Items.Clear();
                return;
            }
            Connection conn = getParentConnectionForTreeViewItem(tvi);
            if (conn == null)
            {
                tvi.Items.Clear();
                return;
            }
            ESRI.ArcGIS.Mapping.Core.DataSources.DataSource ds = DataSourceProvider.CreateNewDataSourceForConnectionType(conn.ConnectionType);
            if (ds == null)
            {
                tvi.Items.Clear();
                return;
            }
            IDataSourceWithResources dsWithResource = ds as IDataSourceWithResources;
            if (dsWithResource == null)
            {
                tvi.Items.Clear();
                return;
            }
            dsWithResource.GetChildResourcesCompleted += (o, args) =>
            {
                resource_GetChildResourcesCompleted(o, args);
            };
            dsWithResource.GetChildResourcesFailed += (o, args) =>
            {
                tvi.Items.Clear();
                Logger.Instance.LogError(args.Exception);
                MessageBoxDialog.Show("Unable to retrieve resources." + Environment.NewLine + args.Exception != null ? args.Exception.Message : "");
            };
            dsWithResource.GetChildResourcesAsync(resource, filter, new object[] { tvi, dsWithResource });
        }

        void resource_GetChildResourcesCompleted(object sender, GetChildResourcesCompletedEventArgs e)
        {
            TreeViewItem tvi = null;
            string childName = "";

            object[] userState = e.UserState as object[];

            IDataSourceWithResources dataSource = userState[1] as IDataSourceWithResources;
            if (dataSource == null)
                return;

            // If the element at [0] is a tree view item, then this is a standard expand action on a parent node and
            // the logic flow is as it was before: remove current contents, add from array
            tvi = userState[0] as TreeViewItem;
            if (tvi != null)
            {
                tvi.Items.Clear();
            }
            else
            {
                // If the item we are adding to is the root (TreeView) then there are no children, but we are instead using the
                // progress indicator and must clear this here.
                showHideProgressIndicator(true);
                enableDisableUrlEntryUI(false);

                // Fabricate parent node and include connection, etc. so that when this node is expanded or child nodes are
                // referenced, the infrastructure will be in place to make existing logic work to obtain child information
                Resource parentResource = userState[2] as Resource;
                tvi = new TreeViewItem
                {
                    HeaderTemplate = LayoutRoot != null ? LayoutRoot.Resources["ResourceNodeDataTemplate"] as DataTemplate : null,
                    Header = parentResource,
                    DataContext = parentResource,
                    IsExpanded = true,
                    IsSelected = true,
                    Style = LayoutRoot != null ? LayoutRoot.Resources["TreeViewItemStyle"] as Style : null,
                };
                Connection connection = userState[3] as Connection;
                tvi.SetValue(TreeViewItemExtensions.ConnectionProperty, connection);
                treeResources.Items.Add(tvi);

                // If the parent item "tag" is assigned a value, store it for comparison against all child elements so that
                // the proper child element can be automatically selected. This is used when the URL contains the child
                // element of a map service, gp service, feature service, etc.
                if (parentResource.Tag != null)
                {
                    childName = parentResource.Tag as string;
                }
            }

            if (e.ChildResources == null)
                return;

            foreach (Resource childResource in e.ChildResources)
            {
                TreeViewItem treeViewItem = new TreeViewItem
                {
                    HeaderTemplate = LayoutRoot != null ? LayoutRoot.Resources["ResourceNodeDataTemplate"] as DataTemplate : null,
                    Header = childResource,
                    DataContext = childResource,
                    IsExpanded = false,
                    Style = LayoutRoot != null ? LayoutRoot.Resources["TreeViewItemStyle"] as Style : null,
                };
                if (childResource.ResourceType == ResourceType.Layer || childResource.ResourceType == ResourceType.EditableLayer)
                {
                    treeViewItem.SetValue(ToolTipService.ToolTipProperty, childResource.ResourceType == ResourceType.Layer ? Strings.MapLayer : Strings.FeatureLayer);
                }
                if (!string.IsNullOrWhiteSpace(childResource.DisplayName))
                {
                    string parentAutomationId = tvi.GetValue(System.Windows.Automation.AutomationProperties.AutomationIdProperty) as string;
                    if (parentAutomationId == null)
                        parentAutomationId = string.Empty;
                    treeViewItem.SetValue(System.Windows.Automation.AutomationProperties.AutomationIdProperty, parentAutomationId + childResource.DisplayName + childResource.ResourceType);
                }

                bool hasChildResource = dataSource.SupportsChildResources(childResource, filter);
                if (hasChildResource)
                {
                    treeViewItem.Items.Add(createRetrievingNode());
                    treeViewItem.Expanded += new RoutedEventHandler(resourceNode_Expanded);
                }

                // If a child Id was successfully extracted from the parent resource, this means there should be a child
                // resource with the same id and we need to select this as the initial URL indicated the desire to use
                // this child resource.
                if (!String.IsNullOrEmpty(childName))
                {
                    if (childResource.ResourceType == ResourceType.GPTool || childResource.ResourceType == ResourceType.DatabaseTable)
                    {
                        if (String.Compare(childName, childResource.DisplayName) == 0)
                            treeViewItem.IsSelected = true;
                    }
                    else if (childResource.ResourceType == ResourceType.Layer)
                    {
                        if (childResource.Tag != null)
                        {
                            int nodeId = (int)childResource.Tag;
                            string nodeName = String.Format("{0}", nodeId);
                            if (String.Compare(childName, nodeName) == 0)
                                treeViewItem.IsSelected = true;
                        }
                    }
                }

                tvi.Items.Add(treeViewItem);
            }

            if (tvi.Items.Count == 0)
            {
                TreeViewItem treeViewItem = CreateTreeNodeWhenNoItems();
                tvi.Items.Add(treeViewItem);
            }

            OnGetChildResourcesCompleted(e);
        }

        private TreeViewItem CreateTreeNodeWhenNoItems()
        {
            string message = "";
            if ((Filter & Filter.GeoprocessingServices) == Filter.GeoprocessingServices)
                message = ESRI.ArcGIS.Mapping.Controls.Resources.Strings.BrowseDialogNoResourcesFound;
            else
                message = ESRI.ArcGIS.Mapping.Controls.Resources.Strings.BrowseDialogNoServerOrLayerFound;

            Resource emptyResource = new Resource
            {
                DisplayName = message,
                ResourceType = ResourceType.Undefined
            };
            TreeViewItem treeViewItem = new TreeViewItem
            {
                HeaderTemplate = LayoutRoot != null ? LayoutRoot.Resources["ResourceNodeDataTemplate"] as DataTemplate : null,
                Header = emptyResource,
                DataContext = emptyResource,
                IsExpanded = false,
                Style = LayoutRoot != null ? LayoutRoot.Resources["TreeViewItemStyle"] as Style : null,
            };
            return treeViewItem;
        }

        private Connection getParentConnectionForTreeViewItem(TreeViewItem item)
        {
            if (item == null)
                return null;
            Resource resource = item.DataContext as Resource;
            if (resource != null)
            {
                Connection c = item.GetValue(TreeViewItemExtensions.ConnectionProperty) as Connection;
                if (c != null)
                    return c;
            }
            Connection ds = item.DataContext as Connection;
            if (ds != null)
                return ds;
            TreeViewItem parent = item.Parent as TreeViewItem;
            if (parent == null)
                parent = ControlTreeHelper.FindAncestorOfType<TreeViewItem>(item);
            if (parent == null)
                return null;
            return getParentConnectionForTreeViewItem(parent);
        }

        private void txtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                getResourcesForUrl(txtUrl.Text);
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            getResourcesForUrl(txtUrl.Text);
        }

        private void treeConnections_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem item = treeResources.SelectedItem as TreeViewItem;
            if (item == null)
            {
                item = Tag as TreeViewItem; // Check the Tag Property (Set only by Unit tests)
                if (item == null)
                    return;
            }
            Layer layer = item.Tag as Layer;
            if (layer == null)
            {
                Connection conn = getParentConnectionForTreeViewItem(item);
                if (conn == null)
                {
                    SelectedResourceUrl = null;
                    return;
                }
                IDataSourceWithResources dataSource = DataSourceProvider.CreateNewDataSourceForConnectionType(conn.ConnectionType) as IDataSourceWithResources;
                if (dataSource == null)
                {
                    SelectedResourceUrl = null;
                    return;
                }
                Resource resource = item.DataContext as Resource;
                if (resource == null)
                {
                    SelectedResourceUrl = null;
                    return;
                }

                bool selectable = dataSource.IsResourceSelectable(resource, filter);
                if (btnOk != null)
                    btnOk.IsEnabled = selectable;

                SelectedResourceUrl = selectable ? resource.Url : null;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (treeResources == null)
                return;
            TreeViewItem item = treeResources.SelectedItem as TreeViewItem;
            if (item == null)
            {
                item = Tag as TreeViewItem;
                if (item == null) // Check the Tag Property (Set only by Unit tests)
                    return;
            }
            Connection conn = getParentConnectionForTreeViewItem(item);
            if (conn == null)
                return;
            Resource resource = item.DataContext as Resource;
            if (resource != null && ShowRestrictedServices)
                resource.ProxyUrl = getProxyUrl();
            OnResourceSelected(new ResourceSelectedEventArgs()
            {
                Resource = resource,
                ConnectionType = conn.ConnectionType
            });
        }

        internal SpatialReference MapSpatialReference { get; set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            OnCancelButtonClicked(EventArgs.Empty);
            txtUrl.Focus();
        }

        #region Helper functions
        private void enableDisableUrlEntryUI(bool disable)
        {
            if (txtUrl != null)
                txtUrl.IsEnabled = !disable;
            // Also show the progress indicator .. if UI entry is disabled, show a progress indicator
            showHideProgressIndicator(!disable);
        }

        void showHideProgressIndicator(bool hide)
        {
            if (ProgressIndicatorSection != null)
                ProgressIndicatorSection.Visibility = hide ? Visibility.Collapsed : Visibility.Visible;

            if (BusyProgressIndicator != null)
            {
                if (hide)
                    BusyProgressIndicator.StopProgressAnimation();
                else
                    BusyProgressIndicator.StartProgressAnimation();
            }

            if (treeResources != null)
                treeResources.Visibility = hide ? Visibility.Visible : Visibility.Collapsed;
        }

        private void getResourcesForUrl(string url)
        {
            // Remove any leading or trailing spaces as this will cause an exception when parsing this
            // or trying to convert it into a URI
            url = url.Trim();

            _currentInputUrl = url;

            if (currentActiveConnectionDataSource != null)
                currentActiveConnectionDataSource.CancelAllCurrentRequests();

            if (treeResources != null)
                treeResources.Items.Clear();

            if (DropDownToggle != null)
                DropDownToggle.IsChecked = false;

            if (string.IsNullOrEmpty(url)) return;

            bool connectionExists = false;
            if (Connections != null)
                connectionExists = Connections.FirstOrDefault<Connection>(c => c != null && (string.Compare(c.Url, url, StringComparison.InvariantCultureIgnoreCase) == 0)) != null;

            enableDisableUrlEntryUI(true);
            Connection newConnection = new Connection()
            {
                ConnectionType = ConnectionType.Unknown,
                Name = url,
                Url = url,
            };
            if (ShowRestrictedServices)
                newConnection.ProxyUrl = getProxyUrl();

            getDataFromConnection(newConnection, !connectionExists);
        }

        /// <summary>
        /// Re-retrieves services for the current URL
        /// </summary>
        private void refresh()
        {
            if (!string.IsNullOrEmpty(_currentInputUrl))
                getResourcesForUrl(_currentInputUrl);
        }

        private TreeViewItem createRetrievingNode()
        {
            return new TreeViewItem
            {
                HeaderTemplate = LayoutRoot != null ? LayoutRoot.Resources["RetrievingNodeDataTemplate"] as DataTemplate : null,
            };
        }

        private bool hadAlreadyRetrievedChildNodes(TreeViewItem tvi)
        {
            if (tvi == null ||
                tvi.Items.Count == 0 ||
                (tvi.Items.Count == 1 && isNodeTheRetrievingNode(tvi.Items[0] as TreeViewItem)))
                return false;
            return true;
        }

        private bool isNodeTheRetrievingNode(TreeViewItem tvi)
        {
            return tvi != null && tvi.HeaderTemplate == (LayoutRoot != null ? LayoutRoot.Resources["RetrievingNodeDataTemplate"] : null);
        }
        #endregion

        private void deleteConnection(Connection connection)
        {
            if (connection == null)
                return;

            MessageBoxDialog.Show(LocalizableStrings.DeleteConnectionPrompt, LocalizableStrings.DeleteConnectionCaption, MessageBoxButton.OKCancel,
                            new MessageBoxClosedEventHandler(delegate(object obj, MessageBoxClosedArgs args1)
                            {
                                if (args1.Result == MessageBoxResult.OK)
                                {
                                    if (Connections != null)
                                        Connections.Remove(connection);
                                    if (ConnectionsProvider != null)
                                        ConnectionsProvider.DeleteConnection(connection); // inform the provider about the deleted connection
                                    if (ConnectionRemoved != null)
                                        ConnectionRemoved(this, new ConnectionRemovedEventArgs() { Connection = connection });
                                }
                            }));
        }

        void cancelConnectionRequest()
        {
            if (currentActiveConnectionDataSource != null)
                currentActiveConnectionDataSource.CancelAllCurrentRequests();

            enableDisableUrlEntryUI(false);
        }

        protected virtual void OnConnectionAdded(ConnectionAddedEventArgs args)
        {
            if (ConnectionAdded != null)
                ConnectionAdded(this, args);
        }

        protected virtual void OnConnectionRemoved(ConnectionRemovedEventArgs args)
        {
            if (ConnectionRemoved != null)
                ConnectionRemoved(this, args);
        }

        public void CloseChildPopups()
        {
            if (DropDownToggle != null)
                DropDownToggle.IsChecked = false;
        }

        public void RepositionPopups()
        {
            if (DropDownToggle != null && DropDownToggle.IsChecked.Value)
                positionDropDownRelativeToTextBox();
        }

        protected virtual void OnCancelButtonClicked(EventArgs e)
        {
            if (CancelButtonClicked != null)
                CancelButtonClicked(this, e);
        }

        protected virtual void OnResourceSelected(ResourceSelectedEventArgs args)
        {
            if (ResourceSelected != null)
                ResourceSelected(this, args);
        }

        protected virtual void OnGetCatalogFailed(core.ExceptionEventArgs e)
        {
            if (GetCatalogFailed != null)
                GetCatalogFailed(this, e);
        }

        protected virtual void OnGetCatalogCompleted(GetCatalogCompletedEventArgs args)
        {
            if (GetCatalogCompleted != null)
                GetCatalogCompleted(this, args);
        }

        protected virtual void OnGetChildResourcesCompleted(GetChildResourcesCompletedEventArgs args)
        {
            if (GetChildResourcesCompleted != null)
                GetChildResourcesCompleted(this, args);
        }

        protected virtual void OnGetChildResourcesFailed(core.ExceptionEventArgs args)
        {
            if (GetChildResourcesFailed != null)
                GetChildResourcesFailed(this, args);
        }

        protected virtual void OnDropDownExpanded(EventArgs args)
        {
            if (DropDownExpanded != null)
                DropDownExpanded(this, args);
        }

        protected virtual void OnDropDownCollapsed(EventArgs args)
        {
            if (DropDownCollapsed != null)
                DropDownCollapsed(this, args);
        }

        public event EventHandler DropDownExpanded;
        public event EventHandler DropDownCollapsed;
        public event EventHandler<core.ExceptionEventArgs> GetChildResourcesFailed;
        public event EventHandler<GetChildResourcesCompletedEventArgs> GetChildResourcesCompleted;
        public event EventHandler<core.ExceptionEventArgs> GetCatalogFailed;
        public event EventHandler<GetCatalogCompletedEventArgs> GetCatalogCompleted;
        public event EventHandler<ConnectionAddedEventArgs> ConnectionAdded;
        public event EventHandler<ConnectionRemovedEventArgs> ConnectionRemoved;
        public event EventHandler CancelButtonClicked;

        public event EventHandler<ResourceSelectedEventArgs> ResourceSelected;
        public event EventHandler<EventArgs> SelectedResourceChanged;
    }
}
