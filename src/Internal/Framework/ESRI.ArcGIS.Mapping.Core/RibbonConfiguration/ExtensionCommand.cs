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
using System.Runtime.Serialization;

namespace ESRI.ArcGIS.Mapping.Core
{
    [DataContract]
    public class ExtensionCommand
    {
        [DataMember(EmitDefaultValue = false)]
        public string ControlId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string CommandValueId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Command { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public string QueryCommand { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Title { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Path { get; set; }
    }
}
