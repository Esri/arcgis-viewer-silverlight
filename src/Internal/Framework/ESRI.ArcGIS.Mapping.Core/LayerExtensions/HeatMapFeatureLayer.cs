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

namespace ESRI.ArcGIS.Mapping.Core
{
	public class HeatMapFeatureLayer : HeatMapLayerBase, ICustomLayer
	{
		public string Url { get; set; }
		public bool DisableClientCaching { get; set; }
		public string Token { get; set; }
		public string ProxyUrl { get; set; }
		public string Where { get; set; }
		public Geometry Geometry { get; set; }
		public string Text { get; set; }
		QueryTask task;

		public override void Initialize()
		{
			#region Do query
			if (task != null)
			{
				task.ExecuteCompleted -= task_QueryComplete;
				task.Failed -= task_Fault;
				if (task.IsBusy)
					task.CancelAsync();
			}
			else
			{
				task = new QueryTask(Url)
				{
					DisableClientCaching = this.DisableClientCaching,
					Token = this.Token,
					ProxyURL = this.ProxyUrl
				};
			}
			task.ExecuteCompleted += task_QueryComplete;
			task.Failed += task_Fault;

			#region Set up query
			Query query = new Query();
			if (string.IsNullOrEmpty(Where) && string.IsNullOrEmpty(Text) && Geometry == null)
				query.Where = "1=1"; //Default to all features
			else
				query.Where = Where;
			query.Text = Text;
			query.OutSpatialReference = MapSpatialReference;
			query.Geometry = Geometry;
			query.ReturnGeometry = true;
			#endregion

			base.OnProgress(0);
			task.ExecuteAsync(query);
			#endregion
		}

		private void task_QueryComplete(object sender, QueryEventArgs args)
		{
			base.OnProgress(100);

			if (args.FeatureSet != null)
			{
				ESRI.ArcGIS.Client.Geometry.PointCollection points = new ESRI.ArcGIS.Client.Geometry.PointCollection();
				foreach (Graphic g in args.FeatureSet.Features)
				{
					if (g.Geometry is ESRI.ArcGIS.Client.Geometry.MapPoint)
					{
						points.Add(g.Geometry as ESRI.ArcGIS.Client.Geometry.MapPoint);
						if (this.SpatialReference == null)
						{
							if (g.Geometry.SpatialReference != null)
								this.SpatialReference = g.Geometry.SpatialReference;
						}
					}
				}
				MapSpatialReference = SpatialReference;
				HeatMapPoints = points;
				OnLayerChanged();
			}
			base.Initialize();
		}

		private void task_Fault(object sender, TaskFailedEventArgs args)
		{
			base.OnProgress(100);
			InitializationFailure = args.Error;
			base.Initialize();
		}

		#region ICustomLayer Members

		public void Serialize(System.Xml.XmlWriter writer, Dictionary<string, string> Namespaces)
		{
			HeatMapFeatureLayerXamlWriter layerXamlWriter = new HeatMapFeatureLayerXamlWriter(writer, Namespaces);
			layerXamlWriter.WriteLayer(this, typeof(HeatMapFeatureLayer).Name, Constants.esriMappingNamespace);
		}
		#endregion

	}
}
