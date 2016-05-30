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
using ESRI.ArcGIS.Mapping.Controls;

namespace ESRI.ArcGIS.Mapping.GP
{
    public class ViewUtility
    {
        static View GetView()
        {
            return ControlTreeHelper.FindChildOfType<View>(Application.Current.RootVisual, 10);
        }
        public static Core.SymbolConfigProvider GetSymbolConfigProvider()
        {
            View view = GetView();
            return view != null ? view.SymbolConfigProvider : null;
        }

        public static ESRI.ArcGIS.Mapping.DataSources.DataSourceProvider GetDataSourceProvider()
        {
            View view = GetView();
            return view != null ? view.DataSourceProvider : null;
        }

        public static ESRI.ArcGIS.Mapping.Core.ConnectionsProvider GetConnectionsProvider()
        {
            View view = GetView();
            return view != null ? view.ConnectionsProvider : null;
        }

        public static double GetViewHeight()
        {
            View view = GetView();
            return view != null ? view.ActualHeight : 300;
        }

        public static double GetViewWidth()
        {
            View view = GetView();
            return view != null ? view.ActualWidth : 400;
        }
    }
}
