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
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.Symbols;
using System.Windows.Markup;

namespace ESRI.ArcGIS.Mapping.Controls
{
	/// <summary>
	/// Extension methods to help get/set Clusterer properties
	/// </summary>
	public static class ClusterHelper
	{
		/// <summary>
		/// Returns layer's clusterer's MaximumFlareCount.  If not available, returns default
		/// </summary>
		/// <param name="layer"></param>
		/// <returns></returns>
		public static int GetClusterMaxPoints(this GraphicsLayer layer)
		{
			if (layer != null && layer.Clusterer is FlareClusterer)
			{
				FlareClusterer clusterer = layer.Clusterer as FlareClusterer;
				return clusterer.MaximumFlareCount;
			}
			else
				return (new FlareClusterer()).MaximumFlareCount;
		}

		public static void ChangeClusterMaxPoints(this GraphicsLayer layer, int newValue)
		{
			if (layer == null) return;
			FlareClusterer clusterer = layer.Clusterer as FlareClusterer;
			if (clusterer != null)
				clusterer.MaximumFlareCount = newValue;
		}

		/// <summary>
		/// Returns layer's clusterer's Radius.  If not available, returns default
		/// </summary>
		/// <param name="layer"></param>
		/// <returns></returns>
		public static int GetClusterRadius(this GraphicsLayer layer)
		{
			if (layer != null && layer.Clusterer is FlareClusterer)
			{
				FlareClusterer clusterer = layer.Clusterer as FlareClusterer;
				return clusterer.Radius;
			}
			else
				return (new FlareClusterer()).Radius;
		}

		public static void ChangeClusterRadius(this GraphicsLayer layer, int newValue)
		{
			if (layer == null) return;
			FlareClusterer clusterer = layer.Clusterer as FlareClusterer;
			if (clusterer != null)
			{
				clusterer.Radius = newValue;
				layer.Refresh();
			}
		}

		public static Color GetClusterFlareForeground(this GraphicsLayer layer)
		{
			if (layer == null) return Colors.Transparent;
			FlareClusterer clusterer = layer.Clusterer as FlareClusterer;
			if (clusterer != null && clusterer.FlareForeground is SolidColorBrush)
				return (clusterer.FlareForeground as SolidColorBrush).Color;
			return Colors.Transparent;
		}

		public static void ChangeClusterFlareForeground(this GraphicsLayer layer, Brush brush)
		{
			if (layer == null) return;
			FlareClusterer clusterer = layer.Clusterer as FlareClusterer;
			if (clusterer != null)
				clusterer.FlareForeground = brush;
		}

		public static void ChangeClusterFlareForeground(this GraphicsLayer layer, Color newColor)
		{
			ChangeClusterFlareForeground(layer, new SolidColorBrush(newColor));
		}

		public static Color GetClusterFlareBackground(this GraphicsLayer layer)
		{
			if (layer == null) return Colors.Transparent;
			FlareClusterer clusterer = layer.Clusterer as FlareClusterer;
			if (clusterer != null && clusterer.FlareBackground is SolidColorBrush)
				return (clusterer.FlareBackground as SolidColorBrush).Color;
			return Colors.Transparent;
		}

		public static void ChangeClusterFlareBackground(this GraphicsLayer layer, Brush brush)
		{
			if (layer == null) return;
			FlareClusterer clusterer = layer.Clusterer as FlareClusterer;
			if (clusterer != null)
				clusterer.FlareBackground = brush;
		}

		public static void ChangeClusterFlareBackground(this GraphicsLayer layer, Color newColor)
		{
			ChangeClusterFlareBackground(layer, new SolidColorBrush(newColor));
		}

		public static void RemoveClusterer(this GraphicsLayer layer)
		{
			if (layer == null) return;
			layer.Clusterer = null;
		}

        /// <summary>
        /// This method ensures the Clusterer object is not null.  If it is null, a new Clusterer with default settings is created.
        /// </summary>
		public static void ApplyClusterer(this GraphicsLayer layer)
		{
			if (layer == null) return;
            // Create a default Clusterer if one does not exist
            if (layer.Clusterer == null)
			    layer.Clusterer = new FlareClusterer();
		}

		public static bool GetUseClustering(this GraphicsLayer layer)
		{
			if (layer == null) return false;
			return layer.Clusterer != null;
		}
	}
}
