/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "ProgressDisplayRight", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "ProgressDisplayLeft", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "ProgressPointerStack", Type = typeof(FrameworkElement))]
    public class ProgressGauge : Control
    {
        #region private fields

        FrameworkElement ProgressDisplayRight;
        FrameworkElement ProgressDisplayLeft;
        FrameworkElement ProgressPointerStack;

        ProgressGauge stopwatch;
        Map map;
        double progressIncrement = 3.6;
        bool isVisible = false;

        #endregion

        public ProgressGauge()
        {
            stopwatch = this;
            this.DefaultStyleKey = typeof(ProgressGauge);
            ChangeVisualState(false);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ProgressDisplayLeft = GetTemplateChild("ProgressDisplayLeft") as FrameworkElement;
            ProgressDisplayRight = GetTemplateChild("ProgressDisplayRight") as FrameworkElement;
            ProgressPointerStack = GetTemplateChild("ProgressPointerStack") as FrameworkElement;
        }

        /// <summary>
        /// Sets or gets the Map control associated with the <see cref="ProgressBar"/>.
        /// </summary>
        public ESRI.ArcGIS.Client.Map Map
        {
            get { return (ESRI.ArcGIS.Client.Map)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Map"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register("Map", typeof(ESRI.ArcGIS.Client.Map), typeof(ProgressGauge), new PropertyMetadata(null, OnMapPropertyChanged));

        private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressGauge sw = d as ProgressGauge;
            Map oldMap = e.OldValue as Map;
            Map newMap = e.NewValue as Map;
            if (oldMap != null)
            {
                oldMap.Progress -= sw.map_Progress;
            }
            if (newMap != null)
            {
                sw.map = newMap;
                newMap.Progress += sw.map_Progress;
            }

        }

        #region Progress Gauge

        private void map_Progress(object sender, ProgressEventArgs e)
        {
            int value = e.Progress;
            isVisible = value < 99;
            ChangeVisualState(true);
            double topangle = 0, bottomangle = 0;
            RotateTransform rt = new RotateTransform();
            RotateTransform rt2 = new RotateTransform();
            RotateTransform rt3 = new RotateTransform();
            double angle = value * progressIncrement;
            if (value > 50)
            {
                bottomangle = (value - 50) * progressIncrement;
                topangle = 180;
            }
            else
            {
                topangle = angle;
                bottomangle = 0;
            }
            rt.Angle = topangle;
            rt2.Angle = bottomangle;
            rt3.Angle = angle;
            if (ProgressDisplayRight != null)
                ProgressDisplayRight.RenderTransform = rt;
            if (ProgressDisplayLeft != null)
                ProgressDisplayLeft.RenderTransform = rt2;
            if (ProgressPointerStack != null)
                ProgressPointerStack.RenderTransform = rt3;

        }


        #endregion

        private void ChangeVisualState(bool useTransitions)
        {
            bool ok = false;
            if (isVisible)
            {
                ok = GoToState(useTransitions, "Show");
            }
            else
            {
                ok = GoToState(useTransitions, "Hide");
            }
        }

        private bool GoToState(bool useTransitions, string stateName)
        {
            return VisualStateManager.GoToState(stopwatch, stateName, useTransitions);
        }

 
    }
}
