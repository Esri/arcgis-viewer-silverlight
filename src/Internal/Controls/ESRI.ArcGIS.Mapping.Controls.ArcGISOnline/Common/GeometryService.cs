/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Tasks;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{

  /// <summary>
  /// Helper class for working with the geometry service.
  /// </summary>
  public class GeometryService
  {
    static string Url
    {
      get { return ArcGISOnlineEnvironment.ConfigurationUrls.GeometryServer; }
    }

    /// <summary>
    /// Projects a single geometry to the specified spatial reference.
    /// </summary>
    private static void ProjectAsync(Geometry geometry, SpatialReference spatialReference, EventHandler<GeometryEventArgs> callback)
    {
      CallbackHelper helper = new CallbackHelper(callback);

      ESRI.ArcGIS.Client.Tasks.GeometryService geometryService = new ESRI.ArcGIS.Client.Tasks.GeometryService(Url);

      geometryService.ProjectCompleted += helper.geometryService_ProjectCompleted;
      geometryService.ProjectAsync(new List<Graphic>(new Graphic[] { new Graphic() { Geometry = geometry } }), spatialReference);
    }

    /// <summary>
    /// Simplifies geometry that has just been edited by the user
    /// </summary>
    public static void SimplifyGraphicAsync(Graphic graphic, EventHandler<GraphicsEventArgs> callback)
    {
        List<Graphic> list = new List<Graphic> {graphic};
        ESRI.ArcGIS.Client.Tasks.GeometryService geometryService = new ESRI.ArcGIS.Client.Tasks.GeometryService(Url);
        geometryService.SimplifyCompleted -= geometryService_SimplifyCompleted;
        geometryService.SimplifyCompleted += geometryService_SimplifyCompleted;
        geometryService.SimplifyAsync(list, callback);
    }

    static void geometryService_SimplifyCompleted(object sender, GraphicsEventArgs e)
    {
        if (e.UserState != null)
            ((EventHandler<GraphicsEventArgs>)e.UserState).Invoke(sender, e);
    }

    class CallbackHelper
    {
      EventHandler<GeometryEventArgs> _callback;

      public CallbackHelper(EventHandler<GeometryEventArgs> callback)
      {
        _callback = callback;
      }

      public void geometryService_ProjectCompleted(object sender, GraphicsEventArgs e)
      {
        if (e.Results.Count == 1)
          _callback(null, new GeometryEventArgs() { Geometry = e.Results[0].Geometry });
        else
          _callback(null, new GeometryEventArgs());
      }
    }

    public static void ProjectEnvelopeAsync(Envelope envelope, SpatialReference spatialReference, EventHandler<GeometryEventArgs> callback)
    {
      if (envelope.SpatialReference.WKID == spatialReference.WKID)
      {
        callback(null, new GeometryEventArgs() { Geometry = envelope });
        return;
      }

      if ((envelope.SpatialReference.WKID == WKIDs.WebMercator || envelope.SpatialReference.WKID == WKIDs.WebMercatorAuxiliarySphere) && spatialReference.WKID == WKIDs.Geographic)
      {
        MapPoint p1 = new MapPoint(Math.Max(envelope.XMin, -19977660), Math.Max(envelope.YMin, -43076226));
        MapPoint p2 = new MapPoint(Math.Min(envelope.XMax, 19998531), Math.Min(envelope.YMax, 33656597));
        Envelope output = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.WebMercatorToGeographic(p1),
                                       ESRI.ArcGIS.Client.Bing.Transform.WebMercatorToGeographic(p2));
        output.SpatialReference = spatialReference;
        callback(null, new GeometryEventArgs() { Geometry = output });
        return;
      }
      
      if (envelope.SpatialReference.WKID == WKIDs.Geographic && (spatialReference.WKID == WKIDs.WebMercator || spatialReference.WKID == WKIDs.WebMercatorAuxiliarySphere))
      {
        MapPoint p1 = new MapPoint(envelope.XMin, envelope.YMin);
        MapPoint p2 = new MapPoint(envelope.XMax, envelope.YMax);
        Envelope output = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(p1),
                                       ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(p2));
        output.SpatialReference = spatialReference;
        callback(null, new GeometryEventArgs() { Geometry = output });
        return;
      }

      ProjectAsync(envelope, spatialReference, callback);
    }
  }

  /// <summary>
  /// Represents the arguments for a geometry operation.
  /// </summary>
  public class GeometryEventArgs : EventArgs
  {
    public ESRI.ArcGIS.Client.Geometry.Geometry Geometry { get; set; }
  }

  /// <summary>
  /// Represents the results of an area request of a polygon.
  /// </summary>
  public class PolygonAreaEventArgs : EventArgs
  {
    public double Area { get; set; }
  }

  public static class WKIDs
  {
    public static int CylindricalEqualAreaWorld { get { return 54034; } }
    public static int WebMercatorAuxiliarySphere { get { return 102100; } }
    public static int WebMercator { get { return 102133; } }
    public static int Geographic { get { return 4326; } }
  }
}
