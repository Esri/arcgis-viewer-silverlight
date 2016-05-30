/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows;
using System.Windows.Input;
using System;
using System.Windows.Threading;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class OverrideMouseRightButtonDownBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.MouseRightButtonDown -= AssociatedObject_MouseRightButtonDown;
                this.AssociatedObject.MouseRightButtonDown += AssociatedObject_MouseRightButtonDown;
            }
        }

        void AssociatedObject_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.MouseRightButtonDown -= AssociatedObject_MouseRightButtonDown;
            }
        }
    }
}
