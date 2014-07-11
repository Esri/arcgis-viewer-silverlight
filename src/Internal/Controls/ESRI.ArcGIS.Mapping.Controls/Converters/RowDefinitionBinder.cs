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

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class RowDefinitionBinder
    {
        public static readonly DependencyProperty RowHeightProperty =
                  DependencyProperty.RegisterAttached("RowHeight",
                  typeof(double), typeof(RowDefinitionBinder),
                  new PropertyMetadata(new PropertyChangedCallback(OnRowHeightChanged)));

        public static void SetRowHeight(DependencyObject o, double value)
        {
            o.SetValue(RowHeightProperty, value);
        }

        public static double GetRowHeight(DependencyObject o)
        {
            return (double)o.GetValue(RowHeightProperty);
        }

        private static void OnRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RowDefinition)d).Height = new GridLength((double)e.NewValue);
        }

        public static readonly DependencyProperty TargetHeightProperty =
              DependencyProperty.RegisterAttached("TargetHeight",
              typeof(double), typeof(RowDefinitionBinder),
              null);

        public static void SetTargetHeight(DependencyObject o, double value)
        {
            o.SetValue(TargetHeightProperty, value);
        }

        public static double GetTargetHeight(DependencyObject o)
        {
            return (double)o.GetValue(TargetHeightProperty);
        }
    }
}
