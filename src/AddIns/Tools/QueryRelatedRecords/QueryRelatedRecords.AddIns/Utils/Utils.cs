/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Resources;
using System.Windows.Shapes;

namespace QueryRelatedRecords.AddIns
{
    internal class Utils
    {
        internal static T FindChildOfType<T>(DependencyObject obj) where T : DependencyObject
        {
            return FindChildOfType<T>(obj, null);
        }

        internal static T FindChildOfType<T>(DependencyObject obj, int? recursionLevels) where T : DependencyObject
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
    }
}
