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

namespace ESRI.ArcGIS.Mapping.Controls
{
    public static class VisualStateManagerHelper
    {
        public static Storyboard FindStoryboard(FrameworkElement element, string groupName, string stateName, bool prefixControlName = false)
        {
            if (element == null)
                return null;

            string group = string.IsNullOrWhiteSpace(groupName) ? null : (prefixControlName ? element.Name + "_" + groupName : groupName);
            string state = string.IsNullOrWhiteSpace(stateName) ? null : (prefixControlName ? element.Name + "_" + stateName : stateName);

            var vsgs = VisualStateManager.GetVisualStateGroups(element);
            if (vsgs != null && vsgs.Count > 0)
            {
                foreach (VisualStateGroup vsg in vsgs)
                {
                    if (!string.IsNullOrWhiteSpace(group) && vsg.Name != group)
                        continue;

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
            if (VisualTreeHelper.GetChildrenCount(element) > 0)
                child = VisualTreeHelper.GetChild(element, 0) as FrameworkElement;

            if (child == null)
                return null;

            vsgs = VisualStateManager.GetVisualStateGroups(child);
            if (vsgs != null && vsgs.Count > 0)
            {
                foreach (VisualStateGroup vsg in vsgs)
                {
                    if (!string.IsNullOrWhiteSpace(group) && vsg.Name != group)
                        continue;

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

        public static bool StateWellDefined(FrameworkElement element, string stateName, bool prefixControlName = false)
        {
            string state = string.IsNullOrWhiteSpace(stateName) ? null : (prefixControlName ? element.Name + "_" + 
                stateName : stateName);

            // Look for the visual state on the first child object.  This is generally where states are declared
            // when they are part of a ControlTemplate.
            FrameworkElement child = null;
            if (VisualTreeHelper.GetChildrenCount(element) > 0)
                child = VisualTreeHelper.GetChild(element, 0) as FrameworkElement;

            if (child == null)
                return false;

            var vsgs = VisualStateManager.GetVisualStateGroups(child);
            if (vsgs != null && vsgs.Count > 0)
            {
                foreach (VisualStateGroup vsg in vsgs)
                {
                    if (vsg.States != null)
                    {
                        foreach (VisualState vs in vsg.States)
                        {
                            if (vs.Name == state)
                                return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
