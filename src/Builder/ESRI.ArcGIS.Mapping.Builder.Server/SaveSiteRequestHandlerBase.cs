/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Mapping.Builder.Common;
using System.Xml.Serialization;
using System.Xml;

namespace ESRI.ArcGIS.Mapping.Builder.Server
{
    public abstract class SaveSiteRequestHandlerBase : ServiceRequestHandlerBase
    {
        protected override void HandleRequest()
        {
            string siteId = Request["siteId"];
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(Request.InputStream);
            XmlElement rootElem = xDoc.DocumentElement;
            string siteTitle = rootElem.GetAttribute("Title");
            
            SitePublishInfo info = Utils.GetSitePublishInfoFromXml(rootElem);
            try
            {
                SaveSite(siteId, siteTitle, info);
            }
            catch (Exception ex)
            {
                base.WriteError(ex.Message);
            }
        }

        protected abstract void SaveSite(string siteId, string newTitle, SitePublishInfo info);
    }
}
