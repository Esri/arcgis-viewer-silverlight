/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
    /// Centers the associated map on the point specified in the query string of the URL of the
    /// current HTML document
    /// </summary>
    [Export(typeof(Behavior<Map>))]
    [LocalizedDisplayName("BehaviorDisplayName")]
    [LocalizedDescription("BehaviorDescription")]
    [LocalizedCategory("BehaviorCategory")]
    public class GetCenterFromQueryStringBehavior : Behavior<Map>, INotifyInitialized
    {
        #region Behavior Overrides
        protected async override void OnAttached()
        {
            try
            {
                base.OnAttached();

                // Verify that the behavior is attached to a map with a valid WKID and that a 
                // query string is present
                if (AssociatedObject != null
                    && AssociatedObject.SpatialReference != null
                    && AssociatedObject.SpatialReference.WKID > 0
                    && HtmlPage.Document != null
                    && HtmlPage.Document.QueryString != null)
                {
                    // Put query string values in a case-insensitive dictionary
                    var queryString = new Dictionary<string, string>(HtmlPage.Document.QueryString, StringComparer.InvariantCultureIgnoreCase);

                    // Check whether query string contains center
                    if (queryString.ContainsKey("center"))
                    {
                        // get the center string
                        string centerString = queryString["center"];

                        // get the list delimiter for the current culture (making sure to account for cultures that use delimiters other than comma)
                        char listDelimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator[0];

                        // Split extent string into discrete values
                        string[] centerPointVals = centerString.Split(listDelimiter);
                        double x, y;

                        // Verify that the expected number of values are present, that they are numeric, and convert the values to doubles
                        IFormatProvider numericFormat = CultureInfo.CurrentCulture.NumberFormat;
                        if ((centerPointVals.Length == 2 || centerPointVals.Length == 3)
                        && double.TryParse(centerPointVals[0], NumberStyles.Number, numericFormat, out x)
                        && double.TryParse(centerPointVals[1], NumberStyles.Number, numericFormat, out y))
                        {
                            // get WKID from the map attached to the behavior
                            int currentWkid = AssociatedObject.Extent.SpatialReference.WKID;
                            int queryStringWkid = currentWkid;

                            // Check whether a WKID was specified (will be the 3rd parameter if present) and grab it if so
                            if (centerPointVals.Length == 3 && !int.TryParse(centerPointVals[2], out queryStringWkid))
                            {
                                // invalid WKID specified, fire InitializationFailed and exit
                                OnInitializationFailed(new Exception(Strings.InvalidWkid));
                                return;
                            }

                            // Initialize a point with the same spatial reference as the attached map
                            MapPoint queryStringPoint = new MapPoint(x, y)
                            {
                                SpatialReference = new SpatialReference(queryStringWkid)
                            };

                            // check whether the specified WKID is different than that of the attached map, meaning the point needs to be projected
                            if (queryStringWkid != currentWkid)
                            {
                                WebMercator wm = new WebMercator();

                                // Use convenience methods to convert between WGS 84 and Web Mercator if possible
                                if (isWebMercator(queryStringWkid) && isWgs84(currentWkid))
                                {
                                    queryStringPoint = (MapPoint)wm.ToGeographic(queryStringPoint);
                                }
                                else if (isWgs84(queryStringWkid) && isWgs84(currentWkid))
                                {
                                    queryStringPoint = (MapPoint)wm.FromGeographic(queryStringPoint);
                                }
                                else if (!string.IsNullOrEmpty(MapApplication.Current.Urls.GeometryServiceUrl))
                                {
                                    // Conversion is not between WGS 84 and Web Mercator, so a call to a geometry service is necessary to perform
                                    // the projection
                                    try
                                    {
                                        GeometryService gs = new GeometryService(MapApplication.Current.Urls.GeometryServiceUrl);
                                        var g = new Graphic() { Geometry = queryStringPoint };
                                        var sr = new SpatialReference(currentWkid);
                                        var result = await gs.ProjectTaskAsync(new Graphic[] { g }, sr);
                                        queryStringPoint = (MapPoint)result.Results[0].Geometry;
                                    }
                                    catch (Exception ex)
                                    {
                                        // Projection failed.  Fire InitializationFailed event and return.
                                        OnInitializationFailed(ex);
                                        return;
                                    }
                                }
                            }

                            var panDuration = AssociatedObject.PanDuration;
                            AssociatedObject.PanDuration = TimeSpan.FromSeconds(0);
                            AssociatedObject.PanTo(queryStringPoint);
                            AssociatedObject.PanDuration = panDuration;
                        }
                    }
                }

                // Fire Initialized event
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

        // Checks whether the specified WKID corresponds to the Web Mercator projection
        private bool isWebMercator(int wkid)
        {
            return wkid == 102100 || wkid == 102113 || wkid == 3857;
        }

        // Checks whether the specified WKID corresponds to the WGS 84 projection
        private bool isWgs84(int wkid)
        {
            return wkid == 4326;
        }
    }
}
