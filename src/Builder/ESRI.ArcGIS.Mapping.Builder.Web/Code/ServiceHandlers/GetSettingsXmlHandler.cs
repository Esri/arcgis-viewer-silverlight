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

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    public class GetSettingsXmlHandler : GetSettingsXmlHandlerBase
    {
        const string SETTINGSCONFIGPATH = "~/App_Data/Settings.xml";
        protected override string GetSettingsXml()
        {
            return ConfigurationStoreManager.GetFileContents(ServerUtility.MapPath(SETTINGSCONFIGPATH));
        }
    }
}
