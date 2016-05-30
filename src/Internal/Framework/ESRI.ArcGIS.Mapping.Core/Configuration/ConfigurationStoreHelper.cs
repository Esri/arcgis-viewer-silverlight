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
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class ConfigurationStoreHelper
    {
        public string GetGeometryServiceUrl(ConfigurationStore configStore)
        {
            if (configStore == null || configStore.GeometryServices == null
                || configStore.GeometryServices.Count < 1)
                return null;
            return configStore.GeometryServices[0].Url;
        }
    }
}
