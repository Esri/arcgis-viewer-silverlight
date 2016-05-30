/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Threading;
using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using System.Diagnostics;
using ESRI.ArcGIS.Client.Tasks;
using System;
using ESRI.ArcGIS.Client.Projection;
using MeasureTool.Addins.Resources;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Calculates length/perimeter, area, latitude, and/or longitude measurements for the targeted
    /// geometry.  Calculations are geodesic.
    /// </summary>
    public class MeasureAction : TargetedTriggerAction<Geometry>
    {
        private GeometryService _geometryService = null;
        private SpatialReference _outSpatialRef = null;

        protected override void Invoke(object parameter)
        {
            // Clear error
            Error = null;

            // Reset measurements
            Latitude = null;
            Longitude = null;
            Length = null;
            Area = null;

            try
            {
                if (Target != null)
                {
                    // Check whether geometric operations (e.g. measure) can be done client-side
                    if (!Target.SupportsClientGeometricOps())
                    {
                        ProjectGeometry(Target);
                    }
                    else
                    {
                        MeasureGeometry(Target);
                        // Raise completed event
                        OnCompleted();
                    }
                }
            }
            catch (Exception ex)
            {
                OnFailed(ex);
            }
        }

        #region Dependency Properties

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Length"/> property
        /// </summary>
        public static DependencyProperty LengthProperty = DependencyProperty.Register(
            "Length", typeof(double?), typeof(MeasureAction), null);

        /// <summary>
        /// Gets the length or perimeter of the last geometry that was measured, in meters
        /// </summary>
        public double? Length
        {
            get { return GetValue(LengthProperty) as double?; }
            private set { SetValue(LengthProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Area"/> property
        /// </summary>
        public static DependencyProperty AreaProperty = DependencyProperty.Register(
            "Area", typeof(double?), typeof(MeasureAction), null);

        /// <summary>
        /// Gets the area of the last geometry that was measured, in meters
        /// </summary>
        public double? Area
        {
            get { return GetValue(AreaProperty) as double?; }
            private set { SetValue(AreaProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Latitude"/> property
        /// </summary>
        public static DependencyProperty LatitudeProperty = DependencyProperty.Register(
            "Latitude", typeof(double?), typeof(MeasureAction), null);

        /// <summary>
        /// Gets the latitude of the last geometry that was measured, in decimal degrees
        /// </summary>
        public double? Latitude
        {
            get { return GetValue(LatitudeProperty) as double?; }
            private set { SetValue(LatitudeProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Longitude"/> property
        /// </summary>
        public static DependencyProperty LongitudeProperty = DependencyProperty.Register(
            "Longitude", typeof(double?), typeof(MeasureAction), null);

        /// <summary>
        /// Gets the longitude of the last geometry that was measured, in decimal degrees
        /// </summary>
        public double? Longitude
        {
            get { return GetValue(LongitudeProperty) as double?;  }
            private set { SetValue(LongitudeProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="GeometryServiceUrl"/> property
        /// </summary>
        public static readonly DependencyProperty GeometryServiceUrlProperty =
            DependencyProperty.Register("GeometryServiceUrl", typeof(string), typeof(MeasureAction), null);

        /// <summary>
        /// Gets or sets the URL to the geometry service to use if reprojection is necessary
        /// </summary>
        public string GeometryServiceUrl
        {
            get { return (string)GetValue(GeometryServiceUrlProperty); }
            set { SetValue(GeometryServiceUrlProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Error" Property />
        /// </summary>
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register("Error",
            typeof(Exception), typeof(MeasureAction), null);

        /// <summary>
        /// Gets exception information if an error has occurred
        /// </summary>
        public Exception Error
        {
            get { return GetValue(ErrorProperty) as Exception; }
            private set { SetValue(ErrorProperty, value); }
        }

        #endregion Dependency Properties

        #region Events

        #region Public Events - Completed, Failed

        /// <summary>
        /// Raised when measurement calculation has completed
        /// </summary>
        public event EventHandler Completed;

        private void OnCompleted()
        {
            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raised when the action fails
        /// </summary>
        public event EventHandler<UnhandledExceptionEventArgs> Failed;

        private void OnFailed(Exception ex)
        {
            Error = ex;
            if (Failed != null)
                Failed(this, new UnhandledExceptionEventArgs(ex, false));
        }

        #endregion

        #region Geometry Service Event Handling

        // Measure the result geometry
        private void GeometryService_ProjectCompleted(object sender, GraphicsEventArgs e)
        {
            if (e.Results.Count > 0)
            {
                // Measure the projected geometry and raise the completed event
                MeasureGeometry(e.Results[0].Geometry);
                OnCompleted();
            }
            else
            {
                // No results were returned from the projection operation.  Raise the failed event.
                string errorMessage = string.Format(Strings.NoProjectionResults, GeometryServiceUrl);
                OnFailed(new Exception(errorMessage));
            }
        }

        // Reprojection failed - throw error
        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            string errorMessage = string.Format(Strings.ProjectFailure, GeometryServiceUrl,
                e.Error.Message);
            OnFailed(new Exception(errorMessage));
        }

        #endregion

        #endregion

        #region Geometry handling methods - projection and measurement

        /// <summary>
        /// Projects the input geometry to WGS 84
        /// </summary>
        private void ProjectGeometry(Geometry geometry)
        {
            // Geometry requires reprojection. Ensure geometry service is specified.
            if (string.IsNullOrEmpty(GeometryServiceUrl))
                throw new Exception(Strings.GeometryServiceNotSpecified);

            // Initialize geometry service object if necessary
            if (_geometryService == null)
            {
                _geometryService = new GeometryService(GeometryServiceUrl);
                _geometryService.ProjectCompleted += GeometryService_ProjectCompleted;
                _geometryService.Failed += GeometryService_Failed;
                _outSpatialRef = new SpatialReference(4326);
            }

            // Project the geometry using the geometry service
            Graphic g = new Graphic() { Geometry = geometry };

            // if a previous projection is still executing, cancel it
            if (_geometryService.IsBusy)
                _geometryService.CancelAsync();
            _geometryService.ProjectAsync(new Graphic[] { g }, _outSpatialRef);
        }

        /// <summary>
        /// Calculates measurements - length/perimeter, area, latitude, and/or longitude - 
        /// for the input geometry
        /// </summary>
        /// <param name="geometry"></param>
        private void MeasureGeometry(Geometry geometry)
        {
            if (geometry is MapPoint) // Update latitude and longitude
            {
                // If necessary, transform the point to WGS 84 to get latitude
                // and longitude in decimal degrees
                MapPoint point = (MapPoint)geometry;
                if (point.SpatialReference.IsWebMercator())
                    point = new WebMercator().ToGeographic(geometry) as MapPoint;

                point = (MapPoint)Geometry.NormalizeCentralMeridian(point);

                Latitude = point.Y;
                Longitude = point.X;
            }
            else if (geometry is Polyline) // Update length
            {
                Length = ((Polyline)geometry).Length();
            }
            else if (geometry is Polygon || geometry is Envelope) // Update area
            {
                // If geometry is an envelope, convert it to a polygon
                Polygon polygon = geometry is Polygon ? (Polygon)geometry :
                    ((Envelope)geometry).ToPolygon();

                Length = polygon.Perimeter();
                Area = polygon.Area();
            }
            else
            {
                throw new Exception(Strings.MeasureUnsupportedGeometryType);
            }
        }

        #endregion
    }
}
