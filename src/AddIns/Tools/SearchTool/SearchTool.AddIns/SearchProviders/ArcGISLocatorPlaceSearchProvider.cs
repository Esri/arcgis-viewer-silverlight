/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Projection;
using ESRI.ArcGIS.Client.Tasks;
using SearchTool.Resources;
using System.Collections.Specialized;

namespace SearchTool
{
    /// <summary>
    /// Provider for executing text searches for places against an ArcGIS Locator service endpoint
    /// </summary>
    [LocalizedDisplayName("Places")]
    [LocalizedDescription("SearchPlaces")]
    public class ArcGISLocatorPlaceSearchProvider : SearchProviderBase, ISupportsWizardConfiguration
    {
        #region Member Variables

        private LengthUnits _mapUnits; // The current map units
        private bool _gotMapUnits; // Whether map units have been retrieved for the current search
        private Map _map; // The map that will contain results
        private Queue<LocatorResultViewModel> _queuedResults = new Queue<LocatorResultViewModel>(); // Queue of results that are waiting to be added
        private double _extentWidthInMapUnits = 0;
        private GeometryService _geometryService;
        private Locator _locator; // The ArcGIS Locator service to search against
        private ObservableCollection<LocatorResultViewModel> _results =
            new ObservableCollection<LocatorResultViewModel>();
        private int _spatialRefWKID;
        private string _lastConfiguration = null; // last saved configuration
        private int _pageSize = 12; // number of results to show on each page

        // configuration pages
        private WizardPage _zoomExtentConfigPage; 
        private WizardPage _locatorConfigPage;
        
        #endregion

        #region Constructors

        public ArcGISLocatorPlaceSearchProvider(Map map) : base()
        {
            if (map == null)
                throw new ArgumentException();

            _map = map;
            _spatialRefWKID = _map.SpatialReference.WKID;

            // Determine map units
            _map.GetMapUnitsAsync(OnGetMapUnitsCompleted, OnGetMapUnitsFailed);

            // Hook to property changed to detect a change in map units
            _map.PropertyChanged += Map_PropertyChanged;

            // Initialize commands
            updateLocatorInfo = new DelegateCommand(doUpdateLocatorInfo, canUpdateLocatorInfo);

            // Initialize views
            ResultsView = new ArcGISLocatorResultsView();
            InputView = new SingleLineSearchInputView();

            _widthUnits = Utils.GetEnumDescriptions<LengthUnits>();
            if (_widthUnits.ContainsKey(LengthUnits.UnitsDecimalDegrees))
                _widthUnits.Remove(LengthUnits.UnitsDecimalDegrees);

            Results = _results;
            PagedResults = new PagedCollectionView(Results) { PageSize = _pageSize };
            ExtentFields = new ExtentFields();

            // Initialize geometry service and proxy URL with values from application environment if available
            if (MapApplication.Current != null)
            {
                // Bind the geometry service's URL to the environment's by default
                Binding b = new Binding("GeometryServiceUrl") { Source = MapApplication.Current.Urls  };
                BindingOperations.SetBinding(this, GeometryServiceUrlProperty, b);
            }

            // Initialize display name
            Properties.SetDisplayName(this, this.GetDisplayNameFromAttribute());

            // Listen for changes to the proxy URL
            Properties.NotifyOnDependencyPropertyChanged("ProxyUrl", this, OnProxyUrlChanged);
        }

        public ArcGISLocatorPlaceSearchProvider(Map map, string locatorUrl) : this(map)
        {
            LocatorServiceUrl = locatorUrl;

            // Retrieve locator service info
            if (UpdateLocatorInfo.CanExecute(null))
                UpdateLocatorInfo.Execute(null);
        }

        #endregion

        #region Public Properties

        #region Read/Write

        private string _locatorServiceUrl;
        /// <summary>
        /// Gets or sets the URL of the ArcGIS Locator service to use for executing the search
        /// </summary>
        public string LocatorServiceUrl
        {
            get { return _locatorServiceUrl; }
            set
            {
                if (_locatorServiceUrl != value)
                {
                    _locatorServiceUrl = value;

                    // Invalidate locator configuration page input
                    if (_locatorConfigPage != null)
                        _locatorConfigPage.InputValid = false;

                    OnPropertyChanged("LocatorServiceUrl");
                    updateLocatorInfo.RaiseCanExecuteChanged();
                }
            }
        }

        public override bool UseProxy
        {
            get { return base.UseProxy; }
            set 
            {
                bool proxyChanged = UseProxy != value;

                // Invalidate locator config page if use of proxy has changed
                if (proxyChanged && _locatorConfigPage != null)
                    _locatorConfigPage.InputValid = false;

                base.UseProxy = value;

                // Since use of proxy effectively changes locator URL, raise changed event for that property
                if (proxyChanged)
                    updateLocatorInfo.RaiseCanExecuteChanged();
            }
        }

        private List<string> _outfields = new List<string>(new string[] { "*" });
        /// <summary>
        /// Gets or sets the attribute fields to include with each result
        /// </summary>
        public List<string> OutFields
        {
            get { return _outfields; }
            set
            {
                if (_outfields != value)
                {
                    _outfields = value;
                    OnPropertyChanged("OutFields");
                }
            }
        }

