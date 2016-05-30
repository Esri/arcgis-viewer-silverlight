/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
        protected override void SaveSite(string siteId, string newTitle, SitePublishInfo info)
        {
            Site site = SiteConfiguration.FindExistingSiteByID(siteId);
            if (site == null)
                throw new Exception("Unable to find site with siteId = " + siteId);

            FaultContract Fault = null;
            if (!(new ApplicationBuilderHelper()).SaveConfigurationForSite(site, info, out Fault, newTitle))
                throw new Exception(Fault != null ? Fault.Message : "Unable to save site");

            if (!string.IsNullOrEmpty(newTitle) && site.Title != newTitle) // Update site's title in site configuration list
            {
                site.Title = newTitle;
                SiteConfiguration.SaveSite(site);
            }
        }
    }
}
