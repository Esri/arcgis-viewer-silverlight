/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    static class AppSettings
    {
        public static string AppsPhysicalDir
        {
            get 
            {
                string appsPhyiscalDir = System.Web.Configuration.WebConfigurationManager.AppSettings["AppsPhysicalDir"];
                if (string.IsNullOrEmpty(appsPhyiscalDir))
                    throw new Exception("Physical path has not been configured. Please contact the system administrator for help.") { };
                
                return appsPhyiscalDir;
            }
        }

        public static string AppsBaseUrl
        {
            get
            {
                string appsBaseUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["AppsBaseUrl"];
                if (string.IsNullOrEmpty(appsBaseUrl))
                    throw new Exception("Base Url has not been configured. Please contact the system administrator for help.");

                return appsBaseUrl;
            }
        }
    }
}
