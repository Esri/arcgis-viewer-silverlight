/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Xml;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Core
{
    public interface ICustomLayer
    {        
        void Serialize(XmlWriter writer, Dictionary<string, string> Namespaces);
    }
}
