/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using ESRI.ArcGIS.Mapping.Builder.Common;
using ESRI.ArcGIS.Mapping.Builder.Server;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    public class CopySiteRequestHandler : CopySiteRequestHandlerBase
    {
        protected override Site CopySite(string sourceSiteId, string targetSiteName, string targetSiteDescription)
        {
            // Resolve source site id into its corresponding site object
            Site sourceSite = SiteConfiguration.FindExistingSiteByID(sourceSiteId);
            if (sourceSite == null)
                throw new Exception("Unable to find site with siteId = " + sourceSiteId);

            // Make sure target site directory does not already exist
            string physicalPath = string.Format("{0}\\{1}", AppSettings.AppsPhysicalDir, targetSiteName);
            if (System.IO.Directory.Exists(physicalPath))
                throw new Exception("Site with name '" + targetSiteName + "' is in use already. Please try an alternate name.");

            // Create a new site
            Site targetSite = new Site();
            targetSite.ID = Guid.NewGuid().ToString("N");
            targetSite.IsHostedOnIIS = true;
            targetSite.Name = targetSiteName;
            targetSite.Description = targetSiteDescription;
            targetSite.PhysicalPath = physicalPath;
            targetSite.Url = string.Format("{0}/{1}", AppSettings.AppsBaseUrl.TrimEnd('/'), targetSiteName.TrimStart('/'));
            targetSite.ProductVersion = sourceSite.ProductVersion;

            // Copy files from source site directory to target site directory
            FaultContract Fault = null;
            if (!(new ApplicationBuilderHelper()).CopySite(sourceSite.PhysicalPath, targetSite, true, out Fault))
                throw new Exception(Fault != null ? Fault.Message : "Unable to copy site");

            // Update site database (XML file)
            SiteConfiguration.AddSite(targetSite);

            // Return target site object
            return targetSite;
        }
    }
}
