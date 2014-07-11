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
    public static class WindowSizeUtility
    {
        private const double PercentageOfWindowSize = .7;

        private static UIElement GetRootVisual()
        {
            if (Application.Current != null && Application.Current.RootVisual != null)
                return Application.Current.RootVisual;

            return null;
        }
        private static Size GetRootVisualSize()
        {
            UIElement element = GetRootVisual();
            if (element != null)
            {
                return element.RenderSize;
            }

            return Size.Empty;
        }

        public static Size GetWindowMaxSize()
        {
            Size size = GetRootVisualSize();
            if (size != Size.Empty && size.Width != double.NaN && size.Height != double.NaN)
            {
                double width, height;

                width = size.Width * PercentageOfWindowSize;
                height = size.Height * PercentageOfWindowSize;

                return new Size(width, height);
            }

            return Size.Empty;
        }
    }
}
