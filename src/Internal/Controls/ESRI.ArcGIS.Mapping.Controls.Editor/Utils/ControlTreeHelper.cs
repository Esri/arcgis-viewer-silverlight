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
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Controls.Editor
{
    public static class ControlTreeHelper
    {
        public static T FindAncestorOfType<T>(DependencyObject obj) where T : DependencyObject
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

        public static T FindParentControl<T>(FrameworkElement obj) where T : FrameworkElement
        {
            while (obj != null)
            {
                var objAsT = obj as T;
                if (objAsT != null)
                    return objAsT;
                obj = obj.Parent as FrameworkElement;
            }
            return null;
        }

        public static T FindChildOfType<T>(DependencyObject obj) where T : DependencyObject
        {
            return FindChildOfType<T>(obj, null);
        }

        public static T FindChildOfType<T>(DependencyObject obj, int? recursionLevels) where T : DependencyObject
        {
            if (recursionLevels == null)
                recursionLevels = 0;

            int childCount = VisualTreeHelper.GetChildrenCount(obj);
            DependencyObject depObj;
            for (int i = 0; i < childCount; i++)
            {
                depObj = VisualTreeHelper.GetChild(obj, i);
                var objAsT = depObj as T;
                if (objAsT != null)
                    return objAsT;

                if (VisualTreeHelper.GetChildrenCount(depObj) > 0 && recursionLevels > 0)
                {
                    objAsT = FindChildOfType<T>(depObj, recursionLevels--);
                    if (objAsT != null)
                        return objAsT;
                }
            }

            return null;
        }

        public static List<T> FindChildrenOfType<T>(DependencyObject obj, int? recursionLevels) where T : DependencyObject
        {
            List<T> children = new List<T>();

            if (recursionLevels == null)
                recursionLevels = 0;

            int childCount = VisualTreeHelper.GetChildrenCount(obj);
            DependencyObject depObj;
            for (int i = 0; i < childCount; i++)
            {
                depObj = VisualTreeHelper.GetChild(obj, i);
                var objAsT = depObj as T;
                if (objAsT != null)
                    children.Add(objAsT);

                if (VisualTreeHelper.GetChildrenCount(depObj) > 0 && recursionLevels > 0)
                {
                    List<T> recursiveChildren = FindChildrenOfType<T>(depObj, recursionLevels--);
                    if (recursionLevels != null)
                        children.AddRange(recursiveChildren);
                }
            }

            return children;
        }
    }
}