        private double _extentWidth = 10000;
        /// <summary>
        /// Gets or sets the width of the extent to use for results.  Used only if extent fields 
        /// <see cref="UseExtentFields"/> is false.  Must be in the units of the map.
        /// </summary>
        public double ExtentWidth
        {
            get { return _extentWidth; }
            set
            {
                if (_extentWidth != value)
                {
                    _extentWidth = value;

                    // Validate input of result zoom extent configuration page
                    validateZoomExtentConfigPage();

                    // Update extent width in map units
                    _extentWidthInMapUnits = Utils.ConvertLength(_extentWidth, WidthUnit, _mapUnits);

                    OnPropertyChanged("ExtentWidth");

                    // Throw exception for validation.  Throw after updating property so validation that
                    // checks the property value can also work.
                    if (value <= 0)
                        throw new ArgumentException(Strings.ZeroExtentWidthError);
                }
            }
        }

        private LengthUnits _widthUnit = LengthUnits.UnitsMeters;
        /// <summary>
        /// Gets or sets the current unit for the extent width of results
        /// </summary>
        public LengthUnits WidthUnit
        {
            get { return _widthUnit; }
            set
            {
                if (_widthUnit != value)
                {
                    _widthUnit = value;
                    OnPropertyChanged("WidthUnit");
                }
            }
        }

        private bool _useExtentFields = true;
        /// <summary>
        /// Gets or sets whether to use fields to define the extent of each search result
        /// </summary>
        public bool UseExtentFields
        {
            get { return _useExtentFields; }
            set
            {
                if (_useExtentFields != value)
                {
                    _useExtentFields = value;

                    // validate input for result zoom extent configuration page
                    validateZoomExtentConfigPage();

                    OnPropertyChanged("UseExtentFields");
                }
            }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="GeometryServiceUrl"/> property
        /// </summary>
        public static readonly DependencyProperty GeometryServiceUrlProperty = DependencyProperty.Register(
            "GeometryServiceUrl", typeof(string), typeof(ArcGISLocatorPlaceSearchProvider),
            new PropertyMetadata("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer",
                OnGeometryServiceUrlChanged));

        /// <summary>
        /// Gets or sets the geometry service to use for re-projecting the extent of results, if necessary
        /// </summary>
        public string GeometryServiceUrl
        {
            get { return GetValue(GeometryServiceUrlProperty) as string; }
            set { SetValue(GeometryServiceUrlProperty, value); }
        }

        // Fires when the GeometryServiceUrl property changes
        private static void OnGeometryServiceUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ArcGISLocatorPlaceSearchProvider provider = (ArcGISLocatorPlaceSearchProvider)d;

            // Make sure the new value is not empty
            string url = e.NewValue as string;
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException(Strings.EmptyGeometryServiceUrl);

            // Push the new URL to the geometry service
            if (provider._geometryService != null)
                provider._geometryService.Url = url;

            provider.OnPropertyChanged("GeometryServiceUrl");
        }

        /// <summary>
        /// Gets whether locator info is currently being updated
        /// </summary>
        private bool _isUpdatingLocatorInfo;
        public virtual bool IsUpdatingLocatorInfo
        {
            get { return _isUpdatingLocatorInfo; }
            protected set
            {
                if (_isUpdatingLocatorInfo != value)
                {
                    _isUpdatingLocatorInfo = value;

                    OnPropertyChanged("IsUpdatingLocatorInfo");
                }
            }
        }

        #endregion

        #region Read Only

        private DelegateCommand updateLocatorInfo;
        /// <summary>
        /// Gets the command for updating locator info
        /// </summary>
        public ICommand UpdateLocatorInfo { get { return updateLocatorInfo; } }

        private ExtentFields extentFields;
        /// <summary>
        /// Gets the fields that specify the extent of each result
        /// </summary>
        public ExtentFields ExtentFields 
        {
            get { return extentFields; }
            private set
            {
                if (extentFields != value)
                {
                    if (extentFields != null)
                        extentFields.PropertyChanged -= ExtentFields_PropertyChanged;

                    extentFields = value;

                    // Validate input of result zoom extent configuration page
                    validateZoomExtentConfigPage();

                    // Wire up handler to re-validate when extent fields change
                    if (extentFields != null)
                        extentFields.PropertyChanged += ExtentFields_PropertyChanged;

                    OnPropertyChanged("ExtentFields");
                }
            }
        }

        private void ExtentFields_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            validateZoomExtentConfigPage();
        }

        private Dictionary<LengthUnits, string> _widthUnits;
        /// <summary>
        /// Gets the available units for extent width of results
        /// </summary>
        public Dictionary<LengthUnits, string> WidthUnits
        {
            get { return _widthUnits; }
        }

        private Type resultType = typeof(LocatorResultViewModel);
        /// <summary>
        /// Gets the type of the results returned by the search
        /// </summary>
        public Type ResultType
        {
            get { return resultType; }
        }

