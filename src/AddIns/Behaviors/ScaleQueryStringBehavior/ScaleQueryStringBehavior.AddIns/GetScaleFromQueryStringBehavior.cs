/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using Esri.ArcGIS.Client.Application.AddIns.Resources;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Projection;
using ESRI.ArcGIS.Client.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Windows.Browser;
using System.Windows.Interactivity;

namespace Esri.ArcGIS.Client.Application.AddIns
{
    /// <summary>
    /// Zooms the associated map to the scale specified in the query string of the URL of the
    /// current HTML document
    /// </summary>
    [Export(typeof(Behavior<Map>))]
    [LocalizedDisplayName("BehaviorDisplayName")]
    [LocalizedDescription("BehaviorDescription")]
    [LocalizedCategory("BehaviorCategory")]
    public class GetScaleFromQueryStringBehavior : Behavior<Map>, INotifyInitialized
    {
        #region Behavior Overrides
        protected override void OnAttached()
        {
            try
            {
                base.OnAttached();

                // Verify that the behavior is attached to a map and that a query string is present 
                if (AssociatedObject != null
                && HtmlPage.Document != null 
                && HtmlPage.Document.QueryString != null)
                {
                    // Put query string values in a case-insensitive dictionary
                    var queryString = new Dictionary<string, string>(HtmlPage.Document.QueryString, StringComparer.InvariantCultureIgnoreCase);

                    // Check whether scale level is defined
                    if (queryString.ContainsKey("scale"))
                    {
                        // get the scale string
                        string scaleString = queryString["scale"];
                        double scale;

                        // Attempt to convert the scale string to a double
                        if (double.TryParse(scaleString, out scale) && scale > 0)
                        {
                            // Calculate the zoom factor required to achieve the specified scale
                            double zoomFactor = AssociatedObject.Scale / scale;

                            // Get the current zoom duration and update zoom duration to zero
                            TimeSpan zoomDuration = AssociatedObject.ZoomDuration;
                            AssociatedObject.ZoomDuration = TimeSpan.FromSeconds(0);

                            // Zoom to the specified scale
                            AssociatedObject.Zoom(zoomFactor);

                            // Restore the zoom duration to what it was before zooming
                            AssociatedObject.ZoomDuration = zoomDuration;
                        }
                        else
                        {
                            // Scale was non-numeric.  Fire OnInitializationFailed event and return.
                            OnInitializationFailed(new Exception(Strings.InvalidScale));
                            return;
                        }
                    }
                }

                // Raise Initialized event
                OnInitialized();
            }
            catch (Exception ex)
            {
                // Raise InitializationFailed event, passing the exception that was thrown
                OnInitializationFailed(ex);
            }
        }
        #endregion

        #region INotifyInitialized Members
        /// <summary>
        /// Gets whether initialization has completed.
        /// </summary>
        public bool IsInitialzed { get; private set; }

        /// <summary>
        /// Gets the initialization error.  Will be populated when initialization fails.
        /// </summary>
        public Exception InitializationError { get; private set; }

        /// <summary>
        /// Raised when initialization fails
        /// </summary>
        public event EventHandler InitializationFailed;
        private void OnInitializationFailed(Exception initError)
        {
            InitializationError = initError;
            if (InitializationFailed != null)
                InitializationFailed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raised when initialization has completed successfully
        /// </summary>
        public event EventHandler Initialized;
        private void OnInitialized()
        {
            IsInitialzed = true;
            if (Initialized != null)
                Initialized(this, EventArgs.Empty);
        }
        #endregion
    }
}
