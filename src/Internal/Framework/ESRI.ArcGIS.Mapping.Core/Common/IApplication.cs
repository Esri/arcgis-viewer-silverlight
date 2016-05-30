/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
