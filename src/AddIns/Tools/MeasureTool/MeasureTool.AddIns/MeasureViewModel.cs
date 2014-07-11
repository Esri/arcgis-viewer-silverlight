/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Provides logic for supporting UI to measure user-drawn shapes and selected features
    /// </summary>
    public class MeasureViewModel : INotifyPropertyChanged
    {
        public MeasureViewModel(Map map, string geometryServiceUrl)
        {
            _linearUnits = Utils.GetEnumDescriptions<LengthUnits>();
            _arealUnits = Utils.GetEnumDescriptions<AreaUnits>();

            LinearUnit = LengthUnits.UnitsMeters;
            ArealUnit = AreaUnits.UnitsSquareMeters;

            Map = map;
            map.Layers.CollectionChanged += Layers_CollectionChanged;
            map.ExtentChanged += Map_ExtentChanged;

            GeometryServiceUrl = geometryServiceUrl;

            DrawLayer = new GraphicsLayer();

            // Initialize the draw object.  Hook to DrawComplete to update the ViewModel's
            // DrawGeometry.
            DrawObject = new Draw(Map);
            DrawObject.DrawComplete += DrawObject_DrawComplete;

            // Bind the FreehandDrawMode attached property on the Draw object to the ViewModel's
            // FreehandDrawMode property.  Pushes updates to the ViewModel's property through to
            // the attached property.
            Binding b = new Binding("FreehandDrawMode") { Source = this };
            BindingOperations.SetBinding(DrawObject, AttachedProperties.FreehandDrawModeProperty, b);

            // hook in for layer visibility changes
            foreach (Layer layer in map.Layers)
            {
                layer.PropertyChanged += Layer_PropertyChanged;

                if (layer is ArcGISDynamicMapServiceLayer)
                {
                    ISublayerVisibilitySupport subLayerSupport = layer as ISublayerVisibilitySupport;
                    subLayerSupport.VisibilityChanged += SubLayerSupport_VisibilityChanged;
                }
            }
        }

        #region Public Properties

        private Map _map;
        /// <summary>
        /// Gets or sets the <see cref="ESRI.ArcGIS.Client.Map"/> used by the ViewModel
        /// </summary>
        public Map Map
        {
            get { return _map; }
            private set
            {
                _map = value;
                OnPropertyChanged("Map");
            }
        }

        #region Measure feature and geometries

        private Graphic _currentGraphic;
        /// <summary>
        /// Gets or sets the <see cref="ESRI.ArcGIS.Client.Graphic"/> being measured
        /// </summary>
        public Graphic CurrentGraphic
        {
            get { return _currentGraphic; }
            set
            {
                _currentGraphic = value;
                OnPropertyChanged("CurrentGraphic");
            }
        }

        private Geometry _drawGeometry;
        /// <summary>
        /// Gets the user-drawn <see cref="ESRI.ArcGIS.Client.Geometry.Geometry"/>
        /// </summary>
        public Geometry DrawGeometry
        {
            get { return _drawGeometry; }
            set
            {
                if (_drawGeometry != value)
                {
                    _drawGeometry = value;
                    OnPropertyChanged("DrawGeometry");
                }
            }
        }

        private Geometry _measurableGeometry;
        /// <summary>
        /// Gets or sets the <see cref="ESRI.ArcGIS.Client.Geometry.Geometry"/> to be measured
        /// </summary>
        public Geometry MeasurableGeometry
        {
            get { return _measurableGeometry; }
            set
            {
                if (_measurableGeometry != value)
                {
                    _measurableGeometry = value;
                    OnPropertyChanged("MeasurableGeometry");
                }
            }
        }

        #endregion

        #region Measurements

        private double? _latitude;
        /// <summary>
        /// Gets or sets the latitude for the <see cref="CurrentGraphic"/>. Used when points are being measured.
        /// </summary>
        public double? Latitude
        {
            get { return _latitude; }
            set
            {
                _latitude = value;
                OnPropertyChanged("Latitude");

                CheckHasMeasurements();
            }
        }

        private double? _longitude;
        /// <summary>
        /// Gets or sets the longitude for the <see cref="CurrentGraphic"/>. Used when points are being measured.
        /// </summary>
        public double? Longitude
        {
            get { return _longitude; }
            set
            {
                _longitude = value;
                OnPropertyChanged("Longitude");

                CheckHasMeasurements();
            }
        }

        private double? _length;
        /// <summary>
        /// Gets or sets the linear value for the <see cref="CurrentGraphic"/>. Used when lines or polygons are 
        /// being measured.
        /// </summary>
        public double? Length
        {
            get { return _length; }
            set
            {
                _length = value;
                OnPropertyChanged("Length");

                // Update length in current units
                LengthInCurrentUnits = Utils.ConvertLength(Length, LinearUnit);

                CheckHasMeasurements();
            }
        }

        private double? _lengthInCurrentUnits;
        /// <summary>
        /// Gets the length or perimeter of the <see cref="CurrentGraphic"/> in the current <see cref="LinearUnit"/>. 
        /// </summary>
        public double? LengthInCurrentUnits
        {
            get { return _lengthInCurrentUnits; }
            private set
            {
                _lengthInCurrentUnits = value;
                OnPropertyChanged("LengthInCurrentUnits");
            }
        }

        private double? _area;
        /// <summary>
        /// Gets or sets the area of the <see cref="CurrentGraphic"/>.  Used when polygons are being measured.
        /// </summary>
        public double? Area
        {
            get { return _area; }
            set
            {
                _area = value;
                OnPropertyChanged("Area");

                // Update area in current units
                AreaInCurrentUnits = Utils.ConvertArea(Area, ArealUnit);

                CheckHasMeasurements();
            }
        }

        private double? _areaInCurrentUnits;
        /// <summary>
        /// Gets the area of the <see cref="CurrentGraphic"/> in the current <see cref="ArealUnit"/>
        /// </summary>
        public double? AreaInCurrentUnits
        {
            get { return _areaInCurrentUnits; }
            private set
            {
                _areaInCurrentUnits = value;
                OnPropertyChanged("AreaInCurrentUnits");
            }
        }

        #endregion

        #region Units

        private Dictionary<LengthUnits, string> _linearUnits;
        /// <summary>
        /// Gets the available units for measuring length
        /// </summary>
        public Dictionary<LengthUnits, string> LinearUnits
        {
            get { return _linearUnits; }
        }

        private Dictionary<AreaUnits, string> _arealUnits;
        /// <summary>
        /// Gets the available units for measuring area
        /// </summary>
        public Dictionary<AreaUnits, string> ArealUnits
        {
            get { return _arealUnits; }
        }

        private LengthUnits _linearUnit;
        /// <summary>
        /// Gets or sets the current unit for measuring length
        /// </summary>
        public LengthUnits LinearUnit
        {
            get { return _linearUnit; }
            set
            {
                _linearUnit = value;
                OnPropertyChanged("LinearUnit");

                // Update length in current units
                LengthInCurrentUnits = Utils.ConvertLength(Length, LinearUnit);
            }
        }

        private AreaUnits _arealUnit;
        /// <summary>
        /// Gets or sets the current unit for measuring area
        /// </summary>
        public AreaUnits ArealUnit
        {
            get { return _arealUnit; }
            set
            {
                _arealUnit = value;
                OnPropertyChanged("ArealUnit");

                // Update area in current units
                AreaInCurrentUnits = Utils.ConvertArea(Area, ArealUnit);
            }
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Gets the layer containing measure geometries
        /// </summary>
        public GraphicsLayer DrawLayer { get; private set; }

        private FreehandDrawMode _freehandDrawMode;
        /// <summary>
        /// Gets or sets the type of shape that will be drawn on the map for freehand drawing modes.
        /// </summary>
        public FreehandDrawMode FreehandDrawMode
        {
            get { return _freehandDrawMode; }
            set
            {
                _freehandDrawMode = value;
                OnPropertyChanged("FreehandDrawMode");
            }
        }

        private Draw _drawObject;
        /// <summary>
        /// Gets or sets the <see cref="ESRI.ArcGIS.Client.Draw"/> instance that manages drawing shapes on the map
        /// </summary>
        public Draw DrawObject
        {
            get
            {
                return _drawObject;
            }
            private set
            {
                _drawObject = value;
                OnPropertyChanged("DrawObject");
            }
        }

        #endregion

        private string _geometryServiceUrl;
        /// <summary>
        /// Gets the URL of a geometry service to use for geometric operations like projection and measuring
        /// </summary>
        public string GeometryServiceUrl
        {
            get { return _geometryServiceUrl; }
            private set
            {
                if (_geometryServiceUrl != value)
                {
                    _geometryServiceUrl = value;
                    OnPropertyChanged("GeometryServiceUrl");
                }
            }
        }

        #region ViewModel State

        private string _statusMessage;
        /// <summary>
        /// Gets or sets the current measure status
        /// </summary>
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged("StatusMessage");

                    // StatusMessage and Error are mutually exclusive. 
                    // If status is not null, make Error null.
                    if (_statusMessage != null)
                        Error = null;
                }
            }
        }

        private Exception _error;
        /// <summary>
        /// Gets or sets the current error
        /// </summary>
        public Exception Error
        {
            get { return _error; }
            set
            {
                if (_error != value)
                {
                    _error = value;
                    OnPropertyChanged("Error");

                    // Error and StatusMessage are mutually exclusive. 
                    // If error is not null, make status null.
                    if (_error != null)
                        StatusMessage = null;
                }
            }
        }

        private bool _retrievingFeatures;
        /// <summary>
        /// Gets or sets whether features are currently being retrieved
        /// </summary>
        public bool RetrievingFeatures
        {
            get { return _retrievingFeatures; }
            set
            {
                if (_retrievingFeatures != value)
                {
                    _retrievingFeatures = value;
                    OnPropertyChanged("RetrievingFeatures");

                    // Update ViewModel's busy state if features are being retrieved
                    if (_retrievingFeatures)
                        IsBusy = true;
                    else if (Error != null) // If there is an error, set busy state to false
                        IsBusy = false;
                }
            }
        }

        private bool _isBusy;
        /// <summary>
        /// Gets or sets whether an operation is processing
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged("IsBusy");
                }
            }
        }

        private bool _hasMeasurements;
        /// <summary>
        /// Gets whether measurements are available
        /// </summary>
        public bool HasMeasurements
        {
            get { return _hasMeasurements; }
            private set
            {
                if (_hasMeasurements != value)
                {
                    _hasMeasurements = value;
                    OnPropertyChanged("HasMeasurements");
                }
            }
        }

        #endregion

        #region Measure Layer

        private ObservableCollection<Layer> _measurableLayers = new ObservableCollection<Layer>();
        /// <summary>
        /// Gets the set of measurable layers in the current map
        /// </summary>
        public ObservableCollection<Layer> MeasurableLayers
        {
            get
            {
                return _measurableLayers;
            }
            private set
            {
                _measurableLayers = value;
                OnPropertyChanged("MeasurableLayers");
            }
        }

        private Layer _measureLayer;
        /// <summary>
        /// Gets or sets the layer containing features to be measured
        /// </summary>
        public Layer MeasureLayer
        {
            get
            {
                return _measureLayer;
            }
            set
            {
                _measureLayer = value;

                // Raise event before subsequent logic so property changes occur in cascading sequence
                OnPropertyChanged("MeasureLayer");

                // Re-initialize measurable sublayers and the currently selected sublayer to measure
                if (_measureLayer is ArcGISDynamicMapServiceLayer)
                {
                    MeasurableSubLayers = 
                        ((ArcGISDynamicMapServiceLayer)_measureLayer).GetMeasurableSubLayers(Map);
                    MeasureSubLayer = MeasurableSubLayers.Count > 0 ? MeasurableSubLayers[0] : null;
                }
                else
                {
                    // The new measure layer does not have sub-layers, so clear the collection
                    MeasurableSubLayers.Clear();
                    MeasureSubLayer = null;

                    // Update name of current measure layer
                    if (_measureLayer != null)
                        MeasureLayerName = MapApplication.GetLayerName(_measureLayer);
                    else
                        MeasureLayerName = null; // not measuring a layer, so clear out the name
                }
            }
        }

        private string _measureLayerName;
        /// <summary>
        /// Gets the name of the current measure layer
        /// </summary>
        public string MeasureLayerName
        {
            get { return _measureLayerName; }
            private set
            {
                _measureLayerName = value;
                OnPropertyChanged("MeasureLayerName");
            }
        }

        private ObservableCollection<LayerInfo> _measurableSubLayers = new ObservableCollection<LayerInfo>();
        /// <summary>
        /// Gets or sets the sub-layers (i.e. layers within a map service layer) in the map that are in a measurable 
        /// state
        /// </summary>
        public ObservableCollection<LayerInfo> MeasurableSubLayers
        {
            get
            {
                return _measurableSubLayers;
            }
            set
            {
                _measurableSubLayers = value;
                OnPropertyChanged("MeasurableSubLayers");
            }
        }

        private LayerInfo _measureSubLayer;
        public LayerInfo MeasureSubLayer
        {
            get { return _measureSubLayer; }
            set
            {
                _measureSubLayer = value;
                OnPropertyChanged("MeasureSubLayer");

                // Update name of layer to be measured if new value is not null
                if (_measureSubLayer != null)
                    MeasureLayerName = MeasureSubLayer.Name;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Clears all measurments from the ViewModel
        /// </summary>
        public void ResetMeasurements()
        {
            Latitude = null;
            Longitude = null;
            Length = null;
            Area = null;
            MeasurableGeometry = null;
        }

        #endregion

        #region Internal

        /// <summary>
        /// Performs cleanup logic such as unhooking event handlers.  Should be invoked when
        /// the <see cref="MeasureViewModel"/> instance is no longer in use.
        /// </summary>
        internal void Deactivate()
        {
            Map.Layers.CollectionChanged -= Layers_CollectionChanged;
            Map.ExtentChanged -= Map_ExtentChanged;
            DrawObject.DrawComplete -= DrawObject_DrawComplete;

            // unhook from visibility changes
            foreach (Layer layer in Map.Layers)
            {
                layer.PropertyChanged -= Layer_PropertyChanged;

                if (layer is ArcGISDynamicMapServiceLayer)
                {
                    ISublayerVisibilitySupport subLayerSupport = layer as ISublayerVisibilitySupport;
                    subLayerSupport.VisibilityChanged -= SubLayerSupport_VisibilityChanged;
                }
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Updates the <see cref="MeasurableLayers"/> property based on the layers in the map.  Will
        /// also update the <see cref="MeasureLayer"/> if the current one is in an unmeasurable state.
        /// </summary>
        private void UpdateMeasurableLayers()
        {
            List<Layer> layersToRemove = new List<Layer>();

            Layer layer;
            int insertIndex = 0;
            // Loop through the layers in the map.  Go in reverse order since the order in the collection
            // is from bottom to top, but the ViewModel's collection should be from top to bottom
            for (int i = Map.Layers.Count - 1; i > -1; i--)
            {
                layer = Map.Layers[i];
                
                // Check whether the layer is measurable
                if (layer.IsMeasurable(Map))
                {
                    // Insert the layer in the measurable layers collection if it is not there already
                    if (!MeasurableLayers.Contains(layer))
                        MeasurableLayers.Insert(insertIndex, layer);

                    // Increment the insertion index for the next layer.  We need to insert and not
                    // Add because sometimes layers will be removed from and need to be re-added to
                    // the middle of the collection.
                    insertIndex++;
                }
                else if (MeasurableLayers.Contains(layer)) 
                {
                    // Layer is not measurable, but it's in the measurable layers collection

                    // We are about to remove the layer from the measurable layers.  But before doing that,
                    // check the current insertion index against the layer's index.  We want to keep that
                    // index immediately after the same set of layers, so if it's greater than the index of
                    // the layer to be removed, it needs to be decremented.
                    if (insertIndex > MeasurableLayers.IndexOf(layer))
                        insertIndex--;

                    // Remove the layer.
                    layersToRemove.Add(layer);
                }
            }

            // Add any layers that are no longer in the map to the set of layers to remove
            foreach (Layer lyr in MeasurableLayers)
            {
                if (!Map.Layers.Contains(lyr))
                    layersToRemove.Add(lyr);
            }

            // Check whether the current measure layer is in a measurable state
            if (MeasureLayer != null && !MeasureLayer.IsMeasurable(Map))
            {
                // Layer is not measurable.  Change to the next measurable one.
                int index = MeasurableLayers.IndexOf(MeasureLayer);
                bool updated = false;

                // First look for a measurable layer after the current one
                for (int i = index + 1; i < MeasurableLayers.Count; i++)
                {
                    if (MeasurableLayers[i].IsMeasurable(Map))
                    {
                        MeasureLayer = MeasurableLayers[i];
                        updated = true;
                        break;
                    }
                }

                if (!updated)
                {
                    // No measurable found found after the current one, so look before it
                    for (int i = index - 1; i > -1; i--)
                    {
                        if (MeasurableLayers[i].IsMeasurable(Map))
                        {
                            MeasureLayer = MeasurableLayers[i];
                            break;
                        }
                    }
                }
            }

            // Remove the layers that have become unmeasurable
            foreach (Layer l in layersToRemove)
                MeasurableLayers.Remove(l);

            // Clear out measure layer if no measurable layers remain
            if (MeasurableLayers.Count == 0)
                MeasureLayer = null;
        }

        /// <summary>
        /// Updates the <see cref="MeasurableSubLayers"/> property based on the state of the sub-layers within
        /// the current <see cref="MeasureLayer"/>.  Will also update <see cref="MeasureSubLayer"/> if the 
        /// current one is in an unmeasurable state.
        /// </summary>
        private void UpdateMeasurableSubLayers()
        {
            // Dynamic map service layer containing sub-layers
            ArcGISDynamicMapServiceLayer parentLayer = (ArcGISDynamicMapServiceLayer)MeasureLayer;

            // index of the current measure sub-layer among the map service's sub-layers
            int indexInMapService = parentLayer.Layers.IndexOf(MeasureSubLayer);
            // index of the measure sub-layer among the set of measurable sub-layers
            int indexInMeasurableLayers = MeasurableSubLayers.IndexOf(MeasureSubLayer);
            // index to insert newly measurable sub-layers
            int insertIndex = indexInMeasurableLayers + 1;
            // tracks whether the current measure sub-layer has become unmeasurable
            bool needsMeasureLayerUpdate = !MeasureSubLayer.IsMeasurable(parentLayer, Map);
       
            // Iterate through the sub-layers in the parent map service that are after 
            // the current measure sub-layer
            for (int i = indexInMapService + 1; i < parentLayer.Layers.Length; i++)
            {
                // Get the sub-layer at the current loop index
                LayerInfo subLayer = parentLayer.Layers[i];

                // Get whether the loop sub-layer is measurable and is already a member
                // of the measurable sub-layers collection
                bool isMeasurable = subLayer.IsMeasurable(parentLayer, Map);
                bool isInCollection = MeasurableSubLayers.Contains(subLayer);

                if (isMeasurable && !isInCollection)    // sub-layer is measurable but not in 
                {                                       // collection, so it needs to be added.

                    MeasurableSubLayers.Insert(insertIndex, subLayer);
                    if (needsMeasureLayerUpdate) // check whether a new measure sub-layer needs to be selected
                    {
                        // Update the measure sub-layer and reset the update flag
                        MeasureSubLayer = subLayer;
                        needsMeasureLayerUpdate = false;
                    }
                    insertIndex++;  // Increment the insertion index so the next sub-layer is inserted
                                    // after this one
                }
                else if (!isMeasurable && isInCollection)   // sub-layer in collection has become 
                {                                           // unmeasurable, so it needs to be removed
                    MeasurableSubLayers.Remove(subLayer);
                }
                else if (isMeasurable && isInCollection)    // sub-layer is measurable and already in 
                {                                           // collection. 
                    if (needsMeasureLayerUpdate)
                    {
                        MeasureSubLayer = subLayer;         // Update the sub-layer if necessary and
                        needsMeasureLayerUpdate = false;    // reset the update flag.
                    }
                    insertIndex++;                          // Increment insertion index so the next one 
                }                                           // gets inserted after this one.
            }

            insertIndex = indexInMeasurableLayers;
            // Iterate through the sub-layers in the parent map service that are before the current 
            // measure sub-layer, looping through them in reverse order
            for (int i = indexInMapService - 1; i > -1; i--)
            {
                // Get the sub-layer at the current loop index
                LayerInfo subLayer = parentLayer.Layers[i];

                // Get whether the loop sub-layer is measurable and is already a member
                // of the measurable sub-layers collection
                bool isMeasurable = subLayer.IsMeasurable(parentLayer, Map);
                bool isInCollection = MeasurableSubLayers.Contains(subLayer);
                if (isMeasurable && !isInCollection)    // sub-layer is measurable but not in 
                {                                       // collection, so it needs to be added.

                    MeasurableSubLayers.Insert(insertIndex, subLayer);
                    if (needsMeasureLayerUpdate) // check whether a new measure sub-layer needs to be selected
                    {
                        // Update the measure sub-layer and reset the update flag
                        MeasureSubLayer = subLayer;
                        needsMeasureLayerUpdate = false;
                    }
                    insertIndex--;  // Decrement the insertion index so the next sub-layer is inserted
                                    // before this one
                }
                else if (!isMeasurable && isInCollection)   // sub-layer in collection has become 
                {                                           // unmeasurable, so it needs to be removed
                    MeasurableSubLayers.Remove(subLayer);
                }
                else if (isMeasurable && isInCollection)    // sub-layer is measurable and already in 
                {                                           // collection. 
                    if (needsMeasureLayerUpdate)
                    {
                        MeasureSubLayer = subLayer;         // Update the sub-layer if necessary and
                        needsMeasureLayerUpdate = false;    // reset the update flag.
                    }
                    insertIndex--;                          // Decrement insertion index so the next one 
                }                                           // gets inserted before this one.
            }

            // Check whether original measure sub-layer needs to be removed
            if (!parentLayer.Layers[indexInMapService].IsMeasurable(parentLayer, Map))
                MeasurableSubLayers.Remove(parentLayer.Layers[indexInMapService]);

            // If there are no more measurable sub-layers in the current measure layer, update
            // the list of measurable layers.
            if (MeasurableSubLayers.Count == 0)
                UpdateMeasurableLayers();
        }

        /// <summary>
        /// Checks whether the ViewModel has any measurements and updates the <see cref="HasMeasurements"/>
        /// property accordingly
        /// </summary>
        private void CheckHasMeasurements()
        {
            HasMeasurements =
                (MeasurableGeometry is MapPoint && Latitude != null && Longitude != null)
                || (MeasurableGeometry is Polyline && Length != null)
                || (MeasurableGeometry is Polygon && Length != null && Area != null);
        }

        #region Event handlers for updating measurable layers

        private ThrottleTimer _updateMeasurableLayersThrottler;
        // when layers are added/removed
        private void Layers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LayerCollection layers = (LayerCollection)sender;
            // Put updating of measurable layers in a throttle timer, meaning that this logic will 
            // only be invoked once if the CollectionChanged event fires multiple times within two 
            // tenths of a second.  This is necessary because changing to a basemap in a different 
            // spatial reference results in clearing and rebuilding the layers collection.
            if (_updateMeasurableLayersThrottler == null)
            {
                _updateMeasurableLayersThrottler = new ThrottleTimer(20, () => 
                {
                    // Check whether all layers are initialized
                    if (layers.Any(l => !l.IsInitialized))
                        layers.LayersInitialized += Layers_LayersInitialized; // Wait for initialization
                    else
                        UpdateMeasurableLayers(); // Update measurable layers collection now
                });
            }
            _updateMeasurableLayersThrottler.Invoke();

            // Check whether layers were added
            if (e.NewItems != null)
            {
                // Hook into visibility changed events for added layers
                foreach (Layer layer in e.NewItems)
                {
                    layer.PropertyChanged += Layer_PropertyChanged;

                    if (layer is ArcGISDynamicMapServiceLayer)
                    {
                        ISublayerVisibilitySupport subLayerSupport = layer as ISublayerVisibilitySupport;
                        subLayerSupport.VisibilityChanged += SubLayerSupport_VisibilityChanged;
                    }
                }
            }

            // Check whether layers were removed
            if (e.OldItems != null)
            {
                // Unhook from visibility changed events for removed layers
                foreach (Layer layer in e.OldItems)
                {
                    layer.PropertyChanged -= Layer_PropertyChanged;

                    if (layer is ArcGISDynamicMapServiceLayer)
                    {
                        ISublayerVisibilitySupport subLayerSupport = layer as ISublayerVisibilitySupport;
                        subLayerSupport.VisibilityChanged -= SubLayerSupport_VisibilityChanged;
                    }
                }
            }

            // If draw layer has been added to the map, check whether it is the top-most
            if (layers.Contains(DrawLayer) && layers.IndexOf(DrawLayer) != layers.Count - 1)
            {
                // Draw layer is not top-most.  Move it to the top by removing and re-adding it.  Wrap in
                // a begin invoke call so the collection is modified outside the CollectionChanged event.
                Map.Dispatcher.BeginInvoke(() =>
                    {
                        layers.Remove(DrawLayer);
                        layers.Add(DrawLayer);
                    });
            }
        }

        // Handle layer visibilty changes.  Layers may become measurable or non-measurable.
        private void Layer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // If any layer's visibility is changed, update the set of measurable layers
            if (e.PropertyName == "Visible")
                UpdateMeasurableLayers();
        }

        // Handle sub-layer visibility changes.  Sub-layers and parent layers may become measurable or non-measurable
        private void SubLayerSupport_VisibilityChanged(object sender, EventArgs e)
        {
            // If sub-layers are being toggled on/off within the current layer, update the measurable sub-layers
            if (sender == MeasureLayer)
                UpdateMeasurableSubLayers();
            else                            // In all other cases, update the measurable layers.  Because if all 
                UpdateMeasurableLayers();   // sub-layers in a layer are turned off, it becomes unmeasurable.
        }

        private double _lastScale;
        // Check measurable layers when zooming in or zooming out.  Measurability may change due to layer
        // scale dependency
        private void Map_ExtentChanged(object sender, ExtentEventArgs e)
        {
            // Check whether the map scale has changed. Layer visibility can change because of map scale changes
            if (Map.Scale == _lastScale)
                return;
            _lastScale = Map.Scale;

            // If the current layer has sub-layers, then the sub-layers collection needs to be checked
            if (MeasureLayer is ArcGISDynamicMapServiceLayer)
                UpdateMeasurableSubLayers();

            // Always check measurable layers on scale change
            UpdateMeasurableLayers();
        }

        // Check measurable layers when layers initialize.  Can occur when layers are being added 
        private void Layers_LayersInitialized(object sender, EventArgs args)
        {
            // Unhook handler
            ((LayerCollection)sender).LayersInitialized -= Layers_LayersInitialized;

            // All layers are initialized, so the measurable layers collection can be updated
            UpdateMeasurableLayers();
        }

        #endregion

        private void DrawObject_DrawComplete(object sender, DrawEventArgs e)
        {
            // update draw geometry with user-drawn geometry
            DrawGeometry = e.Geometry;
        }

        #endregion

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
