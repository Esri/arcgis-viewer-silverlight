/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Threading;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using MeasureTool.Addins.Resources;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Gets a feature from the target layer that intersects with the click point when the layer is clicked
    /// </summary>
    public class GetFeatureOnClickBehavior : Behavior<Layer>
    {
        #region Member variables

        private QueryTask _queryTask = null;
        private Query _query = null;
        private Graphic _mouseDownGraphic = null;
        private DispatcherTimer _clickTimer = new DispatcherTimer() 
            { Interval = TimeSpan.FromMilliseconds(500) };

        #endregion

        #region Behavior overrides

        /// <summary>
        /// Invoked when the behavior is attached to a Layer.  Hooks to events and performs other
        /// initialization logic.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            // Clear current feature
            Feature = null;

            // Hook to events necessary to get clicked features
            if (AssociatedObject is GraphicsLayer)
            {
                GraphicsLayer gLayer = (GraphicsLayer)AssociatedObject; 
                gLayer.MouseLeftButtonDown += GraphicsLayer_MouseLeftButtonDown;
                gLayer.MouseLeftButtonUp += GraphicsLayer_MouseLeftButtonUp;
                _clickTimer.Tick += ClickTimer_Tick;
            }

            if (Map != null)
                Map.MouseClick += Map_MouseClick;
        }

        /// <summary>
        /// Invoked when the behavior is detached from a layer.  Unhooks from events and
        /// performs other cleanup logic.
        /// </summary>
        protected override void OnDetaching()
        {
            // Detach event handlers from target object before calling base method because
            // base method will un-associate target
            if (AssociatedObject is GraphicsLayer)
            {
                GraphicsLayer gLayer = (GraphicsLayer)AssociatedObject;
                gLayer.MouseLeftButtonDown -= GraphicsLayer_MouseLeftButtonDown;
                gLayer.MouseLeftButtonUp -= GraphicsLayer_MouseLeftButtonUp;
            }

            base.OnDetaching();
            if (Map != null)
                Map.MouseClick -= Map_MouseClick;
            _clickTimer.Tick -= ClickTimer_Tick;
        }

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Backing DependencyProperty for the <see cref="SubLayer"/> property
        /// </summary>
        public static DependencyProperty SubLayerProperty = DependencyProperty.Register(
            "SubLayer", typeof(LayerInfo), typeof(GetFeatureOnClickBehavior), null);

        /// <summary>
        /// Gets or sets the sub-layer to retrieve features from
        /// </summary>
        public LayerInfo SubLayer
        {
            get { return GetValue(SubLayerProperty) as LayerInfo; }
            set { SetValue(SubLayerProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Map"/> property
        /// </summary>
        public static DependencyProperty MapProperty = DependencyProperty.Register(
            "Map", typeof(Map), typeof(GetFeatureOnClickBehavior), 
            new PropertyMetadata(OnMapPropertyChanged));

        /// <summary>
        /// Gets or sets the map containing the target layer
        /// </summary>
        public Map Map
        {
            get { return GetValue(MapProperty) as Map; }
            set { SetValue(MapProperty, value); }
        }

        // Invoked when the map changes.  
        private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GetFeatureOnClickBehavior behavior = (GetFeatureOnClickBehavior)d;
            if (e.OldValue != null)
            {
                // Unhook from previous map's click event
                ((Map)e.OldValue).MouseClick -= behavior.Map_MouseClick;

                // Only hook to the event in the case where the map is being changed while
                // the behavior is attached to a layer
                if (e.NewValue != null && behavior.AssociatedObject != null)
                    ((Map)e.NewValue).MouseClick += behavior.Map_MouseClick;
            }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Feature"/> property
        /// </summary>
        public static DependencyProperty FeatureProperty = DependencyProperty.Register(
            "Feature", typeof(Graphic), typeof(GetFeatureOnClickBehavior), null);

        /// <summary>
        /// Gets the clicked feature
        /// </summary>
        public Graphic Feature
        {
            get { return GetValue(FeatureProperty) as Graphic; }
            private set { SetValue(FeatureProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="IsBusy"/> property
        /// </summary>
        public static DependencyProperty IsBusyProperty = DependencyProperty.Register(
            "IsBusy", typeof(bool), typeof(GetFeatureOnClickBehavior), null);

        /// <summary>
        /// Gets whether the behavior is in a busy state
        /// </summary>
        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            private set { SetValue(IsBusyProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Error" Property />
        /// </summary>
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register("Error",
            typeof(Exception), typeof(GetFeatureOnClickBehavior), null);

        /// <summary>
        /// Gets exception information if an error has occurred
        /// </summary>
        public Exception Error
        {
            get { return GetValue(ErrorProperty) as Exception; }
            private set { SetValue(ErrorProperty, value); }
        }

        #endregion

        #region Events

        #region Public Events - Completed, Failed

        /// <summary>
        /// Raised when the geometry simplification has completed
        /// </summary>
        public event EventHandler Completed;

        private void OnCompleted(Graphic feature)
        {
            Feature = feature;
            IsBusy = false;
            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raised when the action fails
        /// </summary>
        public event EventHandler<UnhandledExceptionEventArgs> Failed;

        private void OnFailed(Exception ex)
        {
            Feature = null;
            Error = ex;
            IsBusy = false;
            if (Failed != null)
                Failed(this, new UnhandledExceptionEventArgs(ex, false));
        }

        #endregion

        #region Handle map or layer click

        // Handle mouse-down when the target layer is a graphics layer
        private void GraphicsLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs e)
        {
            // Mark the event as handled so other actions do not occur
            e.Handled = true;

            // Store the graphic so we can check to see if the same graphic is moused-up
            _mouseDownGraphic = e.Graphic;

            // Start the timer to see whether mouse-up happens quickly enough that the
            // mouse-down, mouse-up sequence can be considered a click
            _clickTimer.Start();
        }

        // Handle mouse-up when the target layer is a graphics layer to check whether a graphic was clicked
        private void GraphicsLayer_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs e)
        {
            // Check whether the graphic that the mouse was released over is the same as the one that
            // the mouse was pressed over.  Also see whether the mouse was released within the click 
            // interval.
            if (e.Graphic.Equals(_mouseDownGraphic) && _clickTimer.IsEnabled)
            {
                // Both conditions met - a graphic was clicked.

                // Mark the event as handled so other actions do not occur
                e.Handled = true;

                // Clear error
                Error = null;

                // Set busy state
                IsBusy = true;

                _clickTimer.Stop(); // Stop the timer

                Geometry clickGeometry = e.Graphic.Geometry.Clone();
                clickGeometry.SpatialReference = clickGeometry.SpatialReference ?? Map.SpatialReference.Clone();
                // Update the feature - make a copy of it so it can be added to other layers
                OnCompleted(new Graphic() { Geometry = clickGeometry });
            }
        }

        // Handle when the click timer elapses
        private void ClickTimer_Tick(object sender, EventArgs e)
        {
            // The mouse was depressed on a graphic, but the timer elapsed before the mouse
            // was released, so the action does not qualify as a click.  So stop the timer.
            _clickTimer.Stop();

            IsBusy = false;
        }

        // Handle the map's click event.  Used to query dynamic map service layers.
        private void Map_MouseClick(object sender, Map.MouseEventArgs e)
        {
            // Mark the event as handled so other actions do not occur
            e.Handled = true;

            // Clear error
            Error = null;

            // Set busy state
            IsBusy = true;

            if (AssociatedObject is GraphicsLayer)
            {
                // If the target is a GraphicsLayer, the click-detection timer will be enabled if 
                // a graphic from the layer was clicked
                if (!_clickTimer.IsEnabled)
                    OnFailed(new Exception(Strings.NoFeaturesFound)); // None found - raise failure
            }
            else if (AssociatedObject is ArcGISDynamicMapServiceLayer && SubLayer != null)
            {
                string layerUrl = null;
                // Construct the URL to the sublayer - we need this to query against
                ArcGISDynamicMapServiceLayer dynLayer = (ArcGISDynamicMapServiceLayer)AssociatedObject;
                layerUrl = string.Format("{0}/{1}", dynLayer.Url, SubLayer.ID.ToString());

                // Initialize query task if necessary
                if (_queryTask == null)
                {
                    _queryTask = new QueryTask();
                    _queryTask.ExecuteCompleted += QueryTask_ExecuteCompleted;
                    _queryTask.Failed += QueryTask_Failed;

                    // Instantiate query and initialize properties that won't change regardless of 
                    // object state
                    _query = new Query()
                    {
                        ReturnGeometry = true,
                        SpatialRelationship = SpatialRelationship.esriSpatialRelIntersects
                    };
                }

                _query.OutSpatialReference = Map.SpatialReference;
                _queryTask.Url = layerUrl;

                // get the click point in screen coords
                Point screenPt = Map.MapToScreen(e.MapPoint);

                // Create two rectangle corner points, offset from the click point by a tolerance.
                // Do this on the screen point so the tolerance is always applied in screen pixels.
                int searchTol = 5;
                Point screenLowerLeft = new Point(screenPt.X - searchTol, screenPt.Y - searchTol);
                Point screenUpperRight = new Point(screenPt.X + searchTol, screenPt.Y + searchTol);

                // convert to map points
                MapPoint mapLowerLeft = Map.ScreenToMap(screenLowerLeft);
                MapPoint mapUpperRight = Map.ScreenToMap(screenUpperRight);

                // Create a search rectangle from the corner points and use it to set the query geometry
                _query.Geometry = new Envelope(mapLowerLeft, mapUpperRight) { SpatialReference = e.MapPoint.SpatialReference };

                // If a previous query is still executing, cancel it
                if (_queryTask.IsBusy)
                    _queryTask.CancelAsync();

                // Do the query
                _queryTask.ExecuteAsync(_query);
            }
            else
            {
                OnFailed(new Exception(Strings.UnsupportedLayerType));
                return;
            }
        }

        #endregion

        #region Handle query results

        private void QueryTask_ExecuteCompleted(object sender, QueryEventArgs args)
        {
            // Update the Feature property with the first feature
            if (args.FeatureSet.Features.Count > 0)
                OnCompleted(args.FeatureSet.Features[0]);
            else
                OnFailed(new Exception(Strings.NoFeaturesFound));
        }

        private void QueryTask_Failed(object sender, TaskFailedEventArgs args)
        {
            OnFailed(new Exception(string.Format(Strings.QueryFailed, args.Error.Message)));
        }

        #endregion

        #endregion
    }
}
