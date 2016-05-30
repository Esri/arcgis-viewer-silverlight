/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client.Geometry;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Represents an ArcGIS Server service such as a MapService or ImageService.
  /// </summary>
  [DataContract]
  public class ArcGISService
  {
    /// <summary>
    /// Gets or sets the title of the arcgis service.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the url of the arcgis service.
    /// </summary>
    public string Url { get; set; }

    [DataMember(Name = "spatialReference")]
    public ESRI.ArcGIS.Client.Geometry.SpatialReference SpatialReference { get; set; }

    [DataMember(Name = "units")]
    public string Units { get; set; }

    [DataMember(Name = "description")]
    public string Description { get; set; }

    [DataMember(Name = "serviceDescription")]
    public string ServiceDescription { get; set; }

    [DataMember(Name = "copyrightText")]
    public string CopyrightText { get; set; }

    public bool RequiresProxy { get; set; }

    /// <summary>
    /// Gets the service information for a MapService asynchronously.
    /// </summary>
    public static void GetServiceInfoAsync(string url, object userState, EventHandler<ServiceEventArgs> callback)
    {
      WebUtil.OpenReadAsync(new Uri(url + "?f=json"), null, (sender, e) =>
        {
          if (e.Error != null)
          {
            callback(null, new ServiceEventArgs());
            return;
          }

          MapService mapService = WebUtil.ReadObject<MapService>(e.Result);
          if (mapService != null && mapService.Name != null && mapService.Units != null)
          {
            mapService.Url = url;
            mapService.RequiresProxy = e.UsedProxy;
            mapService.InitTitle();
            callback(null, new ServiceEventArgs() { Service = mapService, UserState = userState });
            return;
          }

          FeatureService featureService = WebUtil.ReadObject<FeatureService>(e.Result);
          if (featureService != null && featureService.Layers != null && featureService.Layers.Length > 0)
          {
            featureService.Url = url;
            featureService.RequiresProxy = e.UsedProxy;
            featureService.InitTitle();
            callback(null, new ServiceEventArgs() { Service = featureService, UserState = userState });
            return;
          }

          ImageService imageService = WebUtil.ReadObject<ImageService>(e.Result);
          if (imageService != null && imageService.PixelType != null)
          {
            imageService.Url = url;
            imageService.RequiresProxy = e.UsedProxy;
            imageService.InitTitle();
            callback(null, new ServiceEventArgs() { Service = imageService, UserState = userState });
            return;
          }

          FeatureLayerService featureLayerService = WebUtil.ReadObject<FeatureLayerService>(e.Result);
          if (featureLayerService != null && featureLayerService.Type == "Feature Layer")
          {
            featureLayerService.Url = url;
            featureLayerService.RequiresProxy = e.UsedProxy;
            featureLayerService.Title = featureLayerService.Name;
            callback(null, new ServiceEventArgs() { Service = featureLayerService, UserState = userState });
            return;
          }

          callback(null, new ServiceEventArgs());
        });
    }
    void InitTitle()
    {
      // construct a title for the map service from the directory structure of the url
      //
      string[] tokens = Url.Split(new char[] { '/'}, StringSplitOptions.RemoveEmptyEntries);
      if (tokens.Length > 2)
        Title = tokens[tokens.Length - 2];
      else
        Title = Url;
    }
  }

  /// <summary>
  /// Represents an ArcGIS Server MapService.
  /// </summary>
  [DataContract]
  public class MapService : ArcGISService
  {
    [DataMember(Name = "singleFusedMapCache")]
    public bool IsTiled { get; set; }

    [DataMember(Name = "mapName")]
    public string Name { get; set; }
  
      [DataMember(Name = "tileInfo")]
    public TileInfoInfo TileInfo { get; set; }
  
  }

  [DataContract]
  public class TileInfoInfo
  {
      // Properties
      [DataMember(Name = "cols")]
      public int Cols { get; set; }
      [DataMember(Name = "compressionQuality")]
      public string CompressionQuality { get; set; }
      [DataMember(Name = "dpi")]
      public int DPI { get; set; }
      [DataMember(Name = "format")]
      public string Format { get; set; }
      [DataMember(Name = "lods")]
      public LODInfo[] LODs { get; set; }
      [DataMember(Name = "origin")]
      public MapPoint Origin { get; set; }
      [DataMember(Name = "rows")]
      public int Rows { get; set; }
  }

  [DataContract]
  public class LODInfo
  {
      // Properties
      [DataMember(Name = "level")]
      public int Level { get; set; }
      [DataMember(Name = "resolution")]
      public double Resolution { get; set; }
      [DataMember(Name = "scale")]
      public double Scale { get; set; }
  }



  /// <summary>
  /// Represents an ArcGIS Server FeatureService.
  /// </summary>
  [DataContract]
  public class FeatureService : ArcGISService
  {
    [DataMember(Name = "layers")]
    public FeatureLayerDescription[] Layers { get; set; }
  }  
  
  /// <summary>
  /// Represents an ArcGIS Server FeatureLayerDescription.
  /// </summary>
  [DataContract]
  public class FeatureLayerDescription
  {
    [DataMember(Name = "id")]
    public int Id { get; set; }
    
    [DataMember(Name = "name")]
    public string Name { get; set; }
  }

  /// <summary>
  /// Represents an ArcGIS Server FeatureLayerService.
  /// </summary>
  [DataContract]
  public class FeatureLayerService : ArcGISService
  {
    Envelope _extent;

    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "type")]
    public string Type { get; set; }

    [DataMember(Name = "geometryType")]
    public string GeometryType { get; set; }

    [DataMember(Name = "extent")]
    public Envelope Extent
    {
      get { return _extent; }
      set
      {
        _extent = value;
        if (_extent != null)
          SpatialReference = _extent.SpatialReference;
      }
    }

  }
  
  /// <summary>
  /// Represents an ArcGIS Server MapService.
  /// </summary>
  [DataContract]
  public class ImageService : ArcGISService
  {
    Envelope _extent;

    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "extent")]
    public Envelope Extent
    {
      get { return _extent; }
      set
      {
        _extent = value;
        if (_extent != null)
          SpatialReference = _extent.SpatialReference;
      }
    }

    [DataMember(Name = "serviceDataType")]
    public string ServiceDataType { get; set; }

    [DataMember(Name = "bandCount")]
    public int BandCount { get; set; }

    [DataMember(Name = "pixelSizeX")]
    public double PixelSizeX { get; set; }

    [DataMember(Name = "pixelSizeY")]
    public double PixelSizeY { get; set; }

    [DataMember(Name = "pixelType")]
    public string PixelType { get; set; }
  }

  /// <summary>
  /// Provides data for asynchronous calls to get service information.
  /// </summary>
  public class ServiceEventArgs : EventArgs
  {
    public ArcGISService Service { get; set; }
    public object UserState { get; set; }
  }

  /// <summary>
  /// Represents an ArGIS Server Services Directory.
  /// </summary>
  [DataContract]
  public class ServicesDirectory
  {
    [DataMember(Name = "folders")]
    public string[] Folders { get; set; }

    [DataMember(Name = "services")]
    public ServiceDescription[] Services { get; set; }
  }

  /// <summary>
  /// Represents an ArGIS Service description.
  /// </summary>
  [DataContract]
  public class ServiceDescription
  {
    [DataMember(Name = "name")]
    public string Title { get; set; }

    [DataMember(Name = "type")]
    public string Type { get; set; }

    public string Url { get; set; }
  }

  /// <summary>
  /// Represents a collection of legends for MapService layers.
  /// </summary>
  [DataContract]
  public class MapServiceLayerLegends
  {
    [DataMember(Name = "layers")]
    public MapServiceLayerLegend[] LayerLegends { get; set; }
  }

  /// <summary>
  /// Represents the legend for a MapService sublayer.
  /// </summary>
  [DataContract]
  public class MapServiceLayerLegend
  {
    [DataMember(Name = "layerId")]
    public string LayerId { get; set; }
    [DataMember(Name = "layerName")]
    public string LayerName { get; set; }
    [DataMember(Name = "legend")]
    public LegendItem[] LegendItems { get; set; }

    /// <summary>
    /// Gets the layer legends of sublayers of the layer that is 
    /// represented by this MapServiceLayerLegend.
    /// </summary>
    public List<MapServiceLayerLegend> Children { get; set; }

    /// <summary>
    /// Gets the layer legends of sublayers that are visible.
    /// </summary>
    public ObservableCollection<MapServiceLayerLegend> VisibleChildren { get; set; }


    /// <summary>
    /// Searches for the MapServiceLayerLegend whose Children collection contains the 
    /// specified layerLegend.
    /// </summary>
    /// <param name="layerLegend">The MapServiceLayerLegend of a MapService sublayer.</param>
    /// <returns></returns>
    public MapServiceLayerLegend FindContainer(MapServiceLayerLegend layerLegend)
    {
      if (Children.Contains(layerLegend))
        return this;

      foreach (MapServiceLayerLegend childLayerLegend in Children)
      {
        MapServiceLayerLegend container = childLayerLegend.FindContainer(layerLegend);
        if (container != null)
          return container;
      }
      return null;
    }
  }

  /// <summary>
  /// Represents an individual symbol description of a layer legend.
  /// </summary>
  [DataContract]
  public class LegendItem
  {
    [DataMember(Name = "label")]
    public string Label { get; set; }
    [DataMember(Name = "imgBytes")]
    public string SymbolBytes { get; set; } //legend swatches returned in byte format

    public ImageSource Symbol
    {
      get 
      {
        if (!string.IsNullOrEmpty(SymbolBytes))
        {
          byte[] bytes = System.Convert.FromBase64String(SymbolBytes);

          BitmapImage image = new BitmapImage();

          using(MemoryStream mS = new MemoryStream(bytes))
            image.SetSource(mS);

          return image;
        }

        return null;
      }
    }
  }
}
