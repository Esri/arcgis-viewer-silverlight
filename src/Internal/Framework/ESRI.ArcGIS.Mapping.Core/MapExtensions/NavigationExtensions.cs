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
    public class NavigationExtensions
    {
        public static readonly DependencyProperty IsFullNavigationEnabledProperty =
                DependencyProperty.RegisterAttached("IsFullNavigationEnabled", typeof(bool), typeof(Map), new PropertyMetadata(false));
        public static void SetIsFullNavigationEnabled(Map o, bool value)
        {
            if (o == null)
                return;

            o.SetValue(IsFullNavigationEnabledProperty, value);
        }

        public static bool GetIsFullNavigationEnabled(Map o)
        {
            object unitObj = o.GetValue(IsFullNavigationEnabledProperty);
            if(unitObj != null && unitObj is bool)
                return (bool)unitObj;

            return false;
        }
    }

    public class IsNavigationZoomOnlyVisibleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            return CheckIsFullNavigationEnabled(value as Map) ? Visibility.Visible : Visibility.Collapsed;
        }

        public static bool CheckIsFullNavigationEnabled(Map map)
        {
            return NavigationExtensions.GetIsFullNavigationEnabled(map);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
