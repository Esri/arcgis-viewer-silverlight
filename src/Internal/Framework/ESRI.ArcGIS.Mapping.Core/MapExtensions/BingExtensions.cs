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
using ESRI.ArcGIS.Client;
using System.Windows.Data;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class BingExtensions
    {
        public static readonly DependencyProperty HasBingLayersProperty =
                DependencyProperty.RegisterAttached("HasBingLayers", typeof(bool), typeof(Map), new PropertyMetadata(false));
        public static void SetHasBingLayers(Map o, bool value)
        {
            if (o == null)
                return;

            o.SetValue(HasBingLayersProperty, value);
        }

        public static bool GetHasBingLayers(Map o)
        {
            object unitObj = o.GetValue(HasBingLayersProperty);
            if(unitObj != null && unitObj is bool)
                return (bool)unitObj;

            return false;
        }
    }
}
