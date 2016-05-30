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
    public class ColumnDefinitionBinder
    {
        public static readonly DependencyProperty ColumnWidthProperty =
                  DependencyProperty.RegisterAttached("ColumnWidth",
                  typeof(double), typeof(ColumnDefinitionBinder),
                  new PropertyMetadata(new PropertyChangedCallback(OnColumnWidthChanged)));

        public static void SetColumnWidth(DependencyObject o, double value)
        {
            o.SetValue(ColumnWidthProperty, value);
        }

        public static double GetColumnWidth(DependencyObject o)
        {
            return (double)o.GetValue(ColumnWidthProperty);
        }

        private static void OnColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColumnDefinition)d).Width = new GridLength((double)e.NewValue);
        }

        public static readonly DependencyProperty TargetWidthProperty =
              DependencyProperty.RegisterAttached("TargetWidth",
              typeof(double), typeof(ColumnDefinitionBinder),
              null);

        public static void SetTargetWidth(DependencyObject o, double value)
        {
            o.SetValue(TargetWidthProperty, value);
        }

        public static double GetTargetWidth(DependencyObject o)
        {
            return (double)o.GetValue(TargetWidthProperty);
        }
    }
}
