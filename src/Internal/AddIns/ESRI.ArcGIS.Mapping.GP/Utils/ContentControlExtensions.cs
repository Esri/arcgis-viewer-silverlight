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

namespace ESRI.ArcGIS.Mapping.GP
{
    public static class ContentControlExtensions
    {
        public static void EnforceForegroundOnChildTextBlocks(this ContentControl control)
        {
            Brush foreground = control.Foreground;

            enforceForeground(control.Content, foreground);
        }

        private static void enforceForeground(object child, Brush foreground)
        {
            if (child is Panel)
            {
                foreach (object o in ((Panel)child).Children)
                    enforceForeground(o, foreground);
            }
            else if (child is Border)
            {
                enforceForeground(((Border)child).Child, foreground);
            }
            else if (child is ContentControl)
            {
                enforceForeground(((ContentControl)child).Content, foreground);
            }
            else if (child is TextBlock)
            {
                ((TextBlock)child).Foreground = foreground;
            }
        }
    }
}
