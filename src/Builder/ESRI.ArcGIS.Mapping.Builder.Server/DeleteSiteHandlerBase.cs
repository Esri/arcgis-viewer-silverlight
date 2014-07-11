/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace ESRI.ArcGIS.Mapping.Builder.Server
{
    public abstract class DeleteSiteHandlerBase : ServiceRequestHandlerBase
    {
        protected override void HandleRequest()
        {
            string siteId = Request["siteId"];
            DeleteSite(siteId);
        }

        protected abstract void DeleteSite(string siteId);
    }
}
