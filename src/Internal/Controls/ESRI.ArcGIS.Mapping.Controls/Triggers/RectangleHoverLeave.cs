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
using System.Windows.Interactivity;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class RectangleHoverLeave : TargetedTriggerAction<Rectangle>
    {
        protected override void Invoke(object parameter)
        {
            Rectangle element = Target as Rectangle;
            if (element != null)
            {
                HoverEnd(element);
            }
        }

        private static SolidColorBrush TransparentBrush = new SolidColorBrush(Colors.Transparent);
        private void HoverEnd(Rectangle element)
        {
            if (element != null)
            {
                element.Fill = TransparentBrush;
                element.Stroke = TransparentBrush;
            }
        }
    }
}
