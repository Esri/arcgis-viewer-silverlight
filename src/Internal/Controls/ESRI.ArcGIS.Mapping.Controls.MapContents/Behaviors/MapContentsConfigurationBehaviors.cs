/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    public class IsExcludedLayerStateChanged : Behavior<CheckBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.Checked += AssociatedObject_Checked;
                this.AssociatedObject.Unchecked += AssociatedObject_Checked;
            }
        }

        void AssociatedObject_Checked(object sender, RoutedEventArgs e)
        {
            LayerInfo layerInfo = AssociatedObject.DataContext as LayerInfo;
            if (layerInfo != null && layerInfo.Layer != null && !string.IsNullOrWhiteSpace(layerInfo.Layer.ID))
            {
                MapContentsConfigControl parentConfigControl = ControlTreeHelper.FindAncestorOfType<MapContentsConfigControl>(AssociatedObject);
                if (parentConfigControl != null)
                {
                    MapContentsConfiguration conf = parentConfigControl.DataContext as MapContentsConfiguration;
                    if (conf != null)
                    {
                        List<string> excludedLayers = conf.ExcludedLayerIds == null ? null : conf.ExcludedLayerIds.ToList<string>();
                        if (AssociatedObject.IsChecked == true)
                        {
                            if (excludedLayers == null)
                            {
                                conf.ExcludedLayerIds = new string[] { layerInfo.Layer.ID };
                            }
                            else if (!excludedLayers.Contains(layerInfo.Layer.ID))
                            {
                                excludedLayers.Add(layerInfo.Layer.ID);
                                conf.ExcludedLayerIds = excludedLayers.ToArray();
                            }
                        }
                        else
                        {
                            if (excludedLayers != null)
                            {
                                excludedLayers.Remove(layerInfo.Layer.ID);
                                conf.ExcludedLayerIds = excludedLayers.ToArray();
                            }
                        }
                    }
                }
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.Checked -= AssociatedObject_Checked;
                this.AssociatedObject.Unchecked -= AssociatedObject_Checked;
            }
        }
    }
}
