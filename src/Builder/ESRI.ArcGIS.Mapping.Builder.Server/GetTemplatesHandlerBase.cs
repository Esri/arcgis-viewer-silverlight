/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Mapping.Builder.Common;
using System.Xml.Serialization;

namespace ESRI.ArcGIS.Mapping.Builder.Server
{
    public abstract class GetTemplatesHandlerBase : ServiceRequestHandlerBase
    {
        protected override void HandleRequest()
        {
            Templates templates = GetTemplates();

            XmlSerializer writer = new XmlSerializer(typeof(Templates));
            writer.Serialize(Response.OutputStream, templates);
        }

        protected abstract Templates GetTemplates();
    }
}
