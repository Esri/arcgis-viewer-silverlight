/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Xml.Serialization;

namespace ESRI.ArcGIS.Mapping.Builder
{
    /// <summary>
    /// Collection of information about a set of layouts
    /// </summary>
    [XmlRoot("Layouts")]
    public class LayoutInfoCollection
    {
        [XmlElement("Layout")]
        public List<LayoutInfo> Layouts { get; set; }
    }

    /// <summary>
    /// Represents metadata about a layout
    /// </summary>
    public class LayoutInfo
    {
        [XmlAttribute("File")]
        public string File { get; set; }

        [XmlAttribute("PreviewImage")]
        public string PreviewImage { get; set; }

        [XmlAttribute("DisplayName")]
        public string DisplayName { get; set; }
    }
}
