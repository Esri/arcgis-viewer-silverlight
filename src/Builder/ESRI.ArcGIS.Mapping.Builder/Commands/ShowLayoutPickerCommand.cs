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
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class ShowLayoutPickerCommand : CommandBase
    {
        LayoutPickerControl layoutPickerControl;
        bool _completed = false;

        public override void Execute(object parameter)
        {
            if (layoutPickerControl == null)
            {
                layoutPickerControl = new LayoutPickerControl();
                layoutPickerControl.Completed += LayoutPickerControl_Completed;
                layoutPickerControl.Cancelled += LayoutPickerControl_Cancelled;
            }

            BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ChangeLayout, 
                layoutPickerControl, false, null, LayoutPicker_WindowClosed);
        }

        private void LayoutPickerControl_Cancelled(object sender, EventArgs e)
        {
            BuilderApplication.Instance.HideWindow(layoutPickerControl);
        }

        private void LayoutPickerControl_Completed(object sender, EventArgs e)
        {
            _completed = true;
            BuilderApplication.Instance.HideWindow(layoutPickerControl);
        }

        private void LayoutPicker_WindowClosed(object sender, EventArgs e)
        {
            if (!_completed)
                layoutPickerControl.Cancel();
            else
                _completed = false;
        }
    }
}
