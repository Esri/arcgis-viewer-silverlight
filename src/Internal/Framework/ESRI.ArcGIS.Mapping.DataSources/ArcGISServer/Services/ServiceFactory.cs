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
using ESRI.ArcGIS.Mapping.Core.DataSources;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    public static class ServiceFactory
    {
        public static IService CreateService(ResourceType type, string url, string proxyUrl)
        {
            if (type == ResourceType.MapServer)
                return new ESRI.ArcGIS.Mapping.DataSources.ArcGISServer.MapService(url, proxyUrl);
            if (type == ResourceType.ImageServer)
                return new ESRI.ArcGIS.Mapping.DataSources.ArcGISServer.ImageService(url, proxyUrl);
            if (type == ResourceType.FeatureServer)
                return new ESRI.ArcGIS.Mapping.DataSources.ArcGISServer.FeatureService(url, proxyUrl);
            if (type == ResourceType.GPServer)
                return new ESRI.ArcGIS.Mapping.DataSources.ArcGISServer.GeoprocessingService(url, proxyUrl);
            return null;
        }
    }
}
