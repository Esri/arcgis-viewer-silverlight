/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Mapping.Builder.Common;
using System.Xml.Serialization;

namespace ESRI.ArcGIS.Mapping.Builder.Server
{
    public abstract class GetSitesHandlerBase : ServiceRequestHandlerBase
    {
        protected override void HandleRequest()
        {
            Sites sites = GetSites();

            XmlSerializer serializer = new XmlSerializer(typeof(Sites));
            serializer.Serialize(Response.OutputStream, sites);
        }

        protected abstract Sites GetSites();
    }
}
