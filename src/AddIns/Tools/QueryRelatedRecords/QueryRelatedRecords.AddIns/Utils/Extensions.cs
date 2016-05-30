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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace QueryRelatedRecords.AddIns
{
    /// <summary>
    /// Provides extension methods for use by the Search add-in
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



        /// <summary>
        /// Provides generic attached properties for 
        /// </summary>
        public class Properties : DependencyObject
        {
            /// <summary>
            /// Attaches a callback used for notification when the specified DependencyProperty changes on the
            /// specified object.
            /// </summary>
            public static DependencyProperty NotifyOnDependencyPropertyChanged(
                string propertyName, DependencyObject source, PropertyChangedCallback callback)
            {
                if (source == null)
                    return null;

                var b = new Binding(propertyName) { Source = source };
                DependencyProperty prop = DependencyProperty.RegisterAttached(
                    "ListenerProperty" + DateTime.Now.Ticks.ToString(),
                    typeof(object),
                    typeof(DependencyObject),
                    new PropertyMetadata(callback));

                BindingOperations.SetBinding(source, prop, b);

                return prop;
            }
        }
    }
}
