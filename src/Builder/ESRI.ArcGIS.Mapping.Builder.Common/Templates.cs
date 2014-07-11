/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.Builder.Common
{
    [XmlRoot("Templates")]
    [Serializable]
    public class Templates : List<Template>
    {
        
    }
}
