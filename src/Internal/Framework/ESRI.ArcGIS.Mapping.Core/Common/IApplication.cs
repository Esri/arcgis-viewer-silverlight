/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/


using System;
namespace ESRI.ArcGIS.Mapping.Core
{
    public interface IApplication
    {
        void PublishNotificationEvent(NotificationEvent notificationEvent, object parameter);
#if !SILVERLIGHT
        IntPtr ViewOwnerHandle { get; }
#endif
    }

    public enum NotificationEvent
    {
        LayersInitialized
    }
}
