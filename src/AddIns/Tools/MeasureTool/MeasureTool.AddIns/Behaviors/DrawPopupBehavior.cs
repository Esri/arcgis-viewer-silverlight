/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using MeasureTool.Addins.Resources;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Displays drawing instructions and measurements next to the mouse cursor while 
    /// drawing on a map using a <see cref="ESRI.ArcGIS.Client.Draw"/> object
    /// </summary>
    public class DrawPopupBehavior : Behavior<Draw>
    {
        private PointCollection _currentDrawVertices; // Tracks the points in the draw geometry
        private static Popup _popup; // The popup containing the draw instructions and measurements
        private Map _map; // The map associated with the Draw object
        private string _drawInstructions = string.Empty; // Stores the instructions shown on the popup

        #region Behavior Overrides

        /// <summary>
        /// Activates the drawing popup to display instructions and measurements while drawing
        /// </summary>
        /// <param name="parameter">Not used</param>
        protected override void OnAttached()
        {
 	        base.OnAttached();
            if (AssociatedObject != null) // Make sure a Draw object is associated
            {
                if (AssociatedObject.Map != null)
                    _map = AssociatedObject.Map;
                else
                    throw new Exception(Strings.DrawPopupBehaviorNoMap);

                bool origVal = AssociatedObject.IsEnabled;
                try
                {
                    // make sure draw is disabled before hooking events
                    AssociatedObject.IsEnabled = false;

                    // Call activate to hook to Draw and Map events
                    activate();
                }
                finally
                {
                    AssociatedObject.IsEnabled = origVal;
                }

                // Bind DrawMode to detect when drawing mode is changed
                Binding b = new Binding("DrawMode") { Source = AssociatedObject };
                BindingOperations.SetBinding(this, DrawModeProperty, b);

                // Bind IsEnabled to detect when draw enabled state changes
                b = new Binding("IsEnabled") { Source = AssociatedObject };
                BindingOperations.SetBinding(this, IsEnabledProperty, b);

                _currentDrawVertices = null;
                _currentDrawVertices = new PointCollection();
            }
        }

        /// <summary>
        /// Deactivates the draw popup
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            deactivate();
        }

        #endregion

        #region Dependency properties

        /// <summary>
        /// Backing DependencyProperty for the <see cref="PopupTemplate"/> property
        /// </summary>
        public static DependencyProperty PopupTemplateProperty = DependencyProperty.Register(
            "PopupTemplate", typeof(Style), typeof(DrawPopupBehavior), null);

        /// <summary>
        /// Gets or sets the appearance of the popup.  Instruction and measurement text is passed
        /// via DataContext.
        /// </summary>
        public Style PopupTemplate
        {
            get { return GetValue(PopupTemplateProperty) as Style; }
            set { SetValue(PopupTemplateProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="LinearUnit"/> property
        /// </summary>
        public static readonly DependencyProperty LinearUnitProperty =
            DependencyProperty.Register("LinearUnit", typeof(LengthUnits), typeof(DrawPopupBehavior), null);

        /// <summary>
        /// Gets or sets the unit to use for linear measurements
        /// </summary>
        public LengthUnits LinearUnit
        {
            get { return (LengthUnits)GetValue(LinearUnitProperty); }
            set 
            { 
                SetValue(LinearUnitProperty, value); 
            }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="ArealUnit"/> property
        /// </summary>
        public static readonly DependencyProperty ArealUnitProperty =
            DependencyProperty.Register("ArealUnit", typeof(AreaUnits), typeof(DrawPopupBehavior), null);

        /// <summary>
        /// Gets or sets the unit to use for areal measurements
        /// </summary>
        public AreaUnits ArealUnit
        {
            get { return (AreaUnits)GetValue(ArealUnitProperty); }
            set { SetValue(ArealUnitProperty, value); }
        }

        /// <summary>
        /// Private DependencyProperty for detecting changes on a property of type 
        /// <see cref="ESRI.ArcGIS.Client.DrawMode"/> 
        /// </summary>
        private static readonly DependencyProperty DrawModeProperty =
            DependencyProperty.Register("DrawMode", typeof(DrawMode), 
            typeof(DrawPopupBehavior), new PropertyMetadata(OnDrawModeChanged));

        private static void OnDrawModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Re-initialize the drawing instructions
            ((DrawPopupBehavior)d).initializePopupMessage();
        }

        /// <summary>
        /// Private DependencyProperty for detecting changes on a property of type 
        /// <see cref="ESRI.ArcGIS.Client.DrawMode"/> 
        /// </summary>
        private static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool),
            typeof(DrawPopupBehavior), new PropertyMetadata(OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DrawPopupBehavior behavior = (DrawPopupBehavior)d;
            bool isEnabled = behavior.AssociatedObject.IsEnabled;
            if (isEnabled)
                behavior.activate();
            else
                behavior.deactivate();
        }

        private string _popupText = string.Empty;
        /// <summary>
        /// Gets or sets the text of the popup shown while drawing
        /// </summary>
        /// <remarks>
        /// A private property is used in order to keep the Popup's DataContext in sync with the popup
        /// text without requiring implementation of a bindable property.
        /// </remarks>
        private string PopupText
        {
            get { return _popupText; }
            set
            {
                if (value != _popupText)
                {
                    _popupText = value;
                    Popup.DataContext = value;
                }
            }
        }

        #endregion Dependency properties

        private Popup Popup
        {
            get
            {
                if (_popup == null && PopupTemplate != null)
                {
                    // Create an instance of the control type specified by the popup's template
                    Control c = Activator.CreateInstance(PopupTemplate.TargetType) as Control;

                    // Assign the popup template
                    c.Style = PopupTemplate;

                    _popup = new Popup() { Child = c };

                    // Find the FrameworkElement containing the map
                    FrameworkElement parent = _map.Parent as FrameworkElement;
                    while (!(parent is Panel) && (parent != null))
                        parent = parent.Parent as FrameworkElement;

                    // Add the popup to the container
                    if (parent != null)
                        (parent as Panel).Children.Add(_popup);
                }
                return _popup;
            }
        }

        #region Draw events (Begin / VertexAdded / Completed)

        // Handle updating of popup once drawing has started
        private void drawObject_DrawBegin(object sender, EventArgs e)
        {
            // Update the drawing instructions on DrawMode
            switch (AssociatedObject.DrawMode)
            {
                case DrawMode.Polyline:
                    PopupText = _drawInstructions = Strings.DrawClickContinueDoubleClickFinish;
                    break;
                case DrawMode.Polygon:
                    PopupText = _drawInstructions = Strings.DrawClickContinue;
                    break;
                case DrawMode.Circle:
                case DrawMode.Ellipse:
                case DrawMode.Freehand:
                case DrawMode.Rectangle:
                    PopupText = _drawInstructions = Strings.DrawReleaseFinish;
                    break;
            }
        }

        // Process points added to the geometry currently being drawn
        private void drawObject_VertexAdded(object sender, VertexAddedEventArgs e)
        {
            // clone the point and add it to the current set of vertices
            _currentDrawVertices.Add(e.Vertex.Clone());

            // Update draw instructions if the 2nd vertex of a polygon has just been added
            if (AssociatedObject.DrawMode == DrawMode.Polygon && _currentDrawVertices.Count == 2)
                PopupText = _drawInstructions = Strings.DrawClickContinueDoubleClickFinish;
        }

        // Handle points being drawn for geometry types that don't raise VertexAdded
        private void map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!AssociatedObject.IsEnabled)
                return;

            // add the first point because VertexAdded doesn't get called for first 
            // pt for circle/ellipse/rect
            if (_currentDrawVertices != null
            && _currentDrawVertices.Count == 0
            && (AssociatedObject.DrawMode == DrawMode.Circle
                || AssociatedObject.DrawMode == DrawMode.Ellipse
                || AssociatedObject.DrawMode == DrawMode.Rectangle))
            {
                // get the mouse position
                MapPoint mapPoint = e.GetMapPoint(_map);

                // add the point to the current set of vertices
                _currentDrawVertices.Add(mapPoint);
            }
        }

        // While a geometry is being drawn, follow the mouse to calculate measurements
        // on the fly and reposition the popup
        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (!AssociatedObject.IsEnabled)
                return;

            // get the mouse position
            MapPoint mapPoint = e.GetMapPoint(_map);

            // Calculate measurements based on the new point
            if (!requiresProjection() && _currentDrawVertices.Count > 0)
                updateMeasurements(mapPoint);

            // Set the position of the popup relative to the mouse event (i.e. cursor)
            updatePopupPosition(e.GetPosition(Popup));
        }

        private void drawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            // Clear the set of vertices and reset the instructions
            _currentDrawVertices.Clear();
            initializePopupMessage();
        }

        // Hide the draw popup when the mouse is not within the map
        private void map_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.IsOpen = false;
        }

        // If the mouse enters the map, show the draw popup if drawing is enabled
        private void map_MouseEnter(object sender, MouseEventArgs e)
        {
            Popup.IsOpen = AssociatedObject.IsEnabled;
        }

        #endregion Draw events (Begin / VertexAdded / Completed)

        #region Private Methods

        #region Measurement Handling

        /// <summary>
        /// Updates the measurements and message shown on the popup based on the in-progress 
        /// geometry, including the passed-in point
        /// </summary>
        /// <param name="mapPoint"></param>
        private void updateMeasurements(MapPoint mapPoint)
        {
            if (_currentDrawVertices != null)
            {
                double length = 0d;
                double area = 0d;

                string measurement1 = null;
                string measurement2 = null;

                switch (AssociatedObject.DrawMode)
                {
                    case DrawMode.Freehand:
                        // Copy the set of vertices and add the passed-in point to it
                        PointCollection tmpPts = _currentDrawVertices.Clone();
                        tmpPts.Add(mapPoint.Clone());

                        // Check whether a polyline or polygon is being drawn
                        FreehandDrawMode freehandeMode = AttachedProperties.GetFreehandDrawMode(AssociatedObject); 
                        if (freehandeMode == FreehandDrawMode.Polyline)
                        {
                            // Get the length of the line and update the measurement text
                            Polyline line = new Polyline() { SpatialReference = _currentDrawVertices[0].SpatialReference };
                            line.Paths.Add(tmpPts);
                            length = line.Length();
                            measurement1 = string.Format(Strings.LengthLabelFormat, getLengthString(length));
                        }
                        else if (freehandeMode == FreehandDrawMode.Polygon)
                        {
                            // Close the polygon by duplicating the first vertex and adding it to the end
                            tmpPts.Add(tmpPts[0].Clone());
                            Polygon polygon = new Polygon { SpatialReference = _currentDrawVertices[0].SpatialReference };
                            polygon.Rings.Add(tmpPts);

                            // Get the perimeter of the polygon and update the measurement text
                            length = polygon.Perimeter();
                            measurement1 = string.Format(Strings.PerimeterLabelFormat, getLengthString(length));
                        }
                        break;

                    case DrawMode.Polyline:
                        // Create a line containing the segement of the polyline currently being drawn.

                        // First, get the most recently drawn point
                        tmpPts = new PointCollection();
                        tmpPts.Add(_currentDrawVertices[_currentDrawVertices.Count - 1].Clone());
                        
                        // Add the passed-in point
                        tmpPts.Add(mapPoint.Clone());

                        // Create a polyline with the points
                        Polyline polyline = new Polyline() { SpatialReference = _currentDrawVertices[0].SpatialReference };
                        polyline.Paths.Add(tmpPts);

                        // Calculate the length and update the segment measurement text
                        length = polyline.Length();
                        measurement1 = string.Format(Strings.SegmentLengthLabelFormat, getLengthString(length));

                        // If there already two or more points in the polyline, calculate total length as well
                        if (_currentDrawVertices.Count > 1)
                        {
                            // Copy the current set of vertices and add the passed-in point
                            tmpPts = _currentDrawVertices.Clone();
                            tmpPts.Add(mapPoint.Clone());

                            // Create a polyline with the points
                            polyline = new Polyline() { SpatialReference = _currentDrawVertices[0].SpatialReference };
                            polyline.Paths.Add(tmpPts);

                            // Get the length
                            length = polyline.Length();
                        }

                        // Update measurement text showing the total length of the line
                        measurement2 = string.Format(Strings.TotalLengthLabelFormat, getLengthString(length));

                        break;
                    case DrawMode.Rectangle:
                        // Create an envelope from the initial point and the passed-in piont
                        Envelope env = new Envelope(_currentDrawVertices[0], mapPoint) 
                            { SpatialReference = mapPoint.SpatialReference };

                        // Get the perimeter and area of the envelope
                        length = env.Perimeter();
                        area = env.Area();

                        // Update the measurement text
                        measurement1 = string.Format(Strings.PerimeterLabelFormat, getLengthString(length));
                        measurement2 = string.Format(Strings.AreaLabelFormat, getAreaString(area));
                        break;
                    case DrawMode.Circle:
                    case DrawMode.Ellipse:
                        // Get the initial (center) point and the current point
                        tmpPts = new PointCollection();
                        tmpPts.Add(_currentDrawVertices[0].Clone());
                        //tmpPts.Add(_currentDrawVertices[_currentDrawVertices.Count - 1].Clone());
                        tmpPts.Add(mapPoint.Clone());

                        // Create a line with the points
                        polyline = new Polyline() { SpatialReference = _currentDrawVertices[0].SpatialReference };
                        polyline.Paths.Add(tmpPts);

                        // Calculate the length of the line.  This is the radius.
                        length = polyline.Length();

                        // For circles, this info is sufficient to calculate perimeter anda area
                        if (AssociatedObject.DrawMode == DrawMode.Circle)
                        {
                            // Calculate perimeter (circumference) and area
                            double circumference = 2 * length * Math.PI;
                            area = Math.PI * Math.Pow(length, 2);

                            // Display measures
                            measurement1 = string.Format(Strings.PerimeterLabelFormat, getLengthString(circumference));
                            measurement2 = string.Format(Strings.AreaLabelFormat, getAreaString(area));
                        }
                        else // Ellipse
                        {
                            // insufficient info for perimeter & area, so just display radius
                            measurement1 = string.Format(Strings.RadiusLabelFormat, getLengthString(length));
                        }
                        break;
                    case DrawMode.Polygon:
                        // Can only calculate perimeter and area if there are already two or more 
                        // vertices in the polygon
                        if (_currentDrawVertices.Count > 1)
                        {
                            // Copy the existing vertices and add the passed-in point
                            tmpPts = _currentDrawVertices.Clone();
                            tmpPts.Add(mapPoint.Clone());

                            // Create a polygon from the points
                            Polygon p = new Polygon() { SpatialReference = mapPoint.SpatialReference };
                            p.Rings.Add(tmpPts);

                            // Calculate the perimeter as well as the area
                            length = p.Perimeter();
                            area = p.Area();

                            // Update measurement text
                            measurement1 = string.Format(Strings.PerimeterLabelFormat, getLengthString(length));
                            measurement2 = string.Format(Strings.AreaLabelFormat, getAreaString(area));
                        }
                        break;
                    default:
                        break;
                }

                // Update the draw popup with the measurement text
                if (measurement1 != null)
                    PopupText = _drawInstructions + "\n" + measurement1;

                if (measurement2 != null)
                    PopupText += "\n" + measurement2;
            }
        }

        private string getLengthString(double length)
        {
            double? lengthInCurrentUnits = Utils.ConvertLength(length, LinearUnit);
            return string.Format(Strings.MeasurementFormat, lengthInCurrentUnits, LinearUnit.GetDescription());
        }

        private string getAreaString(double area)
        {
            double? areaInCurrentUnits = Utils.ConvertArea(area, ArealUnit);
            return string.Format(Strings.MeasurementFormat, areaInCurrentUnits, ArealUnit.GetDescription());
        }

        private bool requiresProjection()
        {
            // Geometry requires projecting if its spatial reference is not 
            // WGS 84 or Web Mercator
            return _map.SpatialReference == null
                || (!_map.SpatialReference.IsWebMercator()
                && _map.SpatialReference.WKID != 4326);

        }

        #endregion

        #region Popup Handling

        private void initializePopupMessage()
        {
            if (!AssociatedObject.IsEnabled)
                Popup.IsOpen = false;

            // set the initial draw instructions
            switch (AssociatedObject.DrawMode)
            {
                case ESRI.ArcGIS.Client.DrawMode.Point:
                    PopupText = _drawInstructions = Strings.DrawShapePoint; 
                    break;
                case ESRI.ArcGIS.Client.DrawMode.Freehand:
                case ESRI.ArcGIS.Client.DrawMode.Rectangle:
                case ESRI.ArcGIS.Client.DrawMode.Circle:
                case ESRI.ArcGIS.Client.DrawMode.Ellipse:
                    PopupText = _drawInstructions = Strings.DrawClickDrag; 
                    break;
                case ESRI.ArcGIS.Client.DrawMode.Polyline:
                case ESRI.ArcGIS.Client.DrawMode.Polygon:
                    PopupText = _drawInstructions = Strings.DrawClickStart; 
                    break;
            }
        }

        // Updates the popup's position relative to the specified mouse position
        private void updatePopupPosition(Point mousePosition)
        {
            // Calculate default popup location based on mouse position plus a small margin
            // from the mouse cursor - will place the popup below and to the right of the cursor
            double marginFromCursor = 20;
            double horizontalOffset = mousePosition.X + marginFromCursor;
            double verticalOffset = mousePosition.Y + marginFromCursor;

            // Check whether the popup would be positioned outside the right or bottom
            // edge of the application and adjust if necessary
            double marginFromEdge = 5;
            if (Application.Current != null && Application.Current.RootVisual != null)
            {
                // Get the width and height of the popup
                double popupWidth = Popup.Child.RenderSize.Width;
                double popupHeight = Popup.Child.RenderSize.Height;

                // Get the position of the right and bottom edge of the popup
                double rightEdge = horizontalOffset + popupWidth;
                double bottomEdge = verticalOffset + popupHeight;

                // Get the application's size
                Size appSize = Application.Current.RootVisual.RenderSize;

                // Check whether the right and bottome edges of the popup would be placed 
                // outside the application 
                bool adjustHorizontalOffset = rightEdge + marginFromEdge > appSize.Width;
                bool adjustVerticalOffset = bottomEdge + marginFromEdge > appSize.Height;

                if (adjustHorizontalOffset && adjustVerticalOffset) 
                {
                    // right and bottom edges are both outside app - place popup above and to the
                    // left of the cursor instead of below and to the right
                    double horizontalAdjustment = 1.3 * marginFromCursor + popupWidth;
                    horizontalOffset -= horizontalAdjustment;

                    double verticalAdjustment = 1.3 * marginFromCursor + popupHeight;
                    verticalOffset -= verticalAdjustment;
                }
                else if (adjustHorizontalOffset)
                {
                    // only right edge is outside app - slide popup to the left to keep it within the app
                    double adjustment = rightEdge - appSize.Width + marginFromEdge;
                    horizontalOffset -= adjustment;
                }
                else if (adjustVerticalOffset)
                {
                    // only bottom edge is outside the app - slide popup up to keep it within the app
                    double adjustment = bottomEdge - appSize.Height + marginFromEdge;
                    verticalOffset -= adjustment;
                }
            }

            // Set the position of the popup, applying the calculated offset
            Popup.HorizontalOffset = horizontalOffset;
            Popup.VerticalOffset = verticalOffset;
        }

        #endregion

        #region Activation/Deactivation

        private bool _isActivated = false;
        private void activate()
        {
            if (_isActivated == true) // Events are already hooked
                return;

            _isActivated = true;

            // hook the events
            AssociatedObject.DrawBegin += drawObject_DrawBegin;
            AssociatedObject.VertexAdded += drawObject_VertexAdded;
            AssociatedObject.DrawComplete += drawObject_DrawComplete;

            _map.MouseMove += map_MouseMove;
            _map.MouseLeave += map_MouseLeave;
            _map.MouseEnter += map_MouseEnter;
            _map.MouseLeftButtonDown += map_MouseLeftButtonDown;
        }

        private void deactivate()
        {
            _currentDrawVertices.Clear();
            Popup.IsOpen = false;

            AssociatedObject.DrawBegin -= drawObject_DrawBegin;
            AssociatedObject.VertexAdded -= drawObject_VertexAdded;
            AssociatedObject.DrawComplete -= drawObject_DrawComplete;
            _map.MouseMove -= map_MouseMove;
            _map.MouseLeave -= map_MouseLeave;
            _map.MouseEnter -= map_MouseEnter;
            _map.MouseLeftButtonDown -= map_MouseLeftButtonDown;

            _isActivated = false;
        }

        #endregion

        #endregion
    }
}
