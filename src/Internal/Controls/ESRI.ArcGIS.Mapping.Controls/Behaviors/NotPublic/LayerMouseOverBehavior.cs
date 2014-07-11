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
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class LayerMouseOverBehavior : Behavior<Rectangle>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject != null)
            {
                AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;                
                AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
            }
        }

        protected override void OnDetaching()
        {
            if (this.AssociatedObject != null)
            {
                AssociatedObject.MouseEnter -= AssociatedObject_MouseEnter;
                AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
            }

            base.OnDetaching();
        }

        private SolidColorBrush _applicationMouseOverColorBrush;
        private SolidColorBrush _applicationSelectionOutlineColorBrush;
        void AssociatedObject_MouseEnter(object sender, MouseEventArgs e)
        {
            if(_applicationMouseOverColorBrush == null)
                _applicationMouseOverColorBrush = Application.Current.Resources["MouseOverColorBrush"] as SolidColorBrush;
            AssociatedObject.Fill = _applicationMouseOverColorBrush;
            if(_applicationSelectionOutlineColorBrush == null)
                _applicationSelectionOutlineColorBrush = Application.Current.Resources["SelectionOutlineColorBrush"] as SolidColorBrush;
            AssociatedObject.Stroke = _applicationSelectionOutlineColorBrush;
        }

        private static SolidColorBrush TransparentBrush = new SolidColorBrush(Colors.Transparent);
        void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            AssociatedObject.Fill = TransparentBrush;
            AssociatedObject.Stroke = TransparentBrush;
        }
    }
}
