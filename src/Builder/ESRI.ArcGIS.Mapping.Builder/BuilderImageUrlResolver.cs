/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder;
using System.Linq;
using ESRI.ArcGIS.Mapping.Builder.Common;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class BuilderImageUrlResolver : IUrlResolver
    {

        public string ResolveUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;
            BuilderApplication instance = BuilderApplication.Instance;
            Site currentSite = instance.CurrentSite;
            if (currentSite == null) // not editing a site
            {
                if (instance.Templates == null)
                    return url;

                Template defaultTemplate = instance.Templates.FirstOrDefault<Template>(t => t.IsDefault);
                if (defaultTemplate == null && instance.Templates.Count > 0)
                    defaultTemplate = instance.Templates[0];

                if (defaultTemplate != null)
                    return string.Format("{0}/{1}", defaultTemplate.BaseUrl.TrimEnd('/'),  url.TrimStart('/'));
            }
            else
            {
                if (currentSite.Url != null)
                    return string.Format("{0}/{1}", currentSite.Url.TrimEnd('/'), url.TrimStart('/'));
            }

            return url;
        }
    }
}
