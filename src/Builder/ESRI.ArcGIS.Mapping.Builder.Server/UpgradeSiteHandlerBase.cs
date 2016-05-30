/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Mapping.Builder.Common;
using System.Xml.Serialization;
using System.Net;

namespace ESRI.ArcGIS.Mapping.Builder.Server
{
    /// <summary>
    /// Base class for an HTTP handler that process requests to upgrade Viewer applications
    /// </summary>
    public abstract class UpgradeSiteHandlerBase : ServiceRequestHandlerBase
    {
        /// <summary>
        /// Processes the current request
        /// </summary>
        protected override void HandleRequest()
        {
            // Get site and template info from the request
            string siteId = Request["siteId"];
            string templateId = Request["templateId"];

            FaultContract fault = null;
            // Perform the upgrade
            Site site = UpgradeSite(siteId, templateId, out fault);

            if (site != null)
            {
                // Return the upgraded site's metadata as the response
                XmlSerializer serializer = new XmlSerializer(typeof(Site));
                serializer.Serialize(Response.OutputStream, site);
                Response.OutputStream.Flush();
            }
            else
            {
                // Return error message
                string errorMessage = fault != null && !string.IsNullOrEmpty(fault.Message) ? fault.Message :
                    "Upgrade failed";
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Response.Write(errorMessage);
                Response.OutputStream.Flush();
            }
        }

        protected abstract Site UpgradeSite(string siteId, string templateId, out FaultContract fault);
    }
}
