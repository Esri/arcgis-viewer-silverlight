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
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Core
{
    public interface ICustomGraphicsLayer
    {
        Collection<FieldInfo> Fields { get; }
        SpatialReference MapSpatialReference { get; set; }
        /// <summary>
        /// Clears the graphics, and fetches a new update from the service.
        /// </summary>
        void Update();
        void ForceRefresh(EventHandler refreshCompletedHander, EventHandler<TaskFailedEventArgs> refreshFailedHandler);
        void UpdateOnMapExtentChanged(ExtentEventArgs e);
        Uri GetServiceDetailsUrl();
        bool SupportsNavigateToServiceDetailsUrl();
        event EventHandler UpdateOnExtentValueChanged;
        event EventHandler UpdateCompleted;
        event EventHandler UpdateFailed;
        bool SupportsItemOnClickBehavior { get; }
        bool IsItemOnClickBehaviorEnabled { get; set; }
        void OnItemClicked(Graphic item);
    }
}
