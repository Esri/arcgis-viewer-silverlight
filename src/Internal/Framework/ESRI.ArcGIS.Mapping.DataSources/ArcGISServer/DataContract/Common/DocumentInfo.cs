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

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    [DataContract]
    public class DocumentInfo
    {
        [DataMember(IsRequired = false)]
        public string Title { get; set; }

        [DataMember(IsRequired = false)]
        public string Author { get; set; }

        [DataMember(IsRequired = false)]
        public string Comments { get; set; }

        [DataMember(IsRequired = false)]
        public string Subject { get; set; }

        [DataMember(IsRequired = false)]
        public string Category { get; set; }

        [DataMember(IsRequired = false)]
        public string Keywords { get; set; }

        [DataMember(IsRequired = false)]
        public string Credits { get; set; }
    }
}
