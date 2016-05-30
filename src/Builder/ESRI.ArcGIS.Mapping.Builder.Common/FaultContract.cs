/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
