/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client.Extensibility;

namespace SearchTool
{
    /// <summary>
    /// Closes the window containing the targeted element
    /// </summary>
    public class CloseWindowAction : TargetedTriggerAction<FrameworkElement>
    {
        protected override void Invoke(object parameter)
        {
            if (Target != null && MapApplication.Current != null)
                MapApplication.Current.HideWindow(Target);
        }
    }
}
