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
    /// Zooms the associated map to the extent specified in the query string of the URL of the
    /// current HTML document
    /// </summary>
    [Export(typeof(Behavior<Map>))]
    [LocalizedDisplayName("BehaviorDisplayName")]
    [LocalizedDescription("BehaviorDescription")]
    [LocalizedCategory("BehaviorCategory")]
    public class GetExtentFromQueryStringBehavior : Behavior<Map>, INotifyInitialized
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
                    // Put query string values into case-insensitive dictionary
                    var queryString = new Dictionary<string, string>(HtmlPage.Document.QueryString, StringComparer.CurrentCultureIgnoreCase);

                    // Check whether extent is present in the query string
                    if (queryString.ContainsKey("extent"))
                    {
                        // get the extent string
                        string extentString = queryString["extent"];

                        // get the list delimiter for the current culture (making sure to account for cultures that use delimiters other than comma)
                        char listDelimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator[0];

                        // Split extent string into discrete values
                        string[] extentVals = extentString.Split(listDelimiter);
                        double xmin, ymin, xmax, ymax;

                        // Verify that the expected number of values are present, that they are numeric, and convert the values to doubles
                        IFormatProvider numericFormat = CultureInfo.CurrentCulture.NumberFormat;
                        if ((extentVals.Length == 4 || extentVals.Length == 5)
                        && double.TryParse(extentVals[0], NumberStyles.Number, numericFormat, out xmin)
                        && double.TryParse(extentVals[1], NumberStyles.Number, numericFormat, out ymin)
                        && double.TryParse(extentVals[2], NumberStyles.Number, numericFormat, out xmax)
                        && double.TryParse(extentVals[3], NumberStyles.Number, numericFormat, out ymax))
                        {
                            // get WKID from the map attached to the behavior
                            int currentWkid = AssociatedObject.Extent.SpatialReference.WKID;
                            int queryStringWkid = currentWkid;

                            // Check whether a WKID was specified (will be the 5th parameter if present) and grab it if so
                            if (extentVals.Length == 5 && !int.TryParse(extentVals[4], out queryStringWkid))
                            {
                                // invalid WKID specified, fire InitializationFailed and exit
                                OnInitializationFailed(new Exception(Strings.InvalidWkid));
                                return;
                            }

                            // Initialize an envelope with the same spatial reference as the attached map
                            Envelope queryStringExtent = new Envelope(xmin, ymin, xmax, ymax)
                            {
                                SpatialReference = new SpatialReference(queryStringWkid)
                            };

                            // check whether the specified WKID is different than that of the attached map, meaning the extent needs to be projected
                            if (queryStringWkid != currentWkid)
                            {
                                WebMercator wm = new WebMercator();

                                // Use convenience methods to convert between WGS 84 and Web Mercator if possible
                                if (isWebMercator(queryStringWkid) && isWgs84(currentWkid))
                                {
                                    queryStringExtent = (Envelope)wm.ToGeographic(queryStringExtent);
                                }
                                else if (isWgs84(queryStringWkid) && isWgs84(currentWkid))
                                {
                                    queryStringExtent = (Envelope)wm.FromGeographic(queryStringExtent);
                                }
                                else if (!string.IsNullOrEmpty(MapApplication.Current.Urls.GeometryServiceUrl))
                                {
                                    // Covnersion is not between WGS 84 and Web Mercator, so a call to a geometry service is necessary to perform
                                    // the projection
                                    try
                                    {
                                        GeometryService gs = new GeometryService(MapApplication.Current.Urls.GeometryServiceUrl);
                                        var g = new Graphic() { Geometry = queryStringExtent };
                                        var sr = new SpatialReference(currentWkid);
                                        var result = await gs.ProjectTaskAsync(new Graphic[] { g }, sr);
                                        queryStringExtent = (Envelope)result.Results[0].Geometry;
                                    }
                                    catch (Exception ex)
                                    {
                                        // Projection failed.  Fire InitializationFailed and return.
                                        OnInitializationFailed(ex);
                                        return;
                                    }
                                }
                            }

                            // Apply the extent from the query string to the attached map
                            AssociatedObject.Extent = queryStringExtent;
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
