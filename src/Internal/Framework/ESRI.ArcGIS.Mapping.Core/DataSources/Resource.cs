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
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Core.DataSources
{
    public class Resource
    {
        public Resource() { }

        public string Url { get; set; }
        public string ProxyUrl { get; set; }
        public string DisplayName { get; set; }
        public ResourceType ResourceType { get; set; }
        public object Tag { get; set; }
    }

    public enum ResourceType
    {
        Undefined,
        Server,
        MapServer,
        Folder,
        GroupLayer,
        Layer,
        Database,
        DatabaseTable,
        SharePointList,
        SharePointView,
        Field,
        ExcelFile,
        ExcelTable,
        ImageServer,
        FeatureServer,
        EditableLayer,
        GPServer,
        GPTool,
        ODataServer,
        ODataCollection,
        ODataFeed
    }

    [Flags]
    public enum Filter
    {
        None = 1,
        CachedResources = 2, 
        SpatiallyEnabledResources = 4, 
        ImageServices = 8,
        FeatureServices = 16,
        GeoprocessingServices = 32
    }
}
