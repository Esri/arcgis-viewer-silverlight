/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;
using System.Windows;

namespace ESRI.ArcGIS.Mapping.Behaviors
{
    public class LayerInitailizeFailedTriggerAction : TriggerAction<Layer>
    {
        protected override void Invoke(object parameter)
        {
            if (ApplicationHelper.CurrentApplication != null)
                ApplicationHelper.CurrentApplication.PublishNotificationEvent(NotificationEvent.LayersInitialized, null);
        }
    }
}
