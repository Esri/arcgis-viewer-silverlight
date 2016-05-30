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
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
	[DisplayName("RefreshLayerDisplayName")]
	[Category("CategoryLayer")]
	[Description("RefreshLayerDescription")]
    public class RefreshLayerCommand : LayerCommandBase 
    {
        #region ICommand Members
        private bool _isRefreshing = false;
        public override bool CanExecute(object parameter)
        {
            if (_isRefreshing)
                return false;
            return Layer != null && Layer.Visible == true;
        }

        public override void Execute(object parameter)
        {
            Layer layer = Layer;
            if (layer == null)
                return;
            RefreshLayer(layer, (o, e) =>
            {
                _isRefreshing = false;
                OnCanExecuteChanged(EventArgs.Empty);
                OnCompleted(EventArgs.Empty);
            }, (o, e) =>
            {
                _isRefreshing = false;
                OnCanExecuteChanged(EventArgs.Empty);
            });
        }
        #endregion

        private void RefreshLayer(Layer layer, EventHandler refreshCompletedHander, EventHandler<TaskFailedEventArgs> refreshFailedHandler)
        {
            _isRefreshing = true;
            FeatureLayer featureLayer = layer as FeatureLayer;
            if (featureLayer != null && !string.IsNullOrEmpty(featureLayer.Url))
            {
                // temporarly unhook the AttributeDisplay's layer while we refresh feature layer
                if (View.Instance != null && View.Instance.AttributeDisplay != null && View.Instance.AttributeDisplay.FeatureDataGrid != null)
                {
                    ToggleTableCommand.SetTableVisibility(Visibility.Collapsed);

                    // Set FeatureDataGrid layer to null so that we don't incur the overhead
                    // of all the UpdateItemSource calls as the AttributeTable Graphics layer is
                    // being set to null.
                    View.Instance.AttributeDisplay.FeatureDataGrid.GraphicsLayer = null;
                    // Set the FilterSource to null to prevent potential GraphicsLayer reference exceptions
                    View.Instance.AttributeDisplay.FeatureDataGrid.FilterSource = null;
                    // Now set the AttributeDisplay GraphicsLayer to null so that it
                    // unhooks all the bindings and events
                    View.Instance.AttributeDisplay.GraphicsLayer = null;

                    // Hook up to the UpdateCompleted/UpdateFailed events so that the layer
                    // can be reset to the SelectedLayer
                    featureLayer.UpdateCompleted -= OnFeatureLayerUpdateCompleted;
                    featureLayer.UpdateCompleted += OnFeatureLayerUpdateCompleted;
                    featureLayer.UpdateFailed -= OnFeatureLayerUpdateFailed;
                    featureLayer.UpdateFailed += OnFeatureLayerUpdateFailed;
                }

                if (refreshCompletedHander != null)
                {
                    featureLayer.UpdateCompleted -= refreshCompletedHander;
                    featureLayer.UpdateCompleted += refreshCompletedHander;
                }
                if (refreshFailedHandler != null)
                {
                    featureLayer.UpdateFailed -= refreshFailedHandler;
                    featureLayer.UpdateFailed += refreshFailedHandler;
                }
                featureLayer.Update();
                return;
            }

            ArcGISDynamicMapServiceLayer dynamicLayer = layer as ArcGISDynamicMapServiceLayer;
            if (dynamicLayer != null)
            {
                dynamicLayer.Refresh();
                if (refreshCompletedHander != null)
                    refreshCompletedHander.Invoke(layer, EventArgs.Empty);
                return;
            }

            ArcGISTiledMapServiceLayer tiledLayer = layer as ArcGISTiledMapServiceLayer;
            if (tiledLayer != null)
            {
                // Tiled layers do not support refreshing
                if (refreshCompletedHander != null)
                    refreshCompletedHander.Invoke(layer, EventArgs.Empty);
                return;
            }

            ArcGISImageServiceLayer imageServiceLayer = layer as ArcGISImageServiceLayer;
            if (imageServiceLayer != null)
            {
                imageServiceLayer.Refresh();
                if (refreshCompletedHander != null)
                    refreshCompletedHander.Invoke(layer, EventArgs.Empty);
                return;
            }

            ICustomGraphicsLayer customGraphicsLayer = layer as ICustomGraphicsLayer;
            if (customGraphicsLayer != null)
            {
                customGraphicsLayer.ForceRefresh(refreshCompletedHander, refreshFailedHandler);
                return;
            }


            HeatMapLayerBase heatMapLayer = layer as HeatMapLayerBase;
            if (heatMapLayer != null)
            {
                heatMapLayer.Update();
                if (refreshCompletedHander != null)
                    refreshCompletedHander.Invoke(layer, EventArgs.Empty);
                return;
            }

            GeoRssLayer geoRssLayer = Layer as GeoRssLayer;
            if (geoRssLayer != null)
            {
                geoRssLayer.Update();
                return;
            }

            WmsLayer wmsLayer = Layer as WmsLayer;
            if (wmsLayer != null)
            {
                wmsLayer.Refresh();
                return;
            }

            GraphicsLayer graphicsLayer = layer as GraphicsLayer;
            if (graphicsLayer != null)
            {
                graphicsLayer.Refresh();
                if (refreshCompletedHander != null)
                    refreshCompletedHander.Invoke(layer, EventArgs.Empty);
                return;
            }
        }

        void OnFeatureLayerUpdateFailed(object sender, TaskFailedEventArgs e)
        {
            ResetGraphicsLayer();
        }

        void OnFeatureLayerUpdateCompleted(object sender, EventArgs e)
        {
            ResetGraphicsLayer();
        }

        private void ResetGraphicsLayer()
        {
            FeatureLayer layer = Layer as FeatureLayer;
            if (layer != null)
            {
                layer.UpdateCompleted -= OnFeatureLayerUpdateCompleted;
                layer.UpdateFailed -= OnFeatureLayerUpdateFailed;
            }
            // reset the AttributeDisplay's feature layer, bindings, and events
            if (View.Instance != null && View.Instance.AttributeDisplay != null && View.Instance.AttributeDisplay.FeatureDataGrid != null)
            {
                View.Instance.AttributeDisplay.GraphicsLayer = View.Instance.SelectedLayer as GraphicsLayer;
            }
        }
        protected virtual void OnCompleted(EventArgs args)
        {
            if (Completed != null)
                Completed(this, args);
        }

        public event EventHandler Completed;
    }
}
