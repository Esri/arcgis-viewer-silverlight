/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ESRI.ArcGIS.Client.WebMap;
using ESRI.ArcGIS.Client.Portal;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Provides access to instance-level properties and functionality of a mapping application
    /// </summary>
    public class MapApplication : DependencyObject, IMapApplication, IApplicationAdmin
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="MapApplication"/> class
        /// </summary>
        private MapApplication()
        {
            ConfigurableControls = new ObservableCollection<FrameworkElement>();
        }
        #endregion

        #region Static Properties
        private static object lockObject = new object();
        private static MapApplication _applicationContext;

        /// <summary>
        /// Gets the <see cref="MapApplication"/> instance for the current application
        /// </summary>
        public static MapApplication Current
        {
            get
            {
                return _applicationContext;
            }
        }

        private static IMapApplication _appHost;
        #endregion

        #region Members
        /// <summary>
        /// Gets the culture of the application
        /// </summary>
        public CultureInfo Culture
        {
            get
            {
                if (DesignerProperties.IsInDesignTool)
                    return null;
                checkApplicationContext();
                return _appHost.Culture;
            }
        }

        /// <summary>
        /// Gets the Map object for the current application instance
        /// </summary>
        public Map Map
        {
            get
            {
                if (DesignerProperties.IsInDesignTool)
                    return null;
                checkApplicationContext();
                return _appHost.Map;
            }
        }

        /// <summary>
        /// Gets whether the application is in edit mode.  If in edit mode, the application's configuration 
        /// (tools, behaviors, layers, etc) can be modified.
        /// </summary>
        public bool IsEditMode
        {
            get
            {
                if (DesignerProperties.IsInDesignTool)
                    return false;
                checkApplicationContext();
                return _appHost.IsEditMode;
            }
        }

        /// <summary>
        /// Gets the URL-accessible endpoints that are used by the application
        /// </summary>
        public ApplicationUrls Urls
        {
            get
            {
                if (DesignerProperties.IsInDesignTool)
                    return null;
                checkApplicationContext();
                return _appHost.Urls;
            }
        }

        /// <summary>
        /// Identifies the <see cref="MapApplication.Portal"/> DependencyProperty
        /// </summary>
        public static DependencyProperty PortalProperty = DependencyProperty.Register(
                "PortalProperty", typeof(ArcGISPortal), typeof(MapApplication), null);

        /// <summary>
        /// Gets the ArcGIS portal endpoint used by the application
        /// </summary>
        public ArcGISPortal Portal 
        {
            get
            {
                if (DesignerProperties.IsInDesignTool)
                    return null;
                checkApplicationContext();
                return _appHost.Portal;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Internal use only.  Establishes the application host object.
        /// </summary>
        /// <param name="applicationHost">Class instance which implements interface for administrative methods.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetApplication(IMapApplication applicationHost)
        {
            if (_applicationContext == null)
            {
                lock (lockObject)
                {
                    if (_applicationContext == null)
                    {
                        _applicationContext = new MapApplication();
                    }
                }
            }

            _appHost = applicationHost;

            FrameworkElement appHostElement = applicationHost as FrameworkElement;
            if (appHostElement != null)
                registerForNotification("SelectedLayer", appHostElement, SelectedLayerPropertyChanged);
        }

        /// <summary>
        /// Displays content in an application window
        /// </summary>
        /// <param name="windowTitle">Title of the window</param>
        /// <param name="windowContents">Contents to be displayed in the window</param>        
        /// <param name="isModal">Determines wheter the window is modal or not</param>
        /// <param name="onHidingHandler">Event handler invoked when window is being hidden, and can be handled to cancel window closure</param>
        /// <param name="onHideHandler">Event handler invoked when the window is hidden</param>        
        /// <param name="windowType">Determines the type of the window</param>        
        /// <param name="top">The distance from the top of the application at which to position the window</param>
        /// <param name="left">The distance from the left of the application at which to position the window</param>
        /// <returns>The window</returns>
        public object ShowWindow(string windowTitle, FrameworkElement windowContents, bool isModal = false, 
            EventHandler<CancelEventArgs> onHidingHandler = null, EventHandler onHideHandler = null, 
            WindowType windowType = WindowType.Floating, double? top = null, double? left = null)
        {
            checkApplicationContext();
            return _appHost.ShowWindow(windowTitle, windowContents, isModal, onHidingHandler, onHideHandler, 
                windowType, top, left);
        }

        /// <summary>
        /// Hides the application window hosting the given content
        /// </summary>
        /// <param name="windowContents">Element previously displayed in a window using ShowWindow</param>
        public void HideWindow(FrameworkElement windowContents)
        {
            checkApplicationContext();
            _appHost.HideWindow(windowContents);
        }

        /// <summary>
        /// Finds a DependencyObject in the layout of the application
        /// </summary>
        /// <param name="objectName">The name of the object</param>
        /// <returns>The object, or null if not found</returns>
        public DependencyObject FindObjectInLayout(string objectName)
        {
            checkApplicationContext();
            return _appHost.FindObjectInLayout(objectName);
        }

        /// <summary>
        /// Resolves a relative URL to a fully-qualified application URL
        /// </summary>
        /// <param name="urlToBeResolved">The relative URL to be resolved</param>
        /// <returns>The resolved URL</returns>
        public Uri ResolveUrl(string urlToBeResolved)
        {
            if (_appHost != null)
                return _appHost.ResolveUrl(urlToBeResolved);
            Uri uri;
            if (Uri.TryCreate(urlToBeResolved, UriKind.RelativeOrAbsolute, out uri))
                return uri;
            return null;
        }

        /// <summary>
        /// Gets the popup info for a given feature
        /// </summary>
        /// <param name="graphic">The graphic representing the feature to retrieve popup info for</param>
        /// <param name="layer">The layer containing the feature to retrieve popup info for</param>
        /// <param name="layerId">The ID of the sub-layer containing the feature to retrieve popup info for</param>
        /// <returns>The feature's popup info</returns>
        public OnClickPopupInfo GetPopup(Graphic graphic, Layer layer, int? layerId = null)
        {
            checkApplicationContext();

            return _appHost.GetPopup(graphic, layer, layerId);
        }

        /// <summary>
        /// Display the popup for a given feature
        /// </summary>
        /// <param name="graphic">The graphic representing the feature to show the popup for</param>
        /// <param name="layer">The layer containing the feature to show the popup for</param>
        /// <param name="layerId">The ID of the sub-layer containing the feature to show the popup for</param>
        public void ShowPopup(Graphic graphic, Layer layer, int? layerId = null)
        {
            checkApplicationContext();

            _appHost.ShowPopup(graphic, layer, layerId);
        }

        /// <summary>
        /// Loads the layers from a map into the application
        /// </summary>
        /// <param name="map">The Map object to load layers from</param>
        /// <param name="callback">The method to fire once the map has been loaded</param>
        /// <param name="userToken">Object to pass through to the callback method</param>
        public void LoadMap(Map map, Action<LoadMapCompletedEventArgs> callback = null, object userToken = null)
        {
            checkApplicationContext();
            _appHost.LoadMap(map, callback, userToken);
        }

        /// <summary>
        /// Loads a web map into the application
        /// </summary>
        /// <param name="id">The id of the web map to load</param>
        /// <param name="document">The <see cref="Document"/> containing information about the web map to load</param>
        /// <param name="callback">The method to fire once the web map has been loaded</param>
        /// <param name="userToken">Object to pass through to the callback method</param>
        public void LoadWebMap(string id, Document document = null, Action<GetMapCompletedEventArgs> callback = null, object userToken = null)
        {
            checkApplicationContext();
            _appHost.LoadWebMap(id, document, callback, userToken);
        }

        #region Private Methods
        /// Listen for change of the dependency property  
        private static void registerForNotification(string propertyName, FrameworkElement element, PropertyChangedCallback callback)
        {
            if (element == null)
                return;

            //Bind to a depedency property  
            Binding b = new Binding(propertyName) { Source = element };
            var prop = System.Windows.DependencyProperty.RegisterAttached(
                propertyName + "ListenerProperty",
                typeof(object),
                typeof(DependencyObject),
                new System.Windows.PropertyMetadata(callback));

            element.SetBinding(prop, b);
        }

        private void checkApplicationContext()
        {
            if (_appHost == null)
                throw new Exception(Resources.Strings.ExceptionNoValidApplicationContextForApplication);
        }
        #endregion

        #endregion

        #region Events
        /// <summary>
        /// Occurs when the application's selected layer changes
        /// </summary>
        public event EventHandler SelectedLayerChanged
        {
            add
            {
                if (DesignerProperties.IsInDesignTool)
                    return;

                checkApplicationContext();
                _appHost.SelectedLayerChanged += value;
            }
            remove
            {
                if (DesignerProperties.IsInDesignTool)
                    return;

                checkApplicationContext();
                _appHost.SelectedLayerChanged -= value;
            }
        }

        /// <summary>
        /// Occurs when the application has been initialized
        /// </summary>
        public event EventHandler Initialized
        {
            add
            {
                if (DesignerProperties.IsInDesignTool)
                    return;

                checkApplicationContext();
                _appHost.Initialized += value;
            }
            remove
            {
                if (DesignerProperties.IsInDesignTool)
                    return;

                checkApplicationContext();
                _appHost.Initialized -= value;
            }
        }

        /// <summary>
        /// Occurs when application initialization fails
        /// </summary>
        public event EventHandler InitializationFailed
        {
            add
            {
                if (DesignerProperties.IsInDesignTool)
                    return;

                checkApplicationContext();
                _appHost.InitializationFailed += value;
            }
            remove
            {
                if (DesignerProperties.IsInDesignTool)
                    return;

                checkApplicationContext();
                _appHost.InitializationFailed -= value;
            }
        }
        #endregion

        #region Dependency Properties
        /// <summary>
        /// Identifies the <see cref="SelectedLayer"/> DependencyProperty
        /// </summary>
        public static DependencyProperty SelectedLayerProperty = DependencyProperty.Register(
                "SelectedLayerProperty", typeof(Layer), typeof(MapApplication), null);

        /// <summary>
        /// Gets or set the application's currently selected layer
        /// </summary>
        public Layer SelectedLayer
        {
            get { return GetValue(SelectedLayerProperty) as Layer; }
            set
            {
                SetValue(SelectedLayerProperty, value);

                checkApplicationContext();
                _appHost.SelectedLayer = value;
            }
        }

        // Fires when the view's SelectedLayer property changes
        private static void SelectedLayerPropertyChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (MapApplication.Current != null)
            {
                // Update the app's SelectedLayer with the new value
                MapApplication.Current.SetValue(SelectedLayerProperty, args.NewValue);
            }
        }

        /// <summary>
        /// Identifies the LayerName attached DependencyProperty
        /// </summary>
        public static readonly DependencyProperty LayerNameProperty =
            DependencyProperty.RegisterAttached("LayerName", typeof(string), typeof(Layer), null);

        /// <summary>
        /// Sets the display name of the given layer
        /// </summary>
        /// <param name="layer">The layer to set the name of</param>
        /// <param name="value">The layer name</param>
        public static void SetLayerName(Layer layer, string value)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            layer.SetValue(MapApplication.LayerNameProperty, value);
            if (string.IsNullOrWhiteSpace(layer.ID))
                layer.ID = value;
        }

        /// <summary>
        /// Gets the display name of the given layer
        /// </summary>
        /// <param name="layer">The layer to get the name of</param>
        /// <returns>The layer name</returns>
        public static string GetLayerName(Layer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            return (string)layer.GetValue(MapApplication.LayerNameProperty);
        }
        #endregion

        #region IApplicationAdmin Explicit Implementation
        /// <summary>
        /// Gets or sets the set of controls in the application that support configuration
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        ObservableCollection<FrameworkElement> IApplicationAdmin.ConfigurableControls
        {
            get
            {
                return ConfigurableControls;
            }
            set
            {
                ConfigurableControls = value;
            }
        }

        private ObservableCollection<FrameworkElement> ConfigurableControls;
        #endregion
    }
}
