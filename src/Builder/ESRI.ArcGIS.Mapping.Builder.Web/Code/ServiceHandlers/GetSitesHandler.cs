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
    public class GetSitesHandler : GetSitesHandlerBase
    {
        protected override Sites GetSites()
        {
            List<Site> sites = SiteConfiguration.GetSites();
            Sites clonedSites = new Sites();
            foreach (Site site in sites)
                clonedSites.Add(new Site() { ID = site.ID, Name = site.Name, Url = site.Url, 
                    ProductVersion = site.ProductVersion });
            return clonedSites;
        }
    }
}
