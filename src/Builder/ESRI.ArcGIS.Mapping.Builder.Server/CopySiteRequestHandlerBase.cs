/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Xml;
using System.Xml.Serialization;
using ESRI.ArcGIS.Mapping.Builder.Common;

namespace ESRI.ArcGIS.Mapping.Builder.Server
{
    public abstract class CopySiteRequestHandlerBase : ServiceRequestHandlerBase
    {
        protected override void HandleRequest()
        {
            // Extract source site id from request
            string siteId = Request["siteId"];

            // Initialize name and description variables
            string siteName = "";
            string description = "";

            // Load message body into XML document for processing
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(Request.InputStream);
            XmlElement rootElem = xDoc.DocumentElement;

            // Extract site name and description node values if they are present
            XmlElement elem = rootElem.SelectSingleNode("siteName") as XmlElement;
            if (elem != null)
                siteName = elem.InnerText;
            elem = rootElem.SelectSingleNode("description") as XmlElement;
            if (elem != null)
                description = elem.InnerText;
            
            // Copy the source site to the target name
            Site site = null;
            try
            {
                site = CopySite(siteId, siteName, description);
            }
            catch (Exception ex)
            {
                base.WriteError(ex.Message);
                return;
            }

            if (site == null)
            {
                base.WriteError("Unable to copy site");
                return;
            }

            // Serialize newly created site and return
            XmlSerializer serializer = new XmlSerializer(typeof(Site));
            serializer.Serialize(Response.OutputStream, site);
            Response.OutputStream.Flush();
        }

        protected abstract Site CopySite(string siteId, string siteName, string description);
    }
}
