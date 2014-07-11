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
using System.Xml;

namespace ESRI.ArcGIS.Mapping.Builder.Server
{
    internal class Utils
    {
        public static SitePublishInfo GetSitePublishInfoFromXml(XmlElement rootElem)
        {
            SitePublishInfo info = new SitePublishInfo();
            XmlElement elem = rootElem.SelectSingleNode("ApplicationXml") as XmlElement;
            if (elem != null)
                info.ApplicationXml = elem.InnerText;
            elem = rootElem.SelectSingleNode("MapXaml") as XmlElement;
            if (elem != null)
                info.MapXaml = elem.InnerText;
            elem = rootElem.SelectSingleNode("BehaviorsXml") as XmlElement;
            if (elem != null)
                info.BehaviorsXml = elem.InnerText;
            elem = rootElem.SelectSingleNode("ColorsXaml") as XmlElement;
            if (elem != null)
                info.ColorsXaml = elem.InnerText;
            elem = rootElem.SelectSingleNode("ControlsXml") as XmlElement;
            if (elem != null)
                info.ControlsXml = elem.InnerText;
            elem = rootElem.SelectSingleNode("ToolsXml") as XmlElement;
            if (elem != null)
                info.ToolsXml = elem.InnerText;
            elem = rootElem.SelectSingleNode("PreviewImageBytes") as XmlElement;
            if (elem != null && elem.InnerText != null)
                info.PreviewImageBytes = Encoding.UTF8.GetBytes(elem.InnerText);
            elem = rootElem.SelectSingleNode("ExtensionsXapsInUse") as XmlElement;
            if (elem != null && elem.InnerText != null)
                info.ExtensionsXapsInUse = elem.InnerText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            return info;
        }

    }
}
