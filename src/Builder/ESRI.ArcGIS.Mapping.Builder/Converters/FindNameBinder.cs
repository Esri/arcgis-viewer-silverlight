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
using System.Windows.Data;
using ESRI.ArcGIS.Mapping.Builder.FileExplorer;
using ESRI.ArcGIS.Mapping.Builder.Common;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public class FindNameBinder : DependencyObject
    {

        public static DependencyProperty SourceElementProperty = DependencyProperty.Register("SourceElement",
            typeof(FrameworkElement), typeof(FindNameBinder), new PropertyMetadata(OnSourceElementPropertyChanged));

        public FrameworkElement SourceElement
        {
            get { return GetValue(SourceElementProperty) as FrameworkElement; }
            set { SetValue(SourceElementProperty, value); }
        }

        public static void OnSourceElementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement element = args.NewValue as FrameworkElement;
            FindNameBinder binder = d as FindNameBinder;
            if (element != null)
                binder.TargetElement = element.FindName(binder.TargetName) as FrameworkElement;
        }

        public static DependencyProperty TargetElementProperty = DependencyProperty.Register("TargetElement",
            typeof(FrameworkElement), typeof(FindNameBinder), new PropertyMetadata(null));

        public FrameworkElement TargetElement
        {
            get { return GetValue(TargetElementProperty) as FrameworkElement; }
            set { SetValue(TargetElementProperty, value); }
        }

        public string TargetName { get; set; }
    }
}
