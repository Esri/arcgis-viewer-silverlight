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
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.WebMap;

namespace ESRI.ArcGIS.Mapping.Core
{
    public static class MapExtensions
    {
        public static WebMapSettings GetWebMapSettings(this Map map)
        {
            return map.GetValue(MapProperties.WebMapSettingsProperty) as WebMapSettings;
        }

        public static void SetWebMapSettings(this Map map, WebMapSettings settings)
        {
            map.SetValue(MapProperties.WebMapSettingsProperty, settings);
        }
    }
}
