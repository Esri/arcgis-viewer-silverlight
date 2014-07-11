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
    public class UpgradeSiteHandler : UpgradeSiteHandlerBase
    {
        /// <summary>
        /// Upgrades the site with the specified ID to the current version of the product
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        protected override Site UpgradeSite(string siteId, string templateId, out FaultContract fault)
        {
            // Get the site
            Site site = SiteConfiguration.FindExistingSiteByID(siteId);
            if (site == null)
                throw new Exception("Unable to find site with siteId = " + siteId);

            // Do upgrade
            fault = null;
            bool upgraded = ApplicationBuilderHelper.UpgradeSite(site, templateId, out fault);

            if (upgraded)
            {
                // Upgrade successful - update the version on the site and save it to disk
                Version currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                site.ProductVersion = string.Format("{0}.{1}.{2}.0", currentVersion.Major, currentVersion.Minor,
                    currentVersion.Build);
                SiteConfiguration.SaveSite(site);
                return site;
            }
            else
            {
                if (fault == null)
                    fault = new FaultContract() { Message = "Upgrade failed" };
                return null;
            }
        }
    }
}
