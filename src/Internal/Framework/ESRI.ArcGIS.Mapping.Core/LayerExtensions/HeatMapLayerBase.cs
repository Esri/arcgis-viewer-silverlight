/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Core
{
	public class HeatMapLayerBase : HeatMapLayer
	{
		[TypeConverter(typeof(SpatialReferenceTypeConverter))]
		public SpatialReference MapSpatialReference { get; set; }

		public virtual void Update()
		{
			base.IsInitialized = false;
			base.Initialized += HeatMapFeatureLayer_Initialized;
			Initialize();
		}

		void HeatMapFeatureLayer_Initialized(object sender, EventArgs e)
		{
			Refresh();
			base.Initialized -= HeatMapFeatureLayer_Initialized;
		}
	}
}
