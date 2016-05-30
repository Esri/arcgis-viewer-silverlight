/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Web;
using ESRI.ArcGIS.Mapping.Builder.Common;
using ESRI.ArcGIS.Mapping.Builder.Server;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    public class GetExtensionsHandler : GetExtensionsHandlerBase
    {
        protected override Extensions GetExtensions()
        {            
            Extensions extensions = ExtensionsManager.GetExtensionLibraries();
            if (extensions != null)
            {
                string baseUrl = Request.Url.AbsoluteUri;
                int pos = baseUrl.IndexOf("Extensions/Get", StringComparison.OrdinalIgnoreCase);
                if (pos > -1)
                    baseUrl = baseUrl.Substring(0, pos);                
                extensions.BaseUrl = baseUrl;
                foreach (Extension extension in extensions)
                    extension.Url = string.Format("{0}/Extensions/{1}.xap", baseUrl.TrimEnd('/'), extension.Name);
            }
            return extensions;
        }
    }
}
