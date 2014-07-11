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

namespace ESRI.ArcGIS.Mapping.Builder
{
    public static class ControlExtensions
    {
        /// <summary>
        /// Attached property for manipulating visibilities on elements added through a ControlTemplate 
        /// </summary>
        public static readonly DependencyProperty ExtendedUIVisibilityProperty = DependencyProperty.RegisterAttached(
            "ExtendedUIVisibility", typeof(Visibility), typeof(Control), null);

        public static void SetExtendedUIVisibility(Control control, Visibility value)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            control.SetValue(ExtendedUIVisibilityProperty, value);
        }

        public static Visibility GetExtendedUIVisibility(Control control)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            return (Visibility)control.GetValue(ExtendedUIVisibilityProperty);
        }

        /// <summary>
        /// Attached property for adding commands to a control
        /// </summary>
        public static readonly DependencyProperty ExtendedCommandProperty = DependencyProperty.RegisterAttached(
            "ExtendedCommand", typeof(ICommand), typeof(Control), null);

        public static void SetExtendedCommand(Control control, ICommand value)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            control.SetValue(ExtendedCommandProperty, value);
        }

        public static ICommand GetExtendedCommand(Control control)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            return control.GetValue(ExtendedCommandProperty) as ICommand;
        }

        /// <summary>
        /// Returns the first ancestor in the visual tree of the specified type
        /// </summary>
        internal static T FindAncestorOfType<T>(this DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                obj = VisualTreeHelper.GetParent(obj);
                var objAsT = obj as T;
                if (objAsT != null)
                    return objAsT;
            }
            return null;
        }

        /// <summary>
        /// Returns the first ancestor in the visual tree of the specified type
        /// </summary>
        internal static T FindDescendantOfType<T>(this DependencyObject obj, string name = null) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject d = VisualTreeHelper.GetChild(obj, i);
                var objAsT = obj as T;
                if (obj != null && (name == null || (obj is FrameworkElement && ((FrameworkElement)obj).Name == name)))
                {
                    return objAsT;
                }
                else
                {
                    objAsT = obj.FindDescendantOfType<T>(name);
                    if (objAsT != null)
                        return objAsT;
                }
            }
            return null;
        }

        internal static FrameworkElement FindDescendantByName(this FrameworkElement obj, string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                FrameworkElement child = VisualTreeHelper.GetChild(obj, i) as FrameworkElement;
                if (child != null && child.Name == name)
                {
                    return child;
                }
                else if (child != null)
                {
                    child = child.FindDescendantByName(name);
                    if (child != null && child.Name == name)
                        return child;
                }
            }
            return null;
        }
    }
}
