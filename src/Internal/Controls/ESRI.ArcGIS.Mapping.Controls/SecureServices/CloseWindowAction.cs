/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows.Controls;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client.Extensibility;
using System;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class CloseWindowAction : TargetedTriggerAction<Control>
    {
        protected override void Invoke(object parameter)
        {
            if (TargetObject != null)
            {
                MapApplication.Current.HideWindow(TargetObject as Control);
                if (TargetObject is IDisposable)
                    ((IDisposable)TargetObject).Dispose();
            }
        }

    }
}


