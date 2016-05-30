/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using ESRI.ArcGIS.Mapping.Builder.Common;
using ESRI.ArcGIS.Mapping.Builder.Server;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    public class CreateSiteFromTemplateHandler : CreateSiteFromTemplateRequestHandlerBase
    {
        protected override Site CreateSite(string targetSiteName, string targetTitle, string targetSiteDescription, string targetSiteTemplateId, SitePublishInfo targetSitePublishInfo)
        {
            // Make sure target site directory does not already exist
            string physicalPath = string.Format("{0}\\{1}", AppSettings.AppsPhysicalDir, targetSiteName);
            if (System.IO.Directory.Exists(physicalPath))
                throw new Exception("Site with name '" + targetSiteName + "' is in use already. Please try an alternate name.");

            // Create a new site
            Site targetSite = new Site()
            {
                ID = Guid.NewGuid().ToString("N"),
                IsHostedOnIIS = true,
                Name = targetSiteName,
                Title = targetTitle,
                Description = targetSiteDescription,
                PhysicalPath = physicalPath,
                Url = string.Format("{0}/{1}", AppSettings.AppsBaseUrl.TrimEnd('/'), targetSiteName.TrimStart('/'))
            };

            Version currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            targetSite.ProductVersion = string.Format("{0}.{1}.{2}.0", currentVersion.Major, currentVersion.Minor,
                currentVersion.Build);

            // Create the viewer application
            ApplicationBuilderHelper appBuilderHelper = new ApplicationBuilderHelper();
            FaultContract Fault = null;
            if (!appBuilderHelper.CreateSiteFromTemplate(targetSiteTemplateId, targetSite, true, out Fault))
                throw new Exception(Fault != null ? Fault.Message : "Unable to create site");

            // Save the configuration files
            if (!appBuilderHelper.SaveConfigurationForSite(targetSite, targetSitePublishInfo, out Fault))
                return null;

            // Add entry to Sites.xml
            SiteConfiguration.AddSite(targetSite);

            // Return target site object
            return targetSite;
        }
    }
}
