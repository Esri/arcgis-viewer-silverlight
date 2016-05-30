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
using System.Windows.Data;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Client.Application.Controls
{
    internal class DependencyPropertyHelper
    {
        internal static void RegisterForChangeNotification(ChangeNotification notification)
        {
            //Bind to a depedency property
            Binding b = new Binding(notification.PropertyName) { Source = notification.Source };
            DependencyProperty prop = DependencyProperty.RegisterAttached(
                string.Format("ListenAttached_{0}_{1}", notification.PropertyName, Guid.NewGuid().ToString()),
                typeof(object),
                typeof(DependencyObject),
                new PropertyMetadata(notification.Callback));

            notification.Target = notification.Target ?? notification.Source;
            BindingOperations.SetBinding(notification.Target, prop, b);

            if (GetChangeNotifications(notification.Target) == null)
                SetChangeNotifications(notification.Target, new List<ChangeNotification>());

            List<ChangeNotification> changeNotifications = GetChangeNotifications(notification.Target);
            changeNotifications.Add(notification);
        }

        public static readonly DependencyProperty ChangeNotificationsProperty = DependencyProperty.RegisterAttached(
            "ChangeNotifications", typeof(List<ChangeNotification>), typeof(DependencyObject), null);
        public static void SetChangeNotifications(DependencyObject d, List<ChangeNotification> value)
        {
            if (d == null)
                throw new ArgumentNullException("d");
            d.SetValue(DependencyPropertyHelper.ChangeNotificationsProperty, value);
        }

        public static List<ChangeNotification> GetChangeNotifications(DependencyObject d)
        {
            if (d == null)
                throw new ArgumentNullException("d");
            return d.GetValue(DependencyPropertyHelper.ChangeNotificationsProperty) as List<ChangeNotification>;
        }

    }

    internal class ChangeNotification
    {
        internal string PropertyName { get; set; }
        internal DependencyObject Source { get; set; }
        internal PropertyChangedCallback Callback { get; set; }
        internal DependencyObject Target { get; set; }
    }
}
