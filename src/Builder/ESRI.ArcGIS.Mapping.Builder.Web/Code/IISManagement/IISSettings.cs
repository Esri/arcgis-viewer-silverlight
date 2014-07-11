/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.Runtime.Serialization;
using System.Xml.Serialization;


namespace ESRI.ArcGIS.Mapping.Builder.Web
{    
    [Serializable]
    public class IISSettings
    { 
        [XmlElement]
        public string IISHost { get; set; }
     
        [XmlElement]
        public int IISPort { get; set; }
        
        [XmlElement]
        public string IISPath { get; set; }
    }
}
