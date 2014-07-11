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
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit.DataSources;

namespace ESRI.ArcGIS.Mapping.Controls
{
	public class HeatMapConfigControl : LayerConfigControl
	{
        private const string PART_IntensitySlider = "IntensitySlider";
        internal Slider IntensitySlider;

		public HeatMapConfigControl()
		{
			DefaultStyleKey = typeof(HeatMapConfigControl);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
            IntensitySlider = GetTemplateChild("IntensitySlider") as Slider;
			bindUI();
		}

        protected override void OnLayerChanged(System.Windows.DependencyPropertyChangedEventArgs e)
		{
			base.OnLayerChanged(e);
			bindUI();
		}


		private void bindUI()
		{
            IsEnabled = HeatMapFeatureLayerHelper.SupportsLayer(Layer) || Layer is HeatMapFeatureLayer ||
                Layer is ESRI.ArcGIS.Client.Toolkit.DataSources.HeatMapLayer;
		}

        #region SymbolConfigProvider
        /// <summary>
        /// 
        /// </summary>
        public SymbolConfigProvider SymbolConfigProvider
        {
            get { return GetValue(SymbolConfigProviderProperty) as SymbolConfigProvider; }
            set { SetValue(SymbolConfigProviderProperty, value); }
        }

        public static readonly System.Windows.DependencyProperty SymbolConfigProviderProperty =
            System.Windows.DependencyProperty.Register(
                "SymbolConfigProvider",
                typeof(SymbolConfigProvider),
                typeof(HeatMapConfigControl),
                null);
        #endregion 
	}
}
