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
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class LayerErrorHandler
    {
        public static string HandleIfLayerError(Exception e)
        {
            if (MapApplication.Current == null || MapApplication.Current.Map == null)
                return null;
            Map map = MapApplication.Current.Map;
            string error = null ;
            foreach (Layer layer in map.Layers)
            {
                if (layer.InitializationFailure != null)
                {
                    layer.SetValue(ESRI.ArcGIS.Client.Extensibility.LayerExtensions.ErrorMessageProperty, layer.InitializationFailure.Message);
                    if (layer.InitializationFailure == e)
                        error = string.Format("Error initializing layer: {0}", LayerExtensions.GetLayerName(layer));
                }
            }
            return error;
        }
    }
}
