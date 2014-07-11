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
    /// Zooms the associated map to the scale level specified in the query string of the URL of the
    /// current HTML document
    /// </summary>
    [Export(typeof(Behavior<Map>))]
    [LocalizedDisplayName("BehaviorDisplayName")]
    [LocalizedDescription("BehaviorDescription")]
    [LocalizedCategory("BehaviorCategory")]
    public class GetScaleLevelFromQueryStringBehavior : Behavior<Map>, INotifyInitialized
    {
        #region Behavior Overrides
        protected override void OnAttached()
        {
            try
            {
                base.OnAttached();

                // Tracks whether scale level was specified in the query string
                bool scaleLevelSpecified = false;

                // Verify that the behavior is attached to a map, that the map's first layer is a tiled map 
                // service layer, and that a query string is present
                if (AssociatedObject != null
                    && AssociatedObject.Layers.Count > 0
                    && AssociatedObject.Layers[0] is ArcGISTiledMapServiceLayer
                    && HtmlPage.Document != null
                    && HtmlPage.Document.QueryString != null)
                {
                    // Put query string values in a case-insensitive dictionary
                    var queryString = new Dictionary<string, string>(HtmlPage.Document.QueryString, StringComparer.InvariantCultureIgnoreCase);

                    // Check whether scale level is defined
                    if (queryString.ContainsKey("level"))
                    {
                        // get the scale string
                        string scaleString = queryString["level"];
                        int scaleLevel;

                        ArcGISTiledMapServiceLayer basemapLayer = (ArcGISTiledMapServiceLayer)AssociatedObject.Layers[0];

                        // Attempt to convert the scale string to a double
                        if (int.TryParse(scaleString, out scaleLevel)
                        && scaleLevel > 1)
                        {
                            // Set flag indicating that a scale level was found in the query string
                            scaleLevelSpecified = true;

                            if (basemapLayer.TileInfo == null)
                            {
                                EventHandler<EventArgs> onInitialized = null;
                                onInitialized = (o, e) =>
                                {
                                    basemapLayer.Initialized -= onInitialized;

                                    // Verify the scale level is within the range of levels defined for the layer
                                    if (scaleLevel > basemapLayer.TileInfo.Lods.Length + 1)
                                    {
                                        // Fire InitializationFailed event and exit
                                        OnInitializationFailed(new Exception(Strings.InvalidScaleLevel));
                                        return;
                                    }

                                    zoomToScaleLevel(scaleLevel, basemapLayer);

                                    // Fire the Initialized event
                                    OnInitialized();
                                };
                                basemapLayer.Initialized += onInitialized;
                            }
                            else
                            {
                                // Verify the scale level is within the range of levels defined for the layer
                                if (scaleLevel > basemapLayer.TileInfo.Lods.Length + 1)
                                {
                                    // Fire InitializationFailed event and exit
                                    OnInitializationFailed(new Exception(Strings.InvalidScaleLevel));
                                    return;
                                }

                                zoomToScaleLevel(scaleLevel, basemapLayer);

                                // Fire the Initialized event
                                OnInitialized();
                            }
                        }
                        else
                        {
                            // Fire InitializationFailed event and exit
                            OnInitializationFailed(new Exception(Strings.InvalidScaleLevel));
                            return;
                        }
                    }
                }

                // No scale level was found in the query string, so raise the initialized event here
                if (!scaleLevelSpecified)
                    OnInitialized();
            }
            catch (Exception ex)
            {
                // Raise InitializationFailed event
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

        private void zoomToScaleLevel(int scaleLevel, ArcGISTiledMapServiceLayer layer)
        {
            double targetResolution = layer.TileInfo.Lods[scaleLevel - 1].Resolution;

            // Get the current zoom duration and update zoom duration to zero
            TimeSpan zoomDuration = AssociatedObject.ZoomDuration;
            AssociatedObject.ZoomDuration = TimeSpan.FromSeconds(0);

            // Zoom to the resolution of the specified scale level
            AssociatedObject.Zoom(targetResolution);

            // Restore the zoom duration to what it was before zooming
            AssociatedObject.ZoomDuration = zoomDuration;
        }
    }
}
