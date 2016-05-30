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
using ESRI.ArcGIS.Client;
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class ShowMoreLessToggleControlBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject != null)
            {
                System.Windows.Controls.Primitives.ButtonBase button = this.AssociatedObject as System.Windows.Controls.Primitives.ButtonBase;
                if (button != null)
                {
                    button.Click -= AssociatedObject_Click;
                    button.Click += AssociatedObject_Click;
                }
                else
                {
                    this.AssociatedObject.MouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
                    this.AssociatedObject.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
                }
            }
        }

        public string ControlName
        {
            get;
            set;
        }

        private object FindName(FrameworkElement startElement, string name, int ancestorDepth)
        {
            FrameworkElement searchElement = startElement;
            for (int i = 0; i < ancestorDepth; i++)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(searchElement) as FrameworkElement;
                if (parent == null || !(parent is FrameworkElement))
                    break;

                searchElement = parent as FrameworkElement;
            }

            return searchElement.FindName(name);
        }

        private void ToggleState()
        {
            if (!string.IsNullOrEmpty(ControlName))
            {
                FrameworkElement element = FindName(AssociatedObject, ControlName, 2) as FrameworkElement;
                if (element != null)
                {
                    element.Visibility = element.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }

        void AssociatedObject_Click(object sender, RoutedEventArgs e)
        {
            ToggleState();
        }

        void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToggleState();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
            {
                System.Windows.Controls.Primitives.ButtonBase button = this.AssociatedObject as System.Windows.Controls.Primitives.ButtonBase;
                if (button != null)
                {
                    button.Click -= AssociatedObject_Click;
                }
                else
                {
                    this.AssociatedObject.MouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
                }
            }
        }
    }
}
