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
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Client.Extensibility;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class SecureServicesHelper : ProxyUrlHelper
    {
        public static void SetProxyUrl(string proxyUrl)
        {
            if (View.Instance != null)
                View.Instance.ProxyUrl = proxyUrl;
        }

        public static void UpdateProxyUrl(string proxyUrl)
        {
            Map map = (MapApplication.Current != null) ? MapApplication.Current.Map : null;
            if (map == null)
                return;

            bool mapRequiresRefresh = false;
            #region Check if layers can be updated in-place
            List<Layer> layersForRefresh = new List<Layer>();
            foreach (Layer layer in map.Layers)
            {
                if (ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetUsesProxy(layer))
                {
                    if (ProxyUrlHelper.CanChangeProxyUrl(layer))
                        layersForRefresh.Add(layer);
                    else
                    {
                        mapRequiresRefresh = true;
                        break;
                    }
                }
            }
            #endregion
            #region Set proxy url if layers can be updated
            if (!mapRequiresRefresh)
            {
                foreach (Layer layer in layersForRefresh)
                    SecureServicesHelper.SetProxyUrl(layer, proxyUrl);
            }
            #endregion
            #region Else, serialize/deserialize map, remove and re-add layers
            else
            {
                MapXamlWriter writer = new MapXamlWriter(true);
                try
                {
                    string mapXaml = writer.MapToXaml(map);

                    if (!string.IsNullOrEmpty(mapXaml))
                    {
                        Map newMap = System.Windows.Markup.XamlReader.Load(mapXaml) as Map;
                        if (newMap != null && newMap.Layers.Count == map.Layers.Count)
                        {
                            map.Layers.Clear();
                            map.Extent = newMap.Extent;
                            List<Layer> layers = new List<Layer>();
                            foreach (Layer layer in newMap.Layers)
                            {
                                if (ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetUsesProxy(layer))
                                    SecureServicesHelper.SetProxyUrl(layer, proxyUrl);
                                layers.Add(layer);
                            }
                            newMap.Layers.Clear();
                            foreach (Layer layer in layers)
                                map.Layers.Add(layer);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Logger.Instance.LogError(ex);
                    MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.ErrorChangingProxies);
                }
            }
            #endregion
        }
    }
}
