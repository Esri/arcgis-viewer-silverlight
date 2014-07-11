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
using ESRI.ArcGIS.Mapping.Builder.Common;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    public class GetTemplatesHandler : GetTemplatesHandlerBase
    {
        protected override Templates GetTemplates()
        {
            string baseUrl = Request.Url.AbsoluteUri;
            int pos = baseUrl.IndexOf("Templates/Get", StringComparison.OrdinalIgnoreCase);
            if (pos > -1)
                baseUrl = baseUrl.Substring(0, pos);         
            Templates templates = TemplateConfiguration.GetTemplates();
            foreach (Template template in templates)
            {
                template.BaseUrl = string.Format("{0}/Templates/{1}", baseUrl.TrimEnd('/'), template.ID);
            }
            return templates;
        }
    }
}
