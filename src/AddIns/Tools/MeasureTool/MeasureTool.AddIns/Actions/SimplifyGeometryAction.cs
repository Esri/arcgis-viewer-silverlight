/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using MeasureTool.Addins.Resources;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Simplifies a geometry
    /// </summary>
    public class SimplifyGeometryAction : TargetedTriggerAction<Geometry>
    {
        private GeometryService _geometryService;
        protected override void Invoke(object parameter)
        {
            // Clear error
            Error = null;

            try
            {
                if (Target != null && !string.IsNullOrEmpty(GeometryServiceUrl))
                {
                    if (Target is Polyline || Target is Polygon) // Only these types need simplification
                    {
                        Graphic g = new Graphic() { Geometry = Target };

                        // If a previous simplify operation is still executing, cancel it
                        if (_geometryService.IsBusy)
                            _geometryService.CancelAsync();
                        _geometryService.SimplifyAsync(new Graphic[] { g });
                    }
                    else
                    {
                        // No simplification required - just complete
                        OutputGeometry = Target;
                        OnCompleted();
                    }
                }
                else
                {
                    OnFailed(new Exception(Strings.SimplifyInputsNotValid));
                }
            }
            catch (Exception ex)
            {
                OnFailed(ex);
            }
        }

        #region Dependency properties

        #region GeometryServiceUrl

        /// <summary>
        /// Backing DependencyProperty for the <see cref="GeometryServiceUrl" Property />
        /// </summary>
        public static readonly DependencyProperty GeometryServiceUrlProperty =
            DependencyProperty.Register("GeometryServiceUrl",
            typeof(string), typeof(SimplifyGeometryAction), new PropertyMetadata(OnGeometryServiceUrlChanged));

        /// <summary>
        /// Gets or sets the URL of a geometry service to use for the simplify operation
        /// </summary>
        public string GeometryServiceUrl
        {
            get { return GetValue(GeometryServiceUrlProperty) as string; }
            set { SetValue(GeometryServiceUrlProperty, value); }
        }

        private static void OnGeometryServiceUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SimplifyGeometryAction a = (SimplifyGeometryAction)d;
            if (a._geometryService == null)
            {
                a._geometryService = new GeometryService();
                a._geometryService.SimplifyCompleted += a.GeometryService_SimplifyCompleted;
                a._geometryService.Failed += a.GeometryService_Failed;
            }

            a._geometryService.Url = a.GeometryServiceUrl;
        }

        #endregion

        #region OutputGeometry

        /// <summary>
        /// Backing DependencyProperty for the <see cref="OutputGeometry" Property />
        /// </summary>
        public static readonly DependencyProperty OutputGeometryProperty =
            DependencyProperty.Register("OutputGeometry",
            typeof(Geometry), typeof(SimplifyGeometryAction), null);

        /// <summary>
        /// Gets the simplified geometry
        /// </summary>
        public Geometry OutputGeometry
        {
            get { return GetValue(OutputGeometryProperty) as Geometry; }
            private set { SetValue(OutputGeometryProperty, value); }
        }

        #endregion

        #region Error

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Error" Property />
        /// </summary>
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register("Error",
            typeof(Exception), typeof(SimplifyGeometryAction), null);

        /// <summary>
        /// Gets exception information if an error has occurred
        /// </summary>
        public Exception Error
        {
            get { return GetValue(ErrorProperty) as Exception; }
            private set { SetValue(ErrorProperty, value); }
        }

        #endregion


        #endregion

        #region Events

        #region Public Events - Completed, Failed

        /// <summary>
        /// Raised when the geometry simplification has completed
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

        private void GeometryService_SimplifyCompleted(object sender, GraphicsEventArgs e)
        {
            // Store the simplified geometry as the output and fire completed event
            if (e.Results.Count > 0)
                OutputGeometry = e.Results[0].Geometry;

            OnCompleted();
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            string errorMessage = string.Format(Strings.SimplifyFailed, GeometryServiceUrl, e.Error.Message);
            OnFailed(new Exception(errorMessage));
        }

        #endregion

        #endregion
    }
}
