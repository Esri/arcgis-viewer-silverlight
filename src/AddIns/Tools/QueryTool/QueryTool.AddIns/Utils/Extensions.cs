/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace QueryTool.AddIns
{
    /// <summary>
    /// Provides extension methods for use by the QueryTool add-in
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Retrieves a storyboard for the visual state of a given <see cref="FrameowrkElement"/>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal static Storyboard FindStoryboard(this FrameworkElement element, string state)
        {
            if (element == null)
                return null;

            // Search for the storyboard the visual states defined on the element itself
            var vsgs = VisualStateManager.GetVisualStateGroups(element);
            if (vsgs != null && vsgs.Count > 0)
            {
                foreach (VisualStateGroup vsg in vsgs)
                {
                    if (vsg.States != null)
                    {
                        foreach (VisualState vs in vsg.States)
                        {
                            if (vs.Name == state)
                                return vs.Storyboard;
                        }
                    }
                }
            }

            // Look for the visual state on the first child object.  This is generally where states are declared
            // when they are part of a ControlTemplate.
            FrameworkElement child = null;
            if (System.Windows.Media.VisualTreeHelper.GetChildrenCount(element) > 0)
                child = System.Windows.Media.VisualTreeHelper.GetChild(element, 0) as FrameworkElement;

            vsgs = VisualStateManager.GetVisualStateGroups(child);
            if (vsgs != null && vsgs.Count > 0)
            {
                foreach (VisualStateGroup vsg in vsgs)
                {
                    if (vsg.States != null)
                    {
                        foreach (VisualState vs in vsg.States)
                        {
                            if (vs.Name == state)
                                return vs.Storyboard;
                        }
                    }
                }
            }
            return null;
        }

        public static T FindAncestorOfType<T>(this DependencyObject obj) where T : DependencyObject
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
    }

    public static class Extensions
    {
        public static readonly DependencyProperty NoItemSelectedTextProperty = DependencyProperty.RegisterAttached(
            "NoItemSelectedText", typeof(string), typeof(Selector), null);

        public static void SetNoItemSelectedText(Selector selector, string text)
        {
            selector.SetValue(NoItemSelectedTextProperty, text);
        }

        public static string GetNoItemSelectedText(Selector selector)
        {
            return selector.GetValue(NoItemSelectedTextProperty) as string;
        }

        public static readonly DependencyProperty WatermarkTextProperty = DependencyProperty.RegisterAttached(
            "WatermarkText", typeof(string), typeof(TextBox), null);

        public static void SetWatermarkText(TextBox textBox, string text)
        {
            textBox.SetValue(WatermarkTextProperty, text);
        }

        public static string GetWatermarkText(TextBox textBox)
        {
            return textBox.GetValue(WatermarkTextProperty) as string;
        }

        public static readonly DependencyProperty ValidationErrorProperty = DependencyProperty.RegisterAttached(
            "ValidationError", typeof(Exception), typeof(DependencyObject), null);

        public static void SetValidationError(DependencyObject d, Exception ex)
        {
            d.SetValue(ValidationErrorProperty, ex);
        }

        public static Exception GetValidationError(DependencyObject d)
        {
            return d.GetValue(ValidationErrorProperty) as Exception;
        }
    }
}
