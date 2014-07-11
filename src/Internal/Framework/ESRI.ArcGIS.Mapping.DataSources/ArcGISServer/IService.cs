/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.DataSources;

namespace ESRI.ArcGIS.Mapping.DataSources.ArcGISServer
{
    public interface IService
    {
        string Uri { get; set; }
        ResourceType Type { get; set; }
        List<Resource> ChildResources { get; set; }

        void GetServiceDetails(object userState);
        void Cancel();

        bool IsServiceInfoNeededToApplyThisFilter(Filter filter);
        bool IsFilteredIn(Filter filter);

        event EventHandler<ServiceDetailsDownloadCompletedEventArgs> ServiceDetailsDownloadCompleted;
        event EventHandler<ExceptionEventArgs> ServiceDetailsDownloadFailed;
    }

    public class ServiceDetailsDownloadCompletedEventArgs : EventArgs
    {
        public object UserState { get; set; }
    }
}
