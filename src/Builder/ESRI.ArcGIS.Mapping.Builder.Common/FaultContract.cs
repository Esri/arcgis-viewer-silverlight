/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Xml.Serialization;

namespace ESRI.ArcGIS.Mapping.Builder.Common
{
    [Serializable]
    public class FaultContract
    {
        [XmlElement]
        public string FaultType { get; set; }
        [XmlElement]
        public string Message { get; set; }
    }
}
