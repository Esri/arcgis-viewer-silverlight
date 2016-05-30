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
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Toolkit;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls.Editor
{
    internal class AttachmentEditorCommand : AttachmentBaseCommand
    {
        protected override FeatureLayer GetFeatureLayer(object parameter)
        {
            if (MapApplication.Current == null || MapApplication.Current.Map == null)
                return null;

            string[] layerIds = parameter as string[];

            int selectedGraphicsCount = 0;
            FeatureLayer gLayer = null;
            foreach (Layer layer in MapApplication.Current.Map.Layers)
            {
                FeatureLayer featureLayer = layer as FeatureLayer;
                if (featureLayer == null || string.IsNullOrWhiteSpace(featureLayer.ID))
                    continue;

                if (layerIds != null && !layerIds.Contains(featureLayer.ID))
                    continue;

                if (featureLayer.LayerInfo.HasAttachments && featureLayer.SelectedGraphics.Count() > 0)
                {
                    selectedGraphicsCount = selectedGraphicsCount + featureLayer.SelectedGraphics.Count();

                    if (selectedGraphicsCount > 1)
                        return null;
                    else
                        gLayer = featureLayer;
                }

            }
            return gLayer;
        }
    }
}
