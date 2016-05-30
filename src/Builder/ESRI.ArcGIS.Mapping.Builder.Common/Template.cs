/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
