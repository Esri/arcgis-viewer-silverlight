/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.ComponentModel.Composition;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    //[Export(typeof(ICommand))]
	//[DisplayName("AddHeatMapDisplayName")]
	//[Category("CategoryMap")]
	//[Description("AddHeatMapDescription")]
    public class AddHeatMapCommand : LayerCommandBase
	{
        public override void Execute(object parameter)
		{
			HeatMapFeatureLayerHelper.AddHeatMapLayer(Layer, View.Instance);
		}

        public override bool CanExecute(object parameter)
        {
            return HeatMapFeatureLayerHelper.SupportsLayer(Layer);
        }		
	}
}
