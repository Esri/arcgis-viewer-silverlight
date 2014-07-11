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

namespace ESRI.ArcGIS.Mapping.Controls
{
    internal static class DialogResizeHelper
    {
        public static void CenterAndSizeDialog(FrameworkElement control, ESRI.ArcGIS.Mapping.Controls.ChildWindow containerWindow)
        {            
            if (control == null || containerWindow == null)
                return;
            
#if SILVERLIGHT
            containerWindow.Width = containerWindow.Height = double.NaN; // allow the container to size to contents
            
            Size rootVisualSize = Application.Current.RootVisual.RenderSize;
            if (!double.IsNaN(control.Width) && control.Width >= (rootVisualSize.Width - 5))
                control.Width = rootVisualSize.Width - 5;
            if (double.IsNaN(control.Height) && control.Height >= (rootVisualSize.Height - 5))
                control.Height = rootVisualSize.Height - 5;

            double controlWidth = double.IsNaN(control.Width) ? 200 : control.Width;
            containerWindow.HorizontalOffset = (rootVisualSize.Width - controlWidth) / 2;

            double controlHeight = double.IsNaN(control.Height) ? 300 : control.Height;
            containerWindow.VerticalOffset = (rootVisualSize.Height - controlHeight) / 2;
#else
            containerWindow.Width = control.Width + 15;
            containerWindow.Height = control.Height + 24;           
#endif      
        }        
    }
}
