/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Portal;
using ESRI.ArcGIS.Client.Tasks;
using SearchTool.Resources;

namespace SearchTool
{
    /// <summary>
    /// Supports displaying and interacting with search results from the web or ArcGIS Portal
    /// </summary>
    public class SearchResultViewModel : INotifyPropertyChanged
    {
        Layer _layer;

        public SearchResultViewModel() { }

        public SearchResultViewModel(object result)
        {
            if (!(result is ArcGISPortalItem) && !(result is WebSearchResultItem) && !(result is ArcGISService))
                throw new NotSupportedException(Strings.InvalidSearchResultType);

            Result = result;
        }

        private dynamic result;
        /// <summary>
        /// Gets or sets the search result 
        /// </summary>
        public dynamic Result 
        {
            get { return result; }
            set
            {
                if (!(value is ArcGISPortalItem) && !(value is WebSearchResultItem) && !(value is ArcGISService))
                    throw new NotSupportedException(Strings.InvalidSearchResultType);
                if (value != result)
                {
                    result = value;
                    OnPropertyChanged("Result");

                    Layer = null;
                    Service = null;
                    IsInitialized = false;
                }
            }
        }

        private ArcGISService _service;
        /// <summary>
        /// Gets the service referenced by the search result
        /// </summary>
        public ArcGISService Service 
        {
            get { return _service; }
            private set
            {
                if (_service != value)
                {
                    _service = value;
                    OnPropertyChanged("Service");
                }
            }
        }

        /// <summary>
        /// Gets the layer referenced by the search result.
        /// </summary>
        public Layer Layer
        {
            get { return _layer; }
            private set
            {
                if (_layer != value)
                {
                    _layer = value;
                    OnPropertyChanged("Layer");
                }
            }
        }

        /// <summary>
        /// Gets the thumbnail for the search result.
        /// </summary>
        public ImageSource Thumbnail
        {
            get
            {
                if (Result is ArcGISPortalItem)
                    return new BitmapImage(Result.ThumbnailUri);
                else if (Result is FeatureService)
                    return new BitmapImage(new Uri("/SearchTool.AddIns;component/Images/featureService.png", UriKind.Relative));
                else if (Result is ImageService)
                    return new BitmapImage(new Uri("/SearchTool.AddIns;component/Images/imageService.png", UriKind.Relative));
                else
                    return new BitmapImage(new Uri("/SearchTool.AddIns;component/Images/mapService.png", UriKind.Relative));
            }
        }

        private bool _isInitialized;
        /// <summary>
        /// Gets whether the search result's <see cref="Service"/> or <see cref="Layer"/> is initialized
        /// </summary>
        public bool IsInitialized
        {
            get { return _isInitialized; }
            private set
            {
                if (_isInitialized != value)
                {
                    _isInitialized = value;
                    OnPropertyChanged("IsInitialized");
                }
            }
        }

        private bool _isInitializing;
        /// <summary>
        /// Gets whether the search result's <see cref="Service"/> or <see cref="Layer"/> is initializing
        /// </summary>
        public bool IsInitializing
        {
            get { return _isInitializing; }
            private set
            {
                if (_isInitializing != value)
                {
                    _isInitializing = value;
                    OnPropertyChanged("IsInitializing");
                }
            }
        }

        private bool _InitializationError;
        /// <summary>
        /// Gets whether there was an error initializing the search result
        /// </summary>
        public bool InitializationError
        {
            get { return _InitializationError; }
            private set
            {
                if (_InitializationError != value)
                {
                    _InitializationError = value;
                    OnPropertyChanged("InitializationError");
                }
            }
        }

        private string _proxyUrl = null;
        /// <summary>
        /// Gets or sets the proxy URL to use for communicating with the result's service
        /// </summary>
        public virtual string ProxyUrl
        {
            get { return _proxyUrl; }
            set
            {
                if (_proxyUrl != value)
                {
                    _proxyUrl = value;
                    OnPropertyChanged("ProxyUrl");
                }
            }
        }

        /// <summary>
        /// Initializes the search result.  This will populate the <see cref="Service"/> and <see cref="Layer"/>
        /// properties
        /// </summary>
        public void Initialize()
        {
            IsInitializing = true;
            initializeService(() =>
            {
                if (Service != null)
                {
                    initializeLayer();
                    IsInitialized = true;
                    OnInitialized();
                }
                else
                {
                    IsInitialized = false;
                    InitializationError = true;
                    OnInitializationFailed();
                }
                IsInitializing = false;
            });
        }

        /// <summary>
        /// Raised when a property on the ViewModel changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        // Fires the PropertyChanged event
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        /// <summary>
        /// Raised when the search result's <see cref="Service"/> and <see cref="Layer"/> has been initialized
        /// </summary>
        public event EventHandler Initialized;

        // Fires the Initialized event
        private void OnInitialized()
        {
            if (Initialized != null)
                Initialized(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raised when initialization fails
        /// </summary>
        public event EventHandler InitializationFailed;

        // Fires the InitializationFailed event
        private void OnInitializationFailed()
        {
            if (InitializationFailed != null)
                InitializationFailed(this, EventArgs.Empty);
        }

        // Initializes the Service property
        private void initializeService(Action callback)
        {
            if (Result is ArcGISService)
            {
                Service = Result as ArcGISService; // Assume service reference is valid
                callback();
            }
            else if (Result is ArcGISPortalItem)
            {
                ArcGISService.GetServiceInfoAsync((string)Result.Url, null, (o, e) =>
                {
                    Service = e.Service;
                    callback();
                });
            }
        }

        // Initializes the Layer property
        private void initializeLayer()
        {
            Layer layer = null;

            // Create the layer based on the type of service encapsulated by the search result
            if (Service is MapService)
            {
                MapService mapService = (MapService)Service;
                if (mapService.IsTiled &&
                mapService.SpatialReference.WKID == MapApplication.Current.Map.SpatialReference.WKID)
                    layer = new ArcGISTiledMapServiceLayer() { Url = Service.Url, ProxyURL = ProxyUrl };
                else
                    layer = new ArcGISDynamicMapServiceLayer() { Url = Service.Url, ProxyURL = ProxyUrl };
            }
            else if (Service is ImageService)
            {
                layer = new ArcGISImageServiceLayer() { Url = Service.Url, ProxyURL = ProxyUrl };
            }
            else if (Service is FeatureLayerService || (Service is FeatureService && ((FeatureService)Service).Layers.Count() == 1))
            {
                string url = Service is FeatureService ? string.Format("{0}/0", Service.Url) : Service.Url;
                layer = new FeatureLayer() 
                { 
                    Url = url, 
                    ProxyUrl = ProxyUrl,
                    OutFields = new OutFields() { "*" }
                };
            }

            // Initialize the layer's ID and display name
            if (layer != null)
            {
                string id = Guid.NewGuid().ToString("N");
                string name = propertyExists("Title") ? Result.Title : id;

                layer.ID = id;
                MapApplication.SetLayerName(layer, name);
            }
            Layer = layer;
        }

        // Gets whether a property exists on the search result
        private bool propertyExists(string propertyName)
        {
            return Result != null && ((object)Result).PropertyExists(propertyName);
        }

    }
}
