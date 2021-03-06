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

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class WindowSizeHelper
    {
        public static double GetMaxWidthForWindow()
        {
            FrameworkElement root = Application.Current.RootVisual as FrameworkElement;
            double viewWidth = root != null ? root.ActualWidth : 300;
            double maxWidth = viewWidth - 150;
            maxWidth = maxWidth < 100 ? 100 : maxWidth;
            return maxWidth;
        }
        public static double GetMaxHeightForWindow()
        {
            FrameworkElement root = Application.Current.RootVisual as FrameworkElement;
            double viewHeight = root != null ? root.ActualHeight : 300;
            double maxHeight = viewHeight - 150;
            maxHeight = maxHeight < 100 ? 100 : maxHeight;
            return maxHeight;
        }
    }
}
