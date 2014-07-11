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
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ConnectionDeleteBehavior : Behavior<Image>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
                this.AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
            }
        }

        void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem connectionItem = ControlTreeHelper.FindAncestorOfType<ListBoxItem>(AssociatedObject);
            if (connectionItem == null)
                return;

            Connection connectionToDelete = connectionItem.DataContext as Connection;
            if (connectionToDelete == null)
                return;

            ConnectionsDropDownPopupControl connectivityControl = ControlTreeHelper.FindAncestorOfType<ConnectionsDropDownPopupControl>(AssociatedObject);
            if (connectivityControl == null)
                return;

            connectivityControl.DeleteConnection(connectionToDelete);

            e.Handled = true;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.MouseLeftButtonUp -= AssociatedObject_MouseLeftButtonDown;
            }
        }
    }
}
