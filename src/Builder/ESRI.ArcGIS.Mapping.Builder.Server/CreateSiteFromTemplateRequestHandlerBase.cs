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
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace ESRI.ArcGIS.Mapping.Builder.Server
{
    public abstract class CreateSiteFromTemplateRequestHandlerBase : ServiceRequestHandlerBase
    {
        protected override void HandleRequest()
        {
            XmlDocument xDoc = new XmlDocument();
            Site site = null;
            try
            {
                xDoc.Load(Request.InputStream);
                XmlElement rootElem = xDoc.DocumentElement;
                string siteName = rootElem.GetAttribute("Name");
                string siteTitle = rootElem.GetAttribute("Title");
                string description = rootElem.GetAttribute("Description");
                string templateId = rootElem.GetAttribute("TemplateId");
                SitePublishInfo info = Utils.GetSitePublishInfoFromXml(rootElem);
                site = CreateSite(siteName, siteTitle, description, templateId, info);
            }
            catch (Exception ex)
            {
                base.WriteError(ex.Message);
                return;
            }

            if (site == null)
            {
                base.WriteError("Unable to create site");
                return;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Site));
            serializer.Serialize(Response.OutputStream, site);
            Response.OutputStream.Flush();
        }
        
        protected abstract Site CreateSite(string siteName, string siteTitle, string description, string templateId, SitePublishInfo sitePublishInfo);
    }
}
