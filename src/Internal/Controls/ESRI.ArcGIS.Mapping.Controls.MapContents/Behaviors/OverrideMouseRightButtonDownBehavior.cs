/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ESRI.ArcGIS.Mapping.Controls.MapContents
{
    public class OverrideMouseRightButtonDownBehavior : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject != null)
                this.AssociatedObject.MouseRightButtonDown += AssociatedObject_MouseRightButtonDown;
        }

        void AssociatedObject_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
                this.AssociatedObject.MouseRightButtonDown -= AssociatedObject_MouseRightButtonDown;
        }
    }
}
