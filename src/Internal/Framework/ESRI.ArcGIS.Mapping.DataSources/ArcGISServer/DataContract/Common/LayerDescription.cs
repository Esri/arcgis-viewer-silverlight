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

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    /// <summary>
    /// Represents the information for a map layer based on a service.
    /// </summary>
    /// <remarks>
    /// The LayerDescription corresponds to the JSON object that is stored in the Web Map
    /// for both operational and basemap layers.
    /// </remarks>
    [DataContract]
    public class LayerDescription
    {
        public LayerDescription()
        {
            Visible = true;
            Opacity = 1;
            QueryMode = ESRI.ArcGIS.Client.FeatureLayer.QueryMode.OnDemand;
        }

        [DataMember(Name = "url", EmitDefaultValue = false, IsRequired = false)]
        public string Url { get; set; }

        [DataMember(Name = "visibility", IsRequired = false)]
        public bool Visible { get; set; }

        [DataMember(Name = "isReference", EmitDefaultValue = false, IsRequired = false)]
        public bool IsReference { get; set; }

        [DataMember(Name = "opacity", IsRequired = false)]
        public double Opacity { get; set; }

        [DataMember(Name = "type", EmitDefaultValue = false, IsRequired = false)]
        public string LayerType { get; set; }

        [DataMember(Name = "title", EmitDefaultValue = false, IsRequired = false)]
        public string Title { get; set; }

        [DataMember(Name = "visibleLayers", EmitDefaultValue = false, IsRequired = false)]
        public List<int> VisibleLayers { get; set; }

        [DataMember(Name = "mode", IsRequired = false)]
        public ESRI.ArcGIS.Client.FeatureLayer.QueryMode QueryMode { get; set; }

        [DataMember(Name = "resourceInfo", IsRequired = false)]
        public ResourceInfo ResourceInfo { get; set; }
    }
}
