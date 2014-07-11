/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace ESRI.ArcGIS.Mapping.Controls.Behaviors
{
    public class ToggleVisibilityOnClick : Behavior<FrameworkElement>
    {
        public ToggleVisibilityOnClick()
        {
        }

        /// <summary>
        /// The name of the toggle button to attach the element's visibility to.
        /// </summary>
        public string ToggleButtonName { get; set; }

        private ToggleButton SourceButton { get; set; }

        /// <summary>
        /// The name of the element to bind visibility to.
        /// </summary>
        public string VisibilitySourceName { get; set; }

        private FrameworkElement VisibilitySource { get; set; }

        /// <summary>
        /// How target element visiblity behaves relative to source element visibility.
        /// If Synchronized, the target element will always be visible when the source
        /// element is visible, regardless of toggle button checked state.  If Inverse,
        /// the target element will always be visible when the source element is collapsed,
        /// regardless of toggle button checked state.  This setting only enforces making
        /// the target element visible.  It does not impose collapsed visibility on the 
        /// target.
        /// </summary>
        public VisibilityBindings VisibilityBindingMode { get; set; }

        public enum VisibilityBindings { None, Synchronized, Inverse }

        protected override void OnAttached()
        {
            base.OnAttached();

            Dispatcher.BeginInvoke(delegate()
            {
                SourceButton = findName(AssociatedObject, ToggleButtonName, 2) as ToggleButton;
                if (SourceButton == null)
                    throw new Exception("Could not find a toggle button with the name \"" + ToggleButtonName + ".\"");

                SourceButton.Checked -= SourceButtonToggled;
                SourceButton.Unchecked -= SourceButtonToggled;
                SourceButton.Checked += SourceButtonToggled;
                SourceButton.Unchecked += SourceButtonToggled;

                if (VisibilityBindingMode != VisibilityBindings.None)
                {
                    VisibilitySource = findName(AssociatedObject, VisibilitySourceName, 2) as FrameworkElement;
                    if (VisibilitySource == null)
                        throw new Exception("Could not find an element with the name \"" + ToggleButtonName + ".\"");

                    Binding b = new Binding("Visibility") { Source = VisibilitySource };
                    if (VisibilityBindingMode == VisibilityBindings.Synchronized)
                    {
                        AssociatedObject.SetBinding(FrameworkElement.VisibilityProperty, b);
                    }
                    else if (VisibilityBindingMode == VisibilityBindings.Inverse)
                    {
                        b.Converter = new InvertVisibilityConverter();
                        AssociatedObject.SetBinding(FrameworkElement.VisibilityProperty, b);
                    }
                }

                updateVisibility();
            });
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            SourceButton.Checked -= SourceButtonToggled;
            SourceButton.Unchecked -= SourceButtonToggled;

            // Remove TargetElementVisibilityProperty's binding to source toggle button's visibility.
            AssociatedObject.SetBinding(FrameworkElement.VisibilityProperty, new Binding());
        }

        private object findName(FrameworkElement startElement, string name, int ancestorDepth)
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

        private void SourceButtonToggled(object sender, RoutedEventArgs e)
        {
            updateVisibility();
        }

        private void updateVisibility()
        {
            if (VisibilityBindingMode == VisibilityBindings.None || 
                (VisibilitySource != null && VisibilityBindingMode == VisibilityBindings.Synchronized &&
                VisibilitySource.Visibility == Visibility.Collapsed) ||
                (VisibilitySource != null && VisibilityBindingMode == VisibilityBindings.Inverse &&
                VisibilitySource.Visibility == Visibility.Visible))
            {
                if (SourceButton.IsChecked == true)
                    AssociatedObject.Visibility = Visibility.Visible;
                else
                    AssociatedObject.Visibility = Visibility.Collapsed;
            }
        }
    }
}
