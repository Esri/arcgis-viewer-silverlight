/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Represents the fundamental information for a map layer based on a service.
  /// </summary>
  /// <remarks>
  /// The LayerDescription corresponds to the JSON object that is stored in the AGOL Web Map
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

    [DataMember(Name = "url", EmitDefaultValue = false)]
    public string Url { get; set; }

    [DataMember(Name = "visibility")]
    public bool Visible { get; set; }

    [DataMember(Name = "isReference", EmitDefaultValue = false)]
    public bool IsReference { get; set; }

    [DataMember(Name = "opacity")]
    public double Opacity { get; set; }

    [DataMember(Name = "type", EmitDefaultValue = false)]
    public string LayerType { get; set; }

    [DataMember(Name = "title", EmitDefaultValue = false)]
    public string Title { get; set; }

    [DataMember(Name = "visibleLayers", EmitDefaultValue = false)]
    public List<int> VisibleLayers { get; set; }

    [DataMember(Name = "mode")]
    public ESRI.ArcGIS.Client.FeatureLayer.QueryMode QueryMode { get; set; }

    /// <summary>
    /// Get the service info for the layer description asynchronously. When the call completes, the MapService
    /// property will be set.
    /// </summary>
    /// <param name="callback"></param>
    public void GetServiceInfoAsync(EventHandler<AsyncEventArgs> callback)
    {
      // test for custom layers first - TODO remove this logic when AGOL web maps can handle bing and OSM
      //
      if (LayerType == "BingMapsAerial" || LayerType == "BingMapsRoad" || LayerType == "BingMapsHybrid" || LayerType == "OpenStreetMap")
      {
        Service = new MapService() { SpatialReference = new SpatialReference(WKIDs.WebMercatorAuxiliarySphere), Units = "esriMeters" };
        callback(this, new AsyncEventArgs() { Succeeded = true });
        return;
      }

      ArcGISService.GetServiceInfoAsync(Url, null, (sender, e) =>
        {
          Service = e.Service;
          callback(this, new AsyncEventArgs() { Succeeded = e.Service != null });
        });
    }

    /// <summary>
    /// Gets/sets the service for the LayerDescription.
    /// </summary>
    public ArcGISService Service { get; set; }

  }

  /// <summary>
  /// Represents a collection of Basemaps.
  /// </summary>
  public class ServiceLayerCollection : ObservableCollection<LayerDescription>
  {
  }
}
