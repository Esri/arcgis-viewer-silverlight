/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client;
using System.Windows;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class FeatureLayerModeConfigControl : LayerConfigControl
    {
        public FeatureLayerModeConfigControl()
        {
            DefaultStyleKey = typeof (FeatureLayerModeConfigControl);
        }

        private DependencyProperty _modePropertyChangedListener;
        protected override void OnLayerChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            base.OnLayerChanged(e);

            FeatureLayer layer = e.OldValue as FeatureLayer;
            
            if (layer != null && _modePropertyChangedListener != null)
                ESRI.ArcGIS.Mapping.Core.Utility.ClearAttachedPropertyListener(_modePropertyChangedListener, layer);
            
            _modePropertyChangedListener = null;
            
            layer = e.NewValue as FeatureLayer;
            if (layer != null && !string.IsNullOrEmpty(layer.Url))
            {
                IsEnabled = true;
                _modePropertyChangedListener = ESRI.ArcGIS.Mapping.Core.Utility.NotifyOnAttachedPropertyChanged("Mode", layer, OnModeChanged, layer.Mode);
            }
            else
                IsEnabled = false;
        }

        //Workaround for issue in core api. Once geometry is being removed from the query string once the mode changes, we can remove this code
        void OnModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FeatureLayer layer = Layer as FeatureLayer;
            if (layer != null)
            {
                FeatureLayer.QueryMode oldMode = (FeatureLayer.QueryMode)e.OldValue;
                FeatureLayer.QueryMode newMode = (FeatureLayer.QueryMode)e.NewValue;

                if (oldMode != newMode)
                {
                    if (oldMode == FeatureLayer.QueryMode.OnDemand && newMode == FeatureLayer.QueryMode.Snapshot)
                        layer.Geometry = null;

                    if (newMode == FeatureLayer.QueryMode.Snapshot ||
                        newMode == FeatureLayer.QueryMode.OnDemand)
                    {
                        // We only need to make sure the client caching is off while we
                        // do this update, and then return it to the original state.
                        bool cachingDisabled = layer.DisableClientCaching;
                        if (!cachingDisabled)
                            layer.DisableClientCaching = true;

                        layer.Update();

                        if (!cachingDisabled)
                            layer.DisableClientCaching = false;
                    }
                }
            }
        }
    }
}
