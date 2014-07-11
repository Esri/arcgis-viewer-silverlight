/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Web;
using ESRI.ArcGIS.Mapping.Builder.Server;
using ESRI.ArcGIS.Mapping.Builder.Common;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    public class SaveSiteRequestHandler : SaveSiteRequestHandlerBase
    {
        protected override void SaveSite(string siteId, SitePublishInfo info)
        {
            Site site = SiteConfiguration.FindExistingSiteByID(siteId);
            if (site == null)
                throw new Exception("Unable to find site with siteId = " + siteId);

            FaultContract Fault = null;
            if (!(new ApplicationBuilderHelper()).SaveConfigurationForSite(site, info, out Fault))
                throw new Exception(Fault != null ? Fault.Message : "Unable to save site");
        }
    }
}
