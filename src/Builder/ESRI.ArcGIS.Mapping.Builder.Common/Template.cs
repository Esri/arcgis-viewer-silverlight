/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.Builder.Common
{

    [Serializable]
    public class Template
    {
        [XmlElement]        
        public string ID { get; set; }

        [XmlElement]
        public string DisplayName { get; set; }

        [XmlElement]
        public bool IsDefault { get; set; }

        [XmlElement]
        public string BaseUrl { get; set; }
    }

}
