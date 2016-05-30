/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

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
    /// Loads the web map corresponding to the ID specified in the query string of the URL of the
    /// current HTML document into the associated <see cref="ESRI.ArcGIS.Client.Map">Map</see> control
    /// </summary>
    [Export(typeof(Behavior<Map>))]
    [LocalizedDisplayName("BehaviorDisplayName")]
    [LocalizedDescription("BehaviorDescription")]
    [LocalizedCategory("BehaviorCategory")]
    public class GetWebMapFromQueryStringBehavior : Behavior<Map>, INotifyInitialized
    {
        #region Behavior Overrides
        protected override void OnAttached()
        {
            try
            {
                base.OnAttached();

                // Track whether the query string contains a web map
                bool webMapSpecified = false;

                // Verify that the behavior is attached to a map and check whether query string is present
                if (AssociatedObject != null && HtmlPage.Document != null && HtmlPage.Document.QueryString != null)
                {
                    // Put query string values in a case-insensitive dictionary
                    var queryString = new Dictionary<string, string>(HtmlPage.Document.QueryString,
                        StringComparer.InvariantCultureIgnoreCase);

                    // Check whether the query string specifies a web map id
                    if (queryString.ContainsKey("webmap"))
                    {
                        webMapSpecified = true;

                        // get the web map id
                        string webMapID = queryString["webmap"];
                        MapApplication.Current.LoadWebMap(webMapID, null, (e) =>
                        {
                            // Fire the InitializationFailed event if there was an error.  Otherwise fire Initialized.
                            if (e.Error == null)
                                OnInitializationFailed(e.Error);
                            else
                                OnInitialized();
                        });
                    }
                }

                // Fire initialized event a web map is not being loaded
                if (!webMapSpecified)
                    OnInitialized();
            }
            catch (Exception ex)
            {
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