        private LocatorService _service;
        /// <summary>
        /// Gets the current locator service's metadata
        /// </summary>
        public LocatorService LocatorInfo
        {
            get { return _service; }
            private set
            {
                if (_service != value)
                {
                    _service = value;

                    // Update validation of locator configuration page
                    if (_locatorConfigPage != null)
                        _locatorConfigPage.InputValid = LocatorInfo != null;

                    OnPropertyChanged("LocatorInfo");
                }
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Raised when the provider's configuration is being saved
        /// </summary>
        public event EventHandler Saving;

        private void OnSaving()
        {
            if (Saving != null)
                Saving(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raised once the provider's configuration has been saved
        /// </summary>
        public event EventHandler Saved;

        private void OnSaved()
        {
            if (Saved != null)
                Saved(this, EventArgs.Empty);
        }

        #region Private Methods

        #region Command Execution 

        #region Search

        // Executes the search
        protected override void doSearch(object parameter)
        {
            base.doSearch(parameter);

            IsSearching = true; // Update busy state so new searches can't be initiated until the current one completes or fails
            _results.Clear(); // Clear results from previous searches
            _queuedResults.Clear();

            // Determine whether input has been passed directly to command or is from Input property
            string input = !string.IsNullOrEmpty(parameter as string) ? parameter as string : Input as string;

            // Trim whitespace from input
            string searchString = input.ToString().Trim();

            // Initialize parameters for place search
            AddressToLocationsParameters locationParameters = new AddressToLocationsParameters();

            // Map the input to the locator's single line address field.  This implementation only supports
            // single-line geocoding.
            locationParameters.Address.Add(LocatorInfo.SingleLineAddressField.Name, searchString);

            // Specify output fields
            if (OutFields.Contains("*"))
            {
                locationParameters.OutFields.Add("*");
            }
            else
            {
                foreach (string field in OutFields)
                    locationParameters.OutFields.Add(field);
            }

            // If extent fields are being used, make sure these are included in the output
            if (UseExtentFields && !locationParameters.OutFields.Contains("*"))
            {
                if (!locationParameters.OutFields.Contains(ExtentFields.XMinField.Name))
                    locationParameters.OutFields.Add(ExtentFields.XMinField.Name);
                if (!locationParameters.OutFields.Contains(ExtentFields.YMinField.Name))
                    locationParameters.OutFields.Add(ExtentFields.YMinField.Name);
                if (!locationParameters.OutFields.Contains(ExtentFields.XMaxField.Name))
                    locationParameters.OutFields.Add(ExtentFields.XMaxField.Name);
                if (!locationParameters.OutFields.Contains(ExtentFields.YMaxField.Name))
                    locationParameters.OutFields.Add(ExtentFields.YMaxField.Name);
            }

            locationParameters.OutSpatialReference = _map.SpatialReference;

            // Execute the search
            _locator.AddressToLocationsAsync(locationParameters);
        }

        // Gets whether a search can be executed given the search parameter and object state
        protected override bool canDoSearch(object parameter)
        {
            base.canDoSearch(parameter);

            // Determin whether input was passed directly to command or is coming from the Input property
            string input = !string.IsNullOrEmpty(parameter as string) ? parameter as string : Input as string;

            return input != null && !string.IsNullOrEmpty(input.ToString().Trim()) 
                && !IsSearching && !IsUpdatingLocatorInfo && LocatorInfo != null && !string.IsNullOrEmpty(LocatorInfo.Url)
                && LocatorInfo.SingleLineAddressField != null && 
                LocatorServiceUrl.ToLower() == LocatorInfo.Url.ToLower();
        }

        #endregion

        #region Cancel

        // Cancels the current search
        protected override void doCancel(object parameter)
        {
            base.doCancel(parameter);

            if (_locator.IsBusy)
                _locator.CancelAsync();

            IsSearching = false;
            OnSearchCompleted();
        }

        #endregion

        #region UpdateLocatorInfo

        // Gets locator metadata for the currently specified locator service URL
        private void doUpdateLocatorInfo(object parameter)
        {
            // Cancel search if one is in-progress
            if (IsSearching)
                doCancel(null);

            if (!(canUpdateLocatorInfo(parameter)))
                throw new Exception(Strings.CommandNotExecutable);

            LocatorInfo = null; // Reset locator info
            IsUpdatingLocatorInfo = true; // Update execution state
            _results.Clear(); // Clear results from previous searches
            _queuedResults.Clear();

            // Get proxy
            string proxyUrl = UseProxy ? ProxyUrl : null;

            // Make request for service metadata
            ArcGISService.GetServiceInfoAsync(LocatorServiceUrl, null, (o, e) =>
            {
                if (e.Service != null && e.Service is LocatorService)
                {
                    LocatorInfo = (LocatorService)e.Service;

                    // Initialize locator task
                    if (_locator == null)
                    {
                        _locator = new Locator();
                        _locator.AddressToLocationsCompleted += AddressToLocationsCompleted;
                        _locator.Failed += Locator_Failed;
                    }
                    _locator.Url = LocatorInfo.Url;
                    _locator.ProxyURL = UseProxy ? ProxyUrl : null;

                    // Try to auto-select fields to use for the extent of results
                    autoSelectExtentFields();

                    UseExtentFields = ExtentFields.XMinField != null && ExtentFields.YMinField != null
                        && ExtentFields.XMaxField != null && ExtentFields.YMaxField != null;
                }

                IsUpdatingLocatorInfo = false; // Update execution state
            }, proxyUrl);
        }

        // Checks whether locator metadata can be retrieved
        private bool canUpdateLocatorInfo(object parameter)
        {
            return !IsSearching && !IsUpdatingLocatorInfo && !string.IsNullOrEmpty(LocatorServiceUrl);
        }

        #endregion

        #endregion

        #region Event Handlers

        // Raised when the provider's proxy URL is changed
        private static void OnProxyUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // If a proxy is being used, re-retrieve locator info to make sure the updated proxy URL works
            ArcGISLocatorPlaceSearchProvider provider = (ArcGISLocatorPlaceSearchProvider)d;
            if (provider.UseProxy && provider.UpdateLocatorInfo.CanExecute(null))
                provider.UpdateLocatorInfo.Execute(null);
        }

        // Raised when the locator search fails
        private void Locator_Failed(object sender, TaskFailedEventArgs e)
        {
            IsSearching = false; // Reset busy state
            OnSearchFailed(e.Error); // Raise failed event
        }

        // Raised when the locator search completes
        private void AddressToLocationsCompleted(object sender, AddressToLocationsEventArgs args)
        {
            if (args.Results == null || args.Results.Count < 1) // No results found
            {
                IsSearching = false; // Reset busy state
                OnSearchCompleted(); // Raise completed event
                return;
            }

            // check that coordinates returned by extent fields are valid
            AddressCandidate result = args.Results[0];
            double xMin, xMax, yMin, yMax;
            bool useExtentFields = UseExtentFields;
            if (UseExtentFields && getExtentFields(result, out xMin, out yMin, out xMax, out yMax)) 
            {                
                if (xMax <= xMin || yMax <= yMin)
                {
                    // Extent fields are returning invalid coordinates.  Disable use of extent fields.
                    useExtentFields = false;
                }                
            }

            // Check whether result extents will need to be reprojected using a geometry service
            SpatialReference locatorSRef = LocatorInfo.SpatialReference;
            bool needsGeometryServiceProjection = (useExtentFields &&
                ((locatorSRef != null && !locatorSRef.IsWebMercator() && !locatorSRef.IsGeographic())
                || (!_map.SpatialReference.IsWebMercator() && !_map.SpatialReference.IsGeographic())))
                || (!useExtentFields && _mapUnits == LengthUnits.UnitsDecimalDegrees);
            List<Graphic> graphicsToProject = new List<Graphic>();
            List<LocatorResultViewModel> geographicResults = new List<LocatorResultViewModel>();

            foreach (AddressCandidate candidate in args.Results)
            {
                // Create the result ViewModel from the result returned from the service
                LocatorResultViewModel resultViewModel = new LocatorResultViewModel(candidate);

                // If extent fields are being used, initialize the extent on the service
                if (useExtentFields && getExtentFields(candidate, out xMin, out yMin, out xMax, out yMax))
                {
                    Envelope extent = new Envelope(xMin, yMin, xMax, yMax);
                    if (LocatorInfo.SpatialReference != null)
                    {
                        extent.SpatialReference = locatorSRef;
                        if (!needsGeometryServiceProjection)
                        {
                            // No geometry service needed, so set extent directly

                            if (locatorSRef.IsWebMercator() && _map.SpatialReference.WKID == 4326)
                                extent = (new WebMercator()).ToGeographic(extent) as Envelope;
                            else if (locatorSRef.WKID == 4326 && _map.SpatialReference.IsWebMercator())
                                extent = (new WebMercator()).FromGeographic(extent) as Envelope;

                            resultViewModel.Extent = extent;
                        }
                        else
                        {
                            // Since results need to be reprojected, add them to collection to be projected
                            // once they've all been gone through
                            graphicsToProject.Add(new Graphic() { Geometry = extent });
                            _queuedResults.Enqueue(resultViewModel);
                        }
                    }
                    else
                    {
                        resultViewModel.Extent = extent;
                    }

                }

                if (resultViewModel.Extent == null && !useExtentFields) 
                {
                    // Initialize the result extent based on the specified extent width

                    if (_gotMapUnits) // check whether map units have been retrieved for the current search
                    {
                        if (!needsGeometryServiceProjection)
                        {
                            initializeResultExtent(resultViewModel);
                            _results.Add(resultViewModel);
                        }
                        else if (_mapUnits != LengthUnits.UnitsDecimalDegrees)
                        {
                            initializeResultExtent(resultViewModel);
                            graphicsToProject.Add(new Graphic() { Geometry = resultViewModel.Extent });
                            _queuedResults.Enqueue(resultViewModel);
                        }
                        else
                        {
                            // results in a geographic coordinate system (units of decimal degrees) require
                            // special handling because an envelope around the result cannot be calculated
                            // in the result's spatial reference.

                            geographicResults.Add(resultViewModel);
                        }
                    }
                    else
                    {
                        // map units are not yet known, so queue up the result to be added after map units are
                        // determined.
                        _queuedResults.Enqueue(resultViewModel);
                    }
                }
                else if (!needsGeometryServiceProjection)
                {
                    // No projection needed, so the result can be added now
                    _results.Add(resultViewModel);
                }
            }

            if (!needsGeometryServiceProjection) // Check whether result extents need to be reprojected
            {
                // No projection needed, so the operation is complete. 

                // Refresh paged collection to update pagination
                PagedResults.Refresh();

                IsSearching = false; // Reset busy state
                OnSearchCompleted(); // Raise completed event
            }
            else if (graphicsToProject.Count > 0)
            {
                // result extents need to be reprojected
                if (_geometryService == null)
                {
                    _geometryService = new GeometryService(GeometryServiceUrl);
                    _geometryService.ProjectCompleted += GeometryService_ProjectCompleted;
                }

                _geometryService.ProjectAsync(graphicsToProject, _map.SpatialReference, _queuedResults);
            }
            else
            {
                // result extents need to be created in a spatial reference that uses linear units (i.e.
                // projected coordinate system), then projected back into the current spatial reference.
                handleGeographicResults(geographicResults);
            }
        }

        private bool getExtentFields(AddressCandidate candidate, out double xMin, out double yMin, out double xMax, out double yMax)
        {
            xMin = yMin = xMax = yMax = 0;
            if (candidate.Attributes.ContainsKey(ExtentFields.XMinField.Name)
            && double.TryParse(candidate.Attributes[ExtentFields.XMinField.Name].ToString(), out xMin))
            {
                if (candidate.Attributes.ContainsKey(ExtentFields.YMinField.Name)
                && double.TryParse(candidate.Attributes[ExtentFields.YMinField.Name].ToString(), out yMin))
                {
                    if (candidate.Attributes.ContainsKey(ExtentFields.XMaxField.Name)
                    && double.TryParse(candidate.Attributes[ExtentFields.XMaxField.Name].ToString(), out xMax))
                    {
                        if (candidate.Attributes.ContainsKey(ExtentFields.YMaxField.Name)
                        && double.TryParse(candidate.Attributes[ExtentFields.YMaxField.Name].ToString(), out yMax))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        // Raised once result extents have been reprojected
        private void GeometryService_ProjectCompleted(object sender, GraphicsEventArgs e)
        {
            Queue<LocatorResultViewModel> locatorResults = (Queue<LocatorResultViewModel>)e.UserState;

            // Update each locator result with the reprojected extent
            foreach (Graphic g in e.Results)
            {
                LocatorResultViewModel locatorResult = locatorResults.Dequeue();
                locatorResult.Extent = (Envelope)g.Geometry;

                // Add the result to the results set
                _results.Add(locatorResult);
            }

            // Refresh paged collection to update pagination
            PagedResults.Refresh();

            IsSearching = false; // Reset busy state
            OnSearchCompleted(); // Raise completed event
        }

        // Raised when a property of the map changes.  Used to check whether map units have changed.
        private void Map_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Map map = ((Map)sender);

            if (map.SpatialReference.WKID != _spatialRefWKID)
            {
                _gotMapUnits = false;
                _spatialRefWKID = map.SpatialReference.WKID;

                // Spatial reference has changed, so determine map units
                if (map.Layers.Count > 0)
                {
                    map.GetMapUnitsAsync(OnGetMapUnitsCompleted, OnGetMapUnitsFailed);
                }
                else
                {
                    NotifyCollectionChangedEventHandler collectionChanged = null;
                    collectionChanged = (o, args) =>
                    {
                        if (map.Layers.Count > 0)
                        {
                            map.Layers.CollectionChanged -= collectionChanged;
                            map.GetMapUnitsAsync(OnGetMapUnitsCompleted, OnGetMapUnitsFailed);
                        }
                    };
                    map.Layers.CollectionChanged += collectionChanged;
                }

                // handle case where map's spatial reference has changed while there are results.  Projection
                // of results will need to be changed to match that of the map.
                if (_results.Count > 0)                
                {
                    SpatialReference oldSRef = new SpatialReference(_spatialRefWKID);
                    if (oldSRef.IsWebMercator() && map.SpatialReference.IsGeographic())
                    {
                        // Transform result extents from Web Mercator to Geographic WGS 84
                        WebMercator mercator = new WebMercator();
                        foreach (LocatorResultViewModel result in _results)
                        {
                            result.Extent = (Envelope)mercator.ToGeographic(result.Extent);
                            AddressCandidate oldCandidate = result.Candidate;
                            MapPoint newLocation = (MapPoint)mercator.ToGeographic(result.Candidate.Location);
                            result.Candidate = new AddressCandidate(oldCandidate.Address, newLocation,
                                oldCandidate.Score, oldCandidate.Attributes);
                        }
                    }
                    else if (oldSRef.IsGeographic() && map.SpatialReference.IsWebMercator())
                    {
                        // Transform result exstents from Geographic WGS 84 to Web Mercator
                        WebMercator mercator = new WebMercator();
                        foreach (LocatorResultViewModel result in _results)
                        {
                            result.Extent = (Envelope)mercator.FromGeographic(result.Extent);
                            AddressCandidate oldCandidate = result.Candidate;
                            MapPoint newLocation = (MapPoint)mercator.FromGeographic(result.Candidate.Location);
                            result.Candidate = new AddressCandidate(oldCandidate.Address, newLocation,
                                oldCandidate.Score, oldCandidate.Attributes);
                        }
                    }
                    else if (!string.IsNullOrEmpty(GeometryServiceUrl))
                    {
                        // Use a geometry service to project the result extents                        

                        List<LocatorResultViewModel> resultsToProject = new List<LocatorResultViewModel>();
                        List<Graphic> extentsToProject = new List<Graphic>();
                        List<Graphic> locationsToProject = new List<Graphic>();
                        foreach (LocatorResultViewModel result in _results)
                        {
                            // Copy the results to a new collection (list) - necessary because the results
                            // could change during the project operation, so maintaining these in a separate
                            // collection ensures that the updated extents are applied to the proper results
                            resultsToProject.Add(result);
                            // Get the geometries to project.  Both extent and candidate location must be re-projected.
                            extentsToProject.Add(new Graphic() { Geometry = result.Extent });
                            locationsToProject.Add(new Graphic() { Geometry = result.Candidate.Location });
                        }

                        GeometryService geomService = new GeometryService(GeometryServiceUrl);
                        GeometryService geomService2 = new GeometryService(GeometryServiceUrl);
                        EventHandler<GraphicsEventArgs> projectExtentsCompleted = null;
                        EventHandler<GraphicsEventArgs> projectLocationsCompleted = null;
                        EventHandler<TaskFailedEventArgs> projectFailed = null;

                        // Update the result extents when the projection completes
                        projectExtentsCompleted = (o, args) =>
                        {
                            geomService.ProjectCompleted -= projectExtentsCompleted;
                            geomService.Failed -= projectFailed;

                            int count = args.Results.Count;
                            for (int i = 0; i < count; i++)
                                resultsToProject[i].Extent = (Envelope)args.Results[i].Geometry;
                        };

                        // Update the result locations when the projection completes
                        projectLocationsCompleted = (o, args) =>
                        {
                            geomService2.ProjectCompleted -= projectLocationsCompleted;
                            geomService2.Failed -= projectFailed;

                            int count = args.Results.Count;
                            LocatorResultViewModel result = null;
                            for (int i = 0; i < count; i++)
                            {
                                result = resultsToProject[i];
                                AddressCandidate oldCandidate = result.Candidate;
                                MapPoint newLocation = (MapPoint)args.Results[i].Geometry;
                                result.Candidate = new AddressCandidate(oldCandidate.Address, newLocation,
                                    oldCandidate.Score, oldCandidate.Attributes);
                            }
                        };

                        // Just clear the results and remove handlers if the projection fails
                        projectFailed = (o, args) =>
                        {
                            geomService.ProjectCompleted -= projectExtentsCompleted;
                            geomService.Failed -= projectFailed;

                            geomService2.ProjectCompleted -= projectLocationsCompleted;
                            geomService2.Failed -= projectFailed;

                            _results.Clear();
                        };

                        geomService.ProjectCompleted += projectExtentsCompleted;
                        geomService.Failed += projectFailed;

                        geomService2.ProjectCompleted += projectLocationsCompleted;
                        geomService2.Failed += projectFailed;

                        geomService.ProjectAsync(extentsToProject, map.SpatialReference);
                        geomService2.ProjectAsync(locationsToProject, map.SpatialReference);
                    }
                }
            }
        }

        // Called when map units are retrieved
        private void OnGetMapUnitsCompleted(LengthUnits units)
        {
            // Update map units and calculate result extent width
            _mapUnits = units;
            if (_mapUnits == LengthUnits.UnitsDecimalDegrees) // calculate in meters if map units are decimal degrees
                _extentWidthInMapUnits = Utils.ConvertLength(ExtentWidth, WidthUnit, LengthUnits.UnitsMeters);
            else
                _extentWidthInMapUnits = Utils.ConvertLength(ExtentWidth, WidthUnit, _mapUnits);

            // set flag indicating retrieval has completed
            _gotMapUnits = true;

            // handle any results that were returned while map units were being retrieved
            if (_queuedResults.Count > 0)
            {
                LocatorResultViewModel result = _queuedResults.Dequeue();
                while (result != null)
                {
                    initializeResultExtent(result);
                    _results.Add(result);
                    result = _queuedResults.Dequeue();
                }
            }
        }

        // Raised when map units retrieval fails
        private void OnGetMapUnitsFailed()
        {
            // Assume map units are meters and handle results as such
            OnGetMapUnitsCompleted(LengthUnits.UnitsMeters);
        }

        #endregion

        #region ISupportsWizardConfiguration members

        private WizardPage currentPage;
        /// <summary>
        /// Gets or sets the current configuration page
        /// </summary>
        public WizardPage CurrentPage
        {
            get { return currentPage; }
            set
            {
                if (currentPage != value)
                    currentPage = value;
            }
        }

        private Size desiredSize = new Size(350, 250);
        /// <summary>
        /// Gets or sets the optimal size for the UI
        /// </summary>
        public Size DesiredSize
        {
            get { return desiredSize; }
            set
            {
                if (desiredSize != value)
                    desiredSize = value;
            }
        }

        /// <summary>
        /// Reverts the provider to the last saved configuration
        /// </summary>
        public void OnCancelled()
        {
            // Load last configuration
            LoadConfiguration(_lastConfiguration);
        }

        /// <summary>
        /// Notifies the provider that configuration has been completed
        /// </summary>
        public void OnCompleted()
        {
            // Update saved configuration
            _lastConfiguration = SaveConfiguration();
        }

        /// <summary>
        /// Notifies the provider that the configuration page is changing
        /// </summary>
        /// <returns>Boolean indicating whether the page change is permitted</returns>
        public bool PageChanging()
        {
            // No need to stop the page changing, so always return true
            return true;
        }

        private ObservableCollection<WizardPage> pages;
        /// <summary>
        /// Gets the wizard pages for configuring the search
        /// </summary>
        public ObservableCollection<WizardPage> Pages
        {
            get
            {
                if (pages == null)
                    pages = createConfigPages();
                return pages;
            }
            set
            {
                if (pages != value)
                    pages = value;
            }
        }

        #endregion

        #region ISupportsConfiguration Members

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Configure() 
        { 
            // No additional logic needed here
        }

        // Types that are serialized when configuration is saved
        private Type[] knownSerializationTypes = new Type[] 
            { typeof(LocatorService), typeof(ExtentFields), typeof(LengthUnits) };

        /// <summary>
        /// Initializes the search provider based on a configuration string
        /// </summary>
        public void LoadConfiguration(string configData)
        {
            // Store configuration string in case configuration needs to be reverted
            _lastConfiguration = configData;

            // Deserialize configuration
            Dictionary<string, object> configSettings =
                configData.DataContractDeserialize<Dictionary<string, object>>(knownSerializationTypes);
                        
            LocatorInfo = (LocatorService)configSettings["LocatorInfo"];
            UseExtentFields = (bool)configSettings["UseExtentFields"];
            
            if (UseExtentFields)
            {
                ExtentFields fields = (ExtentFields)configSettings["ExtentFields"];

                // The extent fields must be the same field instances contained in the locator info's candidate 
                // fields collection.  The deserialization creates new fields instances, so a lookup is required
                // to initialize ExtentFields with the field instances from LocatorInfo.CandidateFields
                ExtentFields = new ExtentFields();
                ExtentFields.XMinField = findCandidateField(fields.XMinField);
                ExtentFields.XMaxField = findCandidateField(fields.XMaxField);
                ExtentFields.YMinField = findCandidateField(fields.YMinField);
                ExtentFields.YMaxField = findCandidateField(fields.YMaxField);
            }
            else
            {
                ExtentWidth = (double)configSettings["ExtentWidth"];
            }

            WidthUnit = (LengthUnits)configSettings["WidthUnit"];

            // Initialize locator task
            if (_locator == null)
            {
                _locator = new Locator();
                _locator.AddressToLocationsCompleted += AddressToLocationsCompleted;
                _locator.Failed += Locator_Failed;
            }
            _locator.Url = LocatorServiceUrl = LocatorInfo.Url;

            // Initialize proxy
            UseProxy = (bool)configSettings["UseProxy"];
            _locator.ProxyURL = UseProxy ? ProxyUrl : null;
        }

        /// <summary>
        /// Serializes search provider configuration into a string
        /// </summary>
        public string SaveConfiguration()
        {
            // Raise Saving event
            OnSaving();

            // Add key settings to a dictionary
            Dictionary<string, object> configSettings = new Dictionary<string, object>();
            configSettings.Add("LocatorInfo", LocatorInfo);
            configSettings.Add("UseExtentFields", UseExtentFields);
            if (UseExtentFields)
                configSettings.Add("ExtentFields", ExtentFields);
            else
                configSettings.Add("ExtentWidth", ExtentWidth);

            configSettings.Add("WidthUnit", WidthUnit);

            configSettings.Add("UseProxy", UseProxy);

            // Raise Saved event
            Dispatcher.BeginInvoke(() => { OnSaved(); });

            // Serialize
            return configSettings.DataContractSerialize(knownSerializationTypes);
        }

        #endregion

        #region Utility Methods

        // Generates wizard pages for configuring the search provider
        private ObservableCollection<WizardPage> createConfigPages()
        {
            ObservableCollection<WizardPage> configPages = new ObservableCollection<WizardPage>();

            // Create page for selecting locator service
            LocatorConfigView configView = new LocatorConfigView()
            {
                Margin = new Thickness(10, 13, 10, 0),
                DataContext = this
            };

            _locatorConfigPage = new WizardPage()
            {
                Content = configView,
                InputValid = LocatorInfo != null
            };

            // Setup binding for page heading
            bindDisplayNameToHeading(_locatorConfigPage, "ConfigureLocatorService");

            // Create page for specifying how to zoom to results
            ResultZoomExtentConfigView zoomExtentConfigView = new ResultZoomExtentConfigView() 
            { 
                Margin = new Thickness(10, 17, 10, 5),
                DataContext = this
            };

            _zoomExtentConfigPage = new WizardPage()
            {
                Content = zoomExtentConfigView
            };
            validateZoomExtentConfigPage();

            // Setup binding for page heading
            bindDisplayNameToHeading(_zoomExtentConfigPage, "ConfigureResultExtent");


            // Add pages
            configPages.Add(_locatorConfigPage);
            configPages.Add(_zoomExtentConfigPage);

            return configPages;
        }

        /// <summary>
        /// Binds the providers display name into the heading of the wizard page, using
        /// a resource string to determine the text surrounding the display name
        /// </summary>
        private void bindDisplayNameToHeading(WizardPage page, string resourceName)
        {
            // Binding XAML that will plug the display name (local:Properties.DisplayName) into
            // a string resource that has a format parameter 
            string xaml = string.Format(
                @"<Binding xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                            xmlns:local=""clr-namespace:SearchTool;assembly=SearchTool.AddIns""        
                            Path=""(local:Properties.DisplayName)"" ConverterParameter=""{0}"">
                                <Binding.Converter>
                                    <local:FormatResourceConverter />
                                </Binding.Converter>
                            </Binding>", resourceName);

            Binding b = XamlReader.Load(xaml) as Binding;
            b.Source = this;

            // Plug the binding into the wizard page's heading
            BindingOperations.SetBinding(page, WizardPage.HeadingProperty, b);
        }

        // Initializes the extent of a locator result based on the specified result extent width
        private void initializeResultExtent(LocatorResultViewModel result)
        {
            double extentHeight = _extentWidthInMapUnits / 3;
            result.Extent = result.Candidate.Location.ToEnvelope(_extentWidthInMapUnits, extentHeight);
        }

        private void handleGeographicResults(List<LocatorResultViewModel> results)
        {
            WebMercator mercator = new WebMercator();
            MapPoint extentCenter = null;
            double extentHeight = _extentWidthInMapUnits / 3;

            if (results[0].Candidate.Location.SpatialReference.IsGeographic())
            {
                foreach (LocatorResultViewModel result in results)
                {
                    extentCenter = (MapPoint)mercator.FromGeographic(result.Candidate.Location);
                    Envelope extentInMeters = extentCenter.ToEnvelope(_extentWidthInMapUnits, extentHeight);
                    result.Extent = (Envelope)mercator.ToGeographic(extentInMeters);
                    _results.Add(result);
                }

                // Refresh paged collection to update pagination
                PagedResults.Refresh();

                IsSearching = false; // Reset busy state
                OnSearchCompleted(); // Raise completed event
            }
            else if (!string.IsNullOrEmpty(GeometryServiceUrl))
            {
                GeometryService geomService = new GeometryService(GeometryServiceUrl);
                List<Graphic> graphicsToProject = new List<Graphic>();
                foreach (LocatorResultViewModel result in results)
                    graphicsToProject.Add(new Graphic() { Geometry = result.Candidate.Location });

                EventHandler<GraphicsEventArgs> projectCompleted = null;
                EventHandler<TaskFailedEventArgs> projectFailed = null;
                projectCompleted = (o, e) =>
                {
                    geomService.ProjectCompleted -= projectCompleted;
                    graphicsToProject.Clear();

                    foreach (Graphic g in e.Results)
                    {
                        extentCenter = (MapPoint)g.Geometry;
                        Envelope extentInMeters = extentCenter.ToEnvelope(_extentWidthInMapUnits, extentHeight);
                        graphicsToProject.Add(new Graphic() { Geometry = extentInMeters });
                    }

                    projectCompleted = (s, a) =>
                    {
                        geomService.ProjectCompleted -= projectCompleted;
                        geomService.Failed -= projectFailed;

                        for (int i = 0; i < a.Results.Count; i++)
                        {
                            LocatorResultViewModel result = results[i];
                            result.Extent = (Envelope)a.Results[i].Geometry;
                            _results.Add(result);
                        }

                        // Refresh paged collection to update pagination
                        PagedResults.Refresh();

                        IsSearching = false; // Reset busy state
                        OnSearchCompleted(); // Raise completed event
                    };

                    geomService.ProjectCompleted += projectCompleted;

                    // Project extents into map spatial reference
                    geomService.ProjectAsync(graphicsToProject, _map.SpatialReference);
                };

                projectFailed = (o, e) =>
                {
                    geomService.ProjectCompleted -= projectCompleted;
                    geomService.Failed -= projectFailed;

                    // Refresh paged collection to update pagination
                    PagedResults.Refresh();

                    IsSearching = false; // Reset busy state
                    OnSearchCompleted(); // Raise completed event
                };

                geomService.ProjectCompleted += projectCompleted;
                geomService.Failed += projectFailed;

                // Project result locations to web mercator
                geomService.ProjectAsync(graphicsToProject, new SpatialReference(3857));
            }
        }

        // Retrieves the specified field from the current locator service's candidate fields
        private Field findCandidateField(Field field)
        {
            return LocatorInfo.CandidateFields.FirstOrDefault(f => f.Name == field.Name);
        }

        // Checks whether the current input is valid on the result zoom extent configuration page
        private void validateZoomExtentConfigPage()
        {
            if (_zoomExtentConfigPage == null)
                return;

            if (UseExtentFields && ExtentFields != null && ExtentFields.XMinField != null
            && ExtentFields.YMinField != null && ExtentFields.XMaxField != null && ExtentFields.YMaxField != null
            && !anyMatch(ExtentFields.XMinField, ExtentFields.YMinField, ExtentFields.XMaxField, ExtentFields.YMaxField))
                _zoomExtentConfigPage.InputValid = true;
            else if (!UseExtentFields && ExtentWidth > 0)
                _zoomExtentConfigPage.InputValid = true;
            else
                _zoomExtentConfigPage.InputValid = false;
        }

        // Checks whether any two objects within the passed-in array are equal.  
        // Returns true if so, otherwise returns false.
        private bool anyMatch(params object[] compare)
        {
            for (int i = 0; i < compare.Length; i++)
            {
                object current = compare[i];
                for (int j = i + 1; j < compare.Length; j++)
                    if (current == compare[j]) return true;
            }

            return false;
        }

        // Auto-initializes extent fields based on well-known field names
        private void autoSelectExtentFields()
        {
            string[] xMinMatchStrings = { "xmin", "minx", "x_min", "min_x", "west_lon", "westlon", 
                "west_longitude", "westlongitude", Strings.Left.Replace(":", "").ToLower(), "left" };

            string[] xMaxMatchStrings = { "xmax", "maxx", "x_max", "max_x", "east_lon", "eastlon", 
                "east_longitude", "eastlongitude", Strings.Right.Replace(":", "").ToLower(), "right" };

            string[] yMinMatchStrings = { "ymin", "miny", "y_min", "min_y", "south_lat", "southlat", 
                "south_latitude", "southlatitude", Strings.Bottom.Replace(":", "").ToLower(), "bottom" };

            string[] yMaxMatchStrings = { "ymax", "maxy", "y_max", "max_y", "north_lat", "northlat", 
                "north_latitude", "northlatitude", Strings.Top.Replace(":", "").ToLower(), "top" };

            ExtentFields.XMinField = LocatorInfo.CandidateFields.FirstOrDefault(f => xMinMatchStrings.Contains(f.Name.ToLower()));
            ExtentFields.XMaxField = LocatorInfo.CandidateFields.FirstOrDefault(f => xMaxMatchStrings.Contains(f.Name.ToLower()));
            ExtentFields.YMinField = LocatorInfo.CandidateFields.FirstOrDefault(f => yMinMatchStrings.Contains(f.Name.ToLower()));
            ExtentFields.YMaxField = LocatorInfo.CandidateFields.FirstOrDefault(f => yMaxMatchStrings.Contains(f.Name.ToLower()));
        }

        #endregion

        #endregion
    }
}
