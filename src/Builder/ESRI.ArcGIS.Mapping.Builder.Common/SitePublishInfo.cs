/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ESRI.ArcGIS.Mapping.Builder.Common
{
    [Serializable]
    public class SitePublishInfo
    {
        [XmlElement]
        public string ApplicationXml { get; set; }
        [XmlElement]
        public string MapXaml { get; set; }
        [XmlElement]
        public string ToolsXml { get; set; }
        [XmlElement]
        public string ControlsXml { get; set; }
        [XmlElement]
        public string BehaviorsXml { get; set; }
        [XmlElement]
        public string ColorsXaml { get; set; }
        [XmlElement]
        public string [] ExtensionsXapsInUse { get; set;}
        [XmlElement]
        public byte[] PreviewImageBytes { get; set; }
    }
}
