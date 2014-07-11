/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
	[DisplayName("GoToLayerDisplayName")]
	[Category("CategoryLayer")]
	[Description("GoToLayerDescription")]
    public class GoToLayerCommand : LayerCommandBase
    {
        // The Minimum Extent Ratio for the Map:
        private const double EXPAND_EXTENT_RATIO = .05;

        private Map Map
        {
            get
            {
                return MapApplication.Current.Map;
            }
        }

        private string GeometryServiceUrl {
            get
            {
                if (View.Instance != null && View.Instance.ConfigurationStore != null
                    && View.Instance.ConfigurationStore.GeometryServices != null
                    && View.Instance.ConfigurationStore.GeometryServices.Count > 0)
                {
                    return View.Instance.ConfigurationStore.GeometryServices[0].Url;
                }
                return null;
            }
        }

        public override bool CanExecute(object parameter)
        {
            return Layer != null && Layer.Visible == true;
        }

        public override void Execute(object parameter)
        {
            if (Layer == null)
                return;
            bool isServerLayer = false;
            Envelope targetExtent = null;
            TiledLayer tiledLayer = Layer as TiledLayer;
            if (tiledLayer != null)
            {
                isServerLayer = true;
                targetExtent = LayerExtentExtensions.GetTiledLayerFullExtent(tiledLayer); // get the cached value (if any) which will likely be in the map's spRef already
                if (targetExtent == null) // value not known, use value on service metadata (will likely be in the services' spatial ref and not neccesarily in map's spRef)
                    targetExtent = tiledLayer.FullExtent;
                if (tiledLayer is ESRI.ArcGIS.Client.Bing.TileLayer)
                {
                    if (targetExtent.SpatialReference == null)
                        targetExtent.SpatialReference = new SpatialReference(102100);
                }
            }

            DynamicLayer dynamicLayer = Layer as DynamicLayer;
            if (dynamicLayer != null)
            {
                isServerLayer = true;
                targetExtent = LayerExtentExtensions.GetDynamicLayerFullExtent(dynamicLayer); // get the cached value (if any) which will likely be in the map's spRef already
                if (targetExtent == null)// value not known, use value on service metadata (will likely be in the services' spatial ref and not neccesarily in map's spRef)
                    targetExtent = dynamicLayer.FullExtent;
            }

            if (isServerLayer)
            {
                if (targetExtent == null || Map == null || Map.SpatialReference == null)
                    return;

                if (Map.SpatialReference.Equals(targetExtent.SpatialReference))
                {
                    // spatial reference matches. can directly zoom
                    Map.ZoomTo(targetExtent);
                }
                else
                {
                    if (string.IsNullOrEmpty(GeometryServiceUrl))
                        throw new Exception(Resources.Strings.ExceptionNoGeometryServiceUrlSpecifiedLayerZoomedIsDifferentSpatialReference);

                    GeometryServiceOperationHelper helper = new GeometryServiceOperationHelper(GeometryServiceUrl);
                    helper.ProjectExtentCompleted += (o, e) =>
                    {
                        // Cache (save) the projected extent values
                        if (tiledLayer != null)
                            LayerExtentExtensions.SetTiledLayerFullExtent(tiledLayer, e.Extent);
                        else if (dynamicLayer != null)
                            LayerExtentExtensions.SetDynamicLayerFullExtent(dynamicLayer, e.Extent);
                        Map.ZoomTo(e.Extent);
                    };
                    helper.ProjectExtent(targetExtent, Map.SpatialReference);
                }
                return;
            }


            // Non server rendererd layers follow

            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            if (Map != null)
            {
                Envelope newMapExtent = null;
                if (graphicsLayer.Graphics.Count < 1)
                {
                    FeatureLayer featureLayer = graphicsLayer as FeatureLayer;
                    if (featureLayer != null && featureLayer.LayerInfo != null && featureLayer.LayerInfo.Extent != null)
                    {
                        Envelope env = featureLayer.LayerInfo.Extent;

                        SpatialReference sr = env.SpatialReference;
                        if (sr == null)
                            sr = featureLayer.LayerInfo.DefaultSpatialReference;

                        if (Map.SpatialReference.Equals(sr))
                        {
                            Map.PanTo(env);
                        }
                        else if (sr != null)
                        {
                            GeometryServiceOperationHelper geomHelper = new GeometryServiceOperationHelper(
                                new ConfigurationStoreHelper().GetGeometryServiceUrl(View.Instance.ConfigurationStore));
                            geomHelper.GeometryServiceOperationFailed += (o, args) =>
                            {
                                Logger.Instance.LogError(args.Exception);
                                throw args.Exception;
                            };
                            geomHelper.ProjectExtentCompleted += (o, args) =>
                            {
                                MapZoomOrPan(args.Extent);
                            };
                            geomHelper.ProjectExtent(env, Map.SpatialReference);
                        }
                    }
                }
                else
                {
                    foreach (Graphic graphic in graphicsLayer.Graphics)
                    {
                        if (graphic.Geometry != null && graphic.Geometry.Extent != null)
                            newMapExtent = graphic.Geometry.Extent.Union(newMapExtent);
                    }
                    newMapExtent = MapZoomOrPan(newMapExtent);
                }
            }
        }

        private Envelope MapZoomOrPan(Envelope newMapExtent)
        {
            if (newMapExtent != null)
            {
                if (newMapExtent.Width > 0 || newMapExtent.Height > 0)
                {
                    newMapExtent = new Envelope(newMapExtent.XMin - newMapExtent.Width * EXPAND_EXTENT_RATIO, newMapExtent.YMin - newMapExtent.Height * EXPAND_EXTENT_RATIO,
                        newMapExtent.XMax + newMapExtent.Width * EXPAND_EXTENT_RATIO, newMapExtent.YMax + newMapExtent.Height * EXPAND_EXTENT_RATIO);
                    Map.ZoomTo(newMapExtent);
                }
                else
                    Map.PanTo(newMapExtent);
            }
            return newMapExtent;
        }
    }

    public static class LayerExtentExtensions
    {
        #region DynamicLayerFullExtent
        /// <summary>
        /// Gets the value of the DynamicLayerFullExtent attached property for a specified DynamicLayer.
        /// </summary>
        /// <param name="element">The DynamicLayer from which the property value is read.</param>
        /// <returns>The DynamicLayerFullExtent property value for the DynamicLayer.</returns>
        public static Envelope GetDynamicLayerFullExtent(DynamicLayer element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return element.GetValue(DynamicLayerFullExtentProperty) as Envelope;
        }

        /// <summary>
        /// Sets the value of the DynamicLayerFullExtent attached property to a specified DynamicLayer.
        /// </summary>
        /// <param name="element">The DynamicLayer to which the attached property is written.</param>
        /// <param name="value">The needed DynamicLayerFullExtent value.</param>
        public static void SetDynamicLayerFullExtent(DynamicLayer element, Envelope value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(DynamicLayerFullExtentProperty, value);
        }

        /// <summary>
        /// Identifies the DynamicLayerFullExtent dependency property.
        /// </summary>
        public static readonly DependencyProperty DynamicLayerFullExtentProperty =
            DependencyProperty.RegisterAttached(
                "DynamicLayerFullExtent",
                typeof(Envelope),
                typeof(DynamicLayer),
                new PropertyMetadata(null));        
        #endregion

        #region TiledLayerFullExtent
        /// <summary>
        /// Gets the value of the TiledLayerFullExtent attached property for a specified TiledLayer.
        /// </summary>
        /// <param name="element">The TiledLayer from which the property value is read.</param>
        /// <returns>The TiledLayerFullExtent property value for the TiledLayer.</returns>
        public static Envelope GetTiledLayerFullExtent(TiledLayer element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return element.GetValue(TiledLayerFullExtentProperty) as Envelope;
        }

        /// <summary>
        /// Sets the value of the TiledLayerFullExtent attached property to a specified TiledLayer.
        /// </summary>
        /// <param name="element">The TiledLayer to which the attached property is written.</param>
        /// <param name="value">The needed TiledLayerFullExtent value.</param>
        public static void SetTiledLayerFullExtent(TiledLayer element, Envelope value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(TiledLayerFullExtentProperty, value);
        }

        /// <summary>
        /// Identifies the TiledLayerFullExtent dependency property.
        /// </summary>
        public static readonly DependencyProperty TiledLayerFullExtentProperty =
            DependencyProperty.RegisterAttached(
                "TiledLayerFullExtent",
                typeof(Envelope),
                typeof(LayerExtentExtensions),
                new PropertyMetadata(null));        
        #endregion
    }
}
