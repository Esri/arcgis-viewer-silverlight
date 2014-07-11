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
    public abstract class GetConfigurationStoreXmlHandlerBase : ServiceRequestHandlerBase
    {
        protected override void HandleRequest()
        {
            Response.Write(GetConfigurationStoreXml());
        }

        protected abstract string GetConfigurationStoreXml();
    }
}
