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
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Geometry;
using dataSources = ESRI.ArcGIS.Mapping.DataSources.ArcGISServer;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    [DataContract]
    public class BasemapInfo
    {
        [DataMember(Name = "baseMapLayers")]
        public List<dataSources.LayerDescription> BasemapLayers { get; set; }

        [DataMember(Name = "operationalLayers")]
        public List<dataSources.LayerDescription> OperationalLayers { get; set; }
         
        [DataMember(Name = "id", IsRequired = false)]
        public string ID { get; set; }

        [DataMember(Name = "title", IsRequired = false)]
        public string Title { get; set; }
    }
}
