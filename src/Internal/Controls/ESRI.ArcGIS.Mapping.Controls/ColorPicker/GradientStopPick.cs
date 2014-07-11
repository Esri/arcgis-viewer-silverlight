/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Core.Symbols;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class GradientStopPick : Control
    {

        public GradientStopPick()
        {
            DefaultStyleKey = typeof(GradientStopPick);
            Outline = new SolidColorBrush(Colors.DarkGray);
        }

        Panel Container;
        public override void OnApplyTemplate()
        {
            if (Container != null)
                Container.MouseLeftButtonUp -= Container_MouseLeftButtonUp;
            base.OnApplyTemplate();
            Container = GetTemplateChild("Container") as Panel;
            if (Container != null)
                Container.MouseLeftButtonUp += Container_MouseLeftButtonUp;
        }

        void Container_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsSelected = true;
        }
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(GradientStopPick), new PropertyMetadata(false, OnIsSelectedPropertyChanged));


        private static void OnIsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GradientStopPick source = d as GradientStopPick;
            if (source.IsSelected)
                source.Outline = new SolidColorBrush(Colors.Black);
            else
                source.Outline = new SolidColorBrush(Colors.DarkGray);
        }

        public Brush Outline
        {
            get { return (Brush)GetValue(OutlineProperty); }
            set { SetValue(OutlineProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Outline.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OutlineProperty =
            DependencyProperty.Register("Outline", typeof(Brush), typeof(GradientStopPick), null);


        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Fill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(GradientStopPick), null);

        
    }

}
