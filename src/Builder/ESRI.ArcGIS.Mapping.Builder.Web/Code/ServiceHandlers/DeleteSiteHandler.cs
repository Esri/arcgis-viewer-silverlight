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
    public class DeleteSiteHandler : DeleteSiteHandlerBase
    {
        protected override void DeleteSite(string siteId)
        {
            Site site = SiteConfiguration.FindExistingSiteByID(siteId);
            if (site == null)
                throw new Exception("Unable to find site with siteId = " + siteId);

            try
            {
                // Delete the folder
                if (System.IO.Directory.Exists(site.PhysicalPath))
                    System.IO.Directory.Delete(site.PhysicalPath, true);
            }
            catch { }

            // Delete the site from the catalog
            SiteConfiguration.DeleteSite(siteId);
        }
    }
}
