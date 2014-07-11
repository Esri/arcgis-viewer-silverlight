/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using System;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
	public class HeatMapFeatureLayerHelper
	{
		public static void AddHeatMapLayer(Layer layer, View view)
		{
			Map map = view.Map;
            string originalTitle = string.Empty;

            originalTitle = layer.GetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty) as string;

			FeatureLayer featureLayer = layer as FeatureLayer;
            if (featureLayer != null && !string.IsNullOrEmpty(featureLayer.Url))
            {
                HeatMapFeatureLayer heatMapFeatureLayer = new HeatMapFeatureLayer();
                heatMapFeatureLayer.DisableClientCaching = featureLayer.DisableClientCaching;
                heatMapFeatureLayer.Geometry = featureLayer.Geometry;
                heatMapFeatureLayer.ProxyUrl = featureLayer.ProxyUrl;
                heatMapFeatureLayer.Text = featureLayer.Text;
                heatMapFeatureLayer.Token = featureLayer.Token;
                heatMapFeatureLayer.Url = featureLayer.Url;
                heatMapFeatureLayer.Where = featureLayer.Where;
                heatMapFeatureLayer.MapSpatialReference = map.SpatialReference;
                heatMapFeatureLayer.ID = "EsriHeatMapLayer__" + Guid.NewGuid().ToString("N");
                foreach (Graphic item in featureLayer.Graphics)
                {
                    ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint = item.Geometry as ESRI.ArcGIS.Client.Geometry.MapPoint;
                    if (mapPoint != null)
                        heatMapFeatureLayer.HeatMapPoints.Add(mapPoint);                    
                }                
                view.AddLayerToMap(heatMapFeatureLayer, true,
					string.IsNullOrEmpty(originalTitle) ? Resources.Strings.HeatMap : string.Format(Resources.Strings.HeatMapTitle, originalTitle));
            }
            else
            {
                GraphicsLayer graphicsLayer = layer as GraphicsLayer;
                if(graphicsLayer != null)
                {
                    ESRI.ArcGIS.Client.Toolkit.DataSources.HeatMapLayer heatMapLayer = new Client.Toolkit.DataSources.HeatMapLayer();
                    foreach (Graphic item in graphicsLayer.Graphics)
                    {
                        ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint = item.Geometry as ESRI.ArcGIS.Client.Geometry.MapPoint;
                        if (mapPoint != null)
                            heatMapLayer.HeatMapPoints.Add(mapPoint);
                    }
                    view.AddLayerToMap(heatMapLayer, true,
										string.IsNullOrEmpty(originalTitle) ? Resources.Strings.HeatMap : string.Format(Resources.Strings.HeatMapTitle, originalTitle));
                }
            }			
		}

		public static bool SupportsLayer(Layer layer)
		{
			if (layer == null)
				return false;
            GraphicsLayer input = layer as GraphicsLayer;
			if (input == null || input.Graphics.Count < 1 ||
					(!(input.Graphics[0].Geometry is ESRI.ArcGIS.Client.Geometry.MapPoint)))
				return false;
			return true;
		}

	}
}
