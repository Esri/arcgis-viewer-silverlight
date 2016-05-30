/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Core.Symbols
{
    public class CustomSymbolXamlWriter : XamlWriterBase
    {
        public CustomSymbolXamlWriter(XmlWriter writer, Dictionary<string, string> namespaces)
            : base(writer, namespaces)
        {
        }
    }
}
