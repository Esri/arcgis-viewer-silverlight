/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
	[DisplayName("FilterLayerDisplayName")]
	[Category("CategoryLayer")]
	[Description("FilterLayerDescription")]
    public class FilterLayerCommand : LayerCommandBase
    {
        ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.DefineQueryControl queryControl;
        public override void Execute(object parameter)
        {
            FeatureLayer featureLayer = Layer as FeatureLayer;
            if (featureLayer == null)
                return;

            if (queryControl == null)
            {
                queryControl = new ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.DefineQueryControl() { Margin = new Thickness(10) };
            }

            featureLayer.UpdateCompleted -= featureLayer_UpdateCompleted;
            featureLayer.UpdateCompleted += featureLayer_UpdateCompleted;

            featureLayer.UpdateFailed -= featureLayer_UpdateFailed;
            featureLayer.UpdateFailed += featureLayer_UpdateFailed;

            queryControl.FeatureLayer = featureLayer;

            View.Instance.WindowManager.ShowWindow(Resources.Strings.FilterLayerTitle, queryControl, false, null, (s, e) =>
                {
                    if (queryControl != null)
                    {
                        queryControl.UpdateQuery();
                    }
                });
		}

        void featureLayer_UpdateFailed(object sender, Client.Tasks.TaskFailedEventArgs e)
        {
            RefreshFeatureDataGrid();
        }

        void featureLayer_UpdateCompleted(object sender, EventArgs e)
        {
            RefreshFeatureDataGrid();
        }

        private void RefreshFeatureDataGrid()
        {
            View view = View.Instance;
            if (view != null && view.AttributeDisplay != null)
            {
                if (view.AttributeDisplay.FilterFeaturesByMapExtent) // we need to update the filter source in this scenario
                    view.AttributeDisplay.Refresh();
            }
        }

        #region ICommand Members

        public override bool CanExecute(object parameter)
        {
            return Layer is FeatureLayer && !string.IsNullOrEmpty(((FeatureLayer)Layer).Url);
        }
        #endregion

    }
}
